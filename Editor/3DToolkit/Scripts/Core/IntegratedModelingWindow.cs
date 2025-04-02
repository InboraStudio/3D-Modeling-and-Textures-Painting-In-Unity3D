using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

namespace ModelingToolkit
{
    // Helper class to manage Unity-style gizmos and handle interactivity
    public static class UnityGizmoUtility
    {
        // Cached materials for custom gizmo drawing
        private static Material _lineMaterial;
        private static Material _handleMaterial;
        
        // Common colors for gizmos matching Unity's standard colors
        public static readonly Color xAxisColor = new Color(1f, 0.15f, 0.15f, 0.8f);
        public static readonly Color yAxisColor = new Color(0.15f, 1f, 0.15f, 0.8f);
        public static readonly Color zAxisColor = new Color(0.15f, 0.15f, 1f, 0.8f);
        public static readonly Color highlightColor = new Color(1f, 1f, 0.15f, 0.8f);
        public static readonly Color selectionColor = new Color(0.2f, 0.7f, 1f, 0.7f);
        
        // Initialize materials if needed
        public static void EnsureMaterialsInitialized()
        {
            if (_lineMaterial == null)
            {
                _lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _lineMaterial.SetInt("_ZWrite", 0);
            }
            
            if (_handleMaterial == null)
            {
                _handleMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                _handleMaterial.hideFlags = HideFlags.HideAndDontSave;
                _handleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _handleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _handleMaterial.SetInt("_ZWrite", 0);
                _handleMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }
        }
        
        // Draw a position handle with customizable appearance
        public static Vector3 PositionHandle(Vector3 position, Quaternion rotation, float handleSize, Color xColor, Color yColor, Color zColor)
        {
            // Use Unity's built-in position handle but with customized appearance
            Vector3 originalPosition = position;
            Color originalHandlesColor = Handles.color;
            
            // Set custom colors for each axis
            // Using Unity's internal handle methods for consistent appearance
            Handles.color = xColor;
            position = Handles.Slider(position, rotation * Vector3.right, handleSize, Handles.ArrowHandleCap, 0.2f);
            
            Handles.color = yColor;
            position = Handles.Slider(position, rotation * Vector3.up, handleSize, Handles.ArrowHandleCap, 0.2f);
            
            Handles.color = zColor;
            position = Handles.Slider(position, rotation * Vector3.forward, handleSize, Handles.ArrowHandleCap, 0.2f);
            
            // Draw center cube for free translation
            Handles.color = originalHandlesColor;
            position = Handles.FreeMoveHandle(position, handleSize * 0.1f, Vector3.zero, Handles.RectangleHandleCap);
            
            // Restore original handle color
            Handles.color = originalHandlesColor;
            
            // Return the new position
            return position;
        }
        
        // Draw a rotation handle with customizable appearance
        public static Quaternion RotationHandle(Quaternion rotation, Vector3 position, float handleSize, Color xColor, Color yColor, Color zColor)
        {
            Color originalHandlesColor = Handles.color;
            
            // Use Unity's standard rotation handle with custom colors
            Quaternion originalRotation = rotation;
            
            // Draw custom rotation circles for each axis
            Handles.color = xColor;
            rotation = Handles.Disc(rotation, position, rotation * Vector3.right, handleSize, false, 0);
            
            Handles.color = yColor;
            rotation = Handles.Disc(rotation, position, rotation * Vector3.up, handleSize, false, 0);
            
            Handles.color = zColor;
            rotation = Handles.Disc(rotation, position, rotation * Vector3.forward, handleSize, false, 0);
            
            // Restore original handle color
            Handles.color = originalHandlesColor;
            
            return rotation;
        }
        
        // Draw a scale handle with customizable appearance
        public static Vector3 ScaleHandle(Vector3 scale, Vector3 position, Quaternion rotation, float handleSize, Color xColor, Color yColor, Color zColor)
        {
            Color originalHandlesColor = Handles.color;
            
            // Use Unity's standard scale handle with custom colors
            // First draw individual axis scale handles
            Handles.color = xColor;
            float xScale = Handles.ScaleSlider(scale.x, position, rotation * Vector3.right, rotation, handleSize, 0.15f);
            
            Handles.color = yColor;
            float yScale = Handles.ScaleSlider(scale.y, position, rotation * Vector3.up, rotation, handleSize, 0.15f);
            
            Handles.color = zColor;
            float zScale = Handles.ScaleSlider(scale.z, position, rotation * Vector3.forward, rotation, handleSize, 0.15f);
            
            // Draw center cube for uniform scaling
            Handles.color = Color.white;
            float uniformScale = Handles.ScaleValueHandle(
                1f, position, rotation, handleSize * 0.15f, Handles.CubeHandleCap, 0.15f);
            
            // Check if uniform scaling was performed
            if (Mathf.Abs(uniformScale - 1f) > 0.001f)
            {
                scale *= uniformScale;
            }
            else
            {
                // Apply axis scaling
                if (Mathf.Abs(xScale - scale.x) > 0.001f) scale.x = xScale;
                if (Mathf.Abs(yScale - scale.y) > 0.001f) scale.y = yScale;
                if (Mathf.Abs(zScale - scale.z) > 0.001f) scale.z = zScale;
            }
            
            // Restore original handle color
            Handles.color = originalHandlesColor;
            
            return scale;
        }
        
        // Draw a selection box around the specified bounds
        public static void DrawSelectionBox(Bounds bounds, Color color)
        {
            Color original = Handles.color;
            Handles.color = color;
            
            Handles.DrawWireCube(bounds.center, bounds.size);
            
            // Draw small handles at the corners for visual feedback
            float handleSize = HandleUtility.GetHandleSize(bounds.center) * 0.05f;
            
            Vector3 extents = bounds.extents;
            Vector3 center = bounds.center;
            
            // Draw 8 corner points
            Vector3[] corners = new Vector3[8];
            corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            corners[1] = center + new Vector3(extents.x, -extents.y, -extents.z);
            corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
            corners[3] = center + new Vector3(extents.x, extents.y, -extents.z);
            corners[4] = center + new Vector3(-extents.x, -extents.y, extents.z);
            corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
            corners[6] = center + new Vector3(-extents.x, extents.y, extents.z);
            corners[7] = center + new Vector3(extents.x, extents.y, extents.z);
            
            foreach (Vector3 corner in corners)
            {
                Handles.DotHandleCap(0, corner, Quaternion.identity, handleSize, EventType.Repaint);
            }
            
            Handles.color = original;
        }
        
        // Draw a ground grid matching Unity's default appearance
        public static void DrawGrid(float size, float cellSize, Color mainColor, Color secondaryColor)
        {
            // Draw coordinate axes 
            float axisLength = size / 2f;
            float axisThickness = 3f;
            
            // Draw X axis (red)
            Handles.color = xAxisColor;
            Handles.DrawLine(new Vector3(-axisLength, 0, 0), new Vector3(axisLength, 0, 0), axisThickness);
            
            // Draw Z axis (blue) 
            Handles.color = zAxisColor;
            Handles.DrawLine(new Vector3(0, 0, -axisLength), new Vector3(0, 0, axisLength), axisThickness);
            
            // Draw Y axis (green)
            Handles.color = yAxisColor;
            Handles.DrawLine(new Vector3(0, -axisLength, 0), new Vector3(0, axisLength, 0), axisThickness);
            
            // Set up grid parameters
            int gridLines = Mathf.FloorToInt(size / cellSize);
            
            // Make grid lines even
            if (gridLines % 2 != 0)
                gridLines++;
            
            // Draw the grid lines with Handles
            for (int i = -gridLines/2; i <= gridLines/2; i++)
            {
                float pos = i * cellSize;
                float lineThickness = 1f;
                
                // Choose line color based on position
                if (i == 0)
                {
                    Handles.color = mainColor;
                    lineThickness = 2f;
                }
                else if (i % 5 == 0)
                {
                    Handles.color = secondaryColor;
                    lineThickness = 1.5f;
                }
                else
                {
                    Handles.color = new Color(
                        secondaryColor.r * 0.7f, 
                        secondaryColor.g * 0.7f, 
                        secondaryColor.b * 0.7f, 
                        secondaryColor.a * 0.7f);
                    lineThickness = 0.5f;
                }
                
                // X axis grid lines (along Z)
                Handles.DrawLine(
                    new Vector3(pos, 0, -size/2), 
                    new Vector3(pos, 0, size/2),
                    lineThickness);
                
                // Z axis grid lines (along X)
                Handles.DrawLine(
                    new Vector3(-size/2, 0, pos), 
                    new Vector3(size/2, 0, pos),
                    lineThickness);
            }
            
            // Draw a center point
            float centerSize = cellSize * 0.1f;
            Handles.color = Color.white;
            Handles.DrawWireCube(Vector3.zero, new Vector3(centerSize, centerSize, centerSize));
        }
        
        // Fallback method for drawing grid when Camera.current is not available
        private static void DrawFallbackGrid(float size, float cellSize, Color mainColor, Color secondaryColor)
        {
            // Draw a simpler grid using Handles which doesn't need Camera.current
            int gridLines = Mathf.FloorToInt(size / cellSize);
            if (gridLines % 2 != 0) gridLines++;
            
            // Draw coordinate axes first
            float axisLength = size / 2f;
            float axisThickness = 3f;
            
            // Draw X axis (red)
            Handles.color = xAxisColor;
            Handles.DrawLine(new Vector3(-axisLength, 0, 0), new Vector3(axisLength, 0, 0), axisThickness);
            
            // Draw Z axis (blue) 
            Handles.color = zAxisColor;
            Handles.DrawLine(new Vector3(0, 0, -axisLength), new Vector3(0, 0, axisLength), axisThickness);
            
            // Draw Y axis (green)
            Handles.color = yAxisColor;
            Handles.DrawLine(new Vector3(0, -axisLength, 0), new Vector3(0, axisLength, 0), axisThickness);
            
            // Draw grid lines using Handles
            for (int i = -gridLines/2; i <= gridLines/2; i++)
            {
                float pos = i * cellSize;
                
                // Choose line color based on position
                if (i == 0)
                    Handles.color = mainColor; // Main axis lines
                else if (i % 5 == 0)
                    Handles.color = secondaryColor; // Major lines
                else
                    Handles.color = new Color(secondaryColor.r * 0.7f, secondaryColor.g * 0.7f, secondaryColor.b * 0.7f, secondaryColor.a * 0.7f); // Minor lines
                
                float lineThickness = (i == 0) ? 2f : (i % 5 == 0) ? 1f : 0.5f;
                
                // X axis grid lines (along Z)
                Handles.DrawLine(new Vector3(pos, 0, -size/2), new Vector3(pos, 0, size/2), lineThickness);
                
                // Z axis grid lines (along X)
                Handles.DrawLine(new Vector3(-size/2, 0, pos), new Vector3(size/2, 0, pos), lineThickness);
            }
            
            // Draw a center point
            float centerSize = cellSize * 0.1f;
            Handles.color = Color.white;
            Handles.DrawWireCube(Vector3.zero, new Vector3(centerSize, centerSize, centerSize));
        }
    }

    public class IntegratedModelingWindow : EditorWindow
    {
        // Main toolbar selection
        private enum ToolMode
        {
            Model,
            UV,
            Texture,
            Material
        }
        
        private ToolMode _currentToolMode = ToolMode.Model;
        
        // Modeling mode selection
        private enum ModelingTool
        {
            Select,
            Move,
            Rotate,
            Scale,
            Extrude,
            Bevel,
            LoopCut
        }
        
        private ModelingTool _currentModelingTool = ModelingTool.Select;
        
        // Selection mode
        private enum SelectionMode
        {
            Object,
            Vertex,
            Edge,
            Face
        }
        
        private SelectionMode _selectionMode = SelectionMode.Object;
        
        // Camera control
        private Vector2 _cameraRotation = new Vector2(0, 0);
        private float _cameraDistance = 5f;
        private Vector3 _cameraPanOffset = Vector3.zero;
        private bool _isDraggingCamera = false;
        private Vector2 _lastMousePosition;
        
        // Target object
        private GameObject _targetObject;
        private MeshFilter _meshFilter;
        private Mesh _workingMesh;
        
        // Components
        private UVEditor _uvEditor;
        private TexturePainter _texturePainter;
        private MaterialEditor _materialEditor;
        
        // Styles
        private GUIStyle _toolbarButtonStyle;
        private GUIStyle _viewportStyle;
        
