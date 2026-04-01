using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CriWare;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// CRIWARE APIを使用したマイク入力管理クラス
/// 音量レベルの検出と表示を行う
/// </summary>
public class MicInput_Criware : MonoBehaviour
{
    [Header("マイク設定")]
    public int sampleSize = 1024; // FFT解析用サンプルサイズ
    public float requiredDuration = 1.0f; // 一定時間音量を超え続ける必要時間

    [Header("マイク感度設定")]
    [Range(0.1f, 25.0f)]
    public float microphoneSensitivity = 1.0f; // マイク感度（1.0が標準）

    [Header("大声検知設定（変化量ベース）")]
    public float volumeChangeThreshold = 0.15f; // 音量変化の閾値
    public float comparisonDuration = 0.1f; // 比較する時間間隔（秒）

    [Header("BGM除去設定")]
    public CriAtomSource bgmAtomSource; // CRIWAREのBGM再生用AtomSource
    [Range(0.0f, 2.0f)]
    public float bgmCancellationStrength = 1.0f; // BGM除去の強度（1.0が標準）
    public bool enableBgmCancellation = true; // BGM除去機能の有効/無効
    [Range(0.0f, 1.0f)]
    public float bgmVolumeThreshold = 0.01f; // BGM音量の最小閾値（これ以下は無視）
    [Range(0.0f, 1.0f)]
    public float bgmVolumeEstimateMultiplier = 0.3f; // BGM音量推定用の係数

    [Header("音量平均化設定")]
    public float averagingDuration = 0.2f; // スライダー表示用の平均化時間（秒）

    [Header("音量評価")]
    public float[] volumeThresholds = new float[4]; // 声量評価用の閾値

    [Header("UI参照")]
    public TextMeshProUGUI text; // 音量表示用のTextMeshPro
    public Slider volumeSlider; // 音量ゲージ用のスライダー
    public Image sliderFillImage; // スライダーの塗りつぶし部分のImage（色変更用）

    [Header("スライダー色設定")]
    public Color level1Color = Color.green; // レベル1未満の色（緑）
    public Color level2Color = Color.blue; // レベル1～2の色（青）
    public Color level3Color = Color.yellow; // レベル2～3の色（黄色）
    public Color level4Color = Color.red; // レベル3～4の色（赤）

    private CriAtomExMic mic; // CRIWAREのマイク入力
    private float volumeAccumulation = 0f; // 音量合計
    private float volumeAccumulationStartTime = 0f; // 音量計測開始時間
    private bool isMeasuring = false; // 計測中かどうか
    private bool isMicActive = true; // マイク入力が有効かどうか

    // スライダー表示用の音量平均化
    private Queue<float> volumeHistory = new Queue<float>(); // 音量履歴
    private Queue<float> timeHistory = new Queue<float>(); // 時間履歴
    private float totalVolume = 0f; // 履歴の音量合計

    // 大声検知用の変化量計算
    private Queue<float> changeDetectionVolumeHistory = new Queue<float>(); // 変化検知用音量履歴
    private Queue<float> changeDetectionTimeHistory = new Queue<float>(); // 変化検知用時間履歴
    private float changeDetectionTotalVolume = 0f; // 変化検知用音量合計

    // BGM音量監視用
    private float currentBgmVolume = 0f; // 現在のBGM音量（推定値）
    private float lastVolumeUpdateTime = 0f; // 前回の音量更新時刻
    private const float VOLUME_UPDATE_INTERVAL = 0.1f; // 音量更新間隔（秒）


    // シングルトン
    public static MicInput_Criware Instance { get; private set; }

    
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        StartCoroutine(InitializeMicrophoneWithDelay());

