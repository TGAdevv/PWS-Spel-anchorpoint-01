using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ClickToCreateBridge : MonoBehaviour
{
    SplineEditor spline;
    float bridgeActive = 0;
    float targetBridgeActive = 0;
    float speed = 3;
    uint selectedPrice;

    public uint[] price;
    public int startIsland;
    public int endIsland;

    public RectTransform CameraCanvas;
    public GameObject MenuPricePrefab;
    public GameObject MenuPriceVarPrefab;

    RectTransform rect;
    TMP_Text priceTXT;

    Vector3 PricePoint;
    void OnPriceChanged(int index)
    {
        selectedPrice = price[index];
        GlobalVariables.SelectedWeightOption = selectedPrice;
        Debug.Log("Selected price updated to: " + GlobalVariables.SelectedWeightOption);
    }
    void Start()
    {
        spline = GetComponent<SplineEditor>();

        if (!spline) {
            Debug.LogError("Could not find SplineEditor!");
            enabled = false;
        }

        GameObject PurchaseBlock = Instantiate((price.Length == 1) ? MenuPricePrefab : MenuPriceVarPrefab, CameraCanvas);
        rect = PurchaseBlock.GetComponent<RectTransform>();
        priceTXT = PurchaseBlock.GetComponentInChildren<TMP_Text>();

        if (price.Length > 1)
        {
            uint selectedPrice = price[0];
            TMP_Dropdown DropdownPrices = PurchaseBlock.GetComponent<TMP_Dropdown>();

            TMP_Dropdown.OptionData[] optionData = new TMP_Dropdown.OptionData[price.Length];
            for (int i = 0; i < price.Length; i++)
                optionData[i] = new(price[i].ToString());

            DropdownPrices.AddOptions(new List<TMP_Dropdown.OptionData>(optionData));
            selectedPrice = price[DropdownPrices.value];
            DropdownPrices.onValueChanged.AddListener(OnPriceChanged);
        }

        priceTXT.text = price[0].ToString();

        PricePoint = spline.SamplePointInCurve(spline.trTOta[Mathf.RoundToInt(spline.resolution * .5f)]) + spline.transform.position;
    }
    private void OnMouseDown()
    {
        uint _price = (price.Length == 1) ? price[0] : uint.Parse(priceTXT.text);
        EventSystem currentEventSys = EventSystem.current;
        PointerEventData eventData = new(currentEventSys);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new();
        currentEventSys.RaycastAll(eventData, results);
        if (results.Count > 0)
            return;

        GlobalVariables.m_Blocks += _price;

        if (targetBridgeActive == 1)
            GlobalVariables.m_Blocks -= _price * 2;

        targetBridgeActive = 1 - targetBridgeActive;

        //check for bridge in globalvariables and update accordingly
        if (targetBridgeActive == 1)
        {
            for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
            {
                string CurrentBridge = GlobalVariables.possibleBridges[i];
                string WeightOfBridge = CurrentBridge.Split(',')[2];
                if (GlobalVariables.possibleBridges[i] == startIsland + "," + endIsland + "," + WeightOfBridge + ",0" || GlobalVariables.possibleBridges[i] == endIsland + "," + startIsland + "," + WeightOfBridge + ",0")
                {
                    GlobalVariables.possibleBridges[i] = startIsland + "," + endIsland + ",1";
                    return;
                }
            }
        }
        else
        {
            for (int i = 0; i < GlobalVariables.possibleBridges.Length; i++)
            {
                string CurrentBridge = GlobalVariables.possibleBridges[i];
                string WeightOfBridge = CurrentBridge.Split(',')[2];
                if (GlobalVariables.possibleBridges[i] == startIsland + "," + endIsland + "," + WeightOfBridge + ",1" || GlobalVariables.possibleBridges[i] == endIsland + "," + startIsland + "," + WeightOfBridge + ",1")
                {
                    GlobalVariables.possibleBridges[i] = startIsland + "," + endIsland + "," + WeightOfBridge + ",0";
                    return;
                }
            }
        }
    }

    float timer = 0;

    private void Update()
    {
        if (bridgeActive != targetBridgeActive) 
        {
            timer += Time.deltaTime * speed;
            bridgeActive = Mathf.Lerp(1 - targetBridgeActive, targetBridgeActive, timer);
        }

        if (timer > 1) 
        {
            bridgeActive = targetBridgeActive;
            timer = 0;
        }

        spline.percentage_transparent = 1 - bridgeActive;

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(PricePoint);
        screenPoint = new Vector2(screenPoint.x / (float)Screen.width, screenPoint.y / (float)Screen.height);

        rect.anchorMax = screenPoint;
        rect.anchorMin = screenPoint;
    }
}