        // Add PreviewRenderUtility field
        private PreviewRenderUtility _previewUtility;
        
        // Add required field for axis constraint
        private enum AxisConstraint
        {
            None,
            X,
            Y,
            Z
        }
        
        private AxisConstraint _axisConstraint = AxisConstraint.None;
        private bool _isTransforming = false;
        
        // Add a field to track object position
        private Vector3 _lastKnownPosition = Vector3.zero;
        
        // Cache grid materials and meshes
        private Material _gridMaterial;
        private Mesh _groundMesh;
        private Material _lineMaterial;
        
        [MenuItem("Window/3D Toolkit/Integrated Modeling Tool")]
        public static void ShowWindow()
        {
            IntegratedModelingWindow window = GetWindow<IntegratedModelingWindow>();
            window.titleContent = new GUIContent("3D Modeling Tool");
            window.minSize = new Vector2(800, 600);
        }
        
        private void OnEnable()
        {
            // Initialize preview utility
            if (_previewUtility == null)
            {
                _previewUtility = new PreviewRenderUtility();
                _previewUtility.cameraFieldOfView = 60f;
                _previewUtility.camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1);
                _previewUtility.camera.clearFlags = CameraClearFlags.Color;
                _previewUtility.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
                _previewUtility.lights[0].intensity = 1.2f;
                _previewUtility.lights[1].intensity = 0.7f;
            }
            
