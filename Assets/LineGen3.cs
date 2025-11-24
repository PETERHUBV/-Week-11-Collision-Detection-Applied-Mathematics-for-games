using UnityEngine;

public class LineGen3 : MonoBehaviour
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

    public Material shapeMaterial;
    public Material platformMaterial;

    public float shapeSize = 1f;
    public int segments = 12;

    public float platformWidth = 8f;
    public float platformHeight = 0.5f;
    public float platformDepth = 5f;

    // Motion math
    private Vector3 shapePos = new Vector3(0, 5f, 0);
    private float velocityY = 0f;
    public float gravity = -9.8f;
    public float jumpStrength = 5f;
    private bool onPlatform = false;

    // Rotation
    public float rotX = 0f;
    public float rotY = 0f;
    public float zRot = 0f;
    public float rotXSpeed = 30f;
    public float rotYSpeed = 20f;
    public float rotZSpeed = 10f;

    // Colors
    private Color currentShapeColor;
    private Color currentPlatformColor;

    void Start()
    {
        currentShapeColor = Color.red;   // default in-air color
        currentPlatformColor = Color.white;
    }

    void Update()
    {
        // Rotation
        rotX += rotXSpeed * Time.deltaTime;
        rotY += rotYSpeed * Time.deltaTime;
        zRot += rotZSpeed * Time.deltaTime;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && onPlatform)
        {
            velocityY = jumpStrength;
            onPlatform = false;
        }

        // Fall 
        if (!onPlatform)
        {
            velocityY += gravity * Time.deltaTime;
            shapePos.y += velocityY * Time.deltaTime;
        }

        // Collision math
        float platformTop = platformHeight / 2f;
        float shapeBottom = shapePos.y - shapeSize / 2f;

        if (shapeBottom <= platformTop)
        {
            shapePos.y = platformTop + shapeSize / 2f;
            velocityY = 0f;
            onPlatform = true;

            // Random colors collision
            currentShapeColor = new Color(Random.value, Random.value, Random.value);
            currentPlatformColor = new Color(Random.value, Random.value, Random.value);
        }
    }

    void OnPostRender()
    {
        if (!shapeMaterial || !platformMaterial) return;

        GL.PushMatrix();
        GL.Begin(GL.LINES);

        // Platform
        platformMaterial.SetColor("_Color", currentPlatformColor);
        platformMaterial.SetPass(0);

        DrawRectColumn(Vector3.zero, platformWidth, platformHeight, platformDepth, segments);

        // Shape
        shapeMaterial.SetColor("_Color", currentShapeColor);
        shapeMaterial.SetPass(0);

        switch (shapeToDraw)
        {
            case ShapeType.Cube:
                DrawCube(shapePos, shapeSize);
                break;
            case ShapeType.Pyramid:
                DrawPyramid(shapePos, shapeSize, segments);
                break;
            case ShapeType.Cylinder:
                DrawCylinder(shapePos, shapeSize, shapeSize * 2f, segments);
                break;
            case ShapeType.RectColumn:
                DrawRectColumn(shapePos, shapeSize, shapeSize * 2f, shapeSize, segments);
                break;
            case ShapeType.Sphere:
                DrawSphere(shapePos, shapeSize, segments);
                break;
        }

        GL.End();
        GL.PopMatrix();
    }

    
    // Rotation
    Vector3 Rotate3D(Vector3 p, float rotX, float rotY)
    {
        float rz = zRot * Mathf.Deg2Rad;
        float rx = rotX * Mathf.Deg2Rad;
        float ry = rotY * Mathf.Deg2Rad;

        // Z
        float x1 = p.x * Mathf.Cos(rz) - p.y * Mathf.Sin(rz);
        float y1 = p.x * Mathf.Sin(rz) + p.y * Mathf.Cos(rz);
        p.x = x1; p.y = y1;

        // X
        float y2 = p.y * Mathf.Cos(rx) - p.z * Mathf.Sin(rx);
        float z2 = p.y * Mathf.Sin(rx) + p.z * Mathf.Cos(rx);
        p.y = y2; p.z = z2;

        // Y
        float x3 = p.x * Mathf.Cos(ry) + p.z * Mathf.Sin(ry);
        float z3 = p.z * Mathf.Cos(ry) - p.x * Mathf.Sin(ry);
        p.x = x3; p.z = z3;

        return p;
    }

  
    // Projection

    Vector2 Project3D(Vector3 p)
    {
        float perspective = 1f / (1f + p.z * 0.1f);
        return new Vector2(p.x * perspective, p.y * perspective);
    }

  
    // Drawing shapes 
  
    void DrawCube(Vector3 center, float size)
    {
        float s = size / 2f;
        Vector3[] corners = {
            new Vector3(-s,-s,-s), new Vector3(s,-s,-s), new Vector3(s,s,-s), new Vector3(-s,s,-s),
            new Vector3(-s,-s,s), new Vector3(s,-s,s), new Vector3(s,s,s), new Vector3(-s,s,s)
        };
        int[,] edges = {
            {0,1},{1,2},{2,3},{3,0},
            {4,5},{5,6},{6,7},{7,4},
            {0,4},{1,5},{2,6},{3,7}
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            GL.Vertex(Project3D(Rotate3D(corners[edges[i, 0]] + center, rotX, rotY)));
            GL.Vertex(Project3D(Rotate3D(corners[edges[i, 1]] + center, rotX, rotY)));
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
        Vector3[] corners = {
            new Vector3(-width/2,-height/2,-depth/2), new Vector3(width/2,-height/2,-depth/2),
            new Vector3(width/2,height/2,-depth/2), new Vector3(-width/2,height/2,-depth/2),
            new Vector3(-width/2,-height/2,depth/2), new Vector3(width/2,-height/2,depth/2),
            new Vector3(width/2,height/2,depth/2), new Vector3(-width/2,height/2,depth/2)
        };
        int[,] edges = {
            {0,1},{1,2},{2,3},{3,0},
            {4,5},{5,6},{6,7},{7,4},
            {0,4},{1,5},{2,6},{3,7}
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            GL.Vertex(Project3D(Rotate3D(corners[edges[i, 0]] + center, rotX, rotY)));
            GL.Vertex(Project3D(Rotate3D(corners[edges[i, 1]] + center, rotX, rotY)));
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