using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 音声設定を管理するコントローラー
/// 各シーンに配置して使用します
/// </summary>
public class AudioSettingsController : MonoBehaviour
{
    // 現在のオーディオ設定
    private AudioSettingsData audioSettings;

    [Header("設定")]
    [SerializeField] private bool applyOnStart = true; // Start時に設定を適用するかどうか

    [Header("オプション")]
    [SerializeField] private SliderVolume volumeUIManager; // スライダーUIを管理するコンポーネント
    [SerializeField] private GameObject micInputObject; // マイク入力を含むGameObject

    // AudioSettingsDataへのアクセサ
    public AudioSettingsData Settings => audioSettings;

    private void Start()
    {
        // 設定を読み込み、適用
        if (applyOnStart)
        {
            Initialize();
        }
    }

    /// <summary>
    /// コントローラーを初期化し、設定を読み込んで適用します
    /// </summary>
    public void Initialize()
    {
        // 保存されている設定を読み込む
        LoadSettings();

        // SliderVolumeコンポーネントを初期化
        if (volumeUIManager != null)
        {
            volumeUIManager.Initialize(this);
        }

        // マイク入力の有効/無効を設定
        UpdateMicInputState(audioSettings.micInputEnabled);

        // シーン内の全てのSliderVolumeコンポーネントを検索して更新（volumeUIManager以外も含む）
        if (volumeUIManager == null)
        {
            SliderVolume[] volumeControllers = FindObjectsByType<SliderVolume>(FindObjectsSortMode.None);
            foreach (var controller in volumeControllers)
            {
                if (controller != volumeUIManager) // 重複更新を避ける
                {
                    controller.Initialize(this);
                }
            }
        }
    }

    /// <summary>
    /// 音声設定を読み込み、適用します
    /// </summary>
    public void LoadSettings()
    {
        audioSettings = AudioSettingsManager.LoadSettings();

        // CRIWAREのカテゴリーに音量を設定
        try
        {
            CriWare.CriAtom.SetCategoryVolume("BGM", audioSettings.bgmVolume);
            CriWare.CriAtom.SetCategoryVolume("SE", audioSettings.seVolume);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"音量設定の適用中にエラーが発生しました: {e.Message}");
        }

        // マイク入力の設定を適用
        UpdateMicInputState(audioSettings.micInputEnabled);

        Debug.Log($"音声設定を読み込みました: BGM={audioSettings.bgmVolume}, SE={audioSettings.seVolume}, MicInput={audioSettings.micInputEnabled}");