            // Initialize line material for gizmos
            if (_lineMaterial == null)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader);
                _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _lineMaterial.SetInt("_ZWrite", 0);
            }
            
            // Create UV Editor
            if (_uvEditor == null)
                _uvEditor = CreateInstance<UVEditor>();
            
            // Create Texture Painter
            if (_texturePainter == null)
                _texturePainter = new TexturePainter();
            
            // Get Material Editor instance
            if (_materialEditor == null)
                _materialEditor = MaterialEditor.Instance;
            
            // Try to get currently selected object
            if (Selection.activeGameObject != null)
            {
                SetTargetObject(Selection.activeGameObject);
            }
            
            // Make sure SceneView callbacks are registered
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
            
            // Set up default camera view
            _cameraRotation = new Vector2(30, -60);
            _cameraDistance = 5f;
            
            // Make sure we repaint constantly during editor updates
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            
            // Subscribe to selection change events
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
            
            // Ensure window gets redrawn frequently (especially for gizmos)
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            SceneView.duringSceneGui -= OnSceneViewGUI;
            EditorApplication.update -= OnEditorUpdate;
            Selection.selectionChanged -= OnSelectionChanged;
            
            // Properly clean up the UV Editor
            if (_uvEditor != null)
            {
                try
                {
                    _uvEditor.Close();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Error closing UV Editor: " + e.Message);
                }
                _uvEditor = null;
            }
            
            if (_texturePainter != null)
            {
                _texturePainter.Cleanup();
                _texturePainter = null;
            }
            
            // Clean up the preview utility
            if (_previewUtility != null)
            {
                _previewUtility.Cleanup();
                _previewUtility = null;
            }
            
            // Clean up materials
            if (_lineMaterial != null)
            {
                DestroyImmediate(_lineMaterial);
                _lineMaterial = null;
            }
            
            if (_gridMaterial != null)
            {
                DestroyImmediate(_gridMaterial);
                _gridMaterial = null;
            }
            
            // Clean up meshes
            if (_groundMesh != null)
            {
                DestroyImmediate(_groundMesh);
                _groundMesh = null;
            }
            
            // Clear mesh references
            _workingMesh = null;
            _meshFilter = null;
            _targetObject = null;
        }
        
        private void OnSelectionChanged()
        {
            // Update target when selection changes in the editor
            if (Selection.activeGameObject != null)
            {
                SetTargetObject(Selection.activeGameObject);
                Repaint();
            }
        }
        
        private void OnEditorUpdate()
        {
            // Only update when absolutely necessary - reduce editor overhead
            if (!EditorWindow.focusedWindow || EditorWindow.focusedWindow != this)
                return;
                
            // Limit repaints to 10 frames per second maximum
            if (EditorApplication.timeSinceStartup - _lastRepaintTime < 0.1f)
                return;
                
            bool needsRepaint = false;
            
            // Only repaint when camera is being actively manipulated
            if (_isDraggingCamera || _isTransforming)
            {
                needsRepaint = true;
            }
            
            // Only check object movement when actually selecting an object
            if (_targetObject != null)
            {
                // Only check position every few frames to save CPU
                if (_frameSkipCounter++ % 5 == 0)
                {
                    Vector3 currentPos = _targetObject.transform.position;
                    
                    // If position changed significantly outside our control, repaint
                    if (Vector3.Distance(currentPos, _lastKnownPosition) > 0.01f)
                    {
                        _lastKnownPosition = currentPos;
                        needsRepaint = true;
                    }
                }
            }
            
            if (needsRepaint)
            {
                _lastRepaintTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }
        
        // Add properties to track repaint timing and frame skipping
        private double _lastRepaintTime = 0;
        private int _frameSkipCounter = 0;
        
        private void OnGUI()
        {
            // Skip processing if window is minimized or hidden
            if (position.width < 50 || position.height < 50)
                return;
                
            // Only initialize styles when needed
            if (_toolbarButtonStyle == null)
            {
                InitializeStyles();
            }
            
            // Draw the toolbar
            DrawToolbar();
            
            // Draw the 3D viewport - the most expensive part
            Rect viewportRect = new Rect(0, 30, position.width, position.height - 60);
            
            // Skip viewport rendering if window is too small (performance optimization)
            if (viewportRect.width < 100 || viewportRect.height < 100)
            {
                EditorGUI.LabelField(viewportRect, "Window too small", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                DrawViewport(viewportRect);
            }
            
            // Draw the status bar
            DrawStatusBar();
            
            // Handle input events - skip when editor is compiling
            if (!EditorApplication.isCompiling)
            {
                HandleInput(viewportRect);
            }
        }
        
        private void InitializeStyles()
        {
            _toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            _toolbarButtonStyle.fixedHeight = 24;
            _toolbarButtonStyle.margin = new RectOffset(2, 2, 2, 2);
            
            _viewportStyle = new GUIStyle();
            _viewportStyle.normal.background = EditorGUIUtility.whiteTexture;
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // File operations
            if (GUILayout.Button("Import", EditorStyles.toolbarButton))
            {
                ImportModel();
            }
            
            if (GUILayout.Button("Export", EditorStyles.toolbarButton))
            {
                ExportModel();
            }
            
            GUILayout.Space(10);
            
            // Tool mode tabs
            if (GUILayout.Toggle(_currentToolMode == ToolMode.Model, "Model", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                _currentToolMode = ToolMode.Model;
                Repaint();
            }
            
            if (GUILayout.Toggle(_currentToolMode == ToolMode.UV, "UV", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                _currentToolMode = ToolMode.UV;
                Repaint();
            }
            
            if (GUILayout.Toggle(_currentToolMode == ToolMode.Texture, "Texture", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                _currentToolMode = ToolMode.Texture;
                Repaint();
            }
            
            if (GUILayout.Toggle(_currentToolMode == ToolMode.Material, "Material", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                _currentToolMode = ToolMode.Material;
                Repaint();
            }
            
            GUILayout.FlexibleSpace();
            
            // Selection tools
            if (_currentToolMode == ToolMode.Model)
            {
                DrawModelingTools();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawModelingTools()
        {
            GUILayout.Label("Tool:", EditorStyles.miniLabel, GUILayout.Width(30));
            
            if (GUILayout.Toggle(_currentModelingTool == ModelingTool.Select, "Select", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                _currentModelingTool = ModelingTool.Select;
            }
            
            if (GUILayout.Toggle(_currentModelingTool == ModelingTool.Move, "Move", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                _currentModelingTool = ModelingTool.Move;
            }
            
            if (GUILayout.Toggle(_currentModelingTool == ModelingTool.Rotate, "Rotate", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                _currentModelingTool = ModelingTool.Rotate;
            }
            
            if (GUILayout.Toggle(_currentModelingTool == ModelingTool.Scale, "Scale", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                _currentModelingTool = ModelingTool.Scale;
            }
            
            GUILayout.Space(10);
            
            // Selection mode
            GUILayout.Label("Select:", EditorStyles.miniLabel, GUILayout.Width(40));
            
            if (GUILayout.Toggle(_selectionMode == SelectionMode.Object, "Obj", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                _selectionMode = SelectionMode.Object;
            }
            
            if (GUILayout.Toggle(_selectionMode == SelectionMode.Vertex, "Vert", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                _selectionMode = SelectionMode.Vertex;
            }
            
            if (GUILayout.Toggle(_selectionMode == SelectionMode.Edge, "Edge", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                _selectionMode = SelectionMode.Edge;
            }
            
            if (GUILayout.Toggle(_selectionMode == SelectionMode.Face, "Face", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                _selectionMode = SelectionMode.Face;
            }
        }
        
        private void DrawViewport(Rect viewportRect)
        {
            // Draw background
            EditorGUI.DrawRect(viewportRect, new Color(0.2f, 0.2f, 0.2f));
            
            if (_targetObject == null)
            {
                GUI.Label(viewportRect, "No object selected. Select an object or import a model.", EditorStyles.centeredGreyMiniLabel);
                
                // Draw object selection field in the center of viewport
                Rect selectionRect = new Rect(viewportRect.center.x - 150, viewportRect.center.y - 10, 300, 20);
                GameObject newTarget = EditorGUI.ObjectField(selectionRect, _targetObject, typeof(GameObject), true) as GameObject;
                
                // Handle drag and drop
                if (Event.current.type == EventType.DragUpdated && viewportRect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                
                if (Event.current.type == EventType.DragPerform && viewportRect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.AcceptDrag();
                    
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        GameObject go = draggedObject as GameObject;
                        if (go != null)
                        {
                            newTarget = go;
                            break;
                        }
                        
                        // Check for model assets
                        if (draggedObject is Mesh)
                        {
                            Mesh mesh = draggedObject as Mesh;
                            // Create a new GameObject with this mesh
                            GameObject newObject = new GameObject(mesh.name);
                            MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
                            meshFilter.sharedMesh = mesh;
                            MeshRenderer renderer = newObject.AddComponent<MeshRenderer>();
                            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
                            
                            newTarget = newObject;
                            break;
                        }
                    }
                    
                    Event.current.Use();
                }
                
                if (newTarget != null && newTarget != _targetObject)
                {
                    SetTargetObject(newTarget);
                }
                
                return;
            }
            
            // Camera setup and render the preview
            RenderPreview(viewportRect);
            
            switch (_currentToolMode)
            {
                case ToolMode.Model:
                    // Standard 3D modeling tools are already visible in toolbar
                    break;
                    
                case ToolMode.UV:
                    DrawUVControls(viewportRect);
                    break;
                    
                case ToolMode.Texture:
                    DrawTextureControls(viewportRect);
                    break;
                    
                case ToolMode.Material:
                    DrawMaterialControls(viewportRect);
                    break;
            }
        }
        
        private void RenderPreview(Rect viewportRect)
        {
            // Skip if not in repaint event to avoid redundant work
            if (Event.current.type != EventType.Repaint)
                return;
                
            if (_previewUtility == null || _targetObject == null)
                return;
                
            // Low quality rendering for better performance
            _previewUtility.camera.fieldOfView = 60f;
            _previewUtility.camera.nearClipPlane = 0.1f;
            _previewUtility.camera.farClipPlane = 100f;
                
            // Only try to render if we have a valid mesh or renderers
            bool hasValidContent = false;
            
            if (_meshFilter != null && _meshFilter.sharedMesh != null)
            {
                hasValidContent = true;
            }
            else if (_targetObject != null)
            {
                // Only do this expensive check occasionally
                if (_frameSkipCounter % 10 == 0)
                {
                    // Check if the object has any renderers - expensive operation
                    Renderer[] renderers = _targetObject.GetComponentsInChildren<Renderer>();
                    hasValidContent = renderers.Length > 0;
                }
            }
            
            if (!hasValidContent)
                return;
                
            try
            {
                // Cache renderer references to avoid expensive GetComponent calls
                if (_cachedRenderers == null || _rendererCacheExpired)
                {
                    CacheRenderers();
                }
                
                List<Renderer> renderersToShow = _cachedRenderers;
                
                // Normalize camera rotation angles for consistency
                _cameraRotation.y = _cameraRotation.y % 360f;
                
                // Set up camera position with improved orbit mechanics
                Quaternion camRotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
                Vector3 camPos = camRotation * (Vector3.back * _cameraDistance);
                
                // Apply pan offset in camera-local space for more intuitive panning
                Vector3 right = camRotation * Vector3.right;
                Vector3 up = camRotation * Vector3.up;
                Vector3 adjustedPanOffset = right * _cameraPanOffset.x + up * _cameraPanOffset.y;
                
                camPos += adjustedPanOffset;
                
                _previewUtility.camera.transform.position = camPos;
                _previewUtility.camera.transform.rotation = camRotation;
                _previewUtility.camera.transform.LookAt(adjustedPanOffset); 
                
                // Begin drawing the preview
                _previewUtility.BeginPreview(viewportRect, GUIStyle.none);
                
                // Only draw grid when close to object
                if (_cameraDistance < 15f)
                {
                    DrawPreviewGrid();
                }
                
                // Draw the model(s)
                foreach (Renderer renderer in renderersToShow)
                {
                    if (renderer == null)
                        continue;

                    // Create a matrix that represents the transform of the renderer
                    Matrix4x4 matrix = renderer.transform.localToWorldMatrix;
                    
                    if (renderer is MeshRenderer meshRenderer && renderer.GetComponent<MeshFilter>() != null)
                    {
                        MeshFilter filter = renderer.GetComponent<MeshFilter>();
                        if (filter.sharedMesh != null)
                        {
                            Material[] materials = renderer.sharedMaterials;
                            
                            // Only draw visible submeshes
                            int submeshCount = Mathf.Min(filter.sharedMesh.subMeshCount, materials.Length);
                            
                            // Use simplified materials for preview
                            if (_simplifiedMaterial == null)
                            {
                                _simplifiedMaterial = new Material(Shader.Find("Standard"));
                                _simplifiedMaterial.hideFlags = HideFlags.HideAndDontSave;
                            }
                            
                            for (int i = 0; i < submeshCount; i++)
                            {
                                Material renderMat = _simplifiedMaterial;
                                
                                if (materials[i] != null)
                                {
                                    renderMat.color = materials[i].color;
                                }
                                else
                                {
                                    renderMat.color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
                                }
                                
                                _previewUtility.DrawMesh(filter.sharedMesh, matrix, renderMat, i);
                            }
                        }
                    }
                    else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                    {
                        if (skinnedMeshRenderer.sharedMesh != null)
                        {
                            if (_simplifiedMaterial == null)
                            {
                                _simplifiedMaterial = new Material(Shader.Find("Standard"));
                                _simplifiedMaterial.hideFlags = HideFlags.HideAndDontSave;
                            }
                            
                            _simplifiedMaterial.color = renderer.sharedMaterial != null ? 
                                                         renderer.sharedMaterial.color : 
                                                         new Color(0.8f, 0.8f, 0.8f, 1.0f);
                                                     
                            _previewUtility.DrawMesh(skinnedMeshRenderer.sharedMesh, matrix, _simplifiedMaterial, 0);
                        }
                    }
                }
                
                // Faster rendering options
                _previewUtility.Render();
                
                Texture previewTexture = _previewUtility.EndPreview();
                
                // Draw the rendered preview texture
                GUI.DrawTexture(viewportRect, previewTexture, ScaleMode.StretchToFill, false);
                
                // Only draw gizmos when actually transforming (huge performance gain)
                if (_isTransforming || _currentModelingTool != ModelingTool.Select)
                {
                    DrawTransformGizmos(viewportRect);
                }
                
                // Only draw overlay when not transforming to save performance
                if (!_isTransforming)
                {
                    DrawOverlayInfo(viewportRect);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error rendering preview: " + e.Message);
                
                try {
                    _previewUtility.EndPreview();
                } catch {
                    // Suppress errors
                }
            }
        }
        
        // Cache variables for improved performance
        private List<Renderer> _cachedRenderers = new List<Renderer>();
        private bool _rendererCacheExpired = true;
        private Material _simplifiedMaterial;
        
        private void CacheRenderers()
        {
            _cachedRenderers.Clear();
            
            if (_meshFilter != null)
            {
                Renderer targetRenderer = _meshFilter.GetComponent<Renderer>();
                if (targetRenderer != null)
                {
                    _cachedRenderers.Add(targetRenderer);
                }
            }
            
            // If no renderer from mesh filter, get all from children
            if (_cachedRenderers.Count == 0 && _targetObject != null)
            {
                // Only get top level renderers to avoid performance issues
                foreach (Renderer childRenderer in _targetObject.GetComponents<Renderer>())
                {
                    _cachedRenderers.Add(childRenderer);
                }
                
                // Only add direct children renderers if we still have nothing
                if (_cachedRenderers.Count == 0)
                {
                    foreach (Transform child in _targetObject.transform)
                    {
                        Renderer childRenderer = child.GetComponent<Renderer>();
                        if (childRenderer != null)
                            _cachedRenderers.Add(childRenderer);
                    }
                }
            }
            
            _rendererCacheExpired = false;
        }
        
        private void DrawPreviewGrid()
        {
            // Skip if preview utility is not available
            if (_previewUtility == null)
                return;
                
            // Create grid material if not cached
            if (_gridMaterial == null)
            {
                _gridMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                _gridMaterial.hideFlags = HideFlags.HideAndDontSave;
                _gridMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _gridMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _gridMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _gridMaterial.SetInt("_ZWrite", 0);
            }
            
            // Create ground mesh if not cached
            if (_groundMesh == null)
            {
                const float meshSize = 20f;
                _groundMesh = new Mesh();
                _groundMesh.vertices = new Vector3[] {
                    new Vector3(-meshSize/2, 0, -meshSize/2),
                    new Vector3(meshSize/2, 0, -meshSize/2),
                    new Vector3(-meshSize/2, 0, meshSize/2),
                    new Vector3(meshSize/2, 0, meshSize/2)
                };
                _groundMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
                _groundMesh.normals = new Vector3[] {
                    Vector3.up, Vector3.up, Vector3.up, Vector3.up
                };
                _groundMesh.uv = new Vector2[] {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                _groundMesh.hideFlags = HideFlags.HideAndDontSave;
            }
            
            // Use consistent grid size and cell size
            float gridSize = 20f;
            float cellSize = 1f;
            
            // Set colors for grid lines
            Color mainAxisColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            Color secondaryLineColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            
            // Draw ground plane with a subtle color
            _gridMaterial.SetPass(0);
            _gridMaterial.color = new Color(0.3f, 0.3f, 0.3f, 0.2f);
            _previewUtility.DrawMesh(_groundMesh, Matrix4x4.identity, _gridMaterial, 0);
            
            // Draw coordinate axes as simple lines
            // X axis (red)
            DrawPreviewLine(Vector3.zero, Vector3.right * gridSize/2, UnityGizmoUtility.xAxisColor, 2f);
            // Y axis (green)
            DrawPreviewLine(Vector3.zero, Vector3.up * gridSize/2, UnityGizmoUtility.yAxisColor, 2f);
            // Z axis (blue)
            DrawPreviewLine(Vector3.zero, Vector3.forward * gridSize/2, UnityGizmoUtility.zAxisColor, 2f);
            
            // Draw grid lines
            int gridLines = Mathf.FloorToInt(gridSize / cellSize);
            if (gridLines % 2 != 0) gridLines++;
            
            // Only draw grid lines for even indices for performance
            for (int i = -gridLines/2; i <= gridLines/2; i += 2)
            {
                float pos = i * cellSize;
                Color lineColor;
                float lineThickness;
                
                // Choose line color and thickness based on position
                if (i == 0)
                {
                    lineColor = mainAxisColor;
                    lineThickness = 1.5f;
                }
                else if (i % 10 == 0)
                {
                    lineColor = secondaryLineColor;
                    lineThickness = 1f;
                }
                else
                {
                    // Skip minor lines when zoomed out
                    if (_cameraDistance > 10f)
                        continue;
                        
                    lineColor = new Color(secondaryLineColor.r * 0.7f, secondaryLineColor.g * 0.7f, secondaryLineColor.b * 0.7f, secondaryLineColor.a * 0.5f);
                    lineThickness = 0.5f;
                }
                
                // X axis grid lines (along Z)
                DrawPreviewLine(
                    new Vector3(pos, 0.01f, -gridSize/2),
                    new Vector3(pos, 0.01f, gridSize/2),
                    lineColor,
                    lineThickness);
                
                // Z axis grid lines (along X)
                DrawPreviewLine(
                    new Vector3(-gridSize/2, 0.01f, pos),
                    new Vector3(gridSize/2, 0.01f, pos),
                    lineColor,
                    lineThickness);
            }
            
            // Add better lighting
            _previewUtility.lights[0].intensity = 1.0f;
            _previewUtility.lights[0].transform.rotation = Quaternion.Euler(50f, 30f, 0f);
            _previewUtility.lights[1].intensity = 0.5f;
            _previewUtility.lights[1].transform.rotation = Quaternion.Euler(-30f, -60f, 0f);
        }
        
        // Simple grid drawing method using PreviewRenderUtility's drawing methods
        private void DrawSimpleGridLines(float gridSize, float cellSize, Color mainColor, Color secondaryColor)
        {
            // Create a simple grid material
            Material gridMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            gridMaterial.hideFlags = HideFlags.HideAndDontSave;
            gridMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            gridMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            gridMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            gridMaterial.SetInt("_ZWrite", 0);

            // Draw the ground plane first
            Mesh groundMesh = new Mesh();
            groundMesh.vertices = new Vector3[] {
                new Vector3(-gridSize/2, 0, -gridSize/2),
                new Vector3(gridSize/2, 0, -gridSize/2),
                new Vector3(-gridSize/2, 0, gridSize/2),
                new Vector3(gridSize/2, 0, gridSize/2)
            };
            groundMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            groundMesh.normals = new Vector3[] {
                Vector3.up, Vector3.up, Vector3.up, Vector3.up
            };
            groundMesh.uv = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            // Draw ground plane with a subtle color
            gridMaterial.SetPass(0);
            gridMaterial.color = new Color(0.3f, 0.3f, 0.3f, 0.2f);
            _previewUtility.DrawMesh(groundMesh, Matrix4x4.identity, gridMaterial, 0);
            
            // Draw coordinate axes as simple lines using DrawLine
            // X axis (red)
            DrawPreviewLine(Vector3.zero, Vector3.right * gridSize/2, UnityGizmoUtility.xAxisColor, 2f);
            // Y axis (green)
            DrawPreviewLine(Vector3.zero, Vector3.up * gridSize/2, UnityGizmoUtility.yAxisColor, 2f);
            // Z axis (blue)
            DrawPreviewLine(Vector3.zero, Vector3.forward * gridSize/2, UnityGizmoUtility.zAxisColor, 2f);
            
            // Draw grid lines
            int gridLines = Mathf.FloorToInt(gridSize / cellSize);
            if (gridLines % 2 != 0) gridLines++;
            
            for (int i = -gridLines/2; i <= gridLines/2; i++)
            {
                float pos = i * cellSize;
                Color lineColor;
                float lineThickness;
                
                // Choose line color and thickness based on position
                if (i == 0)
                {
                    lineColor = mainColor;
                    lineThickness = 1.5f;
                }
                else if (i % 5 == 0)
                {
                    lineColor = secondaryColor;
                    lineThickness = 1f;
                }
                else
                {
                    lineColor = new Color(secondaryColor.r * 0.7f, secondaryColor.g * 0.7f, secondaryColor.b * 0.7f, secondaryColor.a * 0.5f);
                    lineThickness = 0.5f;
                }
                
                // X axis grid lines (along Z)
                DrawPreviewLine(
                    new Vector3(pos, 0.01f, -gridSize/2),
                    new Vector3(pos, 0.01f, gridSize/2),
                    lineColor,
                    lineThickness);
                
                // Z axis grid lines (along X)
                DrawPreviewLine(
                    new Vector3(-gridSize/2, 0.01f, pos),
                    new Vector3(gridSize/2, 0.01f, pos),
                    lineColor,
                    lineThickness);
            }
        }
        
        // Helper method to draw lines in the preview using a simple mesh approach
        private void DrawPreviewLine(Vector3 start, Vector3 end, Color color, float thickness = 1.0f)
        {
            if (_previewUtility == null) return;
            
            // Use a standard material that doesn't need GL calls
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.SetFloat("_Glossiness", 0f); // Make it matte
            lineMaterial.SetFloat("_Metallic", 0f);
            lineMaterial.color = color;
            
            // Create a simple line mesh
            Mesh lineMesh = new Mesh();
            
            // Calculate a perpendicular vector
            Vector3 lineDir = (end - start).normalized;
            Vector3 perpDir = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(lineDir, perpDir)) > 0.9f)
                perpDir = Vector3.forward;
                
            // Create a cross vector for thickness
            Vector3 crossVec = Vector3.Cross(lineDir, perpDir).normalized;
            Vector3 crossVec2 = Vector3.Cross(lineDir, crossVec).normalized;
            
            // Make the thickness reasonable 
            float scaledThickness = thickness * 0.02f;
            
            // Create vertices for a simple rectangular prism
            Vector3[] vertices = new Vector3[] {
                // Near cap
                start + crossVec * scaledThickness + crossVec2 * scaledThickness,
                start + crossVec * scaledThickness - crossVec2 * scaledThickness,
                start - crossVec * scaledThickness - crossVec2 * scaledThickness,
                start - crossVec * scaledThickness + crossVec2 * scaledThickness,
                
                // Far cap
                end + crossVec * scaledThickness + crossVec2 * scaledThickness,
                end + crossVec * scaledThickness - crossVec2 * scaledThickness,
                end - crossVec * scaledThickness - crossVec2 * scaledThickness,
                end - crossVec * scaledThickness + crossVec2 * scaledThickness
            };
            
            // Create triangles for a cube-like shape
            int[] triangles = new int[] {
                // Near cap
                0, 1, 3, 3, 1, 2,
                
                // Far cap
                4, 7, 5, 5, 7, 6,
                
                // Sides
                0, 4, 1, 1, 4, 5,
                1, 5, 2, 2, 5, 6,
                2, 6, 3, 3, 6, 7,
                3, 7, 0, 0, 7, 4
            };
            
            lineMesh.vertices = vertices;
            lineMesh.triangles = triangles;
            lineMesh.RecalculateNormals();
            
            // Draw the line mesh
            _previewUtility.DrawMesh(lineMesh, Matrix4x4.identity, lineMaterial, 0);
        }
        
        private void DrawOverlayInfo(Rect viewportRect)
        {
            // Skip if not in repaint event
            if (Event.current.type != EventType.Repaint)
                return;
                
            // Draw model info and help text overlay - only when necessary
            if (_targetObject == null || viewportRect.width < 200)
                return;
                
            // Use cached GUIStyle to avoid allocations
            if (_cachedHelpBoxStyle == null)
            {
                _cachedHelpBoxStyle = new GUIStyle(EditorStyles.helpBox);
                _cachedHelpBoxStyle.alignment = TextAnchor.MiddleCenter;
                _cachedHelpBoxStyle.normal.textColor = Color.white;
                _cachedHelpBoxStyle.fontSize = 10;
            }
            
            if (_cachedLabelStyle == null)
            {
                _cachedLabelStyle = new GUIStyle(EditorStyles.miniLabel);
                _cachedLabelStyle.alignment = TextAnchor.MiddleCenter;
                _cachedLabelStyle.normal.textColor = Color.white;
            }
            
            // Only draw detailed info when not dragging camera for better performance
            if (!_isDraggingCamera)
            {
                GUILayout.BeginArea(new Rect(viewportRect.x + 10, viewportRect.y + 10, viewportRect.width - 20, 60));
                
                if (_workingMesh != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Object: {_targetObject.name}", _cachedLabelStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Verts: {_workingMesh.vertexCount} | Tris: {_workingMesh.triangles.Length/3}", _cachedLabelStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.EndArea();
            }
            
            // Draw the help text at the bottom
            string helpText = "RMB: Orbit | MMB: Pan | Scroll: Zoom";
            
            // Calculate text size
            Vector2 helpTextSize = _cachedHelpBoxStyle.CalcSize(new GUIContent(helpText));
            
            // Draw at bottom center
            GUI.Label(
                new Rect(
                    viewportRect.center.x - helpTextSize.x/2, 
                    viewportRect.yMax - helpTextSize.y - 5,
                    helpTextSize.x,
                    helpTextSize.y
                ),
                helpText,
                _cachedHelpBoxStyle
            );
        }
        
        // Cached styles to reduce GC allocations
        private GUIStyle _cachedHelpBoxStyle;
        private GUIStyle _cachedLabelStyle;
        
        private void DrawUVControls(Rect viewportRect)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("UV Editor Controls", EditorStyles.boldLabel);
            
            // Validate that we have a valid mesh to work with
            bool validMeshForUV = _targetObject != null && _meshFilter != null && _workingMesh != null;
            
            if (!validMeshForUV)
            {
                EditorGUILayout.HelpBox("Select an object with a mesh to edit UVs.", MessageType.Info);
                
                // Close UV editor if open but no valid mesh is selected
                if (_uvEditor != null)
                {
                    try
                    {
                        _uvEditor.Close();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Error closing UV Editor: " + e.Message);
                    }
                    _uvEditor = null;
                }
                
                EditorGUILayout.EndVertical();
                return;
            }
            
            // Initialize UV editor if not already open
            if (_uvEditor == null)
            {
                if (GUILayout.Button("Open UV Editor", GUILayout.Height(30)))
                {
                    // Open the UV editor window
                    _uvEditor = EditorWindow.GetWindow<UVEditor>("UV Editor");
                    
                    // Set the target object using reflection to avoid direct method call
                    // which could cause issues if the method signature changes
                    System.Type uvEditorType = _uvEditor.GetType();
                    System.Reflection.MethodInfo setTargetMethod = uvEditorType.GetMethod("SetTargetObject", 
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    
                    if (setTargetMethod != null)
                    {
                        setTargetMethod.Invoke(_uvEditor, new object[] { _targetObject });
                    }
                }
            }
            else
            {
                // UV editor is already open
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Focus UV Editor", GUILayout.Height(30)))
                {
                    _uvEditor.Focus();
                }
                
                if (GUILayout.Button("Close UV Editor", GUILayout.Height(30)))
                {
                    try
                    {
                        _uvEditor.Close();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Error closing UV Editor: " + e.Message);
                    }
                    _uvEditor = null;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTextureControls(Rect viewportRect)
        {
            // Draw texture painting controls along the left side
            Rect controlsRect = new Rect(viewportRect.x + 10, viewportRect.y + 10, 150, viewportRect.height - 20);
            GUI.Box(controlsRect, "Texture Tools", EditorStyles.helpBox);
            
            GUILayout.BeginArea(new Rect(controlsRect.x + 5, controlsRect.y + 25, controlsRect.width - 10, controlsRect.height - 30));
            
            GUILayout.Label("Brush:", EditorStyles.boldLabel);
            
            Color brushColor = EditorGUILayout.ColorField("Color", Color.white);
            float brushSize = EditorGUILayout.Slider("Size", 0.1f, 0.01f, 0.5f);
            float brushHardness = EditorGUILayout.Slider("Hardness", 0.5f, 0f, 1f);
            
            GUILayout.Space(10);
            
            GUILayout.Label("Texture Channel:", EditorStyles.boldLabel);
            
            string[] channels = { "Albedo", "Normal", "Metallic", "Roughness", "Height" };
            int selectedChannel = GUILayout.SelectionGrid(-1, channels, 1);
            
            if (selectedChannel != -1)
            {
                // Set active channel
            }
            
            GUILayout.EndArea();
        }
        
        private void DrawMaterialControls(Rect viewportRect)
        {
            // Draw material controls along the left side
            Rect controlsRect = new Rect(viewportRect.x + 10, viewportRect.y + 10, 150, viewportRect.height - 20);
            GUI.Box(controlsRect, "Material Tools", EditorStyles.helpBox);
            
            GUILayout.BeginArea(new Rect(controlsRect.x + 5, controlsRect.y + 25, controlsRect.width - 10, controlsRect.height - 30));
            
            GUILayout.Label("Properties:", EditorStyles.boldLabel);
            
            Color baseColor = EditorGUILayout.ColorField("Base Color", Color.white);
            float metallic = EditorGUILayout.Slider("Metallic", 0f, 0f, 1f);
            float smoothness = EditorGUILayout.Slider("Smoothness", 0.5f, 0f, 1f);
            
            GUILayout.Space(10);
            
            GUILayout.Label("Textures:", EditorStyles.boldLabel);
            
            Texture2D albedoMap = EditorGUILayout.ObjectField("Albedo", null, typeof(Texture2D), false) as Texture2D;
            Texture2D normalMap = EditorGUILayout.ObjectField("Normal", null, typeof(Texture2D), false) as Texture2D;
            Texture2D metallicMap = EditorGUILayout.ObjectField("Metallic", null, typeof(Texture2D), false) as Texture2D;
            
            GUILayout.EndArea();
        }
        
        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (_targetObject != null)
            {
                EditorGUILayout.LabelField($"Object: {_targetObject.name}", EditorStyles.miniLabel);
                
                if (_meshFilter != null && _workingMesh != null)
                {
                    EditorGUILayout.LabelField($"Vertices: {_workingMesh.vertexCount} | Triangles: {_workingMesh.triangles.Length / 3}", EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("No object selected", EditorStyles.miniLabel);
            }
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.LabelField($"Tool: {_currentToolMode} | Camera: {_cameraDistance:F1}m", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void HandleInput(Rect viewportRect)
        {
            Event e = Event.current;
            
            // Only handle events inside the viewport
            if (!viewportRect.Contains(e.mousePosition))
                return;
                
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1) // Right mouse button (Blender-style orbit)
                    {
                        _isDraggingCamera = true;
                        _lastMousePosition = e.mousePosition;
                        e.Use();
                    }
                    else if (e.button == 0) // Left mouse button for selection and manipulation 
                    {
                        // Store mouse position at start of operation
                        _lastMousePosition = e.mousePosition;
                        
                        // Check for active Unity gizmo manipulation and don't handle if it's in progress
                        if (GUIUtility.hotControl != 0)
                        {
                            // A Unity handle control is active, let Unity handle it
                            return;
                        }
                        
                        HandleToolAction(e.mousePosition, viewportRect);
                        e.Use();
                    }
                    else if (e.button == 2) // Middle mouse button for pan (like Blender)
                    {
                        _isDraggingCamera = true;
                        _lastMousePosition = e.mousePosition;
                        e.Use();
                    }
                    break;
                    
                case EventType.MouseUp:
                    if (e.button == 1 || e.button == 2)
                    {
                        _isDraggingCamera = false;
                        e.Use();
                    }
                    else if (e.button == 0 && _isTransforming)
                    {
                        // End transformation
                        _isTransforming = false;
                        EditorApplication.update -= OnTransformUpdate;
                        
                        // Ensure Scene View updates
                        SceneView.RepaintAll();
                        e.Use();
                    }
                    break;
                    
                case EventType.MouseDrag:
                    if (_isDraggingCamera)
                    {
                        Vector2 delta = e.mousePosition - _lastMousePosition;
                        
                        if (e.button == 2) // Middle mouse for pan (Blender-style)
                        {
                            // Pan camera - improved for smooth panning at all distances
                            float panSpeed = Mathf.Clamp(_cameraDistance * 0.015f, 0.01f, 0.5f);
                            Vector3 rightPan = _previewUtility.camera.transform.right * delta.x * panSpeed;
                            Vector3 upPan = _previewUtility.camera.transform.up * delta.y * panSpeed;
                            _cameraPanOffset += rightPan + upPan;
                        }
                        else if (e.button == 1 && e.shift) // Shift+Right mouse for zoom (Blender-style)
                        {
                            // Improved zoom that feels more natural
                            float zoomAmount = delta.y * 0.03f;
                            
                            // Logarithmic zooming for more precision at different distances
                            if (_cameraDistance > 10f)
                                zoomAmount *= _cameraDistance * 0.1f;
                            else if (_cameraDistance < 1f)
                                zoomAmount *= 0.5f;
                                
                            _cameraDistance = Mathf.Clamp(_cameraDistance - zoomAmount, 0.1f, 100f);
                        }
                        else if (e.button == 1 && e.alt) // Alt+Right mouse for precise orbit (Blender-style)
                        {
                            // Slower, more precise orbit
                            _cameraRotation.y += delta.x * 0.3f;
                            _cameraRotation.x -= delta.y * 0.3f;
                            
                            // Allow full rotation around the object (no clamping on Y axis)
                            // Only clamp X to prevent flipping issues
                            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, -89f, 89f);
                        }
                        else if (e.button == 1) // Right mouse for orbit
                        {
                            // Smoother orbit with better sensitivity
                            float sensitivity = 0.8f;
                            _cameraRotation.y += delta.x * sensitivity;
                            _cameraRotation.x -= delta.y * sensitivity;
                            
                            // Allow full 360-degree rotation around Y axis (no clamping)
                            // Only clamp X to prevent flipping issues
                            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, -89f, 89f);
                        }
                        
                        _lastMousePosition = e.mousePosition;
                        SceneView.RepaintAll();
                        Repaint();
                        e.Use();
                    }
                    else if (_isTransforming && GUIUtility.hotControl == 0)
                    {
                        // Only handle the drag if no Unity control is active
                        // Pass updated mouse position to handle tool action
                        HandleToolAction(e.mousePosition, viewportRect);
                        e.Use();
                    }
                    break;
                    
                case EventType.ScrollWheel:
                    // Improved scroll wheel zoom with adaptive speed
                    float wheelZoomFactor = 0.1f;
                    
                    // Make zoom speed proportional to distance for better control
                    if (_cameraDistance < 2f)
                        wheelZoomFactor = 0.05f;
                    else if (_cameraDistance > 20f)
                        wheelZoomFactor = 0.2f;
                        
                    float zoomDelta = e.delta.y * wheelZoomFactor;
                    
                    // Use exponential zooming for more natural feel
                    if (e.delta.y > 0) // Zooming out
                        _cameraDistance *= (1f + zoomDelta);
                    else // Zooming in
                        _cameraDistance /= (1f - zoomDelta);
                        
                    // Enforce limits
                    _cameraDistance = Mathf.Clamp(_cameraDistance, 0.1f, 100f);
                    
                    SceneView.RepaintAll();
                    Repaint();
                    e.Use();
                    break;
                    
                case EventType.KeyDown:
                    // Handle tool switching with keyboard shortcuts that work in both the editor window and scene view
                    bool handled = false;
                    
                    // Blender-style hot keys
                    switch (e.keyCode)
                    {
                        case KeyCode.Home: // Home to frame all (like Blender's Home key)
                        case KeyCode.A when e.alt: // Alt+A to view all (like Blender)
                            FocusOnTarget();
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.R: // R key for rotate mode (Blender-style)
                            _currentModelingTool = ModelingTool.Rotate;
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.G: // G key for grab/move mode (Blender-style)
                            _currentModelingTool = ModelingTool.Move;
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.S: // S key for scale mode (Blender-style)
                            _currentModelingTool = ModelingTool.Scale;
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Q: // Q key for selection tool (additional shortcut)
                            _currentModelingTool = ModelingTool.Select;
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                        
                        case KeyCode.X: // X key for X-axis constraint (Blender-style)
                            if (_isTransforming)
                            {
                                _axisConstraint = AxisConstraint.X;
                                SceneView.RepaintAll();
                                Repaint();
                                handled = true;
                            }
                            break;
                            
                        case KeyCode.Y: // Y key for Y-axis constraint (Blender-style)
                            if (_isTransforming)
                            {
                                _axisConstraint = AxisConstraint.Y;
                                SceneView.RepaintAll();
                                Repaint();
                                handled = true;
                            }
                            break;
                            
                        case KeyCode.Z: // Z key for Z-axis constraint (Blender-style)
                            if (_isTransforming)
                            {
                                _axisConstraint = AxisConstraint.Z;
                                SceneView.RepaintAll();
                                Repaint();
                                handled = true;
                            }
                            break;
                            
                        case KeyCode.Keypad1: // Numpad 1 for front view (Blender-style)
                            _cameraRotation = new Vector2(0, 0);
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Keypad3: // Numpad 3 for side view (Blender-style)
                            _cameraRotation = new Vector2(0, 90);
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Keypad7: // Numpad 7 for top view (Blender-style)
                            _cameraRotation = new Vector2(90, 0);
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Keypad5: // Numpad 5 to toggle ortho/perspective (Blender-style)
                            // Toggle ortho/perspective would be implemented here
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        // Added numpad 2, 4, 6, 8 for rotation in 45-degree increments
                        case KeyCode.Keypad2: // Rotate down 45 degrees
                            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x - 45f, -89f, 89f);
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Keypad8: // Rotate up 45 degrees
                            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x + 45f, -89f, 89f);
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Keypad4: // Rotate left 45 degrees
                            _cameraRotation.y -= 45f;
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Keypad6: // Rotate right 45 degrees
                            _cameraRotation.y += 45f;
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Keypad0: // Reset camera position but keep distance
                            _cameraRotation = new Vector2(30, -60);
                            _cameraPanOffset = Vector3.zero;
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        // Mouse wheel emulation with keyboard
                        case KeyCode.Equals:
                        case KeyCode.Plus:
                        case KeyCode.KeypadPlus:
                            // Zoom in
                            _cameraDistance /= 1.1f;
                            _cameraDistance = Mathf.Clamp(_cameraDistance, 0.1f, 100f);
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Minus:
                        case KeyCode.KeypadMinus:
                            // Zoom out
                            _cameraDistance *= 1.1f;
                            _cameraDistance = Mathf.Clamp(_cameraDistance, 0.1f, 100f);
                            SceneView.RepaintAll();
                            Repaint();
                            handled = true;
                            break;
                            
                        case KeyCode.Escape: // Escape key to cancel transformation
                            if (_isTransforming)
                            {
                                _isTransforming = false;
                                _axisConstraint = AxisConstraint.None;
                                EditorApplication.update -= OnTransformUpdate;
                                SceneView.RepaintAll();
                                Repaint();
                                handled = true;
                            }
                            break;
                    }
                    
                    if (handled)
                        e.Use();
                    break;
            }
        }
        
        private void HandleToolAction(Vector2 mousePosition, Rect viewportRect)
        {
            if (_targetObject == null)
                return;
                
            // Get current event
            Event e = Event.current;
            
            // Check if the operation is just starting
            if (e.type == EventType.MouseDown)
            {
                _isTransforming = true;
                _axisConstraint = AxisConstraint.None; // Reset constraint on new operation
                
                // Determine which gizmo handle was clicked (if any)
                if (_currentModelingTool == ModelingTool.Move || 
                    _currentModelingTool == ModelingTool.Rotate || 
                    _currentModelingTool == ModelingTool.Scale)
                {
                    DetermineAxisConstraint(mousePosition, viewportRect);
                }
                
                // Ensure we keep repainting during transforms
                EditorApplication.update -= OnTransformUpdate;
                EditorApplication.update += OnTransformUpdate;
                
                // Save the initial object position
                if (_targetObject != null)
                {
                    _lastKnownPosition = _targetObject.transform.position;
                }
                
                // Store initial mouse position
                _lastMousePosition = mousePosition;
                return;
            }
            
            // Handle ending the operation
            if (e.type == EventType.MouseUp)
            {
                _isTransforming = false;
                EditorApplication.update -= OnTransformUpdate;
                return;
            }
            
            switch (_currentToolMode)
            {
                case ToolMode.Model:
                    // Only apply transforms if we're actually transforming (mouse is down)
                    if (_isTransforming && 
                        (_currentModelingTool == ModelingTool.Move || 
                         _currentModelingTool == ModelingTool.Rotate || 
                         _currentModelingTool == ModelingTool.Scale))
                    {
                        // Calculate delta from last mouse position
                        Vector2 mouseDelta = mousePosition - _lastMousePosition;
                        _lastMousePosition = mousePosition;
                        
                        // Calculate transform delta based on camera parameters and object size
                        float objectSize = GetObjectSize();
                        float sensitivityScale = Mathf.Clamp(objectSize, 0.1f, 10f);
                        float distanceScale = _cameraDistance / 5f;
                        
                        // Scale delta based on viewport size for consistent behavior
                        float viewportScale = 800f / Mathf.Min(viewportRect.width, viewportRect.height);
                        
                        // Apply all scaling factors to get final delta
                        Vector3 delta = new Vector3(
                            mouseDelta.x * 0.01f * distanceScale * viewportScale,
                            mouseDelta.y * 0.01f * distanceScale * viewportScale,
                            0
                        );
                        
                        // Apply transformation based on current tool and constraint
                        if (_currentModelingTool == ModelingTool.Move)
                        {
                            // Scale move based on object size
                            ApplyBlenderStyleMove(delta * sensitivityScale);
                        }
                        else if (_currentModelingTool == ModelingTool.Rotate)
                        {
                            // Rotation needs special handling
                            Vector3 objectCenter = _targetObject.transform.position;
                            Vector2 screenCenter = WorldToViewport(objectCenter, viewportRect);
                            
                            // For rotation, sensitivity should be inverse to distance
                            float rotationSensitivity = 1.0f + (1.0f / Mathf.Max(1.0f, distanceScale));
                            ApplyBlenderStyleRotation(delta * rotationSensitivity, screenCenter, mousePosition);
                        }
                        else if (_currentModelingTool == ModelingTool.Scale)
                        {
                            // Get distance to center
                            Vector3 objectCenter = _targetObject.transform.position;
                            Vector2 screenCenter = WorldToViewport(objectCenter, viewportRect);
                            float distToCenter = Vector2.Distance(mousePosition, screenCenter);
                            float gizmoSize = CalculateDynamicGizmoSize(viewportRect);
                            
                            // Check if we're close to the center for uniform scaling
                            bool uniformScale = distToCenter < (gizmoSize * 0.2f);
                            
                            // Make scaling more consistent
                            float scaleSensitivity = 1.0f + (sensitivityScale * 0.1f);
                            ApplyBlenderStyleScale(delta * scaleSensitivity, uniformScale);
                        }
                        
                        // Force a repaint after transform
                        Repaint();
                    }
                    break;
                    
                case ToolMode.UV:
                    // Handle UV editing
                    break;
                    
                case ToolMode.Texture:
                    // Handle texture painting
                    break;
                    
                case ToolMode.Material:
                    // Handle material editing
                    break;
            }
        }
        
        private void OnTransformUpdate()
        {
            // Keep repainting during transforms to ensure smooth gizmo updates
            if (_isTransforming)
            {
                Repaint();
            }
        }
        
        // New method to determine which axis was clicked
        private void DetermineAxisConstraint(Vector2 mousePosition, Rect viewportRect)
        {
            // Get the object center in screen space
            Vector3 objectCenter = _targetObject.transform.position;
            Vector2 screenCenter = WorldToViewport(objectCenter, viewportRect);
            
            // Get dynamic gizmo size
            float gizmoSize = CalculateDynamicGizmoSize(viewportRect);
            
            // Calculate handle distances based on gizmo size
            float handleDistance = gizmoSize * 0.5f;
            
            Vector2 xHandle, yHandle, zHandle;
            
            // Position handles appropriately based on the current tool
            switch (_currentModelingTool)
            {
                case ModelingTool.Move:
                    xHandle = screenCenter + new Vector2(handleDistance, 0);  // X axis
                    yHandle = screenCenter + new Vector2(0, -handleDistance); // Y axis
                    zHandle = screenCenter + new Vector2(-handleDistance * 0.7f, -handleDistance * 0.7f); // Z axis diagonal
                    break;
                    
                case ModelingTool.Rotate:
                    // For rotation, use circular positions
                    float rotateRadius = handleDistance * 0.8f;
                    xHandle = screenCenter + new Vector2(rotateRadius, 0);  // X axis circle right
                    yHandle = screenCenter + new Vector2(0, -rotateRadius); // Y axis circle top
                    zHandle = screenCenter + new Vector2(-rotateRadius * 0.7f, -rotateRadius * 0.7f); // Z axis circle diagonal
                    break;
                    
                case ModelingTool.Scale:
                    // For scale, use the handle boxes at the ends of axes
                    xHandle = screenCenter + new Vector2(handleDistance, 0);  // X axis
                    yHandle = screenCenter + new Vector2(0, -handleDistance); // Y axis
                    zHandle = screenCenter + new Vector2(-handleDistance * 0.7f, -handleDistance * 0.7f); // Z axis diagonal
                    break;
                    
                default:
                    return; // Not a transform tool
            }
            
            // Calculate distance to each handle
            float distX = Vector2.Distance(mousePosition, xHandle);
            float distY = Vector2.Distance(mousePosition, yHandle);
            float distZ = Vector2.Distance(mousePosition, zHandle);
            float distCenter = Vector2.Distance(mousePosition, screenCenter);
            
            // Dynamic handle size based on gizmo size and tool type - MUCH LARGER click areas
            float handleSize;
            
            // For move and scale, use box-shaped handles
            if (_currentModelingTool == ModelingTool.Move || _currentModelingTool == ModelingTool.Scale)
            {
                handleSize = gizmoSize * 0.25f; // More generous click area
            }
            // For rotate, use larger circular handles
            else
            {
                handleSize = gizmoSize * 0.3f; // More generous click area
            }
            
            // Determine constraint based on distance
            if (distCenter < handleSize * 0.6f) // Larger center area
            {
                // Clicked center - for uniform scale or free transform
                _axisConstraint = AxisConstraint.None;
            }
            else if (distX < handleSize)
            {
                _axisConstraint = AxisConstraint.X;
            }
            else if (distY < handleSize)
            {
                _axisConstraint = AxisConstraint.Y;
            }
            else if (distZ < handleSize)
            {
                _axisConstraint = AxisConstraint.Z;
            }
            else
            {
                // Not close to any handle, but still clicked - use free transform
                _axisConstraint = AxisConstraint.None;
            }
            
            // Update UI to show the selected constraint
            Repaint();
        }
        
        private void ApplyBlenderStyleMove(Vector3 delta)
        {
            if (_targetObject == null)
                return;
                
            // Get camera orientation to determine movement direction
            Quaternion camRotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
            Vector3 rightAxis = camRotation * Vector3.right;
            Vector3 upAxis = camRotation * Vector3.up;
            Vector3 forwardAxis = camRotation * Vector3.forward;
            
            // Store original position for comparison
            Vector3 originalPosition = _targetObject.transform.position;
            Vector3 movement = Vector3.zero;
            
            // Apply Blender-style move constraints
            switch (_axisConstraint)
            {
                case AxisConstraint.X:
                    // X axis constraint in world space
                    movement = Vector3.right * delta.x * 10f;
                    break;
                    
                case AxisConstraint.Y:
                    // Y axis constraint in world space
                    movement = Vector3.up * -delta.y * 10f;
                    break;
                    
                case AxisConstraint.Z:
                    // Z axis constraint in world space
                    movement = Vector3.forward * (delta.x - delta.y) * 5f;
                    break;
                    
                case AxisConstraint.None:
                    // Free move relative to view - use camera-aligned movement
                    // This feels more natural in 3D space
                    Vector3 horizontalMovement = rightAxis * delta.x * 10f;
                    Vector3 verticalMovement = upAxis * -delta.y * 10f;
                    movement = horizontalMovement + verticalMovement;
                    break;
            }
            
            // Apply movement
            _targetObject.transform.position += movement;
            
            // Update tracked position
            _lastKnownPosition = _targetObject.transform.position;
            
            // Check if the object actually moved (could be constrained by Unity or something else)
            if (Vector3.Distance(originalPosition, _targetObject.transform.position) < 0.0001f && movement.magnitude > 0.0001f)
            {
                // Object didn't move - issue warning only once per operation
                Debug.LogWarning("Object movement constraint detected. The object may be locked or have constraints in Unity.");
            }
        }
        
        private void ApplyBlenderStyleRotation(Vector3 delta, Vector2 screenCenter, Vector2 mousePosition)
        {
            if (_targetObject == null)
                return;
                
            // Store original rotation for comparison
            Quaternion originalRotation = _targetObject.transform.rotation;
            
            // Get rotation axes based on camera orientation
            Quaternion camRotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
            Vector3 viewX = camRotation * Vector3.right;
            Vector3 viewY = camRotation * Vector3.up;
            Vector3 viewZ = camRotation * Vector3.forward;
            
            // Blender-style rotation around constraints
            switch (_axisConstraint)
            {
                case AxisConstraint.X:
                    // For X axis, vertical mouse motion rotates around world X
                    float xAngle = delta.y * 100f;
                    _targetObject.transform.Rotate(Vector3.right, xAngle, Space.World);
                    break;
                    
                case AxisConstraint.Y:
                    // For Y axis, horizontal mouse motion rotates around world Y
                    float yAngle = delta.x * 100f;
                    _targetObject.transform.Rotate(Vector3.up, yAngle, Space.World);
                    break;
                    
                case AxisConstraint.Z:
                    // For Z axis, circular motion around center rotates around world Z
                    Vector2 fromCenter = mousePosition - screenCenter;
                    Vector2 delta2D = new Vector2(delta.x, delta.y);
                    
                    // Only calculate angle if we have meaningful input
                    if (fromCenter.magnitude > 0.01f && delta2D.magnitude > 0.0001f)
                    {
                        float angle = Vector2.SignedAngle(fromCenter, fromCenter - delta2D);
                        _targetObject.transform.Rotate(Vector3.forward, angle, Space.World);
                    }
                    break;
                    
                case AxisConstraint.None:
                    // Trackball-style rotation - more intuitive for 3D
                    // Horizontal movement rotates around view up axis
                    float horizontalAngle = delta.x * 100f;
                    _targetObject.transform.Rotate(viewY, horizontalAngle, Space.World);
                    
                    // Vertical movement rotates around view right axis
                    float verticalAngle = delta.y * 100f;
                    _targetObject.transform.Rotate(viewX, verticalAngle, Space.World);
                    break;
            }
            
            // Update tracked position in case rotation affects world position
            _lastKnownPosition = _targetObject.transform.position;
            
            // Check if rotation actually happened
            if (Quaternion.Angle(originalRotation, _targetObject.transform.rotation) < 0.01f &&
                (Mathf.Abs(delta.x) > 0.0001f || Mathf.Abs(delta.y) > 0.0001f))
            {
                // No rotation occurred despite input
                Debug.LogWarning("Object rotation constraint detected. The object may be locked or have constraints in Unity.");
            }
        }
        
        private void ApplyBlenderStyleScale(Vector3 delta, bool isUniformScale)
        {
            if (_targetObject == null)
                return;
                
            // Store original scale for comparison
            Vector3 originalScale = _targetObject.transform.localScale;
            
            // Get current scale
            Vector3 scale = _targetObject.transform.localScale;
            
            // Calculate a more intuitive scale factor based on pointer movement
            // Use combined X and Y movement for a more natural feeling
            float scaleFactor = 1.0f + (delta.x - delta.y) * 2.0f;
            
            // Ensure the scale factor is reasonable and doesn't jump too much per frame
            scaleFactor = Mathf.Clamp(scaleFactor, 0.9f, 1.1f);
            
            // Apply based on constraint
            switch (_axisConstraint)
            {
                case AxisConstraint.X:
                    scale.x *= scaleFactor;
                    scale.x = Mathf.Max(0.001f, scale.x);
                    break;
                    
                case AxisConstraint.Y:
                    scale.y *= scaleFactor;
                    scale.y = Mathf.Max(0.001f, scale.y);
                    break;
                    
                case AxisConstraint.Z:
                    scale.z *= scaleFactor;
                    scale.z = Mathf.Max(0.001f, scale.z);
                    break;
                    
                case AxisConstraint.None:
                    if (isUniformScale)
                    {
                        // Uniform scale (all axes)
                        scale *= scaleFactor;
                        scale = new Vector3(
                            Mathf.Max(0.001f, scale.x),
                            Mathf.Max(0.001f, scale.y),
                            Mathf.Max(0.001f, scale.z)
                        );
                    }
                    else
                    {
                        // Default to scaling in the view plane (XY)
                        scale.x *= scaleFactor;
                        scale.y *= scaleFactor;
                        scale.x = Mathf.Max(0.001f, scale.x);
                        scale.y = Mathf.Max(0.001f, scale.y);
                    }
                    break;
            }
            
            // Apply the new scale
            _targetObject.transform.localScale = scale;
            
            // Update tracked position in case scale affects position
            _lastKnownPosition = _targetObject.transform.position;
            
            // Check if scaling actually happened
            if (Vector3.Distance(originalScale, _targetObject.transform.localScale) < 0.0001f &&
                Mathf.Abs(scaleFactor - 1.0f) > 0.001f)
            {
                // No scaling occurred despite input
                Debug.LogWarning("Object scaling constraint detected. The object may be locked or have constraints in Unity.");
            }
        }
        
        private void OnSceneViewGUI(SceneView sceneView)
        {
            if (_targetObject == null)
                return;
                
            // Draw custom scene handles and gizmos based on current tool
            switch (_currentToolMode)
            {
                case ToolMode.Model:
                    DrawModelingHandles();
                    break;
                    
                case ToolMode.UV:
                    DrawUVHandles();
                    break;
                    
                case ToolMode.Texture:
                    DrawTexturingHandles();
                    break;
                    
                case ToolMode.Material:
                    // Material doesn't need scene handles
                    break;
            }
        }
        
        private void DrawModelingHandles()
        {
            if (_targetObject == null || _workingMesh == null)
                return;
                
            // Get current event
            Event e = Event.current;
            bool isInSceneView = (e.type == EventType.Layout || e.type == EventType.Repaint);

            // Only proceed with handle drawing if we're in Scene View rendering
            if (!isInSceneView)
                return;
                
            // First, record object state for undo if we're potentially changing it
            if (_targetObject != null)
            {
                if (_currentModelingTool == ModelingTool.Move || 
                    _currentModelingTool == ModelingTool.Rotate || 
                    _currentModelingTool == ModelingTool.Scale)
                {
                    // Record object transform state for undo
                    Undo.RecordObject(_targetObject.transform, _currentModelingTool.ToString());
                }
            }
            
            // Store original position and set color for consistent gizmo appearance
            Vector3 originalPosition = _targetObject.transform.position;
            Vector3 originalRotation = _targetObject.transform.rotation.eulerAngles;
            Vector3 originalScale = _targetObject.transform.localScale;
            
            // Get handle size based on distance and object size for better scaling
            float handleSize = HandleUtility.GetHandleSize(originalPosition);
            
            // Determine active axis colors based on constraint
            Color xAxisColor = (_axisConstraint == AxisConstraint.X) ? 
                UnityGizmoUtility.highlightColor : UnityGizmoUtility.xAxisColor;
                
            Color yAxisColor = (_axisConstraint == AxisConstraint.Y) ? 
                UnityGizmoUtility.highlightColor : UnityGizmoUtility.yAxisColor;
                
            Color zAxisColor = (_axisConstraint == AxisConstraint.Z) ? 
                UnityGizmoUtility.highlightColor : UnityGizmoUtility.zAxisColor;
            
            // Switch between handle types based on current tool
            switch (_currentModelingTool)
            {
                case ModelingTool.Move:
                    // Use custom position handle with proper colors
                    Vector3 newPosition = UnityGizmoUtility.PositionHandle(
                        originalPosition, 
                        _targetObject.transform.rotation, 
                        handleSize, 
                        xAxisColor, 
                        yAxisColor, 
                        zAxisColor);
                    
                    // Apply position change if moved, respecting axis constraints
                    if (newPosition != originalPosition)
                    {
                        // Calculate delta
                        Vector3 delta = newPosition - originalPosition;
                        
                        // Apply axis constraint if active
                        if (_axisConstraint != AxisConstraint.None)
                        {
                            switch (_axisConstraint)
                            {
                                case AxisConstraint.X:
                                    delta.y = delta.z = 0;
                                    break;
                                case AxisConstraint.Y:
                                    delta.x = delta.z = 0;
                                    break;
                                case AxisConstraint.Z:
                                    delta.x = delta.y = 0;
                                    break;
                            }
                        }
                        
                        // Apply movement
                        _targetObject.transform.position = originalPosition + delta;
                        
                        // Update tracked position
                        _lastKnownPosition = _targetObject.transform.position;
                        
                        // Force repaint
                        SceneView.RepaintAll();
                    }
                    break;
                    
                case ModelingTool.Rotate:
                    // Use custom rotation handle with proper colors
                    Quaternion newRotation = UnityGizmoUtility.RotationHandle(
                        _targetObject.transform.rotation,
                        originalPosition,
                        handleSize,
                        xAxisColor,
                        yAxisColor,
                        zAxisColor);
                    
                    // Apply rotation if changed, respecting axis constraints
                    if (newRotation != _targetObject.transform.rotation)
                    {
                        // Apply rotation, respecting axis constraints
                        if (_axisConstraint != AxisConstraint.None)
                        {
                            // Extract rotation differences around each axis
                            Vector3 originalEuler = _targetObject.transform.rotation.eulerAngles;
                            Vector3 newEuler = newRotation.eulerAngles;
                            
                            // Calculate delta for each axis (handling 0-360 wrapping)
                            Vector3 deltaEuler = new Vector3(
                                Mathf.DeltaAngle(originalEuler.x, newEuler.x),
                                Mathf.DeltaAngle(originalEuler.y, newEuler.y),
                                Mathf.DeltaAngle(originalEuler.z, newEuler.z)
                            );
                            
                            // Apply only the constrained axis rotation
                            switch (_axisConstraint)
                            {
                                case AxisConstraint.X:
                                    _targetObject.transform.Rotate(Vector3.right, deltaEuler.x, Space.World);
                                    break;
                                case AxisConstraint.Y:
                                    _targetObject.transform.Rotate(Vector3.up, deltaEuler.y, Space.World);
                                    break;
                                case AxisConstraint.Z:
                                    _targetObject.transform.Rotate(Vector3.forward, deltaEuler.z, Space.World);
                                    break;
                            }
                        }
                        else
                        {
                            // Apply full rotation
                            _targetObject.transform.rotation = newRotation;
                        }
                        
                        // Force repaint
                        SceneView.RepaintAll();
                    }
                    break;
                    
                case ModelingTool.Scale:
                    // Use custom scale handle with proper colors
                    Vector3 newScale = UnityGizmoUtility.ScaleHandle(
                        originalScale,
                        originalPosition,
                        _targetObject.transform.rotation,
                        handleSize,
                        xAxisColor,
                        yAxisColor,
                        zAxisColor);
                    
                    // Apply scale if changed, respecting axis constraints
                    if (newScale != originalScale)
                    {
                        // Calculate delta
                        Vector3 scaleDelta = newScale - originalScale;
                        
                        // Apply axis constraint if active
                        if (_axisConstraint != AxisConstraint.None)
                        {
                            switch (_axisConstraint)
                            {
                                case AxisConstraint.X:
                                    scaleDelta.y = scaleDelta.z = 0;
                                    break;
                                case AxisConstraint.Y:
                                    scaleDelta.x = scaleDelta.z = 0;
                                    break;
                                case AxisConstraint.Z:
                                    scaleDelta.x = scaleDelta.y = 0;
                                    break;
                            }
                        }
                        
                        // Apply scale
                        _targetObject.transform.localScale = originalScale + scaleDelta;
                        
                        // Force repaint
                        SceneView.RepaintAll();
                    }
                    break;
                
                case ModelingTool.Select:
                default:
                    // For selection mode, draw a nice selection bounds box
                    DrawSelectionBounds();
                    break;
            }
        }
        
        private void DrawSelectionBounds()
        {
            if (_targetObject == null)
                return;
                
            // Get the renderer bounds to draw a selection box
            Bounds bounds = new Bounds();
            bool hasBounds = false;
            
            // Try to get bounds from renderers
            Renderer[] renderers = _targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                hasBounds = true;
            }
            // If no renderers, try mesh filter
            else if (_meshFilter != null && _meshFilter.sharedMesh != null)
            {
                bounds = _meshFilter.sharedMesh.bounds;
                bounds.center = _targetObject.transform.position;
                bounds.size = Vector3.Scale(bounds.size, _targetObject.transform.lossyScale);
                hasBounds = true;
            }
            
            if (hasBounds)
            {
                // Use the utility to draw a nice selection box
                UnityGizmoUtility.DrawSelectionBox(bounds, UnityGizmoUtility.selectionColor);
            }
        }
        
        private void DrawUVHandles()
        {
            // Draw UV handles in scene view if needed
        }
        
        private void DrawTexturingHandles()
        {
            // Draw texturing brush preview in scene view
        }
        
        private void SetTargetObject(GameObject obj)
        {
            // Clean up previous references to prevent MissingReferenceException
            if (_uvEditor != null)
            {
                try
                {
                    _uvEditor.Close();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Error closing UV Editor: " + e.Message);
                }
                _uvEditor = null;
            }

            // If we're currently in UV mode, switch back to model mode
            if (_currentToolMode == ToolMode.UV)
            {
                _currentToolMode = ToolMode.Model;
            }

            // Store the new target and check if it has a valid mesh
            _targetObject = obj;
            MeshFilter meshFilter = null;
            
            if (_targetObject != null)
            {
                meshFilter = _targetObject.GetComponent<MeshFilter>();
            }

            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                _meshFilter = meshFilter;
                
                // Create a working copy of the mesh
                _workingMesh = Instantiate(_meshFilter.sharedMesh);
                _workingMesh.name = _meshFilter.sharedMesh.name + " (Working Copy)";
                
                // Track the object's position
                _lastKnownPosition = _targetObject.transform.position;
                
                // Reset the camera to focus on the object
                FocusOnTarget();
            }
            else
            {
                _meshFilter = null;
                _workingMesh = null;
            }
            
            // Clean up the texture painter if it exists
            if (_texturePainter != null)
            {
                _texturePainter.Cleanup();
                _texturePainter = null;
            }
            
            // Mark renderer cache as expired since we changed objects
            _rendererCacheExpired = true;
            
            // No need to repaint immediately - it will happen on next OnGUI call
        }
        
        private void FocusOnTarget()
        {
            if (_targetObject == null)
                return;
                
            // Get bounds from all mesh renderers in the object hierarchy for more accurate focusing
            Renderer[] renderers = _targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                // Start with the first renderer's bounds
                Bounds bounds = renderers[0].bounds;
                
                // Expand to include all other renderers
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                
                // Get the center and size for focusing
                Vector3 center = bounds.center;
                float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                
                // Reset camera orientation to look at the center
                _cameraRotation = new Vector2(30, -45);
                _cameraPanOffset = Vector3.zero;
                
                // Adjust distance to fit the object size
                if (maxDimension > 0.01f)
                {
                    _cameraDistance = maxDimension * 2.0f;
                }
                else
                {
                    _cameraDistance = 5f; // Default fallback
                }
                
                // Ensure distance is within reasonable bounds
                _cameraDistance = Mathf.Clamp(_cameraDistance, 0.5f, 50f);
                
                Debug.Log($"Focus camera at distance: {_cameraDistance}, object bounds: {bounds.size}");
            }
            else if (_meshFilter != null && _meshFilter.sharedMesh != null)
            {
                // Fallback to using just the mesh bounds
                Bounds bounds = _meshFilter.sharedMesh.bounds;
                float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                
                _cameraRotation = new Vector2(30, -45);
                _cameraPanOffset = Vector3.zero;
                
                if (maxDimension < 0.01f)
                    maxDimension = 1f;
                
                _cameraDistance = maxDimension * 2.5f;
                _cameraDistance = Mathf.Clamp(_cameraDistance, 0.5f, 50f);
            }
            else
            {
                // Default view if no reliable bounds can be found
                _cameraRotation = new Vector2(30, -45);
                _cameraPanOffset = Vector3.zero;
                _cameraDistance = 5f;
            }
            
            // Trigger a repaint to update the view
            Repaint();
        }
        
        private void ImportModel()
        {
            string path = EditorUtility.OpenFilePanel("Import 3D Model", "", "obj,fbx");
            
            if (string.IsNullOrEmpty(path))
                return;
                
            try
            {
                // Check if we're importing an OBJ file (which we can handle directly)
                if (Path.GetExtension(path).ToLower() == ".obj")
                {
                    Debug.Log("Importing OBJ from: " + path);
                    
                    // Use MeshIOHandler to import the mesh
                    Mesh importedMesh = MeshIOHandler.ImportMesh(path);
                    
                    if (importedMesh != null)
                    {
                        // Create game object with the imported mesh
                        GameObject newObject = new GameObject(Path.GetFileNameWithoutExtension(path));
                        
                        // Add components
                        MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
                        meshFilter.sharedMesh = importedMesh;
                        
                        MeshRenderer renderer = newObject.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = new Material(Shader.Find("Standard"));
                        
                        // Position at origin
                        newObject.transform.position = Vector3.zero;
                        
                        // Set as the new target
                        SetTargetObject(newObject);
                        
                        // Add to the scene
                        if (newObject.scene.name == null)
                        {
                            SceneManager.MoveGameObjectToScene(newObject, SceneManager.GetActiveScene());
                        }
                        
                        Debug.Log("Successfully imported: " + path);
                    }
                    else
                    {
                        Debug.LogError("Failed to import mesh from: " + path);
                    }
                }
                else
                {
                    // For FBX and other formats, copy to the Assets folder and use Unity's importer
                    string fileName = Path.GetFileName(path);
                    string destPath = Path.Combine("Assets", "ImportedModels");
                    
                    // Create directory if it doesn't exist
                    if (!Directory.Exists(destPath))
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    
                    string assetPath = Path.Combine(destPath, fileName);
                    
                    // Copy the file
                    File.Copy(path, assetPath, true);
                    
                    // Refresh the asset database
                    AssetDatabase.Refresh();
                    
                    // Get the imported object
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    
                    if (prefab != null)
                    {
                        // Instantiate the prefab
                        GameObject newObject = Instantiate(prefab);
                        newObject.name = prefab.name;
                        
                        // Position at origin
                        newObject.transform.position = Vector3.zero;
                        
                        // Set as the new target
                        SetTargetObject(newObject);
                        
                        Debug.Log("Successfully imported: " + path);
                    }
                    else
                    {
                        Debug.LogError("Failed to import asset from: " + path);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error importing model: {e.Message}");
                EditorUtility.DisplayDialog("Import Error", $"Failed to import model: {e.Message}", "OK");
            }
        }
        
        private void ExportModel()
        {
            if (_targetObject == null || _meshFilter == null || _workingMesh == null)
            {
                EditorUtility.DisplayDialog("Export Error", "No valid mesh selected to export.", "OK");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel("Export 3D Model", "", _targetObject.name, "obj");
            
            if (!string.IsNullOrEmpty(path))
            {
                Debug.Log("You would export to: " + path);
                // Implementation would go here using MeshIOHandler
            }
        }
        
        private void DrawTransformGizmos(Rect viewportRect)
        {
            // Skip if not in repaint event
            if (Event.current.type != EventType.Repaint)
                return;
                
            if (_targetObject == null)
                return;

            // Convert world position to screen position
            Vector2 screenPos = WorldToViewport(_targetObject.transform.position, viewportRect);
            
            // Calculate gizmo size dynamically based on viewport size and camera distance
            float gizmoSize = CalculateDynamicGizmoSize(viewportRect);
            
            // Skip drawing if outside viewport (with margin)
            float margin = gizmoSize * 2.0f;
            if (screenPos.x < -margin || screenPos.x > viewportRect.width + margin ||
                screenPos.y < -margin || screenPos.y > viewportRect.height + margin)
            {
                return;
            }

            // Use lightweight GL drawing approach
            if (_lineMaterial == null)
            {
                return;
            }
            
            // Determine colors based on current axis constraint
            Color xColor = (_axisConstraint == AxisConstraint.X) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.xAxisColor;
            Color yColor = (_axisConstraint == AxisConstraint.Y) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.yAxisColor;
            Color zColor = (_axisConstraint == AxisConstraint.Z) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.zAxisColor;
            
            // Draw the appropriate gizmo based on the tool
            switch (_currentModelingTool)
            {
                case ModelingTool.Move:
                    DrawLightweightMoveGizmo(screenPos, gizmoSize, xColor, yColor, zColor);
                    break;
                case ModelingTool.Rotate:
                    DrawLightweightRotateGizmo(screenPos, gizmoSize, xColor, yColor, zColor);
                    break;
                case ModelingTool.Scale:
                    DrawLightweightScaleGizmo(screenPos, gizmoSize, xColor, yColor, zColor);
                    break;
                default:
                    // For other tools, just draw a simple origin marker
                    DrawLightweightOriginGizmo(screenPos, gizmoSize * 0.5f);
                    break;
            }
        }
        
        // Optimized drawing methods that use less CPU and memory
        private void DrawLightweightMoveGizmo(Vector2 center, float size, Color xColor, Color yColor, Color zColor)
        {
            // Draw X axis (red)
            DrawLine(center, new Vector2(center.x + size, center.y), xColor, 2f);
            
            // Draw Y axis (green)
            DrawLine(center, new Vector2(center.x, center.y - size), yColor, 2f);
            
            // Draw Z axis (blue) - projected to 2D
            DrawLine(center, new Vector2(center.x - size * 0.7f, center.y + size * 0.7f), zColor, 2f);
            
            // Draw axis handles
            DrawRect(new Rect(center.x + size - 5, center.y - 5, 10, 10), xColor);
            DrawRect(new Rect(center.x - 5, center.y - size - 5, 10, 10), yColor);
            DrawRect(new Rect(center.x - size * 0.7f - 5, center.y + size * 0.7f - 5, 10, 10), zColor);
        }
        
        private void DrawLightweightRotateGizmo(Vector2 center, float size, Color xColor, Color yColor, Color zColor)
        {
            float radius = size * 0.7f;
            
            // Draw axis circles - fewer segments for better performance
            DrawCircle(center, radius, xColor, 16);
            DrawCircle(center, radius * 0.8f, yColor, 16);
            DrawCircle(center, radius * 0.6f, zColor, 16);
        }
        
        private void DrawLightweightScaleGizmo(Vector2 center, float size, Color xColor, Color yColor, Color zColor)
        {
            // Draw X axis (red)
            DrawLine(center, new Vector2(center.x + size, center.y), xColor, 2f);
            
            // Draw Y axis (green)
            DrawLine(center, new Vector2(center.x, center.y - size), yColor, 2f);
            
            // Draw Z axis (blue) - projected to 2D
            DrawLine(center, new Vector2(center.x - size * 0.7f, center.y + size * 0.7f), zColor, 2f);
            
            // Draw axis handles
            float handleSize = 10f;
            DrawRect(new Rect(center.x + size - handleSize/2, center.y - handleSize/2, handleSize, handleSize), xColor);
            DrawRect(new Rect(center.x - handleSize/2, center.y - size - handleSize/2, handleSize, handleSize), yColor);
            DrawRect(new Rect(center.x - size * 0.7f - handleSize/2, center.y + size * 0.7f - handleSize/2, handleSize, handleSize), zColor);
            
            // Draw center handle for uniform scaling
            Color centerColor = (_axisConstraint == AxisConstraint.None) ? Color.yellow : Color.white;
            DrawRect(new Rect(center.x - handleSize/2, center.y - handleSize/2, handleSize, handleSize), centerColor);
        }
        
        private void DrawLightweightOriginGizmo(Vector2 center, float size)
        {
            // Draw X axis (red)
            DrawLine(center, new Vector2(center.x + size, center.y), UnityGizmoUtility.xAxisColor, 1f);
            
            // Draw Y axis (green)
            DrawLine(center, new Vector2(center.x, center.y - size), UnityGizmoUtility.yAxisColor, 1f);
            
            // Draw Z axis (blue) - projected to 2D
            DrawLine(center, new Vector2(center.x - size * 0.7f, center.y + size * 0.7f), UnityGizmoUtility.zAxisColor, 1f);
        }
        
        // Lightweight drawing helpers
        private void DrawLine(Vector2 start, Vector2 end, Color color, float width = 1f)
        {
            if (_lineMaterial == null) return;
            
            _lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
            GL.End();
            GL.PopMatrix();
        }
        
        private void DrawRect(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
        }
        
        private void DrawCircle(Vector2 center, float radius, Color color, int segments = 32)
        {
            if (_lineMaterial == null) return;
            
            _lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(color);
            
            float angleStep = 2f * Mathf.PI / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;
                
                Vector2 point1 = new Vector2(
                    center.x + radius * Mathf.Cos(angle1),
                    center.y + radius * Mathf.Sin(angle1)
                );
                
                Vector2 point2 = new Vector2(
                    center.x + radius * Mathf.Cos(angle2),
                    center.y + radius * Mathf.Sin(angle2)
                );
                
                GL.Vertex3(point1.x, point1.y, 0);
                GL.Vertex3(point2.x, point2.y, 0);
            }
            
            GL.End();
            GL.PopMatrix();
        }
        
        private float CalculateDynamicGizmoSize(Rect viewportRect)
        {
            // Calculate base size as percentage of viewport
            float baseSize = Mathf.Min(viewportRect.width, viewportRect.height) * 0.08f;
            
            // Scale based on camera distance (closer = bigger)
            float distanceFactor = Mathf.Max(0.5f, 5.0f / _cameraDistance);
            
            // Adjust based on object size
            float objectSize = GetObjectSize();
            float objectFactor = Mathf.Clamp(objectSize / 2.0f, 0.5f, 2.0f);
            
            return baseSize * distanceFactor * objectFactor;
        }
        
        private float GetObjectSize()
        {
            if (_targetObject == null || _meshFilter == null || _meshFilter.sharedMesh == null)
                return 1.0f;
                
            // Get object bounds and return the maximum dimension
            Bounds bounds = _meshFilter.sharedMesh.bounds;
            return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * Mathf.Max(
                _targetObject.transform.localScale.x,
                _targetObject.transform.localScale.y,
                _targetObject.transform.localScale.z);
        }

        private void DrawUnityStyleMoveGizmo(Vector2 center, float size)
        {
            // Use GL for more efficient line drawing
            if (_lineMaterial == null)
            {
                Debug.LogWarning("Line material is missing. Reinitializing...");
                // Reinitialize if somehow missing
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader);
                _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _lineMaterial.SetInt("_ZWrite", 0);
            }
            
            // Draw X axis (red)
            Color xColor = (_axisConstraint == AxisConstraint.X) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.xAxisColor;
            DrawGizmoArrow(center, new Vector2(center.x + size, center.y), xColor, size * 0.2f);
            
            // Draw Y axis (green)
            Color yColor = (_axisConstraint == AxisConstraint.Y) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.yAxisColor;
            DrawGizmoArrow(center, new Vector2(center.x, center.y - size), yColor, size * 0.2f);
            
            // Draw Z axis (blue)
            Color zColor = (_axisConstraint == AxisConstraint.Z) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.zAxisColor;
            DrawGizmoArrow(center, new Vector2(center.x - size * 0.7f, center.y + size * 0.7f), zColor, size * 0.2f);
        }
        
        private void DrawGizmoArrow(Vector2 start, Vector2 end, Color color, float headSize)
        {
            // Calculate direction and angle
            Vector2 dir = (end - start).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            // Draw the shaft
            GL.PushMatrix();
            _lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
            GL.End();
            GL.PopMatrix();
            
            // Draw the arrowhead
            Vector2 arrow1 = end - RotateVector(dir, 150) * headSize;
            Vector2 arrow2 = end - RotateVector(dir, -150) * headSize;
            
            GL.PushMatrix();
            _lineMaterial.SetPass(0);
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);
            GL.Vertex3(end.x, end.y, 0);
            GL.Vertex3(arrow1.x, arrow1.y, 0);
            GL.Vertex3(arrow2.x, arrow2.y, 0);
            GL.End();
            GL.PopMatrix();
        }
        
        private Vector2 RotateVector(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }

        private void DrawUnityStyleRotateGizmo(Vector2 center, float size)
        {
            float radius = size * 0.7f;
            float thickness = size * 0.1f;

            // X-axis circle (red)
            Color xColor = (_axisConstraint == AxisConstraint.X) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.xAxisColor;
            DrawCircleArc(center, radius, xColor, 0, thickness);
            
            // Y-axis circle (green)
            Color yColor = (_axisConstraint == AxisConstraint.Y) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.yAxisColor;
            DrawCircleArc(center, radius * 0.8f, yColor, 45, thickness);
            
            // Z-axis circle (blue)
            Color zColor = (_axisConstraint == AxisConstraint.Z) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.zAxisColor;
            DrawCircleArc(center, radius * 0.6f, zColor, 90, thickness);
        }
        
        private void DrawCircleArc(Vector2 center, float radius, Color color, float startAngleOffset, float thickness)
        {
            const int segments = 32;
            
            GL.PushMatrix();
            _lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(color);
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = startAngleOffset + i * (360f / segments) * Mathf.Deg2Rad;
                float angle2 = startAngleOffset + (i + 1) * (360f / segments) * Mathf.Deg2Rad;
                
                float x1 = center.x + radius * Mathf.Cos(angle1);
                float y1 = center.y + radius * Mathf.Sin(angle1);
                float x2 = center.x + radius * Mathf.Cos(angle2);
                float y2 = center.y + radius * Mathf.Sin(angle2);
                
                GL.Vertex3(x1, y1, 0);
                GL.Vertex3(x2, y2, 0);
            }
            
            GL.End();
            GL.PopMatrix();
            
            // Draw indicator handle
            float handleAngle = startAngleOffset + 45 * Mathf.Deg2Rad;
            float handleX = center.x + radius * Mathf.Cos(handleAngle);
            float handleY = center.y + radius * Mathf.Sin(handleAngle);
            
            DrawSolidCube(new Vector2(handleX, handleY), thickness);
        }

        private void DrawUnityStyleScaleGizmo(Vector2 center, float size)
        {
            float cubeSize = size * 0.15f;
            
            // Draw X axis (red)
            Color xColor = (_axisConstraint == AxisConstraint.X) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.xAxisColor;
            Vector2 xEnd = new Vector2(center.x + size, center.y);
            DrawGizmoLine(center, xEnd, xColor);
            DrawSolidCube(xEnd, cubeSize, xColor);
            
            // Draw Y axis (green)
            Color yColor = (_axisConstraint == AxisConstraint.Y) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.yAxisColor;
            Vector2 yEnd = new Vector2(center.x, center.y - size);
            DrawGizmoLine(center, yEnd, yColor);
            DrawSolidCube(yEnd, cubeSize, yColor);
            
            // Draw Z axis (blue)
            Color zColor = (_axisConstraint == AxisConstraint.Z) ? UnityGizmoUtility.highlightColor : UnityGizmoUtility.zAxisColor;
            Vector2 zEnd = new Vector2(center.x - size * 0.7f, center.y + size * 0.7f);
            DrawGizmoLine(center, zEnd, zColor);
            DrawSolidCube(zEnd, cubeSize, zColor);
            
            // Draw center cube for uniform scaling
            Color uniformColor = (_axisConstraint == AxisConstraint.None) ? UnityGizmoUtility.highlightColor : Color.white;
            DrawSolidCube(center, cubeSize * 1.5f, uniformColor);
        }
        
        private void DrawGizmoLine(Vector2 start, Vector2 end, Color color)
        {
            GL.PushMatrix();
            _lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
            GL.End();
            GL.PopMatrix();
        }

        private void DrawUnityStyleOriginGizmo(Vector2 center, float size)
        {
            float lineSize = size * 0.7f;
            
            // Draw X axis (red)
            DrawGizmoLine(center, new Vector2(center.x + lineSize, center.y), UnityGizmoUtility.xAxisColor);
            
            // Draw Y axis (green)
            DrawGizmoLine(center, new Vector2(center.x, center.y - lineSize), UnityGizmoUtility.yAxisColor);
            
            // Draw Z axis (blue)
            DrawGizmoLine(center, new Vector2(center.x - lineSize * 0.7f, center.y + lineSize * 0.7f), UnityGizmoUtility.zAxisColor);
            
            // Draw center point
            DrawSolidCube(center, size * 0.2f, Color.white);
        }

        private void DrawSolidCube(Vector2 center, float size)
        {
            DrawSolidCube(center, size, Color.white);
        }
        
        private void DrawSolidCube(Vector2 center, float size, Color color)
        {
            Rect rect = new Rect(center.x - size/2, center.y - size/2, size, size);
            EditorGUI.DrawRect(rect, color);
        }
        
        // Convert world space position to viewport space - improved for accuracy
        private Vector2 WorldToViewport(Vector3 worldPosition, Rect viewportRect)
        {
            if (_previewUtility == null || _previewUtility.camera == null)
                return Vector2.zero;
                
            // Get camera matrices - refresh these each time to ensure accuracy
            Matrix4x4 projMatrix = _previewUtility.camera.projectionMatrix;
            Matrix4x4 viewMatrix = _previewUtility.camera.worldToCameraMatrix;
            Matrix4x4 vpMatrix = projMatrix * viewMatrix;
            
            // Transform world to clip space
            Vector4 clipPos = vpMatrix * new Vector4(worldPosition.x, worldPosition.y, worldPosition.z, 1.0f);
            
            // Perspective division
            if (Mathf.Abs(clipPos.w) > 0.0001f)
            {
                clipPos.x /= clipPos.w;
                clipPos.y /= clipPos.w;
            }
            
            // Convert to viewport coordinates
            Vector2 screenPos = new Vector2(
                viewportRect.x + viewportRect.width * (clipPos.x + 1.0f) * 0.5f,
                viewportRect.y + viewportRect.height * (1.0f - (clipPos.y + 1.0f) * 0.5f)
            );
            
            return screenPos;
        }
    }
} 