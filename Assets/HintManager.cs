using UnityEngine;
using UnityEngine.Events;

public class HintManager : MonoBehaviour
{
    public UnityEvent OnHintBoughtSuccesful;

    private void Start()
    {
        GlobalVariables.m_Coins = 5;
    }

    public void GenerateHint()
    {

        if (!GlobalVariables.PurchaseWithCoins(5))
        {
            Debug.LogWarning("User tried to buy hints without sufficient coins");
            return;
        }
        OnHintBoughtSuccesful.Invoke();
        print("Generate hint!");
    }
}
