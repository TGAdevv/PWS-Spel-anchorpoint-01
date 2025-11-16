using UnityEngine;

public class Hide_Show : MonoBehaviour
{
    public void toggleHideShow() 
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
