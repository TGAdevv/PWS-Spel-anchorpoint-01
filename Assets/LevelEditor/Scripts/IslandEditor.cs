using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
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

    public int LevelScaleMod;

    public Canvas canvas;
    public LevelImporter levelImporter;
    public Slider BuildingBlocksSlider;

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

    bool TestForOverflow(int iteration, [CallerLineNumber] int codeLine = 0)
    {
        if (iteration > 10000)
        {
            string ErrorLink = "<a href=\"Assets/LevelEditor/Scripts/IslandEditor.cs\" line=\"" + codeLine + "\">Custom Error 0.0</a>";
            Debug.LogWarning(ErrorLink + ": Exceeded max iteration count of 10000");
        }
        return (iteration > 10000);
    }

    // Turns everything into a graph
    public string ExportLevel(bool splinePointsHaveYValue, bool copyToClipboard = true) 
    {
        CultureInfo US = new("en-US");
        Thread.CurrentThread.CurrentCulture = US;

        Vector2[] islandPositions = new Vector2[IslandScripts.Count];
        Vector2Int[] islandSizes = new Vector2Int[islandPositions.Length];
        for (int i = 0; i < islandPositions.Length; i++)
        {
            if (TestForOverflow(i, 210)) return "ERROR";

            islandPositions[i] = IslandScripts[i].rect.position;
            islandSizes[i]     = new(IslandScripts[i].nWidth, IslandScripts[i].nHeight);
        }

        Graph result = new(islandPositions, islandSizes);

        int curIndex = 0;
        for (int i = 0; i < IslandScripts.Count; i++)
        {
            if (TestForOverflow(i)) return "ERROR";

            for (int j = 0; j < IslandScripts[i].Connections.Count; j++)
            {
                if (TestForOverflow(j)) return "ERROR";

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
                        if (TestForOverflow(k)) return "ERROR";
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
                        if (TestForOverflow(k)) return "ERROR";
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

        string Result_TXT = result.ExportGraph();

        //                              Important for converting to worldspace
        Result_TXT += "_" + Display.main.renderingWidth + "," + Display.main.renderingHeight;
        if (BeginIslandDefined && EndIslandDefined)
            Result_TXT += "_}" + BeginIsland + "," + EndIsland;
        Result_TXT += "_" + (uint)BuildingBlocksSlider.value;

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

    public void ImportLevel(TMP_Text _level) 
    {
        CultureInfo US = new("en-US");
        Thread.CurrentThread.CurrentCulture = US;

        // Wipe everything from what's currently being edited
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

        string buildingblocks = null;
        string startend = null;

        if (level.Split("_").Length > 3)
        {
            if (level.Split("_")[3].StartsWith("}"))
            {
                // parts[3] is }start,end
                startend = level.Split("_")[3];
            }
            else
            {
                // parts[3] is buildingblocks
                buildingblocks = level.Split("_")[3];
                if (!int.TryParse(buildingblocks.Trim(), out int sliderValue)){
                    string cleanBB = new(buildingblocks.Where(c => char.IsDigit(c)).ToArray());
                    if (int.TryParse(cleanBB.Trim(), out sliderValue))
                    {
                        BuildingBlocksSlider.value = sliderValue;
                    }
                    else
                    {
                        sliderValue = 1;  
                    }  
                }
                BuildingBlocksSlider.value = sliderValue;
            }
        }
        if (level.Split("_").Length > 4)
        {
            buildingblocks = level.Split("_")[4];
            if (!int.TryParse(buildingblocks.Trim(), out int sliderValue)){
                string cleanBB = new(buildingblocks.Where(c => char.IsDigit(c)).ToArray());
                if (int.TryParse(cleanBB.Trim(), out sliderValue))
                {
                    BuildingBlocksSlider.value = sliderValue;
                } 
                else
                {
                    sliderValue = 1;  
                }
                }
                BuildingBlocksSlider.value = sliderValue;
        }

        // Create islands
        for (int i = 0; i < islands.Length; i++)
        {
            if (TestForOverflow(i)) return;

            string[] islandParams = islands[i].Split(",");
            CreateIsland();
            IslandScripts[^1].DefineStartVariables();
            IslandScripts[^1].UpdateConnections(int.Parse(islandParams[2].Trim()), int.Parse(islandParams[3].Trim()));
            IslandScripts[^1].rect.position = new Vector2(float.Parse(islandParams[0].Trim()), float.Parse(islandParams[1].Trim()));
        }

        for (int i = 0; i < Allconnections.Length; i++)
        {
            if (TestForOverflow(i)) return;
            string[] connections = Allconnections[i].Split(";");

            for (int j = 0; j < connections.Length; j++)
            {
                if (TestForOverflow(j)) return;
                if (string.IsNullOrWhiteSpace(connections[j]))
                    continue;

                string[] connectionParams = connections[j].Split(",");
                bool emptySpace = false;
                for (int p = 0; p < connectionParams.Length; p++)
                {
                    connectionParams[p] = connectionParams[p].Trim();
                    if (string.IsNullOrEmpty(connectionParams[p]))
                        emptySpace = true;
                }
                if (connectionParams.Length < 6 || emptySpace)
                    continue;
                
                RectTransform begin, end;
                LineRenderer lineRenderer;

                int screenX = int.Parse(level.Split("_")[2].Split(",")[0].Trim());
                float relativeScaleMod = levelImporter.LevelScaleMod * (1000f / screenX);

                Vector2 beginPosition = new Vector2(float.Parse(connectionParams[2].Trim()), float.Parse(connectionParams[4].Trim())) / relativeScaleMod;
                Vector2 endPosition   = new Vector2(float.Parse(connectionParams[^3].Trim()),float.Parse(connectionParams[^1].Trim())) / relativeScaleMod;

                begin = FindConnectionRect(IslandScripts[i], beginPosition);
                end   = FindConnectionRect(IslandScripts[int.Parse(connectionParams[1].Trim())], endPosition);

                lineRenderer = IslandScripts[i].AddLineRenderer(begin.parent.GetComponent<RectTransform>());
                lineRenderer.positionCount = CurveRes;

                int weight = 0;
                int[] weights = new int[0];
                if (connectionParams[0].Contains("?"))
                {
                    string[] weightsText = connectionParams[0].Split("?");
                    weights = new int[weightsText.Length];
                    for (int k = 0; k < weightsText.Length; k++)
                        weights[k] = int.Parse(weightsText[k].Trim());
                }
                else
                    weight = int.Parse(connectionParams[0].Trim());

                IslandScripts[i].Connections.Add(new(begin, end, lineRenderer, weight, weights));
            }
            if (i < IslandScripts.Count)
                IslandScripts[i].RefreshLineRenderers();
        }
        
        // Set start and end islands if provided
        if (!string.IsNullOrEmpty(startend))
        {
            string[] startEndParts = startend.Split("}")[1].Split(",");
            BeginIsland = int.Parse(startEndParts[0].Trim());
            //endisland needs to be set carefully, as there could be trailing whitespace that disturbs parsing
            if (!int.TryParse(startEndParts[1].Trim(), out int endValue)){
                string cleanEE = new(startEndParts[1].Where(c => char.IsDigit(c)).ToArray());
                if (int.TryParse(cleanEE.Trim(), out endValue))
                {
                    EndIsland = endValue;
                } 
                else
                {
                    endValue = 0;
                }
                }
            EndIsland = endValue;
            // Temporarily set CurrentIsland to mark the islands correctly
            int originalCurrent = CurrentIsland;
            CurrentIsland = EndIsland;
            MarkEndIsland();
            CurrentIsland = BeginIsland;
            MarkBeginIsland();
            CurrentIsland = originalCurrent;
        }
            
        
    }
}
