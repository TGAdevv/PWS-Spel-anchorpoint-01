using UnityEngine;
using TMPro;

public class UpdateCurrencyText : MonoBehaviour
{
    [SerializeField] TMP_Text BuidingBlocksTXT, CoinsTXT, DiamondsTXT;

    private void Update()
    {
        BuidingBlocksTXT.text = Currency.m_Blocks.ToString();
        CoinsTXT.text = Currency.m_Coins.ToString();
        DiamondsTXT.text = Currency.m_Diamonds.ToString();
    }
}
