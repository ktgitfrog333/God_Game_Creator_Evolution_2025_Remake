using System.Collections;
using System.Collections.Generic;
using CriWare;
using UnityEngine;

public class CRIWARE_AisacChange : MonoBehaviour
{
    public static CRIWARE_AisacChange Instance { get; private set; } // シングルトンを設定

    private CriAtomSource currentSource;
    private CriAtomExPlayer atomExPlayer;
    private bool ChangeLock = false;

    // AISACコントロール名（互換性のため残す）
    public string AisacControl_00 = "Aisac_0";
    public string AisacControl_01 = "Aisac_1";

    [HideInInspector] public bool OnNoBgm = false; // BGM変更をロックするかどうか

    // フェード時間の設定
    public float transitionDuration = 3.0f;
    public float transitionDuration2 = 0.05f;

    // AISACコントロールの現在値を追跡するための変数（互換性のため残す）
    private float currentAisacValue_00 = 0.9f;
    private float currentAisacValue_01 = 0.0f;

    // 音量管理
    private float targetVolume = 1.0f; // 目標音量
    private float baseVolume = 1.0f; // 基本音量（外部から設定される）

    public bool isAutoStart = false;
    private Dictionary<string, Coroutine> currentCoroutines = new Dictionary<string, Coroutine>();
    private Coroutine volumeFadeCoroutine = null; // 音量フェード用コルーチン

    // インスペクターから設定するBPMと開始拍
    public float bpm = 120.0f;
    public int startBeat = 0; // 初期値はゼロ拍目から

    /// <summary>最初に呼ばれる再生処理が完了しているか</summary>
    private bool _isCompletedPlayStart;
    /// <summary>最初に呼ばれる再生処理が完了しているか</summary>
    public bool IsCompletedPlayStart => _isCompletedPlayStart;

    void Start()
    {
        // シングルトンの処理
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AutoStart()
    {
        if (isAutoStart)
        {
            float startTimeInSeconds = BeatToSeconds(startBeat, bpm);
            atomExPlayer.SetStartTime((long)(startTimeInSeconds * 1000));
            BGM0();
            currentSource.Play();
        }

        if (OnNoBgm)
        {
            FadeVolume(0.0f, transitionDuration2);
        }
    }

    public void SetSource(CriAtomSource atomSource)
    {
        currentSource = atomSource;
        atomExPlayer = currentSource.player;

        // 現在のソースの音量を基本音量として保存
        if (currentSource != null)
        {
            baseVolume = currentSource.volume;
        }
    }

    public void PlayStart()
    {
        float startTimeInSeconds = BeatToSeconds(startBeat, bpm);
        atomExPlayer.SetStartTime((long)(startTimeInSeconds * 1000));
        currentSource.Play();
        Debug.Log("再生開始");
        _isCompletedPlayStart = true;
    }

    private void OnDisable()
    {
        // シーン遷移時に音声を停止
        if (currentSource != null)
        {
            currentSource.Stop();
        }

        // コルーチンを停止
        if (volumeFadeCoroutine != null)
        {
            StopCoroutine(volumeFadeCoroutine);
            volumeFadeCoroutine = null;
        }
    }

    public void PauseOn()
    {
        if (currentSource != null)
        {
            currentSource.Pause(true);
        }
    }

    public void PauseOff()
    {
        if (currentSource != null)
        {
            currentSource.Pause(false);
        }
    }

    public void ApplyAisac(CriAtomSource source)
    {
        currentSource = source;

        if (currentSource == null) return;

        // 新しいソースに切り替わった時は基本音量を更新
        baseVolume = currentSource.volume;

        // 現在の目標音量を適用
        currentSource.volume = baseVolume * targetVolume;
    }

    /// <summary>
    /// 音量をフェードさせる
    /// </summary>
    /// <param name="volume">目標音量（0.0 ~ 1.0）</param>
    /// <param name="duration">フェード時間（秒）</param>
    public void FadeVolume(float volume, float duration)
    {
        if (currentSource == null) return;

        // 既存のフェードコルーチンがあれば停止
        if (volumeFadeCoroutine != null)
        {
            StopCoroutine(volumeFadeCoroutine);
        }

        volumeFadeCoroutine = StartCoroutine(FadeVolumeCoroutine(volume, duration));
    }

    private IEnumerator FadeVolumeCoroutine(float targetVol, float duration)
    {
        float startVolume = targetVolume;
        float startTime = Time.time;

        Debug.Log($"音量フェード開始: {startVolume} → {targetVol} ({duration}秒)");

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            targetVolume = Mathf.Lerp(startVolume, targetVol, t);
            currentSource.volume = baseVolume * targetVolume;
            yield return null;
        }

        targetVolume = targetVol;
        currentSource.volume = baseVolume * targetVolume;

        Debug.Log($"音量フェード完了: volume = {currentSource.volume}");

        volumeFadeCoroutine = null;
    }

