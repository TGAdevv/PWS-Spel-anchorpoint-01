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
    public int RouteIndex;
    public int IndexInRoute;
    public SplineEditor Spline;

    public Train (GameObject trainOBJ, float _t, int routeIndex, int indexInRoute, SplineEditor spline, float duration)
    {
        TrainOBJ = trainOBJ;
        t = _t;
        RouteIndex = routeIndex;
        IndexInRoute = indexInRoute;
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
    public UnityEvent ShowUI;
    public UnityEvent OnAnimationStart;

    public TMP_Text CoinRewardTXT;
    public CheckIfLevelFinished checkIfLevelFinished;

    public GameObject TrainPrefab;
    public float AnimDuration;

    [Header("Index 0 -> hide all stars\n Index 1 -> show star 1\n Index 2 -> show star 2\n etc")]
    public UnityEvent[] ShowStar;

    //                                     0 Stars->0 coins, 1 Star->2 coins etc
    readonly uint[] CoinRewardPerStarCount = new uint[4] { 0, 2, 3, 5 };

    Train AnimateTrain(Train train, Vector3 originalTrainScale, bool inverseDir = false)
    {
        train.t = Mathf.Min(train.t + (Time.deltaTime / train.Duration), 1);

        float t = inverseDir ? 1 - train.t : train.t;

        Vector3 p = train.Spline.SamplePointInCurve(t);
        Vector3 pDeriv = train.Spline.SamplePointInCurve(t + train.Spline.d) - p;

        train.TrainOBJ.transform.SetPositionAndRotation(p, Quaternion.LookRotation(pDeriv));
        train.TrainOBJ.transform.localScale = originalTrainScale * (1 - 16 * Mathf.Pow(t - .5f, 4));

        return train;
    }

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
            List<Train> TrainOBJs = new(GlobalVariables.bridgeObjects.Count);

            float maxCurveLength = 0;

            foreach (var bridgeOBJ in GlobalVariables.bridgeObjects)
            {
                SplineEditor splineScript = bridgeOBJ.GetComponent<SplineEditor>();
                if (splineScript.curveLength > maxCurveLength)
                    maxCurveLength = splineScript.curveLength;

                ClickToCreateBridge ClickToCreate = bridgeOBJ.GetComponent<ClickToCreateBridge>();
                if (ClickToCreate)
                    Destroy(ClickToCreate);
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
                    -1, -1, // route index and index in route (not needed)
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
                    AnimationDone = false;
                    TrainOBJs[i] = AnimateTrain(TrainOBJs[i], originalTrainScale);
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
        if (GlobalVariables.m_LevelGoal == LevelGoal.OPTIMIZE_PROCESS)
        {
            List<List<Vector3Int>> Bridges = checkIfLevelFinished.AllRoutes;
            List<Train> TrainOBJs = new(Bridges.Count * 2);

            float animDurationFactor = 5f / GlobalVariables.LongestProcess;

            for (int i = 0; i < Bridges.Count; i++)
            {
                int bridgeIndex = Bridges[i][0].z;
                uint weight = GlobalVariables.possibleBridges[bridgeIndex].weight;
                weight = (weight == 0) ? GlobalVariables.SelectedWeightOption : weight;

                print("Made train object");

                TrainOBJs.Add(new(
                    Instantiate(TrainPrefab), // Train OBJ
                    0, // t-value (0-1)
                    i, // Route index
                    0, // Index in route
                    GlobalVariables.bridgeObjects[bridgeIndex].GetComponent<SplineEditor>(), // Spline script of the bridge
                    weight * animDurationFactor // Duration
                ));
            }

            bool AnimationDone = false;
            int frame = 0;
            while (!AnimationDone)
            {
                frame++;

                AnimationDone = true;
                for (int i = TrainOBJs.Count - 1; i >= 0; i--)
                {
                    if (!TrainOBJs[i])
                        continue;

                    if (TrainOBJs[i].t >= 1)
                    {
                        Train train = TrainOBJs[i];
                        if (train.IndexInRoute + 1 >= Bridges[train.RouteIndex].Count)
                            continue;

                        train.IndexInRoute++;
                        train.t = 0;

                        int bridgeIndex = Bridges[train.RouteIndex][train.IndexInRoute].z;
                        uint weight = GlobalVariables.possibleBridges[bridgeIndex].weight;
                        weight = (weight == 0) ? GlobalVariables.SelectedWeightOption : weight;

                        train.Spline = GlobalVariables.bridgeObjects[bridgeIndex].GetComponent<SplineEditor>();
                        train.Duration = weight * animDurationFactor;

                        TrainOBJs[i] = train;
                    }

                    AnimationDone = false;
                    Vector3Int bridgeInfo = Bridges[TrainOBJs[i].RouteIndex][TrainOBJs[i].IndexInRoute];
                    TrainOBJs[i] = AnimateTrain(TrainOBJs[i], originalTrainScale, bridgeInfo.x != GlobalVariables.possibleBridges[bridgeInfo.z].startIsland);
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

        ShowUI.Invoke();

        if (StarCount == -1)
        {
            print("Play on level not finished");
            checkIfLevelFinished.OnLevelNotFinished.Invoke();
            yield break;
        }

        CoinRewardTXT.text = CoinRewardPerStarCount[StarCount].ToString();
        GlobalVariables.m_Coins += CoinRewardPerStarCount[StarCount];
        OnAnimationFinished.Invoke();
        yield break;
    }
}