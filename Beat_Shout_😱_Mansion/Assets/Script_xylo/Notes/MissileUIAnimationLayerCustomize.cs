using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIアニメーションレイヤーの基本クラスカスタマイズ
/// </summary>
public class MissileUIAnimationLayerCustomize : MonoBehaviour
{
    /// <summary>UIアニメーションレイヤーの基本クラスのカスタマイズテーブル</summary>
    [SerializeField] private MissileUIAnimationLayerCustomizeTable missileUIAnimationLayerCustomizeTable;
    /// <summary>UIアニメーションレイヤーの基本クラスカスタマイズのプレビュー</summary>
    [SerializeField] private MissileUIAnimationLayerCustomizePreview missileUIAnimationLayerCustomizePreview;

    private Image _image;
    private Material _ringMaterial;
    private MissileAnimationType _missileAnimationType;

    private float _frameDelay;
    private int _currentFrame = 0;
    private bool isPlaying = false;
    private Coroutine animationCoroutine;

    // tint色（デフォルトは白＝色変化なし）
    private Color tintColor = Color.white;

    private void Start()
    {
        var preview = missileUIAnimationLayerCustomizePreview;
        if (preview != null && preview.isPreviewEnabled)
        {
            Initialize();
            ChangeSprites(preview.missileAnimationType);
        }
    }

    private void FixedUpdate()
    {
        var preview = missileUIAnimationLayerCustomizePreview;
        if (preview != null && preview.isPreviewEnabled)
        {
            var table = missileUIAnimationLayerCustomizeTable;
            var type = table.GetNotesType(preview.missileAnimationType);
            switch (type)
            {
                case 1:
                    _ringMaterial.SetFloat("_RingThickness", preview.ringThickness);
                    _ringMaterial.SetFloat("_RingRadius", preview.ringRadius);
                    _ringMaterial.SetColor("_FillColor", preview.fillColor);
                    _ringMaterial.SetFloat("_FillRadius", preview.fillRadius);
                    _ringMaterial.SetFloat("_FillMaskRadius", preview.fillMaskRadius);
                    _ringMaterial.SetFloat("_Ring_1Thickness", preview.ring_1Thickness);
                    _ringMaterial.SetFloat("_Ring_1Radius", preview.ring_1Radius);
                    _ringMaterial.SetFloat("_FillAmount", preview.fillAmount);

                    break;
                default:
                    _ringMaterial.SetFloat("_RingThickness", preview.ringThickness);
                    _ringMaterial.SetFloat("_RingRadius", preview.ringRadius);

                    break;
            }
            _image.material = _ringMaterial;
        }
    }

    private void OnDestroy()
    {
        var table = missileUIAnimationLayerCustomizeTable;
        table.Dispose();
    }

    /// <summary>
    /// UIアニメーションレイヤーの基本クラスのカスタマイズテーブルをセット
    /// </summary>
    /// <param name="missileUIAnimationLayerCustomizeTable">UIアニメーションレイヤーの基本クラスのカスタマイズテーブル</param>
    /// <remarks>初回ロードの場合は<see cref="missileUIAnimationLayerCustomizeTable"/>は読み込めるが<br/>
    /// 2回目以降のロードの場合はNULLになるため呼び出し元からセットする</remarks>
    public void SetMissileUIAnimationLayerCustomizeTable(MissileUIAnimationLayerCustomizeTable missileUIAnimationLayerCustomizeTable)
    {
        this.missileUIAnimationLayerCustomizeTable = missileUIAnimationLayerCustomizeTable;
    }

    public void Initialize()
    {
        var table = missileUIAnimationLayerCustomizeTable;
        _image = GetComponent<Image>();
        table.Initialize();
    }

    /// <summary>
    /// tint色を設定（alphaはCanvasGroupで管理するのでRGBのみ反映）
    /// </summary>
    public void SetTintColor(Color color)
    {
        tintColor = color;
    }

    public void ChangeSprites(MissileAnimationType missileAnimationType)
    {
        var table = missileUIAnimationLayerCustomizeTable;
        _missileAnimationType = missileAnimationType;
        int notesType = table.GetNotesType(missileAnimationType);
        switch (notesType)
        {
            case 1:
                _ringMaterial = new Material(table.ringMaterialLong);
                break;
            default:
                _ringMaterial = new Material(table.ringMaterialShort);
                break;
        }
        _image.material = _ringMaterial;
    }

    public void SetVisibility(bool visible)
    {
        _image.enabled = visible;
        if (!visible && animationCoroutine != null)
        {
            StopAnimation();
        }
        else if (visible && animationCoroutine == null && isPlaying)
        {
            SafeRestartAnimation();
        }
    }

    public void SetFrameDelay(float delay)
    {
        _frameDelay = delay;
    }

