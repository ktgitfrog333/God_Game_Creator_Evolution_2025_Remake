using UnityEngine;

/// <summary>
/// オーディオ設定データを保持するクラス
/// シリアライズして永続化します
/// </summary>
[System.Serializable]
public class AudioSettingsData
{
    // デフォルト値の定数
    private const float DEFAULT_BGM_VOLUME = 1.0f;
    private const float DEFAULT_SE_VOLUME = 1.0f;
    private const bool DEFAULT_MIC_INPUT_ENABLED = true;
    private const bool DEFAULT_VIBRATION_ENABLED = true; // 追加

    // BGM音量 (0.0 - 1.0)
    public float bgmVolume = DEFAULT_BGM_VOLUME;

    // SE音量 (0.0 - 1.0)
    public float seVolume = DEFAULT_SE_VOLUME;

    // マイク入力の有効/無効
    public bool micInputEnabled = DEFAULT_MIC_INPUT_ENABLED;

    // 振動の有効/無効（新規追加）
    public bool vibrationEnabled = DEFAULT_VIBRATION_ENABLED;

    /// <summary>
    /// デフォルト設定のインスタンスを作成します
    /// </summary>
    /// <returns>デフォルト設定のインスタンス</returns>
    public static AudioSettingsData CreateDefault()
    {
        return new AudioSettingsData
        {
            bgmVolume = DEFAULT_BGM_VOLUME,
            seVolume = DEFAULT_SE_VOLUME,
            micInputEnabled = DEFAULT_MIC_INPUT_ENABLED,
            vibrationEnabled = DEFAULT_VIBRATION_ENABLED // 追加
        };
    }

    /// <summary>
    /// 設定のコピーを作成します
    /// </summary>
    /// <returns>設定のコピー</returns>
    public AudioSettingsData Clone()
    {
        return new AudioSettingsData
        {
            bgmVolume = this.bgmVolume,
            seVolume = this.seVolume,
            micInputEnabled = this.micInputEnabled,
            vibrationEnabled = this.vibrationEnabled // 追加
        };
    }

    /// <summary>
    /// 音量値を有効な範囲（0-1）に制限します
    /// </summary>
    public void ClampValues()
    {
        bgmVolume = Mathf.Clamp01(bgmVolume);
        seVolume = Mathf.Clamp01(seVolume);
    }

    /// <summary>
    /// 設定の概要を文字列で返します
    /// </summary>
    /// <returns>設定の概要</returns>
    public override string ToString()
    {
        return $"AudioSettings[BGM:{bgmVolume:F2}, SE:{seVolume:F2}, MicInput:{(micInputEnabled ? "ON" : "OFF")}, Vibration:{(vibrationEnabled ? "ON" : "OFF")}]";
    }
}