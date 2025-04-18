using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// テンポに合わせてミサイルを順番に生成するスポナー
/// </summary>
public class MissileTempoSpawner : MonoBehaviour
{
    [Header("生成パターン")]
    [Tooltip("角度(A-H)+ミサイルID(1-9)または0(スキップ)で指定（例：A1B20C3）")]
    [SerializeField] private string missilePattern = "A1B2C3D4E5F6G7H8";

    [Header("角度設定")]
    [Tooltip("角度A（度）0=右(3時), 90=上(12時), 180=左(9時), 270=下(6時)")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleA = 0f;     // 右方向(3時)
    [Tooltip("角度B（度）0=右(3時), 90=上(12時), 180=左(9時), 270=下(6時)")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleB = 45f;    // 右上方向(1時30分)
    [Tooltip("角度C（度）0=右(3時), 90=上(12時), 180=左(9時), 270=下(6時)")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleC = 90f;    // 上方向(12時)
    [Tooltip("角度D（度）0=右(3時), 90=上(12時), 180=左(9時), 270=下(6時)")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleD = 135f;   // 左上方向(10時30分)
    [Tooltip("角度E（度）0=右(3時), 90=上(12時), 180=左(9時), 270=下(6時)")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleE = 180f;   // 左方向(9時)
    [Tooltip("角度F（度）0=右(3時), 90=上(12時), 180=左(9時), 270=下(6時)")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleF = 225f;   // 左下方向(7時30分)
    [Tooltip("角度G（度）0=右(3時), 90=上(12時), 180=左(9時), 270=下(6時)")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleG = 270f;   // 下方向(6時)
    [Tooltip("角度H（度）0=右(3時), 90=上(12時), 180=左(9時), 270=下(6時)")]
    [Range(0f, 360f)]
    [SerializeField] private float positionAngleH = 315f;   // 右下方向(4時30分)

    [Header("距離設定")]
    [Tooltip("生成するミサイルの距離（0の場合は自分の位置）")]
    [SerializeField] private float spawnDistance = 0f;

    [Header("参照")]
    [Tooltip("ミサイルプーラーへの参照（空の場合は自動検索）")]
    [SerializeField] private MissileObjectPooler missilePooler;

    // 内部変数
    private int currentBeatIndex = 0;
    private Vector3 originalScale;
    private List<(char angle, int missileId)> patternList = new List<(char, int)>(); // 角度とIDのペアのリスト

    // 角度文字と実際の角度のマッピング用ディクショナリ
    private Dictionary<char, float> angleMap = new Dictionary<char, float>();

    private void Start()
    {
        // 元のサイズを保存
        originalScale = transform.localScale;

        // ミサイルプーラーが未設定の場合は検索
        if (missilePooler == null)
        {
            missilePooler = FindAnyObjectByType<MissileObjectPooler>();
            if (missilePooler == null)
            {
                Debug.LogError("MissileTempoSpawner: MissileObjectPooler が見つかりません。");
            }
        }

        // 角度マッピングを初期化
        InitializeAngleMap();

        // パターン文字列を解析
        ParsePatternString();
    }

    private void OnEnable()
    {
        // 元のサイズを保存
        originalScale = transform.localScale;

        // 角度マッピングを初期化
        InitializeAngleMap();

        // テンポイベントに登録
        CRIWARE_conductor.TempoMethodEvent1 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent2 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent3 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent4 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent5 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent6 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent7 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent8 += TempoMethod;

        // カウンターリセット
        currentBeatIndex = 0;

        // パターン文字列を再解析
        ParsePatternString();
    }

    private void OnDisable()
    {
        // テンポイベントから登録解除
        CRIWARE_conductor.TempoMethodEvent1 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent2 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent3 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent4 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent5 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent6 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent7 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent8 -= TempoMethod;
    }

    /// <summary>
    /// 角度マッピングを初期化
    /// </summary>
    private void InitializeAngleMap()
    {
        angleMap.Clear();
        angleMap['A'] = positionAngleA;
        angleMap['B'] = positionAngleB;
        angleMap['C'] = positionAngleC;
        angleMap['D'] = positionAngleD;
        angleMap['E'] = positionAngleE;
        angleMap['F'] = positionAngleF;
        angleMap['G'] = positionAngleG;
        angleMap['H'] = positionAngleH;
    }

    /// <summary>
    /// パターン文字列を解析して角度とミサイルIDのペアのリストに変換
    /// </summary>
    private void ParsePatternString()
    {
        // パターンリストをクリア
        patternList.Clear();

        // 文字列が空または無効な場合
        if (string.IsNullOrEmpty(missilePattern))
        {
            Debug.LogWarning("MissileTempoSpawner: ミサイルパターンが空です。");
            return;
        }

        // 新しいパターン解析ロジック：
        // 1. 大文字A-Hの後に数字1-9が続く場合 -> 指定角度でミサイルを生成
        // 2. 数字0単体の場合 -> 何も生成しない
        Regex regexWithAngle = new Regex(@"([A-H])([1-9])"); // 角度+ミサイル(1-9)
        Regex regexSkip = new Regex(@"0"); // 0単体（スキップ）

        int position = 0;
        while (position < missilePattern.Length)
        {
            // まず角度+ミサイルのパターンをチェック
            Match matchWithAngle = regexWithAngle.Match(missilePattern, position);
            if (matchWithAngle.Success && matchWithAngle.Index == position)
            {
                // 角度とミサイルIDのペアを抽出
                char angleChar = matchWithAngle.Groups[1].Value[0]; // 角度文字 (A-H)
                int missileId = int.Parse(matchWithAngle.Groups[2].Value); // ミサイルID (1-9)

                patternList.Add((angleChar, missileId));
                position += matchWithAngle.Length;
                continue;
            }

            // 次に数字0（スキップ）をチェック
            Match matchSkip = regexSkip.Match(missilePattern, position);
            if (matchSkip.Success && matchSkip.Index == position)
            {
                // スキップを表す特殊値として(X, 0)を追加
                // Xは任意の角度文字（実際には使用されない）
                patternList.Add(('X', 0));
                position += matchSkip.Length;
                continue;
            }

            // どちらにもマッチしない文字はスキップ
            position++;
        }

        // パターンが空の場合のデフォルト設定
        if (patternList.Count == 0)
        {
            Debug.LogWarning("MissileTempoSpawner: 有効なパターンが見つかりません。デフォルトパターン 'A1B2C3D4E5F6G7H8' を使用します。");

            // デフォルトパターンを設定
            patternList.Add(('A', 1));
            patternList.Add(('B', 2));
            patternList.Add(('C', 3));
            patternList.Add(('D', 4));
            patternList.Add(('E', 5));
            patternList.Add(('F', 6));
            patternList.Add(('G', 7));
            patternList.Add(('H', 8));
        }

        // 詳細なデバッグログ
        Debug.Log($"MissileTempoSpawner: パターン文字列 '{missilePattern}' から {patternList.Count} 個のパターンを抽出しました。");
        for (int i = 0; i < patternList.Count; i++)
        {
            if (patternList[i].missileId == 0)
            {
                Debug.Log($"  パターン {i + 1}: スキップ（何も生成しない）");
            }
            else
            {
                Debug.Log($"  パターン {i + 1}: 角度={patternList[i].angle} ({angleMap[patternList[i].angle]}°), ミサイルID={patternList[i].missileId}");
            }
        }
    }

    /// <summary>
    /// テンポイベント発生時の処理
    /// </summary>
    private void TempoMethod()
    {
        // プーラーが見つからない場合は何もしない
        if (missilePooler == null) return;

        // パターンリストが空の場合は何もしない
        if (patternList.Count == 0) return;

        // 現在のビートに対応するミサイルと角度を取得
        var currentPattern = patternList[currentBeatIndex];
        char angleChar = currentPattern.angle;
        int missileId = currentPattern.missileId;

        // ミサイルIDが有効（1-9）なら生成、0ならスキップ
        if (missileId >= 1 && missileId <= 9)
        {
            SpawnMissile(missileId, angleChar);
        }
        else
        {
            // ID=0の場合はスキップするがデバッグログは出力
            Debug.Log($"MissileTempoSpawner: ビート {currentBeatIndex + 1} はスキップします");
        }

        // 次のビートインデックスに進む（循環）
        currentBeatIndex = (currentBeatIndex + 1) % patternList.Count;

        // ビジュアルフィードバック（生成時に少し拡大する）
        StartCoroutine(PulseScale());
    }

    /// <summary>
    /// 指定した角度文字に基づく回転を生成（生成位置とカメラ位置を結ぶ線を中心軸とした角度、0度は常に上方向）
    /// </summary>
    private Quaternion GenerateRotationFromAngle(char angleChar)
    {
        // 角度マッピングから角度を取得
        float angle = 0f;
        if (!angleMap.TryGetValue(angleChar, out angle))
        {
            // マッピングにない場合は0度（上方向）をデフォルトとする
            Debug.LogWarning($"MissileTempoSpawner: 角度文字 '{angleChar}' が不明です。デフォルト角度(0°)を使用します。");
            angle = 0f;
        }

        // カメラの参照を取得
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("MissileTempoSpawner: カメラが見つかりません。デフォルトの回転を使用します。");
            return Quaternion.identity;
        }

        // 生成位置（またはスポナー位置）とカメラ位置を結ぶ軸
        Vector3 spawnPosition;
        if (spawnDistance <= 0f)
        {
            spawnPosition = transform.position;
        }
        else
        {
            spawnPosition = transform.position + transform.forward * spawnDistance;
        }

        // 生成位置からカメラ位置への方向ベクトル（中心軸）
        Vector3 axisToCameraDirection = (mainCamera.transform.position - spawnPosition).normalized;

        // 中心軸周りの角度を生成するための準備
        // 0度は常に上方向（12時）として指定するために調整
        // 水平面上での方向として扱うため、Y軸上方向を基準に
        Vector3 worldUp = Vector3.up;

        // 生成位置からカメラへの方向を水平面に投影して基準方向を決定
        Vector3 projectedDirection = new Vector3(axisToCameraDirection.x, 0, axisToCameraDirection.z).normalized;

        // 投影ができない場合（カメラが真上または真下にある場合）
        if (projectedDirection.magnitude < 0.01f)
        {
            // カメラが真上/真下の場合は、任意の水平方向を選択
            projectedDirection = Vector3.forward;
        }

        // 0度を12時方向（上）、90度を3時方向（右）とするための調整
        // 基準となる12時方向（上方向）と3時方向（右方向）を計算
        Vector3 twelveOClock = worldUp;
        Vector3 threeOClock = Vector3.Cross(axisToCameraDirection, worldUp).normalized;

        // もし軸方向とほぼ平行な場合は別の方向を使用
        if (threeOClock.magnitude < 0.01f)
        {
            // 軸方向が上方向とほぼ平行の場合、前方向を基準に
            threeOClock = Vector3.Cross(axisToCameraDirection, Vector3.forward).normalized;
            if (threeOClock.magnitude < 0.01f)
            {
                // それでも平行なら右方向を使用
                threeOClock = Vector3.Cross(axisToCameraDirection, Vector3.right).normalized;
            }
        }

        // 9時方向（左）を計算
        Vector3 nineOClock = -threeOClock;

        // 6時方向（下）を計算
        Vector3 sixOClock = -twelveOClock;

        // 角度に応じて方向ベクトルを補間
        Vector3 directionVector;

        // 角度を調整（0度が12時方向、90度が3時方向になるよう回転）
        // 12時方向を0度にするために-90度回転
        float adjustedAngle = angle - 90f;
        if (adjustedAngle < 0f) adjustedAngle += 360f;

        // 角度に応じて方向ベクトルを計算
        float angleRad = adjustedAngle * Mathf.Deg2Rad;

        // 極座標からデカルト座標に変換
        // X軸を3時方向、Y軸を12時方向として計算
        Vector3 directionInClockPlane = threeOClock * Mathf.Cos(angleRad) + twelveOClock * Mathf.Sin(angleRad);

        // 最終的な方向ベクトル
        directionVector = directionInClockPlane.normalized;

        // 方向ベクトルを向く回転を計算
        Quaternion rotation = Quaternion.LookRotation(directionVector);

        return rotation;
    }

    /// <summary>
    /// 指定したミサイルIDと角度でミサイルを生成
    /// </summary>
    private void SpawnMissile(int missileId, char angleChar)
    {
        // 生成位置を決定
        Vector3 spawnPosition;

        if (spawnDistance <= 0f)
        {
            // 自分の位置
            spawnPosition = transform.position;
        }
        else
        {
            // 自分の位置から指定距離だけ前方
            spawnPosition = transform.position + transform.forward * spawnDistance;
        }

        // 指定角度で回転を生成
        Quaternion spawnRotation = GenerateRotationFromAngle(angleChar);

        // ミサイルを生成（指定角度の回転を適用）
        GameObject missile = missilePooler.SpawnMissileAtPosition(missileId, spawnPosition, spawnRotation);

        if (missile != null)
        {
            // HomingObjectコンポーネントがあれば初期化処理を呼び出す
            HomingObject homingObject = missile.GetComponent<HomingObject>();
            if (homingObject != null)
            {
                homingObject.Init();
            }

            // 生成に成功
            string missileName = missilePooler.GetMissileNameById(missileId);
            Debug.Log($"MissileTempoSpawner: ミサイル {missileName} (ID:{missileId}) を角度 {angleChar} ({angleMap[angleChar]}°) で生成しました (パターン位置: {currentBeatIndex + 1}/{patternList.Count})");
        }
        else
        {
            Debug.LogWarning($"MissileTempoSpawner: ミサイル ID {missileId} の生成に失敗しました。プールが空か、IDが無効です。");
        }
    }

    /// <summary>
    /// ビートに合わせてオブジェクトを一瞬拡大するアニメーション
    /// </summary>
    private IEnumerator PulseScale()
    {
        // 拡大
        transform.localScale = originalScale * 1.2f;

        // 少し待機
        yield return new WaitForSeconds(0.1f);

        // 元のサイズに戻す
        transform.localScale = originalScale;
    }

    /// <summary>
    /// パターンを外部から設定するメソッド
    /// </summary>
    public void SetMissilePattern(string newPattern)
    {
        missilePattern = newPattern;
        ParsePatternString();
        currentBeatIndex = 0;

        Debug.Log($"MissileTempoSpawner: パターンを '{newPattern}' に変更しました。");
    }

    /// <summary>
    /// 現在のパターンを取得
    /// </summary>
    public string GetCurrentPattern()
    {
        return missilePattern;
    }

    /// <summary>
    /// パターンの長さを取得
    /// </summary>
    public int GetPatternLength()
    {
        return patternList.Count;
    }

    /// <summary>
    /// 現在のビート位置をリセット
    /// </summary>
    public void ResetBeatPosition()
    {
        currentBeatIndex = 0;
    }

    /// <summary>
    /// 角度設定を取得
    /// </summary>
    public Dictionary<char, float> GetAngleSettings()
    {
        return new Dictionary<char, float>(angleMap);
    }
}