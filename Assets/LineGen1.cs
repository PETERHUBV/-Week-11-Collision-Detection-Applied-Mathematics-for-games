using UnityEngine;

public class LineGen1 : MonoBehaviour
{
    public enum ShapeType
    {
        Cube,
        Pyramid,
        Cylinder,
        RectColumn,
        Sphere
    }

    public ShapeType shapeToDraw;

    public Material material;
    public float size = 1f;
    public Vector2 cubePos;
    public Vector2 cubePos2;
    public float zPos2;
    public float zPos;

    // Rotation
    public float rotX = 0f;
    public float rotY = 0f;
    public float rotZ = 0f;

    // Animation speeds
    public float rotXSpeed = 30f;
    public float rotYSpeed = 20f;
    public float rotZSpeed = 10f;

    public int segments = 12; // used for all shapes

    void Update()
    {
        // Animate rotations
        rotX += rotXSpeed * Time.deltaTime;
        rotY += rotYSpeed * Time.deltaTime;
        rotZ += rotZSpeed * Time.deltaTime;
    }

    void OnPostRender()
    {
        if (material == null) return;
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        material.SetPass(0);

        switch (shapeToDraw)
        {
            case ShapeType.Cube:
                DrawCube(Vector3.zero, size, segments);
                break;
            case ShapeType.Pyramid:
                DrawPyramid(Vector3.zero, size, segments);
                break;
            case ShapeType.Cylinder:
                DrawCylinder(Vector3.zero, size, size * 2f, segments);
                break;
            case ShapeType.RectColumn:
                DrawRectColumn(Vector3.zero, size, size * 2f, size, segments);
                break;
            case ShapeType.Sphere:
                DrawSphere(Vector3.zero, size, segments);
                break;
        }

        GL.End();
        GL.PopMatrix();
    }

    // ---------------- Rotation & Projection ----------------

    Vector3 Rotate3D(Vector3 p, float rotX, float rotY)
    {
        float rz = rotZ * Mathf.Deg2Rad;
        float rx = rotX * Mathf.Deg2Rad;
        float ry = rotY * Mathf.Deg2Rad;

        // Z rotation
        float x1 = p.x * Mathf.Cos(rz) - p.y * Mathf.Sin(rz);
        float y1 = p.x * Mathf.Sin(rz) + p.y * Mathf.Cos(rz);
        p.x = x1; p.y = y1;

        // X rotation
        float y2 = p.y * Mathf.Cos(rx) - p.z * Mathf.Sin(rx);
        float z2 = p.y * Mathf.Sin(rx) + p.z * Mathf.Cos(rx);
        p.y = y2; p.z = z2;

        // Y rotation
        float x3 = p.x * Mathf.Cos(ry) + p.z * Mathf.Sin(ry);
        float z3 = p.z * Mathf.Cos(ry) - p.x * Mathf.Sin(ry);
        p.x = x3; p.z = z3;

        return p;
    }

    Vector2 Project3D(Vector3 p)
    {
        float perspective = PerspectiveCamera.Instance.GetPerspective(p.z);
        return new Vector2(p.x * perspective, p.y * perspective);
    }

    
 
    void DrawCube(Vector3 center, float size, int segments)
    {
        float step = size / segments;
        float half = size * 0.5f;

        for (int x = 0; x <= segments; x++)
        {
            for (int y = 0; y <= segments; y++)
            {
                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-half + x * step, -half + y * step, -half), rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-half + x * step, -half + y * step, half), rotX, rotY)));

                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-half + x * step, -half, -half + y * step), rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-half + x * step, half, -half + y * step), rotX, rotY)));

                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-half, -half + x * step, -half + y * step), rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(center + new Vector3(half, -half + x * step, -half + y * step), rotX, rotY)));
            }
        }
    }

    void DrawPyramid(Vector3 center, float size, int segments)
    {
        Vector3 top = center + Vector3.up * size;
        for (int i = 0; i < segments; i++)
        {
            float angle0 = 2 * Mathf.PI * i / segments;
            float angle1 = 2 * Mathf.PI * (i + 1) / segments;
            Vector3 p0 = center + new Vector3(Mathf.Cos(angle0) * size, 0, Mathf.Sin(angle0) * size);
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * size, 0, Mathf.Sin(angle1) * size);

            GL.Vertex(Project3D(Rotate3D(p0, rotX, rotY)));
            GL.Vertex(Project3D(Rotate3D(p1, rotX, rotY)));

            GL.Vertex(Project3D(Rotate3D(top, rotX, rotY)));
            GL.Vertex(Project3D(Rotate3D(p0, rotX, rotY)));
        }
    }

    void DrawRectColumn(Vector3 center, float width, float height, float depth, int segments)
    {
        float stepX = width / segments;
        float stepY = height / segments;
        float stepZ = depth / segments;
        float halfW = width * 0.5f, halfH = height * 0.5f, halfD = depth * 0.5f;

        for (int x = 0; x <= segments; x++)
        {
            for (int y = 0; y <= segments; y++)
            {
                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-halfW + x * stepX, -halfH + y * stepY, -halfD), rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-halfW + x * stepX, -halfH + y * stepY, halfD), rotX, rotY)));

                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-halfW + x * stepX, -halfH, -halfD + y * stepZ), rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-halfW + x * stepX, halfH, -halfD + y * stepZ), rotX, rotY)));

                GL.Vertex(Project3D(Rotate3D(center + new Vector3(-halfW, -halfH + x * stepX, -halfD + y * stepZ), rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(center + new Vector3(halfW, -halfH + x * stepX, -halfD + y * stepZ), rotX, rotY)));
            }
        }
    }

    void DrawCylinder(Vector3 center, float radius, float height, int segments)
    {
        Vector3[] bottom = new Vector3[segments];
        Vector3[] top = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            bottom[i] = center + new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
            top[i] = bottom[i] + Vector3.up * height;
        }

        for (int i = 0; i < segments; i++)
        {
            int n = (i + 1) % segments;
            GL.Vertex(Project3D(Rotate3D(bottom[i], rotX, rotY)));
            GL.Vertex(Project3D(Rotate3D(bottom[n], rotX, rotY)));

            GL.Vertex(Project3D(Rotate3D(top[i], rotX, rotY)));
            GL.Vertex(Project3D(Rotate3D(top[n], rotX, rotY)));

            GL.Vertex(Project3D(Rotate3D(bottom[i], rotX, rotY)));
            GL.Vertex(Project3D(Rotate3D(top[i], rotX, rotY)));
        }
    }

    void DrawSphere(Vector3 center, float radius, int segments)
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

                GL.Vertex(Project3D(Rotate3D(p00, rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(p01, rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(p00, rotX, rotY)));
                GL.Vertex(Project3D(Rotate3D(p10, rotX, rotY)));
            }
        }
    }
}