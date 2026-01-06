using System.Collections.Generic;
using UnityEngine;

//포켓몬 도감 전체 데이터베이스(ScriptableObject). CSV 임포트 결과를 저장하고 조회한다.
[CreateAssetMenu(fileName = "PokemonDatabase", menuName = "Data/Pokedex/Pokemon Database")]
public class PokemonDatabaseSO : ScriptableObject
{
    [SerializeField] private List<PokemonEntry> entries = new List<PokemonEntry>(); //전체 포켓몬 목록

    public IReadOnlyList<PokemonEntry> Entries => entries;

    public void SetEntries(List<PokemonEntry> newEntries)
    {
        entries = newEntries;
    }
}
