using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//CSV를 읽어서 PokemonDatabase.asset(ScriptableObject)를 생성/갱신하는 에디터 임포터 툴.
public class PokemonCsvImporterWindow : EditorWindow
{
    [SerializeField] private TextAsset csvFile; //임포트할 CSV(TextAsset)
    [SerializeField] private string outputAssetPath = "Assets/03.Data/Pokedex/PokemonDatabase.asset"; //생성/갱신될 에셋 경로

    [MenuItem("Tools/Pokedex/CSV Importer")]
    public static void Open()
    {
        GetWindow<PokemonCsvImporterWindow>("Pokemon CSV Importer");
    }

    private void OnGUI()
    {
        //입력 UI
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        outputAssetPath = EditorGUILayout.TextField("Output Asset Path", outputAssetPath);

        GUILayout.Space(8);

        //실행 버튼
        if (GUILayout.Button("Import CSV -> PokemonDatabase.asset", GUILayout.Height(28)))
        {
            if (csvFile == null)
            {
                Debug.LogError("CSV File이 비어있어 임포트할 수 없어.");
            }
            else
            {
                Import(csvFile.text, outputAssetPath);
            }
        }
    }

    private void Import(string csvText, string assetPath)
    {
        //CSV 파싱
        List<PokemonEntry> parsed = ParseCsv(csvText);

        //에셋 폴더 생성 보장
        EnsureFolderExists(Path.GetDirectoryName(assetPath));

        //기존 에셋 로드 또는 생성
        PokemonDatabaseSO db = AssetDatabase.LoadAssetAtPath<PokemonDatabaseSO>(assetPath);
        if (db == null)
        {
            db = CreateInstance<PokemonDatabaseSO>();
            AssetDatabase.CreateAsset(db, assetPath);
        }

        //데이터 반영
        db.SetEntries(parsed);
        EditorUtility.SetDirty(db);

        //저장/갱신
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"임포트 완료: {parsed.Count}개 항목 -> {assetPath}");
    }

    private List<PokemonEntry> ParseCsv(string text)
    {
        //개행 정리
        string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = normalized.Split('\n');

        List<PokemonEntry> result = new List<PokemonEntry>();

        //빈 파일 방어
        if (lines.Length == 0)
        {
            return result;
        }

        //헤더 확인 후 시작 라인 결정
        int startIndex = 0;
        if (IsHeaderLine(lines[0]))
        {
            startIndex = 1;
        }

        for (int i = startIndex; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            //CSV는 쉼표 기준, TSV는 탭 기준인데, 둘 다 대응
            string[] cols = SplitCsvOrTsvLine(line);

            //최소 컬럼수 방어(필요한 것만이라도)
            if (cols.Length < 4)
            {
                continue;
            }

            int no = ToIntSafe(Get(cols, 0));
            string name = Get(cols, 1);
            string type1 = Get(cols, 2);
            string type2 = Get(cols, 3);

            //나머지는 없으면 기본값
            string abilities = Get(cols, 4);
            int hp = ToIntSafe(Get(cols, 5));
            int atk = ToIntSafe(Get(cols, 6));
            int def = ToIntSafe(Get(cols, 7));
            int spAtk = ToIntSafe(Get(cols, 8));
            int spDef = ToIntSafe(Get(cols, 9));
            int speed = ToIntSafe(Get(cols, 10));
            int value = ToIntSafe(Get(cols, 11));
            int nextEvoLevel = ToIntSafe(Get(cols, 12));

            //단일타입이면 type2는 빈칸 유지(입력 그대로)
            if (type2 == null)
            {
                type2 = "";
            }

            result.Add(new PokemonEntry(no, name, type1, type2, abilities, hp, atk, def, spAtk, spDef, speed, value, nextEvoLevel));
        }

        return result;
    }

    private bool IsHeaderLine(string firstLine)
    {
        //No, Name, Type1 같은 글자가 있으면 헤더로 취급
        string lower = firstLine.ToLowerInvariant();
        if (lower.Contains("no") && lower.Contains("name"))
        {
            return true;
        }
        return false;
    }

    private string[] SplitCsvOrTsvLine(string line)
    {
        //탭이 있으면 TSV 우선
        if (line.Contains("\t"))
        {
            return line.Split('\t');
        }

        //기본은 CSV(단순 split, 쿼트 복잡케이스는 일단 제외)
        return line.Split(',');
    }

    private string Get(string[] cols, int index)
    {
        if (index < 0 || index >= cols.Length)
        {
            return "";
        }
        return cols[index].Trim();
    }

    private int ToIntSafe(string s)
    {
        if (int.TryParse(s, out int v))
        {
            return v;
        }
        return 0;
    }

    private void EnsureFolderExists(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        //Assets로 시작하지 않으면 스킵
        if (!folderPath.StartsWith("Assets"))
        {
            return;
        }

        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath);
        string name = Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolderExists(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }
}
