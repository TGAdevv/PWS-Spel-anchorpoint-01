using UnityEngine;
using UnityEngine.Audio;

public class SoundMixer : MonoBehaviour
{
    const float MaxFrequencyLP = 22000;

    [Range(10, 22000)]
    public float TargetFrequencyLP;
    [Range(0, 3)]
    public float TransistionDuration;

    public AudioMixer Mixer;
    float lp_freq;

    float target_freq;

    public void DisableLowPass()
    {
        target_freq = MaxFrequencyLP;
    }
    public void EnableLowPass()
    {
        target_freq = TargetFrequencyLP;
    }

    private void Start()
    {
        target_freq = MaxFrequencyLP;
    }
    private void Update()
    {
        Mixer.GetFloat("LP_Freq", out lp_freq);

        if (lp_freq != target_freq)
        {
            Mixer.SetFloat("LP_Freq", Mathf.Clamp(
                // Value
                lp_freq +
                (MaxFrequencyLP - TargetFrequencyLP)
                * Time.deltaTime / TransistionDuration
                * Mathf.Sign(target_freq - lp_freq),
            // Min and Max bounds
            lp_freq > target_freq ? target_freq : 0,
            lp_freq < target_freq ? target_freq : MaxFrequencyLP));
        }
    }
}
