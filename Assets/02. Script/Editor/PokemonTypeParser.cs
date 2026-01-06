using System;
using System.Collections.Generic;
using UnityEngine;

//포켓몬 타입 문자열을 PokemonType enum으로 변환하는 유틸
public static class PokemonTypeParser
{
    //한글 타입 문자열 -> enum 매핑
    private static readonly Dictionary<string, PokemonType> typeMap = new Dictionary<string, PokemonType>()
    {
        {"노말", PokemonType.Normal},
        {"불꽃", PokemonType.Fire},
        {"물", PokemonType.Water},
        {"전기", PokemonType.Electric},
        {"풀", PokemonType.Grass},
        {"얼음", PokemonType.Ice},
        {"격투", PokemonType.Fighting},
        {"독", PokemonType.Poison},
        {"땅", PokemonType.Ground},
        {"비행", PokemonType.Flying},
        {"에스퍼", PokemonType.Psychic},
        {"벌레", PokemonType.Bug},
        {"바위", PokemonType.Rock},
        {"고스트", PokemonType.Ghost},
        {"드래곤", PokemonType.Dragon},
        {"악", PokemonType.Dark},
        {"강철", PokemonType.Steel},
        {"페어리", PokemonType.Fairy},
    };

    public static PokemonType ParseSingleType(string rawType)
    {
        if (string.IsNullOrEmpty(rawType))
        {
            return PokemonType.None;
        }

        string trimmed = rawType.Trim();

        if (typeMap.TryGetValue(trimmed, out PokemonType result))
        {
            return result;
        }

        Debug.LogWarning($"알 수 없는 타입 문자열: {trimmed}");
        return PokemonType.None;
    }
}