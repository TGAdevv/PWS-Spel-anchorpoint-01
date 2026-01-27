using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

struct Train
{
    public GameObject TrainOBJ;
    public float t;
    public float Duration;
    public SplineEditor Spline;

    public Train (GameObject trainOBJ, float _t, SplineEditor spline, float duration)
    {
        TrainOBJ = trainOBJ;
        t = _t;
        Spline = spline;
        Duration = duration;
    }

    public static bool operator !(Train train)
    {
        return train.TrainOBJ == null;
    }
}

public class ShowLevelFinishedAnimation : MonoBehaviour
{
    public UnityEvent OnAnimationFinished;
    public UnityEvent OnAnimationStart;

    public TMP_Text CoinRewardTXT;
    public CheckIfLevelFinished checkIfLevelFinished;

    public GameObject TrainPrefab;
    public float AnimDuration;

    [Header("Index 0 -> hide all stars\n Index 1 -> show star 1\n Index 2 -> show star 2\n etc")]
    public UnityEvent[] ShowStar;

    //                                     0 Stars->0 coins, 1 Star->2 coins etc
    readonly uint[] CoinRewardPerStarCount = new uint[4] { 0, 2, 3, 5 };

    public IEnumerator LevelFinished(int StarCount)
    {
        ShowStar[0].Invoke();
        for (int i = StarCount; i >= 1; i--) {
            ShowStar[i].Invoke(); 
        }

        Vector3 originalTrainScale = TrainPrefab.transform.localScale;

        // Play animation here

        OnAnimationStart.Invoke();

        if (GlobalVariables.m_LevelGoal == LevelGoal.CONNECT_ALL_ISLANDS)
        {
            SceneWidgets.ClearAll();
            List<Train> TrainOBJs = new(GlobalVariables.bridgeObjects.Count);

            float maxCurveLength = 0;

            foreach (var bridgeOBJ in GlobalVariables.bridgeObjects)
            {
                SplineEditor splineScript = bridgeOBJ.GetComponent<SplineEditor>();

                if (splineScript.curveLength > maxCurveLength)
                    maxCurveLength = splineScript.curveLength;
            }

            for (int i = GlobalVariables.bridgeObjects.Count - 1; i >= 0; i--)
            {
                if (!GlobalVariables.bridgeObjects[i])
                {
                    Debug.LogWarning("found empty obj");
                    continue;
                }

                if (GlobalVariables.possibleBridges.Length - 1 >= i)
                    if (!GlobalVariables.possibleBridges[i].activated)
                    {
                        Destroy(GlobalVariables.bridgeObjects[i]);
                        continue;
                    }

                TrainOBJs.Add(new(
                    Instantiate(TrainPrefab), // Train Prefab
                    0f, // t-value (goes from 0-1 starting at 0)
                    GlobalVariables.bridgeObjects[i].GetComponent<SplineEditor>(), // Spline script of the bridge
                    GlobalVariables.bridgeObjects[i].GetComponent<SplineEditor>().curveLength / maxCurveLength * AnimDuration // Duration of the animation
                ));
            }

            bool AnimationDone = false;
            while (!AnimationDone)
            {
                AnimationDone = true;
                for (int i = 0; i < TrainOBJs.Count; i++)
                {
                    if (TrainOBJs[i].t >= 1 || !TrainOBJs[i])
                        continue;

                    Train train = TrainOBJs[i];

                    AnimationDone = false;
                    train.t = Mathf.Min(train.t + (Time.deltaTime / train.Duration), 1);

                    Vector3 p = train.Spline.SamplePointInCurve(train.t);
                    Vector3 pDeriv = train.Spline.SamplePointInCurve(train.t + train.Spline.d) - p;

                    train.TrainOBJ.transform.SetPositionAndRotation(p, Quaternion.LookRotation(pDeriv));
                    train.TrainOBJ.transform.localScale = originalTrainScale * (1 - 16 * Mathf.Pow(train.t - .5f, 4));
                    TrainOBJs[i] = train;
                }
                yield return new WaitForEndOfFrame();
            }
            for (int i = 0; i < TrainOBJs.Count; i++)
            {
                if (!TrainOBJs[i])
                    continue;

                Destroy(TrainOBJs[i].TrainOBJ);
            }
        }

        CoinRewardTXT.text = CoinRewardPerStarCount[StarCount].ToString();
        GlobalVariables.m_Coins += CoinRewardPerStarCount[StarCount];
        OnAnimationFinished.Invoke();
        yield return new();
    }
}