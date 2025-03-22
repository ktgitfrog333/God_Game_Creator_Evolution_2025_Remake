using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// テンポに合わせてミサイルを順番に生成するスポナー
/// </summary>
public class MissileTempoSpawner : MonoBehaviour
{
    [Header("生成パターン")]
    [Tooltip("8桁の数字でミサイルの生成パターンを指定（0=何も生成しない、1-9=ミサイルID）")]
    [SerializeField] private string missilePattern = "12345678";

    [Header("距離設定")]
    [Tooltip("生成するミサイルの距離（0の場合は自分の位置）")]
    [SerializeField] private float spawnDistance = 0f;

    [Header("角度設定")]
    [Tooltip("ランダム射出の最大角度（°）")]
    [SerializeField] private float maxRandomAngle = 180f;
    [Tooltip("前回の射出方向との最小角度差（°）")]
    [SerializeField] private float minAngleDifference = 70f;

    [Header("参照")]
    [Tooltip("ミサイルプーラーへの参照（空の場合は自動検索）")]
    [SerializeField] private MissileObjectPooler missilePooler;

    // 内部変数
    private int currentBeatIndex = 0;
    private Vector3 originalScale;
    private int[] patternArray = new int[8];
    private Quaternion lastSpawnRotation; // 前回の射出角度を保存
    private bool hasSpawnedBefore = false; // 初回射出判定用


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

        // パターン文字列を解析
        ParsePatternString();

        // 射出角度の初期化
        hasSpawnedBefore = false;
    }

    private void OnEnable()
    {
        // 元のサイズを保存
        originalScale = transform.localScale;

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

        // 射出角度の初期化
        hasSpawnedBefore = false;


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
    /// パターン文字列を解析して整数配列に変換
    /// </summary>
    private void ParsePatternString()
    {
        // パターン配列を初期化
        for (int i = 0; i < patternArray.Length; i++)
        {
            patternArray[i] = 0;
        }

        // 文字列が空または無効な場合
        if (string.IsNullOrEmpty(missilePattern))
        {
            Debug.LogWarning("MissileTempoSpawner: ミサイルパターンが空です。");
            return;
        }

        // 文字列を解析
        for (int i = 0; i < Mathf.Min(missilePattern.Length, patternArray.Length); i++)
        {
            char c = missilePattern[i];

            // 数字かどうかチェック
            if (char.IsDigit(c))
            {
                // 文字を数値に変換（'0'は48なので、'0'を引くと数値になる）
                int value = c - '0';
                patternArray[i] = value;
            }
            else
            {
                Debug.LogWarning($"MissileTempoSpawner: パターンに無効な文字 '{c}' があります。0として扱います。");
                patternArray[i] = 0;
            }
        }

        // デバッグログ
        string pattern = string.Join(", ", patternArray);
      }

    /// <summary>
    /// テンポイベント発生時の処理
    /// </summary>
    private void TempoMethod()
    {
        // プーラーが見つからない場合は何もしない
        if (missilePooler == null) return;

        // 現在のビートに対応するミサイルを生成
        int missileId = patternArray[currentBeatIndex];

        // ミサイルIDが有効（1-9）なら生成
        if (missileId >= 1 && missileId <= 9)
        {
            SpawnMissile(missileId);
        }

        // 次のビートインデックスに進む（循環）
        currentBeatIndex = (currentBeatIndex + 1) % patternArray.Length;

        // ビジュアルフィードバック（生成時に少し拡大する）
        StartCoroutine(PulseScale());
    }

    /// <summary>
    /// 前回の角度と十分に異なるランダムな回転を生成
    /// </summary>
    private Quaternion GenerateRandomRotation()
    {
        // カメラ参照の取得
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return transform.rotation;

        // カメラの前方向ベクトル
        Vector3 cameraForward = mainCamera.transform.forward;

        // カメラの前方向を保持しつつ、垂直面内でのみランダム化
        float randomYawAngle = Random.Range(0f, 360f); // 水平方向は完全にランダム

        // 基準となる回転を計算（カメラ前方向は固定）
        // まず、カメラと同じ前方向を向く回転を計算
        Quaternion lookAtCamera = Quaternion.LookRotation(cameraForward);

        // その回転から、Y軸周りのみランダムに回転させる
        Quaternion randomRotation = lookAtCamera * Quaternion.Euler(0f, randomYawAngle, 0f);

        // 前回の生成方向との差が十分か確認
        if (hasSpawnedBefore &&
            Quaternion.Angle(lastSpawnRotation, randomRotation) < minAngleDifference)
        {
            // 差が十分でない場合は、反対側に生成
            randomRotation = lookAtCamera * Quaternion.Euler(0f, randomYawAngle + 180f, 0f);
        }

        // 生成した回転を保存
        hasSpawnedBefore = true;
        lastSpawnRotation = randomRotation;

        return randomRotation;
    }
    /// <summary>
    /// 指定したミサイルIDのミサイルを生成
    /// </summary>
    private void SpawnMissile(int missileId)
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

        // ランダムな射出角度を生成（前回と30度以上異なる）
        Quaternion spawnRotation = GenerateRandomRotation();

        // ミサイルを生成（ランダムな回転を適用）
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
    }
}