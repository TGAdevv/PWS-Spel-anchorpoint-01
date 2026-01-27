using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public void Switch(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