        // スライダーの範囲を設定
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f; // 最小値
            volumeSlider.maxValue = 2f; // 音量が1を超える可能性を考慮して2に設定
            volumeSlider.value = 0f;
        }

        // スライダーのFillImageを自動取得（設定されていない場合）
        if (sliderFillImage == null && volumeSlider != null)
        {
            Transform fillArea = volumeSlider.transform.Find("Fill Area");
            if (fillArea != null)
            {
                Transform fill = fillArea.Find("Fill");
                if (fill != null)
                {
                    sliderFillImage = fill.GetComponent<Image>();
                }
            }
        }
    }

    /// <summary>
    /// 遅延を入れてマイクを初期化するコルーチン
    /// </summary>
    private IEnumerator InitializeMicrophoneWithDelay()
    {
        yield return null; // 1フレーム待つ

        // CRIWAREのマイクモジュールを初期化（まだ初期化されていない場合）
        if (!CriAtomExMic.isInitialized)
        {
            CriAtomExMic.InitializeModule();
        }

        InitializeMicrophone();
    }

    void Update()
    {
        // マイクが有効な場合のみ音量をチェック
        if (isMicActive && mic != null)
        {
            CheckVolume();
        }

        // BGM音量を更新
        if (enableBgmCancellation)
        {
            UpdateBgmVolume();
        }
    }

    /// <summary>
    /// マイク入力の有効/無効を切り替えるメソッド
    /// </summary>
    /// <param name="active">有効にする場合はtrue、無効にする場合はfalse</param>
    public void SetMicrophoneActive(bool active)
    {
        // 既に同じ状態なら何もしない
        if (isMicActive == active)
            return;

        isMicActive = active;

        if (active)
        {
            // マイク入力を再開
            if (mic == null)
            {
                // マイクが初期化されていない場合は初期化から行う
                StartCoroutine(InitializeMicrophoneWithDelay());
            }
            else
            {
                // マイクが既に初期化されている場合は録音を再開
                mic.Start();
                Debug.Log("マイク入力を再開しました");
            }
        }
        else
        {
            // マイク入力を停止
            if (mic != null)
            {
                mic.Stop();
                Debug.Log("マイク入力を停止しました");
            }

            // UI表示をリセット
            if (text != null)
                text.text = "Vol: 0.00";
            if (volumeSlider != null)
                volumeSlider.value = 0f;

            // スライダーの色をリセット（レベル1の色に）
            if (sliderFillImage != null)
                sliderFillImage.color = level1Color;

            // 音量履歴をクリア
            ClearVolumeHistory();
            ClearChangeDetectionHistory();
        }
    }

    /// <summary>
    /// マイクを初期化するメソッド
    /// </summary>
    void InitializeMicrophone()
    {
        // マイク入力が無効な場合は初期化しない
        if (!isMicActive)
            return;

        // マイクのデバイスが利用可能か確認
        if (!CriAtomExMic.isInitialized)
        {
            Debug.LogError("CRIWAREのマイクモジュールが初期化されていません！");
            return;
        }

        var devices = CriAtomExMic.GetDevices();
        if (devices == null || devices.Length == 0)
        {
            Debug.LogError("利用可能なマイクデバイスが見つかりません！");
            return;
        }

        // 既存のマイクインスタンスをクリーンアップ
        if (mic != null)
        {
            mic.Stop();
            mic.Dispose();
            mic = null;
        }

        // マイクの設定を作成
        var config = CriAtomExMic.Config.Default;
        config.deviceId = devices[0].deviceId; // 最初のデバイスを使用
        config.numChannels = 1; // モノラル
        config.samplingRate = 44100;
        config.frameSize = (uint)sampleSize;

        // CRI AtomExMic の初期化
        mic = CriAtomExMic.Create(config);

        if (mic == null)
        {
            Debug.LogError("CRIWAREのマイク入力の初期化に失敗しました！");
            return;
        }

        mic.Start(); // マイク入力を開始
        Debug.Log("CRIWAREのマイク入力を開始しました！");
    }

    /// <summary>
    /// マイク入力の音量をチェックするメソッド
    /// </summary>
    /// <summary>
    /// マイク入力の音量をチェックするメソッド
    /// </summary>
    /// <summary>
    /// マイク入力の音量をチェックするメソッド
    /// </summary>
    void CheckVolume()
    {
        if (mic == null) return;

        float[] micBuffer = new float[sampleSize];
        uint samplesRead = mic.ReadData(micBuffer, (uint)sampleSize);

        if (samplesRead > 0)
        {
            float instantVolume = CalculateRMS(micBuffer, (int)samplesRead);

            // マイク感度を適用
            instantVolume *= microphoneSensitivity;

            // BGM除去処理
            if (enableBgmCancellation)
            {
                instantVolume = ApplyBgmCancellation(instantVolume);
            }

            // 音量履歴を更新（スライダー表示用）
            UpdateVolumeHistory(instantVolume);

            // 変化検知用履歴を更新
            UpdateChangeDetectionHistory(instantVolume);

            // 平均音量を計算
            float averagedVolume = GetAveragedVolume();

            // 音量をスライダーとテキストに反映（平均化された値を使用）
            if (text != null)
                text.text = $"Vol: {averagedVolume:F2} (BGM: {currentBgmVolume:F2})";

            // スライダーの値を設定（volumeThresholdsの1段階目に達しない場合は0にする）
            if (volumeSlider != null)
            {
                // 音量レベルを評価
                int volumeLevel = GetVolumeDisplayLevel(averagedVolume);

                if (volumeLevel == 0)
                {
                    // レベル0（1段階目に達しない）の場合はスライダーを0にする
                    volumeSlider.value = 0f;
                }
                else
                {
                    // 1段階目以上の場合、volumeThresholds[0]を基準とした相対値を表示
                    float baseThreshold = (volumeThresholds != null && volumeThresholds.Length > 0)
                                          ? volumeThresholds[0]
                                          : 1.0f;
                    float sliderValue = averagedVolume - baseThreshold;
                    volumeSlider.value = Mathf.Max(0f, sliderValue); // 負の値にならないように制限
                }
            }

            // スライダーの色を音量レベルに応じて変更
            UpdateSliderColor(averagedVolume);

            // 大声検知の判定は音量変化量を使用
            float volumeChange = GetVolumeChange();
            if (volumeChange > volumeChangeThreshold)
            {
                if (!isMeasuring)
                {
                    isMeasuring = true;
                    volumeAccumulation = 0f;
                    volumeAccumulationStartTime = Time.time;
                    Debug.Log($"音量変化を検出: {volumeChange:F3} (閾値: {volumeChangeThreshold:F3})");
                }

                volumeAccumulation += instantVolume;

                if (Time.time - volumeAccumulationStartTime >= requiredDuration)
                {
                    float averageVolume = volumeAccumulation / (Time.time - volumeAccumulationStartTime);
                    int volumeLevel = EvaluateVolumeLevel(averageVolume);
                    Debug.Log($"大声検知完了: 平均音量 {averageVolume:F3}, レベル {volumeLevel}");

                    isMeasuring = false;
                }
            }
            else
            {
                if (isMeasuring)
                {
                    // 音量変化が閾値を下回った場合、測定をリセット
                    isMeasuring = false;
                }
            }
        }
    }

    /// <summary>
    /// 音量履歴を更新するメソッド
    /// </summary>
    private void UpdateVolumeHistory(float volume)
    {
        float currentTime = Time.time;

        // 新しい音量と時間を追加
        volumeHistory.Enqueue(volume);
        timeHistory.Enqueue(currentTime);
        totalVolume += volume;

        // 古いデータを削除（averagingDuration秒より古いデータ）
        while (timeHistory.Count > 0 && currentTime - timeHistory.Peek() > averagingDuration)
        {
            timeHistory.Dequeue();
            totalVolume -= volumeHistory.Dequeue();
        }
    }

    /// <summary>
    /// 平均化された音量を取得するメソッド
    /// </summary>
    public float GetAveragedVolume()
    {
        if (volumeHistory.Count == 0)
            return 0f;

        return totalVolume / volumeHistory.Count;
    }

    /// <summary>
    /// 音量履歴をクリアするメソッド
    /// </summary>
    private void ClearVolumeHistory()
    {
        volumeHistory.Clear();
        timeHistory.Clear();
        totalVolume = 0f;
    }

    /// <summary>
    /// 変化検知用の音量履歴を更新するメソッド
    /// </summary>
    private void UpdateChangeDetectionHistory(float volume)
    {
        float currentTime = Time.time;

        // 新しい音量と時間を追加
        changeDetectionVolumeHistory.Enqueue(volume);
        changeDetectionTimeHistory.Enqueue(currentTime);
        changeDetectionTotalVolume += volume;

        // 古いデータを削除（comparisonDuration * 2秒より古いデータ）
        // 0.1秒前の平均と現在の平均を比較するために、0.2秒分のデータを保持
        float keepDuration = comparisonDuration * 2.0f;
        while (changeDetectionTimeHistory.Count > 0 && currentTime - changeDetectionTimeHistory.Peek() > keepDuration)
        {
            changeDetectionTimeHistory.Dequeue();
            changeDetectionTotalVolume -= changeDetectionVolumeHistory.Dequeue();
        }
    }

    /// <summary>
    /// 音量変化量を取得するメソッド
    /// 0.1秒前の平均値と現在から0.1秒前までの平均値の差を計算
    /// </summary>
    private float GetVolumeChange()
    {
        if (changeDetectionVolumeHistory.Count == 0)
            return 0f;

        float currentTime = Time.time;
        float pastTime = currentTime - comparisonDuration; // 0.1秒前の時刻

        // 現在から0.1秒前までの平均を計算
        float recentSum = 0f;
        int recentCount = 0;
        Queue<float> tempVolumeQueue = new Queue<float>(changeDetectionVolumeHistory);
        Queue<float> tempTimeQueue = new Queue<float>(changeDetectionTimeHistory);

        while (tempTimeQueue.Count > 0)
        {
            float time = tempTimeQueue.Dequeue();
            float volume = tempVolumeQueue.Dequeue();

            if (time >= pastTime) // 0.1秒前以降のデータ
            {
                recentSum += volume;
                recentCount++;
            }
        }

        if (recentCount == 0)
            return 0f;

        float recentAverage = recentSum / recentCount;

        // 0.1秒前の時点での平均を計算
        float pastSum = 0f;
        int pastCount = 0;
        tempVolumeQueue = new Queue<float>(changeDetectionVolumeHistory);
        tempTimeQueue = new Queue<float>(changeDetectionTimeHistory);

        while (tempTimeQueue.Count > 0)
        {
            float time = tempTimeQueue.Dequeue();
            float volume = tempVolumeQueue.Dequeue();

            if (time < pastTime) // 0.1秒前より古いデータ
            {
                pastSum += volume;
                pastCount++;
            }
        }

        if (pastCount == 0)
            return 0f; // 比較対象がない場合は変化なしとする

        float pastAverage = pastSum / pastCount;

        // 変化量を計算（現在の平均 - 過去の平均）
        return recentAverage - pastAverage;
    }

    /// <summary>
    /// 音量レベルに応じてスライダーの色を更新するメソッド
    /// </summary>
    private void UpdateSliderColor(float volume)
    {
        if (sliderFillImage == null) return;

        // volumeThresholdsに基づいてレベルを判定
        int level = GetVolumeDisplayLevel(volume);

        Color targetColor;
        switch (level)
        {
            case 1:
                targetColor = level2Color; // 1～2: 青
                break;
            case 2:
                targetColor = level3Color; // 2～3: 黄色
                break;
            case 3:
            case 4:
                targetColor = level4Color; // 3～4: 赤
                break;
            default:
                targetColor = level1Color; // 1未満: 緑
                break;
        }

        sliderFillImage.color = targetColor;
    }

    /// <summary>
    /// 音量からスライダー表示用のレベルを取得するメソッド
    /// volumeThresholdsの設定値と比較して判定
    /// </summary>
    private int GetVolumeDisplayLevel(float volume)
    {
        // volumeThresholdsが設定されていない場合のデフォルト値
        if (volumeThresholds == null || volumeThresholds.Length == 0)
        {
            // デフォルトの閾値で判定
            if (volume < 1.0f) return 0; // 1未満
            if (volume < 2.0f) return 1; // 1～2
            if (volume < 3.0f) return 2; // 2～3
            return 3; // 3以上
        }

        // volumeThresholdsの設定値で判定
        for (int i = 0; i < volumeThresholds.Length; i++)
        {
            if (volume <= volumeThresholds[i])
            {
                return i; // 0～3の値を返す
            }
        }
        return volumeThresholds.Length; // 全ての閾値を超えた場合
    }

    /// <summary>
    /// BGM音量を更新するメソッド（CRIWARE対応）
    /// </summary>
    private void UpdateBgmVolume()
    {
        // 更新間隔をチェック
        if (Time.time - lastVolumeUpdateTime < VOLUME_UPDATE_INTERVAL)
        {
            return;
        }
        lastVolumeUpdateTime = Time.time;

        if (bgmAtomSource == null)
        {
            currentBgmVolume = 0f;
            return;
        }

        // CRIWAREのAtomSourceから音量情報を取得
        // ※CRIWAREでは直接オーディオデータを取得できないため、
        // 設定されているボリューム値から推定する方法を使用

        // AtomSourceの再生状態をチェック
        if (bgmAtomSource.status == CriAtomSource.Status.Playing)
        {
            // 各種ボリューム設定を取得
            float volume = bgmAtomSource.volume; // 基本ボリューム

            // 追加のボリューム設定がある場合は考慮
            // （ゲーム内設定やAISACなどの動的パラメータも影響する可能性がある）

            // BGM音量を推定（実際のスピーカー出力レベルに近づけるための係数を適用）
            currentBgmVolume = volume * bgmVolumeEstimateMultiplier;
        }
        else
        {
            currentBgmVolume = 0f;
        }
    }

    /// <summary>
    /// BGM除去処理を適用するメソッド
    /// </summary>
    private float ApplyBgmCancellation(float micVolume)
    {
        // BGM音量が閾値以下の場合は除去処理をスキップ
        if (currentBgmVolume < bgmVolumeThreshold)
        {
            return micVolume;
        }

        // BGM音量を除去強度で調整して差し引く
        float adjustedBgmVolume = currentBgmVolume * bgmCancellationStrength;
        float cancelledVolume = micVolume - adjustedBgmVolume;

        // 結果が負の値にならないように制限
        return Mathf.Max(0f, cancelledVolume);
    }

    /// <summary>
    /// 変化検知用音量履歴をクリアするメソッド
    /// </summary>
    private void ClearChangeDetectionHistory()
    {
        changeDetectionVolumeHistory.Clear();
        changeDetectionTimeHistory.Clear();
        changeDetectionTotalVolume = 0f;
    }

    /// <summary>
    /// 平均音量からボリュームレベルを評価するメソッド
    /// </summary>
    int EvaluateVolumeLevel(float averageVolume)
    {
        for (int i = 0; i < volumeThresholds.Length; i++)
        {
            if (averageVolume <= volumeThresholds[i])
            {
                return i + 1; // 1～4の評価
            }
        }
        return volumeThresholds.Length; // 最大値を超えた場合は最高評価
    }

    /// <summary>
    /// サンプルデータから平方平均平方根（RMS）を計算するメソッド
    /// </summary>
    float CalculateRMS(float[] samples, int length)
    {
        float sum = 0f;
        for (int i = 0; i < length; i++)
        {
            sum += samples[i] * samples[i];
        }
        return Mathf.Sqrt(sum / length);
    }

    void OnDestroy()
    {
        if (mic != null)
        {
            mic.Stop();
            mic.Dispose();
            mic = null;
        }

        // CRIWAREのマイクモジュールを終了
        if (CriAtomExMic.isInitialized)
        {
            CriAtomExMic.FinalizeModule();
        }
    }
}