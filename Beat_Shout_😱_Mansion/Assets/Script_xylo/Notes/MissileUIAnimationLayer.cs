using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIアニメーションレイヤーの基本クラス
/// </summary>
public class MissileUIAnimationLayer : MonoBehaviour
{
    // アニメーション用変数
    protected Sprite[] sprites;
    protected float frameDelay;
    protected Image image;
    protected CanvasGroup canvasGroup;
    protected int currentFrame = 0;
    protected float frameTimer = 0f;
    protected bool isPlaying = false;
    protected Coroutine animationCoroutine;

    /// <summary>
    /// 初期化
    /// </summary>
    public virtual void Initialize(Sprite[] sprites, float frameDelay)
    {
        this.sprites = sprites;
        this.frameDelay = frameDelay;

        // コンポーネントの取得
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (image == null)
        {
            Debug.LogError("MissileUIAnimationLayer: Image コンポーネントが見つかりません");
            return;
        }

        if (canvasGroup == null)
        {
            Debug.LogError("MissileUIAnimationLayer: CanvasGroup コンポーネントが見つかりません");
            return;
        }

        // 初期化時は非表示に
        SetVisibility(false);
    }

    /// <summary>
    /// スプライト配列を変更
    /// </summary>
    public virtual void ChangeSprites(Sprite[] newSprites)
    {
        this.sprites = newSprites;

        // スプライトが設定されていない場合は空の配列を使用
        if (this.sprites == null)
        {
            this.sprites = new Sprite[0];
            Debug.LogWarning("MissileUIAnimationLayer: スプライト配列が null です。空の配列を使用します。");
        }

        // 現在のフレームが範囲内かチェック
        if (sprites.Length > 0 && image != null)
        {
            currentFrame = 0;
            image.sprite = sprites[0];
        }
    }

    /// <summary>
    /// 可視性を設定
    /// </summary>
    public virtual void SetVisibility(bool visible)
    {
        if (image == null) return;

        image.enabled = visible;

        // 非表示時はアニメーションも停止
        if (!visible && animationCoroutine != null)
        {
            StopAnimation();
        }
        else if (visible && animationCoroutine == null)
        {
            // 表示時にアニメーションを開始
            RestartAnimation();
        }
    }

    /// <summary>
    /// 透明度を設定
    /// </summary>
    public virtual void SetAlpha(float alpha)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = alpha;
    }

    /// <summary>
    /// フレーム間の時間を設定
    /// </summary>
    public virtual void SetFrameDelay(float delay)
    {
        this.frameDelay = delay;
    }

    /// <summary>
    /// 可視状態かどうかを取得
    /// </summary>
    public virtual bool IsVisible()
    {
        return image != null && image.enabled;
    }

    /// <summary>
    /// アニメーションを再開
    /// </summary>
    public virtual void RestartAnimation()
    {
        // 既存のアニメーションを停止
        StopAnimation();

        // スプライトがない場合は何もしない
        if (sprites == null || sprites.Length == 0 || image == null) return;

        // 初期フレームを設定
        currentFrame = 0;
        image.sprite = sprites[0];

        // アニメーションコルーチンを開始
        animationCoroutine = StartCoroutine(AnimationCoroutine());
    }

    /// <summary>
    /// アニメーションを停止
    /// </summary>
    public virtual void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        isPlaying = false;
    }

    /// <summary>
    /// アニメーションコルーチン
    /// </summary>
    protected virtual IEnumerator AnimationCoroutine()
    {
        isPlaying = true;

        // スプライトが1つ以下なら固定表示
        if (sprites.Length <= 1)
        {
            if (sprites.Length == 1)
            {
                image.sprite = sprites[0];
            }

            isPlaying = false;
            yield break;
        }

        // ループアニメーション
        while (isPlaying)
        {
            // フレーム間の時間を待機
            yield return new WaitForSeconds(frameDelay);

            // 次のフレームに進む
            currentFrame = (currentFrame + 1) % sprites.Length;
            image.sprite = sprites[currentFrame];
        }
    }
}