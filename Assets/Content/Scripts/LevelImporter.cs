using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

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

    Vector3 mapCenter = new(0,0,0);

    public Transform CameraCanvas;
    public GameObject MenuPricePrefab;
    public GameObject MenuPriceVarPrefab;

    public GameObject LoadScreenUI;

    public int currentLevel = 0;

    // New fields for start and end islands, initialized to -1 (undefined)
    public int startIsland = -1;
    public int endIsland = -1;

    public UnityEvent OnImport;

    [Header("FOR EDITOR LEAVE NULL IN GAME SCENE")]
    public GameObject axisPrefab;

    private void Start()
    {
        if (!axisPrefab)
            ImportLevel(currentLevel);
    }

    bool TestForOverflow(int iteration, [CallerLineNumber] int codeLine = 0)
    {
        if (iteration > 10000)
        {
            string ErrorLink = "<a href=\"Assets/Content/Scripts/LevelImporter.cs\" line=\"" + codeLine + "\">Custom Error 0.1</a>";
            Debug.LogWarning(ErrorLink + ": Exceeded max iteration count of 10000");
        }
        return (iteration > 10000);
    }

    public void ImportNextLevel()
    {
        currentLevel++;
        if (currentLevel >= levels.Length)
        {
            currentLevel--;
            Debug.LogWarning("Tried to load next level on last level");
            return;
        }
        ImportLevel(currentLevel);
    }

    GameObject instantiateIsland(GameObject island, int i, int j, Vector2Int size, float unitSize)
    {
        return Instantiate(
                    //TEMP! Should not be 0 (but the tile actually needed)
                    IslandTiles[Mathf.RoundToInt(Random.Range(0.0f, 4.0f))],
                    island.transform.position + new Vector3(i - (size.x - 1) * .5f, 0, j - (size.y - 1) * .5f) * unitSize,
                    Quaternion.identity,
                    island.transform);
    }

    bool finished = false;

    IEnumerator CreateIsland(Vector2 pos, Vector2Int size)
    {
        GameObject island = new("island " + pos.ToString());
        island.transform.position = new Vector3(pos.x, 0, pos.y) * relativeLevelScaleMod;
        island.transform.parent = IslandsParent;
        mapCenter += island.transform.position;

        float unitSize = .3f * (screenRes.x / screenRes.y) * relativeLevelScaleMod;

        int curIndex = 0;
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                GameObject newTile = instantiateIsland(island, i, j, size, unitSize);
                newTile.transform.localScale *= unitSize;

                if (curIndex % 3 == 0)
                    yield return new WaitForFixedUpdate();
                curIndex++;
            }
        }

        Islands.Add(island.transform);
        finished = true;
    }
    void CreateBridge(Vector3[] splinePoints, uint[] weight, Vector2Int index)
    {
        float unitSize = .3f * (screenRes.x / screenRes.y) * relativeLevelScaleMod;

        string weightOptionsTXT = "";
        foreach (var item in weight)
        {
            weightOptionsTXT += item + ", ";
        }

        GameObject newBridge = new("Bridge " + index.x + ", " + index.y + ((weight.Length > 0) ? (" Weight options are: " + weightOptionsTXT) : ""));
        newBridge.transform.parent = BridgesParent;
        newBridge.layer = LayerMask.NameToLayer("Bridge");
        newBridge.AddComponent<MeshCollider>();
        newBridge.AddComponent<MeshFilter>();
        newBridge.AddComponent<MeshRenderer>();

        float splineDistance = Vector3.Distance(splinePoints[0], splinePoints[^1]);

        SplineEditor newSpline = newBridge.AddComponent<SplineEditor>();
        newSpline.BridgeMats  = BridgeMats;
        newSpline.resolution  = Mathf.Clamp(Mathf.CeilToInt(splineDistance  / DistancePerSample), 2, 1000);
        newSpline.pillarCount = Mathf.RoundToInt(splineDistance / DistancePerPillar);
        newSpline.bridgeWidth = unitSize * .15f;
        newSpline.bridgeHeight = unitSize * .25f;
        if (axisPrefab)
            newSpline.percentage_transparent = 0;

        List<Transform> splineTransforms = new();

        for (int i = 0; i < splinePoints.Length; i++)
        {
            GameObject newSplinePoint = new("P" + i);
            newSplinePoint.transform.position = splinePoints[i] + transform.position;
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
            clickToCreateBridge.price = weight;
            clickToCreateBridge.MenuPricePrefab = MenuPricePrefab;
            clickToCreateBridge.MenuPriceVarPrefab = MenuPriceVarPrefab;
            clickToCreateBridge.CameraCanvas = CameraCanvas;
        }

        Bridges.Add(newSpline);
    }

    int LevelId = 0;
    bool PreviewMode = false;

    IEnumerator importLevel()
    {
        CultureInfo US = new("en-US");
        Thread.CurrentThread.CurrentCulture = US;
        transform.position = Vector3.zero;
        mapCenter = Vector3.zero;

        //First wipe everything from the current scene
        for (int i = 0; i < CameraCanvas.childCount; i++)
            Destroy(CameraCanvas.GetChild(i).gameObject);
        foreach (var island in Islands)
            Destroy(island.gameObject);
        foreach (var bridge in Bridges)
            Destroy(bridge.gameObject);
        Islands = new();
        Bridges = new();

        string level = levels[LevelId];

        // Split the level string into parts by '_'
        string[] parts = level.Split('_');

        // Parse islands: first part, split by ';'
        string[] islands = parts[0].Split(";");

        // Parse connections: second part, split by '/'
        string[] Allconnections = parts[1].Split("/");

        // Parse screen resolution: third part, split by ','
        string[] screenResComponents = parts[2].Split(",");
        screenRes = new(int.Parse(screenResComponents[0]), int.Parse(screenResComponents[1]));

        relativeLevelScaleMod = LevelScaleMod * (1000 / screenRes.x);

        // Parse start and end islands: fourth part starts with '}', then optional 'start,end'
        string possibleStartEndPart = parts[3];
        if (possibleStartEndPart.StartsWith("}")) {
            string startEnd = possibleStartEndPart.Substring(1); // Remove the '}'
            if (!string.IsNullOrEmpty(startEnd))
            {
                string[] startEndParts = startEnd.Split(",");
                startIsland = int.Parse(startEndParts[0].Trim());
                //endisland needs to be set carefully, as there could be trailing whitespace that disturbs parsing
                if (!int.TryParse(startEndParts[1].Trim(), out int endValue)){
                    string cleanEE = new(startEndParts[1].Where(c => char.IsDigit(c)).ToArray());
                    if (int.TryParse(cleanEE.Trim(), out endValue))
                    {
                        endIsland = endValue;
                    } 
                    else
                    {
                        endValue = 0;
                    }
                }
                endIsland = endValue;
            }

        } else {
            // parts[3] contains building blocks info 
            GlobalVariables.m_Blocks = uint.Parse(parts[3]);
            startIsland = -1; // Undefined
            endIsland = -1; // Undefined
        }

        // Parse building blocks: fifth part if exists, otherwise keep default
        if (parts.Length > 4) {
            if (parts[4].StartsWith("}")) {
                string startEnd = parts[4].Substring(1);
                startIsland = int.Parse(startEnd.Split(",")[0]);
                // End island needs to be set carefully, as there could be trailing whitespace that disturbs parsing
                if (!int.TryParse(startEnd.Split(",")[1].Trim(), out int endValue)){
                    string cleanEE = new string(startEnd.Split(",")[1].Where(c => char.IsDigit(c)).ToArray());
                    if (int.TryParse(cleanEE.Trim(), out endValue))
                    {
                        endIsland = endValue;
                    } 
                    else
                    {
                        endValue = 0;
                    }
                }
                endIsland = endValue;
            } else {
                if (!string.IsNullOrWhiteSpace(parts[4])) {
                    GlobalVariables.m_Blocks = uint.Parse(parts[4]);
                }
            }
        }

        for (int i = 0; i < islands.Length; i++)
        {
            string[] islandParams = islands[i].Split(",");

            if (islandParams.Length < 4)
                continue;

            Vector2 pos = new(float.Parse(islandParams[0]), float.Parse(islandParams[1]));
            Vector2Int size = new(int.Parse(islandParams[2]), int.Parse(islandParams[3]));

            finished = false;
            StartCoroutine(CreateIsland(pos, size));
            yield return new WaitUntil(delegate { return finished; });
        }

        if (!axisPrefab)
            transform.position = -mapCenter / islands.Length;

        for (int i = 0; i < Allconnections.Length; i++)
        {
            if (TestForOverflow(i)) yield return -1;
            string[] connections = Allconnections[i].Split(";");

            for (int j = 0; j < connections.Length; j++)
            {
                if (TestForOverflow(j)) yield return -1;
                string[] connectionParams = connections[j].Split(",");
                if (connectionParams.Length < 6)
                    continue;

                Vector3[] splinePoints = new Vector3[Mathf.FloorToInt((connectionParams.Length - 2) / 3f)];
                for (int k = 0; k < splinePoints.Length; k++)
                {
                    if (TestForOverflow(k)) yield return -1;
                    splinePoints[k] = new Vector3(float.Parse(connectionParams[k * 3 + 2]), float.Parse(connectionParams[k * 3 + 3]), float.Parse(connectionParams[k * 3 + 4])) * (PreviewMode ? relativeLevelScaleMod : 1);
                }
                uint[] weights = new uint[1];
                if (connectionParams[0].Contains("?"))
                {
                    string[] weightsText = connectionParams[0].Split("?");
                    weights = new uint[weightsText.Length];
                    for (int k = 0; k < weightsText.Length; k++)
                    {
                        if (weightsText[k].Trim() == "") continue;
                        uint addableWeight = uint.Parse(weightsText[k].Trim());
                        weights[k] = addableWeight;
                    }
                }
                else
                    weights[0] = uint.Parse(connectionParams[0]);

                CreateBridge(splinePoints, weights, new(i, j));
            }
            yield return new WaitForFixedUpdate();
        }
        LoadScreenUI.SetActive(false);
    }

    public void ImportLevel(int levelID, bool previewMode=false)
    {
        if (levelID >= levels.Length)
            levelID = levels.Length - 1;

        GlobalVariables.m_Level = levelID;
        OnImport.Invoke();

        LoadScreenUI.SetActive(true);
        LevelId = levelID;
        PreviewMode = previewMode;
        StartCoroutine(importLevel());
    }
}
