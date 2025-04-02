using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace ModelingToolkit
{
    public class MeshIOHandler
    {
        // Available file formats
        public enum FileFormat
        {
            OBJ,
            FBX,
            GLB
        }
        
        // Export mesh to file
        public static bool ExportMesh(Mesh mesh, string filePath, FileFormat format = FileFormat.OBJ)
        {
            if (mesh == null)
            {
                Debug.LogError("Cannot export a null mesh.");
                return false;
            }
            
            try
            {
                switch (format)
                {
                    case FileFormat.OBJ:
                        return ExportOBJ(mesh, filePath);
                    case FileFormat.FBX:
                        return ExportFBX(mesh, filePath);
                    case FileFormat.GLB:
                        return ExportGLB(mesh, filePath);
                    default:
                        Debug.LogError("Unsupported file format.");
                        return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error exporting mesh: {e.Message}");
                return false;
            }
        }
        
        // Import mesh from file
        public static Mesh ImportMesh(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("File does not exist: " + filePath);
                return null;
            }
            
            string extension = Path.GetExtension(filePath).ToLower();
            
            try
            {
                switch (extension)
                {
                    case ".obj":
                        return ImportOBJ(filePath);
                    case ".fbx":
                        return ImportFBX(filePath);
                    case ".glb":
                        return ImportGLB(filePath);
                    default:
                        Debug.LogError("Unsupported file format: " + extension);
                        return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error importing mesh: {e.Message}");
                return null;
            }
        }
        
        // Export mesh to OBJ format
        private static bool ExportOBJ(Mesh mesh, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            
            // Add header
            sb.AppendLine("# Exported from Unity by ModelingToolkit");
            sb.AppendLine("# https://github.com/yourusername/modelingtoolkit");
            sb.AppendLine();
            
            // Add vertices
            Vector3[] vertices = mesh.vertices;
            foreach (Vector3 v in vertices)
            {
                // Convert from Unity's left-handed to OBJ's right-handed coordinate system
                sb.AppendLine($"v {v.x.ToString(CultureInfo.InvariantCulture)} {v.y.ToString(CultureInfo.InvariantCulture)} {v.z.ToString(CultureInfo.InvariantCulture)}");
            }
            
            // Add UVs
            Vector2[] uvs = mesh.uv;
            if (uvs != null && uvs.Length > 0)
            {
                foreach (Vector2 uv in uvs)
                {
                    // OBJ format has origin at bottom-left, Unity has at top-left
                    sb.AppendLine($"vt {uv.x.ToString(CultureInfo.InvariantCulture)} {uv.y.ToString(CultureInfo.InvariantCulture)}");
                }
            }
            
            // Add normals
            Vector3[] normals = mesh.normals;
            if (normals != null && normals.Length > 0)
            {
                foreach (Vector3 n in normals)
                {
                    // Convert from Unity's left-handed to OBJ's right-handed coordinate system
                    sb.AppendLine($"vn {n.x.ToString(CultureInfo.InvariantCulture)} {n.y.ToString(CultureInfo.InvariantCulture)} {n.z.ToString(CultureInfo.InvariantCulture)}");
                }
            }
            
            // Add object name
            sb.AppendLine($"g {Path.GetFileNameWithoutExtension(filePath)}");
            
            // Add faces
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // OBJ uses 1-based indices, but Unity uses 0-based indices
                // Also reverse winding order to convert from Unity's coordinate system
                if (uvs.Length > 0 && normals.Length > 0)
                {
                    sb.AppendLine($"f {triangles[i+2]+1}/{triangles[i+2]+1}/{triangles[i+2]+1} {triangles[i+1]+1}/{triangles[i+1]+1}/{triangles[i+1]+1} {triangles[i]+1}/{triangles[i]+1}/{triangles[i]+1}");
                }
                else if (uvs.Length > 0)
                {
                    sb.AppendLine($"f {triangles[i+2]+1}/{triangles[i+2]+1} {triangles[i+1]+1}/{triangles[i+1]+1} {triangles[i]+1}/{triangles[i]+1}");
                }
                else if (normals.Length > 0)
                {
                    sb.AppendLine($"f {triangles[i+2]+1}//{triangles[i+2]+1} {triangles[i+1]+1}//{triangles[i+1]+1} {triangles[i]+1}//{triangles[i]+1}");
                }
                else
                {
                    sb.AppendLine($"f {triangles[i+2]+1} {triangles[i+1]+1} {triangles[i]+1}");
                }
            }
            
            // Write to file
            File.WriteAllText(filePath, sb.ToString());
            
            Debug.Log("Mesh exported to: " + filePath);
            return true;
        }
        
        // Import mesh from OBJ format
        private static Mesh ImportOBJ(string filePath)
        {
            if (!File.Exists(filePath))
                return null;
                
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();
            
            // Dictionary to store vertex/normal/uv combinations
            Dictionary<string, int> vertexCache = new Dictionary<string, int>();
            int vertexCount = 0;
            
            // Read the file line by line
            string[] lines = File.ReadAllLines(filePath);
            
            // First pass: Read all vertices, normals, and UVs
            foreach (string line in lines)
            {
                string[] parts = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length == 0)
                    continue;
                    
                if (parts[0] == "v" && parts.Length >= 4)
                {
                    // Parse vertex
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    
                    // Convert from OBJ's right-handed to Unity's left-handed coordinate system
                    vertices.Add(new Vector3(x, y, z));
                }
                else if (parts[0] == "vn" && parts.Length >= 4)
                {
                    // Parse normal
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    
                    // Convert from OBJ's right-handed to Unity's left-handed coordinate system
                    normals.Add(new Vector3(x, y, z));
                }
                else if (parts[0] == "vt" && parts.Length >= 3)
                {
                    // Parse texture coordinate
                    float u = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float v = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    
                    // OBJ format has origin at bottom-left, Unity has at top-left
                    uvs.Add(new Vector2(u, v));
                }
            }
            
            // Second pass: Read all faces and create triangles
            List<Vector3> finalVertices = new List<Vector3>();
            List<Vector3> finalNormals = new List<Vector3>();
            List<Vector2> finalUVs = new List<Vector2>();
            
            foreach (string line in lines)
            {
                string[] parts = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length == 0)
                    continue;
                    
                if (parts[0] == "f")
                {
                    // Parse face
                    if (parts.Length >= 4)
                    {
                        // Parse each vertex in the face
                        for (int i = 1; i < parts.Length; i++)
                        {
                            string vertexInfo = parts[i];
                            
                            if (!vertexCache.TryGetValue(vertexInfo, out int index))
                            {
                                // Parse vertex/uv/normal indices
                                string[] indices = vertexInfo.Split('/');
                                
                                int vertexIndex = int.Parse(indices[0]) - 1; // OBJ is 1-based
                                
                                int uvIndex = -1;
                                if (indices.Length > 1 && indices[1] != "")
                                    uvIndex = int.Parse(indices[1]) - 1;
                                    
                                int normalIndex = -1;
                                if (indices.Length > 2 && indices[2] != "")
                                    normalIndex = int.Parse(indices[2]) - 1;
                                    
                                // Add vertex
                                finalVertices.Add(vertices[vertexIndex]);
                                
                                // Add UV if available
                                if (uvIndex >= 0 && uvIndex < uvs.Count)
                                    finalUVs.Add(uvs[uvIndex]);
                                else if (finalUVs.Count > 0) // If some UVs exist, add a default
                                    finalUVs.Add(Vector2.zero);
                                    
                                // Add normal if available
                                if (normalIndex >= 0 && normalIndex < normals.Count)
                                    finalNormals.Add(normals[normalIndex]);
                                else if (finalNormals.Count > 0) // If some normals exist, add a default
                                    finalNormals.Add(Vector3.up);
                                    
                                // Cache vertex and assign index
                                vertexCache[vertexInfo] = vertexCount;
                                index = vertexCount;
                                vertexCount++;
                            }
                            
                            // Add to triangle list for triangulation
                            triangles.Add(index);
                        }
                        
                        // Triangulate the face (assuming it's convex)
                        if (triangles.Count >= 3)
                        {
                            for (int i = 1; i < triangles.Count - 1; i++)
                            {
                                // Need to reverse winding order for Unity
                                triangles.Add(triangles[0]);
                                triangles.Add(triangles[i + 1]);
                                triangles.Add(triangles[i]);
                            }
                        }
                        
                        triangles.Clear();
                    }
                }
            }
            
            // Create and configure mesh
            Mesh mesh = new Mesh();
            mesh.indexFormat = finalVertices.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.vertices = finalVertices.ToArray();
            
            if (finalUVs.Count == finalVertices.Count)
                mesh.uv = finalUVs.ToArray();
                
            if (finalNormals.Count == finalVertices.Count)
                mesh.normals = finalNormals.ToArray();
            else
                mesh.RecalculateNormals();
                
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        // Export mesh to FBX format
        private static bool ExportFBX(Mesh mesh, string filePath)
        {
            Debug.LogWarning("FBX export requires Unity's FBX Exporter package. Please install it from the Package Manager.");
            
            // This would use the Unity FBX Exporter package
            // For simplicity, we're not implementing this here, as it requires additional packages
            
            return false;
        }
        
        // Import mesh from FBX format
        private static Mesh ImportFBX(string filePath)
        {
            Debug.LogWarning("FBX import is handled by Unity's built-in importer. This method will create an asset in your project.");
            
            // Get the relative path within the project
            string projectPath = Application.dataPath;
            string relativePath = "";
            
            if (filePath.StartsWith(projectPath))
            {
                relativePath = "Assets" + filePath.Substring(projectPath.Length);
            }
            else
            {
                // Copy the file to the project if it's external
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(Application.dataPath, "Models", fileName);
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                
                File.Copy(filePath, destPath, true);
                AssetDatabase.Refresh();
                
                relativePath = "Assets/Models/" + fileName;
            }
            
            // Import the asset
            ModelImporter importer = AssetImporter.GetAtPath(relativePath) as ModelImporter;
            if (importer != null)
            {
                // Configure import settings if needed
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.SaveAndReimport();
            }
            
            // Load the mesh from the imported model
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
            if (prefab != null)
            {
                MeshFilter meshFilter = prefab.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null)
                {
                    return Object.Instantiate(meshFilter.sharedMesh);
                }
            }
            
            return null;
        }
        
        // Export mesh to GLB format
        private static bool ExportGLB(Mesh mesh, string filePath)
        {
            Debug.LogWarning("GLB export is not implemented in this version. It requires the glTF exporter package.");
            
            // This would use a glTF/GLB exporter package
            // For simplicity, we're not implementing this here
            
            return false;
        }
        
        // Import mesh from GLB format
        private static Mesh ImportGLB(string filePath)
        {
            Debug.LogWarning("GLB import is not implemented in this version. It requires the glTF importer package.");
            
            // This would use a glTF/GLB importer package
            // For simplicity, we're not implementing this here
            
            return null;
        }
    }
}