    /// <summary>
    /// 外部から基本音量を設定する（CRIWARE_conductorから呼ばれる）
    /// </summary>
    public void SetBaseVolume(float volume)
    {
        baseVolume = Mathf.Clamp01(volume);
        if (currentSource != null)
        {
            currentSource.volume = baseVolume * targetVolume;
        }
    }

    //ここから先はBGM切り替えメソッド
    public void BGM0() // 通常再生
    {
        if (ChangeLock || OnNoBgm) return;
        FadeVolume(1.0f, transitionDuration * 2);
        ChangeAisacControlIfExists(AisacControl_00, 0.9f, transitionDuration * 2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration * 2);
    }

    public void BGM0Now() // 瞬時に通常再生
    {
        if (ChangeLock || OnNoBgm) return;
        FadeVolume(1.0f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_00, 0.9f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, 0.1f);
    }

    public void BGM1()
    {
        if (ChangeLock || OnNoBgm) return;
        FadeVolume(1.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_01, 0.9f, transitionDuration);
    }

    public void BGM1Now()
    {
        if (ChangeLock || OnNoBgm) return;
        FadeVolume(1.0f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_01, 0.9f, 0.1f);
    }

    public void BGM2()
    {
        if (ChangeLock || OnNoBgm) return;
        FadeVolume(1.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration2);
    }

    public void BGM3()
    {
        if (ChangeLock || OnNoBgm) return;
        FadeVolume(1.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration);
    }

    public void BGM4() // フェードアウト（無音化）
    {
        if (OnNoBgm) return;
        ChangeLock = true;
        FadeVolume(0.0f, transitionDuration2);
        Debug.Log("BGM4: フェードアウト開始");
    }

    public void BGM5() // フェードイン
    {
        if (OnNoBgm) return;
        ChangeLock = false;
        FadeVolume(1.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_00, 0.9f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration2);
    }

    public void BGM6() // フェードイン（速め）
    {
        if (OnNoBgm) return;
        ChangeLock = false;
        FadeVolume(1.0f, transitionDuration / 2);
        ChangeAisacControlIfExists(AisacControl_00, 0.9f, transitionDuration / 2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration / 2);
    }

    public void BGM7() // フェードアウト
    {
        if (OnNoBgm) return;
        ChangeLock = false;
        FadeVolume(0.0f, transitionDuration / 3);
    }

    public void BGM8() // フェードアウト（長め）
    {
        if (OnNoBgm) return;
        ChangeLock = true;
        FadeVolume(0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_01, 0.9f, transitionDuration);
    }

    // AISAC関連のメソッド（互換性のため残す）
    private bool HasAisacControl(string aisacName)
    {
        if (string.IsNullOrEmpty(aisacName)) return false;

        try
        {
            atomExPlayer.SetAisacControl(aisacName, 0.0f);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    private void ChangeAisacControlIfExists(string aisacName, float targetValue, float Setduration)
    {
        if (string.IsNullOrEmpty(aisacName)) return;

        if (HasAisacControl(aisacName))
        {
            ChangeAisacControl(aisacName, targetValue, Setduration);
        }
    }

    private void ChangeAisacControl(string aisacName, float targetValue, float Setduration)
    {
        if (currentCoroutines.TryGetValue(aisacName, out Coroutine runningCoroutine))
        {
            StopCoroutine(runningCoroutine);
            currentCoroutines.Remove(aisacName);
        }

        Coroutine newCoroutine = StartCoroutine(ChangeAisacControlValue(aisacName, targetValue, Setduration));
        currentCoroutines[aisacName] = newCoroutine;
    }

    IEnumerator ChangeAisacControlValue(string aisacName, float targetValue, float Setduration)
    {
        float startTime = Time.time;
        float startValue = GetCurrentAisacValue(aisacName);

        while (Time.time - startTime < Setduration)
        {
            float t = (Time.time - startTime) / Setduration;
            float newValue = Mathf.Lerp(startValue, targetValue, t);
            currentSource.SetAisacControl(aisacName, newValue);
            SetCurrentAisacValue(aisacName, newValue);
            yield return null;
        }
        currentSource.SetAisacControl(aisacName, targetValue);
        SetCurrentAisacValue(aisacName, targetValue);

        if (currentCoroutines.ContainsKey(aisacName))
        {
            currentCoroutines.Remove(aisacName);
        }
    }

    private float GetCurrentAisacValue(string aisacName)
    {
        if (aisacName == AisacControl_00) return currentAisacValue_00;
        if (aisacName == AisacControl_01) return currentAisacValue_01;
        return 0.0f;
    }

    private void SetCurrentAisacValue(string aisacName, float newValue)
    {
        if (aisacName == AisacControl_00) currentAisacValue_00 = newValue;
        else if (aisacName == AisacControl_01) currentAisacValue_01 = newValue;
    }

    private float BeatToSeconds(int beat, float bpm)
    {
        return (beat * 60.0f) / bpm;
    }

    public void SetTransitionDuration(float duration)
    {
        transitionDuration = duration;
    }
}