using UnityEngine;
using TMPro;

public class UpdateCurrencyText : MonoBehaviour
{
    [SerializeField] TMP_Text BuidingBlocksTXT, CoinsTXT, DiamondsTXT;

    private void Update()
    {
        uint blocks = GlobalVariables.m_Blocks;
        BuidingBlocksTXT.text = (blocks > 999) ? "-" : blocks.ToString();
        CoinsTXT.text         = GlobalVariables.m_Coins.ToString();
    }
}