        // SE_Pickerなどの他のコンポーネントにも音量を適用
        UpdateAllAudioComponents();
    }

    /// <summary>
    /// 音声設定を保存します
    /// </summary>
    public void SaveSettings()
    {
        bool success = AudioSettingsManager.SaveSettings(audioSettings);
        if (success)
        {
            Debug.Log($"音声設定を保存しました: BGM={audioSettings.bgmVolume}, SE={audioSettings.seVolume}, MicInput={audioSettings.micInputEnabled}");
        }
        else
        {
            Debug.LogWarning("音声設定の保存に失敗しました。");
        }
    }

    /// <summary>
    /// BGM音量を更新します
    /// </summary>
    /// <param name="volume">新しいBGM音量 (0.0 - 1.0)</param>
    public void SetBGMVolume(float volume)
    {
        audioSettings.bgmVolume = volume;

        // CRIWAREのBGMカテゴリーに音量を設定
        try
        {
            CriWare.CriAtom.SetCategoryVolume("BGM", volume);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BGM音量の設定中にエラーが発生しました: {e.Message}");
        }

        // SE_Pickerなどの他のコンポーネントにも音量を適用
        UpdateBGMComponents(volume);

        // 設定を保存
        SaveSettings();
    }

    /// <summary>
    /// SE音量を更新します
    /// </summary>
    /// <param name="volume">新しいSE音量 (0.0 - 1.0)</param>
    public void SetSEVolume(float volume)
    {
        audioSettings.seVolume = volume;

        // CRIWAREのSEカテゴリーに音量を設定
        try
        {
            CriWare.CriAtom.SetCategoryVolume("SE", volume);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SE音量の設定中にエラーが発生しました: {e.Message}");
        }

        // SE_Pickerなどの他のコンポーネントにも音量を適用
        UpdateSEComponents(volume);

        // 設定を保存
        SaveSettings();
    }

    /// <summary>
    /// マイク入力の有効/無効を切り替えます
    /// </summary>
    /// <param name="sliderValue">スライダーの値 (0 = 無効, 1 = 有効)</param>
    public void SetMicInputState(float sliderValue)
    {
        // 0または1の値に丸める（念のため）
        int binaryValue = Mathf.RoundToInt(sliderValue);

        // 0=false, 1=trueに変換して設定
        bool isEnabled = binaryValue == 1;
        audioSettings.micInputEnabled = isEnabled;

        // マイク入力の状態を更新
        UpdateMicInputState(isEnabled);

        // 設定を保存
        SaveSettings();

        Debug.Log($"マイク入力を{(isEnabled ? "有効" : "無効")}に設定しました");

        // SliderVolumeに通知（UIに反映）
        if (volumeUIManager != null)
        {
            volumeUIManager.UpdateMicSlider(isEnabled);
        }
    }

    /// <summary>
    /// マイク入力状態を実際のオブジェクトに適用
    /// GameObjectの有効/無効と、MicInput_Criwareのアクティブ状態を切り替えます
    /// </summary>
    private void UpdateMicInputState(bool isEnabled)
    {
        // 指定されたマイク入力オブジェクトを有効/無効化
        if (micInputObject != null)
        {
            // オブジェクトのアクティブ状態を変更
            micInputObject.SetActive(isEnabled);

            // マイク入力コンポーネントのアクティブ状態も変更
            MicInput_Criware micInput = micInputObject.GetComponent<MicInput_Criware>();
            if (micInput != null)
            {
                micInput.SetMicrophoneActive(isEnabled);
            }

            Debug.Log($"マイク入力オブジェクトを{(isEnabled ? "有効" : "無効")}にしました");
        }

        // マイク入力系のオブジェクトを全て検索して設定（オプション）
        if (micInputObject == null)
        {
            // シーン内の全てのMicInput_Criwareコンポーネントを検索
            var micComponents = FindObjectsByType<MicInput_Criware>(FindObjectsSortMode.None);
            foreach (var mic in micComponents)
            {
                if (mic != null)
                {
                    // マイク入力コンポーネントのアクティブ状態を変更
                    mic.SetMicrophoneActive(isEnabled);

                    // 親GameObjectも有効/無効化
                    mic.gameObject.SetActive(isEnabled);
                }
            }
        }
    }

    /// <summary>
    /// 音声設定をデフォルト値にリセットします
    /// </summary>
    public void ResetToDefault()
    {
        audioSettings = AudioSettingsManager.ResetToDefault();

        // CRIWAREのカテゴリーに音量を設定
        try
        {
            CriWare.CriAtom.SetCategoryVolume("BGM", audioSettings.bgmVolume);
            CriWare.CriAtom.SetCategoryVolume("SE", audioSettings.seVolume);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"音量設定の適用中にエラーが発生しました: {e.Message}");
        }

        // マイク入力の設定をリセット
        UpdateMicInputState(audioSettings.micInputEnabled);

        // SE_Pickerなどの他のコンポーネントにも音量を適用
        UpdateAllAudioComponents();

        // SliderVolumeが設定されている場合、そのインスタンスも更新
        if (volumeUIManager != null)
        {
            volumeUIManager.UpdateAllSliders(audioSettings);
        }

        // シーン内の全てのSliderVolumeコンポーネントを検索して更新
        SliderVolume[] volumeControllers = FindObjectsByType<SliderVolume>(FindObjectsSortMode.None);
        foreach (var controller in volumeControllers)
        {
            if (controller != volumeUIManager) // 重複更新を避ける
            {
                controller.UpdateAllSliders(audioSettings);
            }
        }

        Debug.Log("音声設定をデフォルト値にリセットしました");
    }

    /// <summary>
    /// シーン内の全てのオーディオコンポーネントを更新します
    /// </summary>
    private void UpdateAllAudioComponents()
    {
        UpdateBGMComponents(audioSettings.bgmVolume);
        UpdateSEComponents(audioSettings.seVolume);
    }

    /// <summary>
    /// シーン内のBGM関連コンポーネントを更新します
    /// </summary>
    private void UpdateBGMComponents(float volume)
    {
        // CRIWARE_conductorがあれば音量を設定
        var conductors = FindObjectsByType<CRIWARE_conductor>(FindObjectsSortMode.None);
        foreach (var conductor in conductors)
        {
            if (conductor != null)
            {
                conductor.SetMasterBGMVolume(volume);
            }
        }
    }

    /// <summary>
    /// シーン内のSE関連コンポーネントを更新します
    /// </summary>
    private void UpdateSEComponents(float volume)
    {
        // SE_Pickerがあれば音量を設定
        SE_Picker sePicker = SE_Picker.Instance;
        if (sePicker != null)
        {
            sePicker.SetMasterSEVolume(volume);
        }

        // 全てのSe_3D_Pickerに音量を設定
        var pickers = FindObjectsByType<Se_3D_Picker>(FindObjectsSortMode.None);
        foreach (var picker in pickers)
        {
            if (picker != null)
            {
                picker.SetMasterSEVolume(volume);
            }
        }
    }

    /// <summary>
    /// テスト用SEを再生します
    /// </summary>
    public void PlayTestSE()
    {
        SE_Picker sePicker = SE_Picker.Instance;
        if (sePicker != null)
        {
            sePicker.PlayFootStep(1.0f);
            Debug.Log("テストSEを再生しました");
        }
        else
        {
            Debug.LogWarning("SE_Pickerが見つからないため、テストSEを再生できません");
        }
    }
}