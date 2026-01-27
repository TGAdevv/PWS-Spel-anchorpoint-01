using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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

    void Start()
    {
        spline = GetComponent<SplineEditor>();

        if (!spline) {
            Debug.LogError("Could not find SplineEditor!");
            enabled = false;
        }
    }
    private void OnMouseDown()
    {
        EventSystem currentEventSys = EventSystem.current;
        PointerEventData eventData = new(currentEventSys);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new();
        currentEventSys.RaycastAll(eventData, results);
        if (results.Count > 0)
            return;

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

    float timer = 0;

    private void Update()
    {
        if (bridgeActive != (targetBridgeActive ? 1f : 0f)) 
        {
            timer += Time.deltaTime * speed;
            bridgeActive = Mathf.Lerp(1 - (targetBridgeActive ? 1 : 0), targetBridgeActive ? 1 : 0, Mathf.Round(timer*10f)*.1f);
        }

        if (timer >= 1 || bridgeActive == (targetBridgeActive ? 1 : 0)) 
        {
            bridgeActive = targetBridgeActive ? 1 : 0;
            timer = 0;
        }

        spline.percentage_transparent = 1 - bridgeActive;
    }
}
