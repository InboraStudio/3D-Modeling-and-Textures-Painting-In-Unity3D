using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ModelingToolkit
{
    public class MeshOperations
    {
        // Basic primitive creation
        public static Mesh CreateCubeMesh(float size = 1f)
        {
            Mesh mesh = new Mesh();
            
            // Define the 8 corners of a cube
            Vector3 p0 = new Vector3(-size * 0.5f, -size * 0.5f, -size * 0.5f);
            Vector3 p1 = new Vector3(size * 0.5f, -size * 0.5f, -size * 0.5f);
            Vector3 p2 = new Vector3(size * 0.5f, -size * 0.5f, size * 0.5f);
            Vector3 p3 = new Vector3(-size * 0.5f, -size * 0.5f, size * 0.5f);
            Vector3 p4 = new Vector3(-size * 0.5f, size * 0.5f, -size * 0.5f);
            Vector3 p5 = new Vector3(size * 0.5f, size * 0.5f, -size * 0.5f);
            Vector3 p6 = new Vector3(size * 0.5f, size * 0.5f, size * 0.5f);
            Vector3 p7 = new Vector3(-size * 0.5f, size * 0.5f, size * 0.5f);
            
            // Define vertices
            Vector3[] vertices = new Vector3[]
            {
                // Bottom face
                p0, p1, p2, p3,
                // Top face
                p4, p5, p6, p7,
                // Front face
                p3, p2, p6, p7,
                // Back face
                p0, p1, p5, p4,
                // Left face
                p0, p3, p7, p4,
                // Right face
                p1, p2, p6, p5
            };
            
            // Define triangles (two triangles per face)
            int[] triangles = new int[]
            {
                // Bottom face
                0, 1, 2, 0, 2, 3,
                // Top face
                4, 6, 5, 4, 7, 6,
                // Front face
                8, 9, 10, 8, 10, 11,
                // Back face
                12, 14, 13, 12, 15, 14,
                // Left face
                16, 17, 18, 16, 18, 19,
                // Right face
                20, 22, 21, 20, 23, 22
            };
            
            // Define UVs
            Vector2[] uvs = new Vector2[]
            {
                // Bottom face
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                // Top face
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                // Front face
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                // Back face
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                // Left face
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                // Right face
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
            };
            
            // Assign to mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            
            // Calculate normals
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        public static Mesh CreateSphereMesh(float radius = 0.5f, int segments = 24)
        {
            Mesh mesh = new Mesh();
            
            // Ensure we have enough segments
            segments = Mathf.Max(segments, 4);
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // Create top and bottom vertices
            vertices.Add(new Vector3(0, radius, 0)); // Top vertex
            vertices.Add(new Vector3(0, -radius, 0)); // Bottom vertex
            
            // Add UV coordinates for top and bottom vertices
            uvs.Add(new Vector2(0.5f, 1)); // Top vertex
            uvs.Add(new Vector2(0.5f, 0)); // Bottom vertex
            
            // Create vertices for each latitude ring
            for (int lat = 1; lat < segments; lat++)
            {
                float theta = lat * Mathf.PI / segments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                
                // Create vertices for each longitude line
                for (int lon = 0; lon < segments; lon++)
                {
                    float phi = lon * 2 * Mathf.PI / segments;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);
                    
                    float x = radius * sinTheta * cosPhi;
                    float y = radius * cosTheta;
                    float z = radius * sinTheta * sinPhi;
                    
                    vertices.Add(new Vector3(x, y, z));
                    
                    // Calculate UV coordinates
                    float u = (float)lon / segments;
                    float v = (float)lat / segments;
                    uvs.Add(new Vector2(u, v));
                }
            }
            
            // Create triangles for top cap
            for (int lon = 0; lon < segments; lon++)
            {
                int current = lon + 2;
                int next = (lon + 1) % segments + 2;
                
                triangles.Add(0); // Top vertex
                triangles.Add(current);
                triangles.Add(next);
            }
            
            // Create triangles for each latitude strip
            for (int lat = 0; lat < segments - 2; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    int current = lat * segments + lon + 2;
                    int next = lat * segments + (lon + 1) % segments + 2;
                    int currentBelow = (lat + 1) * segments + lon + 2;
                    int nextBelow = (lat + 1) * segments + (lon + 1) % segments + 2;
                    
                    triangles.Add(current);
                    triangles.Add(currentBelow);
                    triangles.Add(next);
                    
                    triangles.Add(next);
                    triangles.Add(currentBelow);
                    triangles.Add(nextBelow);
                }
            }
            
            // Create triangles for bottom cap
            for (int lon = 0; lon < segments; lon++)
            {
                int current = (segments - 2) * segments + lon + 2;
                int next = (segments - 2) * segments + (lon + 1) % segments + 2;
                
                triangles.Add(1); // Bottom vertex
                triangles.Add(next);
                triangles.Add(current);
            }
            
            // Assign to mesh
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            
            // Calculate normals
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        public static Mesh CreateCylinderMesh(float radius = 0.5f, float height = 1f, int segments = 24)
        {
            Mesh mesh = new Mesh();
            
            // Ensure we have enough segments
            segments = Mathf.Max(segments, 3);
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // Create top and bottom center vertices
            vertices.Add(new Vector3(0, height/2, 0)); // Top center
            vertices.Add(new Vector3(0, -height/2, 0)); // Bottom center
            
            // Add UV coordinates for centers
            uvs.Add(new Vector2(0.5f, 0.5f)); // Top center
            uvs.Add(new Vector2(0.5f, 0.5f)); // Bottom center
            
            // Create vertices for top and bottom caps
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2 * Mathf.PI / segments;
                float x = radius * Mathf.Cos(angle);
                float z = radius * Mathf.Sin(angle);
                
                // Top rim vertex
                vertices.Add(new Vector3(x, height/2, z));
                
                // Bottom rim vertex
                vertices.Add(new Vector3(x, -height/2, z));
                
                // UV coordinates
                float u = (Mathf.Cos(angle) + 1) / 2;
                float v = (Mathf.Sin(angle) + 1) / 2;
                
                // Add UVs for top rim
                uvs.Add(new Vector2(u, v));
                
                // Add UVs for bottom rim
                uvs.Add(new Vector2(u, v));
            }
            
            // Create triangles for top cap
            for (int i = 0; i < segments; i++)
            {
                int current = i * 2 + 2;
                int next = ((i + 1) % segments) * 2 + 2;
                
                triangles.Add(0); // Top center
                triangles.Add(current);
                triangles.Add(next);
            }
            
            // Create triangles for bottom cap
            for (int i = 0; i < segments; i++)
            {
                int current = i * 2 + 3;
                int next = ((i + 1) % segments) * 2 + 3;
                
                triangles.Add(1); // Bottom center
                triangles.Add(next);
                triangles.Add(current);
            }
            
            // Create triangles for cylinder sides
            for (int i = 0; i < segments; i++)
            {
                int current = i * 2 + 2; // Top rim
                int next = ((i + 1) % segments) * 2 + 2; // Next top rim
                int currentBottom = i * 2 + 3; // Bottom rim
                int nextBottom = ((i + 1) % segments) * 2 + 3; // Next bottom rim
                
                // First triangle
                triangles.Add(current);
                triangles.Add(currentBottom);
                triangles.Add(next);
                
                // Second triangle
                triangles.Add(next);
                triangles.Add(currentBottom);
                triangles.Add(nextBottom);
            }
            
            // Assign to mesh
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            
            // Calculate normals
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        public static Mesh CreatePlaneMesh(float width = 1f, float height = 1f, int widthSegments = 1, int heightSegments = 1)
        {
            Mesh mesh = new Mesh();
            
            // Ensure we have at least 1 segment
            widthSegments = Mathf.Max(widthSegments, 1);
            heightSegments = Mathf.Max(heightSegments, 1);
            
            // Calculate vertices
            int vertexCount = (widthSegments + 1) * (heightSegments + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            
            for (int z = 0; z <= heightSegments; z++)
            {
                for (int x = 0; x <= widthSegments; x++)
                {
                    float xPos = ((float)x / widthSegments - 0.5f) * width;
                    float zPos = ((float)z / heightSegments - 0.5f) * height;
                    int index = z * (widthSegments + 1) + x;
                    
                    vertices[index] = new Vector3(xPos, 0, zPos);
                    uvs[index] = new Vector2((float)x / widthSegments, (float)z / heightSegments);
                }
            }
            
            // Calculate triangles
            int triangleCount = widthSegments * heightSegments * 6;
            int[] triangles = new int[triangleCount];
            int t = 0;
            
            for (int z = 0; z < heightSegments; z++)
            {
                for (int x = 0; x < widthSegments; x++)
                {
                    int index = z * (widthSegments + 1) + x;
                    
                    triangles[t++] = index;
                    triangles[t++] = index + 1;
                    triangles[t++] = index + widthSegments + 1;
                    
                    triangles[t++] = index + widthSegments + 1;
                    triangles[t++] = index + 1;
                    triangles[t++] = index + widthSegments + 2;
                }
            }
            
            // Assign to mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            
            // Calculate normals
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        // Mesh modification operations
        public static Mesh ExtrudeFaces(Mesh mesh, int[] triangleIndices, float distance)
        {
            if (mesh == null || triangleIndices == null || triangleIndices.Length == 0)
                return mesh;
                
            // Get mesh data
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector2[] uvs = mesh.uv;
            Vector3[] normals = mesh.normals;
            
            // Validate triangle indices
            for (int i = 0; i < triangleIndices.Length; i++)
            {
                if (triangleIndices[i] < 0 || triangleIndices[i] >= triangles.Length)
                {
                    Debug.LogError("Invalid triangle index: " + triangleIndices[i]);
                    return mesh;
                }
            }
            
            // Create new arrays for the modified mesh
            List<Vector3> newVertices = new List<Vector3>(vertices);
            List<int> newTriangles = new List<int>(triangles);
            List<Vector2> newUVs = new List<Vector2>(uvs);
            List<Vector3> newNormals = new List<Vector3>(normals);
            
            // Extrude each face (triangle)
            for (int i = 0; i < triangleIndices.Length; i += 3)
            {
                // Get the three vertex indices of the triangle
                int i1 = triangles[triangleIndices[i]];
                int i2 = triangles[triangleIndices[i + 1]];
                int i3 = triangles[triangleIndices[i + 2]];
                
                // Get the three vertices of the triangle
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];
                
                // Calculate the face normal
                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
                
                // Create three new vertices by extruding along the normal
                Vector3 v1Extruded = v1 + normal * distance;
                Vector3 v2Extruded = v2 + normal * distance;
                Vector3 v3Extruded = v3 + normal * distance;
                
                // Add the new vertices
                int newV1Index = newVertices.Count;
                newVertices.Add(v1Extruded);
                newUVs.Add(uvs[i1]);
                newNormals.Add(normal);
                
                int newV2Index = newVertices.Count;
                newVertices.Add(v2Extruded);
                newUVs.Add(uvs[i2]);
                newNormals.Add(normal);
                
                int newV3Index = newVertices.Count;
                newVertices.Add(v3Extruded);
                newUVs.Add(uvs[i3]);
                newNormals.Add(normal);
                
                // Create the new extruded face
                newTriangles.Add(newV1Index);
                newTriangles.Add(newV2Index);
                newTriangles.Add(newV3Index);
                
                // Create the side faces
                // Side 1
                newTriangles.Add(i1);
                newTriangles.Add(i2);
                newTriangles.Add(newV1Index);
                
                newTriangles.Add(newV1Index);
                newTriangles.Add(i2);
                newTriangles.Add(newV2Index);
                
                // Side 2
                newTriangles.Add(i2);
                newTriangles.Add(i3);
                newTriangles.Add(newV2Index);
                
                newTriangles.Add(newV2Index);
                newTriangles.Add(i3);
                newTriangles.Add(newV3Index);
                
                // Side 3
                newTriangles.Add(i3);
                newTriangles.Add(i1);
                newTriangles.Add(newV3Index);
                
                newTriangles.Add(newV3Index);
                newTriangles.Add(i1);
                newTriangles.Add(newV1Index);
            }
            
            // Create and return the new mesh
            Mesh newMesh = new Mesh();
            newMesh.vertices = newVertices.ToArray();
            newMesh.triangles = newTriangles.ToArray();
            newMesh.uv = newUVs.ToArray();
            newMesh.normals = newNormals.ToArray();
            
            newMesh.RecalculateBounds();
            
            return newMesh;
        }
        
        public static Mesh BevelEdges(Mesh mesh, int[] edgeIndices, float amount)
        {
            // This is a placeholder for a bevel operation
            // In a real implementation, this would create a beveled edge
            Debug.LogWarning("BevelEdges operation is not fully implemented in this version");
            
            return mesh;
        }
        
        public static Mesh LoopCut(Mesh mesh, Vector3 planePosition, Vector3 planeNormal)
        {
            // This is a placeholder for a loop cut operation
            // In a real implementation, this would add a loop of edges
            Debug.LogWarning("LoopCut operation is not fully implemented in this version");
            
            return mesh;
        }
        
        // Boolean operations
        public static Mesh BooleanUnion(Mesh meshA, Mesh meshB)
        {
            // This is a placeholder for a boolean union operation
            // For a real implementation, a computational geometry library would be needed
            Debug.LogWarning("Boolean operations are not implemented in this version");
            
            return meshA;
        }
        
        public static Mesh BooleanSubtract(Mesh meshA, Mesh meshB)
        {
            // This is a placeholder for a boolean subtraction operation
            Debug.LogWarning("Boolean operations are not implemented in this version");
            
            return meshA;
        }
        
        public static Mesh BooleanIntersect(Mesh meshA, Mesh meshB)
        {
            // This is a placeholder for a boolean intersection operation
            Debug.LogWarning("Boolean operations are not implemented in this version");
            
            return meshA;
        }
        
        // Mesh smoothing
        public static Mesh SmoothMesh(Mesh mesh, float strength, int iterations)
        {
            if (mesh == null)
                return null;
                
            Mesh result = Object.Instantiate(mesh);
            Vector3[] vertices = result.vertices;
            int[] triangles = result.triangles;
            
            // Build adjacency information
            Dictionary<int, List<int>> adjacentVertices = new Dictionary<int, List<int>>();
            
            for (int i = 0; i < vertices.Length; i++)
            {
                adjacentVertices[i] = new List<int>();
            }
            
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];
                
                if (!adjacentVertices[v1].Contains(v2)) adjacentVertices[v1].Add(v2);
                if (!adjacentVertices[v1].Contains(v3)) adjacentVertices[v1].Add(v3);
                
                if (!adjacentVertices[v2].Contains(v1)) adjacentVertices[v2].Add(v1);
                if (!adjacentVertices[v2].Contains(v3)) adjacentVertices[v2].Add(v3);
                
                if (!adjacentVertices[v3].Contains(v1)) adjacentVertices[v3].Add(v1);
                if (!adjacentVertices[v3].Contains(v2)) adjacentVertices[v3].Add(v2);
            }
            
            // Perform smoothing
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Vector3[] newVertices = new Vector3[vertices.Length];
                
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (adjacentVertices[i].Count > 0)
                    {
                        Vector3 average = Vector3.zero;
                        
                        foreach (int adjacentIndex in adjacentVertices[i])
                        {
                            average += vertices[adjacentIndex];
                        }
                        
                        average /= adjacentVertices[i].Count;
                        
                        // Interpolate between original position and average based on strength
                        newVertices[i] = Vector3.Lerp(vertices[i], average, strength);
                    }
                    else
                    {
                        newVertices[i] = vertices[i];
                    }
                }
                
                vertices = newVertices;
            }
            
            result.vertices = vertices;
            result.RecalculateNormals();
            result.RecalculateBounds();
            
            return result;
        }
        
        // Subdivision
        public static Mesh SubdivideMesh(Mesh mesh, int iterations)
        {
            if (mesh == null)
                return null;
                
            Mesh result = Object.Instantiate(mesh);
            
            for (int i = 0; i < iterations; i++)
            {
                result = SubdivideOnce(result);
            }
            
            return result;
        }
        
        private static Mesh SubdivideOnce(Mesh mesh)
        {
            // This is a simplified subdivision implementation
            // A full implementation would use a proper subdivision scheme like Catmull-Clark
                
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector2[] uvs = mesh.uv;
            
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector2> newUVs = new List<Vector2>();
            List<int> newTriangles = new List<int>();
            
            // Add original vertices
            for (int i = 0; i < vertices.Length; i++)
            {
                newVertices.Add(vertices[i]);
                if (i < uvs.Length)
                    newUVs.Add(uvs[i]);
                else
                    newUVs.Add(Vector2.zero);
            }
            
            // Dictionary to store edge midpoints
            Dictionary<Edge, int> edgeMidpoints = new Dictionary<Edge, int>();
            
            // Process each triangle
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];
                
                // Get or create edge midpoints
                int e1 = GetEdgeMidpoint(edgeMidpoints, v1, v2, vertices, uvs, newVertices, newUVs);
                int e2 = GetEdgeMidpoint(edgeMidpoints, v2, v3, vertices, uvs, newVertices, newUVs);
                int e3 = GetEdgeMidpoint(edgeMidpoints, v3, v1, vertices, uvs, newVertices, newUVs);
                
                // Create 4 new triangles (center and three corners)
                newTriangles.Add(v1); newTriangles.Add(e1); newTriangles.Add(e3);
                newTriangles.Add(e1); newTriangles.Add(v2); newTriangles.Add(e2);
                newTriangles.Add(e3); newTriangles.Add(e2); newTriangles.Add(v3);
                newTriangles.Add(e1); newTriangles.Add(e2); newTriangles.Add(e3);
            }
            
            // Create new mesh
            Mesh newMesh = new Mesh();
            newMesh.vertices = newVertices.ToArray();
            newMesh.triangles = newTriangles.ToArray();
            newMesh.uv = newUVs.ToArray();
            
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();
            
            return newMesh;
        }
        
        private static int GetEdgeMidpoint(Dictionary<Edge, int> edgeMidpoints, int v1, int v2, 
                                         Vector3[] vertices, Vector2[] uvs, 
                                         List<Vector3> newVertices, List<Vector2> newUVs)
        {
            Edge edge = new Edge(v1, v2);
            
            if (edgeMidpoints.TryGetValue(edge, out int midpointIndex))
            {
                return midpointIndex;
            }
            else
            {
                Vector3 midpoint = (vertices[v1] + vertices[v2]) * 0.5f;
                Vector2 uvMidpoint = Vector2.zero;
                
                if (v1 < uvs.Length && v2 < uvs.Length)
                {
                    uvMidpoint = (uvs[v1] + uvs[v2]) * 0.5f;
                }
                
                int index = newVertices.Count;
                newVertices.Add(midpoint);
                newUVs.Add(uvMidpoint);
                edgeMidpoints[edge] = index;
                
                return index;
            }
        }
        
        // Helper struct for subdivision
        private struct Edge
        {
            public int v1;
            public int v2;
            
            public Edge(int a, int b)
            {
                // Ensure v1 is always the smaller index for consistent dictionary keys
                if (a < b)
                {
                    v1 = a;
                    v2 = b;
                }
                else
                {
                    v1 = b;
                    v2 = a;
                }
            }
            
            public override bool Equals(object obj)
            {
                if (obj is Edge other)
                {
                    return v1 == other.v1 && v2 == other.v2;
                }
                return false;
            }
            
            public override int GetHashCode()
            {
                return v1 * 100000 + v2;
            }
        }
    }
} 