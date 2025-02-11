using UnityEngine;
using TMPro;
using CriWare;
using System.Collections;

public class MicInput_Criware : MonoBehaviour
{
    public int sampleSize = 1024; // FFT解析用サンプルサイズ
    public float requiredDuration = 1.0f; // 一定時間音量を超え続ける必要時間
    public float startThreshold = 0.2f; // 音量しきい値

    public float[] volumeThresholds = new float[4]; // 声量評価用の閾値
    public TextMeshProUGUI text; // 音量表示用のTextMeshPro

    private CriAtomExMic mic; // CRIWAREのマイク入力
    private float volumeAccumulation = 0f; // 音量合計
    private float volumeAccumulationStartTime = 0f; // 音量計測開始時間
    private bool isMeasuring = false; // 計測中かどうか

    void Start()
    {
        StartCoroutine(InitializeMicrophoneWithDelay());
    }

    private IEnumerator InitializeMicrophoneWithDelay()
    {
        yield return null; // 1フレーム待つ

        // CRIWAREのマイクモジュールを初期化
        CriAtomExMic.InitializeModule();

        InitializeMicrophone();
    }

    void Update()
    {
        CheckVolume();
    }

    void InitializeMicrophone()
    {
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

    void CheckVolume()
    {
        if (mic == null) return;

        float[] micBuffer = new float[sampleSize];
        uint samplesRead = mic.ReadData(micBuffer, (uint)sampleSize);

        if (samplesRead > 0)
        {
            float volume = CalculateRMS(micBuffer, (int)samplesRead);
            text.text = $"Vol: {volume:F2}";

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
        CriAtomExMic.FinalizeModule();
    }
}
