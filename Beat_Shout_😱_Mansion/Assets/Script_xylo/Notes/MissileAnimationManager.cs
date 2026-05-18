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
        layer1st.SetActive(true); // 明示的にアクティブに設定
        animLayer1st = layer1st.AddComponent<MissileUIAnimationLayer>();
        animLayer1st.Initialize(parent.GetSpritesForType(MissileAnimationType.Short1st), parent.GetNormalFrameDelay());
        animLayer1st.SetVisibility(false); // 初期状態では非表示
        animLayer1st.SetAlpha(uiManager.GetLayer1Alpha());

        // 2ndレイヤー
        GameObject layer2nd = uiManager.CreateLayer("AnimLayer2nd");
        layer2nd.SetActive(true); // 明示的にアクティブに設定
        animLayer2nd = layer2nd.AddComponent<MissileUIAnimationLayer>();
        animLayer2nd.Initialize(parent.GetSpritesForType(MissileAnimationType.Short2nd), parent.GetNormalFrameDelay());
        animLayer2nd.SetVisibility(false); // 初期状態では非表示
        animLayer2nd.SetAlpha(uiManager.GetLayer2Alpha());

        // 3rdレイヤー
        GameObject layer3rd = uiManager.CreateLayer("AnimLayer3rd");
        layer3rd.SetActive(true); // 明示的にアクティブに設定
        animLayer3rd = layer3rd.AddComponent<MissileUIAnimationLayer>();
        animLayer3rd.Initialize(parent.GetSpritesForType(MissileAnimationType.Short3rd), parent.GetNormalFrameDelay());
        animLayer3rd.SetVisibility(false); // 初期状態では非表示

        // 4thレイヤー
        GameObject layer4th = uiManager.CreateLayer("AnimLayer4th");
        layer4th.SetActive(true); // 明示的にアクティブに設定
        animLayer4th = layer4th.AddComponent<MissileUIAnimationLayer>();
        animLayer4th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long4th), parent.GetNormalFrameDelay());
        animLayer4th.SetVisibility(false); // 初期状態では非表示

        // 5thレイヤー (長押し2拍、3拍用)
        GameObject layer5th = uiManager.CreateLayer("AnimLayer5th");
        layer5th.SetActive(true); // 明示的にアクティブに設定
        animLayer5th = layer5th.AddComponent<MissileUIAnimationLayer>();
        animLayer5th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long02_01), parent.GetNormalFrameDelay());
        animLayer5th.SetVisibility(false); // 初期状態では非表示

        // 6thレイヤー (長押し3拍用)
        GameObject layer6th = uiManager.CreateLayer("AnimLayer6th");
        layer6th.SetActive(true); // 明示的にアクティブに設定
        animLayer6th = layer6th.AddComponent<MissileUIAnimationLayer>();
        animLayer6th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long03_01), parent.GetNormalFrameDelay());
        animLayer6th.SetVisibility(false); // 初期状態では非表示

        // Hitレイヤー関連のコードはコメントアウトされているので触れていません
    }

    /// <summary>
    /// 全レイヤーを非表示に設定
    /// </summary>
    public void SetAllLayersInvisible()
    {
        if (animLayer1st != null) animLayer1st.SetVisibility(false);
        if (animLayer2nd != null) animLayer2nd.SetVisibility(false);
        if (animLayer3rd != null) animLayer3rd.SetVisibility(false);
        if (animLayer4th != null) animLayer4th.SetVisibility(false);
        if (animLayer5th != null) animLayer5th.SetVisibility(false);
        if (animLayer6th != null) animLayer6th.SetVisibility(false);
        // if (hitLayer != null) hitLayer.SetVisibility(false);
    }

    /// <summary>
    /// すべてのアニメーションレイヤーを停止
    /// </summary>
    public void StopAllAnimations()
    {
        if (animLayer1st != null) animLayer1st.StopAnimation();
        if (animLayer2nd != null) animLayer2nd.StopAnimation();
        if (animLayer3rd != null) animLayer3rd.StopAnimation();
        if (animLayer4th != null) animLayer4th.StopAnimation();
        if (animLayer5th != null) animLayer5th.StopAnimation();
        if (animLayer6th != null) animLayer6th.StopAnimation();
        // if (hitLayer != null) hitLayer.StopAnimation();
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
        if (animLayer1st != null && animLayer1st.IsVisible())
            animLayer1st.SafeRestartAnimation();

        if (animLayer2nd != null && animLayer2nd.IsVisible())
            animLayer2nd.SafeRestartAnimation();

        if (animLayer3rd != null && animLayer3rd.IsVisible())
            animLayer3rd.SafeRestartAnimation();

        if (animLayer4th != null && animLayer4th.IsVisible())
            animLayer4th.SafeRestartAnimation();

        if (animLayer5th != null && animLayer5th.IsVisible())
            animLayer5th.SafeRestartAnimation();

        if (animLayer6th != null && animLayer6th.IsVisible())
            animLayer6th.SafeRestartAnimation();

        // hitLayerのコードはコメントアウトされているので触れていません
    }

    /// <summary>
    /// 通常レイヤーのフレーム間の時間を更新
    /// </summary>
    public void UpdateFrameDelays(float frameDelay)
    {
        if (animLayer1st != null) animLayer1st.SetFrameDelay(frameDelay);
        if (animLayer2nd != null) animLayer2nd.SetFrameDelay(frameDelay);
        if (animLayer3rd != null) animLayer3rd.SetFrameDelay(frameDelay);
        if (animLayer4th != null) animLayer4th.SetFrameDelay(frameDelay);
        if (animLayer5th != null) animLayer5th.SetFrameDelay(frameDelay);
        if (animLayer6th != null) animLayer6th.SetFrameDelay(frameDelay);
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

        // レイヤーのゲームオブジェクトのアクティブ状態を確認
        EnsureLayersActive();

        // 次のアニメーションステージに進む
        AdvanceAnimationStage(noteType);
    }

    /// <summary>
    /// すべてのレイヤーのゲームオブジェクトがアクティブであることを確認
    /// </summary>
    private void EnsureLayersActive()
    {
        if (animLayer1st != null && animLayer1st.gameObject != null) animLayer1st.gameObject.SetActive(true);
        if (animLayer2nd != null && animLayer2nd.gameObject != null) animLayer2nd.gameObject.SetActive(true);
        if (animLayer3rd != null && animLayer3rd.gameObject != null) animLayer3rd.gameObject.SetActive(true);
        if (animLayer4th != null && animLayer4th.gameObject != null) animLayer4th.gameObject.SetActive(true);
        if (animLayer5th != null && animLayer5th.gameObject != null) animLayer5th.gameObject.SetActive(true);
        if (animLayer6th != null && animLayer6th.gameObject != null) animLayer6th.gameObject.SetActive(true);
        // if (hitLayer != null && hitLayer.gameObject != null) hitLayer.gameObject.SetActive(true);
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

        // レイヤーのゲームオブジェクトがアクティブであることを確認
        EnsureLayersActive();

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
    /// レイヤーのアニメーションを安全に開始するヘルパーメソッド
    /// </summary>
    private void SafePlayAnimation(MissileUIAnimationLayer layer, MissileAnimationType type)
    {
        if (layer == null) return;

        // ゲームオブジェクトがnullであるか非アクティブな場合はスキップ
        if (layer.gameObject == null)
        {
            Debug.LogWarning($"アニメーションレイヤーのゲームオブジェクトがnullです: {type}");
            return;
        }

        // ゲームオブジェクトをアクティブにする
        layer.gameObject.SetActive(true);

        // スプライトを変更
        layer.ChangeSprites(parent.GetSpritesForType(type));

        // 可視性を設定
        layer.SetVisibility(true);

        try
        {
            // アニメーションを再開（安全なメソッドを使用）
            layer.SafeRestartAnimation();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"アニメーション開始エラー: {e.Message}");
        }
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

        // アニメーションタイプに応じたレイヤーで安全に再生
        switch (currentType)
        {
            case MissileAnimationType.Short1st:
            case MissileAnimationType.Long1st:
                SafePlayAnimation(animLayer1st, currentType);
                break;
            case MissileAnimationType.Short2nd:
            case MissileAnimationType.Long2nd:
                SafePlayAnimation(animLayer2nd, currentType);
                break;
            case MissileAnimationType.Short3rd:
            case MissileAnimationType.Long3rd:
                SafePlayAnimation(animLayer3rd, currentType);
                break;
            case MissileAnimationType.Long4th:
                SafePlayAnimation(animLayer4th, currentType);
                break;
            case MissileAnimationType.Long02_01:
            case MissileAnimationType.Long02_02:
            case MissileAnimationType.Long03_01:
            case MissileAnimationType.Long03_02:
                SafePlayAnimation(animLayer5th, currentType);
                break;
            case MissileAnimationType.Long03_03:
                SafePlayAnimation(animLayer6th, currentType);
                break;
                // Hitアニメーションのケースはコメントアウトされています
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
        // hitLayerが使用されていないためコメントアウト
        // if (hitLayer != null)
        // {
        //     hitLayer.SetVisibility(true);
        //     hitLayer.SafeRestartAnimation();
        // }
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