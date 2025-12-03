using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class SplineEditor : MonoBehaviour
{
    public List<Transform> splinePoints = new();
    public int resolution;
    public float d = .001f;
    public float bridgeWidth = 2;
    public float bridgeHeight = 3;
    public float uv_scale = .1f;

    [Range(0, .5f)]
    public float AngleFallof;

    [Range(1, 25)]
    public int pillarCount;

    [Range(0, .5f)]
    public float pillarFillerArea = .3f;

    private MeshFilter meshFilter;
    private MeshRenderer Renderer;
    private MeshCollider col;

    public Material[] BridgeMats = new Material[2];

    float beginAngle;
    float endAngle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        Renderer = GetComponent<MeshRenderer>();

        if (Application.isPlaying)
        {
            GenerateSpline();
            RenderSpline();
        }
    }

    public Vector3 SamplePointInCurve(float t) 
    {
        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Explicit_definition

        Vector3 result = Vector3.zero;
        int n = splinePoints.Count - 1;

        for (int i = 0; i <= n; i++)
            result += CustomMath.nCr(n, i) * Mathf.Pow((1-t), n-i) * Mathf.Pow(t, i) * (splinePoints[i].position - transform.position);

        return result;
    }

    bool realtimeMeshUpdate = false;

    [ContextMenu("EDITOR/Create basic setup")]
    void CreateBasicSetup() 
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        GameObject[] splinepoints = new GameObject[3] { new(), new(), new() };
        string[] splinenames = new string[3] { "begin", "end", "a" };
        Vector3[] standardsplinepositions = new Vector3[3] { Vector3.zero, Vector3.right*16, Vector3.right * 18 };

        for (int i = 0; i < 3; i++)
        {
            splinepoints[i].name = splinenames[i];
            splinepoints[i].transform.parent = transform;
            splinepoints[i].transform.position = standardsplinepositions[i];
        }

        splinePoints = new List<Transform> { splinepoints[0].transform, splinepoints[2].transform, splinepoints[1].transform };
        resolution = 50;
        d = .001f;
        bridgeWidth = 2;
        bridgeHeight = 3.2f;
        pillarCount = 6;
        pillarFillerArea = .4f;
        uv_scale = 1;

        GenerateSpline();
        RenderSpline();
    }

    string alphabet = "abcdefghijklmnopqrstuvwxyz";

    [ContextMenu("EDITOR/Add spline point", false, 2)]
    void AddSplinePoint() 
    {
        GameObject newPoint = new();

        newPoint.name = alphabet[splinePoints.Count - 2] + "";
        newPoint.transform.parent = transform;
        newPoint.transform.position = .5f * (splinePoints[0].position + splinePoints[^1].position);

        splinePoints.Insert(splinePoints.Count - 1, newPoint.transform);
    }

    [ContextMenu("EDITOR/Toggle constant update", false, 1)]
    void FLipRealtimeMeshUpdate() 
    {
        realtimeMeshUpdate = !realtimeMeshUpdate;
        print("Constant mesh update is set to " + realtimeMeshUpdate + "!");
    }

    public float[] trTOta;
    float curveLength;
    void GenerateTrToTa() 
    {
        curveLength = 0;

        trTOta = new float[resolution];
        trTOta[0] = 0;

        int currentTaIndex = 1;

        for (float i = 0; i < resolution * 100; i++)
        {
            curveLength += Vector3.Magnitude(SamplePointInCurve(i/(resolution * 100)) - SamplePointInCurve((i + 1) / (resolution * 100)));
        }
        float tempLength = 0;
        float prevLength = 0;
        float targetLength = 1 / ((float)resolution + 1f);
        for (float i = 0; i < resolution * 100; i++)
        {
            if (currentTaIndex+1 == resolution)
            {
                trTOta[currentTaIndex] = 1;
                break;
            }

            prevLength = tempLength;
            tempLength += Vector3.Magnitude(SamplePointInCurve(i / (resolution * 100)) - SamplePointInCurve((i+1) / (resolution * 100)));
            if (Mathf.Abs(tempLength - targetLength * curveLength) < Mathf.Abs(prevLength - targetLength * curveLength))
                trTOta[currentTaIndex] = i / ((float)resolution * 100);
            else
            {
                currentTaIndex++;
                targetLength = currentTaIndex / ((float)resolution + 1f);
            }
        }
    }

    List<int> triangles = new();

    Vector3[] vertexOuput;
    Vector2[] texCoordOutput;

    [ContextMenu("EDITOR/Generate Spline")]
    void GenerateSpline() 
    {
        GenerateTrToTa();
        triangles = new();

        beginAngle = splinePoints[0 ].transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
        endAngle   = splinePoints[^1].transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        Vector2 texCoordOffset = Vector2.zero;

        List<Vector2> texCoord = new();
        List<Vector3> vertices = new();

        // Vertices
        for (float i = 0; i < resolution; i++)
        {
            float t = trTOta[Mathf.RoundToInt(i)];

            Vector3 p = SamplePointInCurve(t);
            Vector3 p_deriv = Vector3.Normalize(SamplePointInCurve(t + d) - p);

            Vector3 offset = new(p_deriv.z, 0, -p_deriv.x);
            if (t < AngleFallof)
                offset = Vector3.Slerp(new Vector3(Mathf.Cos(beginAngle), 0, Mathf.Sin(beginAngle)), offset, t / AngleFallof);
            if (t > AngleFallof && AngleFallof > 0)
                offset = Vector3.Slerp(offset, new Vector3(Mathf.Cos(endAngle), 0, Mathf.Sin(endAngle)), (t - 1 + AngleFallof) / AngleFallof);

            Vector3 _bridgeheight = Vector3.up * bridgeHeight * Mathf.Clamp(1 + pillarFillerArea - Mathf.Abs(Mathf.Sin(Mathf.PI * t * pillarCount)), 0, 1);

            vertices.Add(p + offset * bridgeWidth);
            vertices.Add(p - offset * bridgeWidth);
            vertices.Add(p + offset * bridgeWidth - _bridgeheight);
            vertices.Add(p - offset * bridgeWidth - _bridgeheight);

            if (i != 0) 
                texCoordOffset += Vector2.up * Vector3.Distance(vertices[^4], vertices[^8]) * uv_scale;

            texCoord.Add(texCoordOffset);
            texCoord.Add(texCoordOffset + Vector2.right * Vector3.Distance(vertices[^4], vertices[^3]) * uv_scale);
            texCoord.Add(texCoordOffset + Vector2.right * bridgeHeight * uv_scale);
            texCoord.Add(texCoordOffset + Vector2.right * Vector3.Distance(vertices[^4], vertices[^3]) * uv_scale + Vector2.right * bridgeHeight * uv_scale);
        }
        // Triangles
        for (int i = 0; i < resolution - 1; i++)
        {
            //Top part bridge
            triangles.Add(i * 4 + 1);
            triangles.Add(i * 4 + 4);
            triangles.Add(i * 4    );

            triangles.Add(i * 4 + 1);
            triangles.Add(i * 4 + 5);
            triangles.Add(i * 4 + 4);


            //Bottom part bridge
            triangles.Add(i * 4 + 2);
            triangles.Add(i * 4 + 6);
            triangles.Add(i * 4 + 3);

            triangles.Add(i * 4 + 6);
            triangles.Add(i * 4 + 7);
            triangles.Add(i * 4 + 3);

            //Switch to duplicate vertices
            i += resolution;
            //LSide part bridge
            triangles.Add(i * 4);
            triangles.Add(i * 4 + 4);
            triangles.Add(i * 4 + 2);

            triangles.Add(i * 4 + 4);
            triangles.Add(i * 4 + 6);
            triangles.Add(i * 4 + 2);


            //RSide part bridge
            triangles.Add(i * 4 + 3);
            triangles.Add(i * 4 + 5);
            triangles.Add(i * 4 + 1);

            triangles.Add(i * 4 + 3);
            triangles.Add(i * 4 + 7);
            triangles.Add(i * 4 + 5);
            i -= resolution;
        }
        triangles.Add(2);
        triangles.Add(1);
        triangles.Add(0);

        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(1);


        triangles.Add(vertices.Count-4);
        triangles.Add(vertices.Count-3);
        triangles.Add(vertices.Count-2);

        triangles.Add(vertices.Count-3);
        triangles.Add(vertices.Count-1);
        triangles.Add(vertices.Count-2);

        vertexOuput = new Vector3[vertices.Count * 2];
        texCoordOutput = new Vector2[texCoord.Count * 2];

        //Duplicate vertices (for surface normals)
        vertices.CopyTo(vertexOuput, 0);
        vertices.CopyTo(vertexOuput, vertices.Count);
        texCoord.CopyTo(texCoordOutput, 0);
        texCoord.CopyTo(texCoordOutput, texCoord.Count);

        col = (!col ? GetComponent<MeshCollider>() : col);
        Mesh colMesh = new();

        colMesh.vertices = vertexOuput;
        colMesh.triangles = triangles.ToArray();

        col.sharedMesh = colMesh;
    }
    public void RenderSpline() 
    {
        Mesh output = new();

        Mathf.Clamp(percentage_transparent, 0, 1);

        float amountTrianglesOpaque = triangles.Count * (1 - percentage_transparent);
        amountTrianglesOpaque = Mathf.Ceil(amountTrianglesOpaque / 3) * 3;

        int[] trianglesOpaque = new int[(int)amountTrianglesOpaque];
        int[] trianglesTransparent = new int[triangles.Count - (int)amountTrianglesOpaque];

        triangles.CopyTo(0, trianglesOpaque, 0, trianglesOpaque.Length);
        triangles.CopyTo(trianglesOpaque.Length, trianglesTransparent, 0, trianglesTransparent.Length);

        output.vertices = vertexOuput;
        output.uv = texCoordOutput;

        output.subMeshCount = 2;
        output.SetTriangles(trianglesOpaque, 0);
        output.SetTriangles(trianglesTransparent, 1);

        output.RecalculateNormals();
        output.RecalculateTangents();
        output.Optimize();

        Renderer = (!Renderer ? GetComponent<MeshRenderer>() : Renderer);
        meshFilter = (!meshFilter ? GetComponent<MeshFilter>() : meshFilter);

        meshFilter.mesh = output;
        Renderer.sharedMaterials = BridgeMats;
    }

    [Range(0, 1)]
    public float percentage_transparent = 0;
    float prevPercentage_transparent;

    // Update is called once per frame
    void Update()
    {
        if (splinePoints.Count == 0)
            return;

        for (int i = splinePoints.Count - 1; i >= 0; i--)
            if (!splinePoints[i])
                splinePoints.RemoveAt(i);

        if (realtimeMeshUpdate && !Application.isPlaying)
            GenerateSpline();
        if ((realtimeMeshUpdate && !Application.isPlaying) || (Application.isPlaying && percentage_transparent != prevPercentage_transparent))
            RenderSpline();

        prevPercentage_transparent = percentage_transparent;
    }
}
