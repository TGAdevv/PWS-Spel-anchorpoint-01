using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Connection
{
    public RectTransform begin, end;
    public LineRenderer lineRenderer;
    public int weight;

    public Connection(RectTransform _begin, RectTransform _end, LineRenderer _lineRenderer, int _weight)
    {
        begin = _begin;
        end = _end;
        lineRenderer = _lineRenderer;
        weight = _weight;
    }

    public override bool Equals(object obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
}

public class IslandEditor : MonoBehaviour
{
    List<ManageIslandConnections> IslandScripts = new();

    int CurrentIsland;
    public bool DragIsland;

    public GameObject islandPrefab;

    int BeginIsland, EndIsland;
    bool BeginIslandDefined, EndIslandDefined;

    [Range(10, 50)]
    [SerializeField] float UnitSize;

    void ClickedButton(int buttonID) 
    {
        CurrentIsland = buttonID;
        DragIsland = true;
    }
    void ReleaseButton(int buttonID)
    {
        DragIsland = false;
    }

    Vector2 PrevMousePos = new();
    public int CurveRes = 30;

    private void Update()
    {
        if (DragIsland)
        {
            Vector2 dPos = new((Input.mousePosition.x - PrevMousePos.x) / Screen.width, (Input.mousePosition.y - PrevMousePos.y) / Screen.height);
            IslandScripts[CurrentIsland].rect.anchorMax += dPos;
            IslandScripts[CurrentIsland].rect.anchorMin += dPos;
        }
        PrevMousePos = Input.mousePosition;
    }



    public void CreateIsland()
    {
        GameObject prefab = Instantiate(islandPrefab, GetComponent<RectTransform>());
        IslandScripts.Add(prefab.GetComponent<ManageIslandConnections>());

        int buttonID = IslandScripts.Count - 1;
        IslandScripts[^1].id = buttonID;
        IslandScripts[^1].hostScript = this;

        EventTrigger buttonEvents = IslandScripts[^1].gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry buttonDown = new() { eventID = EventTriggerType.PointerDown };
        buttonDown.callback.AddListener(delegate { ClickedButton(buttonID); });

        EventTrigger.Entry buttonUp = new() { eventID = EventTriggerType.PointerUp };
        buttonUp.callback.AddListener(delegate { ReleaseButton(buttonID); });

        buttonEvents.triggers.Add(buttonDown);
        buttonEvents.triggers.Add(buttonUp);
    }
    public void MarkBeginIsland()
    {
        if (BeginIslandDefined)
            IslandScripts[BeginIsland].button.image.color = Color.white;
        if (BeginIslandDefined && BeginIsland == CurrentIsland)
        {
            BeginIslandDefined = false;
            return;
        }
        IslandScripts[CurrentIsland].button.image.color = Color.green;
        BeginIsland = CurrentIsland;
        BeginIslandDefined = true;
    }
    public void MarkEndIsland()
    {
        if (EndIslandDefined)
            IslandScripts[EndIsland].button.image.color = Color.white;
        if (EndIslandDefined && EndIsland == CurrentIsland)
        {
            EndIslandDefined = false;
            return;
        }
        IslandScripts[CurrentIsland].button.image.color = Color.red;
        EndIsland = CurrentIsland;
        EndIslandDefined = true;
    }

    public void DeleteIsland(int id) 
    {
        Destroy(IslandScripts[id].gameObject);
        IslandScripts.RemoveAt(id);
        for (int i = id; i < IslandScripts.Count; i++)
            IslandScripts[i].id--;
    }

    // Turns everything into a graph
    public void ExportLevel() 
    {
        Vector2[] islandPositions = new Vector2[IslandScripts.Count];
        Vector2Int[] islandSizes = new Vector2Int[islandPositions.Length];
        for (int i = 0; i < islandPositions.Length; i++)
        {
            islandPositions[i] = IslandScripts[i].rect.anchorMax;
            islandSizes[i]     = new(IslandScripts[i].nWidth, IslandScripts[i].nHeight);
        }

        Graph result = new(islandPositions, islandSizes);

        for (int i = 0; i < IslandScripts.Count; i++)
        {
            for (int j = 0; j < IslandScripts[i].Connections.Count; j++)
            {
                Connection curConnection = IslandScripts[i].Connections[j];
                Vector2[] bezierPoints = IslandScripts[i].CalculateSplinePoints(curConnection.begin.position, curConnection.end.position, curConnection.end.parent.parent.parent.GetComponent<RectTransform>());
                result.AddEdge(i, curConnection.end.parent.parent.parent.GetComponent<ManageIslandConnections>().id, curConnection.weight, bezierPoints);
            }
        }

        print(result.ToString());
        string Result_TXT = result.ExportGraph();// + ";" + JsonUtility.ToJson(result.m_Connections);

        TextEditor te = new();
        te.text = Result_TXT;
        te.SelectAll();
        te.Copy();
    }

    RectTransform findConnectionRect(ManageIslandConnections islandScript, Vector2 pos) 
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

        print("Result is " + result.position.ToString() + " with distance " + closestDistance);
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
            IslandScripts[^1].rect.anchorMax = new Vector2(float.Parse(islandParams[0]), float.Parse(islandParams[1]));
            IslandScripts[^1].rect.anchorMin = IslandScripts[^1].rect.anchorMax;
        }

        for (int i = 0; i < Allconnections.Length; i++)
        {
            string[] connections = Allconnections[i].Split(";");
            print("Connections length: " + connections.Length);

            for (int j = 0; j < connections.Length; j++)
            {
                string[] connectionParams = connections[j].Split(",");
                print(connections[j]);
                if (connectionParams.Length < 6)
                    continue;

                RectTransform begin, end;
                LineRenderer lineRenderer;

                print(connectionParams[2]);
                print(connectionParams[3]);

                Vector2 beginPosition = new Vector2(float.Parse(connectionParams[2]), float.Parse(connectionParams[3]));

                float r;
                print(connectionParams[^2] + "; " + float.TryParse(connectionParams[^2], out r));
                print(connectionParams[^1] + "; " + float.TryParse(connectionParams[^1], out r));

                Vector2 endPosition   = new Vector2(float.Parse(connectionParams[^2]),float.Parse(connectionParams[^1]));

                print("Connecction Param parse succes");

                begin = findConnectionRect(IslandScripts[i], beginPosition);
                end   = findConnectionRect(IslandScripts[int.Parse(connectionParams[1])], endPosition);

                lineRenderer = IslandScripts[i].AddLineRenderer(begin.parent.GetComponent<RectTransform>());
                lineRenderer.positionCount = CurveRes;
                IslandScripts[i].Connections.Add(new(begin, end, lineRenderer, int.Parse(connectionParams[0])));
            }
            if (i < IslandScripts.Count)
                IslandScripts[i].RefreshLineRenderers();
        }
    }
}
