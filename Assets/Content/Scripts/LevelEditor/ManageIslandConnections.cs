using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ManageIslandConnections : MonoBehaviour
{
    public IslandEditor hostScript;
    public float UnitSize;
    public RectTransform rect;
    public Button button;
    public int id;

    public int nWidth = 1, nHeight = 1;

    [SerializeField] RectTransform connectionTop,   connectionBottom;
    [SerializeField] RectTransform connectionRight, connectionLeft;

    public List<RectTransform> ConnectionsTop   = new(), ConnectionsBottom = new();
    public List<RectTransform> ConnectionsRight = new(), ConnectionsLeft   = new();

    Material lineRenderMaterial;

    public LineRenderer currentLineRenderer;
    Camera cam;

    public List<Connection> Connections = new();

    Slider CurveSize;

    int ConnectionSelected = -1;
    int CurveRes;

    public void DefineStartVariables()
    {
        CurveRes = hostScript.CurveRes;
        rect   = GetComponent<RectTransform>();
        button = GetComponent<Button>();

        ConnectionsTop.Add(connectionTop);
        ConnectionsBottom.Add(connectionBottom);
        ConnectionsRight.Add(connectionRight);
        ConnectionsLeft.Add(connectionLeft);

        lineRenderMaterial = new(Shader.Find("Universal Render Pipeline/Unlit"));
        cam = Camera.main;

        CurveSize = GameObject.FindGameObjectWithTag("CurveSize").GetComponent<Slider>();
    }

    private void Start()
    {
        if (!rect)
            DefineStartVariables();
    }

    private void Update()
    {
        if (currentLineRenderer != null) 
        {
            currentLineRenderer.SetPosition(1, (Vector2)cam.ScreenToWorldPoint(Input.mousePosition));
        }

        if (ConnectionSelected != -1)
            if (ConnectionSelected < Connections.Count)
            {
                if (LevelEditorParameters.currentConnection != Connections[ConnectionSelected])
                    ConnectionSelected = -1;
            }
            else
                ConnectionSelected = -1;


        if (ConnectionSelected != -1)
            Connections[ConnectionSelected].begin.GetComponent<TMPro.TMP_Text>().text = Connections[ConnectionSelected].weight.ToString();

        if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            RefreshLineRenderers();
    }

    public void RefreshLineRenderers()
    {
        for (int i = Connections.Count-1; i >= 0; i--)
        {
            var connection = Connections[i];
            if (connection.end == null)
            {
                connection.begin.GetComponent<TMPro.TMP_Text>().text = "";
                Destroy(connection.lineRenderer);
                Connections.RemoveAt(i);
                continue;
            }
            connection.begin.GetComponent<TMPro.TMP_Text>().text = connection.weight.ToString();
            connection.lineRenderer.SetPositions(GenerateBezierConnection(connection.begin.position, connection.end.position, connection.end.parent.parent.parent.GetComponent<RectTransform>(), CurveRes));
        }
    }

    void DestroyComponents<T>(RectTransform[] Transforms) where T:Component
    {
        foreach (RectTransform Transform in Transforms)
            if (Transform.TryGetComponent(out T tempComp)) Destroy(tempComp);
    }

    Vector2 roundVector2(Vector2 vec) => new(Mathf.Round(vec.x), Mathf.Round(vec.y));

    public Vector2 SamplePointInCurve2D(float t, Vector2[] splinePoints)
    {
        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Explicit_definition

        Vector2 result = Vector2.zero;
        int n = splinePoints.Length - 1;

        for (int i = 0; i <= n; i++)
            result += CustomMath.nCr(n, i) * Mathf.Pow((1 - t), n - i) * Mathf.Pow(t, i) * splinePoints[i];

        return result;
    }
    public Vector2[] CalculateSplinePoints(Vector2 beginPoint, Vector2 endPoint, RectTransform endIslandRect)
    {
        Vector2 directionBeg = roundVector2(((Vector2)rect.position - beginPoint).normalized / Mathf.Sqrt(2)) * Vector2.Distance(beginPoint, endPoint);
        Vector2 directionEnd = roundVector2(((Vector2)endIslandRect.position - endPoint).normalized / Mathf.Sqrt(2)) * Vector2.Distance(beginPoint, endPoint);

        return new Vector2[4] { beginPoint, beginPoint - directionBeg * CurveSize.value, endPoint - directionEnd * CurveSize.value, endPoint };
    }
    Vector3[] GenerateBezierConnection(Vector2 beginPoint, Vector2 endPoint, RectTransform endIslandRect, int resolution) 
    {
        Vector2[] splinePoints = CalculateSplinePoints(beginPoint, endPoint, endIslandRect);
        Vector3[] result = new Vector3[resolution];

        for (int i = 0; i < resolution; i++)
        {
            result[i] = SamplePointInCurve2D((float)i / ((float)resolution - 1f), splinePoints);
        }

        return result;
    }

    public void UpdateConnections(int new_nWidth, int new_nHeight) 
    {
        if (new_nWidth == 0 || new_nHeight == 0)
            hostScript.DeleteIsland(id);

        rect.sizeDelta = new Vector2(new_nWidth, new_nHeight) * UnitSize;

        if (new_nWidth > nWidth) 
        {
            // More width
            for (int i = 0; i < new_nWidth - nWidth; i++)
            {
                RectTransform newConnectionTop    = Instantiate(ConnectionsTop[0],    connectionTop.parent);
                RectTransform newConnectionBottom = Instantiate(ConnectionsBottom[0], connectionBottom.parent);

                newConnectionTop.localPosition    += Vector3.right * UnitSize * ConnectionsTop.Count;
                newConnectionBottom.localPosition += Vector3.right * UnitSize * ConnectionsTop.Count;

                DestroyComponents<LineRenderer>(new RectTransform[2] { newConnectionTop, newConnectionBottom});

                ConnectionsTop.Add(newConnectionTop);
                ConnectionsBottom.Add(newConnectionBottom);
            }
        }
        else if (new_nWidth < nWidth)
        {
            // Less width
            for (int i = 0; i < nWidth - new_nWidth; i++) 
            {
                Destroy(ConnectionsTop[^1].gameObject);
                Destroy(ConnectionsBottom[^1].gameObject);

                ConnectionsTop.RemoveAt(nWidth - 1);
                ConnectionsBottom.RemoveAt(nWidth - 1);
            }
        }

        if (new_nHeight > nHeight)
        {
            // More height
            for (int i = 0; i < new_nHeight - nHeight; i++)
            {
                RectTransform newConnectionRight = Instantiate(ConnectionsRight[0], connectionRight.parent);
                RectTransform newConnectionLeft  = Instantiate(ConnectionsLeft[0],  connectionLeft.parent);

                newConnectionRight.localPosition += Vector3.up * UnitSize * ConnectionsLeft.Count;
                newConnectionLeft.localPosition  += Vector3.up * UnitSize * ConnectionsLeft.Count;

                DestroyComponents<LineRenderer>(new RectTransform[2] { newConnectionRight, newConnectionLeft });

                ConnectionsRight.Add(newConnectionRight);
                ConnectionsLeft.Add(newConnectionLeft);
            }
        }
        else if (new_nHeight < nHeight)
        {
            // Less height
            for (int i = 0; i < nHeight - new_nHeight; i++)
            {
                Destroy(ConnectionsRight[^1].gameObject);
                Destroy(ConnectionsLeft[^1].gameObject);

                ConnectionsRight.RemoveAt(nHeight - 1);
                ConnectionsLeft.RemoveAt(nHeight - 1);
            }
        }

        nWidth = new_nWidth;
        nHeight = new_nHeight;

        RefreshLineRenderers();
    }

    public LineRenderer AddLineRenderer(RectTransform connection) 
    {
        LineRenderer result = connection.gameObject.AddComponent<LineRenderer>();
        result.widthMultiplier = .05f;
        result.materials = new Material[1] { lineRenderMaterial };
        return result;
    }
    public void ClickedConnection(RectTransform connection)
    {
        RectTransform centerRect = connection.GetChild(0).GetComponent<RectTransform>();

        if (connection.gameObject.GetComponent<LineRenderer>())
            for (int i = 0; i < Connections.Count; i++)
                if (Connections[i].begin == centerRect)
                {
                    LevelEditorParameters.currentConnection = Connections[i];
                    ConnectionSelected = i;
                    return;
                }

        currentLineRenderer = AddLineRenderer(connection);
        currentLineRenderer.positionCount = 2;
        currentLineRenderer.SetPosition(0, (Vector2)centerRect.position);
    }
    public void ReleasedConnection(RectTransform connection)
    {
        if (!currentLineRenderer)
            return;

        EventSystem currentEventSys = EventSystem.current;
        PointerEventData eventData = new(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("Connection")) 
            {
                RectTransform ConnectionRect = result.gameObject.GetComponent<RectTransform>();
                if (ConnectionRect == connection)
                    continue;

                currentLineRenderer.positionCount = CurveRes;

                Connections.Add(new(connection.GetChild(0).GetComponent<RectTransform>(), ConnectionRect.GetChild(0).GetComponent<RectTransform>(), currentLineRenderer, 0));
                Connections[^1].begin.GetComponent<TMPro.TMP_Text>().text = "0";
                currentLineRenderer = null;

                LevelEditorParameters.currentConnection = Connections[^1];
                ConnectionSelected = Connections.Count - 1;

                RefreshLineRenderers();
                return;
            }
        }
        Destroy(currentLineRenderer);
        currentLineRenderer = null;
    }

    public void AddRowOrCollumn(bool addRow)
    {
        if (addRow)
            UpdateConnections(nWidth, nHeight + 1);
        else
            UpdateConnections(nWidth + 1, nHeight);
    }
    public void DeleteRowOrCollumn(bool deleteRow)
    {
        if (deleteRow)
            UpdateConnections(nWidth, nHeight - 1);
        else
            UpdateConnections(nWidth - 1, nHeight);
    }
}
