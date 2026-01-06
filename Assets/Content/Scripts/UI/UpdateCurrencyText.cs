using UnityEngine;
using TMPro;

public class UpdateCurrencyText : MonoBehaviour
{
    [SerializeField] TMP_Text BuidingBlocksTXT, CoinsTXT, DiamondsTXT;

    private void Update()
    {
        BuidingBlocksTXT.text = GlobalVariables.m_Blocks.ToString();
        CoinsTXT.text         = GlobalVariables.m_Coins.ToString();
        DiamondsTXT.text      = GlobalVariables.m_Diamonds.ToString();
    }
}
