using UnityEngine;

public class ImportPreview : MonoBehaviour
{
    LevelImporter levelImporter;
    [SerializeField] IslandEditor islandEditor;

    public void Import()
    {
        if (!levelImporter)
            levelImporter = GetComponent<LevelImporter>();

        string levelCode = islandEditor.ExportLevel(false, false);

        print("code is " + levelCode);

        levelImporter.levels = new string[1] { levelCode };
        levelImporter.ImportLevel(0, true);
    }
}
