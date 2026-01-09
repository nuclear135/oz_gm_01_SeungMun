using System.Collections.Generic;
using UnityEngine;

/*
PokedexDebug는씬/오브젝트에붙는MonoBehaviour컴포넌트다.
-데이터테이블/ScriptableObject를읽어런타임조회가가능하도록초기화한다.
-컴포넌트참조는Awake에서캐싱하고,null을가드한다.
-Update에서GC유발패턴을피한다.
*/
public class PokedexDebug : MonoBehaviour
{
    [SerializeField] private int testNo = 6;//확인할 도감번호

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance가 없어.");
            return;
        }

        IReadOnlyList<PokemonEntry> list;
        if (!GameManager.Instance.Pokedex.TryGetAllByNo(testNo, out list))
        {
            Debug.LogWarning($"No={testNo}를 찾지 못했어.");
            return;
        }

        Debug.Log($"No={testNo}entries={list.Count}");
        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            Debug.Log($"[{i}]No={e.No},Name={e.Name},EvolutionCode={e.EvolutionCode},Special={e.SpecialEvolutionKind},MegaVar={e.MegaVariantIndex}");
        }

        PokemonEntry def;
        if (GameManager.Instance.Pokedex.TryGetDefaultByNo(testNo, out def))
        {
            Debug.Log($"Default->No={def.No},Name={def.Name},EvolutionCode={def.EvolutionCode}");
        }
    }
}