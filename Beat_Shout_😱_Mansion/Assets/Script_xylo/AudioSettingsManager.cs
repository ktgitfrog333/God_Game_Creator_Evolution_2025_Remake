using UnityEngine;
using System.IO;

/// <summary>
/// オーディオ設定の保存と読み込みを管理する静的クラス
/// </summary>
public static class AudioSettingsManager
{
    // 設定を保存するPlayerPrefsのキー
    private const string SETTINGS_KEY = "AudioSettings";

    /// <summary>
    /// オーディオ設定を読み込みます
    /// 保存されている設定がない場合はデフォルト値を返します
    /// </summary>
    /// <returns>読み込まれたオーディオ設定</returns>
    public static AudioSettingsData LoadSettings()
    {
        // PlayerPrefsから設定を読み込む
        if (PlayerPrefs.HasKey(SETTINGS_KEY))
        {
            string json = PlayerPrefs.GetString(SETTINGS_KEY);
            try
            {
                // JSONから設定をデシリアライズ
                AudioSettingsData settings = JsonUtility.FromJson<AudioSettingsData>(json);

                // 念のため値の範囲を確認
                settings.ClampValues();

                Debug.Log($"オーディオ設定を読み込みました: {settings}");
                return settings;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"オーディオ設定の読み込み中にエラーが発生しました: {e.Message}");
                // エラー時はデフォルト設定を返す
                return AudioSettingsData.CreateDefault();
            }
        }
        else
        {
            // 保存された設定がない場合はデフォルト設定を返す
            Debug.Log("保存されたオーディオ設定がないため、デフォルト設定を使用します");
            return AudioSettingsData.CreateDefault();
        }
    }

    /// <summary>
    /// オーディオ設定を保存します
    /// </summary>
    /// <param name="settings">保存するオーディオ設定</param>
    /// <returns>保存が成功したかどうか</returns>
    public static bool SaveSettings(AudioSettingsData settings)
    {
        if (settings == null)
        {
            Debug.LogError("保存するオーディオ設定がnullです");
            return false;
        }

        try
        {
            // 念のため値の範囲を確認
            settings.ClampValues();

            // 設定をJSONにシリアライズ
            string json = JsonUtility.ToJson(settings);

            // PlayerPrefsに保存
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();

            Debug.Log($"オーディオ設定を保存しました: {settings}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"オーディオ設定の保存中にエラーが発生しました: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 設定をデフォルト値にリセットして保存します
    /// </summary>
    /// <returns>デフォルトのオーディオ設定</returns>
    public static AudioSettingsData ResetToDefault()
    {
        AudioSettingsData defaultSettings = AudioSettingsData.CreateDefault();
        SaveSettings(defaultSettings);
        Debug.Log("オーディオ設定をデフォルト値にリセットしました");
        return defaultSettings;
    }

    /// <summary>
    /// 設定ファイルが存在するかどうかを確認します
    /// </summary>
    /// <returns>設定ファイルが存在する場合はtrue</returns>
    public static bool SettingsExist()
    {
        return PlayerPrefs.HasKey(SETTINGS_KEY);
    }

    /// <summary>
    /// 全ての設定を削除します（デバッグ用）
    /// </summary>
    public static void DeleteAllSettings()
    {
        if (PlayerPrefs.HasKey(SETTINGS_KEY))
        {
            PlayerPrefs.DeleteKey(SETTINGS_KEY);
            PlayerPrefs.Save();
            Debug.Log("オーディオ設定を削除しました");
        }
    }

    /// <summary>
    /// 現在の設定をログに出力します（デバッグ用）
    /// </summary>
    public static void LogCurrentSettings()
    {
        if (PlayerPrefs.HasKey(SETTINGS_KEY))
        {
            string json = PlayerPrefs.GetString(SETTINGS_KEY);
            Debug.Log($"現在のオーディオ設定（JSON）: {json}");
        }
        else
        {
            Debug.Log("保存されたオーディオ設定はありません");
        }
    }
}