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
    //縦の遷移は４パターンまで登録可能。必要に応じて追加する
    public string AisacControl_00;
    public string AisacControl_01;
    public string AisacControl_02;
    public string AisacControl_03;

    [HideInInspector] public bool OnNoBgm = false; // BGM変更をロックするかどうか

    // AISACコントロールの変更にかかる時間をインスペクターから設定可能にする
    public float transitionDuration = 3.0f;
    public float transitionDuration2 = 0.05f;

    // AISACコントロールの現在値を追跡するための変数
    private float currentAisacValue_00 = 0.9f; // 初期値
    private float currentAisacValue_01 = 0.0f; // 初期値
    private float currentAisacValue_02 = 0.0f; // 初期値
    private float currentAisacValue_03 = 0.0f; // 初期値
    public bool isAutoStart = false;
    private Dictionary<string, Coroutine> currentCoroutines = new Dictionary<string, Coroutine>();

    // インスペクターから設定するBPMと開始拍
    public float bpm = 120.0f;
    public int startBeat = 0; // 初期値はゼロ拍目から

    void Start()
    {
        // シングルトンの処理
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 二つ目のインスタンスが作成された場合は破棄
        }
    }

    public void AutoStart()
    {
        if (isAutoStart)
        {
            // BGMとSEの音量をDataクラスから取得して設定するメソッドをここに追記
            //    float bgmVolume = 
            //    float seVolume = 

            // 取得した音量をCriAtomのカテゴリに設定
            //   CriAtom.SetCategoryVolume("BGM", bgmVolume);
            //   CriAtom.SetCategoryVolume("SE", seVolume);


            float startTimeInSeconds = BeatToSeconds(startBeat, bpm);
            atomExPlayer.SetStartTime((long)(startTimeInSeconds * 1000)); // ミリ秒単位で設定
            BGM0();
            currentSource.Play();
        }

        if (OnNoBgm)
        {
            ChangeAisacControl(AisacControl_00, 0.0f, transitionDuration2);
            ChangeAisacControl(AisacControl_01, 0.0f, transitionDuration2);
            ChangeAisacControl(AisacControl_02, 0.0f, transitionDuration2);
            ChangeAisacControl(AisacControl_03, 0.0f, transitionDuration2);
        }
    }

    public void SetSource(CriAtomSource atomSource)
    {
        currentSource = atomSource;
        atomExPlayer = currentSource.player; // CriAtomExPlayer を取得
    }

    public void PlayStart()
    {
        float startTimeInSeconds = BeatToSeconds(startBeat, bpm);
        atomExPlayer.SetStartTime((long)(startTimeInSeconds * 1000)); // ミリ秒単位で設定
        currentSource.Play();
        Debug.Log("再生開始");
    }

    private void OnDisable()
    {
        // シーン遷移時に音声を停止
        if (currentSource != null)
        {
            currentSource.Stop();
        }
    }

    public void PauseOn()
    //一時停止中はビート同期を停止させる。BGMではなく単発SEという形で音楽を再生する
    //CRIWARE側の設定でカテゴリをBGMとしておけば、音量調整はBGMのものが適用される
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

        ChangeAisacControl(AisacControl_00, currentAisacValue_00, transitionDuration * 2);
        ChangeAisacControl(AisacControl_01, currentAisacValue_01, transitionDuration * 2);
        ChangeAisacControl(AisacControl_02, currentAisacValue_02, transitionDuration * 2);
        ChangeAisacControl(AisacControl_03, currentAisacValue_03, transitionDuration * 2);
    }

    //ここから先はAisacの変更と変更秒数に関するメソッド。必要に応じて作成する
    public void BGM0()　//ゆっくりBGM_Aへ
    {
        if (ChangeLock || OnNoBgm) return;
        ChangeAisacControlIfExists(AisacControl_00, 0.9f, transitionDuration * 2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration * 2);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, transitionDuration * 2);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, transitionDuration * 2);
    }

    public void BGM0Now()　//瞬時にBGM_Aへ
    {
        if (ChangeLock || OnNoBgm) return;
        ChangeAisacControlIfExists(AisacControl_00, 0.9f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, 0.1f);
    }

    public void BGM1()
    {
        if (ChangeLock || OnNoBgm) return;
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_01, 0.9f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, transitionDuration);
    }

    public void BGM1Now()
    {
        if (ChangeLock || OnNoBgm) return;
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_01, 0.9f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, 0.1f);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, 0.1f);
    }

    public void BGM2()
    {
        if (ChangeLock || OnNoBgm) return;
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_02, 0.9f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, transitionDuration2);
    }

    public void BGM3()
    {
        if (ChangeLock || OnNoBgm) return;
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_03, 0.9f, transitionDuration);
    }

    public void BGM4()
    {
        if (OnNoBgm) return;
        ChangeLock = true; // BGM変更をロック
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, transitionDuration2);
    }

    public void BGM5()
    {
        if (OnNoBgm) return;
        ChangeLock = false; // BGM変更ロックを解除
        ChangeAisacControlIfExists(AisacControl_00, 0.9f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, transitionDuration2);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, transitionDuration2);
    }

    public void BGM6()
    {
        if (OnNoBgm) return;
        ChangeLock = false; // BGM変更ロックを解除
        ChangeAisacControlIfExists(AisacControl_00, 0.9f, transitionDuration / 2);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration / 2);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, transitionDuration / 2);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, transitionDuration / 2);
    }

    public void BGM7() // 1秒かけてフェードアウト
    {
        if (OnNoBgm) return;
        ChangeLock = false; // BGM変更ロックを解除
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration / 3);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration / 3);
        ChangeAisacControlIfExists(AisacControl_02, 0.0f, transitionDuration / 3);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, transitionDuration / 3);
    }

    public void BGM8() // 1秒かけてフェードアウト
    {
        if (OnNoBgm) return;
        ChangeLock = true; // BGM変更ロック
        ChangeAisacControlIfExists(AisacControl_00, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_01, 0.0f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_02, 0.9f, transitionDuration);
        ChangeAisacControlIfExists(AisacControl_03, 0.0f, transitionDuration);
    }

    private bool HasAisacControl(string aisacName)
    {
        try
        {
            // 一時的にAISACコントロールを設定しようとする
            atomExPlayer.SetAisacControl(aisacName, 0.0f);
            return true;
        }
        catch (System.Exception)
        {
            // 失敗した場合はAISACコントロールが存在しないと判断
            return false;
        }
    }

    private void ChangeAisacControlIfExists(string aisacName, float targetValue, float Setduration)
    {
        if (HasAisacControl(aisacName))
        {
            ChangeAisacControl(aisacName, targetValue, Setduration);
        }
        else
        {
            Debug.LogWarning("指定されたAISACが存在しません: " + aisacName);
        }
    }

    private void ChangeAisacControl(string aisacName, float targetValue, float Setduration)
    {
        // 既に実行中のコルーチンがあれば停止
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

        // コルーチンの実行が完了したら辞書から削除
        if (currentCoroutines.ContainsKey(aisacName))
        {
            currentCoroutines.Remove(aisacName);
        }
    }

    private float GetCurrentAisacValue(string aisacName)
    {
        switch (aisacName)
        {
            case "AisacControl_00": return currentAisacValue_00;
            case "AisacControl_01": return currentAisacValue_01;
            case "AisacControl_02": return currentAisacValue_02;
            case "AisacControl_03": return currentAisacValue_03;
            default: return 0.0f; // 不明なAISAC名の場合は0.0fを返す
        }
    }

    private void SetCurrentAisacValue(string aisacName, float newValue)
    {
        switch (aisacName)
        {
            case "AisacControl_00": currentAisacValue_00 = newValue; break;
            case "AisacControl_01": currentAisacValue_01 = newValue; break;
            case "AisacControl_02": currentAisacValue_02 = newValue; break;
            case "AisacControl_03": currentAisacValue_03 = newValue; break;
        }
    }

    // 拍数から秒数への変換メソッド
    private float BeatToSeconds(int beat, float bpm)
    {
        return (beat * 60.0f) / bpm;
    }

    // transitionDurationのセッター
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = duration;
    }
}
