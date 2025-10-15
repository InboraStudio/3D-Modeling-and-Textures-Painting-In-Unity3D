using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ModelingToolkit
{
    public class ModelingToolkitWindow : EditorWindow
    {
        // Main window instance
        private static ModelingToolkitWindow _window;
        
        // Current tool mode
        private ToolMode _currentToolMode = ToolMode.View;
        
        // Selection mode
        private SelectionMode _selectionMode = SelectionMode.Object;
        
        // Reference to the active mesh
        private MeshFilter _activeMeshFilter;
        private Mesh _activeMesh;
        
        // Tab states
        private bool _showModelingTools = true;
        private bool _showUVTools = false;
        private bool _showTextureTools = false;
        private bool _showMaterialTools = false;
        
        // Scroll position
        private Vector2 _scrollPosition;
        
        // Editor styles
        private GUIStyle _toolbarButtonStyle;
        private GUIStyle _tabButtonStyle;
        
        // Enum definitions
        public enum ToolMode
        {
            View,
            Select,
            Move,
            Rotate,
            Scale,
            Extrude,
            Bevel,
            LoopCut,
            Paint
        }
        
        public enum SelectionMode
        {
            Object,
            Vertex,
            Edge,
            Face
        }
        
        [MenuItem("Window/3D Toolkit/Modeling Toolkit")]
        public static void ShowWindow()
        {
            _window = GetWindow<ModelingToolkitWindow>("3D Modeling Toolkit");
            _window.minSize = new Vector2(300, 400);
            
            // Make sure SceneView callbacks are registered
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }
        
        private void OnEnable()
        {
            // Initialize styles and other resources
            InitializeStyles();
            
            // Register for scene view events
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            
            // Make sure we clean up when window is closed
            EditorApplication.quitting += OnEditorQuitting;
        }
        
        private void OnDisable()
        {
            // Unregister from scene view events
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.quitting -= OnEditorQuitting;
        }
        
        private void OnEditorQuitting()
        {
            // Clean up any resources
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        
        private void InitializeStyles()
        {
            // Initialize custom styles
            ModelingToolkitStyles.Initialize();
            
            _toolbarButtonStyle = ModelingToolkitStyles.ToolbarButtonStyle;
            _tabButtonStyle = ModelingToolkitStyles.TabButtonStyle;
        }
        
        private void OnGUI()
        {
            if (_toolbarButtonStyle == null)
                InitializeStyles();
                
            DrawToolbar();
            
            EditorGUILayout.Space();
            
            DrawTabs();
            
            EditorGUILayout.Space();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            if (_showModelingTools)
                DrawModelingTools();
                
            if (_showUVTools)
                DrawUVTools();
                
            if (_showTextureTools)
                DrawTextureTools();
                
            if (_showMaterialTools)
                DrawMaterialTools();
                
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            GUILayout.FlexibleSpace();
            DrawStatusBar();
        }
        
        private void DrawToolbar()
        {
            // Draw the toolbar background
            Rect toolbarRect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(toolbarRect, new Color(0.2f, 0.2f, 0.2f));
            
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // File Operations
            if (GUILayout.Button("New", EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Cube"), false, () => CreatePrimitive(PrimitiveType.Cube));
                menu.AddItem(new GUIContent("Sphere"), false, () => CreatePrimitive(PrimitiveType.Sphere));
                menu.AddItem(new GUIContent("Cylinder"), false, () => CreatePrimitive(PrimitiveType.Cylinder));
                menu.AddItem(new GUIContent("Plane"), false, () => CreatePrimitive(PrimitiveType.Plane));
                menu.ShowAsContext();
            }
            
            if (GUILayout.Button("Import", EditorStyles.toolbarButton))
            {
                // Import functionality
            }
            
            if (GUILayout.Button("Export", EditorStyles.toolbarButton))
            {
                // Export functionality
            }
            
            GUILayout.FlexibleSpace();
            
            // Tool modes
            DrawToolModeButton(ToolMode.View, "View");
            DrawToolModeButton(ToolMode.Select, "Select");
            DrawToolModeButton(ToolMode.Move, "Move");
            DrawToolModeButton(ToolMode.Rotate, "Rotate");
            DrawToolModeButton(ToolMode.Scale, "Scale");
            
            GUILayout.FlexibleSpace();
            
            // Selection modes (only shown when in select mode)
            if (_currentToolMode == ToolMode.Select)
            {
                DrawSelectionModeButton(SelectionMode.Object, "Obj");
                DrawSelectionModeButton(SelectionMode.Vertex, "Vert");
                DrawSelectionModeButton(SelectionMode.Edge, "Edge");
                DrawSelectionModeButton(SelectionMode.Face, "Face");
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawToolModeButton(ToolMode mode, string label)
        {
            bool isActive = _currentToolMode == mode;
            
            if (ModelingToolkitStyles.ToolbarToggleButton(label, isActive))
            {
                _currentToolMode = mode;
                SceneView.RepaintAll();
            }
        }
        
        private void DrawSelectionModeButton(SelectionMode mode, string label)
        {
            bool isActive = _selectionMode == mode;
            
            if (ModelingToolkitStyles.ToolbarToggleButton(label, isActive))
            {
                _selectionMode = mode;
                SceneView.RepaintAll();
            }
        }
        
        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            
            if (ModelingToolkitStyles.TabToggleButton("Modeling", _showModelingTools))
            {
                _showModelingTools = !_showModelingTools;
            }
            
            if (ModelingToolkitStyles.TabToggleButton("UV Editing", _showUVTools))
            {
                _showUVTools = !_showUVTools;
            }
            
            if (ModelingToolkitStyles.TabToggleButton("Texturing", _showTextureTools))
            {
                _showTextureTools = !_showTextureTools;
            }
            
            if (ModelingToolkitStyles.TabToggleButton("Materials", _showMaterialTools))
            {
                _showMaterialTools = !_showMaterialTools;
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawModelingTools()
        {
            ModelingToolkitStyles.DrawHeader("Modeling Tools");
            
            ModelingToolkitStyles.BeginPanel();
            
            EditorGUILayout.Space();
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Extrude"))
            {
                _currentToolMode = ToolMode.Extrude;
            }
            
            if (GUILayout.Button("Bevel"))
            {
                _currentToolMode = ToolMode.Bevel;
            }
            
            if (GUILayout.Button("Loop Cut"))
            {
                _currentToolMode = ToolMode.LoopCut;
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Subdivide"))
            {
                // Subdivide mesh
            }
            
            if (GUILayout.Button("Smooth"))
            {
                // Smooth mesh
            }
            
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            ModelingToolkitStyles.DrawHeader("Boolean Operations");
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Union"))
            {
                // Union operation
            }
            
            if (GUILayout.Button("Subtract"))
            {
                // Subtract operation
            }
            
            if (GUILayout.Button("Intersect"))
            {
                // Intersect operation
            }
            
            GUILayout.EndHorizontal();
            
            ModelingToolkitStyles.EndPanel();
        }
        
        private void DrawUVTools()
        {
            ModelingToolkitStyles.DrawHeader("UV Editing Tools");
            
            ModelingToolkitStyles.BeginPanel();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Generate UVs"))
            {
                // Auto-generate UVs
            }
            
            if (GUILayout.Button("Open UV Editor"))
            {
                // Open UV editor
                UVEditor.ShowWindow();
            }
            
            EditorGUILayout.Space();
            
            // UV preview and editing will go here
            
            ModelingToolkitStyles.EndPanel();
        }
        
        private void DrawTextureTools()
        {
            ModelingToolkitStyles.DrawHeader("Texture Painting Tools");
            
            ModelingToolkitStyles.BeginPanel();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Start Painting"))
            {
                _currentToolMode = ToolMode.Paint;
                TexturePaintingWindow.ShowWindow();
            }
            
            EditorGUILayout.Space();
            
            // Texture painting options will go here
            ModelingToolkitStyles.DrawHeader("Brush Settings");
            
            // Brush size slider
            EditorGUILayout.Slider("Brush Size", 1f, 0.1f, 10f);
            
            // Brush hardness slider
            EditorGUILayout.Slider("Hardness", 0.5f, 0f, 1f);
            
            // Brush color field
            EditorGUILayout.ColorField("Color", Color.white);
            
            ModelingToolkitStyles.EndPanel();
        }
        
        private void DrawMaterialTools()
        {
            ModelingToolkitStyles.DrawHeader("Material Editor");
            
            ModelingToolkitStyles.BeginPanel();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Open Material Editor"))
            {
                MaterialEditor.ShowWindow();
            }
            
            EditorGUILayout.Space();
            
            // PBR material editing controls will go here
            EditorGUILayout.ColorField("Base Color", Color.white);
            EditorGUILayout.Slider("Metallic", 0f, 0f, 1f);
            EditorGUILayout.Slider("Smoothness", 0.5f, 0f, 1f);
            EditorGUILayout.ColorField("Emission Color", Color.black);
            EditorGUILayout.Slider("Emission Intensity", 0f, 0f, 10f);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Apply Material"))
            {
                // Apply material to selected object
            }
            
            ModelingToolkitStyles.EndPanel();
        }
        
        private void DrawStatusBar()
        {
            // Draw status bar background
            Rect statusRect = EditorGUILayout.GetControlRect(false, 24);
            EditorGUI.DrawRect(statusRect, new Color(0.25f, 0.25f, 0.25f));
            
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Display current selection info
            if (_activeMesh != null)
            {
                EditorGUILayout.LabelField($"Vertices: {_activeMesh.vertexCount} | Triangles: {_activeMesh.triangles.Length / 3}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("No mesh selected", EditorStyles.miniLabel);
            }
            
            GUILayout.FlexibleSpace();
            
            // Display current tool mode
            EditorGUILayout.LabelField($"Mode: {_currentToolMode}", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (_window == null)
                return;
                
            // Handle scene view interaction based on current tool mode
            _window.HandleSceneGUI(sceneView);
        }
        
        private void HandleSceneGUI(SceneView sceneView)
        {
            // Get the currently selected object
            if (Selection.activeGameObject != null)
            {
                MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    _activeMeshFilter = meshFilter;
                    _activeMesh = meshFilter.sharedMesh;
                }
            }
            
            // Handle different tool modes
            switch (_currentToolMode)
            {
                case ToolMode.Select:
                    HandleSelectMode(sceneView);
                    break;
                case ToolMode.Move:
                    HandleMoveMode(sceneView);
                    break;
                case ToolMode.Rotate:
                    HandleRotateMode(sceneView);
                    break;
                case ToolMode.Scale:
                    HandleScaleMode(sceneView);
                    break;
                case ToolMode.Extrude:
                    HandleExtrudeMode(sceneView);
                    break;
                case ToolMode.Bevel:
                    HandleBevelMode(sceneView);
                    break;
                case ToolMode.LoopCut:
                    HandleLoopCutMode(sceneView);
                    break;
                case ToolMode.Paint:
                    HandlePaintMode(sceneView);
                    break;
            }
            
            // Force the scene view to repaint
            sceneView.Repaint();
        }
        
        private void HandleSelectMode(SceneView sceneView)
        {
            // Selection handling code based on selection mode (vertex, edge, face, object)
            switch (_selectionMode)
            {
                case SelectionMode.Vertex:
                    // Vertex selection
                    break;
                case SelectionMode.Edge:
                    // Edge selection
                    break;
                case SelectionMode.Face:
                    // Face selection
                    break;
                case SelectionMode.Object:
                    // Object selection is handled by Unity
                    break;
            }
        }
        
        private void HandleMoveMode(SceneView sceneView)
        {
            // Move tool implementation
        }
        
        private void HandleRotateMode(SceneView sceneView)
        {
            // Rotate tool implementation
        }
        
        private void HandleScaleMode(SceneView sceneView)
        {
            // Scale tool implementation
        }
        
        private void HandleExtrudeMode(SceneView sceneView)
        {
            // Extrude tool implementation
        }
        
        private void HandleBevelMode(SceneView sceneView)
        {
            // Bevel tool implementation
        }
        
        private void HandleLoopCutMode(SceneView sceneView)
        {
            // Loop cut tool implementation
        }
        
        private void HandlePaintMode(SceneView sceneView)
        {
            // Texture painting tool implementation
        }
        
        private void CreatePrimitive(PrimitiveType primitiveType)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = $"{primitiveType} (Modeling Toolkit)";
            
            // Position the new primitive in front of the scene view camera
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                primitive.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
            }
            
            // Select the newly created primitive
            Selection.activeGameObject = primitive;
            
            // Set it as our active mesh
            _activeMeshFilter = primitive.GetComponent<MeshFilter>();
            _activeMesh = _activeMeshFilter.sharedMesh;
        }
        
        private void OnDestroy()
        {
            // Clean up resources
            ModelingToolkitStyles.Cleanup();
        }
    }
} 