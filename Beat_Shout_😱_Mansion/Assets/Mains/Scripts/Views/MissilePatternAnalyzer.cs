using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// ミサイルパターン解析用テストツール
/// 生成ノーツの詳細情報を調査・表示する
/// </summary>
public class MissilePatternAnalyzer : MonoBehaviour
{
    [Header("解析対象")]
    [Tooltip("解析するパターン文字列（例：A1B2C3D4E5F6G7H8）")]
    [SerializeField] private string targetPattern = "A1B2C3D4E5F6G7H8";

    [Header("角度設定（MissileTempoSpawnerと同期）")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleA = 0f;
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleB = 45f;
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleC = 90f;
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleD = 135f;
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleE = 180f;
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleF = 225f;
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleG = 270f;
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleH = 315f;

    [Header("ループ設定（オプション）")]
    [Tooltip("ループ開始インデックス（-1で無効）")]
    [SerializeField] private int loopStartIndex = -1;
    [Tooltip("ループ終了インデックス（-1で無効）")]
    [SerializeField] private int loopEndIndex = -1;

    [Header("出力設定")]
    [Tooltip("コンソールに詳細ログを出力")]
    [SerializeField] private bool logToConsole = true;
    [Tooltip("GUIに結果を表示")]
    [SerializeField] private bool showGUI = true;

    // 解析結果
    private List<NoteInfo> _noteInfos = new List<NoteInfo>();
    private string _analysisResult = string.Empty;
    private Vector2 _scrollPosition = Vector2.zero;

    // 角度マッピング
    private Dictionary<char, float> _angleMap = new Dictionary<char, float>();

    /// <summary>
    /// ノーツ情報構造体
    /// </summary>
    public struct NoteInfo
    {
        public int Index;           // インデックス
        public char AngleChar;      // 角度文字 (A-H) or X（スキップ）
        public float AngleDeg;      // 角度（度）
        public int MissileId;       // ミサイルID（0はスキップ）
        public bool IsSkip;         // スキップノートか
        public bool IsValid;        // 有効なノートか

        public override string ToString()
        {
            if (IsSkip) return $"[{Index:00}] SKIP (ミサイル生成なし)";
            return $"[{Index:00}] {AngleChar} → {AngleDeg:F1}° : Missile ID={MissileId}";
        }
    }

    private void Start()
    {
        InitializeAngleMap();
        AnalyzePattern();
    }

    private void Update()
    {
        // ショートカットキーで再解析
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift))
        {
            AnalyzePattern();
        }
    }

    private void OnValidate()
    {
        // インスペクタで値が変わったら再解析
        if (Application.isPlaying)
        {
            InitializeAngleMap();
            AnalyzePattern();
        }
    }

    /// <summary>
    /// 角度マッピングを初期化
    /// </summary>
    private void InitializeAngleMap()
    {
        _angleMap.Clear();
        _angleMap['A'] = positionAngleA;
        _angleMap['B'] = positionAngleB;
        _angleMap['C'] = positionAngleC;
        _angleMap['D'] = positionAngleD;
        _angleMap['E'] = positionAngleE;
        _angleMap['F'] = positionAngleF;
        _angleMap['G'] = positionAngleG;
        _angleMap['H'] = positionAngleH;
    }

    /// <summary>
    /// パターンを解析
    /// </summary>
    public void AnalyzePattern()
    {
        _noteInfos.Clear();

        if (string.IsNullOrEmpty(targetPattern))
        {
            _analysisResult = "* パターン文字列が空です";
            return;
        }

        // 正規表現パターン
        Regex regexWithAngle = new Regex(@"([A-H])([1-9])");
        Regex regexSkip = new Regex(@"0");

        int position = 0;
        int index = 0;

        while (position < targetPattern.Length)
        {
            Match matchWithAngle = regexWithAngle.Match(targetPattern, position);
            if (matchWithAngle.Success && matchWithAngle.Index == position)
            {
                char angleChar = matchWithAngle.Groups[1].Value[0];
                int missileId = int.Parse(matchWithAngle.Groups[2].Value);

                float angleDeg = _angleMap.TryGetValue(angleChar, out float angle) ? angle : 0f;

                _noteInfos.Add(new NoteInfo
                {
                    Index = index,
                    AngleChar = angleChar,
                    AngleDeg = angleDeg,
                    MissileId = missileId,
                    IsSkip = false,
                    IsValid = true
                });

                position += matchWithAngle.Length;
                index++;
                continue;
            }

            Match matchSkip = regexSkip.Match(targetPattern, position);
            if (matchSkip.Success && matchSkip.Index == position)
            {
                _noteInfos.Add(new NoteInfo
                {
                    Index = index,
                    AngleChar = 'X',
                    AngleDeg = 0f,
                    MissileId = 0,
                    IsSkip = true,
                    IsValid = true
                });

                position += matchSkip.Length;
                index++;
                continue;
            }

            // 不明な文字はスキップ
            Debug.LogWarning($"不明な文字 '{targetPattern[position]}' をスキップしました");
            position++;
        }

        // 結果を生成
        BuildAnalysisResult();

        if (logToConsole)
        {
            Debug.Log(_analysisResult);
        }
    }

    /// <summary>
    /// 解析結果を構築
    /// </summary>
    private void BuildAnalysisResult()
    {
        var sb = new StringBuilder();
        sb.AppendLine("-------------------------------------------------------");
        sb.AppendLine($"** ミサイルパターン解析結果");
        sb.AppendLine($"** パターン: {targetPattern}");
        sb.AppendLine($"** 総ノーツ数: {_noteInfos.Count}");

        // 有効なノーツ数をカウント
        int validCount = 0;
        int skipCount = 0;
        foreach (var note in _noteInfos)
        {
            if (note.IsSkip) skipCount++;
            else validCount++;
        }
        sb.AppendLine($"   ├─ 有効ノーツ: {validCount}");
        sb.AppendLine($"   └─ スキップ:   {skipCount}");

        // ループ設定
        if (loopStartIndex >= 0 && loopEndIndex >= 0 && loopStartIndex < loopEndIndex)
        {
            sb.AppendLine($"** ループ設定: {loopStartIndex} → {loopEndIndex}");
            sb.AppendLine($"   ├─ ループ範囲ノーツ数: {loopEndIndex - loopStartIndex + 1}");
            sb.AppendLine($"   └─ ループ前ノーツ: {loopStartIndex}枚, ループ後: {_noteInfos.Count - loopEndIndex - 1}枚");
        }
        else
        {
            sb.AppendLine($"** ループ設定: なし（全ノーツを1回再生）");
        }

        sb.AppendLine("───────────────────────────────────────────────────────");

        // ノーツ詳細
        sb.AppendLine("** ノーツ詳細:");
        sb.AppendLine(" No | 角度 | 度 | ID | 状態");
        sb.AppendLine("────┼──────┼──────┼────┼────────────");

        foreach (var note in _noteInfos)
        {
            if (note.IsSkip)
            {
                sb.AppendLine($" {note.Index:00} │  SKIP  │  --  │ -- │ ** スキップ");
            }
            else
            {
                string angleLabel = note.AngleChar.ToString();
                sb.AppendLine($" {note.Index:00} │   {angleLabel}   │ {note.AngleDeg,4:F1} │  {note.MissileId}  │ ? 生成");
            }
        }

        // 角度統計
        sb.AppendLine("───────────────────────────────────────────────────────");
        sb.AppendLine("** 角度別統計:");
        var angleStats = new Dictionary<char, int>();
        foreach (var note in _noteInfos)
        {
            if (!note.IsSkip)
            {
                if (!angleStats.ContainsKey(note.AngleChar))
                    angleStats[note.AngleChar] = 0;
                angleStats[note.AngleChar]++;
            }
        }

        foreach (var kvp in _angleMap)
        {
            char angleChar = kvp.Key;
            int count = angleStats.TryGetValue(angleChar, out int c) ? c : 0;
            string label = count > 0 ? $"{count}回" : "0回";
            sb.AppendLine($"   {angleChar} ({kvp.Value:F1}°) → {label}");
        }

        // ミサイルID統計
        sb.AppendLine("───────────────────────────────────────────────────────");
        sb.AppendLine("** ミサイルID別統計:");
        var idStats = new Dictionary<int, int>();
        foreach (var note in _noteInfos)
        {
            if (!note.IsSkip && note.MissileId > 0)
            {
                if (!idStats.ContainsKey(note.MissileId))
                    idStats[note.MissileId] = 0;
                idStats[note.MissileId]++;
            }
        }

        foreach (var kvp in idStats)
        {
            sb.AppendLine($"   ID {kvp.Key} → {kvp.Value}回");
        }

        sb.AppendLine("-------------------------------------------------------");

        _analysisResult = sb.ToString();
    }

    /// <summary>
    /// パターンをJSON形式で出力（外部ツール連携用）
    /// </summary>
    public string GetPatternJson()
    {
        var list = new List<object>();
        foreach (var note in _noteInfos)
        {
            list.Add(new
            {
                index = note.Index,
                angleChar = note.AngleChar.ToString(),
                angleDeg = note.AngleDeg,
                missileId = note.MissileId,
                isSkip = note.IsSkip
            });
        }

        return JsonUtility.ToJson(new { pattern = targetPattern, notes = list }, true);
    }

    /// <summary>
    /// コンソールに結果を出力
    /// </summary>
    public void PrintToConsole()
    {
        Debug.Log(_analysisResult);
    }

    /// <summary>
    /// 指定したインデックスのノーツ情報を取得
    /// </summary>
    public NoteInfo? GetNoteInfo(int index)
    {
        if (index < 0 || index >= _noteInfos.Count) return null;
        return _noteInfos[index];
    }

    /// <summary>
    /// 有効なノーツ数を取得
    /// </summary>
    public int GetValidNoteCount()
    {
        int count = 0;
        foreach (var note in _noteInfos)
        {
            if (!note.IsSkip) count++;
        }
        return count;
    }

    private void OnGUI()
    {
        if (!showGUI) return;

        // ウィンドウ表示
        GUILayout.Window(0, new Rect(10, 10, 500, 400), DrawWindow, "** ミサイルパターンアナライザー");
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();

        // パターン入力
        GUILayout.BeginHorizontal();
        GUILayout.Label("パターン:", GUILayout.Width(60));
        string newPattern = GUILayout.TextField(targetPattern, GUILayout.Width(200));
        if (newPattern != targetPattern)
        {
            targetPattern = newPattern;
            AnalyzePattern();
        }
        if (GUILayout.Button("再解析", GUILayout.Width(80)))
        {
            AnalyzePattern();
        }
        GUILayout.EndHorizontal();

        // 結果表示（スクロール可能）
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(280));
        GUILayout.TextArea(_analysisResult, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        // アクションボタン
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("** コンソールに出力"))
        {
            PrintToConsole();
        }
        if (GUILayout.Button("** JSON出力"))
        {
            Debug.Log(GetPatternJson());
        }
        if (GUILayout.Button("** 統計のみ"))
        {
            Debug.Log(GetStatisticsSummary());
        }
        GUILayout.EndHorizontal();

        // 情報表示
        GUILayout.Label($"ノーツ数: {_noteInfos.Count} | 有効: {GetValidNoteCount()} | スキップ: {_noteInfos.Count - GetValidNoteCount()}");

        GUILayout.EndVertical();

        // ウィンドウをドラッグ可能に
        GUI.DragWindow();
    }

    /// <summary>
    /// 統計情報のみを取得
    /// </summary>
    public string GetStatisticsSummary()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"** パターン統計: {targetPattern}");
        sb.AppendLine($"   総ノーツ: {_noteInfos.Count}");
        sb.AppendLine($"   有効: {GetValidNoteCount()}");
        sb.AppendLine($"   スキップ: {_noteInfos.Count - GetValidNoteCount()}");
        return sb.ToString();
    }
}
