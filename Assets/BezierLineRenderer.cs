using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class BezierLineRenderer : MonoBehaviour
{
    [SerializeField] private LineSegment[] segments = null;
    [SerializeField] private Material lineMaterial = null;
    
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        this.meshFilter = GetComponent<MeshFilter>();
        this.meshRenderer = GetComponent<MeshRenderer>();
        UpdateMesh();
    }
    
    private void Rotate(List<Vector3> vertices, Vector2 angles, Vector3 startPos, int firstVertexIndex, int vertexCount)
    {
        Vector3 center = Vector3.zero;
        Quaternion lookRot = Quaternion.Euler(angles.x, angles.y, 0.0f);
        
        for (int i = firstVertexIndex; i < vertexCount; i++)
        {
            vertices[i] = startPos + (lookRot * (vertices[i] - center) + center);
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < this.segments.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(this.segments[i].StartPoint, 0.025f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(this.segments[i].EndPoint, 0.025f);
        }
    }

    private Vector2 CalculateAngles(LineSegment segment)
    {
        Vector3 start = segment.StartPoint;
        start.y = 0.0f;
        Vector3 end = segment.EndPoint;
        end.y = 0.0f;
        
        

        Vector3 dir = end - start;

        float yAngle = Vector3.SignedAngle(dir, Vector3.forward, Vector3.up);
        float xAngle = -Vector3.Angle(segment.EndPoint - segment.StartPoint, Vector3.up);
        
        
        return new Vector2(-xAngle - 90.0f, -yAngle);
    }

    public void UpdateMesh()
    {
        if (!meshFilter)
            meshFilter = GetComponent<MeshFilter>();
        
        if(!meshRenderer)
            meshRenderer = GetComponent<MeshRenderer>();
        
        var refMesh = meshFilter.sharedMesh;
        refMesh = new Mesh();
        refMesh.Clear();

        refMesh.subMeshCount = segments.Length;
        
        Material[] materials = new Material[segments.Length];
        for (int i = 0; i < materials.Length; i++) materials[i] = lineMaterial;


        this.meshRenderer.sharedMaterials = materials;

        int firstVertexIndex = 0;
        int lastVertexIndex = 0;


        List<Vector3> a_vertices = new List<Vector3>();
        List<Vector3> a_normals = new List<Vector3>();
        List<Vector2> a_uvs = new List<Vector2>();
        List<TriangleListHolder> a_triangles = new List<TriangleListHolder>(); 

        for (int i = 0; i < this.segments.Length; i++)
        {
            int previousVertexCount = a_vertices.Count;
            
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();
            
            Vector2 angles = CalculateAngles(segments[i]);
            
            LineSegment copy = new LineSegment(segments[i])
            {
                StartPoint = Vector3.zero,
                EndPoint = new Vector3(0, 0, Vector3.Distance(segments[i].StartPoint, segments[i].EndPoint))
            };

            AddEdge(vertices, uvs, normals, triangles, copy, previousVertexCount);
            Rotate(vertices, angles, segments[i].StartPoint, firstVertexIndex, vertices.Count - firstVertexIndex);
            
            
            a_vertices.AddRange(vertices);
            a_normals.AddRange(normals);
            a_uvs.AddRange(uvs);
            
            a_triangles.Add(new TriangleListHolder(triangles));
        }
        
        
        refMesh.SetVertices(a_vertices);

        for (int i = 0; i < a_triangles.Count; i++)
        {
            refMesh.SetTriangles(a_triangles[i].Triangles, i);
        }
        
        refMesh.SetNormals(a_normals);
        refMesh.SetUVs(0, a_uvs);

        refMesh.RecalculateNormals();
        
        meshFilter.sharedMesh = refMesh;
    }

    private void OnValidate()
    {
        UpdateMesh();
    }

    private void AddTriangles(List<int> triangles, int offset)
    {
        int[] faceTriangles = {0, 1, 2, 3, 2, 1};
        
        for (int i = 0; i < faceTriangles.Length; i++)
        {
            triangles.Add(offset + faceTriangles[i]);
        }
    }
    
    private void AddEdge(List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, LineSegment segment, int previousVertexCount)
    {
        Vector3[] startingValues =
        {
            segment.StartPoint + new Vector3(-segment.Thickness, -segment.Thickness, -segment.Thickness),
            segment.StartPoint + new Vector3(-segment.Thickness, +segment.Thickness, -segment.Thickness),
            segment.StartPoint + new Vector3(+segment.Thickness, -segment.Thickness, -segment.Thickness),
            segment.StartPoint + new Vector3(+segment.Thickness, +segment.Thickness, -segment.Thickness),
        };
        
        Vector3[] endingValues =
        {
            segment.EndPoint + new Vector3(-segment.Thickness, -segment.Thickness, +segment.Thickness),
            segment.EndPoint + new Vector3(+segment.Thickness, -segment.Thickness, +segment.Thickness),
            segment.EndPoint + new Vector3(-segment.Thickness, +segment.Thickness, +segment.Thickness),
            segment.EndPoint + new Vector3(+segment.Thickness, +segment.Thickness, +segment.Thickness),
        };
        
        
        // South side
        AddTriangles(triangles, vertices.Count + previousVertexCount);
        
        vertices.Add(startingValues[0]);
        vertices.Add(startingValues[1]);
        vertices.Add(startingValues[2]);
        vertices.Add(startingValues[3]);
        
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        // East side
        Subdivide(vertices, uvs, normals, triangles, false, segment,
            new [] {
                segment.StartPoint + new Vector3(+segment.Thickness, -segment.Thickness, -segment.Thickness),
                segment.StartPoint + new Vector3(+segment.Thickness, +segment.Thickness, -segment.Thickness)
            }, new [] {
                segment.EndPoint + new Vector3(+segment.Thickness, -segment.Thickness, +segment.Thickness),
                segment.EndPoint + new Vector3(+segment.Thickness, +segment.Thickness, +segment.Thickness),
            }, Vector3.left, previousVertexCount
        );
        
        // West side
        Subdivide(vertices, uvs, normals, triangles, false, segment,
            new [] {
                segment.StartPoint + new Vector3(-segment.Thickness, +segment.Thickness, -segment.Thickness),
                segment.StartPoint + new Vector3(-segment.Thickness, -segment.Thickness, -segment.Thickness),
            }, new [] {
                segment.EndPoint + new Vector3(-segment.Thickness, +segment.Thickness, +segment.Thickness),
                segment.EndPoint + new Vector3(-segment.Thickness, -segment.Thickness, +segment.Thickness),
            }, Vector3.right, previousVertexCount
        );
        
        // Upper side
        Subdivide(vertices, uvs, normals, triangles, false, segment,
            new [] {
                segment.StartPoint + new Vector3(+segment.Thickness, +segment.Thickness, -segment.Thickness),
                segment.StartPoint + new Vector3(-segment.Thickness, +segment.Thickness, -segment.Thickness),
            }, new [] {
                segment.EndPoint + new Vector3(+segment.Thickness, +segment.Thickness, +segment.Thickness),
                segment.EndPoint + new Vector3(-segment.Thickness, +segment.Thickness, +segment.Thickness),
            }, Vector3.up, previousVertexCount
        );
        
        // Down side
        Subdivide(vertices, uvs, normals, triangles, false, segment,
            new [] {
                segment.StartPoint + new Vector3(-segment.Thickness, -segment.Thickness, -segment.Thickness),
                segment.StartPoint + new Vector3(+segment.Thickness, -segment.Thickness, -segment.Thickness),
            }, new [] {
                segment.EndPoint + new Vector3(-segment.Thickness, -segment.Thickness, +segment.Thickness),
                segment.EndPoint + new Vector3(+segment.Thickness, -segment.Thickness, +segment.Thickness),
            }, Vector3.down, previousVertexCount
        );
        
        // North side
        AddTriangles(triangles, vertices.Count + previousVertexCount);
        vertices.Add(endingValues[0]);
        vertices.Add(endingValues[1]);
        vertices.Add(endingValues[2]);
        vertices.Add(endingValues[3]);
        
        
        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
    }
    
    
    private void Subdivide(List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, 
        bool flip, LineSegment segment, Vector3[] startingSegments, Vector3[] endingSegments, Vector3 normal, int previousVertexCount)
    {
        AddTriangles(triangles, vertices.Count + previousVertexCount);
        
        if (flip)
        {
            vertices.Add(startingSegments[1]);
            vertices.Add(startingSegments[0]);
        }
        else
        {
            vertices.Add(startingSegments[0]);
            vertices.Add(startingSegments[1]);
        }

        uvs.Add(new Vector2(0.0f, 0.0f));
        uvs.Add(new Vector2(0.0f, 1.0f));

        normals.Add(normal);
        normals.Add(normal);
        
        for (int i = 0; i < segment.SubDivisions; i++)
        {
            float t = (i + 1) / (float) segment.SubDivisions;
            Vector3 lowerDivision = Vector3.Lerp(
                startingSegments[0],
                endingSegments[0], 
                t
            );
            lowerDivision.y += ParabolaFunction(t, segment.Amplitude);
            
            Vector3 upperDivision = Vector3.Lerp(
                startingSegments[1], 
                endingSegments[1], 
                t);
            upperDivision.y += ParabolaFunction(t, segment.Amplitude);
            
            AddTriangles(triangles, vertices.Count + previousVertexCount);

            if (flip)
            {
                vertices.Add(lowerDivision);
                vertices.Add(upperDivision);
            }
            else
            {
                vertices.Add(lowerDivision);
                vertices.Add(upperDivision);
            }
            
            uvs.Add(new Vector2(t, 0.0f));
            uvs.Add(new Vector2(t, 1.0f));
    
            normals.Add(normal);
            normals.Add(normal);
        }
        
        vertices.Add(endingSegments[0]);
        vertices.Add(endingSegments[1]);

        
        uvs.Add(new Vector2(1.0f, 0.0f));
        uvs.Add(new Vector2(1.0f, 1.0f));
        
        normals.Add(normal);
        normals.Add(normal);
    }
    
    private float ParabolaFunction(float x, float minValue)
    {
        // https://i.imgur.com/2FcymbT.png
        float a = -4 * minValue;
        return a * ((x - 0.5f) * (x - 0.5f)) + minValue;
    }

    private class TriangleListHolder
    {
        public List<int> Triangles;

        public TriangleListHolder(List<int> triangles)
        {
            Triangles = triangles;
        }
    }
}
