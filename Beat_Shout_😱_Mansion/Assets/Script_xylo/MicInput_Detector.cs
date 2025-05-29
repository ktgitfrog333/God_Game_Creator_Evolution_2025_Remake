using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CriWare;
using System.Collections;

/// <summary>
/// CRIWARE APIを使用したマイク入力管理クラス
/// 音量レベルの検出と表示を行う
/// </summary>
public class MicInput_Criware : MonoBehaviour
{
    [Header("マイク設定")]
    public int sampleSize = 1024; // FFT解析用サンプルサイズ
    public float requiredDuration = 1.0f; // 一定時間音量を超え続ける必要時間
    public float startThreshold = 0.2f; // 音量しきい値

    [Header("音量評価")]
    public float[] volumeThresholds = new float[4]; // 声量評価用の閾値

    [Header("UI参照")]
    public TextMeshProUGUI text; // 音量表示用のTextMeshPro
    public Slider volumeSlider; // 音量ゲージ用のスライダー

    private CriAtomExMic mic; // CRIWAREのマイク入力
    private float volumeAccumulation = 0f; // 音量合計
    private float volumeAccumulationStartTime = 0f; // 音量計測開始時間
    private bool isMeasuring = false; // 計測中かどうか
    private bool isMicActive = true; // マイク入力が有効かどうか

    void Start()
    {
        StartCoroutine(InitializeMicrophoneWithDelay());

        // スライダーの範囲を設定
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f; // 最小値
            volumeSlider.maxValue = 2f; // 音量が1を超える可能性を考慮して2に設定
            volumeSlider.value = 0f;
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
    void CheckVolume()
    {
        if (mic == null) return;

        float[] micBuffer = new float[sampleSize];
        uint samplesRead = mic.ReadData(micBuffer, (uint)sampleSize);

        if (samplesRead > 0)
        {
            float volume = CalculateRMS(micBuffer, (int)samplesRead);

            // 音量をスライダーとテキストに反映
            if (text != null)
                text.text = $"Vol: {volume:F2}";
            if (volumeSlider != null)
                volumeSlider.value = volume; // スライダーに反映

            if (volume > startThreshold)
            {
                if (!isMeasuring)
                {
                    isMeasuring = true;
                    volumeAccumulation = 0f;
                    volumeAccumulationStartTime = Time.time;
                }

                volumeAccumulation += volume;

                if (Time.time - volumeAccumulationStartTime >= requiredDuration)
                {
                    float averageVolume = volumeAccumulation / (Time.time - volumeAccumulationStartTime);
                    int volumeLevel = EvaluateVolumeLevel(averageVolume);
                    text.text = $"Vol: {averageVolume:F2} (Lv {volumeLevel})";
                    Debug.Log($"設定値を超える音量を検出: 平均音量 {averageVolume}, レベル {volumeLevel}");

                    isMeasuring = false;
                }
            }
            else
            {
                isMeasuring = false;
            }
        }
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