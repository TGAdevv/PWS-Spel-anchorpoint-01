using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClickToCreateBridge : MonoBehaviour
{
    SplineEditor spline;
    float bridgeActive = 0;
    float targetBridgeActive = 0;
    float speed = 3;

    public uint[] price;

    public Transform CameraCanvas;
    public GameObject MenuPricePrefab;
    public GameObject MenuPriceVarPrefab;

    RectTransform rect;
    TMP_Text priceTXT;

    Vector3 PricePoint;

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
            TMP_Dropdown DropdownPrices = PurchaseBlock.GetComponent<TMP_Dropdown>();

            TMP_Dropdown.OptionData[] optionData = new TMP_Dropdown.OptionData[price.Length];
            for (int i = 0; i < price.Length; i++)
                optionData[i] = new(price[i].ToString());

            DropdownPrices.AddOptions(new List<TMP_Dropdown.OptionData>(optionData));
        }

        priceTXT.text = price[0].ToString();

        PricePoint = spline.SamplePointInCurve(spline.trTOta[Mathf.RoundToInt(spline.resolution * .5f)]) + spline.transform.position;
    }
    private void OnMouseDown()
    {
        uint _price = (price.Length == 1) ? price[0] : uint.Parse(priceTXT.text);

        if (targetBridgeActive == 0 && GlobalVariables.m_Blocks < _price)
            return;

        EventSystem currentEventSys = EventSystem.current;
        PointerEventData eventData = new(currentEventSys);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new();
        currentEventSys.RaycastAll(eventData, results);
        if (results.Count > 0)
            return;

        GlobalVariables.m_Blocks -= _price;

        if (targetBridgeActive == 1)
            GlobalVariables.m_Blocks += _price * 2;

        targetBridgeActive = 1 - targetBridgeActive;
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
