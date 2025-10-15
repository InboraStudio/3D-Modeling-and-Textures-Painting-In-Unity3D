using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ModelingToolkit
{
    public class TexturePaintingWindow : EditorWindow
    {
        // Texture painter instance
        private TexturePainter _texturePainter;
        
        // Target object and material
        private GameObject _targetObject;
        private Material _targetMaterial;
        
        // Brush settings
        private Color _brushColor = Color.white;
        private float _brushSize = 0.1f;
        private float _brushHardness = 0.5f;
        private float _brushOpacity = 1.0f;
        private TexturePainter.TextureChannel _activeChannel = TexturePainter.TextureChannel.Albedo;
        
        // Texture size options
        private int[] _textureSizes = { 512, 1024, 2048, 4096 };
        private int _selectedSizeIndex = 1; // Default to 1024
        
        // UI state
        private Vector2 _scrollPosition;
        private bool _isPaintingActive = false;
        private Texture2D _previewTexture;
        
        // Open the texture painter window
        [MenuItem("Window/3D Toolkit/Texture Painter")]
        public static void ShowWindow()
        {
            TexturePaintingWindow window = GetWindow<TexturePaintingWindow>("Texture Painter");
            window.minSize = new Vector2(350, 450);
        }
        
        private void OnEnable()
        {
            // Initialize texture painter
            _texturePainter = new TexturePainter();
            
            // Register for scene view callbacks
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            
            // Try to use the currently selected object
            if (Selection.activeGameObject != null)
            {
                Renderer renderer = Selection.activeGameObject.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    _targetObject = Selection.activeGameObject;
                    _targetMaterial = renderer.sharedMaterial;
                    UpdatePreviewTexture();
                }
            }
        }
        
        private void OnDisable()
        {
            // Unregister from scene view callbacks
            SceneView.duringSceneGui -= OnSceneGUI;
            
            // Clean up resources
            EndPaintingSession();
            if (_texturePainter != null)
            {
                _texturePainter.Cleanup();
                _texturePainter = null;
            }
        }
        
        private void OnGUI()
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
                        // End current painting session if active
                        if (_isPaintingActive)
                        {
                            EndPaintingSession();
                        }
                        
                        _targetMaterial = renderer.sharedMaterial;
                        UpdatePreviewTexture();
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
            
            // Texture channel selection
            if (_targetMaterial != null)
            {
                EditorGUI.BeginChangeCheck();
                _activeChannel = (TexturePainter.TextureChannel)EditorGUILayout.EnumPopup("Texture Channel", _activeChannel);
                if (EditorGUI.EndChangeCheck() && _isPaintingActive)
                {
                    _texturePainter.SetActiveChannel(_activeChannel);
                    UpdatePreviewTexture();
                }
            }
            
            EditorGUILayout.Space();
            
            // Texture size selection (only available before starting painting)
            if (!_isPaintingActive)
            {
                string[] sizesText = new string[_textureSizes.Length];
                for (int i = 0; i < _textureSizes.Length; i++)
                {
                    sizesText[i] = _textureSizes[i] + "x" + _textureSizes[i];
                }
                
                _selectedSizeIndex = EditorGUILayout.Popup("Texture Size", _selectedSizeIndex, sizesText);
            }
            
            EditorGUILayout.Space();
            ModelingToolkitStyles.DrawHeader("Brush Settings");
            ModelingToolkitStyles.BeginPanel();
            
            // Brush settings
            _brushColor = EditorGUILayout.ColorField("Brush Color", _brushColor);
            _brushSize = EditorGUILayout.Slider("Brush Size", _brushSize, 0.01f, 0.5f);
            _brushHardness = EditorGUILayout.Slider("Brush Hardness", _brushHardness, 0.0f, 1.0f);
            _brushOpacity = EditorGUILayout.Slider("Brush Opacity", _brushOpacity, 0.0f, 1.0f);
            
            // Update brush settings if painting is active
            if (_isPaintingActive)
            {
                _texturePainter.BrushColor = new Color(_brushColor.r, _brushColor.g, _brushColor.b, _brushOpacity);
                _texturePainter.BrushSize = _brushSize;
                _texturePainter.BrushHardness = _brushHardness;
            }
            
            ModelingToolkitStyles.EndPanel();
            
            EditorGUILayout.Space();
            
            // Preview texture
            if (_previewTexture != null)
            {
                ModelingToolkitStyles.DrawHeader("Texture Preview");
                ModelingToolkitStyles.BeginPanel();
                
                // Calculate a reasonable preview size to fit in window
                float maxPreviewWidth = EditorGUIUtility.currentViewWidth - 40;
                float previewWidth = Mathf.Min(256, maxPreviewWidth);
                float previewHeight = previewWidth;
                
                Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
                EditorGUI.DrawPreviewTexture(previewRect, _previewTexture);
                
                ModelingToolkitStyles.EndPanel();
            }
            
            EditorGUILayout.Space();
            
            // Painting controls
            ModelingToolkitStyles.BeginPanel();
            
            if (_targetMaterial != null)
            {
                if (!_isPaintingActive)
                {
                    if (GUILayout.Button("Start Painting"))
                    {
                        StartPaintingSession();
                    }
                }
                else
                {
                    if (GUILayout.Button("End Painting"))
                    {
                        EndPaintingSession();
                    }
                    
                    EditorGUILayout.HelpBox("Click in the Scene View to paint on the model.", MessageType.Info);
                }
                
                EditorGUILayout.Space();
                
                // Export/Import buttons (disabled during active painting)
                GUI.enabled = !_isPaintingActive && _targetMaterial != null;
                
                if (GUILayout.Button("Export Texture"))
                {
                    ExportTexture();
                }
                
                if (GUILayout.Button("Import Texture"))
                {
                    ImportTexture();
                }
                
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.HelpBox("Select a 3D object with a material to begin painting.", MessageType.Info);
            }
            
            ModelingToolkitStyles.EndPanel();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void StartPaintingSession()
        {
            if (_targetMaterial == null || _texturePainter == null)
                return;
                
            _texturePainter.BrushColor = new Color(_brushColor.r, _brushColor.g, _brushColor.b, _brushOpacity);
            _texturePainter.BrushSize = _brushSize;
            _texturePainter.BrushHardness = _brushHardness;
            _texturePainter.ActiveChannel = _activeChannel;
            
            _texturePainter.StartPainting(_targetMaterial, _textureSizes[_selectedSizeIndex]);
            _isPaintingActive = true;
            
            UpdatePreviewTexture();
            SceneView.RepaintAll();
        }
        
        private void EndPaintingSession()
        {
            if (!_isPaintingActive || _texturePainter == null || _targetMaterial == null)
                return;
                
            _texturePainter.EndPainting(_targetMaterial);
            _isPaintingActive = false;
            
            UpdatePreviewTexture();
            SceneView.RepaintAll();
            
            AssetDatabase.Refresh();
        }
        
        private void UpdatePreviewTexture()
        {
            // In a full implementation, this would get the current texture from the TexturePainter
            // For now, we'll just display the material's main texture
            if (_targetMaterial != null)
            {
                Texture mainTexture = _targetMaterial.GetTexture("_MainTex");
                if (mainTexture != null)
                {
                    _previewTexture = mainTexture as Texture2D;
                }
            }
        }
        
        private void ExportTexture()
        {
            if (_targetMaterial == null)
                return;
                
            Texture2D texture = _targetMaterial.GetTexture("_MainTex") as Texture2D;
            if (texture == null)
                return;
                
            string path = EditorUtility.SaveFilePanel("Export Texture", "", "texture", "png");
            if (!string.IsNullOrEmpty(path))
            {
                if (_texturePainter != null)
                {
                    _texturePainter.ExportTexture(path);
                }
            }
        }
        
        private void ImportTexture()
        {
            if (_targetMaterial == null)
                return;
                
            string path = EditorUtility.OpenFilePanel("Import Texture", "", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(path))
            {
                if (_texturePainter != null)
                {
                    _texturePainter.ImportTexture(path);
                    UpdatePreviewTexture();
                }
            }
        }
        
        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_isPaintingActive || _targetObject == null || _texturePainter == null)
                return;
                
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (e.button == 0) // Left mouse button
                {
                    // Cast ray from camera to scene
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.gameObject == _targetObject)
                        {
                            // Paint at UV coordinates
                            _texturePainter.Paint(hit.textureCoord);
                            
                            // Consume the event
                            e.Use();
                            
                            // Force repaint
                            sceneView.Repaint();
                            Repaint(); // Also repaint the window to update preview
                        }
                    }
                }
            }
        }
    }
} 