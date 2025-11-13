public static class Currency
{
    // ------------------------------
    //  Currency->VARIABLES
    // ------------------------------
    public static uint m_Coins = 0;
    public static uint m_Diamonds = 0;

    // ------------------------------
    //  Currency->FUNCTIONS
    // ------------------------------
    public static bool PurchaseWithCoins(uint amount) 
    {
        if (m_Coins < amount)
            return false;

        m_Coins -= amount;
        return true;
    }
    public static bool PurchaseWithDiamonds(uint amount)
    {
        if (m_Diamonds < amount)
            return false;

        m_Diamonds -= amount;
        return true;
    }
    // =======================================
}
