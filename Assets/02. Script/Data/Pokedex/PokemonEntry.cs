using System;
using UnityEngine;

//포켓몬 1개 항목 데이터. CSV의 한 줄을 그대로 담는 직렬화 구조
[Serializable]
public class PokemonEntry
{
    [SerializeField] private int no; //전국도감 번호
    [SerializeField] private string name; //한국어 이름
    [SerializeField] private string type1; //타입1
    [SerializeField] private string type2; //타입2(단일타입이면 빈칸)
    [SerializeField] private string abilities; //특성 문자열
    [SerializeField] private int hp; //HP
    [SerializeField] private int atk; //Atk
    [SerializeField] private int def; //Def
    [SerializeField] private int spAtk; //SpAtk
    [SerializeField] private int spDef; //SpDef
    [SerializeField] private int speed; //Speed
    [SerializeField] private int value; //합계(표에서 보려고 둔 값)
    [SerializeField] private int nextEvoLevel; //다음진화레벨(없으면 0)

    public int No => no;
    public string Name => name;
    public string Type1 => type1;
    public string Type2 => type2;
    public string Abilities => abilities;
    public int HP => hp;
    public int Atk => atk;
    public int Def => def;
    public int SpAtk => spAtk;
    public int SpDef => spDef;
    public int Speed => speed;
    public int Value => value;
    public int NextEvoLevel => nextEvoLevel;

    public PokemonEntry(int no, string name, string type1, string type2, string abilities, int hp, int atk, int def, int spAtk, int spDef, int speed, int value, int nextEvoLevel)
    {
        this.no = no;
        this.name = name;
        this.type1 = type1;
        this.type2 = type2;
        this.abilities = abilities;
        this.hp = hp;
        this.atk = atk;
        this.def = def;
        this.spAtk = spAtk;
        this.spDef = spDef;
        this.speed = speed;
        this.value = value;
        this.nextEvoLevel = nextEvoLevel;
    }
}

