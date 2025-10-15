using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ModelingToolkit
{
    public class MaterialEditor : ScriptableObject
    {
        // Target object and material
        private GameObject _targetObject;
        private Material _targetMaterial;
        private Material _previewMaterial;
        
        // Material properties
        private Color _baseColor = Color.white;
        private float _metallic = 0f;
        private float _smoothness = 0.5f;
        private float _normalStrength = 1f;
        private Color _emissionColor = Color.black;
        private float _emissionIntensity = 0f;
        private bool _emissionEnabled = false;
        
        // Material textures
        private Texture2D _albedoMap;
        private Texture2D _normalMap;
        private Texture2D _metallicMap;
        private Texture2D _roughnessMap;
        private Texture2D _heightMap;
        private Texture2D _emissionMap;
        private Texture2D _occlusionMap;
        
        // Preview mesh
        private Mesh _previewMesh;
        private Vector2 _previewRotation;
        private float _previewZoom = 1f;
        
        // UI state
        private Vector2 _scrollPosition;
        private bool _showTextureSettings = true;
        private bool _showMaterialSettings = true;
        private bool _showSpecialMaps = false;
        
        // Static instance to maintain state
        private static MaterialEditor _instance;
        
        // Get or create the instance
        public static MaterialEditor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<MaterialEditor>();
                }
                return _instance;
            }
        }
        
        // Open the material editor window using a separate EditorWindow class
        [MenuItem("Window/3D Toolkit/Material Editor")]
        public static void ShowWindow()
        {
            MaterialEditorWindow.ShowWindow();
        }
        
        // Initialize the material editor
        public void Initialize()
        {
            // Initialize default values
            _baseColor = Color.white;
            _metallic = 0f;
            _smoothness = 0.5f;
            _normalStrength = 1f;
            _emissionColor = Color.black;
            _emissionIntensity = 0f;
            _emissionEnabled = false;
            
            // Set up preview mesh
            if (_previewMesh == null)
            {
                _previewMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                if (_previewMesh == null)
                {
                    // Create a simple sphere as fallback
                    _previewMesh = new Mesh();
                    _previewMesh.name = "Preview Sphere";
                    // Add code to create sphere vertices and triangles if needed
                }
            }
            
            // Initialize preview material
            _previewMaterial = new Material(Shader.Find("Standard"));
            _previewRotation = new Vector2(30, 0);
        }
        
        // Set the target material to edit
        public void SetTargetMaterial(Material material)
        {
            if (material == null)
                return;
                
            _targetMaterial = material;
            
            // Load material properties
            _baseColor = _targetMaterial.color;
            _metallic = _targetMaterial.HasProperty("_Metallic") ? _targetMaterial.GetFloat("_Metallic") : 0f;
            _smoothness = _targetMaterial.HasProperty("_Glossiness") ? _targetMaterial.GetFloat("_Glossiness") : 0.5f;
            _normalStrength = _targetMaterial.HasProperty("_BumpScale") ? _targetMaterial.GetFloat("_BumpScale") : 1f;
            _emissionEnabled = _targetMaterial.IsKeywordEnabled("_EMISSION");
            
            if (_emissionEnabled && _targetMaterial.HasProperty("_EmissionColor"))
            {
                _emissionColor = _targetMaterial.GetColor("_EmissionColor");
                _emissionIntensity = _emissionColor.maxColorComponent;
                if (_emissionIntensity > 0)
                {
                    _emissionColor /= _emissionIntensity;
                }
            }
            
            // Load textures
            _albedoMap = _targetMaterial.HasProperty("_MainTex") ? _targetMaterial.GetTexture("_MainTex") as Texture2D : null;
            _normalMap = _targetMaterial.HasProperty("_BumpMap") ? _targetMaterial.GetTexture("_BumpMap") as Texture2D : null;
            _metallicMap = _targetMaterial.HasProperty("_MetallicGlossMap") ? _targetMaterial.GetTexture("_MetallicGlossMap") as Texture2D : null;
            _roughnessMap = _targetMaterial.HasProperty("_SpecGlossMap") ? _targetMaterial.GetTexture("_SpecGlossMap") as Texture2D : null;
            _heightMap = _targetMaterial.HasProperty("_ParallaxMap") ? _targetMaterial.GetTexture("_ParallaxMap") as Texture2D : null;
            _emissionMap = _targetMaterial.HasProperty("_EmissionMap") ? _targetMaterial.GetTexture("_EmissionMap") as Texture2D : null;
            _occlusionMap = _targetMaterial.HasProperty("_OcclusionMap") ? _targetMaterial.GetTexture("_OcclusionMap") as Texture2D : null;
            
            // Update preview material
            if (_previewMaterial != null)
            {
                // Copy important properties
                _previewMaterial.CopyPropertiesFromMaterial(_targetMaterial);
            }
        }
        
        // Draw the material editor UI
        public void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.Space();
            
            // Target object selection
            EditorGUI.BeginChangeCheck();
            _targetObject = EditorGUILayout.ObjectField("Target Object", _targetObject, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                if (_targetObject != null)
                {
                    Renderer renderer = _targetObject.GetComponent<Renderer>();
                    if (renderer != null && renderer.sharedMaterial != null)
                    {
                        SetTargetMaterial(renderer.sharedMaterial);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Object", "The selected object must have a Renderer with a material.", "OK");
                        _targetObject = null;
                        _targetMaterial = null;
                    }
                }
                else
                {
                    _targetMaterial = null;
                }
            }
            
            EditorGUILayout.Space();
            
            // Material direct selection
            EditorGUI.BeginChangeCheck();
            Material newMaterial = EditorGUILayout.ObjectField("Material", _targetMaterial, typeof(Material), false) as Material;
            if (EditorGUI.EndChangeCheck() && newMaterial != null)
            {
                SetTargetMaterial(newMaterial);
            }
            
            EditorGUILayout.Space();
            
            if (_targetMaterial == null)
            {
                EditorGUILayout.HelpBox("Select an object or material to edit.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }
            
            // Material preview
            Rect previewRect = GUILayoutUtility.GetRect(0, 200);
            DrawMaterialPreview(previewRect);
            
            EditorGUILayout.Space();
            
            // Material properties
            _showMaterialSettings = ModelingToolkitStyles.DrawFoldout("Material Properties", _showMaterialSettings);
            
            if (_showMaterialSettings)
            {
                ModelingToolkitStyles.BeginPanel();
                
                // Base properties
                EditorGUI.BeginChangeCheck();
                _baseColor = EditorGUILayout.ColorField("Base Color", _baseColor);
                if (EditorGUI.EndChangeCheck())
                {
                    _targetMaterial.color = _baseColor;
                    if (_previewMaterial != null) _previewMaterial.color = _baseColor;
                }
                
                // Metallic
                EditorGUI.BeginChangeCheck();
                _metallic = EditorGUILayout.Slider("Metallic", _metallic, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    _targetMaterial.SetFloat("_Metallic", _metallic);
                    if (_previewMaterial != null) _previewMaterial.SetFloat("_Metallic", _metallic);
                }
                
                // Smoothness
                EditorGUI.BeginChangeCheck();
                _smoothness = EditorGUILayout.Slider("Smoothness", _smoothness, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    _targetMaterial.SetFloat("_Glossiness", _smoothness);
                    if (_previewMaterial != null) _previewMaterial.SetFloat("_Glossiness", _smoothness);
                }
                
                // Normal strength
                if (_normalMap != null)
                {
                    EditorGUI.BeginChangeCheck();
                    _normalStrength = EditorGUILayout.Slider("Normal Strength", _normalStrength, 0f, 2f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _targetMaterial.SetFloat("_BumpScale", _normalStrength);
                        if (_previewMaterial != null) _previewMaterial.SetFloat("_BumpScale", _normalStrength);
                    }
                }
                
                // Emission
                EditorGUI.BeginChangeCheck();
                _emissionEnabled = EditorGUILayout.Toggle("Emission Enabled", _emissionEnabled);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_emissionEnabled)
                    {
                        _targetMaterial.EnableKeyword("_EMISSION");
                        if (_previewMaterial != null) _previewMaterial.EnableKeyword("_EMISSION");
                    }
                    else
                    {
                        _targetMaterial.DisableKeyword("_EMISSION");
                        if (_previewMaterial != null) _previewMaterial.DisableKeyword("_EMISSION");
                    }
                }
                
                if (_emissionEnabled)
                {
                    EditorGUI.BeginChangeCheck();
                    _emissionColor = EditorGUILayout.ColorField("Emission Color", _emissionColor);
                    _emissionIntensity = EditorGUILayout.Slider("Emission Intensity", _emissionIntensity, 0f, 10f);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        // Apply emission with intensity
                        Color finalEmission = _emissionColor * _emissionIntensity;
                        _targetMaterial.SetColor("_EmissionColor", finalEmission);
                        if (_previewMaterial != null) _previewMaterial.SetColor("_EmissionColor", finalEmission);
                    }
                }
                
                ModelingToolkitStyles.EndPanel();
            }
            
            EditorGUILayout.Space();
            
            // Texture maps
            _showTextureSettings = ModelingToolkitStyles.DrawFoldout("Texture Maps", _showTextureSettings);
            
            if (_showTextureSettings)
            {
                ModelingToolkitStyles.BeginPanel();
                
                // Albedo map
                EditorGUI.BeginChangeCheck();
                _albedoMap = EditorGUILayout.ObjectField("Albedo Map", _albedoMap, typeof(Texture2D), false) as Texture2D;
                if (EditorGUI.EndChangeCheck())
                {
                    _targetMaterial.SetTexture("_MainTex", _albedoMap);
                    if (_previewMaterial != null) _previewMaterial.SetTexture("_MainTex", _albedoMap);
                }
                
                // Normal map
                EditorGUI.BeginChangeCheck();
                _normalMap = EditorGUILayout.ObjectField("Normal Map", _normalMap, typeof(Texture2D), false) as Texture2D;
                if (EditorGUI.EndChangeCheck())
                {
                    if (_normalMap != null)
                    {
                        _targetMaterial.EnableKeyword("_NORMALMAP");
                        _targetMaterial.SetTexture("_BumpMap", _normalMap);
                        
                        if (_previewMaterial != null)
                        {
                            _previewMaterial.EnableKeyword("_NORMALMAP");
                            _previewMaterial.SetTexture("_BumpMap", _normalMap);
                        }
                    }
                    else
                    {
                        _targetMaterial.DisableKeyword("_NORMALMAP");
                        _targetMaterial.SetTexture("_BumpMap", null);
                        
                        if (_previewMaterial != null)
                        {
                            _previewMaterial.DisableKeyword("_NORMALMAP");
                            _previewMaterial.SetTexture("_BumpMap", null);
                        }
                    }
                }
                
                // Metallic map
                EditorGUI.BeginChangeCheck();
                _metallicMap = EditorGUILayout.ObjectField("Metallic Map", _metallicMap, typeof(Texture2D), false) as Texture2D;
                if (EditorGUI.EndChangeCheck())
                {
                    if (_metallicMap != null)
                    {
                        _targetMaterial.EnableKeyword("_METALLICGLOSSMAP");
                        _targetMaterial.SetTexture("_MetallicGlossMap", _metallicMap);
                        
                        if (_previewMaterial != null)
                        {
                            _previewMaterial.EnableKeyword("_METALLICGLOSSMAP");
                            _previewMaterial.SetTexture("_MetallicGlossMap", _metallicMap);
                        }
                    }
                    else
                    {
                        _targetMaterial.DisableKeyword("_METALLICGLOSSMAP");
                        _targetMaterial.SetTexture("_MetallicGlossMap", null);
                        
                        if (_previewMaterial != null)
                        {
                            _previewMaterial.DisableKeyword("_METALLICGLOSSMAP");
                            _previewMaterial.SetTexture("_MetallicGlossMap", null);
                        }
                    }
                }
                
                // Show additional maps
                _showSpecialMaps = EditorGUILayout.Foldout(_showSpecialMaps, "Advanced Maps");
                
                if (_showSpecialMaps)
                {
                    // Height map
                    EditorGUI.BeginChangeCheck();
                    _heightMap = EditorGUILayout.ObjectField("Height Map", _heightMap, typeof(Texture2D), false) as Texture2D;
                    if (EditorGUI.EndChangeCheck())
                    {
                        _targetMaterial.SetTexture("_ParallaxMap", _heightMap);
                        if (_previewMaterial != null) _previewMaterial.SetTexture("_ParallaxMap", _heightMap);
            
            if (_heightMap != null)
            {
                            _targetMaterial.EnableKeyword("_PARALLAXMAP");
                            if (_previewMaterial != null) _previewMaterial.EnableKeyword("_PARALLAXMAP");
                        }
                        else
                        {
                            _targetMaterial.DisableKeyword("_PARALLAXMAP");
                            if (_previewMaterial != null) _previewMaterial.DisableKeyword("_PARALLAXMAP");
                        }
                    }
                    
                    // Occlusion map
                    EditorGUI.BeginChangeCheck();
                    _occlusionMap = EditorGUILayout.ObjectField("Occlusion Map", _occlusionMap, typeof(Texture2D), false) as Texture2D;
                    if (EditorGUI.EndChangeCheck())
                    {
                        _targetMaterial.SetTexture("_OcclusionMap", _occlusionMap);
                        if (_previewMaterial != null) _previewMaterial.SetTexture("_OcclusionMap", _occlusionMap);
                    }
                }
                
                ModelingToolkitStyles.EndPanel();
            }
            
            EditorGUILayout.Space();
            
            // Shader selection
            EditorGUI.BeginChangeCheck();
            Shader shader = EditorGUILayout.ObjectField("Shader", _targetMaterial.shader, typeof(Shader), false) as Shader;
            if (EditorGUI.EndChangeCheck() && shader != null)
            {
                bool confirmChange = EditorUtility.DisplayDialog(
                    "Change Shader", 
                    "Changing the shader may reset some material properties. Continue?", 
                    "Yes", "Cancel");
                    
                if (confirmChange)
                {
                    _targetMaterial.shader = shader;
                    if (_previewMaterial != null)
                    {
                        _previewMaterial.shader = shader;
                    }
                    
                    // Reload material properties after shader change
                    SetTargetMaterial(_targetMaterial);
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        // Draw material preview
        private void DrawMaterialPreview(Rect rect)
        {
            if (_targetMaterial == null || _previewMesh == null)
                return;
                
            // Handle preview input
            HandlePreviewInput(rect);
            
            // Draw the preview mesh
            EditorGUI.DrawPreviewTexture(rect, RenderPreview(rect.width, rect.height));
            
            // Handle control labels
            GUI.Label(new Rect(rect.x + 5, rect.y + 5, 200, 20), "Left-click and drag to rotate", EditorStyles.miniLabel);
        }
        
        // Handle input for preview rotation
        private void HandlePreviewInput(Rect rect)
        {
            Event e = Event.current;
            
            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    _lastMousePosition = e.mousePosition;
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    _previewRotation.y += (e.mousePosition.x - _lastMousePosition.x) * 0.5f;
                    _previewRotation.x += (e.mousePosition.y - _lastMousePosition.y) * 0.5f;
                    _lastMousePosition = e.mousePosition;
                    e.Use();
                }
                else if (e.type == EventType.ScrollWheel)
                {
                    _previewZoom = Mathf.Clamp(_previewZoom + e.delta.y * 0.05f, 0.5f, 2f);
                    e.Use();
                }
            }
        }
        
        // Vector2 for the last mouse position during preview interaction
        private Vector2 _lastMousePosition;
        
        // Render the preview
        private Texture RenderPreview(float width, float height)
        {
            if (_previewMaterial == null || _previewMesh == null)
                return null;
                
            // Create a temporary camera for rendering
            GameObject cameraObj = new GameObject("PreviewCamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            
            // Set up camera position
            camera.transform.position = new Vector3(0, 0, -5f / _previewZoom);
            camera.transform.LookAt(Vector3.zero);
            
            // Create render texture
            RenderTexture renderTexture = RenderTexture.GetTemporary((int)width, (int)height, 16, RenderTextureFormat.ARGB32);
            camera.targetTexture = renderTexture;
            
            // Create a temporary game object for the preview mesh
            GameObject previewObj = new GameObject("PreviewMesh");
            previewObj.transform.rotation = Quaternion.Euler(_previewRotation.x, _previewRotation.y, 0);
            
            MeshFilter meshFilter = previewObj.AddComponent<MeshFilter>();
            meshFilter.mesh = _previewMesh;
            
            MeshRenderer meshRenderer = previewObj.AddComponent<MeshRenderer>();
            meshRenderer.material = _previewMaterial;
            
            // Add a light
            GameObject lightObj = new GameObject("PreviewLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0);
            
            // Render the scene
            camera.Render();
            
            // Clean up
            Object.DestroyImmediate(lightObj);
            Object.DestroyImmediate(previewObj);
            Object.DestroyImmediate(cameraObj);
            
            RenderTexture result = RenderTexture.GetTemporary((int)width, (int)height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(renderTexture, result);
            
            RenderTexture.ReleaseTemporary(renderTexture);
            
            return result;
        }
        
        // Export the current material to a file
        public void ExportMaterial()
        {
            if (_targetMaterial == null)
                return;
                
            string path = EditorUtility.SaveFilePanelInProject(
                "Export Material",
                _targetMaterial.name + ".mat",
                "mat",
                "Save material asset"
            );
            
            if (string.IsNullOrEmpty(path))
                return;
                
            // Create a new asset
            AssetDatabase.CreateAsset(new Material(_targetMaterial), path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            
            // Show the new asset
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Material>(path);
        }
        
        // Cleanup the resources
        public void Cleanup()
        {
            if (_previewMaterial != null)
            {
                Object.DestroyImmediate(_previewMaterial);
                _previewMaterial = null;
            }
        }
    }
    
    // Editor window that hosts the MaterialEditor
    public class MaterialEditorWindow : EditorWindow
    {
        [MenuItem("Window/3D Toolkit/Material Editor")]
        public static void ShowWindow()
        {
            MaterialEditorWindow window = GetWindow<MaterialEditorWindow>("Material Editor");
            window.minSize = new Vector2(350, 450);
        }
        
        private void OnEnable()
        {
            // Initialize the MaterialEditor
            MaterialEditor.Instance.Initialize();
        }
        
        private void OnGUI()
        {
            // Draw the MaterialEditor UI
            MaterialEditor.Instance.OnGUI();
        }
        
        private void OnDisable()
        {
            // Clean up resources
            MaterialEditor.Instance.Cleanup();
        }
    }
} 