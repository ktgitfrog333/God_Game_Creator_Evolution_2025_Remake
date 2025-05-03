using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 入力検出と処理を管理するクラス
/// </summary>
public class MissileInputManager
{
    private MissileDirectAnimManagerB parent;
    private float clickGracePeriod;
    private float oneBeat;
    private MissileNoteType currentNoteType;

    // クリック検出用変数
    private bool clickable = false;
    //  private bool requireLongPress = false;
    private int requiredBeats = 1;        // 長押しの場合の必要拍数
    private bool waitingForRelease = false;
    private bool clickPressed = false;
    private float clickTimer = 0f;
    private float clickTargetTime = 0f;
    private float releaseTargetTime = 0f;

    private float absoluteReleaseTargetTime;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MissileInputManager(MissileDirectAnimManagerB parent, float clickGracePeriod, float oneBeat)
    {
        this.parent = parent;
        this.clickGracePeriod = clickGracePeriod;
        this.oneBeat = oneBeat;
        this.currentNoteType = parent.GetNoteType();
    }

    /// <summary>
    /// テンポ情報を更新
    /// </summary>
    public void UpdateTempoInfo(float oneBeat, MissileNoteType noteType)
    {
        this.oneBeat = oneBeat;
        this.currentNoteType = noteType;
    }

    /// <summary>
    /// 入力状態をリセット
    /// </summary>
    public void Reset()
    {
        clickable = false;
        waitingForRelease = false;
        clickPressed = false;
        clickTimer = 0f;
        //   requireLongPress = false;
        requiredBeats = 1;
    }

    /// <summary>
    /// 長押しが開始されているかを確認
    /// </summary>
    public bool IsLongPressStarted()
    {
        return waitingForRelease && clickPressed;
    }


    /// <summary>
    /// 長押しリリース処理（外部から呼び出し可能）
    /// </summary>
    /// 

    public void HandleLongPressRelease(float elapsedTime)
    {
        if (!waitingForRelease || !clickPressed) return;

        // リリースタイミングを計算
        absoluteReleaseTargetTime = CalculateReleaseTargetTime(currentNoteType);
        float releaseGracePeriod = clickGracePeriod;
        float releaseTimingDifference = elapsedTime - absoluteReleaseTargetTime;
        bool inReleaseWindow = Mathf.Abs(releaseTimingDifference) <= releaseGracePeriod;

        if (inReleaseWindow)
        {
            // 成功！
            parent.TriggerSuccessEvent();
        }
        else
        {
            // 失敗 - 離すタイミング外
            parent.TriggerFailEvent();
        }

        // クリック判定を無効化
        clickable = false;
        waitingForRelease = false;
        clickPressed = false;
    }

    /// <summary>
    /// クリック可能状態を設定
    /// </summary>
    public void SetClickable(bool clickable)
    {
        this.clickable = clickable;

        if (clickable)
        {
            // ノーツタイプに基づいて長押し情報を設定
            switch (currentNoteType)
            {
                case MissileNoteType.Short:
                    //  requireLongPress = false;
                    requiredBeats = 0;
                    clickTargetTime = oneBeat * 4; // 4拍目でクリック
                    break;

                case MissileNoteType.Long1Beat:
                    //   requireLongPress = true;
                    requiredBeats = 1;
                    clickTargetTime = oneBeat * 4;    // 4拍目で押す
                    releaseTargetTime = oneBeat * 5;  // 5拍目で離す
                    break;

                case MissileNoteType.Long2Beat:
                    //    requireLongPress = true;
                    requiredBeats = 2;
                    clickTargetTime = oneBeat * 4;    // 4拍目で押す
                    releaseTargetTime = oneBeat * 6;  // 6拍目で離す (2拍後)
                    break;

                case MissileNoteType.Long3Beat:
                    //     requireLongPress = true;
                    requiredBeats = 3;
                    clickTargetTime = oneBeat * 4;    // 4拍目で押す
                    releaseTargetTime = oneBeat * 7;  // 7拍目で離す (3拍後)
                    break;
            }
        }
    }

    /// <summary>
    /// 入力処理を実行
    /// </summary>
    public void HandleInput(MissileNoteType noteType, float elapsedTime)
    {
    //    if (!clickable) return;
       // Debug.Log("入力がクリッカブルになって見張っているよ");
        this.currentNoteType = noteType;

        switch (noteType)
        {
            case MissileNoteType.Short:
                HandleShortClickAbsoluteTime(elapsedTime);
                break;

            case MissileNoteType.Long1Beat:
            case MissileNoteType.Long2Beat:
            case MissileNoteType.Long3Beat:
                HandleLongClickAbsoluteTime(elapsedTime, noteType);
                break;
        }
    }

