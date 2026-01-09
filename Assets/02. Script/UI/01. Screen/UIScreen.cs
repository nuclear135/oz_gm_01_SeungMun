using UnityEngine;
using static EnumData;

/*
UIScreen은큰화면UI공통베이스클래스다.
-UIManager가Register/Show/Hide를호출한다.
-씬별스크린은ScreenRegistry가자동등록후ScreenRoot로편입한다.
*/
public abstract class UIScreen : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private ScreenId screenId;//스크린아이디

    private bool initialized;//초기화여부

    public ScreenId ScreenId => screenId;

    public void Show()
    {
        if (!initialized)
        {
            initialized = true;
            OnInit();
        }

        gameObject.SetActive(true);
        OnShow();
    }

    public void Hide()
    {
        OnHide();
        gameObject.SetActive(false);
    }

    protected virtual void OnInit() { }
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}