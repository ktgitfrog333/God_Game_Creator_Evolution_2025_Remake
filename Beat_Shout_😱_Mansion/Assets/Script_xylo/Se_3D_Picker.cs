using CriWare;
using UnityEngine;

public class Se_3D_Picker : MonoBehaviour
{
    private bool ActiveNow = false;
    // インスペクター上での設定項目
    public CriAtomSource source; // CriAtomSourceコンポーネントへの参照

    // マスター音量（SEと同じ値を使用）
    private float masterSEVolume = 1.0f;

    // PlayerPrefsキー（SliderVolumeと共通）
    private const string Key_SEVolume = "Key_SEVolume";

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

    // マスターSE音量を読み込む
    private void LoadMasterSEVolume()
    {
        // PlayerPrefsから保存されているSE音量を読み込む
        masterSEVolume = PlayerPrefs.GetFloat(Key_SEVolume, 1.0f);
    }

    // マスターSE音量を設定するメソッド（SliderVolumeから呼び出される）
    public void SetMasterSEVolume(float volume)
    {
        masterSEVolume = Mathf.Clamp(volume, 0f, 1f);

        // 既に再生中の音の音量も更新
        if (source != null && source.status == CriAtomSource.Status.Playing)
        {
            source.volume = source.volume * masterSEVolume / (masterSEVolume != 0 ? masterSEVolume : 1);
        }
    }

    public void CleanUp()
    {
        // 再生中の場合は停止
        if (ActiveNow && source != null && source.status == CriAtomSource.Status.Playing)
        {
            source.Stop();
        }
        ActiveNow = false;
    }

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

    public void StopSound()
    {
        if (source != null && source.status == CriAtomSource.Status.Playing)
        {
            source.Stop();
        }
    }
}