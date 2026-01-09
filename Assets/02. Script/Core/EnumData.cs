using UnityEngine;

/*
UISceneCanvasKind는Core영역에서사용되는enum다.
-씬전환을비동기로처리하고,전환중중복호출을가드한다.
-UI상태전환/페이드/버튼입력등화면흐름을담당한다.
-컴포넌트참조는Awake에서캐싱하고,null을가드한다.
*/
public static class EnumData
{
    public enum ScreenId
    {
        None = 0,
        Boot = 1,
        Lobby = 2,
        Game = 3,
        PartyList = 4,
        Pokedex = 5
            ,
    }

    public enum PopupId
    {
        None = 0,

        Settings,
        Confirm,
        GameOver,
        GameClear,
        LevelUp,
        Shop,
        Item,
        ContextMenu,
        MoveSelect,
    }
}

//UICanvas역할구분용
public enum UISceneCanvasKind
{
    Boot,    //부트UI
    Lobby,   //로비UI
    Game,    //게임UI
    Loading, //로딩UI
}

//EvolutionCode음수해석결과용
public enum SpecialEvolutionKind
{
    None = 0,          //특수변형아님
    MegaEvolution = 1, //메가진화(-1,-101,-102...)
    Gigantamax = 2,    //거다이맥스(-2)
    FormChange1 = 31,  //폼체인지1(-3)
    FormChange2 = 32,  //폼체인지2(-4)
    FormChange3 = 33,  //폼체인지3(-5)
    Unknown = 99,      //규칙밖값
}

//타입공용정의용
public enum PokemonType
{
    None = 0, //타입2없음
    Normal,   //노말
    Fire,     //불꽃
    Water,    //물
    Electric, //전기
    Grass,    //풀
    Ice,      //얼음
    Fighting, //격투
    Poison,   //독
    Ground,   //땅
    Flying,   //비행
    Psychic,  //에스퍼
    Bug,      //벌레
    Rock,     //바위
    Ghost,    //고스트
    Dragon,   //드래곤
    Dark,     //악
    Steel,    //강철
    Fairy,    //페어리
}
