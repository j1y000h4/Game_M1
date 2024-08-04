using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Managers : MonoBehaviour
{
    // 단 한번만 초기화 하겠다는 flag
    public static bool Initialized { get; set; } = false;

    // 싱글톤
    private static Managers s_instance;
    private static Managers Instance { get { Init(); return s_instance; } }

    #region Contents
    private GameManager _gameManager = new GameManager();
    private ObjectManager _objectManager = new ObjectManager();
    private MapManager _mapManager = new MapManager();

    public static GameManager gameManager { get { return Instance?._gameManager; } }
    public static ObjectManager objectManager { get { return Instance?._objectManager; } }
    public static MapManager mapManager { get { return Instance?._mapManager; } }
    #endregion

    #region Core
    private DataManager _dataManager = new DataManager();
    private PoolManager _poolManager = new PoolManager();
    private ResourceManager _resourceManager = new ResourceManager();
    private SceneManagerEx _sceneManager = new SceneManagerEx();
    private SoundManager _soundManager = new SoundManager();
    private UIManager _uiManager = new UIManager();

    public static DataManager dataManager { get { return Instance?._dataManager; } }
    public static PoolManager poolManager { get {return Instance?._poolManager; } }
    public static ResourceManager resourceManager { get { return Instance?._resourceManager; } }
    public static SceneManagerEx sceneManager { get {return Instance?._sceneManager; } }
    public static SoundManager soundManager { get {return Instance?._soundManager; } }
    public static UIManager uiManager { get { return Instance?._uiManager; } }
    #endregion

    public static void Init()
    {
        if(s_instance == null && Initialized == false)
        {
            Initialized = true;

            GameObject go = GameObject.Find("@Managers");
            if(go == null)
            {
                go = new GameObject { name = "@Mangaers" };
                go.AddComponent<Managers>();
            }
            DontDestroyOnLoad(go);

            // 초기화
            s_instance = go.GetComponent<Managers>();
        }
    }

    #region Skill 판정
    // 스킬 판정과 관련된 부분
    public List<Creature> FindConeRangeTargets(Creature owner, Vector3 dir, float range, int angleRange, bool isAllies = false)
    {
        List<Creature> targets = new List<Creature>();
        List<Creature> ret = new List<Creature>();

        // 아군? or 적군?
        ECreatureType targetType = Util.DetermineTargetType(owner.CreatureType, isAllies);

        if (targetType == ECreatureType.Monster)
        {
            var objs = Managers.mapManager.GatherObjects<Monster>(owner.transform.position, range, range);
            targets.AddRange(objs);
        }
        else if (targetType == ECreatureType.Hero)
        {
            var objs = Managers.mapManager.GatherObjects<Hero>(owner.transform.position, range, range);
            targets.AddRange(objs);
        }

        foreach (var target in targets)
        {
            // 1. 거리안에 있는지 확인
            var targetPos = target.transform.position;
            float distance = Vector3.Distance(targetPos, owner.transform.position);

            if (distance > range)
                continue;

            // 2. 각도 확인
            if (angleRange != 360)
            {
                BaseObject ownerTarget = (owner as Creature).Target;

                // 2. 부채꼴 모양 각도 계산
                float dot = Vector3.Dot((targetPos - owner.transform.position).normalized, dir.normalized);
                float degree = Mathf.Rad2Deg * Mathf.Acos(dot);

                if (degree > angleRange / 2f)
                    continue;
            }

            ret.Add(target);
        }

        return ret;
    }

    #endregion
}
