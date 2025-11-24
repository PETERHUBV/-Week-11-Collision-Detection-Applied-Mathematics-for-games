using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;


public class LineGen : MonoBehaviour
{

    public enum ShapeType
    {
        Cube,
        Pyramid,
        Cylinder,
        RectColumn,
        Sphere
    }

    public ShapeType shapeToDraw; // Choose which shape to draw
    public Material material;

    public float cubeSize = 1f;
    public Vector2 cubePos;
    public Vector2 cubePos2;
    public float zPos = 0f;
    public float zPos2 = 0f;
    public float shapeScale = 1f;
    public int segments = 12; //   Cylinder, Sphere, Pyramid 

    private void OnPostRender()
    {
        DrawLine();
    }

    public void DrawLine()
    {
        if (material == null)
        {
            Debug.LogError("You need to add a material");
            return;
        }

        GL.PushMatrix();
        GL.Begin(GL.LINES);
        material.SetPass(0);

    
        if (shapeToDraw == ShapeType.Cube)
        {
            DrawCube(cubePos, cubeSize * shapeScale);
        }
        else if (shapeToDraw == ShapeType.Pyramid)
        {
            DrawPyramid(new Vector3(0, 0, 3), 1f * shapeScale, segments);
        }
        else if (shapeToDraw == ShapeType.Cylinder)
        {
            DrawCylinder(new Vector3(0, 0, 3), 1f * shapeScale, 2f * shapeScale, segments);
        }
        else if (shapeToDraw == ShapeType.RectColumn)
        {
            DrawRectColumn(new Vector3(0, 0, 3), 1f * shapeScale, 2f * shapeScale, 1f * shapeScale, segments);
        }
        else if (shapeToDraw == ShapeType.Sphere)
        {
            DrawSphere(new Vector3(0, 0, 3), 1f * shapeScale, segments);
        }

        GL.End();
        GL.PopMatrix();
    }

    // 

    void DrawCube(Vector2 pos, float size)
    {
        Vector2[] square = GetCube(pos);
        float frontZ = PerspectiveCamera.Instance.GetPerspective(zPos + size * 0.5f);
        float backZ = PerspectiveCamera.Instance.GetPerspective(zPos - size * 0.5f);

        Vector2[] computedFront = RenderSquare(square, frontZ);
        Vector2[] computedBack = RenderSquare(square, backZ);

        for (int i = 0; i < 4; i++)
        {
            GL.Vertex(computedFront[i]);
            GL.Vertex(computedBack[i]);
        }
    }

    public Vector2[] GetCube(Vector2 pos)
    {
        Vector2[] faceArray = new Vector2[]
        {
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(-1, -1),
            new Vector2(1, -1)
        };

        for (int i = 0; i < faceArray.Length; i++)
        {
            faceArray[i] = (faceArray[i] + pos) * cubeSize;
        }

        return faceArray;
    }

    Vector2[] RenderSquare(Vector2[] square, float perspective)
    {
        Vector2[] computed = new Vector2[square.Length];
        for (int i = 0; i < square.Length; i++)
        {
            computed[i] = square[i] * perspective;
            GL.Vertex(square[i] * perspective);
            GL.Vertex(square[(i + 1) % square.Length] * perspective);
        }
        return computed;
    }

    Vector2 Project3D(Vector3 p)
    {
        float perspective = PerspectiveCamera.Instance.GetPerspective(p.z);
        return new Vector2(p.x * perspective, p.y * perspective);
    }

    public void DrawPyramid(Vector3 center, float size, int segments)
    {
        Vector3 top = center + new Vector3(0, size, 0);

        
        for (int i = 0; i < 4; i++)
        {
            Vector3 a = center + new Vector3(size * (i % 2 == 0 ? -1 : 1), 0, size * (i < 2 ? -1 : 1));
            Vector3 b = center + new Vector3(size * ((i + 1) % 2 == 0 ? -1 : 1), 0, size * ((i + 1) < 2 ? -1 : 1));

            GL.Vertex(Project3D(a));
            GL.Vertex(Project3D(b));

            GL.Vertex(Project3D(top));
            GL.Vertex(Project3D(a));
        }
    }

    public void DrawRectColumn(Vector3 center, float width, float height, float depth, int segments)
    {
        Vector3[] corners = new Vector3[8];
        float w = width * 0.5f, h = height * 0.5f, d = depth * 0.5f;

        int i = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    corners[i++] = center + new Vector3(x * w, y * h, z * d);

        int[,] edges =
        {
            {0,1},{1,3},{3,2},{2,0},
            {4,5},{5,7},{7,6},{6,4},
            {0,4},{1,5},{2,6},{3,7}
        };

        for (i = 0; i < edges.GetLength(0); i++)
        {
            GL.Vertex(Project3D(corners[edges[i, 0]]));
            GL.Vertex(Project3D(corners[edges[i, 1]]));
        }
    }

    public void DrawCylinder(Vector3 center, float radius, float height, int segments)
    {
        Vector3[] bottom = new Vector3[segments];
        Vector3[] top = new Vector3[segments];

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            bottom[i] = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            top[i] = bottom[i] + new Vector3(0, height, 0);
        }

        for (int i = 0; i < segments; i++)
        {
            int n = (i + 1) % segments;
            GL.Vertex(Project3D(bottom[i])); GL.Vertex(Project3D(bottom[n]));
            GL.Vertex(Project3D(top[i])); GL.Vertex(Project3D(top[n]));
            GL.Vertex(Project3D(bottom[i])); GL.Vertex(Project3D(top[i]));
        }
    }

    public void DrawSphere(Vector3 center, float radius, int segments)
    {
        for (int lat = 0; lat < segments; lat++)
        {
            float a0 = Mathf.PI * lat / segments;
            float a1 = Mathf.PI * (lat + 1) / segments;

            for (int lon = 0; lon < segments; lon++)
            {
                float b0 = 2 * Mathf.PI * lon / segments;
                float b1 = 2 * Mathf.PI * (lon + 1) / segments;

                Vector3 p00 = center + new Vector3(radius * Mathf.Sin(a0) * Mathf.Cos(b0), radius * Mathf.Cos(a0), radius * Mathf.Sin(a0) * Mathf.Sin(b0));
                Vector3 p01 = center + new Vector3(radius * Mathf.Sin(a0) * Mathf.Cos(b1), radius * Mathf.Cos(a0), radius * Mathf.Sin(a0) * Mathf.Sin(b1));
                Vector3 p10 = center + new Vector3(radius * Mathf.Sin(a1) * Mathf.Cos(b0), radius * Mathf.Cos(a1), radius * Mathf.Sin(a1) * Mathf.Sin(b0));

                GL.Vertex(Project3D(p00)); GL.Vertex(Project3D(p01));
                GL.Vertex(Project3D(p00)); GL.Vertex(Project3D(p10));
            }
        }
    }
}