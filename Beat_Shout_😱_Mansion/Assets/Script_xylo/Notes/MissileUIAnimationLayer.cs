using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIアニメーションレイヤーの基本クラス
/// </summary>
public class MissileUIAnimationLayer : MonoBehaviour
{
    protected Sprite[] sprites;
    protected float frameDelay;
    protected Image image;
    protected CanvasGroup canvasGroup;
    protected int currentFrame = 0;
    protected float frameTimer = 0f;
    protected bool isPlaying = false;
    protected Coroutine animationCoroutine;

    // tint色（デフォルトは白＝色変化なし）
    protected Color tintColor = Color.white;

    public virtual void Initialize(Sprite[] sprites, float frameDelay)
    {
        this.sprites = sprites;
        this.frameDelay = frameDelay;

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

        SetVisibility(false);
    }

    /// <summary>
    /// tint色を設定（alphaはCanvasGroupで管理するのでRGBのみ反映）
    /// </summary>
    public virtual void SetTintColor(Color color)
    {
        tintColor = color;
        if (image != null)
        {
            // alphaは既存のImage.color.aを維持しつつRGBを変更
            Color applied = new Color(color.r, color.g, color.b, image.color.a);
            image.color = applied;
        }
    }

    public virtual void ChangeSprites(Sprite[] newSprites)
    {
        this.sprites = newSprites;

        if (this.sprites == null)
        {
            this.sprites = new Sprite[0];
            Debug.LogWarning("MissileUIAnimationLayer: スプライト配列が null です。空の配列を使用します。");
        }

        if (sprites.Length > 0 && image != null)
        {
            currentFrame = 0;
            image.sprite = sprites[0];
        }
    }

    public virtual void SetVisibility(bool visible)
    {
        if (image == null) return;

        image.enabled = visible;

        if (!visible && animationCoroutine != null)
        {
            StopAnimation();
        }
        else if (visible && animationCoroutine == null && isPlaying)
        {
            SafeRestartAnimation();
        }
    }

    public virtual void SetAlpha(float alpha)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = alpha;
    }

    public virtual void SetFrameDelay(float delay)
    {
        this.frameDelay = delay;
    }

    public virtual bool IsVisible()
    {
        return image != null && image.enabled;
    }

    public virtual void SafeRestartAnimation()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"{gameObject.name}が非アクティブなためアニメーションコルーチンを開始できません");
            isPlaying = true;
            return;
        }

        RestartAnimation();
    }

    public virtual void RestartAnimation()
    {
        StopAnimation();

        if (sprites == null || sprites.Length == 0 || image == null) return;

        currentFrame = 0;
        image.sprite = sprites[0];

        // tint色を再適用
        Color applied = new Color(tintColor.r, tintColor.g, tintColor.b, image.color.a);
        image.color = applied;

        if (gameObject.activeInHierarchy)
        {
            isPlaying = true;
            animationCoroutine = StartCoroutine(AnimationCoroutine());
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}が非アクティブなためアニメーションコルーチンを開始できません");
            isPlaying = true;
        }
    }

    public virtual void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        isPlaying = false;
    }

    protected virtual IEnumerator AnimationCoroutine()
    {
        isPlaying = true;

        if (sprites.Length <= 1)
        {
            if (sprites.Length == 1)
            {
                image.sprite = sprites[0];
            }

            isPlaying = false;
            yield break;
        }

        while (isPlaying)
        {
            yield return new WaitForSeconds(frameDelay);

            currentFrame = (currentFrame + 1) % sprites.Length;

            if (this == null || !gameObject.activeInHierarchy)
            {
                isPlaying = false;
                yield break;
            }

            image.sprite = sprites[currentFrame];
        }
    }
}