using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マイク音量ベースの入力判定を管理するクラス
/// </summary>
public class MissileMicInputManager
{
    private MissileDirectAnimManagerB parent;
    private float oneBeat;

    // チェックポイント設定
    private int checkpointCount;
    private float thresholdA;
    private float thresholdB;
    private float thresholdC;

    // スコア管理
    private int score = 100;
    private bool isMeasuring = false;
    private float measureStartTime = 0f;
    private float measureDuration = 0f;
    private int checkpointsFired = 0;
    private bool isFinished = false;

    // 長押し状態
    private bool longPressStarted = false;

    public MissileMicInputManager(MissileDirectAnimManagerB parent, float oneBeat,
        int checkpointCount, float thresholdA, float thresholdB, float thresholdC)
    {
        this.parent = parent;
        this.oneBeat = oneBeat;
        this.checkpointCount = checkpointCount;
        this.thresholdA = thresholdA;
        this.thresholdB = thresholdB;
        this.thresholdC = thresholdC;
    }

    public void Reset()
    {
        score = 100;
        isMeasuring = false;
        measureStartTime = 0f;
        checkpointsFired = 0;
        isFinished = false;
        longPressStarted = false;
    }

    public bool IsLongPressStarted() => longPressStarted;
    public bool IsFinished() => isFinished;
    public int GetCurrentScore() => score;

    /// <summary>
    /// oneBeatを更新（インスタンスを再生成せずに更新）
    /// </summary>
    public void UpdateOneBeat(float oneBeat)
    {
        this.oneBeat = oneBeat;
    }

    public void StartMeasure(float elapsedTime, float duration)
    {
        longPressStarted = true;
        isMeasuring = true;
        measureStartTime = elapsedTime;
        measureDuration = duration;
        score = 100;
        checkpointsFired = 0;
        isFinished = false;
        Debug.Log($"[MicInput] StartMeasure: elapsedTime={elapsedTime:F3}, oneBeat={oneBeat:F3}, measureDuration={measureDuration:F3}");
    }
    public void UpdateMeasure(float elapsedTime)
    {
        if (!isMeasuring || isFinished) return;

        float measureElapsed = elapsedTime - measureStartTime;
        float measureProgress = measureElapsed / measureDuration;

    //    Debug.Log($"[MicInput] UpdateMeasure: elapsedTime={elapsedTime:F3}, measureElapsed={measureElapsed:F3}, measureProgress={measureProgress:F3}, checkpointsFired={checkpointsFired}/{checkpointCount}");

        int expectedCheckpoints = Mathf.FloorToInt(measureProgress * checkpointCount);
        expectedCheckpoints = Mathf.Clamp(expectedCheckpoints, 0, checkpointCount);

        while (checkpointsFired < expectedCheckpoints)
        {
            FireCheckpoint();
            checkpointsFired++;
        }

        if (measureProgress >= 1.0f)
        {
            Debug.Log($"[MicInput] 計測終了条件に到達: measureProgress={measureProgress:F3}");
            FinishMeasure();
        }
    }

    private void FireCheckpoint()
    {
        float currentVolume = GetCurrentVolume();
        int pointsBefore = score;

        if (currentVolume >= thresholdC)
        {
            score -= 8;
            Debug.Log($"[MicInput] FireCheckpoint[{checkpointsFired + 1}/{checkpointCount}]: C以上 volume={currentVolume:F3} thresholdC={thresholdC:F3} → -8点 ({pointsBefore}→{score})");
        }
        else if (currentVolume >= thresholdB)
        {
            score -= 5;
            Debug.Log($"[MicInput] FireCheckpoint[{checkpointsFired + 1}/{checkpointCount}]: B以上 volume={currentVolume:F3} thresholdB={thresholdB:F3} → -5点 ({pointsBefore}→{score})");
        }
        else if (currentVolume >= thresholdA)
        {
            score -= 3;
            Debug.Log($"[MicInput] FireCheckpoint[{checkpointsFired + 1}/{checkpointCount}]: A以上 volume={currentVolume:F3} thresholdA={thresholdA:F3} → -3点 ({pointsBefore}→{score})");
        }
        else
        {
            Debug.Log($"[MicInput] FireCheckpoint[{checkpointsFired + 1}/{checkpointCount}]: 閾値未満 volume={currentVolume:F3} thresholdA={thresholdA:F3} → 0点 (残り{score}点)");
        }
    }

    /// <summary>
    /// 現在の音量を取得（Qキーデバッグ対応）
    /// </summary>
    private float GetCurrentVolume()
    {
        // QキーでC以上相当の音量を強制
        if (Input.GetKey(KeyCode.Q))
        {
            Debug.Log("Qキーデバッグ: C以上相当として処理");
            return thresholdC;
        }

        if (MicInput_Criware.Instance != null)
        {
            return MicInput_Criware.Instance.GetAveragedVolume();
        }

        Debug.LogWarning("MicInput_Criware.Instanceがnullです");
        return 0f;
    }

    /// <summary>
    /// 計測終了時の成否判定
    /// </summary>

    private void FinishMeasure()
    {
        isFinished = true;
        isMeasuring = false;

        Debug.Log($"[MicInput] FinishMeasure: 最終スコア={score}, 成功={score <= 0}");

        if (score <= 0)
        {
            Debug.Log("[MicInput] → TriggerSuccessEvent呼び出し");
            parent.TriggerSuccessEvent();
        }
        else
        {
            Debug.Log($"[MicInput] → TriggerFailEvent呼び出し (残り{score}点)");
            parent.TriggerFailEvent();
        }
    }
}