    public void SafeRestartAnimation()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"{gameObject.name}が非アクティブなためアニメーションコルーチンを開始できません");
            isPlaying = true;
            return;
        }

        RestartAnimation();
    }

    private void RestartAnimation()
    {
        StopAnimation();

        if (_image == null) return;

        var table = missileUIAnimationLayerCustomizeTable;
        _currentFrame = 0;
        var missileAnimationType = _missileAnimationType;
        int notesType = table.GetNotesType(missileAnimationType);
        switch (notesType)
        {
            case 1:
                var longRange = table.GetRingRadiusLongRange(missileAnimationType);

                if (longRange == null)
                    return;

                _ringMaterial.SetFloat("_RingRadius", longRange.ringRadiusMax);
                _ringMaterial.SetFloat("_FillRadius", longRange.fillRadiusMax);
                _ringMaterial.SetFloat("_FillMaskRadius", longRange.fillMaskRadiusMax);
                _ringMaterial.SetFloat("_Ring_1Radius", longRange.ring_1RadiusMax);
                _ringMaterial.SetFloat("_FillAmount", longRange.fillAmountMax);

                // tint色を再適用
                Color applied = new Color(tintColor.r, tintColor.g, tintColor.b, _image.color.a);
                _ringMaterial.SetColor("_FillColor", applied);

                break;
            default:
                var shortRange = table.GetRingRadiusShortRange(missileAnimationType);

                if (shortRange == null)
                    return;

                _ringMaterial.SetFloat("_RingRadius", shortRange.ringRadiusMax);

                break;
        }

        if (gameObject.activeInHierarchy)
        {
            isPlaying = true;
            switch (notesType)
            {
                case 1:
                    animationCoroutine = StartCoroutine(AnimationLongCoroutine());

                    break;
                default:
                    animationCoroutine = StartCoroutine(AnimationShortCoroutine());

                    break;
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}が非アクティブなためアニメーションコルーチンを開始できません");
            isPlaying = true;
        }
    }

    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        isPlaying = false;
    }

    private IEnumerator AnimationLongCoroutine()
    {
        isPlaying = true;

        var table = missileUIAnimationLayerCustomizeTable;
        var frameDelay = _frameDelay;
        float ringRadius = 0f;
        float fillRadius = 0f;
        float fillMaskRadius = 0f;
        float ring_1Radius = 0f;
        float fillAmount = 0f;

        var radiusRange = table.GetRingRadiusLongRange(_missileAnimationType);

        if (radiusRange == null)
        {
            yield break;
        }

        while (isPlaying)
        {
            yield return new WaitForSeconds(frameDelay);

            _currentFrame++;

            if (this == null || !gameObject.activeInHierarchy)
            {
                isPlaying = false;
                yield break;
            }

            var currentFrame = _currentFrame;
            ringRadius = radiusRange.ringRadiusMax - radiusRange.RingRadiusPiece * currentFrame;
            fillRadius = radiusRange.fillRadiusMax - radiusRange.FillRadiusPiece * currentFrame;
            fillMaskRadius = radiusRange.fillMaskRadiusMax - radiusRange.FillMaskRadiusPiece * currentFrame;
            ring_1Radius = radiusRange.ring_1RadiusMax - radiusRange.Ring_1RadiusPiece * currentFrame;
            fillAmount = radiusRange.fillAmountMax - radiusRange.FillAmountPiece * currentFrame;

            _ringMaterial.SetFloat("_RingRadius", ringRadius);
            _ringMaterial.SetFloat("_FillRadius", fillRadius);
            _ringMaterial.SetFloat("_FillMaskRadius", fillMaskRadius);
            _ringMaterial.SetFloat("_Ring_1Radius", ring_1Radius);
            _ringMaterial.SetFloat("_FillAmount", fillAmount);
        }
    }

    private IEnumerator AnimationShortCoroutine()
    {
        isPlaying = true;

        var table = missileUIAnimationLayerCustomizeTable;
        var frameDelay = _frameDelay;
        float ringRadius = 0f;

        var radiusRange = table.GetRingRadiusShortRange(_missileAnimationType);

        if (radiusRange == null)
        {
            yield break;
        }

        while (isPlaying)
        {
            yield return new WaitForSeconds(frameDelay);

            _currentFrame++;

            if (this == null || !gameObject.activeInHierarchy)
            {
                isPlaying = false;
                yield break;
            }

            var currentFrame = _currentFrame;
            ringRadius = radiusRange.ringRadiusMax - radiusRange.RingRadiusPiece * currentFrame;
            _ringMaterial.SetFloat("_RingRadius", ringRadius);
        }
    }
}

/// <summary>
/// UIアニメーションレイヤーの基本クラスカスタマイズのプレビュー
/// </summary>
[System.Serializable]
public class MissileUIAnimationLayerCustomizePreview
{
    public bool isPreviewEnabled;
    public MissileAnimationType missileAnimationType;
    public float ringThickness = 0.04f;
    public float ringRadius = 0.345f;
    public float fillRadius = 0.495f;
    public float fillMaskRadius = 0.33f;
    public Color fillColor = Color.white;
    public float ring_1Thickness = 0.04f;
    public float ring_1Radius = 0.495f;
    [Range(0f, 1f)] public float fillAmount = 1.0f;
}
