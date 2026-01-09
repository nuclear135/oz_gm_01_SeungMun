using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
UISceneCanvas는씬별UI루트의활성/페이드를담당한다.
-활성화시하위전체오브젝트를강제활성화해부분비활성로인한미표시를방지한다.
-CanvasGroup알파/입력플래그를복구해페이드잔여값(alpha=0)으로인한미표시를방지한다.
*/
public class UISceneCanvas : MonoBehaviour
{
    [SerializeField] private UISceneCanvasKind kind;//루트종류
    [SerializeField] private bool forceEnableChildren = true;//하위강제활성
    [SerializeField] private bool ensureCanvasGroup = true;//캔버스그룹보장

    private CanvasGroup canvasGroup;//캐시

    public UISceneCanvasKind Kind => kind;

    public void InitializeAll(Scene scene)
    {
        if (forceEnableChildren)
        {
            SetChildrenActive(true);
        }

        if (ensureCanvasGroup)
        {
            EnsureCanvasGroup();
        }

        if (canvasGroup != null && gameObject.activeInHierarchy)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null)
        {
            return;
        }

        canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (canvasGroup != null)
        {
            return;
        }

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void SetChildrenActive(bool isActive)
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform c = transform.GetChild(i);
            if (c == null)
            {
                continue;
            }

            c.gameObject.SetActive(isActive);
        }
    }
}