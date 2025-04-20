using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アニメーションの管理を行うクラス
/// </summary>
public class MissileAnimationManager
{
    private MissileDirectAnimManagerB parent;
    private MissileUIManager uiManager;

    // アニメーションレイヤー
    private MissileUIAnimationLayer animLayer1st;
    private MissileUIAnimationLayer animLayer2nd;
    private MissileUIAnimationLayer animLayer3rd;
    private MissileUIAnimationLayer animLayer4th;
    private MissileUIAnimationLayer animLayer5th;  // 長押し2拍、3拍用の追加レイヤー
    private MissileUIAnimationLayer animLayer6th;  // 長押し3拍用の追加レイヤー
    private MissileUIHitAnimationLayer hitLayer;

    // アニメーション状態
    private bool animationStarted = false;
    private bool animationCompleted = false;
    private int currentAnimStage = 0;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MissileAnimationManager(MissileDirectAnimManagerB parent, MissileUIManager uiManager)
    {
        this.parent = parent;
        this.uiManager = uiManager;

        // アニメーションレイヤーを作成
        CreateAnimationLayers();
    }

    /// <summary>
    /// アニメーションレイヤーを作成して初期化する
    /// </summary>
    private void CreateAnimationLayers()
    {
        // 1stレイヤー
        GameObject layer1st = uiManager.CreateLayer("AnimLayer1st");
        animLayer1st = layer1st.AddComponent<MissileUIAnimationLayer>();
        animLayer1st.Initialize(parent.GetSpritesForType(MissileAnimationType.Short1st), parent.GetNormalFrameDelay());
        animLayer1st.SetVisibility(false); // 初期状態では非表示
        animLayer1st.SetAlpha(uiManager.GetLayer1Alpha());

        // 2ndレイヤー
        GameObject layer2nd = uiManager.CreateLayer("AnimLayer2nd");
        animLayer2nd = layer2nd.AddComponent<MissileUIAnimationLayer>();
        animLayer2nd.Initialize(parent.GetSpritesForType(MissileAnimationType.Short2nd), parent.GetNormalFrameDelay());
        animLayer2nd.SetVisibility(false); // 初期状態では非表示
        animLayer2nd.SetAlpha(uiManager.GetLayer2Alpha());

        // 3rdレイヤー
        GameObject layer3rd = uiManager.CreateLayer("AnimLayer3rd");
        animLayer3rd = layer3rd.AddComponent<MissileUIAnimationLayer>();
        animLayer3rd.Initialize(parent.GetSpritesForType(MissileAnimationType.Short3rd), parent.GetNormalFrameDelay());
        animLayer3rd.SetVisibility(false); // 初期状態では非表示

        // 4thレイヤー
        GameObject layer4th = uiManager.CreateLayer("AnimLayer4th");
        animLayer4th = layer4th.AddComponent<MissileUIAnimationLayer>();
        animLayer4th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long4th), parent.GetNormalFrameDelay());
        animLayer4th.SetVisibility(false); // 初期状態では非表示

        // 5thレイヤー (長押し2拍、3拍用)
        GameObject layer5th = uiManager.CreateLayer("AnimLayer5th");
        animLayer5th = layer5th.AddComponent<MissileUIAnimationLayer>();
        animLayer5th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long02_01), parent.GetNormalFrameDelay());
        animLayer5th.SetVisibility(false); // 初期状態では非表示

        // 6thレイヤー (長押し3拍用)
        GameObject layer6th = uiManager.CreateLayer("AnimLayer6th");
        animLayer6th = layer6th.AddComponent<MissileUIAnimationLayer>();
        animLayer6th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long03_01), parent.GetNormalFrameDelay());
        animLayer6th.SetVisibility(false); // 初期状態では非表示

        //// Hitレイヤー（特殊なHitアニメーション用のレイヤー）
        //GameObject layerHit = uiManager.CreateLayer("HitLayer");
        //hitLayer = layerHit.AddComponent<MissileUIHitAnimationLayer>();
        //hitLayer.Initialize(parent.GetSpritesForType(MissileAnimationType.Hit), parent.GetHitFrameDelay());
        //hitLayer.SetVisibility(false); // 初期状態は非表示
    }

    /// <summary>
    /// 全レイヤーを非表示に設定
    /// </summary>
    public void SetAllLayersInvisible()
    {
        animLayer1st.SetVisibility(false);
        animLayer2nd.SetVisibility(false);
        animLayer3rd.SetVisibility(false);
        animLayer4th.SetVisibility(false);
        animLayer5th.SetVisibility(false);
        animLayer6th.SetVisibility(false);
  //      hitLayer.SetVisibility(false);
    }

    /// <summary>
    /// すべてのアニメーションレイヤーを停止
    /// </summary>
    public void StopAllAnimations()
    {
        animLayer1st.StopAnimation();
        animLayer2nd.StopAnimation();
        animLayer3rd.StopAnimation();
        animLayer4th.StopAnimation();
        animLayer5th.StopAnimation();
        animLayer6th.StopAnimation();
      //  hitLayer.StopAnimation();
    }

    /// <summary>
    /// アニメーションを手動でリセットする
    /// </summary>
    public void ResetAllAnimations()
    {
        animationStarted = false;
        animationCompleted = false;
        currentAnimStage = 0;
        SetAllLayersInvisible();
    }

    /// <summary>
    /// 表示されているアニメーションレイヤーのみを再スタート
    /// </summary>
    public void RestartVisibleAnimations()
    {
        if (animLayer1st.IsVisible())
            animLayer1st.RestartAnimation();

        if (animLayer2nd.IsVisible())
            animLayer2nd.RestartAnimation();

        if (animLayer3rd.IsVisible())
            animLayer3rd.RestartAnimation();

        if (animLayer4th.IsVisible())
            animLayer4th.RestartAnimation();

        if (animLayer5th.IsVisible())
            animLayer5th.RestartAnimation();

        if (animLayer6th.IsVisible())
            animLayer6th.RestartAnimation();

        // Hitレイヤーは可視状態の場合のみ再スタート
        //if (hitLayer.IsVisible())
        //    hitLayer.RestartAnimation();
    }

    /// <summary>
    /// 通常レイヤーのフレーム間の時間を更新
    /// </summary>
    public void UpdateFrameDelays(float frameDelay)
    {
        animLayer1st.SetFrameDelay(frameDelay);
        animLayer2nd.SetFrameDelay(frameDelay);
        animLayer3rd.SetFrameDelay(frameDelay);
        animLayer4th.SetFrameDelay(frameDelay);
        animLayer5th.SetFrameDelay(frameDelay);
        animLayer6th.SetFrameDelay(frameDelay);
        // hitLayerは固定値なので更新しない
    }

    /// <summary>
    /// テンポに合わせたアニメーション更新
    /// </summary>
    public void HandleTempoTick(MissileNoteType noteType)
    {
        // すでにアニメーションが完了している場合は何もしない
        if (animationCompleted) return;

        // アニメーションをまだ開始していない場合は開始する
        if (!animationStarted)
        {
            StartAnimation(noteType);
            return;
        }

        // 次のアニメーションステージに進む
        AdvanceAnimationStage(noteType);
    }

    /// <summary>
    /// アニメーションを開始する
    /// </summary>
    private void StartAnimation(MissileNoteType noteType)
    {
        animationStarted = true;
        currentAnimStage = 0;

        // すべてのレイヤーを非表示にする
        SetAllLayersInvisible();

        // ノーツタイプに基づいて最初のアニメーションを設定
        PlayCurrentAnimationStage(noteType);
    }

    /// <summary>
    /// 次のアニメーションステージに進む
    /// </summary>
    private void AdvanceAnimationStage(MissileNoteType noteType)
    {
        currentAnimStage++;

        // 選択されたパターンの配列長をチェック
        MissileAnimationType[] sequence = MissileAnimationSequences.GetSequenceForNoteType(noteType);

        // すべてのステージが終了したかチェック
        if (sequence.Length == 0 || currentAnimStage >= sequence.Length)
        {
            CompleteAnimation();
            return;
        }

        // 次のアニメーションを再生
        PlayCurrentAnimationStage(noteType);
    }

    /// <summary>
    /// 現在のステージのアニメーションを再生する
    /// </summary>
    private void PlayCurrentAnimationStage(MissileNoteType noteType)
    {
        // すべてのレイヤーを非表示にする
        SetAllLayersInvisible();

        // パターンがNone（なし）の場合は何も表示しない
        if (noteType == MissileNoteType.None) return;

        // 現在のアニメーションタイプを取得
        MissileAnimationType[] sequence = MissileAnimationSequences.GetSequenceForNoteType(noteType);

        if (sequence.Length == 0 || currentAnimStage >= sequence.Length)
        {
            return;
        }

        MissileAnimationType currentType = sequence[currentAnimStage];

        // アニメーションタイプに応じたレイヤーを表示
        switch (currentType)
        {
            case MissileAnimationType.Short1st:
                animLayer1st.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Short1st));
                animLayer1st.SetVisibility(true);
                animLayer1st.RestartAnimation();
                break;
            case MissileAnimationType.Short2nd:
                animLayer2nd.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Short2nd));
                animLayer2nd.SetVisibility(true);
                animLayer2nd.RestartAnimation();
                break;
            case MissileAnimationType.Short3rd:
                animLayer3rd.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Short3rd));
                animLayer3rd.SetVisibility(true);
                animLayer3rd.RestartAnimation();
                break;
            case MissileAnimationType.Long1st:
                animLayer1st.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long1st));
                animLayer1st.SetVisibility(true);
                animLayer1st.RestartAnimation();
                break;
            case MissileAnimationType.Long2nd:
                animLayer2nd.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long2nd));
                animLayer2nd.SetVisibility(true);
                animLayer2nd.RestartAnimation();
                break;
            case MissileAnimationType.Long3rd:
                animLayer3rd.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long3rd));
                animLayer3rd.SetVisibility(true);
                animLayer3rd.RestartAnimation();
                break;
            case MissileAnimationType.Long4th:
                animLayer4th.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long4th));
                animLayer4th.SetVisibility(true);
                animLayer4th.RestartAnimation();
                break;
            case MissileAnimationType.Long02_01:  // 2拍長押し1段階目
                animLayer5th.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long02_01));
                animLayer5th.SetVisibility(true);
                animLayer5th.RestartAnimation();
                break;
            case MissileAnimationType.Long02_02:  // 2拍長押し2段階目
                animLayer5th.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long02_02));
                animLayer5th.SetVisibility(true);
                animLayer5th.RestartAnimation();
                break;
            case MissileAnimationType.Long03_01:  // 3拍長押し1段階目
                animLayer5th.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long03_01));
                animLayer5th.SetVisibility(true);
                animLayer5th.RestartAnimation();
                break;
            case MissileAnimationType.Long03_02:  // 3拍長押し2段階目
                animLayer5th.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long03_02));
                animLayer5th.SetVisibility(true);
                animLayer5th.RestartAnimation();
                break;
            case MissileAnimationType.Long03_03:  // 3拍長押し3段階目
                animLayer6th.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Long03_03));
                animLayer6th.SetVisibility(true);
                animLayer6th.RestartAnimation();
                break;
            //case MissileAnimationType.Hit:
            //    hitLayer.ChangeSprites(parent.GetSpritesForType(MissileAnimationType.Hit));
            //    hitLayer.SetVisibility(true);
            //    hitLayer.RestartAnimation();
            //    break;
        }
    }

    /// <summary>
    /// アニメーションを完了
    /// </summary>
    private void CompleteAnimation()
    {
        animationCompleted = true;
        SetAllLayersInvisible();
    }

    /// <summary>
    /// Hitアニメーションを手動で開始
    /// </summary>
    public void TriggerHitAnimation()
    {
        hitLayer.SetVisibility(true);
        hitLayer.RestartAnimation();
    }

    /// <summary>
    /// アニメーションが完了しているかどうか
    /// </summary>
    public bool IsAnimationCompleted()
    {
        return animationCompleted;
    }

    /// <summary>
    /// 現在のアニメーションステージを取得
    /// </summary>
    public int GetCurrentAnimStage()
    {
        return currentAnimStage;
    }

    /// <summary>
    /// アニメーションが開始されているかどうか
    /// </summary>
    public bool IsAnimationStarted()
    {
        return animationStarted;
    }
}