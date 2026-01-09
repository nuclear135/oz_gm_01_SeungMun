using UnityEngine;

/*
ScreenRegistry는씬에배치된UIScreen들을자동으로UIManager에등록하는MonoBehaviour컴포넌트다.
-비활성스크린까지탐색해등록한다.
*/
public class ScreenRegistry : MonoBehaviour
{
    [SerializeField] private bool registerOnStart = true;

    private void Start()
    {
        if (!registerOnStart)
        {
            return;
        }

        if (UIManager.Instance == null)
        {
            Debug.LogError("//ScreenRegistry failed:UIManager.Instance null");
            return;
        }

        UIScreen[] screens = GetComponentsInChildren<UIScreen>(true);
        for (int i = 0; i < screens.Length; i++)
        {
            UIScreen screen = screens[i];
            if (screen == null)
            {
                continue;
            }

            UIManager.Instance.RegisterScreen(screen);
        }
    }
}