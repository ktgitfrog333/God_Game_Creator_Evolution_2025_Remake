using CriWare;
using UnityEngine;

/// <summary>
/// 3D空間でSEを再生するためのコンポーネント
/// CriAtomSourceを使用して位置に応じた音声を再生する
/// </summary>
public class Se_3D_Picker : MonoBehaviour
{
    private bool ActiveNow = false;

    [Header("CRIWARE設定")]
    [Tooltip("CriAtomSourceコンポーネントへの参照")]
    public CriAtomSource source; // CriAtomSourceコンポーネントへの参照

    // マスター音量（SEと同じ値を使用）
    private float masterSEVolume = 1.0f;

    private void Start()
    {
        // 保存されているSE音量を読み込む
        LoadMasterSEVolume();
    }

    private void OnEnable()
    {
        // コンポーネントがアクティブになった時に音量を更新
        LoadMasterSEVolume();
    }

    private void OnDisable()
    {
        // 非アクティブになる前に必要な処理
        CleanUp();
    }

    /// <summary>
    /// マスターSE音量を読み込むメソッド
    /// AudioSettingsManagerから設定を取得
    /// </summary>
    private void LoadMasterSEVolume()
    {
        // AudioSettingsManagerから設定を読み込む
        AudioSettingsData settings = AudioSettingsManager.LoadSettings();
        masterSEVolume = settings.seVolume;
        Debug.Log($"Se_3D_Picker: マスターSE音量を読み込みました: {masterSEVolume:F1}");
    }

    /// <summary>
    /// マスターSE音量を設定するメソッド（AudioSettingsControllerから呼び出される）
    /// </summary>
    /// <param name="volume">設定する音量 (0.0 - 1.0)</param>
    public void SetMasterSEVolume(float volume)
    {
        float previousVolume = masterSEVolume;
        masterSEVolume = Mathf.Clamp(volume, 0f, 1f);

        // 既に再生中の音の音量も更新
        if (source != null && source.status == CriAtomSource.Status.Playing)
        {
            // 以前の音量で割って、新しい音量を掛ける（比率を保持）
            float volumeRatio = previousVolume > 0 ? masterSEVolume / previousVolume : masterSEVolume;
            source.volume = source.volume * volumeRatio;
        }

        Debug.Log($"Se_3D_Picker: マスターSE音量を設定しました: {masterSEVolume:F1}");
    }

    /// <summary>
    /// リソースを解放するメソッド
    /// </summary>
    public void CleanUp()
    {
        // 再生中の場合は停止
        if (ActiveNow && source != null && source.status == CriAtomSource.Status.Playing)
        {
            source.Stop();
        }
        ActiveNow = false;
    }

    /// <summary>
    /// 指定されたキュー名が再生可能かどうかを確認するメソッド
    /// </summary>
    /// <param name="cueName">チェックするキュー名</param>
    /// <returns>再生可能かどうか</returns>
    private bool IsPlayable(string cueName)
    {
        if (string.IsNullOrEmpty(cueName))
        {
            Debug.LogError("Cue name is null or empty.");
            return false;
        }

        if (!ActiveNow)
        {
            // 自動的にアクティブにする
            ActiveNow = true;
        }

        if (source == null)
        {
            Debug.LogError("CriAtomSource is not assigned.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 指定した音声を再生するメソッド
    /// </summary>
    /// <param name="SeName">再生するキュー名</param>
    /// <param name="volume">個別の音量 (0.0 - 1.0)</param>
    public void PlaySound(string SeName, float volume)
    {
        // CriAtomSourceがアタッチされているか確認
        if (source == null)
        {
            Debug.LogError("CriAtomSource is not assigned.");
            return;
        }

        if (!IsPlayable(SeName)) return;

        // 個別の音量にマスター音量を乗算して最終的な音量を決定
        float finalVolume = volume * masterSEVolume;
        finalVolume = Mathf.Clamp(finalVolume, 0f, 1f); // 音量を制限

        // CriAtomSourceを使用して音を再生
        source.cueName = SeName;
        source.volume = finalVolume;
        source.Play();
    }

    /// <summary>
    /// 現在再生中の音声を停止するメソッド
    /// </summary>
    public void StopSound()
    {
        if (source != null && source.status == CriAtomSource.Status.Playing)
        {
            source.Stop();
        }
    }
}