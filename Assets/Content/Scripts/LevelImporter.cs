using System.Collections.Generic;
using UnityEngine;

public class LevelImporter : MonoBehaviour
{
    [SerializeField] string[] levels;

    public Transform IslandsParent;
    public Transform BridgesParent;

    public Material[] BridgeMats = new Material[2];

    List<Transform> Islands = new();
    List<SplineEditor> Bridges = new();

    public float LevelScaleMod = 3;
    public float DistancePerPillar;
    public float DistancePerSample;

    public GameObject[] IslandTiles;
    public AnchorToPosition anchorToPosition;

    private void Start()
    {
        ImportLevel(0);
    }

    void CreateIsland(Vector2 pos, Vector2Int size) 
    {
        print("pos: " + pos.ToString());

        GameObject island = new("island " + pos.ToString());
        island.transform.position = anchorToPosition._AnchorToPosition(pos) * LevelScaleMod;
        island.transform.parent = IslandsParent;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                //TEMP! Should not be 0 (but the tile actually needed)
                GameObject newTile = Instantiate(IslandTiles[0], island.transform.position + new Vector3(i, 0, j) * LevelScaleMod, Quaternion.identity, island.transform);
                newTile.transform.localScale = Vector3.one * LevelScaleMod;
            }
        }
    }
    void CreateBridge(Vector3[] splinePoints, float weight, Vector2Int index)
    {
        GameObject newBridge = new("Bridge " + index.x + ", " + index.y);
        newBridge.transform.parent = BridgesParent;
        newBridge.AddComponent<MeshCollider>();
        newBridge.AddComponent<MeshFilter>();
        newBridge.AddComponent<MeshRenderer>();

        float splineDistance = Vector3.Distance(splinePoints[0], splinePoints[^1]);

        SplineEditor newSpline = newBridge.AddComponent<SplineEditor>();
        newSpline.BridgeMats  = BridgeMats;
        newSpline.resolution  = Mathf.CeilToInt(splineDistance  / DistancePerSample);
        newSpline.pillarCount = Mathf.RoundToInt(splineDistance / DistancePerPillar);

        List<Transform> splineTransforms = new();

        for (int i = 0; i < splinePoints.Length; i++)
        {
            GameObject newSplinePoint = new("P" + i);
            newSplinePoint.transform.position = splinePoints[i];
            newSplinePoint.transform.parent   = newBridge.transform;
            splineTransforms.Add(newSplinePoint.transform);
        }

        newSpline.splinePoints = splineTransforms;
    }

    public void ImportLevel(int levelID)
    {
        //First wipe everything from the current scene
        foreach (var island in Islands)
            Destroy(island.gameObject);
        foreach (var bridge in Bridges)
            Destroy(bridge.gameObject);
        Islands = new();
        Bridges = new();

        string level = levels[levelID];

        string[] islands = level.Split("_")[0].Split(";");
        string[] Allconnections = level.Split("_")[1].Split("/");

        for (int i = 0; i < islands.Length; i++)
        {
            string[] islandParams = islands[i].Split(",");

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

                Vector3[] splinePoints = new Vector3[Mathf.FloorToInt((connectionParams.Length - 2)*.5f)];
                for (int k = 0; k < splinePoints.Length; k++)
                {
                    splinePoints[k] = new(float.Parse(connectionParams[k * 2 + 2]), (k == 0 || k == splinePoints.Length-1) ? 0 : Random.Range(.1f, .5f), float.Parse(connectionParams[k * 2 + 3]));
                    splinePoints[k] *= LevelScaleMod;
                }
                float weight = float.Parse(connectionParams[0]);

                CreateBridge(splinePoints, weight, new(i, j));
            }
        }
    }
}
