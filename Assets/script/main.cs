using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{
    public int learnMode = 1;
    public int Trylimit = 1000;
    public Material[] trailColor = new Material[2];
    public int episodeLimit = 100;
    int[] countStore;

    const int R = 1;
    const int L = 2;
    const int U = 3;
    const int D = 4;
    int[] Action = { R, L, U, D };
    int[] Status;
    Dictionary<string, int> StatusConveter = new Dictionary<string, int>();

    const double FAIL = -5;
    const double SAFE = 0;
    const double GOAL = 100;

    private int ActionCount = 0;
    private int TryCount = 0;
    
    bool rewardCatch = true;
    bool mazeEnd = true;
    bool tryEnd = false;
    bool sarsaFirst = false;

    private int nowStatus;
    private int nextStatus;
    private int nowAction;
    private int nextAction;
    private double nowReward;

    private makeMazer makeScript;
    private motion motionScript;

    const int Q = 1;
    const int S = 2;
    const int P = 3;

    private Qlearning qlearning;
    private Sarsa sarsa;
    private ProfitSharing profitsharing;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<makeMazer>().enabled = false;
        motionScript = this.GetComponent<motion>();
        makeScript = this.GetComponent<makeMazer>();

        Status = new int[makeScript.StatusSet.Length];
        countStore = new int[Trylimit];
        int i = 0;
        while (makeScript.StatusSet[i] != "END")
        {
            StatusConveter.Add(makeScript.StatusSet[i], i);
            Status[i] = i;
            i++;
        }

        qlearning = new Qlearning(0.2f, 0.99f, Status, Action);
        sarsa = new Sarsa(0.2f, 0.99f, Status, Action);
        profitsharing = new ProfitSharing(1, episodeLimit, Status, Action);

        nowStatus = StatusConveter["START"];
        TryCount = 1;

        //profitsharingのみここでイプシロン初期化
        profitsharing.EpsilonSet(1.0f / (double)(TryCount));
    }

    // Update is called once per frame
    void Update()
    {
        switch (learnMode) {
            case Q:
                qlearning.EpsilonSet(1.0f / (double)(TryCount));
                if (motionScript.animeEnd && rewardCatch && !tryEnd)
                {
                    nowAction = qlearning.SelectAction(nowStatus);
                    Ainction(Action[nowAction]);
                    ActionCount++;
                    rewardCatch = false;
                }
                else if (motionScript.animeEnd && !rewardCatch && !tryEnd)
                {
                    nextStatus = GetSituation();
                    nowReward = GetReward(nextStatus);
                    qlearning.ValueUpdate(nowStatus, nowAction, nextStatus, nowReward);
                    nowStatus = nextStatus;
                    if (nowStatus == StatusConveter["GOAL"])
                    {
                        mazeEnd = true;
                    }
                }
                break;

            case S:
                sarsa.EpsilonSet(1.0f / (double)(TryCount));
                if (motionScript.animeEnd && rewardCatch && !tryEnd && !sarsaFirst)
                {
                    nextAction = sarsa.SelectAction(nowStatus);
                    Ainction(Action[nextAction]);
                    nowAction = nextAction;
                    ActionCount++;
                    rewardCatch = false;
                }
                else if (motionScript.animeEnd && !rewardCatch && !tryEnd && !sarsaFirst)
                {
                    nextStatus = GetSituation();
                    nowReward = GetReward(nextStatus);
                    if (nextStatus == StatusConveter["GOAL"])
                    {
                        mazeEnd = true;
                    }
                    sarsaFirst = true;
                }
                else if (motionScript.animeEnd && rewardCatch && !tryEnd)
                {
                    nextAction = sarsa.SelectAction(nextStatus);
                    Ainction(Action[nextAction]);
                    ActionCount++;
                    sarsa.ValueUpdate(nowStatus, nowAction, nextStatus, nextAction, nowReward);
                    nowAction = nextAction;
                    nowStatus = nextStatus;
                    rewardCatch = false;
                }
                else if (motionScript.animeEnd && !rewardCatch && !tryEnd)
                {
                    nextStatus = GetSituation();
                    nowReward = GetReward(nextStatus);
                    if (nextStatus == StatusConveter["GOAL"])
                    {
                        mazeEnd = true;
                    }
                }
                break;

            case P:
                if (motionScript.animeEnd && rewardCatch && !tryEnd)
                {
                    nowAction = profitsharing.SelectAction(nowStatus);
                    Ainction(Action[nowAction]);
                    ActionCount++;
                    profitsharing.EpisodeUpdate(nowStatus, nowAction);
                    rewardCatch = false;
                }
                else if (motionScript.animeEnd && !rewardCatch && !tryEnd)
                {
                    nextStatus = GetSituation();
                    nowReward = GetReward(nextStatus);
                    profitsharing.ValueUpdate(nowReward);
                    
                    nowStatus = nextStatus;
                    if (nowStatus == StatusConveter["GOAL"])
                    {
                        mazeEnd = true;
                    }
                }
                break;
        }

        if (mazeEnd) TryAgain();
        if (TryCount == Trylimit) tryEnd = true;
    }

    void Ainction(int dir)
    {
        motionScript.dir = dir;
        motionScript.go = true;
    }

    double GetReward(int s)
    {
        rewardCatch = true;
        if (motionScript.MissingCollision)
        {
            return FAIL;
        }
        else if (s == StatusConveter["GOAL"])
        {
            return GOAL;
        }
        else
        {
            return SAFE;
        }
    }

    int GetSituation()
    {
        Transform myTransform = this.transform;
        Vector3 origin = myTransform.position;
        Vector3 direction = new Vector3(0, -1.0f, 0); // 方向ベクトル

        Ray ray = new Ray(origin, direction); // Ray生成
        RaycastHit hit;
        string name = null;
        if (Physics.Raycast(ray, out hit)) // 何らかのコライダー衝突
        {
            name = hit.collider.gameObject.name; // 衝突したオブジェクトの名前を取得
            if (hit.collider.gameObject.name != "START" && hit.collider.gameObject.name != "GOAL") {
                hit.collider.gameObject.GetComponent<MeshRenderer>().material = trailColor[0];
                hit.collider.gameObject.tag = "trail";
            }
        }

        return StatusConveter[name];
    }

    void TryAgain()
    {
        GameObject objContainer = GameObject.Find("START");
        Transform startTransform = objContainer.GetComponent<Transform>();
        Vector3 newCoodinater = startTransform.position;
        newCoodinater.y = 3.0f;

        Transform myTransform = this.transform;
        myTransform.position = newCoodinater;

        mazeEnd = false;
        sarsaFirst = false;
        nowStatus = StatusConveter["START"];

        GameObject[] g = GameObject.FindGameObjectsWithTag("trail");
        int g_num = g.Length;
        for (int i = 0; i < g_num; i++)
        {
            g[i].GetComponent<MeshRenderer>().material = trailColor[1];
            g[i].gameObject.tag = "Untagged";
        }

        countStore[TryCount - 1] = ActionCount;
        ActionCount = 0;
        Debug.Log("try count:" + TryCount);

        TryCount++;
    }
}
