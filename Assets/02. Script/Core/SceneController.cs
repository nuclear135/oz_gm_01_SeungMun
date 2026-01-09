using UnityEngine;

/*
SceneController는씬/오브젝트에붙는MonoBehaviour컴포넌트다.
-씬전환을비동기로처리하고,전환중중복호출을가드한다.
-외부에서는OnClickGoLobby을호출해이기능을사용한다.
-외부에서는OnClickGoGame을호출해이기능을사용한다.
*/
public class SceneController : MonoBehaviour
{
    //버튼연결용
    public void OnClickGoLobby()
    {
        if(GameManager.Instance == null)
        {
            Debug.LogError("//GameManager.Instance null");
            return;
        }

        GameManager.Instance.LoadLobby();
    }

    //버튼연결용
    public void OnClickGoGame()
    {
        if(GameManager.Instance == null)
        {
            Debug.LogError("//GameManager.Instance null");
            return;
        }

        GameManager.Instance.LoadGame();
    }
}
