using UnityEngine;
using UnityEditor;

namespace ModelingToolkit
{
    public class TexturePainterTester : EditorWindow
    {
        private GameObject targetObject;
        private TexturePainter texturePainter;
        private Vector2 scrollPosition;
        private Color brushColor = Color.red;
        private float brushSize = 0.1f;
        private float brushHardness = 0.5f;
        private TexturePainter.TextureChannel activeChannel = TexturePainter.TextureChannel.Albedo;
        private Texture2D previewTexture;
        private bool isPaintingActive = false;

        [MenuItem("Window/3D Toolkit/Texture Painter Test")]
        public static void ShowWindow()
        {
            GetWindow<TexturePainterTester>("Texture Painter Test");
        }

        private void OnEnable()
        {
            texturePainter = new TexturePainter();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            if (texturePainter != null)
            {
                texturePainter.Cleanup();
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Texture Painter Test", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            targetObject = EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true) as GameObject;

            if (targetObject != null)
            {
                Renderer renderer = targetObject.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Brush Settings", EditorStyles.boldLabel);
                    brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);
                    brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.01f, 0.5f);
                    brushHardness = EditorGUILayout.Slider("Brush Hardness", brushHardness, 0.1f, 1.0f);
                    
                    EditorGUILayout.Space();
                    activeChannel = (TexturePainter.TextureChannel)EditorGUILayout.EnumPopup("Texture Channel", activeChannel);
                    
                    EditorGUILayout.Space();
                    
                    if (!isPaintingActive)
                    {
                        if (GUILayout.Button("Start Painting"))
                        {
                            StartPainting(renderer);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("End Painting"))
                        {
                            EndPainting(renderer);
                        }
                        
                        EditorGUILayout.HelpBox("Click in the Scene View to paint on the object.", MessageType.Info);
                    }
                    
                    // Preview texture
                    if (previewTexture != null)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Texture Preview:");
                        Rect previewRect = GUILayoutUtility.GetRect(256, 256);
                        EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Selected object must have a Renderer with a material.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a 3D object with a texture.", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void StartPainting(Renderer renderer)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                texturePainter.BrushColor = brushColor;
                texturePainter.BrushSize = brushSize;
                texturePainter.BrushHardness = brushHardness;
                texturePainter.ActiveChannel = activeChannel;
                
                texturePainter.StartPainting(renderer.sharedMaterial);
                isPaintingActive = true;
                
                // Update preview
                UpdatePreview();
                
                SceneView.RepaintAll();
            }
        }
        
        private void EndPainting(Renderer renderer)
        {
            if (isPaintingActive && renderer != null && renderer.sharedMaterial != null)
            {
                texturePainter.EndPainting(renderer.sharedMaterial);
                isPaintingActive = false;
                
                SceneView.RepaintAll();
            }
        }
        
        private void UpdatePreview()
        {
            // This method would update the preview texture
            // In a full implementation, we'd get this from the TexturePainter
        }
        
        private void OnSceneGUI(SceneView sceneView)
        {
            if (!isPaintingActive || targetObject == null)
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
                        if (hit.transform.gameObject == targetObject)
                        {
                            // Paint at UV coordinates
                            texturePainter.Paint(hit.textureCoord);
                            
                            // Update preview
                            UpdatePreview();
                            
                            // Consume the event
                            e.Use();
                            
                            // Force repaint
                            sceneView.Repaint();
                        }
                    }
                }
            }
        }
    }
} 