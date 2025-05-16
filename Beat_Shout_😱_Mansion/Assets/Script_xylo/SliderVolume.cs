using CriWare;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BGMとSEの音量をスライダーで調整し、値を保存するクラス
/// CRIWAREのBGMカテゴリーとSE_Picker、Se_3D_Picker、CRIWARE_conductorクラスに音量を反映させる
/// </summary>
public class SliderVolume : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider; // BGM音量用スライダー
    [SerializeField] private Slider seSlider;  // SE音量用スライダー

    // BGMとSEの音量を保持するデータ
    private float bgmVolume = 1.0f;
    private float seVolume = 1.0f;

    // PlayerPrefsキー
    private const string Key_BGMVolume = "Key_BGMVolume";
    private const string Key_SEVolume = "Key_SEVolume";

    // SE_Pickerへの参照（シングルトンパターンを使用）
    private SE_Picker sePicker;

    // 全てのSe_3D_Pickerコンポーネントを格納するリスト
    private List<Se_3D_Picker> se3DPickers = new List<Se_3D_Picker>();

    // CRIWARE_conductorへの参照（シングルトンパターンを使用）
    private CRIWARE_conductor criwareConductor;

    private void Start()
    {
        // 保存されている音量設定を読み込む
        LoadBGMVolume();
        LoadSEVolume();

        // スライダーの初期値を設定
        bgmSlider.value = bgmVolume;
        seSlider.value = seVolume;

        // 音量設定を適用
        ApplyBGMVolume(bgmVolume);
        ApplySEVolume(seVolume);

        // スライダーの値が変更された時のイベントリスナーを設定
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        seSlider.onValueChanged.AddListener(OnSEVolumeChanged);

        // 各種音源管理クラスのインスタンスを取得（Delayが必要かもしれないので遅延実行）
        Invoke("GetSoundManagers", 1.5f);
    }

    // 各種音源管理クラスのインスタンスを取得
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
            sePicker.SetMasterSEVolume(seVolume);
        }

        // シーン内の全てのSe_3D_Pickerコンポーネントを検索（新しい推奨APIを使用）
        Se_3D_Picker[] pickers = Object.FindObjectsByType<Se_3D_Picker>(FindObjectsSortMode.None);
        se3DPickers.AddRange(pickers);

        // 全てのSe_3D_Pickerに音量を設定
        foreach (var picker in se3DPickers)
        {
            if (picker != null)
            {
                picker.SetMasterSEVolume(seVolume);
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
            criwareConductor.SetMasterBGMVolume(bgmVolume);
        }
    }

    /// <summary>
    /// BGM音量スライダーの値が変更された時に呼ばれるメソッド
    /// </summary>
    /// <param name="volume">新しい音量 (0.0 - 1.0)</param>
    public void OnBGMVolumeChanged(float volume)
    {
        bgmVolume = volume;

        // BGMの音量を適用
        ApplyBGMVolume(volume);

        // 音量を保存
        SaveBGMVolume(volume);
    }

    /// <summary>
    /// SE音量スライダーの値が変更された時に呼ばれるメソッド
    /// </summary>
    /// <param name="volume">新しい音量 (0.0 - 1.0)</param>
    public void OnSEVolumeChanged(float volume)
    {
        seVolume = volume;

        // SEの音量を適用
        ApplySEVolume(volume);

        // 音量を保存
        SaveSEVolume(volume);
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
            CriAtom.SetCategoryVolume("BGM", volume);

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

            // デバッグログ
            Debug.Log($"BGM音量を{volume}に設定しました。");
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
            CriAtom.SetCategoryVolume("SE", volume);

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

            // デバッグログ
            Debug.Log($"SE音量を{volume}に設定しました。");
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
    /// BGMの音量を保存するメソッド
    /// </summary>
    /// <param name="volume">音量 (0.0 - 1.0)</param>
    public void SaveBGMVolume(float volume)
    {
        bgmVolume = volume;
        PlayerPrefs.SetFloat(Key_BGMVolume, bgmVolume);
        PlayerPrefs.Save(); // PlayerPrefsを保存します
    }

    /// <summary>
    /// BGMの音量を読み込むメソッド
    /// </summary>
    /// <returns>BGMの音量 (0.0 - 1.0)</returns>
    public float LoadBGMVolume()
    {
        bgmVolume = PlayerPrefs.GetFloat(Key_BGMVolume, 1.0f); // デフォルト値は1.0f
        return bgmVolume;
    }

    /// <summary>
    /// SEの音量を保存するメソッド
    /// </summary>
    /// <param name="volume">音量 (0.0 - 1.0)</param>
    public void SaveSEVolume(float volume)
    {
        seVolume = volume;
        PlayerPrefs.SetFloat(Key_SEVolume, seVolume);
        PlayerPrefs.Save(); // PlayerPrefsを保存します
    }

    /// <summary>
    /// SEの音量を読み込むメソッド
    /// </summary>
    /// <returns>SEの音量 (0.0 - 1.0)</returns>
    public float LoadSEVolume()
    {
        seVolume = PlayerPrefs.GetFloat(Key_SEVolume, 1.0f); // デフォルト値は1.0f
        return seVolume;
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
}