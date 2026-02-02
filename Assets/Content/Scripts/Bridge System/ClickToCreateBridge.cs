using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class ClickToCreateBridge : MonoBehaviour
{
    float speed = 1.4f;

    float bridgeActive = 0;
    public bool targetBridgeActive = false;
    SplineEditor spline;

    public uint price;
    public int startIsland;
    public int endIsland;

    public AudioSource buildBridgeSFX;
    public AudioSource destroyBridgeSFX;
    public AudioSource bridgeSnap;

    // Puzzle mode variables
    public UnityEvent HideAllUI, ShowAllUI;
    private bool isParentBridge = false;
    private bool inPuzzleMode;
    private List<GameObject> segmentBridges = new();
    private Dictionary<GameObject, Vector3> originalSegmentPositions = new();
    private HashSet<GameObject> snappedSegments = new();
    private GameObject currentlyDraggingSegment = null;
    private float snapDistance = 8f;  // Increased from 5f for more forgiving snapping
    private Vector3 dragOffset = Vector3.zero;
    private float raiseHeight = 3f;
    private Vector3 segmentStartPos = Vector3.zero;

    public GameObject PuzzleModeCanvas;
    public Button CancelButton;

    void Start()
    {
        spline = GetComponent<SplineEditor>();

        // Check if this is a parent bridge with segments
        if (GlobalVariables.bridgeBuildParts != null && GlobalVariables.bridgeBuildParts.ContainsKey(gameObject))
        {
            isParentBridge = true;
            segmentBridges = GlobalVariables.bridgeBuildParts[gameObject];
            foreach (var segment in segmentBridges)
            {
                // Get original position from GlobalVariables
                if (GlobalVariables.bridgeSegmentOriginalPositions != null && GlobalVariables.bridgeSegmentOriginalPositions.ContainsKey(segment))
                    originalSegmentPositions[segment] = GlobalVariables.bridgeSegmentOriginalPositions[segment];
            }
        }
    }
    private void OnMouseDown()
    {
        if (!enabled)
            return;

        if (GlobalVariables.inPuzzleMode && !inPuzzleMode)
            return;

        EventSystem currentEventSys = EventSystem.current;
        PointerEventData eventData = new(currentEventSys);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new();
        currentEventSys.RaycastAll(eventData, results);
        if (results.Count > 0)
            return;

        // Handle parent bridge click to start puzzle mode
        if (isParentBridge && !GlobalVariables.inPuzzleMode && !targetBridgeActive)
        {
            EnterPuzzleMode();
            return;
        }

        // Don't allow building/destroying blocks while in puzzle mode
        if (GlobalVariables.inPuzzleMode)
            return;

        // Normal bridge creation logic
        GlobalVariables.m_Blocks += price;

        if (targetBridgeActive)
            GlobalVariables.m_Blocks -= price * 2;

        float pitchMod = Random.Range(0.9f, 1.1f);
        AudioSource sourceToPlay = targetBridgeActive ? destroyBridgeSFX : buildBridgeSFX;
        sourceToPlay.pitch = pitchMod;
        speed = 1.6f * pitchMod;
        sourceToPlay.Play();

        targetBridgeActive = !targetBridgeActive;
        Bridge ThisBridge = new(startIsland, endIsland, price, targetBridgeActive);

        //check for bridge in globalvariables and update accordingly
        for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
        {
            Bridge CurBridge = GlobalVariables.possibleBridges[i];
            if (ThisBridge == CurBridge)
            {
                GlobalVariables.possibleBridges[i] = ThisBridge;
                return;
            }
        }
    }

    private void EnterPuzzleMode()
    {
        if (inPuzzleMode)
            return;
        HideAllUI.Invoke();

        PuzzleModeCanvas.SetActive(true);
        CancelButton.onClick.RemoveAllListeners();
        CancelButton.onClick.AddListener(delegate { CancelPuzzleMode(); });

        GlobalVariables.inPuzzleMode = true;
        inPuzzleMode = true;
        snappedSegments.Clear();
        
        // Make parent bridge see-through during puzzle
        spline.percentage_transparent = 1f;

        // Validate all segments exist and have required components
        int validSegments = 0;
        foreach (var segment in segmentBridges)
        {
            if (segment == null)
                continue;

            // Ensure segment has all required components
            MeshCollider collider = segment.GetComponent<MeshCollider>();
            MeshRenderer renderer = segment.GetComponent<MeshRenderer>();
            SplineEditor splineEd = segment.GetComponent<SplineEditor>();
            
            if (!collider || !renderer || !splineEd)
                continue;
            
            // Enable all components
            renderer.enabled = true;
            segment.SetActive(true);

            validSegments++;
        }

        // Scatter segment bridges around the screen and raise them
        foreach (var segment in segmentBridges)
        {
            if (segment == null) continue;

            Vector3 randomPos = new(segment.transform.localPosition.x + Random.Range(-15f, 15f), 0, segment.transform.localPosition.z + Random.Range(-15f, 15f));
            randomPos.y += raiseHeight; // Raise by default
            segment.transform.localPosition = randomPos;
        }
        // update global variable to mark bridge as in puzzle mode
        Bridge thisBridge = new(startIsland, endIsland, price, targetBridgeActive, true);
        for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
        {
            Bridge CurBridge = GlobalVariables.possibleBridges[i];
            if (thisBridge == CurBridge)
            {
                GlobalVariables.possibleBridges[i] = thisBridge;
                break;
            }
        }
    }

    void CancelPuzzleMode()
    {
        ShowAllUI.Invoke();

        PuzzleModeCanvas.SetActive(false);

        GlobalVariables.inPuzzleMode = false;
        inPuzzleMode = false;
        currentlyDraggingSegment = null;
        // Hide segment bridges and disable their colliders
        foreach (var segment in segmentBridges)
        {
            if (segment != null)
                segment.SetActive(false);
        }

        // Ensure parent bridge is fully visible
        gameObject.SetActive(true);
    }

    private void ExitPuzzleMode()
    {
        ShowAllUI.Invoke();

        PuzzleModeCanvas.SetActive(false);

        GlobalVariables.inPuzzleMode = false;
        inPuzzleMode = false;
        currentlyDraggingSegment = null;
        // Hide segment bridges and disable their colliders
        foreach (var segment in segmentBridges)
        {
            if (segment != null)
                segment.SetActive(false);
        }

        // Ensure parent bridge is fully visible
        gameObject.SetActive(true);
        targetBridgeActive = true;  // Make sure bridge appears active/visible
        GlobalVariables.m_Blocks += price;
        bridgeActive = 1;  // Set to fully visible
        timer = 0;  // Reset animation timer
        spline.percentage_transparent = 0;  // Set to fully opaque
        
        // Play sound and Log completion in global variables
        float pitchMod = Random.Range(0.9f, 1.1f);
        buildBridgeSFX.pitch = pitchMod;
        buildBridgeSFX.Play();
        Bridge thisBridge = new(startIsland, endIsland, price, targetBridgeActive, false);
        for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
        {
            Bridge CurBridge = GlobalVariables.possibleBridges[i];
            if (thisBridge == CurBridge)
            {
                GlobalVariables.possibleBridges[i] = thisBridge;
                break;
            }
        }
    }

    float timer = 0;

    private void Update()
    {
    if (!inPuzzleMode)
        goto NormalBridgeLogic;

    Camera cam = Camera.main;

    // Convert mouse to XZ world position
    Vector2 mousePos = Input.mousePosition;
    Ray screenRay = cam.ScreenPointToRay(mousePos);

    Plane xzPlane = new(Vector3.up, 0f); // Y = 0 plane
    if (!xzPlane.Raycast(screenRay, out float planeEnter))
        return;

    Vector3 mouseWorldXZ = screenRay.GetPoint(planeEnter);

    // Draw vertical ray (for debugging)
    Vector3 rayStart = mouseWorldXZ + Vector3.up * 100f;
    Debug.DrawRay(rayStart, Vector3.down * 200f, Color.red);

    // START DRAG
    if (Input.GetMouseButtonDown(0) && currentlyDraggingSegment == null)
    {
        Ray downRay = new(rayStart, Vector3.down);

        if (Physics.Raycast(downRay, out RaycastHit hit, 200f))
        {
            if (segmentBridges.Contains(hit.collider.gameObject) &&
                !snappedSegments.Contains(hit.collider.gameObject))
            {
                currentlyDraggingSegment = hit.collider.gameObject;
                segmentStartPos = currentlyDraggingSegment.transform.position;

                dragOffset = currentlyDraggingSegment.transform.position - hit.point;
            }
        }
    }

    // DRAGGING
    if (currentlyDraggingSegment != null && Input.GetMouseButton(0))
    {
        Vector3 newPos = mouseWorldXZ + dragOffset;
        newPos.y = segmentStartPos.y;   // Keep raised height
        currentlyDraggingSegment.transform.position = newPos;
    }

    // RELEASE
    if (Input.GetMouseButtonUp(0) && currentlyDraggingSegment != null)
    {
        Vector3 droppedPos = currentlyDraggingSegment.transform.position;
        droppedPos.y -= raiseHeight;
        currentlyDraggingSegment.transform.position = droppedPos;

        Vector3 targetPos = transform.position;

        Vector3 droppedXZ = new(droppedPos.x, 0, droppedPos.z);
        Vector3 targetXZ = new(targetPos.x, 0, targetPos.z);

        float dist = Vector3.Distance(droppedXZ, targetXZ);

        if (dist < snapDistance)
        {
            currentlyDraggingSegment.transform.localPosition = GlobalVariables.bridgeSegmentOriginalPositions[currentlyDraggingSegment];
            snappedSegments.Add(currentlyDraggingSegment);
            currentlyDraggingSegment.GetComponent<SplineEditor>().percentage_transparent = 0f; // Make segment fully visible
            CheckPuzzleCompletion();
        }
        else
        {
            droppedPos.y += raiseHeight;
            currentlyDraggingSegment.transform.position = droppedPos;
        }

        currentlyDraggingSegment = null;
    }

    return;

    NormalBridgeLogic:

    if (bridgeActive != (targetBridgeActive ? 1f : 0f)) 
    {
        timer += Time.deltaTime * speed;
        bridgeActive = Mathf.Lerp(
            1 - (targetBridgeActive ? 1 : 0),
            targetBridgeActive ? 1 : 0,
            Mathf.Round(timer * 10f) * .1f
        );
    }

    if (timer >= 1 || bridgeActive == (targetBridgeActive ? 1 : 0)) 
    {
        bridgeActive = targetBridgeActive ? 1 : 0;
        timer = 0;
    }

    spline.percentage_transparent = 1 - bridgeActive;
    }


    private void CheckPuzzleCompletion()
    {
        if (segmentBridges.Count == 0)
            return;

        int snappedCount = 0;
        
        foreach (var segment in segmentBridges)
        {
            if (segment == null)
                return;

            if (snappedSegments.Contains(segment))
            {
                snappedCount++;
            }
        }

        if (snappedCount == segmentBridges.Count)
            ExitPuzzleMode();
        else
        {
            bridgeSnap.pitch = Random.Range(.9f, 1.1f);
            bridgeSnap.Play();
        }
    }

    public bool IsInPuzzleMode()
    {
        return inPuzzleMode;
    }
}
