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
    private bool requireLongPress = false;
    private int requiredBeats = 1;        // 長押しの場合の必要拍数
    private bool waitingForRelease = false;
    private bool clickPressed = false;
    private float clickTimer = 0f;
    private float clickTargetTime = 0f;
    private float releaseTargetTime = 0f;

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
        requireLongPress = false;
        requiredBeats = 1;
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
                    requireLongPress = false;
                    requiredBeats = 0;
                    clickTargetTime = oneBeat * 4; // 4拍目でクリック
                    break;

                case MissileNoteType.Long1Beat:
                    requireLongPress = true;
                    requiredBeats = 1;
                    clickTargetTime = oneBeat * 4;    // 4拍目で押す
                    releaseTargetTime = oneBeat * 5;  // 5拍目で離す
                    break;

                case MissileNoteType.Long2Beat:
                    requireLongPress = true;
                    requiredBeats = 2;
                    clickTargetTime = oneBeat * 4;    // 4拍目で押す
                    releaseTargetTime = oneBeat * 6;  // 6拍目で離す (2拍後)
                    break;

                case MissileNoteType.Long3Beat:
                    requireLongPress = true;
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
        if (!clickable) return;

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
            Debug.Log($"短押しノーツ: マウスクリックを検出しました（経過時間: {elapsedTime:F2}秒, 目標時間: {absoluteClickTargetTime:F2}秒）");

            // クリック位置がUI上かチェック
            if (parent.IsPointerOverUI())
            {
                Debug.Log($"短押しノーツ: UI上のクリックを検出しました");
                if (inClickWindow)
                {
                    // 成功！
                    Debug.Log($"クリック成功！ ジャストタイミングとの差: {timingDifference:F3}秒（猶予時間: ±{clickGracePeriod:F3}秒）【絶対時間】");
                    parent.TriggerSuccessEvent();
                }
                else
                {
                    // 失敗 - タイミング外
                    Debug.Log($"クリック失敗: ジャストタイミングとの差: {timingDifference:F3}秒（猶予時間: ±{clickGracePeriod:F3}秒）【絶対時間】");

                    // 早すぎるクリックの場合でもアニメーションは継続
                    if (timingDifference < 0)
                    {
                        // 早すぎるクリックだが、アニメーションは停止せずに失敗のみ記録
                        parent.TriggerFailEvent();
                    }
                    else
                    {
                        // 遅すぎるクリックは通常の失敗処理
                        parent.TriggerFailEvent();
                    }
                }

                // クリック判定を無効化
                clickable = false;
            }
            else
            {
                Debug.Log($"短押しノーツ: クリックはUI上ではありませんでした");
            }
        }

        // デバッグ用：クリック判定ウィンドウの表示
        if (inClickWindow && !clickPressed)
        {
            Debug.Log($"短押しノーツ: 現在クリック判定ウィンドウ内です！ 残り時間: {(clickGracePeriod - Mathf.Abs(timingDifference)):F3}秒");
        }
    }
    // MissileInputManagerクラスに追加する新しいメソッド
    public void StartLongPress(float startTime)
    {
        waitingForRelease = true;
        clickPressed = true;

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

        Debug.Log($"長押し開始: リリース目標時間は {releaseTargetTime:F2}秒です");
    }

    /// <summary>
    /// 絶対時間ベースのLongクリック判定
    /// </summary>
    private void HandleLongClickAbsoluteTime(float elapsedTime, MissileNoteType noteType)
    {
        // ジャストタイミングの絶対時間（生成から4ビート後に押し、noteTypeに基づいた拍数後に離す）
        float absolutePressTargetTime = oneBeat * 4;

        // リリースタイミングはノーツタイプによって異なる
        float absoluteReleaseTargetTime;
        switch (noteType)
        {
            case MissileNoteType.Long1Beat:
                absoluteReleaseTargetTime = oneBeat * 5;  // 1拍後
                break;
            case MissileNoteType.Long2Beat:
                absoluteReleaseTargetTime = oneBeat * 6;  // 2拍後
                break;
            case MissileNoteType.Long3Beat:
                absoluteReleaseTargetTime = oneBeat * 7;  // 3拍後
                break;
            default:
                absoluteReleaseTargetTime = oneBeat * 5;  // デフォルト
                break;
        }

        // 押すタイミング判定
        float pressTimingDifference = elapsedTime - absolutePressTargetTime;
        bool inPressWindow = Mathf.Abs(pressTimingDifference) <= clickGracePeriod;

        // 離すタイミング判定 
        float releaseGracePeriod = clickGracePeriod;
        float releaseTimingDifference = elapsedTime - absoluteReleaseTargetTime;
        bool inReleaseWindow = Mathf.Abs(releaseTimingDifference) <= releaseGracePeriod;

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

                        Debug.Log($"長押し開始成功！ ジャストタイミングとの差: {pressTimingDifference:F3}秒（猶予時間: ±{clickGracePeriod:F3}秒）【絶対時間】");

                        // 長押し中の色に変更（青）
                        parent.SetHoldingColor();
                    }
                    else
                    {
                        // 失敗 - 押すタイミング外
                        Debug.Log($"長押し開始失敗: ジャストタイミングとの差: {pressTimingDifference:F3}秒（猶予時間: ±{clickGracePeriod:F3}秒）【絶対時間】");

                        // 早すぎるクリックの場合でもアニメーションは継続
                        if (pressTimingDifference < 0)
                        {
                            // 早すぎるクリックだが、アニメーションは停止せずに失敗のみ記録
                            parent.TriggerFailEvent();
                        }
                        else
                        {
                            // 遅すぎるクリックは通常の失敗処理
                            parent.TriggerFailEvent();
                        }

                        clickable = false;
                    }
                }
            }
        }
        else
        {
            // マウスを離す検出
            if (Input.GetMouseButtonUp(0))
            {
                if (inReleaseWindow)
                {
                    // 成功！
                    Debug.Log($"長押し解放成功！ ジャストタイミングとの差: {releaseTimingDifference:F3}秒（猶予時間: ±{releaseGracePeriod:F3}秒）【絶対時間】");
                    parent.TriggerSuccessEvent();
                }
                else
                {
                    // 失敗 - 離すタイミング外
                    Debug.Log($"長押し解放失敗: ジャストタイミングとの差: {releaseTimingDifference:F3}秒（猶予時間: ±{releaseGracePeriod:F3}秒）【絶対時間】");
                    parent.TriggerFailEvent();
                }

                // クリック判定を無効化
                clickable = false;
                waitingForRelease = false;
                clickPressed = false;
            }
        }
    }
}