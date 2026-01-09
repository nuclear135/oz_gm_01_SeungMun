using UnityEngine;

/*
PopupRegistry는씬에배치된UIPopup들을자동으로UIManager에등록하는MonoBehaviour컴포넌트다.
-비활성팝업까지탐색해등록한다.
*/
public class PopupRegistry : MonoBehaviour
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
            Debug.LogError("//PopupRegistry failed:UIManager.Instance null");
            return;
        }

        UIPopup[] popups = GetComponentsInChildren<UIPopup>(true);
        for (int i = 0; i < popups.Length; i++)
        {
            UIPopup popup = popups[i];
            if (popup == null)
            {
                continue;
            }

            UIManager.Instance.RegisterPopup(popup);
        }
    }
}