    /// <summary>
    /// ノーツタイプに基づいたリリースターゲット時間を計算
    /// </summary>
    private float CalculateReleaseTargetTime(MissileNoteType noteType)
    {
        // 押すタイミングは常に4拍目
        float absolutePressTargetTime = oneBeat * 4;

        // リリースタイミングはノーツタイプによって異なる
        switch (noteType)
        {
            case MissileNoteType.Long1Beat:
                return oneBeat * 5;  // 1拍後
            case MissileNoteType.Long2Beat:
                return oneBeat * 6;  // 2拍後
            case MissileNoteType.Long3Beat:
                return oneBeat * 7;  // 3拍後
            default:
                return oneBeat * 5;  // デフォルト
        }
    }

    /// <summary>
    /// 絶対時間ベースのShortクリック判定
    /// </summary>
    private void HandleShortClickAbsoluteTime(float elapsedTime)
    {
        // ジャストタイミングの絶対時間（生成から4ビート後）
        float absoluteClickTargetTime = oneBeat * 4;

        // タイミング差と判定
        float timingDifference = elapsedTime - absoluteClickTargetTime;
        bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

        // マウスクリック/タッチ検出
        if (Input.GetMouseButtonDown(0))
        {

            // クリック位置がUI上かチェック
            if (parent.IsPointerOverUI())
            {
                if (inClickWindow)
                {
                    // 成功！
                    parent.TriggerSuccessEvent();
                }
                else
                {
                    // 失敗 - タイミング外
                    parent.TriggerFailEvent();
                }

                // クリック判定を無効化
                clickable = false;
            }

        }
    }

    /// <summary>
    /// 長押し開始メソッド（外部から設定可能）
    /// </summary>
    public void StartLongPress(float startTime)
    {
        waitingForRelease = true;
        clickPressed = true;
        Debug.Log("長押し始めたよ");

        // 必要なビート数に応じてリリースタイミングを設定
        switch (currentNoteType)
        {
            case MissileNoteType.Long1Beat:
                releaseTargetTime = startTime + oneBeat;
                break;
            case MissileNoteType.Long2Beat:
                releaseTargetTime = startTime + (oneBeat * 2);
                break;
            case MissileNoteType.Long3Beat:
                releaseTargetTime = startTime + (oneBeat * 3);
                break;
            default:
                releaseTargetTime = startTime + oneBeat;
                break;
        }

    }

    /// <summary>
    /// 絶対時間ベースのLongクリック判定
    /// </summary>
    private void HandleLongClickAbsoluteTime(float elapsedTime, MissileNoteType noteType)
    {
        // ジャストタイミングの絶対時間（生成から4ビート後に押し、noteTypeに基づいた拍数後に離す）
        float absolutePressTargetTime = oneBeat * 4;
        float absoluteReleaseTargetTime = CalculateReleaseTargetTime(noteType);

        // 押すタイミング判定
        float pressTimingDifference = elapsedTime - absolutePressTargetTime;
        bool inPressWindow = Mathf.Abs(pressTimingDifference) <= clickGracePeriod;

        // 離すタイミング判定 
        float releaseGracePeriod = clickGracePeriod;
        float releaseTimingDifference = elapsedTime - absoluteReleaseTargetTime;
        bool inReleaseWindow = Mathf.Abs(releaseTimingDifference) <= releaseGracePeriod;

        // 押し始めの処理
        if (!waitingForRelease)
        {
            // マウス押下検出
            if (Input.GetMouseButtonDown(0))
            {
                if (parent.IsPointerOverUI())
                {
                    if (inPressWindow)
                    {
                        // 押すタイミングOK、離すのを待つ
                        waitingForRelease = true;
                        clickPressed = true;

                        // 長押し中の色に変更
                        parent.SetHoldingColor();
                    }
                    else
                    {
                        // 失敗 - 押すタイミング外
                        parent.TriggerFailEvent();
                        clickable = false;
                    }
                }
            }
        }
        // 長押し中の処理
        else if (waitingForRelease && clickPressed)
        {
            Debug.Log("長押ししているよ");
            // 自動成功判定: 指定された離すタイミングに達した場合
            if (elapsedTime >= absoluteReleaseTargetTime)
            {
                parent.TriggerSuccessEvent();
                Debug.Log("長押し自動成功");

                // 成功処理後、状態をリセット
                clickable = false;
                waitingForRelease = false;
                clickPressed = false;

                // 処理済みなので、ここで早期リターン
                return;
            }

            // マウスを離す検出
            if (Input.GetMouseButtonUp(0))
            {
                // リリース判定を別メソッドへ移動して呼び出し
                HandleLongPressRelease(elapsedTime);
                // 処理済みなので、ここで早期リターン
                return;
            }

            // 既に離すべき時間を過ぎているか確認
            if (elapsedTime > absoluteReleaseTargetTime)
            {
                // 長押しが大幅に長すぎる場合の処理
                parent.TriggerSuccessEvent();
                Debug.Log("長押し持続成功（猶予時間外）");

                // 成功処理後、状態をリセット
                clickable = false;
                waitingForRelease = false;
                clickPressed = false;

                // 処理済みなので、ここで早期リターン
                return;
            }
        }
    }
}