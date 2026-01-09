using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EnumData;

/*
UIManager는UI레이어/스택을관리하는MonoBehaviour컴포넌트다.
-UIRoot는DontDestroyOnLoad로유지된다.
-화면(UIScreen)은ScreenRoot하위에서자동등록한다(비활성포함).
-씬이바뀌면(ActiveSceneChanged)부트/로비/게임화면을자동으로선택해보여준다.
-Esc는Settings팝업토글입력이며,외부게이트조건을통과할때만허용한다.
-X/Backspace는Top팝업닫기입력이다.
*/
[DefaultExecutionOrder(-900)]
public sealed class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Roots (UIRoot children)")]
    [SerializeField] private Transform screenRoot;
    [SerializeField] private Transform popupRoot;
    [SerializeField] private Transform systemRoot;
    [SerializeField] private Transform toastRoot;

    [Header("Auto Screen By SceneName (contains)")]
    [SerializeField] private string bootSceneKey = "Boot";
    [SerializeField] private string lobbySceneKey = "Lobby";
    [SerializeField] private string gameSceneKey = "Game";
    [SerializeField] private ScreenId bootScreenId = ScreenId.Boot;
    [SerializeField] private ScreenId lobbyScreenId = ScreenId.Lobby;
    [SerializeField] private ScreenId gameScreenId = ScreenId.Game;

    [Header("GlobalInput")]
    [SerializeField] private bool enableGlobalInput = true;
    [SerializeField] private PopupId settingsPopupId = PopupId.Settings;

    private readonly Stack<UIScreen> screenStack = new Stack<UIScreen>();
    private readonly Stack<UIPopup> popupStack = new Stack<UIPopup>();

    private readonly Dictionary<ScreenId, UIScreen> screenTable = new Dictionary<ScreenId, UIScreen>();
    private readonly Dictionary<PopupId, UIPopup> popupTable = new Dictionary<PopupId, UIPopup>();

    private Func<bool> canToggleSettings;

    public Transform ScreenRoot => screenRoot;
    public Transform PopupRoot => popupRoot;
    public Transform SystemRoot => systemRoot;
    public Transform ToastRoot => toastRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.root == transform)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }

        if (screenRoot == null || popupRoot == null || systemRoot == null || toastRoot == null)
        {
            Debug.LogError("//UIManager roots not assigned");
        }

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        AutoRegisterScreens();
        AutoRegisterPopups();

        ShowScreenForScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Instance = null;
        }
    }

    private void Update()
    {
        if (!enableGlobalInput)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CanToggleSettingsInternal())
            {
                TogglePopup(settingsPopupId);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Backspace))
        {
            CloseTopPopup();
        }
    }

    private void OnActiveSceneChanged(Scene prev, Scene next)
    {
        AutoRegisterScreens();
        AutoRegisterPopups();
        ShowScreenForScene(next);
    }

    public void SetSettingsGate(Func<bool> gate)
    {
        canToggleSettings = gate;
    }

    public void ClearSettingsGate()
    {
        canToggleSettings = null;
    }

    public bool HasAnyPopup()
    {
        return popupStack.Count > 0;
    }

    public Coroutine Run(IEnumerator routine)
    {
        if (routine == null)
        {
            return null;
        }

        return StartCoroutine(routine);
    }

    public void Stop(Coroutine coroutine)
    {
        if (coroutine == null)
        {
            return;
        }

        StopCoroutine(coroutine);
    }

    private bool CanToggleSettingsInternal()
    {
        if (canToggleSettings == null)
        {
            return true;
        }

        return canToggleSettings();
    }

    public void AutoRegisterScreens()
    {
        if (screenRoot == null)
        {
            return;
        }

        UIScreen[] screens = screenRoot.GetComponentsInChildren<UIScreen>(true);
        for (int i = 0; i < screens.Length; i++)
        {
            UIScreen s = screens[i];
            if (s == null)
            {
                continue;
            }

            RegisterScreen(s.ScreenId, s);
        }
    }

    public void AutoRegisterPopups()
    {
        if (popupRoot == null)
        {
            return;
        }

        UIPopup[] popups = popupRoot.GetComponentsInChildren<UIPopup>(true);
        for (int i = 0; i < popups.Length; i++)
        {
            UIPopup p = popups[i];
            if (p == null)
            {
                continue;
            }

            RegisterPopup(p.PopupId, p);
        }
    }

    public void ShowScreenForScene(Scene scene)
    {
        if (!scene.IsValid())
        {
            return;
        }

        string name = scene.name;

        if (!string.IsNullOrWhiteSpace(bootSceneKey) && name.Contains(bootSceneKey))
        {
            ShowScreen(bootScreenId, true);
            return;
        }

        if (!string.IsNullOrWhiteSpace(lobbySceneKey) && name.Contains(lobbySceneKey))
        {
            ShowScreen(lobbyScreenId, true);
            return;
        }

        if (!string.IsNullOrWhiteSpace(gameSceneKey) && name.Contains(gameSceneKey))
        {
            ShowScreen(gameScreenId, true);
            return;
        }
    }

    // ---------- Screen ----------
    public void RegisterScreen(UIScreen screen)
    {
        if (screen == null)
        {
            Debug.LogError("//RegisterScreen failed:screen null");
            return;
        }

        RegisterScreen(screen.ScreenId, screen);
    }

    public void RegisterScreen(ScreenId id, UIScreen screen)
    {
        if (screen == null)
        {
            Debug.LogError($"//RegisterScreen failed:{id}");
            return;
        }

        if (id == ScreenId.None)
        {
            Debug.LogError($"//RegisterScreen failed:ScreenId None name={screen.name}");
            return;
        }

        if (screenRoot != null && screen.transform.parent != screenRoot)
        {
            screen.transform.SetParent(screenRoot, false);
        }

        if (!screenTable.ContainsKey(id))
        {
            screenTable.Add(id, screen);
        }
        else
        {
            screenTable[id] = screen;
        }

        screen.gameObject.SetActive(false);
    }

    public void ShowScreen(ScreenId id, bool clearStack = true)
    {
        if (!screenTable.TryGetValue(id, out UIScreen screen) || screen == null)
        {
            Debug.LogError($"//ShowScreen failed:not registered:{id}");
            return;
        }

        ShowScreen(screen, clearStack);
    }

    public void ShowScreen(UIScreen screen, bool clearStack = true)
    {
        if (screen == null)
        {
            return;
        }

        if (clearStack)
        {
            while (screenStack.Count > 0)
            {
                UIScreen top = screenStack.Pop();
                if (top != null)
                {
                    top.Hide();
                }
            }
        }

        screenStack.Push(screen);
        screen.Show();
    }

    // ---------- Popup ----------
    public void RegisterPopup(UIPopup popup)
    {
        if (popup == null)
        {
            Debug.LogError("//RegisterPopup failed:popup null");
            return;
        }

        RegisterPopup(popup.PopupId, popup);
    }

    public void RegisterPopup(PopupId id, UIPopup popup)
    {
        if (popup == null)
        {
            Debug.LogError($"//RegisterPopup failed:{id}");
            return;
        }

        if (id == PopupId.None)
        {
            Debug.LogError($"//RegisterPopup failed:PopupId None name={popup.name}");
            return;
        }

        if (popupRoot != null && popup.transform.parent != popupRoot)
        {
            popup.transform.SetParent(popupRoot, false);
        }

        if (!popupTable.ContainsKey(id))
        {
            popupTable.Add(id, popup);
        }
        else
        {
            popupTable[id] = popup;
        }

        popup.gameObject.SetActive(false);
    }

    public void ShowPopup(PopupId id)
    {
        if (!popupTable.TryGetValue(id, out UIPopup popup) || popup == null)
        {
            Debug.LogError($"//ShowPopup failed:not registered:{id}");
            return;
        }

        popupStack.Push(popup);
        popup.Open();
    }

    public void TogglePopup(PopupId id)
    {
        if (popupStack.Count > 0)
        {
            UIPopup top = popupStack.Peek();
            if (top != null && top.PopupId == id)
            {
                CloseTopPopup();
                return;
            }
        }

        ShowPopup(id);
    }

    public void CloseTopPopup()
    {
        if (popupStack.Count == 0)
        {
            return;
        }

        UIPopup top = popupStack.Pop();
        if (top == null)
        {
            return;
        }

        top.Close();
    }

    public void CloseAllPopup()
    {
        while (popupStack.Count > 0)
        {
            UIPopup top = popupStack.Pop();
            if (top != null)
            {
                top.Close();
            }
        }
    }

    // ---------- System/Toast ----------
    public void AdoptToSystemRoot(Transform t)
    {
        if (t == null || systemRoot == null)
        {
            return;
        }

        t.SetParent(systemRoot, false);
    }

    public void AdoptToToastRoot(Transform t)
    {
        if (t == null || toastRoot == null)
        {
            return;
        }

        t.SetParent(toastRoot, false);
    }
}
