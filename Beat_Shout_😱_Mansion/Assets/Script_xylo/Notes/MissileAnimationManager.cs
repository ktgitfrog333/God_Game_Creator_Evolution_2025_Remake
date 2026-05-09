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

    private MissileUIAnimationLayer animLayer1st;
    private MissileUIAnimationLayer animLayer2nd;
    private MissileUIAnimationLayer animLayer3rd;
    private MissileUIAnimationLayer animLayer4th;
    private MissileUIAnimationLayer animLayer5th;
    private MissileUIAnimationLayer animLayer6th;
    private MissileUIHitAnimationLayer hitLayer;

    private bool animationStarted = false;
    private bool animationCompleted = false;
    private int currentAnimStage = 0;

    // 色tint用
    private Color layerTintColor = Color.white;

    public MissileAnimationManager(MissileDirectAnimManagerB parent, MissileUIManager uiManager)
    {
        this.parent = parent;
        this.uiManager = uiManager;
        CreateAnimationLayers();
    }

    private void CreateAnimationLayers()
    {
        GameObject layer1st = uiManager.CreateLayer("AnimLayer1st");
        layer1st.SetActive(true);
        animLayer1st = layer1st.AddComponent<MissileUIAnimationLayer>();
        animLayer1st.Initialize(parent.GetSpritesForType(MissileAnimationType.Short1st), parent.GetNormalFrameDelay());
        animLayer1st.SetVisibility(false);
        animLayer1st.SetAlpha(uiManager.GetLayer1Alpha());

        GameObject layer2nd = uiManager.CreateLayer("AnimLayer2nd");
        layer2nd.SetActive(true);
        animLayer2nd = layer2nd.AddComponent<MissileUIAnimationLayer>();
        animLayer2nd.Initialize(parent.GetSpritesForType(MissileAnimationType.Short2nd), parent.GetNormalFrameDelay());
        animLayer2nd.SetVisibility(false);
        animLayer2nd.SetAlpha(uiManager.GetLayer2Alpha());

        GameObject layer3rd = uiManager.CreateLayer("AnimLayer3rd");
        layer3rd.SetActive(true);
        animLayer3rd = layer3rd.AddComponent<MissileUIAnimationLayer>();
        animLayer3rd.Initialize(parent.GetSpritesForType(MissileAnimationType.Short3rd), parent.GetNormalFrameDelay());
        animLayer3rd.SetVisibility(false);

        GameObject layer4th = uiManager.CreateLayer("AnimLayer4th");
        layer4th.SetActive(true);
        animLayer4th = layer4th.AddComponent<MissileUIAnimationLayer>();
        animLayer4th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long4th), parent.GetNormalFrameDelay());
        animLayer4th.SetVisibility(false);

        GameObject layer5th = uiManager.CreateLayer("AnimLayer5th");
        layer5th.SetActive(true);
        animLayer5th = layer5th.AddComponent<MissileUIAnimationLayer>();
        animLayer5th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long02_01), parent.GetNormalFrameDelay());
        animLayer5th.SetVisibility(false);

        GameObject layer6th = uiManager.CreateLayer("AnimLayer6th");
        layer6th.SetActive(true);
        animLayer6th = layer6th.AddComponent<MissileUIAnimationLayer>();
        animLayer6th.Initialize(parent.GetSpritesForType(MissileAnimationType.Long03_01), parent.GetNormalFrameDelay());
        animLayer6th.SetVisibility(false);
    }

    /// <summary>
    /// 全レイヤーに色tintを設定
    /// </summary>
    public void SetLayerTintColor(Color color)
    {
        layerTintColor = color;
        ApplyTintToAllLayers();
    }

    private void ApplyTintToAllLayers()
    {
        if (animLayer1st != null) animLayer1st.SetTintColor(layerTintColor);
        if (animLayer2nd != null) animLayer2nd.SetTintColor(layerTintColor);
        if (animLayer3rd != null) animLayer3rd.SetTintColor(layerTintColor);
        if (animLayer4th != null) animLayer4th.SetTintColor(layerTintColor);
        if (animLayer5th != null) animLayer5th.SetTintColor(layerTintColor);
        if (animLayer6th != null) animLayer6th.SetTintColor(layerTintColor);
    }

    public void SetAllLayersInvisible()
    {
        if (animLayer1st != null) animLayer1st.SetVisibility(false);
        if (animLayer2nd != null) animLayer2nd.SetVisibility(false);
        if (animLayer3rd != null) animLayer3rd.SetVisibility(false);
        if (animLayer4th != null) animLayer4th.SetVisibility(false);
        if (animLayer5th != null) animLayer5th.SetVisibility(false);
        if (animLayer6th != null) animLayer6th.SetVisibility(false);
    }

    public void StopAllAnimations()
    {
        if (animLayer1st != null) animLayer1st.StopAnimation();
        if (animLayer2nd != null) animLayer2nd.StopAnimation();
        if (animLayer3rd != null) animLayer3rd.StopAnimation();
        if (animLayer4th != null) animLayer4th.StopAnimation();
        if (animLayer5th != null) animLayer5th.StopAnimation();
        if (animLayer6th != null) animLayer6th.StopAnimation();
    }

    public void ResetAllAnimations()
    {
        animationStarted = false;
        animationCompleted = false;
        currentAnimStage = 0;
        SetAllLayersInvisible();
    }

    public void RestartVisibleAnimations()
    {
        if (animLayer1st != null && animLayer1st.IsVisible()) animLayer1st.SafeRestartAnimation();
        if (animLayer2nd != null && animLayer2nd.IsVisible()) animLayer2nd.SafeRestartAnimation();
        if (animLayer3rd != null && animLayer3rd.IsVisible()) animLayer3rd.SafeRestartAnimation();
        if (animLayer4th != null && animLayer4th.IsVisible()) animLayer4th.SafeRestartAnimation();
        if (animLayer5th != null && animLayer5th.IsVisible()) animLayer5th.SafeRestartAnimation();
        if (animLayer6th != null && animLayer6th.IsVisible()) animLayer6th.SafeRestartAnimation();
    }

    public void UpdateFrameDelays(float frameDelay)
    {
        if (animLayer1st != null) animLayer1st.SetFrameDelay(frameDelay);
        if (animLayer2nd != null) animLayer2nd.SetFrameDelay(frameDelay);
        if (animLayer3rd != null) animLayer3rd.SetFrameDelay(frameDelay);
        if (animLayer4th != null) animLayer4th.SetFrameDelay(frameDelay);
        if (animLayer5th != null) animLayer5th.SetFrameDelay(frameDelay);
        if (animLayer6th != null) animLayer6th.SetFrameDelay(frameDelay);
    }

    public void HandleTempoTick(MissileNoteType noteType)
    {
        if (animationCompleted) return;

        if (!animationStarted)
        {
            StartAnimation(noteType);
            return;
        }

        EnsureLayersActive();
        AdvanceAnimationStage(noteType);
    }

    private void EnsureLayersActive()
    {
        if (animLayer1st != null && animLayer1st.gameObject != null) animLayer1st.gameObject.SetActive(true);
        if (animLayer2nd != null && animLayer2nd.gameObject != null) animLayer2nd.gameObject.SetActive(true);
        if (animLayer3rd != null && animLayer3rd.gameObject != null) animLayer3rd.gameObject.SetActive(true);
        if (animLayer4th != null && animLayer4th.gameObject != null) animLayer4th.gameObject.SetActive(true);
        if (animLayer5th != null && animLayer5th.gameObject != null) animLayer5th.gameObject.SetActive(true);
        if (animLayer6th != null && animLayer6th.gameObject != null) animLayer6th.gameObject.SetActive(true);
    }

    private void StartAnimation(MissileNoteType noteType)
    {
        animationStarted = true;
        currentAnimStage = 0;
        SetAllLayersInvisible();
        EnsureLayersActive();

        // tint色を適用してからアニメーション開始
        ApplyTintToAllLayers();

        PlayCurrentAnimationStage(noteType);
    }

    private void AdvanceAnimationStage(MissileNoteType noteType)
    {
        currentAnimStage++;

        MissileAnimationType[] sequence = MissileAnimationSequences.GetSequenceForNoteType(noteType);

        if (sequence.Length == 0 || currentAnimStage >= sequence.Length)
        {
            CompleteAnimation();
            return;
        }

        PlayCurrentAnimationStage(noteType);
    }

    private void SafePlayAnimation(MissileUIAnimationLayer layer, MissileAnimationType type)
    {
        if (layer == null) return;

        if (layer.gameObject == null)
        {
            Debug.LogWarning($"アニメーションレイヤーのゲームオブジェクトがnullです: {type}");
            return;
        }

        layer.gameObject.SetActive(true);
        layer.ChangeSprites(parent.GetSpritesForType(type));
        layer.SetTintColor(layerTintColor); // tint色を適用
        layer.SetVisibility(true);

        try
        {
            layer.SafeRestartAnimation();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"アニメーション開始エラー: {e.Message}");
        }
    }

    private void PlayCurrentAnimationStage(MissileNoteType noteType)
    {
        SetAllLayersInvisible();

        if (noteType == MissileNoteType.None) return;

        MissileAnimationType[] sequence = MissileAnimationSequences.GetSequenceForNoteType(noteType);

        if (sequence.Length == 0 || currentAnimStage >= sequence.Length) return;

        MissileAnimationType currentType = sequence[currentAnimStage];

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
        }
    }

    private void CompleteAnimation()
    {
        animationCompleted = true;
        SetAllLayersInvisible();
    }

    public void TriggerHitAnimation()
    {
    }

    public bool IsAnimationCompleted() => animationCompleted;
    public int GetCurrentAnimStage() => currentAnimStage;
    public bool IsAnimationStarted() => animationStarted;
}