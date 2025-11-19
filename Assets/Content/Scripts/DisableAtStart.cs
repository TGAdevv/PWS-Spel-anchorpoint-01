using UnityEngine;

public class DisableAtStart : MonoBehaviour
{
    void Start()
    {
        if (Time.frameCount < 15)
            gameObject.SetActive(false);
    }
}
