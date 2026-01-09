using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
SceneLoader는씬전환을비동기로처리하는MonoBehaviour컴포넌트다.
-로딩UI를표시하고한프레임양보해렌더링기회를보장한다.
-allowSceneActivation을사용해0.9로딩완료후최소표시시간을만족한뒤활성화한다.
*/
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private float minLoadingVisibleSeconds = 1.2f;//최소표시시간(디버그확인용)
    [SerializeField] private bool enableDebug = true;//디버그로그토글

    private bool isLoading;//중복로드방지
    public bool IsLoading => isLoading;

    public IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (isLoading)
        {
            Debug.LogWarning($"//AlreadyLoading:{sceneName}");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("//LoadSceneAsync sceneName empty");
            yield break;
        }

        isLoading = true;

        float shownAt = Time.unscaledTime;

        Dbg($"//LoadSceneAsync begin name={sceneName},mode={mode},active={SceneManager.GetActiveScene().name}");

        if (UIController.Instance != null)
        {
            UIController.Instance.ShowLoading();
            shownAt = Time.unscaledTime;
            yield return null;//로딩UI그릴프레임양보
            Dbg("//LoadSceneAsync afterShowLoading");
        }
        else
        {
            Dbg("//LoadSceneAsync UIController null");
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
        if (op == null)
        {
            Debug.LogError($"//LoadSceneAsync failed:{sceneName}");
            if (UIController.Instance != null)
            {
                UIController.Instance.HideLoading();
            }
            isLoading = false;
            yield break;
        }

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            DbgProgress(op.progress);
            yield return null;
        }

        float remain = minLoadingVisibleSeconds - (Time.unscaledTime - shownAt);
        Dbg($"//LoadSceneAsync progress=0.9 remain={remain:0.000}");

        if (remain > 0f)
        {
            yield return new WaitForSecondsRealtime(remain);
        }

        op.allowSceneActivation = true;
        Dbg("//LoadSceneAsync allowSceneActivation=true");

        while (!op.isDone)
        {
            yield return null;
        }

        Dbg($"//LoadSceneAsync done active={SceneManager.GetActiveScene().name}");

        if (UIController.Instance != null)
        {
            UIController.Instance.HideLoading();
        }

        isLoading = false;
    }

    public IEnumerator LoadSceneReplaceActiveAsync(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning($"//AlreadyLoading:{sceneName}");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("//LoadSceneReplaceActiveAsync sceneName empty");
            yield break;
        }

        isLoading = true;

        Scene prevActive = SceneManager.GetActiveScene();
        float shownAt = Time.unscaledTime;

        Dbg($"//Replace begin name={sceneName},prevActive={prevActive.name}");

        if (UIController.Instance != null)
        {
            UIController.Instance.ShowLoading();
            shownAt = Time.unscaledTime;
            yield return null;//로딩UI그릴프레임양보
            Dbg("//Replace afterShowLoading");
        }
        else
        {
            Dbg("//Replace UIController null");
        }

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"//LoadSceneReplaceActiveAsync load failed:{sceneName}");
            if (UIController.Instance != null)
            {
                UIController.Instance.HideLoading();
            }
            isLoading = false;
            yield break;
        }

        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.9f)
        {
            DbgProgress(loadOp.progress);
            yield return null;
        }

        float remain = minLoadingVisibleSeconds - (Time.unscaledTime - shownAt);
        Dbg($"//Replace progress=0.9 remain={remain:0.000}");

        if (remain > 0f)
        {
            yield return new WaitForSecondsRealtime(remain);
        }

        loadOp.allowSceneActivation = true;
        Dbg("//Replace allowSceneActivation=true");

        while (!loadOp.isDone)
        {
            yield return null;
        }

        Scene loaded = SceneManager.GetSceneByName(sceneName);
        if (!loaded.IsValid() || !loaded.isLoaded)
        {
            Debug.LogError($"//LoadSceneReplaceActiveAsync scene not loaded:{sceneName}");
            if (UIController.Instance != null)
            {
                UIController.Instance.HideLoading();
            }
            isLoading = false;
            yield break;
        }

        SceneManager.SetActiveScene(loaded);
        Dbg($"//Replace setActiveScene loaded={loaded.name}");

        if (prevActive.IsValid() && prevActive.isLoaded && prevActive.name != loaded.name)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(prevActive);
            Dbg($"//Replace unload prevActive={prevActive.name}");

            if (unloadOp != null)
            {
                while (!unloadOp.isDone)
                {
                    yield return null;
                }
            }
        }

        if (UIController.Instance != null)
        {
            UIController.Instance.HideLoading();
        }

        Dbg($"//Replace done active={SceneManager.GetActiveScene().name}");

        isLoading = false;
    }

    private void Dbg(string msg)
    {
        if (!enableDebug)
        {
            return;
        }

        Debug.Log(msg);
    }

    private void DbgProgress(float p)
    {
        if (!enableDebug)
        {
            return;
        }

        Debug.Log($"//Loading progress={p:0.000}");
    }
}
