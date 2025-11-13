/*using System.IO;
using System.Text.Json;
using UnityEngine;

public class LoadSaveCoinsDiamonds: MonoBehaviour
{
    struct SaveFile
    {
        public uint Coins;
        public uint Diamonds;

        public SaveFile(uint c_Coins, uint c_Diamonds) 
        {
            //c_ -> constructor
            Coins = c_Coins;
            Diamonds = c_Diamonds;
        }
    };

    private void Save() 
    {
        SavedValues = new SaveFile(Currency.m_Coins, Currency.m_Diamonds);

        using (StreamWriter writetext = new StreamWriter("SaveFile.json", false))
        {
            writetext.WriteLine(JsonUtility.ToJson(SavedValues));
        }

        using StreamReader reader = new("SaveFile.json");
        string JSON_TXT = reader.ReadToEnd();
    }

    SaveFile SavedValues;
    bool doneInitializing = false;

    private void Start()
    {
        try
        {
            // Open the JSON file using a stream reader.
            using StreamReader reader = new("SaveFile.json");
            string JSON_TXT = reader.ReadToEnd();

            //Load Coin/Diamonds values from file
            SavedValues = JsonUtility.FromJson<SaveFile>(JSON_TXT);
            Currency.m_Coins = SavedValues.Coins;
            Currency.m_Diamonds = SavedValues.Diamonds;
        }
        catch
        {
            // File doesn't exist yet->create it
            Save();
        }

        doneInitializing = true;
    }

    private void Update()
    {
        if (!doneInitializing)
            return;

        if (SavedValues.Coins != Currency.m_Coins || SavedValues.Diamonds != Currency.m_Diamonds)
            Save();
    }
}
*/