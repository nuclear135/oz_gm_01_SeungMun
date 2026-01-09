using UnityEngine.SceneManagement;

/*
IUISceneInitializer는UI영역에서사용되는interface다.
-씬전환을비동기로처리하고,전환중중복호출을가드한다.
-UI상태전환/페이드/버튼입력등화면흐름을담당한다.
-컴포넌트참조는Awake에서캐싱하고,null을가드한다.
*/
public interface IUISceneInitializer
{
    void InitializeOnSceneLoad(Scene scene);//씬진입시UI상태초기화훅
}
