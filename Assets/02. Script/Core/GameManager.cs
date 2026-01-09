using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
GameManager는게임전역상태와부트플로우를관리하는MonoBehaviour컴포넌트다.
-부트에서데이터초기화를시도하고부트화면을최소시간표시한다.
-데이터에셋이없으면초기화를스킵하고플로우는계속진행한다.
-씬전환직전에로딩UI를먼저표시해전환중로딩화면이반드시보이도록한다.
-외부에서LoadLobby/LoadGame을호출해씬전환을요청할수있다.
-외부에서Pokedex로도감서비스에접근할수있다.
*/
[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }//싱글톤

    [Header("Scenes")]
    [SerializeField] private string bootSceneName = "01. BootScene";//부트씬이름
    [SerializeField] private string lobbySceneName = "02. LobbyScene";//로비씬이름
    [SerializeField] private string gameSceneName = "03. GameScene";//게임씬이름

    [Header("BootFlow")]
    [SerializeField] private bool autoEnterLobbyFromBoot = true;//부트자동진입
    [SerializeField] private float bootMinVisibleSeconds = 3f;//부트최소표시시간
    [SerializeField] private float bootExtraRandomSeconds = 2f;//부트추가랜덤(0~N)
    [SerializeField] private bool enableDebug = true;//디버그로그토글

    [Header("Data")]
    [SerializeField] private PokemonDatabaseSO pokemonDatabase;//도감DB

    [Header("Managers")]
    [SerializeField] private SceneLoader sceneLoader;//씬로더
    [SerializeField] private PokedexService pokedexService;//도감서비스

    public SceneLoader SceneLoader => sceneLoader;//외부접근
    public PokedexService Pokedex => pokedexService;//외부접근

    private bool isBootstrapped;//부트중복방지
    private bool isTransitioning;//전환중중복방지

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureCoreComponents();
        Dbg("//GameManager Awake");
    }

    private void Start()
    {
        if (isBootstrapped)
        {
            return;
        }

        isBootstrapped = true;
        StartCoroutine(BootstrapRoutine());
    }

    //외부호출용
    public void LoadLobby()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(lobbySceneName));
    }

    //외부호출용
    public void LoadGame()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(gameSceneName));
    }

    private void EnsureCoreComponents()
    {
        if (sceneLoader == null)
        {
            sceneLoader = GetComponent<SceneLoader>();
        }

        if (sceneLoader == null)
        {
            sceneLoader = gameObject.AddComponent<SceneLoader>();
        }

        if (pokedexService == null)
        {
            pokedexService = GetComponent<PokedexService>();
        }

        if (pokedexService == null)
        {
            pokedexService = gameObject.AddComponent<PokedexService>();
        }
    }

    private IEnumerator BootstrapRoutine()
    {
        Scene active = SceneManager.GetActiveScene();
        if (!active.IsValid() || active.name != bootSceneName)
        {
            yield break;
        }

        Dbg($"//Bootstrap begin scene={active.name}");


        float startedAt = Time.unscaledTime;

        if (pokemonDatabase == null)
        {
            Dbg("//PokemonDatabaseSO missing,skip init");
        }
        else
        {
            if (pokedexService != null)
            {
                Dbg("//PokedexService Initialize");
                pokedexService.Initialize(pokemonDatabase);
            }
        }

        float minSeconds = bootMinVisibleSeconds;
        if (bootExtraRandomSeconds > 0f)
        {
            float add = Random.Range(0f, bootExtraRandomSeconds);
            minSeconds += add;
            Dbg($"//Boot extraRandom={add:0.000}");
        }

        float elapsed = Time.unscaledTime - startedAt;
        float remain = minSeconds - elapsed;
        Dbg($"//Boot wait remain={remain:0.000}");

        if (remain > 0f)
        {
            yield return new WaitForSecondsRealtime(remain);
        }

        if (!autoEnterLobbyFromBoot)
        {
            Dbg("//AutoEnter disabled");
            yield break;
        }

        yield return EnterLobbyFromBootRoutine();
    }

    private IEnumerator EnterLobbyFromBootRoutine()
    {
        if (isTransitioning)
        {
            yield break;
        }

        isTransitioning = true;

        EnsureCoreComponents();
        if (sceneLoader == null)
        {
            Debug.LogError("//SceneLoader missing");
            isTransitioning = false;
            yield break;
        }

        Dbg("//EnterLobby showLoading");
        if (LoadingController.Instance != null)
        {
            LoadingController.Instance.Show();
            yield return null;//로딩UI그릴프레임양보
        }

        Scene active = SceneManager.GetActiveScene();
        Dbg($"//EnterLobby active={active.name}");

        if (active.IsValid() && active.name == bootSceneName)
        {
            yield return sceneLoader.LoadSceneReplaceActiveAsync(lobbySceneName);
        }
        else
        {
            yield return sceneLoader.LoadSceneAsync(lobbySceneName);
        }

        isTransitioning = false;
        Dbg("//EnterLobby done");
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        EnsureCoreComponents();

        if (sceneLoader == null)
        {
            Debug.LogError("//SceneLoader missing");
            yield break;
        }

        if (sceneLoader.IsLoading)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("//SceneName empty");
            yield break;
        }

        isTransitioning = true;

        Dbg($"//LoadSceneRoutine showLoading name={sceneName}");
        if (LoadingController.Instance != null)
        {
            LoadingController.Instance.Show();
            yield return null;//로딩UI그릴프레임양보
        }

        yield return sceneLoader.LoadSceneAsync(sceneName);

        isTransitioning = false;
        Dbg($"//LoadSceneRoutine done name={sceneName}");
    }

    private void Dbg(string msg)
    {
        if (!enableDebug)
        {
            return;
        }

        Debug.Log(msg);
    }
}