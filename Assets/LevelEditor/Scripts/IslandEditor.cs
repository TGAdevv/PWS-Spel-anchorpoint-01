using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Connection
{
    public RectTransform begin, end;
    public LineRenderer lineRenderer;
    public int weight;

    //Leave empty to disable variable weight
    public List<int> weightOptions;

    public Connection(RectTransform _begin, RectTransform _end, LineRenderer _lineRenderer, int _weight, int[] _weights = null)
    {
        begin = _begin;
        end = _end;
        lineRenderer = _lineRenderer;
        weight = _weight;
        if (_weights == null)
            weightOptions = new();
        else if (_weights.Length > 0)
            weightOptions = new List<int>(_weights);
        else
            weightOptions = new();
    }

    public override bool Equals(object obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
}

public class IslandEditor : MonoBehaviour
{
    public List<ManageIslandConnections> IslandScripts = new();

    int CurrentIsland;
    public bool DragIsland;

    public GameObject islandPrefab;

    int BeginIsland, EndIsland;
    bool BeginIslandDefined, EndIslandDefined;

    [Range(10, 50)]
    [SerializeField] float UnitSize;

    public Canvas canvas;
    public LevelImporter levelImporter;

    void ClickedButton(int buttonID) 
    {
        CurrentIsland = buttonID;
        DragIsland = true;
    }
    void ReleaseButton()
    {
        DragIsland = false;
    }

    Vector3 PrevMousePos = new();
    public int CurveRes = 30;

    Color defaultIslandColor;

    private void Start()
    {
        defaultIslandColor = new(0.8396226f, 0.8396226f, 0.6930847f);
    }

    private void Update()
    {
        if (DragIsland)
        {
            if (CurrentIsland > IslandScripts.Count - 1)
                return;

            Vector3 dPos = Input.mousePosition - PrevMousePos;
            IslandScripts[CurrentIsland].rect.localPosition += dPos * (1000 / (float)Display.main.renderingWidth);
        }
        PrevMousePos = Input.mousePosition;
    }

    public void CreateIsland()
    {
        GameObject prefab = Instantiate(islandPrefab, GetComponent<RectTransform>());
        IslandScripts.Add(prefab.GetComponent<ManageIslandConnections>());

        int _id = IslandScripts.Count - 1;
        IslandScripts[^1].id = _id;
        IslandScripts[^1].hostScript = this;

        EventTrigger buttonEvents = IslandScripts[^1].gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry buttonDown = new() { eventID = EventTriggerType.PointerDown };
        buttonDown.callback.AddListener(delegate { ClickedButton(_id); });

        EventTrigger.Entry buttonUp = new() { eventID = EventTriggerType.PointerUp };
        buttonUp.callback.AddListener(delegate { ReleaseButton(); });

        buttonEvents.triggers.Add(buttonDown);
        buttonEvents.triggers.Add(buttonUp);
    }
    public void AddWeightOption()
    {
        Connection connection = LevelEditorParameters.currentConnection;
        ManageIslandConnections connectionScript = connection.begin.parent.parent.parent.GetComponent<ManageIslandConnections>();

        int connectionIndex = connectionScript.Connections.FindIndex(delegate (Connection a) { return a == connection; });
        connectionScript.Connections[connectionIndex].weightOptions.Add(connection.weight);
    }
    public void RemoveCurConnection()
    {
        Connection connection = LevelEditorParameters.currentConnection;

        if (connection == null)
            return;
        if (!connection.begin)
            return;
        if (!connection.end)
            return;

        ManageIslandConnections connectionScript = connection.begin.parent.parent.parent.GetComponent<ManageIslandConnections>();
        connection.begin.parent.GetComponent<Image>().color = new(0.1908152f, 0.3495883f, 0.6037736f);

        connection.begin.GetComponent<TMP_Text>().text = "";
        connectionScript.Connections.Remove(connection);

        if (connection.lineRenderer) Destroy(connection.lineRenderer);
    }
    public void PassDownConnectionToWeightsConfig(ConfigureWeightOptions weightsConfigScript)
    {
        Connection connection = LevelEditorParameters.currentConnection;
        ManageIslandConnections connectionScript = connection.begin.parent.parent.parent.GetComponent<ManageIslandConnections>();
        int connectionIndex = connectionScript.Connections.FindIndex(delegate (Connection a) { return a == connection; });
        weightsConfigScript.CurrentConnection = connectionScript.Connections[connectionIndex];
    }
    public void MarkBeginIsland()
    {
        if (BeginIslandDefined)
            IslandScripts[BeginIsland].button.image.color = defaultIslandColor;
        if (BeginIslandDefined && BeginIsland == CurrentIsland)
        {
            BeginIslandDefined = false;
            return;
        }
        IslandScripts[CurrentIsland].button.image.color = Color.green * defaultIslandColor;
        BeginIsland = CurrentIsland;
        BeginIslandDefined = true;
        if (EndIsland == CurrentIsland && EndIslandDefined)
            EndIslandDefined = false;
    }
    public void MarkEndIsland()
    {
        if (EndIslandDefined)
            IslandScripts[EndIsland].button.image.color = defaultIslandColor;
        if (EndIslandDefined && EndIsland == CurrentIsland)
        {
            EndIslandDefined = false;
            return;
        }
        IslandScripts[CurrentIsland].button.image.color = Color.red * defaultIslandColor;
        EndIsland = CurrentIsland;
        EndIslandDefined = true;
        if (BeginIsland == CurrentIsland && BeginIslandDefined)
            BeginIslandDefined = false;
    }

    public void DeleteIsland(int id) 
    {
        Destroy(IslandScripts[id].gameObject);
        IslandScripts.RemoveAt(id);
        for (int i = id; i < IslandScripts.Count; i++)
        {
            IslandScripts[i].id--;
            int _id = i;

            EventTrigger.Entry buttonDown = new() { eventID = EventTriggerType.PointerDown };
            buttonDown.callback.AddListener(delegate { ClickedButton(_id); });

            IslandScripts[i].GetComponent<EventTrigger>().triggers[0] = buttonDown;
        }
    }

    public void EventExportLevel(int splinePointsHaveYValue)
    {
        ExportLevel(splinePointsHaveYValue == 1);
    }

    // Turns everything into a graph
    public string ExportLevel(bool splinePointsHaveYValue, bool copyToClipboard = true) 
    {
        Vector2[] islandPositions = new Vector2[IslandScripts.Count];
        Vector2Int[] islandSizes = new Vector2Int[islandPositions.Length];
        for (int i = 0; i < islandPositions.Length; i++)
        {
            islandPositions[i] = IslandScripts[i].rect.position;
            islandSizes[i]     = new(IslandScripts[i].nWidth, IslandScripts[i].nHeight);
        }

        Graph result = new(islandPositions, islandSizes);

        int curIndex = 0;
        for (int i = 0; i < IslandScripts.Count; i++)
        {
            for (int j = 0; j < IslandScripts[i].Connections.Count; j++)
            {
                Connection curConnection = IslandScripts[i].Connections[j];
                Vector3[] bezierPoints;
                if (!splinePointsHaveYValue)
                {
                    try { curConnection.end.parent.parent.parent.GetComponent<RectTransform>();  }
                    catch
                    {
                        Debug.LogWarning("COULD NOT FIND RECT OF CONNECTION " + j + " AT ISLAND INDEX " + i);
                        return "";
                    }
                    Vector2[] bezierPoints2D = IslandScripts[i].CalculateSplinePoints(curConnection.begin.position, curConnection.end.position, curConnection.end.parent.parent.parent.GetComponent<RectTransform>());
                    bezierPoints = new Vector3[bezierPoints2D.Length];

                    for (int k = 0; k < bezierPoints2D.Length; k++)
                    {
                        bezierPoints[k] = new(bezierPoints2D[k].x, (k == 0 || k == bezierPoints.Length-1) ? 0 : .15f, bezierPoints2D[k].y);
                    }
                }
                else
                {
                    if (curIndex >= levelImporter.Bridges.Count)
                    {
                        Debug.LogWarning("Array length exceeded: " + curIndex + " vs " + (levelImporter.Bridges.Count - 1));
                        continue;
                    }

                    Transform[] splineTransforms = levelImporter.Bridges[curIndex].splinePoints.ToArray();
                    bezierPoints = new Vector3[splineTransforms.Length];
                    for (int k = 0; k < splineTransforms.Length; k++)
                    {
                        bezierPoints[k] = splineTransforms[k].position;
                    }
                }
                if (curConnection.weightOptions.Count == 0)
                    result.AddEdge(i, curConnection.end.parent.parent.parent.GetComponent<ManageIslandConnections>().id, curConnection.weight, bezierPoints);
                else
                    result.AddEdge(i, curConnection.end.parent.parent.parent.GetComponent<ManageIslandConnections>().id, 0, bezierPoints, curConnection.weightOptions.ToArray());

                curIndex++;
            }
        }

        print(result.ToString());
        string Result_TXT = result.ExportGraph();

        //                              Important for converting to worldspace
        Result_TXT += "_" + Display.main.renderingWidth + "," + Display.main.renderingHeight;
        if (BeginIslandDefined && EndIslandDefined)
            Result_TXT += "}" + BeginIsland + "," + EndIsland;

        if (copyToClipboard)
        {
            TextEditor te = new();
            te.text = Result_TXT;
            te.SelectAll();
            te.Copy();
        }

        return Result_TXT;
    }

    RectTransform FindConnectionRect(ManageIslandConnections islandScript, Vector2 pos) 
    {
        RectTransform result = null;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < 4; i++)
        {
            List<RectTransform> curCheckedDir = new();
            if (i == 0) curCheckedDir = islandScript.ConnectionsBottom;
            if (i == 1) curCheckedDir = islandScript.ConnectionsLeft;
            if (i == 2) curCheckedDir = islandScript.ConnectionsRight;
            if (i == 3) curCheckedDir = islandScript.ConnectionsTop;

            for (int j = 0; j < curCheckedDir.Count; j++)
            {
                RectTransform curConnection = curCheckedDir[j].GetChild(0).GetComponent<RectTransform>();
                if (Vector2.Distance(curConnection.position, pos) < closestDistance)
                {
                    result = curConnection;
                    closestDistance = Vector2.Distance(curConnection.position, pos);
                }
            }
        }
        return result;
    }

    public void ImportLevel(TMPro.TMP_Text _level) 
    {
        //First wipe everything from whats currently being edited
        foreach (ManageIslandConnections island in IslandScripts)
            Destroy(island.gameObject);
        IslandScripts = new();
        LevelEditorParameters.currentConnection = null;
        CurrentIsland = 0;
        BeginIsland = 0;
        BeginIslandDefined = false;
        EndIsland = 0;
        EndIslandDefined = false;

        string level = _level.text;

        string[] islands        = level.Split("_")[0].Split(";");
        string[] Allconnections = level.Split("_")[1].Split("/");

        for (int i = 0; i < islands.Length; i++)
        {
            string[] islandParams = islands[i].Split(",");
            CreateIsland();
            IslandScripts[^1].DefineStartVariables();
            IslandScripts[^1].UpdateConnections(int.Parse(islandParams[2]), int.Parse(islandParams[3]));
            IslandScripts[^1].rect.position = new Vector2(float.Parse(islandParams[0]), float.Parse(islandParams[1]));
        }

        for (int i = 0; i < Allconnections.Length; i++)
        {
            string[] connections = Allconnections[i].Split(";");

            for (int j = 0; j < connections.Length; j++)
            {
                string[] connectionParams = connections[j].Split(",");
                if (connectionParams.Length < 6)
                    continue;

                RectTransform begin, end;
                LineRenderer lineRenderer;

                int screenX = int.Parse(level.Split("_")[2].Split(",")[0]);
                float relativeScaleMod = levelImporter.LevelScaleMod * (1000f / screenX);

                Vector2 beginPosition = new Vector2(float.Parse(connectionParams[2]), float.Parse(connectionParams[4])) / relativeScaleMod;
                Vector2 endPosition   = new Vector2(float.Parse(connectionParams[^3]),float.Parse(connectionParams[^1]))/ relativeScaleMod;

                begin = FindConnectionRect(IslandScripts[i], beginPosition);
                end   = FindConnectionRect(IslandScripts[int.Parse(connectionParams[1])], endPosition);

                lineRenderer = IslandScripts[i].AddLineRenderer(begin.parent.GetComponent<RectTransform>());
                lineRenderer.positionCount = CurveRes;

                int weight = 1;
                int[] weights = new int[0];
                if (connectionParams[0].Contains("?"))
                {
                    string[] weightsText = connectionParams[0].Split("?");
                    weights = new int[weightsText.Length];
                    for (int k = 0; k < weightsText.Length; k++)
                        weights[k] = int.Parse(weightsText[k]);
                }
                else
                    weight = int.Parse(connectionParams[0]);

                IslandScripts[i].Connections.Add(new(begin, end, lineRenderer, weight, weights));
            }
            if (i < IslandScripts.Count)
                IslandScripts[i].RefreshLineRenderers();
        }
    }
}
