using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigureWeightOptions : MonoBehaviour
{
    public RectTransform[] WeightOptionTransforms;
    public Button[] DeleteButtons;

    public Connection CurrentConnection;

    public void RefreshWeights()
    {
        List<int> weights = LevelEditorParameters.currentConnection.weightOptions;

        for (int i = 0; i < WeightOptionTransforms.Length; i++)
        {
            WeightOptionTransforms[i].gameObject.SetActive(i < weights.Count);
            if (i < weights.Count)
                WeightOptionTransforms[i].GetChild(0).GetComponent<TMPro.TMP_Text>().text = weights[i].ToString();
        }
    }
    public void DeleteWeight(int index)
    {
        CurrentConnection.weightOptions.RemoveAt(index);
        LevelEditorParameters.currentConnection = CurrentConnection;
        RefreshWeights();
    }
}
