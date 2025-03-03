using CriWare;
using UnityEngine;

public class Se_3D_Picker : MonoBehaviour
{
    private bool ActiveNow = false;

    // インスペクター上での設定項目
    public CriAtomSource source; // CriAtomSourceコンポーネントへの参照

    private void OnDisable()
    {
        // 非アクティブになる前に必要な処理
        CleanUp();
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

        volume = Mathf.Clamp(volume, 0f, 1f); // 音量を制限

        // CriAtomSourceを使用して音を再生
        source.cueName = SeName;
        source.volume = volume;
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