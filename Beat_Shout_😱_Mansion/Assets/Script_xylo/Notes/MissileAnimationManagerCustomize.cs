using UnityEngine;

/// <summary>
/// アニメーションの管理を行うクラスカスタマイズ
/// </summary>
public class MissileAnimationManagerCustomize
{
    private MissileDirectAnimManagerB parent;
    private MissileUIManager uiManager;

    private MissileUIAnimationLayerCustomize animLayer1st;

    private bool animationStarted = false;
    private bool animationCompleted = false;
    private int currentAnimStage = 0;

    // 色tint用
    private Color layerTintColor = Color.white;

    private MissileUIAnimationLayerCustomizeTable _missileUIAnimationLayerCustomizeTable;

    public MissileAnimationManagerCustomize(MissileDirectAnimManagerB parent, MissileUIManager uiManager, MissileUIAnimationLayerCustomizeTable missileUIAnimationLayerCustomizeTable)
    {
        this.parent = parent;
        this.uiManager = uiManager;
        _missileUIAnimationLayerCustomizeTable = missileUIAnimationLayerCustomizeTable;
        CreateAnimationLayers();
    }

    private void CreateAnimationLayers()
    {
        /*
         旧ソースでは、アニメーションごとにレイヤーを分けている
        また、アニメーションは、1拍の時間範囲で表現可能な範囲でぶつ切りにしており、
        それを繋げることで1つのアニメーションであるかのように見せている

        改修後は、レイヤーを1つのみにする
         */
        GameObject layer1st = uiManager.CreateLayer("AnimLayer1st");
        layer1st.SetActive(true);
        animLayer1st = layer1st.AddComponent<MissileUIAnimationLayerCustomize>();
        animLayer1st.SetMissileUIAnimationLayerCustomizeTable(_missileUIAnimationLayerCustomizeTable);
        animLayer1st.Initialize();
        animLayer1st.SetVisibility(false);
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
    }

    public void SetAllLayersInvisible()
    {
        if (animLayer1st != null) animLayer1st.SetVisibility(false);
    }

    public void StopAllAnimations()
    {
        if (animLayer1st != null) animLayer1st.StopAnimation();
    }

    public void ResetAllAnimations()
    {
        animationStarted = false;
        animationCompleted = false;
        currentAnimStage = 0;
        SetAllLayersInvisible();
    }

    public void UpdateFrameDelays(float frameDelay)
    {
        if (animLayer1st != null) animLayer1st.SetFrameDelay(frameDelay);
    }

    public void HandleTempoTick(MissileNoteType noteType)
    {
        if (animationCompleted) return;

        EnsureLayersActive();
        if (!animationStarted)
        {
            StartAnimation(noteType);
            return;
        }
        
        AdvanceAnimationStage(noteType);
    }

    private void StartAnimation(MissileNoteType noteType)
    {
        animationStarted = true;
        currentAnimStage = 0;
        SetAllLayersInvisible();

        // tint色を適用してからアニメーション開始
        ApplyTintToAllLayers();

        PlayCurrentAnimationStage(noteType);
    }

    private void EnsureLayersActive()
    {
        if (animLayer1st != null && animLayer1st.gameObject != null) animLayer1st.gameObject.SetActive(true);
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
                SafePlayAnimation(animLayer1st, currentType);
                break;
            case MissileAnimationType.Short3rd:
            case MissileAnimationType.Long3rd:
                SafePlayAnimation(animLayer1st, currentType);
                break;
            case MissileAnimationType.Long4th:
                SafePlayAnimation(animLayer1st, currentType);
                break;
            case MissileAnimationType.Long02_01:
            case MissileAnimationType.Long02_02:
            case MissileAnimationType.Long03_01:
            case MissileAnimationType.Long03_02:
                SafePlayAnimation(animLayer1st, currentType);
                break;
            case MissileAnimationType.Long03_03:
                SafePlayAnimation(animLayer1st, currentType);
                break;
        }
    }

    private void SafePlayAnimation(MissileUIAnimationLayerCustomize layer, MissileAnimationType type)
    {
        if (layer == null) return;

        if (layer.gameObject == null)
        {
            Debug.LogWarning($"アニメーションレイヤーのゲームオブジェクトがnullです: {type}");
            return;
        }

        layer.gameObject.SetActive(true);
        layer.ChangeSprites(type);
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

    private void CompleteAnimation()
    {
        animationCompleted = true;
        SetAllLayersInvisible();
    }

    public bool IsAnimationCompleted() => animationCompleted;
}
