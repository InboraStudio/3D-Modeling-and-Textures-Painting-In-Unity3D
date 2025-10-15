using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ModelingToolkit
{
    public class UVEditor : EditorWindow
    {
        // Target object
        private GameObject _targetObject;
        private MeshFilter _meshFilter;
        private Mesh _originalMesh;
        private Mesh _workingMesh;
        
        // UV data
        private Vector2[] _uvs;
        private int[] _triangles;
        private Vector3[] _vertices;
        
        // Selection
        private List<int> _selectedUVs = new List<int>();
        private bool _selectMultiple = false;
        private int _lastSelectedIndex = -1;
        
        // UI state
        private Vector2 _scrollPosition;
        private Vector2 _zoomCenter = Vector2.one * 0.5f;
        private float _zoomLevel = 1f;
        private Vector2 _panOffset = Vector2.zero;
        private bool _isDragging = false;
        private Vector2 _lastMousePosition;
        
        // Caching for performance
        private Texture2D _gridTexture;
        private Texture2D _uvLineTexture;
        private Material _lineMaterial;
        private Mesh _cachedLineMesh;
        private Dictionary<int, Rect> _uvPointRects = new Dictionary<int, Rect>();
        private bool _needsUVRecalculation = true;
        
        // UV view settings
        private Color _gridColor = new Color(0.2f, 0.2f, 0.2f);
        private Color _uvLineColor = new Color(0.8f, 0.8f, 0.8f);
        private Color _selectionColor = new Color(0f, 0.8f, 1f, 0.8f);
        private float _uvPointSize = 5f;
        
        // References valid flag
        private bool _referencesValid = false;
        
        [MenuItem("Window/3D Toolkit/UV Editor")]
        public static void ShowWindow()
        {
            UVEditor window = GetWindow<UVEditor>("UV Editor");
            window.minSize = new Vector2(400, 400);
        }
        
        private void OnEnable()
        {
            // Initialize materials and textures
            _lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            
            // Try to use the currently selected object
            if (Selection.activeGameObject != null)
            {
                SetTargetObject(Selection.activeGameObject);
            }
            
            // Register to selection change events
            Selection.selectionChanged += OnSelectionChanged;
            
            // Force repaint frequently for responsive UI
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
        }
        
        private void OnDisable()
        {
            // Unregister from events
            Selection.selectionChanged -= OnSelectionChanged;
            
            // Clean up cached resources
            DestroyImmediate(_lineMaterial);
            DestroyImmediate(_gridTexture);
            DestroyImmediate(_uvLineTexture);
            DestroyImmediate(_cachedLineMesh);
            
            // Ensure we save any changes
            SaveChangesIfNeeded();
            
            // Clear references
            _targetObject = null;
            _meshFilter = null;
            _originalMesh = null;
            _workingMesh = null;
            _uvs = null;
            _triangles = null;
            _vertices = null;
            _selectedUVs.Clear();
            _uvPointRects.Clear();
        }
        
        private void SaveChangesIfNeeded()
        {
            // Check if we need to save changes
            if (_referencesValid && _workingMesh != null && _originalMesh != null && _meshFilter != null && _workingMesh != _originalMesh)
            {
                try
                {
                    ApplyUVChanges();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Error saving UV changes: " + e.Message);
                }
            }
        }
        
        private void OnSelectionChanged()
        {
            // Update when selection changes
            if (Selection.activeGameObject != null)
            {
                SetTargetObject(Selection.activeGameObject);
            }
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Object selection
            EditorGUI.BeginChangeCheck();
            GameObject newTarget = EditorGUILayout.ObjectField("Target Object", _targetObject, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck() && newTarget != _targetObject)
            {
                SaveChangesIfNeeded();
                SetTargetObject(newTarget);
            }
            
            EditorGUILayout.Space();
            
            // Draw toolbar
            DrawToolbar();
            
            // Draw UV view
            Rect uvViewRect = EditorGUILayout.GetControlRect(false, position.height - 120);
            DrawUVView(uvViewRect);
            
            EditorGUILayout.Space();
            
            // Draw status bar
            DrawStatusBar();
            
            EditorGUILayout.EndVertical();
            
            // Handle input events
            HandleInputEvents(uvViewRect);
            
            // Force repaint if dragging for smooth movement
            if (_isDragging || _needsUVRecalculation)
            {
                Repaint();
            }
        }
        
        private void SetTargetObject(GameObject obj)
        {
            if (obj == null)
            {
                _referencesValid = false;
                _targetObject = null;
                _meshFilter = null;
                _originalMesh = null;
                _workingMesh = null;
                _uvs = null;
                _triangles = null;
                _vertices = null;
                _selectedUVs.Clear();
                _uvPointRects.Clear();
                _needsUVRecalculation = true;
                Repaint();
                return;
            }
            
            // Clear current data
            _selectedUVs.Clear();
            _uvPointRects.Clear();
            _needsUVRecalculation = true;
            
            _targetObject = obj;
            _meshFilter = _targetObject.GetComponent<MeshFilter>();
            
            if (_meshFilter != null && _meshFilter.sharedMesh != null)
            {
                _originalMesh = _meshFilter.sharedMesh;
                
                // Create a working copy
                _workingMesh = Object.Instantiate(_originalMesh);
                _workingMesh.name = _originalMesh.name + " (Working Copy)";
                
                // Get data from mesh
                _vertices = _workingMesh.vertices;
                _triangles = _workingMesh.triangles;
                _uvs = _workingMesh.uv;
                
                // If the mesh doesn't have UVs, create default ones
                if (_uvs == null || _uvs.Length == 0)
                {
                    _uvs = new Vector2[_vertices.Length];
                    for (int i = 0; i < _uvs.Length; i++)
                    {
                        _uvs[i] = new Vector2(0, 0);
                    }
                    _workingMesh.uv = _uvs;
                }
                
                _referencesValid = true;
            }
            else
            {
                _referencesValid = false;
                _originalMesh = null;
                _workingMesh = null;
                _vertices = null;
                _triangles = null;
                _uvs = null;
            }
            
            Repaint();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Reset View", EditorStyles.toolbarButton))
            {
                ResetView();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Select All", EditorStyles.toolbarButton))
            {
                SelectAllUVs();
            }
            
            if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton))
            {
                _selectedUVs.Clear();
                Repaint();
            }
            
            GUILayout.Space(5);
            
            _selectMultiple = GUILayout.Toggle(_selectMultiple, "Multiple Selection", EditorStyles.toolbarButton);
            
            GUILayout.FlexibleSpace();
            
            if (_referencesValid)
            {
                if (GUILayout.Button("Apply", EditorStyles.toolbarButton))
                {
                    ApplyUVChanges();
                }
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("Unwrap", EditorStyles.toolbarButton))
                {
                    UnwrapUVs();
                }
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("Reset UVs", EditorStyles.toolbarButton))
                {
                    ResetUVs();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawUVView(Rect rect)
        {
            // Only redraw when necessary
            if (Event.current.type != EventType.Repaint)
                return;

            // Draw background
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
            
            if (!_referencesValid || _workingMesh == null || _uvs == null)
            {
                GUI.Label(rect, "No mesh selected or mesh has no UVs", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            // Begin clip area for UV display
            GUI.BeginClip(rect);
            
            // Draw grid
            DrawGrid(rect);
            
            // Cache UV point positions if needed
            if (_needsUVRecalculation)
            {
                RecalculateUVPositions(rect);
                _needsUVRecalculation = false;
            }
            
            // Draw UV edges
            DrawUVEdges(rect);
            
            // Draw selected UVs
            DrawSelectedUVs(rect);
            
            // End clipping
            GUI.EndClip();
            
            // Draw border
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y));
            Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.y + rect.height));
            Handles.DrawLine(new Vector3(rect.x + rect.width, rect.y), new Vector3(rect.x + rect.width, rect.y + rect.height));
            Handles.DrawLine(new Vector3(rect.x, rect.y + rect.height), new Vector3(rect.x + rect.width, rect.y + rect.height));
        }
        
        private void RecalculateUVPositions(Rect rect)
        {
            if (_uvs == null) return;
            
            // Clear the dictionary
            _uvPointRects.Clear();
            
            // Pre-calculate all UV point positions - allocate capacity for better performance
            int capacity = Mathf.Min(_uvs.Length, 65536); // Reasonable upper limit
            _uvPointRects = new Dictionary<int, Rect>(capacity);
            
            for (int i = 0; i < _uvs.Length; i++)
            {
                Vector2 uv = WorldToViewport(_uvs[i], rect);
                float halfSize = _uvPointSize / 2f;
                _uvPointRects[i] = new Rect(uv.x - halfSize, uv.y - halfSize, _uvPointSize, _uvPointSize);
            }
        }
        
        private void DrawGrid(Rect rect)
        {
            // Draw UV space border (0-1)
            Vector2 uvMin = WorldToViewport(new Vector2(0, 0), rect);
            Vector2 uvMax = WorldToViewport(new Vector2(1, 1), rect);
            
            Handles.color = _gridColor * 1.5f;
            Handles.DrawLine(new Vector3(uvMin.x, uvMin.y), new Vector3(uvMax.x, uvMin.y));
            Handles.DrawLine(new Vector3(uvMin.x, uvMin.y), new Vector3(uvMin.x, uvMax.y));
            Handles.DrawLine(new Vector3(uvMax.x, uvMin.y), new Vector3(uvMax.x, uvMax.y));
            Handles.DrawLine(new Vector3(uvMin.x, uvMax.y), new Vector3(uvMax.x, uvMax.y));
            
            // Draw grid lines
            Handles.color = _gridColor;
            float gridStep = 0.1f;
            
            // Only draw grid if zoom level permits
            if (_zoomLevel > 0.5f)
            {
                for (float x = gridStep; x < 1f; x += gridStep)
                {
                    Vector2 start = WorldToViewport(new Vector2(x, 0), rect);
                    Vector2 end = WorldToViewport(new Vector2(x, 1), rect);
                    Handles.DrawLine(new Vector3(start.x, start.y), new Vector3(end.x, end.y));
                }
                
                for (float y = gridStep; y < 1f; y += gridStep)
                {
                    Vector2 start = WorldToViewport(new Vector2(0, y), rect);
                    Vector2 end = WorldToViewport(new Vector2(1, y), rect);
                    Handles.DrawLine(new Vector3(start.x, start.y), new Vector3(end.x, end.y));
                }
            }
        }
        
        private void DrawUVEdges(Rect rect)
        {
            if (_triangles == null || _uvs == null)
                return;
                
            Handles.color = _uvLineColor;
            
            // BatchDrawing for better performance
            GL.PushMatrix();
            _lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(_uvLineColor);
            
            // Draw triangles
            for (int i = 0; i < _triangles.Length; i += 3)
            {
                int v1 = _triangles[i];
                int v2 = _triangles[i + 1];
                int v3 = _triangles[i + 2];
                
                if (v1 >= _uvs.Length || v2 >= _uvs.Length || v3 >= _uvs.Length)
                    continue;
                    
                if (_uvPointRects.TryGetValue(v1, out Rect uv1Rect) &&
                    _uvPointRects.TryGetValue(v2, out Rect uv2Rect) &&
                    _uvPointRects.TryGetValue(v3, out Rect uv3Rect))
                {
                    Vector2 uv1 = new Vector2(uv1Rect.center.x, uv1Rect.center.y);
                    Vector2 uv2 = new Vector2(uv2Rect.center.x, uv2Rect.center.y);
                    Vector2 uv3 = new Vector2(uv3Rect.center.x, uv3Rect.center.y);
                    
                    // Draw lines using GL for better performance
                    GL.Vertex3(uv1.x, uv1.y, 0);
                    GL.Vertex3(uv2.x, uv2.y, 0);
                    
                    GL.Vertex3(uv2.x, uv2.y, 0);
                    GL.Vertex3(uv3.x, uv3.y, 0);
                    
                    GL.Vertex3(uv3.x, uv3.y, 0);
                    GL.Vertex3(uv1.x, uv1.y, 0);
                }
            }
            
            GL.End();
            GL.PopMatrix();
            
            // Draw all UV points efficiently using GUI.Box
            Color pointColor = _uvLineColor;
            GUIStyle pointStyle = new GUIStyle();
            pointStyle.normal.background = EditorGUIUtility.whiteTexture;
            
            foreach (var kvp in _uvPointRects)
            {
                GUI.color = pointColor;
                GUI.Box(kvp.Value, GUIContent.none, pointStyle);
            }
            
            // Reset GUI color
            GUI.color = Color.white;
        }
        
        private void DrawSelectedUVs(Rect rect)
        {
            if (_selectedUVs.Count == 0)
                return;
                
            // Draw selected points
            GUIStyle selectionStyle = new GUIStyle();
            selectionStyle.normal.background = EditorGUIUtility.whiteTexture;
            GUI.color = _selectionColor;
            
            foreach (int i in _selectedUVs)
            {
                if (_uvPointRects.TryGetValue(i, out Rect pointRect))
                {
                    GUI.Box(pointRect, GUIContent.none, selectionStyle);
                }
            }
            
            // Reset GUI color
            GUI.color = Color.white;
        }
        
        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (_referencesValid)
            {
                int vertexCount = _vertices != null ? _vertices.Length : 0;
                EditorGUILayout.LabelField($"Vertices: {vertexCount} | Selected UVs: {_selectedUVs.Count}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("No mesh selected", EditorStyles.miniLabel);
            }
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.LabelField($"Zoom: {_zoomLevel:F2}x", EditorStyles.miniLabel, GUILayout.Width(70));
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void HandleInputEvents(Rect viewRect)
        {
            Event e = Event.current;
            
            // Only process events inside the UV view rect
            if (!viewRect.Contains(e.mousePosition))
                return;
                
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        HandleSelection(viewRect, e.mousePosition);
                        e.Use();
                    }
                    else if (e.button == 2 || e.button == 1)
                    {
                        _isDragging = true;
                        _lastMousePosition = e.mousePosition;
                        e.Use();
                    }
                    break;
                    
                case EventType.MouseUp:
                    if (_isDragging)
                    {
                        _isDragging = false;
                        e.Use();
                    }
                    break;
                    
                case EventType.MouseDrag:
                    if (_isDragging)
                    {
                        Vector2 delta = e.mousePosition - _lastMousePosition;
                        _panOffset += delta / (_zoomLevel * viewRect.width);
                        _lastMousePosition = e.mousePosition;
                        _needsUVRecalculation = true;
                        Repaint();
                        e.Use();
                    }
                    else if (e.button == 0 && _selectedUVs.Count > 0)
                    {
                        MoveSelectedUVs(viewRect, e.mousePosition - _lastMousePosition);
                        _lastMousePosition = e.mousePosition;
                        _needsUVRecalculation = true;
                        Repaint();
                        e.Use();
                    }
                    break;
                    
                case EventType.ScrollWheel:
                    float zoomDelta = -e.delta.y * 0.05f;
                    float previousZoom = _zoomLevel;
                    _zoomLevel = Mathf.Clamp(_zoomLevel + zoomDelta, 0.1f, 10f);
                    
                    if (previousZoom != _zoomLevel)
                    {
                        _needsUVRecalculation = true;
                        Repaint();
                    }
                    
                    e.Use();
                    break;
                    
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Delete && _selectedUVs.Count > 0)
                    {
                        // Reset selected UVs to (0,0)
                        foreach (int i in _selectedUVs)
                        {
                            if (i < _uvs.Length)
                            {
                                _uvs[i] = Vector2.zero;
                            }
                        }
                        _workingMesh.uv = _uvs;
                        _needsUVRecalculation = true;
                        Repaint();
                        e.Use();
                    }
                    break;
            }
        }
        
        private void HandleSelection(Rect viewRect, Vector2 mousePosition)
        {
            if (_uvs == null || !_referencesValid)
                return;
                
            // Find the closest UV point to the mouse position
            float closestDistance = float.MaxValue;
            int closestIndex = -1;
            
            foreach (var kvp in _uvPointRects)
            {
                if (kvp.Value.Contains(mousePosition))
                {
                    float distance = Vector2.Distance(kvp.Value.center, mousePosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestIndex = kvp.Key;
                    }
                }
            }
            
            // If we found a close UV point, select it
            if (closestIndex >= 0)
            {
                if (!_selectMultiple)
                {
                    _selectedUVs.Clear();
                }
                
                if (_selectedUVs.Contains(closestIndex))
                {
                    _selectedUVs.Remove(closestIndex);
                }
                else
                {
                    _selectedUVs.Add(closestIndex);
                    _lastSelectedIndex = closestIndex;
                }
                
                _lastMousePosition = mousePosition;
                Repaint();
            }
        }
        
        private void MoveSelectedUVs(Rect viewRect, Vector2 delta)
        {
            if (_selectedUVs.Count == 0 || _uvs == null || !_referencesValid)
                return;
                
            // Convert delta from screen to UV space
            Vector2 uvDelta = delta / (viewRect.width * _zoomLevel);
            
            // Move all selected UVs
            bool uvsMoved = false;
            foreach (int i in _selectedUVs)
            {
                if (i < _uvs.Length)
                {
                    _uvs[i] += uvDelta;
                    uvsMoved = true;
                }
            }
            
            // Only apply changes if UVs were actually moved
            if (uvsMoved)
            {
                _workingMesh.uv = _uvs;
            }
        }
        
        private Vector2 WorldToViewport(Vector2 uvPosition, Rect viewport)
        {
            // Convert from UV space (0-1) to viewport space with zoom and pan
            float x = ((uvPosition.x + _panOffset.x) * _zoomLevel * viewport.width) + viewport.x;
            float y = viewport.height - ((uvPosition.y + _panOffset.y) * _zoomLevel * viewport.height) + viewport.y;
            return new Vector2(x, y);
        }
        
        private Vector2 ViewportToWorld(Vector2 viewportPosition, Rect viewport)
        {
            // Convert from viewport space to UV space (0-1) with zoom and pan
            float x = (viewportPosition.x - viewport.x) / (viewport.width * _zoomLevel) - _panOffset.x;
            float y = ((viewport.height - viewportPosition.y) + viewport.y) / (viewport.height * _zoomLevel) - _panOffset.y;
            return new Vector2(x, y);
        }
        
        private void ResetView()
        {
            _zoomLevel = 1f;
            _panOffset = Vector2.zero;
            _needsUVRecalculation = true;
            Repaint();
        }
        
        private void SelectAllUVs()
        {
            if (_uvs == null || !_referencesValid)
                return;
                
            _selectedUVs.Clear();
            for (int i = 0; i < _uvs.Length; i++)
            {
                _selectedUVs.Add(i);
            }
            Repaint();
        }
        
        private void ApplyUVChanges()
        {
            // Check for null references to prevent MissingReferenceException
            if (!_referencesValid || _originalMesh == null || _workingMesh == null || _meshFilter == null)
            {
                Debug.LogWarning("Cannot apply UV changes - missing references");
                return;
            }
            
            try
            {
                // Create a new mesh to avoid modifying original assets if they are built-in
                Mesh newMesh = new Mesh();
                newMesh.vertices = _originalMesh.vertices;
                newMesh.triangles = _originalMesh.triangles;
                newMesh.normals = _originalMesh.normals;
                newMesh.colors = _originalMesh.colors;
                newMesh.tangents = _originalMesh.tangents;
                newMesh.uv = _workingMesh.uv;  // Use our modified UVs
                
                // Apply the new mesh to the target
                _meshFilter.mesh = newMesh;
                
                // If this is part of an asset, prompt to save
                if (EditorUtility.IsPersistent(_originalMesh))
                {
                    if (EditorUtility.DisplayDialog("Save UV Changes", 
                        "Do you want to save these UV changes to the original mesh asset?", 
                        "Yes", "No"))
                    {
                        string path = EditorUtility.SaveFilePanelInProject(
                            "Save Mesh Asset",
                            _originalMesh.name + "_UVEdited.asset",
                            "asset",
                            "Choose a location to save the modified mesh");
                            
                        if (!string.IsNullOrEmpty(path))
                        {
                            AssetDatabase.CreateAsset(newMesh, path);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error applying UV changes: " + e.Message);
            }
        }
        
        private void UnwrapUVs()
        {
            if (!_referencesValid || _workingMesh == null)
                return;
                
            // Reset UVs first
            Unwrapping.GenerateSecondaryUVSet(_workingMesh);
            
            // Get the updated UVs
            _uvs = _workingMesh.uv;
            _needsUVRecalculation = true;
            
            Repaint();
        }
        
        private void ResetUVs()
        {
            if (!_referencesValid || _workingMesh == null || _vertices == null)
                return;
                
            // Create default planar UVs
            _uvs = new Vector2[_vertices.Length];
            for (int i = 0; i < _vertices.Length; i++)
            {
                // Simple planar projection on XY plane
                _uvs[i] = new Vector2(_vertices[i].x, _vertices[i].y);
            
                // Normalize to 0-1 range
                _uvs[i] += Vector2.one * 0.5f;
            }
            
            _workingMesh.uv = _uvs;
            _needsUVRecalculation = true;
            Repaint();
        }
    }
} 