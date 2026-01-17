using UnityEngine;

public class DynamicWater : MonoBehaviour
{
    [SerializeField] Material WaterMaterial;

    public float windDir;
    public float windSpeed;

    private void Update()
    {
        //Change texture offset (of albedo, normal, etc) to mimic moving water
        WaterMaterial.mainTextureOffset += Time.deltaTime * windSpeed * new Vector2(Mathf.Cos(windDir), Mathf.Sin(windDir));
    }
}
