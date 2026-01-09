using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
LoadingController는씬전환중표시되는로딩패널을제어하는MonoBehaviour컴포넌트다.
-Show로켜고Hide로최소표시시간을보장한뒤끄는역할만한다.
-CanvasGroup을사용하지않는다.
-입력차단이필요하면패널에전체화면Graphic(Image 등) Raycast Target을사용한다.
-UIRoot가DontDestroyOnLoad라면본오브젝트는자식으로두고DontDestroyOnLoad를호출하지않는다.
*/
[DefaultExecutionOrder(-850)]
public sealed class LoadingController : MonoBehaviour
{
    public static LoadingController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject panelRoot;//로딩UI루트(없으면본인)

    [Header("Behavior")]
    [SerializeField] private float minVisibleSeconds = 0.2f;//최소표시
    [SerializeField] private bool blockRaycastsWhileVisible = true;//입력차단
    [SerializeField] private bool dontDestroyOnLoad = false;//UIRoot가영구면false권장

    private float shownAtUnscaled = -999f;
    private Coroutine hideRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }

        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        HideImmediate();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        panelRoot.SetActive(true);
        shownAtUnscaled = Time.unscaledTime;

        if (blockRaycastsWhileVisible)
        {
            SetRaycastTarget(panelRoot, true);
        }
    }

    public void Hide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        hideRoutine = StartCoroutine(HideAfterMin());
    }

    public void HideImmediate()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        SetRaycastTarget(panelRoot, false);
        panelRoot.SetActive(false);
    }

    private IEnumerator HideAfterMin()
    {
        float elapsed = Time.unscaledTime - shownAtUnscaled;
        float remain = minVisibleSeconds - elapsed;

        if (remain > 0f)
        {
            yield return new WaitForSecondsRealtime(remain);
        }

        HideImmediate();
        hideRoutine = null;
    }

    private static void SetRaycastTarget(GameObject root, bool enabled)
    {
        if (root == null)
        {
            return;
        }

        Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic g = graphics[i];
            if (g == null)
            {
                continue;
            }

            g.raycastTarget = enabled;
        }
    }
}
