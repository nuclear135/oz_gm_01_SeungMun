using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
UIController는전역UI흐름을관리하는MonoBehaviour컴포넌트다.
-씬안의UIRoot를UIController자식으로편입시켜DontDestroyOnLoad로영구화한다.
-kind별UISceneCanvas를등록하고,씬로드시해당UI루트를선택해보여준다.
-Loading은항상독립적으로켜지고,다른루트전환에의해꺼지지않는다.
*/
[DefaultExecutionOrder(-900)]
public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }//싱글톤

    [SerializeField] private string uiRootName = "UIRoot";//씬내UIRoot이름
    [SerializeField] private int baseOrder = 0;//기본정렬
    [SerializeField] private int loadingOrder = 5000;//로딩정렬
    [SerializeField] private float minLoadingVisibleSeconds = 0.25f;//로딩최소표시
    [SerializeField] private bool enableDebug = true;//디버그로그토글

    private Transform uiRoot;//영구UI루트
    private readonly Dictionary<UISceneCanvasKind, UISceneCanvas> canvases = new Dictionary<UISceneCanvasKind, UISceneCanvas>();//등록캐시
    private float loadingShownAtUnscaled = -999f;//로딩표시시각
    private Coroutine hideLoadingRoutine;//로딩숨김코루틴

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        AdoptOrCreateUIRoot();
        RebuildRegistry();

        HideLoadingImmediate();
        ApplyCanvasForScene(SceneManager.GetActiveScene());

        Dbg($"//UIController Awake uiRoot={(uiRoot != null ? uiRoot.name : "null")},activeScene={SceneManager.GetActiveScene().name}");
        DumpRegistry("//UIController Registry");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    public void RebuildRegistry()
    {
        AutoRegisterCanvases();
        ApplySortingOrders();
    }

    public void ShowBoot()
    {
        Dbg("//ShowBoot");
        SetExclusive(UISceneCanvasKind.Boot);
    }

    public void ShowLoading()
    {
        Dbg("//ShowLoading called");

        if (!canvases.TryGetValue(UISceneCanvasKind.Loading, out UISceneCanvas loading) || loading == null)
        {
            Debug.LogError("//ShowLoading failed:Loading canvas not registered");
            DumpRegistry("//ShowLoading Registry");
            return;
        }

        if (hideLoadingRoutine != null)
        {
            StopCoroutine(hideLoadingRoutine);
            hideLoadingRoutine = null;
            Dbg("//ShowLoading stopHideRoutine");
        }

        loadingShownAtUnscaled = Time.unscaledTime;

        loading.gameObject.SetActive(true);
        loading.InitializeAll(SceneManager.GetActiveScene());

        ApplySortingOrders();

        DumpCanvasState(loading, "//ShowLoading state");
        DumpRegistry("//ShowLoading Registry");
    }

    public void HideLoading()
    {
        Dbg("//HideLoading called");

        if (hideLoadingRoutine != null)
        {
            StopCoroutine(hideLoadingRoutine);
            hideLoadingRoutine = null;
            Dbg("//HideLoading stopPrevRoutine");
        }

        hideLoadingRoutine = StartCoroutine(HideLoadingAfterMin());
    }

    private IEnumerator HideLoadingAfterMin()
    {
        float elapsed = Time.unscaledTime - loadingShownAtUnscaled;
        float remain = minLoadingVisibleSeconds - elapsed;

        Dbg($"//HideLoading wait remain={remain:0.000}");

        if (remain > 0f)
        {
            yield return new WaitForSecondsRealtime(remain);
        }

        HideLoadingImmediate();
        ApplySortingOrders();
        hideLoadingRoutine = null;

        if (canvases.TryGetValue(UISceneCanvasKind.Loading, out UISceneCanvas loading) && loading != null)
        {
            DumpCanvasState(loading, "//HideLoading state");
        }
    }

    private void HideLoadingImmediate()
    {
        if (canvases.TryGetValue(UISceneCanvasKind.Loading, out UISceneCanvas loading) && loading != null)
        {
            loading.gameObject.SetActive(false);
            Dbg("//HideLoadingImmediate setActiveFalse");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Dbg($"//OnSceneLoaded name={scene.name},mode={mode}");

        AdoptOrCreateUIRoot();
        RebuildRegistry();
        ApplyCanvasForScene(scene);

        foreach (KeyValuePair<UISceneCanvasKind, UISceneCanvas> kv in canvases)
        {
            UISceneCanvas c = kv.Value;
            if (c == null)
            {
                continue;
            }

            c.InitializeAll(scene);
        }

        DumpRegistry("//OnSceneLoaded Registry");
    }

    private void ApplyCanvasForScene(Scene scene)
    {
        if (!scene.IsValid())
        {
            return;
        }

        string name = scene.name;

        if (name.Contains("Boot"))
        {
            Dbg("//ApplyCanvasForScene Boot");
            SetExclusive(UISceneCanvasKind.Boot);
            return;
        }

        if (name.Contains("Lobby"))
        {
            Dbg("//ApplyCanvasForScene Lobby");
            SetExclusive(UISceneCanvasKind.Lobby);
            return;
        }

        if (name.Contains("Game"))
        {
            Dbg("//ApplyCanvasForScene Game");
            SetExclusive(UISceneCanvasKind.Game);
            return;
        }
    }

    private void SetExclusive(UISceneCanvasKind activeKind)
    {
        Dbg($"//SetExclusive active={activeKind}");

        foreach (KeyValuePair<UISceneCanvasKind, UISceneCanvas> kv in canvases)
        {
            UISceneCanvasKind kind = kv.Key;
            UISceneCanvas canvas = kv.Value;

            if (canvas == null)
            {
                continue;
            }

            if (kind == UISceneCanvasKind.Loading)
            {
                continue;
            }

            bool isActive = kind == activeKind;
            canvas.gameObject.SetActive(isActive);

            if (isActive)
            {
                canvas.InitializeAll(SceneManager.GetActiveScene());
            }
        }
    }

    private void AdoptOrCreateUIRoot()
    {
        if (uiRoot != null)
        {
            return;
        }

        GameObject found = GameObject.Find(uiRootName);
        if (found != null)
        {
            uiRoot = found.transform;
            uiRoot.SetParent(transform, true);
            Dbg($"//AdoptUIRoot found={uiRoot.name}");
            return;
        }

        GameObject created = new GameObject(uiRootName);
        uiRoot = created.transform;
        uiRoot.SetParent(transform, false);
        Debug.LogWarning("//UIRoot not found,created empty UIRoot");
    }

    private void AutoRegisterCanvases()
    {
        canvases.Clear();

        if (uiRoot == null)
        {
            Dbg("//AutoRegisterCanvases uiRoot null");
            return;
        }

        UISceneCanvas[] found = uiRoot.GetComponentsInChildren<UISceneCanvas>(true);
        for (int i = 0; i < found.Length; i++)
        {
            UISceneCanvas c = found[i];
            if (c == null)
            {
                continue;
            }

            if (!canvases.ContainsKey(c.Kind))
            {
                canvases.Add(c.Kind, c);
                Dbg($"//Register kind={c.Kind},name={c.name},activeSelf={c.gameObject.activeSelf}");
            }
        }
    }

    private void ApplySortingOrders()
    {
        SetCanvasOrder(UISceneCanvasKind.Boot, baseOrder);
        SetCanvasOrder(UISceneCanvasKind.Lobby, baseOrder);
        SetCanvasOrder(UISceneCanvasKind.Game, baseOrder);
        SetCanvasOrder(UISceneCanvasKind.Loading, loadingOrder);
    }

    private void SetCanvasOrder(UISceneCanvasKind kind, int order)
    {
        if (!canvases.TryGetValue(kind, out UISceneCanvas root) || root == null)
        {
            return;
        }

        Canvas[] cs = root.GetComponentsInChildren<Canvas>(true);
        for (int i = 0; i < cs.Length; i++)
        {
            Canvas c = cs[i];
            if (c == null)
            {
                continue;
            }

            c.overrideSorting = true;
            c.sortingOrder = order;
        }
    }

    private void DumpRegistry(string tag)
    {
        if (!enableDebug)
        {
            return;
        }

        string s = tag;

        foreach (KeyValuePair<UISceneCanvasKind, UISceneCanvas> kv in canvases)
        {
            UISceneCanvasKind kind = kv.Key;
            UISceneCanvas root = kv.Value;

            if (root == null)
            {
                s += $"|{kind}:null";
                continue;
            }

            Canvas first = root.GetComponentInChildren<Canvas>(true);
            CanvasGroup cg = root.GetComponentInChildren<CanvasGroup>(true);

            string order = first != null ? first.sortingOrder.ToString() : "noCanvas";
            string alpha = cg != null ? cg.alpha.ToString("0.00") : "noGroup";

            s += $"|{kind}:activeSelf={root.gameObject.activeSelf},activeHier={root.gameObject.activeInHierarchy},order={order},alpha={alpha},name={root.name}";
        }

        Debug.Log(s);
    }

    private void DumpCanvasState(UISceneCanvas canvas, string tag)
    {
        if (!enableDebug)
        {
            return;
        }

        if (canvas == null)
        {
            Debug.Log(tag + "|canvas=null");
            return;
        }

        Canvas first = canvas.GetComponentInChildren<Canvas>(true);
        CanvasGroup cg = canvas.GetComponentInChildren<CanvasGroup>(true);

        string order = first != null ? first.sortingOrder.ToString() : "noCanvas";
        string alpha = cg != null ? cg.alpha.ToString("0.00") : "noGroup";

        Debug.Log($"{tag}|name={canvas.name},activeSelf={canvas.gameObject.activeSelf},activeHier={canvas.gameObject.activeInHierarchy},order={order},alpha={alpha}");
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