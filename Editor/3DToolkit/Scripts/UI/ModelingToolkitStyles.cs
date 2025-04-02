using UnityEngine;
using UnityEditor;

namespace ModelingToolkit
{
    public static class ModelingToolkitStyles
    {
        // GUI Styles
        public static GUIStyle ToolbarButtonStyle;
        public static GUIStyle TabButtonStyle;
        public static GUIStyle HeaderStyle;
        public static GUIStyle PanelStyle;
        
        // Textures
        private static Texture2D _toolbarButtonTex;
        private static Texture2D _toolbarButtonActiveTex;
        private static Texture2D _tabButtonTex;
        private static Texture2D _tabButtonActiveTex;
        private static Texture2D _panelBackgroundTex;
        private static Texture2D _headerBackgroundTex;
        
        // Colors
        private static readonly Color HeaderColor = new Color(0.25f, 0.25f, 0.25f);
        private static readonly Color PanelColor = new Color(0.22f, 0.22f, 0.22f);
        private static readonly Color ButtonColor = new Color(0.3f, 0.3f, 0.3f);
        private static readonly Color ButtonActiveColor = new Color(0.4f, 0.4f, 0.4f);
        
        public static void Initialize()
        {
            // Create textures
            _toolbarButtonTex = MakeTexture(2, 2, ButtonColor);
            _toolbarButtonActiveTex = MakeTexture(2, 2, ButtonActiveColor);
            _tabButtonTex = MakeTexture(2, 2, ButtonColor);
            _tabButtonActiveTex = MakeTexture(2, 2, ButtonActiveColor);
            _panelBackgroundTex = MakeTexture(2, 2, PanelColor);
            _headerBackgroundTex = MakeTexture(2, 2, HeaderColor);
            
            // Create styles
            ToolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                normal = { background = _toolbarButtonTex },
                active = { background = _toolbarButtonActiveTex },
                fixedHeight = 24,
                margin = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(6, 6, 3, 3)
            };
            
            TabButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                normal = { background = _tabButtonTex },
                active = { background = _tabButtonActiveTex },
                fixedHeight = 28,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(10, 10, 5, 5)
            };
            
            HeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { background = _headerBackgroundTex, textColor = Color.white },
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(0, 0, 5, 5),
                fixedHeight = 24,
                alignment = TextAnchor.MiddleLeft
            };
            
            PanelStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { background = _panelBackgroundTex },
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 0, 10)
            };
        }
        
        public static bool ToolbarToggleButton(string label, bool isActive)
        {
            GUIStyle buttonStyle = new GUIStyle(ToolbarButtonStyle);
            if (isActive)
            {
                buttonStyle.normal.background = _toolbarButtonActiveTex;
            }
            
            return GUILayout.Button(label, buttonStyle, GUILayout.ExpandWidth(false));
        }
        
        public static bool TabToggleButton(string label, bool isActive)
        {
            GUIStyle buttonStyle = new GUIStyle(TabButtonStyle);
            if (isActive)
            {
                buttonStyle.normal.background = _tabButtonActiveTex;
                buttonStyle.normal.textColor = Color.white;
            }
            
            return GUILayout.Button(label, buttonStyle, GUILayout.ExpandWidth(true));
        }
        
        public static void DrawHeader(string text)
        {
            EditorGUILayout.LabelField(text, HeaderStyle);
        }
        
        public static void BeginPanel()
        {
            GUILayout.BeginVertical(PanelStyle);
        }
        
        public static void EndPanel()
        {
            GUILayout.EndVertical();
        }
        
        // Draw a foldout header with custom styling
        public static bool DrawFoldout(string text, bool isExpanded)
        {
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.fontSize = 12;
            
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), foldoutStyle, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, HeaderColor);
            
            bool newState = EditorGUI.Foldout(rect, isExpanded, text, true, foldoutStyle);
            EditorGUILayout.Space(2);
            
            return newState;
        }
        
        // Helper method to create a colored texture
        private static Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }
        
        public static void Cleanup()
        {
            // Destroy textures
            if (_toolbarButtonTex != null) Object.DestroyImmediate(_toolbarButtonTex);
            if (_toolbarButtonActiveTex != null) Object.DestroyImmediate(_toolbarButtonActiveTex);
            if (_tabButtonTex != null) Object.DestroyImmediate(_tabButtonTex);
            if (_tabButtonActiveTex != null) Object.DestroyImmediate(_tabButtonActiveTex);
            if (_panelBackgroundTex != null) Object.DestroyImmediate(_panelBackgroundTex);
            if (_headerBackgroundTex != null) Object.DestroyImmediate(_headerBackgroundTex);
        }
    }
} 