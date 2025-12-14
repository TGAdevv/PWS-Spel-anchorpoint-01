using System.Collections.Generic;
using UnityEngine;

public class LevelImporter : MonoBehaviour
{
    public string[] levels;

    public Transform IslandsParent;
    public Transform BridgesParent;

    public Material[] BridgeMats = new Material[2];

    List<Transform> Islands = new();
    public List<SplineEditor> Bridges = new();

    public float LevelScaleMod = 3;
    public float DistancePerPillar;
    public float DistancePerSample;

    float relativeLevelScaleMod = 0;
    Vector2 screenRes;

    public GameObject[] IslandTiles;
    public AnchorToPosition anchorToPosition;

    Vector3 mapCenter = new(0,0,0);

    public Transform CameraCanvas;
    public GameObject PriceUIPrefab;

    [Header("FOR EDITOR LEAVE NULL IN GAME SCENE")]
    public GameObject axisPrefab;

    private void Start()
    {
        if (!axisPrefab)
            ImportLevel(0);
    }

    void CreateIsland(Vector2 pos, Vector2Int size) 
    {
        GameObject island = new("island " + pos.ToString());
        island.transform.position = new Vector3(pos.x, 0, pos.y) * relativeLevelScaleMod;
        island.transform.parent = IslandsParent;

        mapCenter += island.transform.position;

        float unitSize = .3f * (screenRes.x / screenRes.y) * relativeLevelScaleMod;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            { 
                GameObject newTile = Instantiate(
                    //TEMP! Should not be 0 (but the tile actually needed)
                    IslandTiles[Mathf.RoundToInt(Random.Range(0.0f, 1.0f))], 
                    island.transform.position + new Vector3(i - (size.x - 1) * .5f, 0, j - (size.y - 1) * .5f) * unitSize, 
                    Quaternion.identity, 
                    island.transform);
                newTile.transform.localScale *= unitSize;
            }
        }

        Islands.Add(island.transform);
    }
    void CreateBridge(Vector3[] splinePoints, float weight, Vector2Int index, float[] weights = null)
    {
        float unitSize = .3f * (screenRes.x / screenRes.y) * relativeLevelScaleMod;

        string weightOptionsTXT = "";
        foreach (var item in weights)
        {
            weightOptionsTXT += item + ", ";
        }

        GameObject newBridge = new("Bridge " + index.x + ", " + index.y + ((weights.Length > 0) ? (" Weight options are: " + weightOptionsTXT) : ""));
        newBridge.transform.parent = BridgesParent;
        newBridge.layer = LayerMask.NameToLayer("Bridge");
        newBridge.AddComponent<MeshCollider>();
        newBridge.AddComponent<MeshFilter>();
        newBridge.AddComponent<MeshRenderer>();

        float splineDistance = Vector3.Distance(splinePoints[0], splinePoints[^1]);

        SplineEditor newSpline = newBridge.AddComponent<SplineEditor>();
        newSpline.BridgeMats  = BridgeMats;
        newSpline.resolution  = Mathf.CeilToInt(splineDistance  / DistancePerSample);
        newSpline.pillarCount = Mathf.RoundToInt(splineDistance / DistancePerPillar);
        newSpline.bridgeWidth = unitSize * .15f;
        newSpline.bridgeHeight = unitSize * .25f;
        if (axisPrefab)
            newSpline.percentage_transparent = 0;

        List<Transform> splineTransforms = new();

        for (int i = 0; i < splinePoints.Length; i++)
        {
            GameObject newSplinePoint = new("P" + i);
            newSplinePoint.transform.position = splinePoints[i];
            newSplinePoint.transform.parent   = newBridge.transform;
            splineTransforms.Add(newSplinePoint.transform);

            if (i != 0 && i != splinePoints.Length-1 && axisPrefab)
            {
                GameObject AxisOBJ = Instantiate(axisPrefab, newSplinePoint.transform);
                AxisOBJ.GetComponent<AxisScript>().host = newSpline;
            }
        }

        newSpline.splinePoints = splineTransforms;

        if (!axisPrefab)
        {
            ClickToCreateBridge clickToCreateBridge = newBridge.AddComponent<ClickToCreateBridge>();
            clickToCreateBridge.price = (uint)weight;
            clickToCreateBridge.MenuPricePrefab = PriceUIPrefab;
            clickToCreateBridge.CameraCanvas = CameraCanvas;
        }

        Bridges.Add(newSpline);
    }

    public void ImportLevel(int levelID, bool PreviewMode=false)
    {
        transform.position = Vector3.zero;

        //First wipe everything from the current scene
        for (int i = 0; i < CameraCanvas.childCount; i++)
            Destroy(CameraCanvas.GetChild(i).gameObject);
        foreach (var island in Islands)
            Destroy(island.gameObject);
        foreach (var bridge in Bridges)
            Destroy(bridge.gameObject);
        Islands = new();
        Bridges = new();

        string level = levels[levelID];

        string[] islands = level.Split("_")[0].Split(";");
        string[] Allconnections = level.Split("_")[1].Split("/");

        string[] screenResComponents = level.Split("_")[2].Split(",");
        screenRes = new(int.Parse(screenResComponents[0]), int.Parse(screenResComponents[1]));

        relativeLevelScaleMod = LevelScaleMod * (1000 / screenRes.x);

        for (int i = 0; i < islands.Length; i++)
        {
            string[] islandParams = islands[i].Split(",");

            if (islandParams.Length < 4)
                continue;

            Vector2 pos     = new(float.Parse(islandParams[0]), float.Parse(islandParams[1]));
            Vector2Int size = new(int.Parse(islandParams[2]),   int.Parse(islandParams[3]));

            CreateIsland(pos, size);
        }

        for (int i = 0; i < Allconnections.Length; i++)
        {
            string[] connections = Allconnections[i].Split(";");

            for (int j = 0; j < connections.Length; j++)
            {
                string[] connectionParams = connections[j].Split(",");
                if (connectionParams.Length < 6)
                    continue;

                Vector3[] splinePoints = new Vector3[Mathf.FloorToInt((connectionParams.Length - 2)/3f)];
                for (int k = 0; k < splinePoints.Length; k++)
                    splinePoints[k]  = new Vector3(float.Parse(connectionParams[k * 3 + 2]), float.Parse(connectionParams[k * 3 + 3]), float.Parse(connectionParams[k * 3 + 4])) * (PreviewMode ? relativeLevelScaleMod : 1);
                float weight = 1;
                float[] weights = new float[0];
                if (connectionParams[0].Contains("?"))
                {
                    string[] weightsText = connectionParams[0].Split("?");
                    weights = new float[weightsText.Length];
                    for (int k = 0; k < weightsText.Length; k++)
                        weights[k] = float.Parse(weightsText[k]);
                }
                else
                    weight = float.Parse(connectionParams[0]);

                CreateBridge(splinePoints, weight, new(i, j), weights);
            }
        }

        if (!axisPrefab)
            transform.position = -mapCenter / islands.Length;
    }
}
