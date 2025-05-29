using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// BGMとSEの音量をスライダーで調整し、値を保存するクラス
/// CRIWAREのBGMカテゴリーとSE_Picker、Se_3D_Picker、CRIWARE_conductorクラスに音量を反映させる
/// </summary>
public class SliderVolume : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider; // BGM音量用スライダー
    [SerializeField] private Slider seSlider;  // SE音量用スライダー
    [SerializeField] private Slider micSlider; // マイク入力用スライダー（オプション）

    [Header("刻み値設定")]
    [SerializeField] private float volumeStepSize = 0.1f; // 音量の刻み値
    [SerializeField] private bool useSteppedValues = true; // 刻み値を使用するか

    [Header("オプション")]
    [SerializeField] private AudioSettingsController audioController; // 関連するAudioSettingsControllerへの参照

    // 音声設定を保持するデータ
    private AudioSettingsData audioSettings;

    // SE_Pickerへの参照（シングルトンパターンを使用）
    private SE_Picker sePicker;

    // 全てのSe_3D_Pickerコンポーネントを格納するリスト
    private List<Se_3D_Picker> se3DPickers = new List<Se_3D_Picker>();

    // CRIWARE_conductorへの参照（シングルトンパターンを使用）
    private CRIWARE_conductor criwareConductor;

    private void Start()
    {
        // AudioControllerが設定されている場合は、Startで初期化は行わない
        // Initialize メソッドから呼び出される
        if (audioController == null)
        {
            // スクリプトが有効になった時に自動で設定を読み込む
            LoadSettings();

            // スライダーの初期値を設定
            UpdateSlidersFromSettings();

            // スライダーの値が変更された時のイベントリスナーを設定
            SetupSliderEvents();

            // 各種音源管理クラスのインスタンスを取得（Delayが必要かもしれないので遅延実行）
            Invoke("GetSoundManagers", 1.5f);
        }
    }

    /// <summary>
    /// このコンポーネントを初期化します
    /// AudioSettingsController から呼び出されます
    /// </summary>
    /// <param name="controller">関連付けるAudioSettingsController</param>
    public void Initialize(AudioSettingsController controller)
    {
        audioController = controller;

        if (audioController == null)
        {
            Debug.LogError("AudioSettingsControllerが設定されていません");
            return;
        }

        // 設定を適用
        audioSettings = audioController.Settings;

        // スライダーの初期値を設定
        UpdateSlidersFromSettings();

        // スライダーの値が変更された時のイベントリスナーを設定
        SetupSliderEvents();

        // 各種音源管理クラスのインスタンスを取得
        Invoke("GetSoundManagers", 1.5f);

        Debug.Log("SliderVolume が初期化されました");
    }

    /// <summary>
    /// 各種音源管理クラスのインスタンスを取得
    /// </summary>
    private void GetSoundManagers()
    {
        // SE_Pickerのインスタンスを取得
        sePicker = SE_Picker.Instance;
        if (sePicker == null)
        {
            Debug.LogWarning("SE_Picker.Instance is null. SEの音量設定が反映されない可能性があります。");
        }
        else
        {
            // SE_PickerにもSE音量を設定
            sePicker.SetMasterSEVolume(audioSettings.seVolume);
        }

        // シーン内の全てのSe_3D_Pickerコンポーネントを検索（新しい推奨APIを使用）
        Se_3D_Picker[] pickers = Object.FindObjectsByType<Se_3D_Picker>(FindObjectsSortMode.None);
        se3DPickers.AddRange(pickers);

        // 全てのSe_3D_Pickerに音量を設定
        foreach (var picker in se3DPickers)
        {
            if (picker != null)
            {
                picker.SetMasterSEVolume(audioSettings.seVolume);
            }
        }

        // CRIWARE_conductorのインスタンスを取得
        criwareConductor = CRIWARE_conductor.Instance;
        if (criwareConductor == null)
        {
            Debug.LogWarning("CRIWARE_conductor.Instance is null. BGMの音量設定が反映されない可能性があります。");
        }
        else
        {
            // CRIWARE_conductorにBGM音量を設定
            criwareConductor.SetMasterBGMVolume(audioSettings.bgmVolume);
        }
    }

    /// <summary>
    /// 各スライダーのイベントを設定
    /// </summary>
    private void SetupSliderEvents()
    {
        // BGM音量スライダーのイベント設定
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        // SE音量スライダーのイベント設定
        if (seSlider != null)
        {
            seSlider.onValueChanged.RemoveAllListeners();
            seSlider.onValueChanged.AddListener(OnSEVolumeChanged);
        }

        // マイク入力スライダーのイベント設定（オプション）
        if (micSlider != null)
        {
            micSlider.onValueChanged.RemoveAllListeners();
            micSlider.onValueChanged.AddListener(OnMicInputChanged);
        }
    }

    /// <summary>
    /// 値を指定した刻み値に丸める
    /// </summary>
    private float StepValue(float value, float stepSize)
    {
        if (!useSteppedValues || stepSize <= 0) return value;
        return Mathf.Round(value / stepSize) * stepSize;
    }

    /// <summary>
    /// BGM音量スライダーの値が変更された時に呼ばれるメソッド
    /// </summary>
    /// <param name="value">新しい音量 (0.0 - 1.0)</param>
    public void OnBGMVolumeChanged(float value)
    {
        // 値を刻み値に丸める
        float steppedValue = StepValue(value, volumeStepSize);

        // 値が変わった場合のみUIを更新（無限ループ防止）
        if (bgmSlider.value != steppedValue)
        {
            bgmSlider.SetValueWithoutNotify(steppedValue);
        }

        // 音量を設定
        audioSettings.bgmVolume = steppedValue;

        // BGMの音量を適用
        ApplyBGMVolume(steppedValue);

        // AudioSettingsControllerがアタッチされている場合はそちらを使用
        if (audioController != null)
        {
            audioController.SetBGMVolume(steppedValue);
        }
        else
        {
            // AudioSettingsControllerがない場合は直接保存
            SaveSettings();
        }

    
    }

    /// <summary>
    /// SE音量スライダーの値が変更された時に呼ばれるメソッド
    /// </summary>
    /// <param name="value">新しい音量 (0.0 - 1.0)</param>
    public void OnSEVolumeChanged(float value)
    {
        // 値を刻み値に丸める
        float steppedValue = StepValue(value, volumeStepSize);

        // 値が変わった場合のみUIを更新（無限ループ防止）
        if (seSlider.value != steppedValue)
        {
            seSlider.SetValueWithoutNotify(steppedValue);
        }

        // 音量を設定
        audioSettings.seVolume = steppedValue;

        // SEの音量を適用
        ApplySEVolume(steppedValue);

        // AudioSettingsControllerがアタッチされている場合はそちらを使用
        if (audioController != null)
        {
            audioController.SetSEVolume(steppedValue);
        }
        else
        {
            // AudioSettingsControllerがない場合は直接保存
            SaveSettings();
        }

    
    }

    /// <summary>
    /// マイク入力の値が変更された時に呼ばれるメソッド
    /// </summary>
    public void OnMicInputChanged(float value)
    {
        // マイク入力は0か1なので刻み値は適用しない
        int binaryValue = Mathf.RoundToInt(value);
        bool isEnabled = binaryValue == 1;

        // AudioSettingsControllerがアタッチされている場合はそちらを使用
        if (audioController != null)
        {
            audioController.SetMicInputState(binaryValue);
        }
        else
        {
            // AudioSettingsControllerがない場合は直接設定
            audioSettings.micInputEnabled = isEnabled;
            SaveSettings();
        }
    }

    /// <summary>
    /// BGMの音量を適用するメソッド
    /// </summary>
    /// <param name="volume">設定する音量 (0.0 - 1.0)</param>
    private void ApplyBGMVolume(float volume)
    {
        try
        {
            // CRIWAREのBGMカテゴリーに音量を設定
            CriWare.CriAtom.SetCategoryVolume("BGM", volume);

            // CRIWARE_conductorが存在する場合はそちらにも音量を設定
            if (criwareConductor != null)
            {
                criwareConductor.SetMasterBGMVolume(volume);
            }
            else
            {
                // インスタンスが見つからない場合、再取得を試みる
                criwareConductor = CRIWARE_conductor.Instance;
                if (criwareConductor != null)
                {
                    criwareConductor.SetMasterBGMVolume(volume);
                }
            }

          
        }
        catch (System.Exception e)
        {
            Debug.LogError("BGM音量の設定中にエラーが発生しました: " + e.Message);
        }
    }

    /// <summary>
    /// SEの音量を適用するメソッド
    /// SE_PickerとSe_3D_Pickerのすべての効果音にこの音量が乗算される
    /// </summary>
    /// <param name="volume">設定する音量 (0.0 - 1.0)</param>
    private void ApplySEVolume(float volume)
    {
        try
        {
            // CRIWAREのSEカテゴリーに音量を設定
            CriWare.CriAtom.SetCategoryVolume("SE", volume);

            // SE_Pickerが存在する場合はそちらにも音量を設定
            if (sePicker != null)
            {
                sePicker.SetMasterSEVolume(volume);
            }
            else
            {
                // インスタンスが見つからない場合、再取得を試みる
                sePicker = SE_Picker.Instance;
                if (sePicker != null)
                {
                    sePicker.SetMasterSEVolume(volume);
                }
            }

            // Se_3D_Picker全てに音量を設定
            foreach (var picker in se3DPickers)
            {
                if (picker != null)
                {
                    picker.SetMasterSEVolume(volume);
                }
            }

            // シーン切り替え後など、新しく追加されたSe_3D_Pickerを探す
            UpdateSe3DPickersList();

         
        }
        catch (System.Exception e)
        {
            Debug.LogError("SE音量の設定中にエラーが発生しました: " + e.Message);
        }
    }

    /// <summary>
    /// シーン内のSe_3D_Pickerリストを更新する
    /// </summary>
    private void UpdateSe3DPickersList()
    {
        // シーン内の全てのSe_3D_Pickerコンポーネントを検索（新しい推奨APIを使用）
        Se_3D_Picker[] pickers = Object.FindObjectsByType<Se_3D_Picker>(FindObjectsSortMode.None);

        // リストをクリアして新しく検出したものを追加
        se3DPickers.Clear();
        se3DPickers.AddRange(pickers);
    }

    /// <summary>
    /// 音声設定を読み込むメソッド
    /// </summary>
    public void LoadSettings()
    {
        // AudioSettingsControllerがアタッチされている場合はそちらから設定を取得
        if (audioController != null && audioController.Settings != null)
        {
            audioSettings = audioController.Settings;
        }
        else
        {
            // AudioSettingsControllerがない場合は直接マネージャーから読み込む
            audioSettings = AudioSettingsManager.LoadSettings();
        }

        // スライダーの値を更新
        UpdateSlidersFromSettings();

        // 音量設定を適用
        ApplyBGMVolume(audioSettings.bgmVolume);
        ApplySEVolume(audioSettings.seVolume);

        Debug.Log($"音声設定を読み込みました: BGM={audioSettings.bgmVolume:F1}, SE={audioSettings.seVolume:F1}, MicInput={audioSettings.micInputEnabled}");
    }

    /// <summary>
    /// スライダーの値を設定から更新
    /// </summary>
    private void UpdateSlidersFromSettings()
    {
        // スライダーが既に設定されている場合は、値を反映
        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(audioSettings.bgmVolume);
        }

        if (seSlider != null)
        {
            seSlider.SetValueWithoutNotify(audioSettings.seVolume);
        }

        if (micSlider != null)
        {
            micSlider.SetValueWithoutNotify(audioSettings.micInputEnabled ? 1 : 0);
        }
    }

    /// <summary>
    /// 音声設定を保存するメソッド
    /// </summary>
    public void SaveSettings()
    {
        // AudioSettingsManagerに設定を保存
        bool success = AudioSettingsManager.SaveSettings(audioSettings);

        if (success)
        {
            Debug.Log($"音声設定を保存しました: BGM={audioSettings.bgmVolume:F1}, SE={audioSettings.seVolume:F1}, MicInput={audioSettings.micInputEnabled}");
        }
        else
        {
            Debug.LogWarning("音声設定の保存に失敗しました");
        }
    }

    /// <summary>
    /// 音声設定をデフォルト値にリセットするメソッド
    /// </summary>
    public void ResetToDefault()
    {
        // AudioSettingsControllerがアタッチされている場合はそちらを使用
        if (audioController != null)
        {
            audioController.ResetToDefault();
            audioSettings = audioController.Settings;
        }
        else
        {
            // AudioSettingsControllerがない場合は直接リセット
            audioSettings = AudioSettingsManager.ResetToDefault();
        }

        // スライダー値を更新
        UpdateSlidersFromSettings();

        // 音量設定を適用
        ApplyBGMVolume(audioSettings.bgmVolume);
        ApplySEVolume(audioSettings.seVolume);

        Debug.Log("音声設定をデフォルトにリセットしました");
    }

    /// <summary>
    /// 音量設定のテスト再生を行うメソッド（オプション）
    /// </summary>
    public void PlayTestSE()
    {
        if (sePicker != null)
        {
            // ボリュームテスト用に足音SEを再生する例
            sePicker.PlayFootStep(1.0f); // ここでは1.0fを指定し、マスター音量でスケールされる
        }
        else
        {
            Debug.LogWarning("SE_Pickerがnullのため、テストSEを再生できません。");
        }
    }

    /// <summary>
    /// AudioSettingsControllerから呼び出されるメソッド
    /// 設定が変更された場合にスライダー値を更新
    /// </summary>
    public void UpdateAllSliders(AudioSettingsData settings)
    {
        if (settings == null) return;

        audioSettings = settings;
        UpdateSlidersFromSettings();
    }

    /// <summary>
    /// マイクスライダーの値を更新
    /// </summary>
    public void UpdateMicSlider(bool isEnabled)
    {
        if (micSlider != null)
        {
            micSlider.SetValueWithoutNotify(isEnabled ? 1 : 0);
        }
    }
}