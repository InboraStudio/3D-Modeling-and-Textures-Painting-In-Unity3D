using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ModelingToolkit
{
    public class TexturePainter
    {
        // Texture painting settings
        private Color _brushColor = Color.white;
        private float _brushSize = 0.05f;
        private float _brushHardness = 0.5f;
        private TextureChannel _activeChannel = TextureChannel.Albedo;
        
        // Render textures
        private RenderTexture _paintTexture;
        private RenderTexture _tempTexture;
        
        // Material for painting
        private Material _paintMaterial;
        
        // Active textures
        private Texture2D _albedoMap;
        private Texture2D _normalMap;
        private Texture2D _metallicMap;
        private Texture2D _roughnessMap;
        private Texture2D _heightMap;
        
        // Texture channels
        public enum TextureChannel
        {
            Albedo,
            Normal,
            Metallic,
            Roughness,
            Height
        }
        
        // Brush settings
        public Color BrushColor
        {
            get { return _brushColor; }
            set { _brushColor = value; }
        }
        
        public float BrushSize
        {
            get { return _brushSize; }
            set { _brushSize = Mathf.Clamp(value, 0.001f, 0.5f); }
        }
        
        public float BrushHardness
        {
            get { return _brushHardness; }
            set { _brushHardness = Mathf.Clamp01(value); }
        }
        
        public TextureChannel ActiveChannel
        {
            get { return _activeChannel; }
            set { _activeChannel = value; }
        }
        
        public TexturePainter()
        {
            // Initialize painting material
            Shader paintShader = Shader.Find("Hidden/Internal-Colored");
            if (paintShader != null)
            {
                _paintMaterial = new Material(paintShader);
                _paintMaterial.hideFlags = HideFlags.HideAndDontSave;
                _paintMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _paintMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _paintMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _paintMaterial.SetInt("_ZWrite", 0);
            }
        }
        
        // Initialize with a specific texture
        public void Initialize(Texture2D texture)
        {
            if (texture == null)
                return;
                
            // Create render textures based on input texture
            int textureSize = Mathf.Max(texture.width, texture.height);
            if (_paintTexture == null || _paintTexture.width != textureSize)
            {
                if (_paintTexture != null)
                    Object.DestroyImmediate(_paintTexture);
                    
                if (_tempTexture != null)
                    Object.DestroyImmediate(_tempTexture);
                    
                _paintTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
                _tempTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
                
                _paintTexture.enableRandomWrite = true;
                _tempTexture.enableRandomWrite = true;
                
                _paintTexture.Create();
                _tempTexture.Create();
            }
            
            // Copy texture to render texture
            Graphics.Blit(texture, _paintTexture);
        }
        
        // Apply the current paint texture back to the original texture
        public void ApplyChanges()
        {
            if (_paintTexture == null)
                return;
                
            // Create a new texture2D from the render texture
            RenderTexture.active = _paintTexture;
            
            Texture2D result = new Texture2D(_paintTexture.width, _paintTexture.height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, _paintTexture.width, _paintTexture.height), 0, 0);
            result.Apply();
            
            RenderTexture.active = null;
            
            // Here you would apply this texture back to your material
            // This depends on your specific implementation
        }
        
        // Paint a circle at the specified UV position
        public void PaintCircle(Vector2 uvPosition, float size, Color color, float hardness)
        {
            if (_paintTexture == null || _paintMaterial == null)
                return;
                
            // Store original values
            Color originalColor = _brushColor;
            float originalSize = _brushSize;
            float originalHardness = _brushHardness;
            
            // Set new values
            _brushColor = color;
            _brushSize = size;
            _brushHardness = hardness;
            
            // Paint
            Paint(uvPosition);
            
            // Restore original values
            _brushColor = originalColor;
            _brushSize = originalSize;
            _brushHardness = originalHardness;
        }
        
        // Start a painting session for a specific material
        public void StartPainting(Material targetMaterial, int textureSize = 1024)
        {
            // Load textures from the material
            if (targetMaterial != null)
            {
                _albedoMap = targetMaterial.GetTexture("_MainTex") as Texture2D;
                _normalMap = targetMaterial.GetTexture("_BumpMap") as Texture2D;
                _metallicMap = targetMaterial.GetTexture("_MetallicGlossMap") as Texture2D;
                // For roughness and height, it depends on the shader being used
                _roughnessMap = targetMaterial.GetTexture("_SpecGlossMap") as Texture2D;
                _heightMap = targetMaterial.GetTexture("_ParallaxMap") as Texture2D;
            }
            
            // Create render textures if needed
            if (_paintTexture == null || _paintTexture.width != textureSize)
            {
                if (_paintTexture != null)
                    Object.DestroyImmediate(_paintTexture);
                    
                if (_tempTexture != null)
                    Object.DestroyImmediate(_tempTexture);
                    
                _paintTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
                _tempTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
                
                _paintTexture.enableRandomWrite = true;
                _tempTexture.enableRandomWrite = true;
                
                _paintTexture.Create();
                _tempTexture.Create();
            }
            
            // Set initial texture based on active channel
            CopyTextureToRenderTexture(GetActiveTexture());
        }
        
        // End the painting session and apply changes
        public void EndPainting(Material targetMaterial)
        {
            if (targetMaterial == null)
                return;
                
            // Save the current texture
            Texture2D result = SaveRenderTextureToTexture();
            
            // Apply the texture to the material based on the active channel
            switch (_activeChannel)
            {
                case TextureChannel.Albedo:
                    targetMaterial.SetTexture("_MainTex", result);
                    _albedoMap = result;
                    break;
                case TextureChannel.Normal:
                    targetMaterial.SetTexture("_BumpMap", result);
                    _normalMap = result;
                    break;
                case TextureChannel.Metallic:
                    targetMaterial.SetTexture("_MetallicGlossMap", result);
                    _metallicMap = result;
                    break;
                case TextureChannel.Roughness:
                    targetMaterial.SetTexture("_SpecGlossMap", result);
                    _roughnessMap = result;
                    break;
                case TextureChannel.Height:
                    targetMaterial.SetTexture("_ParallaxMap", result);
                    _heightMap = result;
                    break;
            }
        }
        
        // Change the active texture channel
        public void SetActiveChannel(TextureChannel channel)
        {
            if (_activeChannel == channel)
                return;
                
            // Save current texture
            Texture2D savedTexture = SaveRenderTextureToTexture();
            
            // Update the appropriate texture
            switch (_activeChannel)
            {
                case TextureChannel.Albedo:
                    _albedoMap = savedTexture;
                    break;
                case TextureChannel.Normal:
                    _normalMap = savedTexture;
                    break;
                case TextureChannel.Metallic:
                    _metallicMap = savedTexture;
                    break;
                case TextureChannel.Roughness:
                    _roughnessMap = savedTexture;
                    break;
                case TextureChannel.Height:
                    _heightMap = savedTexture;
                    break;
            }
            
            // Set new active channel
            _activeChannel = channel;
            
            // Load new texture
            CopyTextureToRenderTexture(GetActiveTexture());
        }
        
        // Paint at a UV location
        public void Paint(Vector2 uvPosition)
        {
            if (_paintTexture == null || _paintMaterial == null)
                return;
                
            // Calculate brush position in texture space
            Vector2 brushPosition = new Vector2(
                uvPosition.x * _paintTexture.width,
                uvPosition.y * _paintTexture.height);
                
            float brushSizeInPixels = _brushSize * _paintTexture.width;
            
            // Draw a brush stroke
            RenderTexture.active = _paintTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, _paintTexture.width, _paintTexture.height, 0);
            
            _paintMaterial.color = _brushColor;
            
            // Draw a circle for the brush
            DrawBrush(brushPosition, brushSizeInPixels);
            
            GL.PopMatrix();
            RenderTexture.active = null;
        }
        
        // Draw the brush at the specified position with the current settings
        private void DrawBrush(Vector2 position, float size)
        {
            if (_paintMaterial == null)
                return;
                
            // Draw the brush as a series of circles with decreasing opacity
            float hardnessFactor = Mathf.Max(0.01f, _brushHardness);
            int steps = Mathf.CeilToInt(10 * (1 - hardnessFactor));
            
            // Ensure at least one step
            steps = Mathf.Max(1, steps);
            
            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)steps;
                float radius = size * (1 - t * hardnessFactor);
                
                // Calculate alpha based on distance from center
                float alpha = 1 - t;
                
                // Adjust color for this step
                Color stepColor = _brushColor;
                stepColor.a *= alpha;
                _paintMaterial.color = stepColor;
                
                // Draw the circle
                _paintMaterial.SetPass(0);
                
                GL.Begin(GL.TRIANGLE_STRIP);
                
                // Create a circle using triangle strip
                int segments = 36;
                for (int j = 0; j <= segments; j++)
                {
                    float angle = (j / (float)segments) * Mathf.PI * 2;
                    float x = position.x + Mathf.Cos(angle) * radius;
                    float y = position.y + Mathf.Sin(angle) * radius;
                    GL.Vertex3(x, y, 0);
                    
                    // Inner vertex (creates the triangle strip)
                    float innerRadius = radius * 0.9f;
                    x = position.x + Mathf.Cos(angle) * innerRadius;
                    y = position.y + Mathf.Sin(angle) * innerRadius;
                    GL.Vertex3(x, y, 0);
                }
                
                GL.End();
            }
        }
        
        // Save render texture to a regular texture2D
        private Texture2D SaveRenderTextureToTexture()
        {
            if (_paintTexture == null)
                return null;
                
            RenderTexture.active = _paintTexture;
            
            Texture2D texture = new Texture2D(_paintTexture.width, _paintTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, _paintTexture.width, _paintTexture.height), 0, 0);
            texture.Apply();
            
            RenderTexture.active = null;
            
            return texture;
        }
        
        // Copy a regular texture to the render texture
        private void CopyTextureToRenderTexture(Texture2D source)
        {
            if (_paintTexture == null)
                return;
                
            if (source != null)
            {
                // Copy the source texture to the render texture
                Graphics.Blit(source, _paintTexture);
            }
            else
            {
                // Create a new blank texture if no source exists
                RenderTexture.active = _paintTexture;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = null;
            }
        }
        
        // Get the current active texture based on the channel
        private Texture2D GetActiveTexture()
        {
            switch (_activeChannel)
            {
                case TextureChannel.Albedo:
                    return _albedoMap;
                case TextureChannel.Normal:
                    return _normalMap;
                case TextureChannel.Metallic:
                    return _metallicMap;
                case TextureChannel.Roughness:
                    return _roughnessMap;
                case TextureChannel.Height:
                    return _heightMap;
                default:
                    return null;
            }
        }
        
        // Export the current texture to a file
        public void ExportTexture(string path)
        {
            if (_paintTexture == null)
                return;
                
            Texture2D texture = SaveRenderTextureToTexture();
            
            if (texture != null)
            {
                byte[] bytes;
                string extension;
                
                if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                {
                    bytes = texture.EncodeToPNG();
                    extension = ".png";
                }
                else
                {
                    bytes = texture.EncodeToJPG();
                    extension = ".jpg";
                }
                
                string fullPath = path;
                if (!Path.HasExtension(fullPath))
                {
                    fullPath += extension;
                }
                
                try
                {
                    File.WriteAllBytes(fullPath, bytes);
                    AssetDatabase.Refresh();
                    Debug.Log("Texture saved to: " + fullPath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to save texture: " + e.Message);
                }
                
                Object.DestroyImmediate(texture);
            }
        }
        
        // Import a texture from a file
        public void ImportTexture(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
                
            try
            {
                Texture2D texture = new Texture2D(2, 2);
                byte[] fileData = File.ReadAllBytes(path);
                texture.LoadImage(fileData);
                
                CopyTextureToRenderTexture(texture);
                
                Object.DestroyImmediate(texture);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to import texture: " + e.Message);
            }
        }
        
        // Clean up resources
        public void Cleanup()
        {
            if (_paintTexture != null)
            {
                Object.DestroyImmediate(_paintTexture);
                _paintTexture = null;
            }
            
            if (_tempTexture != null)
            {
                Object.DestroyImmediate(_tempTexture);
                _tempTexture = null;
            }
            
            if (_paintMaterial != null)
            {
                Object.DestroyImmediate(_paintMaterial);
                _paintMaterial = null;
            }
        }
    }
} 