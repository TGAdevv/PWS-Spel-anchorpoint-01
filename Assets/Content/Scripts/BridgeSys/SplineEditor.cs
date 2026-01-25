using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[ExecuteAlways]
public class SplineEditor : MonoBehaviour
{
    public List<Transform> splinePoints = new();
    public int resolution;
    public float d = .001f;
    public float bridgeWidth = .5f;
    public float bridgeHeight = 2;
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

    bool TestForOverflow(int iteration, [CallerLineNumber] int codeLine = 0)
    {
        if (iteration > 10000)
        {
            string ErrorLink = "<a href=\"Assets/Content/Scripts/BridgeSys/SplineEditor.cs\" line=\"" + codeLine + "\">Custom Error 0.2</a>";
            Debug.LogWarning(ErrorLink + ": Exceeded max iteration count of 10000");
        }
        return (iteration > 10000);
    }

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
    readonly string alphabet = "abcdefghijklmnopqrstuvwxyz";

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

        for (float i = 0; i < resolution * 2; i++)
        {
            if (TestForOverflow((int)i)) return;
            curveLength += Vector3.Magnitude(SamplePointInCurve(i/(resolution * 2)) - SamplePointInCurve((i + 1) / (resolution * 2)));
        }
        float tempLength = 0;
        float prevLength = 0;
        float targetLength = 1 / ((float)resolution + 1f);
        for (float i = 0; i < resolution * 3; i++)
        {
            if (TestForOverflow((int)i)) return;

            if (currentTaIndex+1 == resolution)
            {
                trTOta[currentTaIndex] = 1;
                break;
            }

            prevLength = tempLength;
            tempLength += Vector3.Magnitude(SamplePointInCurve(i / (resolution * 3)) - SamplePointInCurve((i+1) / (resolution * 3)));
            if (Mathf.Abs(tempLength - targetLength * curveLength) < Mathf.Abs(prevLength - targetLength * curveLength))
                trTOta[currentTaIndex] = i / ((float)resolution * 3 - 1);
            else
            {
                if (currentTaIndex > 0)
                    if (trTOta[currentTaIndex] == 0 && trTOta[currentTaIndex-1] > 0)
                        trTOta[currentTaIndex] = trTOta[currentTaIndex - 1];

                currentTaIndex++;
                targetLength = currentTaIndex / ((float)resolution + 1f);
            }
        }
    }

    List<int> triangles = new();

    Vector3[] vertexOuput;
    Vector2[] texCoordOutput;

    [ContextMenu("EDITOR/Generate Spline")]
    public void GenerateSpline() 
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
            if (TestForOverflow((int)i)) return;
            float t = trTOta[Mathf.RoundToInt(i)];

            Vector3 p = SamplePointInCurve(t);
            Vector3 p_deriv = Vector3.Normalize(SamplePointInCurve(t + d) - p);

            Vector3 offset = new(p_deriv.z, 0, -p_deriv.x);
            if (t < AngleFallof)
                offset = Vector3.Slerp(new Vector3(Mathf.Cos(beginAngle), 0, Mathf.Sin(beginAngle)), offset, t / AngleFallof);
            if (t > AngleFallof && AngleFallof > 0)
                offset = Vector3.Slerp(offset, new Vector3(Mathf.Cos(endAngle), 0, Mathf.Sin(endAngle)), (t - 1 + AngleFallof) / AngleFallof);

            Vector3 _bridgeheight = bridgeHeight * Mathf.Clamp(1 + pillarFillerArea - Mathf.Abs(Mathf.Sin(Mathf.PI * t * pillarCount)), 0, 1) * Vector3.up;

            vertices.Add(p + offset * bridgeWidth);
            vertices.Add(p - offset * bridgeWidth);
            vertices.Add(p + offset * bridgeWidth - _bridgeheight);
            vertices.Add(p - offset * bridgeWidth - _bridgeheight);

            if (i != 0) 
                texCoordOffset += uv_scale * Vector3.Distance(vertices[^4], vertices[^8]) * Vector2.up;

            texCoord.Add(texCoordOffset);
            texCoord.Add(texCoordOffset + uv_scale * Vector3.Distance(vertices[^4], vertices[^3]) * Vector2.right);
            texCoord.Add(texCoordOffset + bridgeHeight * uv_scale * Vector2.right);
            texCoord.Add(texCoordOffset + uv_scale * Vector3.Distance(vertices[^4], vertices[^3]) * Vector2.right + bridgeHeight * uv_scale * Vector2.right);
        }
        // Triangles
        for (int i = 0; i < resolution - 1; i++)
        {
            if (TestForOverflow((int)i)) return;
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

    [ContextMenu("EDITOR/Render Spline")]
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
        {
            if (TestForOverflow(i)) return;
            if (!splinePoints[i])
                splinePoints.RemoveAt(i);
        }

        if (realtimeMeshUpdate && !Application.isPlaying)
            GenerateSpline();
        if ((realtimeMeshUpdate && !Application.isPlaying) || (Application.isPlaying && percentage_transparent != prevPercentage_transparent))
            RenderSpline();

        prevPercentage_transparent = percentage_transparent;
    }
}
