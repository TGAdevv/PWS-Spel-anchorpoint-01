using UnityEngine;

public class CheckIfLevelFinished : MonoBehaviour
{
    public GameObject LevelCompleet;
    public GameObject ster1obj;
    public GameObject ster2obj;
    public GameObject ster3obj;
    public AnimateUI LevelCompleetAnim;
    public AnimateUI BackgroundAnim;
    public AnimateUI ster1;
    public AnimateUI ster2;
    public AnimateUI ster3;
    public AudioSource LevelCompleteSound;

    public struct GraphEdge
    {
        public uint weight;
        public uint connectTo;

        public GraphEdge(uint _weight, uint _connectTo)
        {
            weight    = _weight;
            connectTo = _connectTo;
        }
    }
    public struct GraphVertex
    {
        public GraphEdge[] connections;

        public GraphVertex(GraphEdge[] _connections)
        {
            connections = _connections;
        }
        public GraphVertex(GraphEdge _connection)
        {
            connections = new GraphEdge[1] { _connection };
        }
    }

    public GraphVertex[] ActiveGraph;

    bool HasRoute(uint index)
    {
        for (int i = 0; i < ActiveGraph[index].connections.Length; i++)
        {
            if (ActiveGraph[index].connections[i].connectTo == ActiveGraph.Length - 1)
                return true;
            return HasRoute(ActiveGraph[index].connections[i].connectTo);
        }
        return false;
    }

    public bool Check()
    {
        switch (GlobalVariables.m_LevelGoal)
        {
            case LevelGoal.CONNECT_ALL_ISLANDS:
                for (int i = 0; i < ActiveGraph.Length; i++)
                    if (ActiveGraph[i].connections.Length == 0) return false;
                break;

            case LevelGoal.FIND_SHORTEST_ROUTE:
                if (!HasRoute(0))
                    return false;
                break;

            case LevelGoal.OPTIMIZE_PROCESS:

                break;

            default:
                break;
        }

        return true;
    }

    public void LevelFinished()
    {
        LevelCompleteSound.Play();
        LevelCompleet.SetActive(true);
        LevelCompleetAnim.PlayAnimation(0);
        BackgroundAnim.PlayAnimation(0);
        // first disable star gameobjects, only make them appear when earned
        ster1obj.SetActive(false);
        ster2obj.SetActive(false);
        ster3obj.SetActive(false);
        switch (GlobalVariables.m_LevelGoal)
        {
            case 0:
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks)
                {
                    ster3obj.SetActive(true);
                    ster3.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 1)
                {
                    ster2obj.SetActive(true);
                    ster2.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 2)
                {
                    ster1obj.SetActive(true);
                    ster1.PlayAnimation(0);
                }
                break;
            case LevelGoal.FIND_SHORTEST_ROUTE:
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks)
                {
                    ster3obj.SetActive(true);
                    ster3.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 1)
                {
                    ster2obj.SetActive(true);
                    ster2.PlayAnimation(0);
                }
                if (GlobalVariables.m_Blocks <= GlobalVariables.m_requiredBlocks + 2)
                {
                    ster1obj.SetActive(true);
                    ster1.PlayAnimation(0);
                }
                break;

            case LevelGoal.OPTIMIZE_PROCESS:
                ster1obj.SetActive(true);
                ster2obj.SetActive(true);
                ster3obj.SetActive(true);
                ster1.PlayAnimation(0);
                ster2.PlayAnimation(0);
                ster3.PlayAnimation(0);
                break;

            default:
                break;
        }
    }
}
