using UnityEngine;
using static EnumData;

/*
UIPopup은팝업UI공통베이스클래스다.
-UIManager가팝업을스택으로관리하며Open/Close만호출한다.
-씬별팝업은PopupRegistry가자동등록후PopupRoot로편입한다.
*/
public abstract class UIPopup : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private PopupId popupId;//팝업아이디

    private bool initialized;//초기화여부

    public PopupId PopupId => popupId;

    public void Open()
    {
        if (!initialized)
        {
            initialized = true;
            OnInit();
        }

        gameObject.SetActive(true);
        OnOpen();
    }

    public void Close()
    {
        OnClose();
        gameObject.SetActive(false);
    }

    protected virtual void OnInit() { }
    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }
}