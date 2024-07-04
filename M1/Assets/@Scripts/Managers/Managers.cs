using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static GameManager gameManager { get { return Instance?._gameManager; } }
    public static ObjectManager objectManager { get { return Instance?._objectManager; } }
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
}
