using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 複数のアニメーションレイヤーを持つマウスストーカーを管理するメインクラス
/// </summary>
public class NotesRingManagerXylo : MonoBehaviour
{
    [Header("表示設定")]
    [Tooltip("画像の大きさ")]
    public Vector2 imageSize = new Vector2(100, 100);

    [Header("透過度設定")]
    [Range(0, 1)]
    [Tooltip("第一レイヤーの透過度 (0 = 完全透明, 1 = 不透明)")]
    public float layer1Alpha = 0.6f; // 40%不透明（60%透明）

    [Range(0, 1)]
    [Tooltip("第二レイヤーの透過度 (0 = 完全透明, 1 = 不透明)")]
    public float layer2Alpha = 0.3f; // 30%不透明（70%透明）

    [Header("アニメーションパターン")]
    [Tooltip("第一レイヤーのアニメーションパターン (0=なし, 1=Short1st, 2=Long1st)")]
    public int[] layer1Pattern = new int[] { 0, 1, 2, 0, 1, 2, 0, 1, 2 };

    [Tooltip("現在のパターンインデックス")]
    [SerializeField]
    private int currentPatternIndex = 0;

    [Header("アニメーションセット")]
    [Tooltip("Long1stアニメーション用スプライト")]
    public Sprite[] longFirstSprites;

    [Tooltip("Long2ndアニメーション用スプライト")]
    public Sprite[] longSecondSprites;

    [Tooltip("Long3rdアニメーション用スプライト")]
    public Sprite[] longThirdSprites;

    [Tooltip("Long4thアニメーション用スプライト")]
    public Sprite[] longFourthSprites;

    [Tooltip("Short1stアニメーション用スプライト")]
    public Sprite[] shortFirstSprites;

    [Tooltip("Short2ndアニメーション用スプライト")]
    public Sprite[] shortSecondSprites;

    [Tooltip("Short3rdアニメーション用スプライト")]
    public Sprite[] shortThirdSprites;

    [Tooltip("Hitアニメーション用スプライト")]
    public Sprite[] hitSprites;

    [Header("フレーム設定")]
    [Tooltip("通常アニメーションのフレーム間の時間（秒）")]
    public float frameDelay = 0.1f;

    [Tooltip("Hitアニメーションのフレーム間の時間（秒）固定値")]
    public float hitFrameDelay = 0.07f;

    // レイヤー管理用変数
    private RectTransform rectTransform;
    private UIAnimationLayer animLayer1st;
    private UIAnimationLayer animLayer2nd;
    private UIAnimationLayer animLayer3rd;
    private UIAnimationLayer animLayer4th;
    private UIHitAnimationLayer hitLayer;

    // 現在のアニメーションタイプを追跡する変数
    private AnimationType currentType1st = AnimationType.None;
    private AnimationType prevType1st = AnimationType.None;
    private AnimationType currentType2nd = AnimationType.None;
    private AnimationType prevType2nd = AnimationType.None;
    private AnimationType currentType3rd = AnimationType.None;
    private AnimationType prevType3rd = AnimationType.None;
    private AnimationType currentType4th = AnimationType.None;

    // 初期ビートフラグ
    private bool isFirstBeat = true;
    private bool isSecondBeat = false;
    private bool isThirdBeat = false;

    // 前回のLong4thの状態を保存する変数を追加
    private AnimationType prevType4th = AnimationType.None;
    // テンポ情報
    private float oneBeat = 0.4f;
    private bool isInitialized = false;

    // アニメーションタイプの列挙型
    public enum AnimationType
    {
        None,       // アニメーションなし（非表示）
        Long1st,
        Long2nd,
        Long3rd,
        Long4th,
        Short1st,
        Short2nd,
        Short3rd,
        Hit
    }

    private void Awake()
    {
        // RectTransformを取得
        rectTransform = GetComponent<RectTransform>();

        // パターン配列のチェック
        if (layer1Pattern == null || layer1Pattern.Length == 0)
        {
            layer1Pattern = new int[] { 1 }; // デフォルト値を設定
            Debug.LogWarning("NotesRingManagerXylo: パターン配列が空です。デフォルト値を設定しました。");
        }

        // 各レイヤーを作成
        CreateAnimationLayers();
    }

    private void Start()
    {
        // 初期状態では全レイヤー非表示
        SetAllLayersInvisible();
    }

    /// <summary>
    /// 全レイヤーを非表示に設定
    /// </summary>
    private void SetAllLayersInvisible()
    {
        animLayer1st.SetVisibility(false);
        animLayer2nd.SetVisibility(false);
        animLayer3rd.SetVisibility(false);
        animLayer4th.SetVisibility(false);
        hitLayer.SetVisibility(false);
    }

    /// <summary>
    /// レイヤーの透過度を更新
    /// </summary>
    private void UpdateLayerTransparencies()
    {
        animLayer1st.SetAlpha(layer1Alpha);
        animLayer2nd.SetAlpha(layer2Alpha);
        // 他のレイヤーは透過度を変更しない（完全不透明）
    }

    /// <summary>
    /// パターン配列から第一レイヤーのアニメーションタイプを更新
    /// </summary>
    private void UpdateFirstLayerFromPattern()
    {
        // パターン配列のチェック
        if (layer1Pattern == null || layer1Pattern.Length == 0)
        {
            Debug.LogWarning("NotesRingManagerXylo: パターン配列が空です。デフォルト値を使用します。");
            currentType1st = AnimationType.Short1st;
            ChangeAnimationType(1, currentType1st);
            return;
        }

        // インデックスが範囲外の場合は0に戻す
        if (currentPatternIndex >= layer1Pattern.Length)
        {
            currentPatternIndex = 0;
        }

        // 現在のパターン値を取得
        int patternValue = layer1Pattern[currentPatternIndex];

        // 前回の値を保存
        prevType1st = currentType1st;

        // パターン値に基づいてアニメーションタイプを決定
        switch (patternValue)
        {
            case 0:
                currentType1st = AnimationType.None;
                break;
            case 1:
                currentType1st = AnimationType.Short1st;
                break;
            case 2:
                currentType1st = AnimationType.Long1st;
                break;
            default:
                currentType1st = AnimationType.Short1st; // デフォルト
                break;
        }

        // 第一レイヤーのアニメーションタイプを変更
        ChangeAnimationType(1, currentType1st);

        // パターンインデックスを次に進める
        currentPatternIndex = (currentPatternIndex + 1) % layer1Pattern.Length;
    }

    /// <summary>
    /// カスケードアニメーションを更新
    /// </summary>
    private void UpdateCascadeAnimation()
    {
        if (isFirstBeat)
        {
            // 初回のビートでは第一レイヤーのみ設定
            isFirstBeat = false;
            isSecondBeat = true;
            return;
        }

        if (isSecondBeat)
        {
            // 2回目のビートでは第一レイヤーと第二レイヤーを設定
            // 第二レイヤーは前回の第一レイヤーのタイプに基づく
            UpdateSecondLayer();
            isSecondBeat = false;
            isThirdBeat = true;
            return;
        }

        if (isThirdBeat)
        {
            // 3回目のビートでは第一、第二、第三レイヤーを設定
            // 第二レイヤーは前回の第一レイヤーに基づく
            // 第三レイヤーは前回の第二レイヤーに基づく
            UpdateSecondLayer();
            UpdateThirdLayer();
            isThirdBeat = false;
            return;
        }

        // 4回目以降のビートでは全レイヤーを更新
        // 各レイヤーは前回の一つ前のレイヤーのアニメーションに基づく
        UpdateSecondLayer();
        UpdateThirdLayer();
        UpdateFourthLayer();
        UpdateHitLayer();
    }

    /// <summary>
    /// 第二レイヤーを更新（前回の第一レイヤーに基づく）
    /// </summary>
    private void UpdateSecondLayer()
    {
        // 前回の値を保存
        prevType2nd = currentType2nd;

        // 前回の第一レイヤーに基づいて第二レイヤーを設定
        if (prevType1st == AnimationType.Short1st)
        {
            currentType2nd = AnimationType.Short2nd;
        }
        else if (prevType1st == AnimationType.Long1st)
        {
            currentType2nd = AnimationType.Long2nd;
        }
        else
        {
            currentType2nd = AnimationType.None;
        }

        ChangeAnimationType(2, currentType2nd);
    }

    /// <summary>
    /// 第三レイヤーを更新（前回の第二レイヤーに基づく）
    /// </summary>
    private void UpdateThirdLayer()
    {
        // 前回の値を保存
        prevType3rd = currentType3rd;

        // 前回の第二レイヤーに基づいて第三レイヤーを設定
        if (prevType2nd == AnimationType.Short2nd)
        {
            currentType3rd = AnimationType.Short3rd;
        }
        else if (prevType2nd == AnimationType.Long2nd)
        {
            currentType3rd = AnimationType.Long3rd;
        }
        else
        {
            currentType3rd = AnimationType.None;
        }

        ChangeAnimationType(3, currentType3rd);
    }

    /// <summary>
    /// 第四レイヤーを更新（前回の第三レイヤーに基づく）
    /// </summary>
    private void UpdateFourthLayer()
    {
        // 前回の第三レイヤーに基づいて第四レイヤーを設定
        if (prevType3rd == AnimationType.Long3rd)
        {
            currentType4th = AnimationType.Long4th;
        }
        else
        {
            // Short3rdまたはNoneの場合は何も表示しない
            currentType4th = AnimationType.None;
        }

        ChangeAnimationType(4, currentType4th);
    }

    /// <summary>
    /// Hitレイヤーを更新
    /// </summary>
    // UpdateHitLayerメソッドを修正
    private void UpdateHitLayer()
    {
        // 直前の第三レイヤーがShort3rdの場合または前回のビートでLong4thが表示されていた場合にのみHitを表示
        bool shouldShowHit = (prevType3rd == AnimationType.Short3rd) || (prevType4th == AnimationType.Long4th);
        hitLayer.SetVisibility(shouldShowHit);

        if (shouldShowHit)
        {
            hitLayer.RestartAnimation();
        }

        // 現在の第四レイヤーの状態を保存（次のビートで使用）
        prevType4th = currentType4th;
    }

    /// <summary>
    /// アニメーションレイヤーを作成して初期化する
    /// </summary>
    private void CreateAnimationLayers()
    {
        // 1stレイヤー
        GameObject layer1st = CreateLayer("AnimLayer1st");
        animLayer1st = layer1st.AddComponent<UIAnimationLayer>();
        animLayer1st.Initialize(shortFirstSprites, frameDelay);
        animLayer1st.SetVisibility(false); // 初期状態では非表示

        // 2ndレイヤー
        GameObject layer2nd = CreateLayer("AnimLayer2nd");
        animLayer2nd = layer2nd.AddComponent<UIAnimationLayer>();
        animLayer2nd.Initialize(shortSecondSprites, frameDelay);
        animLayer2nd.SetVisibility(false); // 初期状態では非表示

        // 3rdレイヤー
        GameObject layer3rd = CreateLayer("AnimLayer3rd");
        animLayer3rd = layer3rd.AddComponent<UIAnimationLayer>();
        animLayer3rd.Initialize(shortThirdSprites, frameDelay);
        animLayer3rd.SetVisibility(false); // 初期状態では非表示

        // 4thレイヤー
        GameObject layer4th = CreateLayer("AnimLayer4th");
        animLayer4th = layer4th.AddComponent<UIAnimationLayer>();
        animLayer4th.Initialize(longFourthSprites, frameDelay);
        animLayer4th.SetVisibility(false); // 初期状態では非表示

        // Hitレイヤー（特殊なHitアニメーション用のレイヤー）
        GameObject layerHit = CreateLayer("HitLayer");
        hitLayer = layerHit.AddComponent<UIHitAnimationLayer>();
        hitLayer.Initialize(hitSprites, hitFrameDelay);
        hitLayer.SetVisibility(false); // 初期状態は非表示
    }

    /// <summary>
    /// レイヤー用のGameObjectを作成
    /// </summary>
    private GameObject CreateLayer(string name)
    {
        GameObject layerObj = new GameObject(name);
        layerObj.transform.SetParent(transform, false);

        // Image コンポーネントを追加
        Image image = layerObj.AddComponent<Image>();

        // アルファブレンドを有効にする設定
        image.material = new Material(Shader.Find("UI/Default"));
        image.preserveAspect = true;

        // 透過を可能にするためのCanvasGroupを追加
        CanvasGroup canvasGroup = layerObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // RectTransformの設定
        RectTransform layerRect = layerObj.GetComponent<RectTransform>();
        layerRect.anchorMin = Vector2.zero;
        layerRect.anchorMax = Vector2.one;
        layerRect.offsetMin = Vector2.zero;
        layerRect.offsetMax = Vector2.zero;

        return layerObj;
    }

    private void OnEnable()
    {
        // イベントリスナーの登録
        CRIWARE_conductor.TempoMethodEvent1 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent2 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent3 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent4 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent5 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent6 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent7 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent8 += TempoMethod;

        // CRIWAREからテンポ情報を取得
        if (CRIWARE_conductor.Instance != null && CRIWARE_conductor.Instance.BasicBeat > 0)
        {
            oneBeat = CRIWARE_conductor.Instance.BasicBeat;
            frameDelay = oneBeat / 8;
            isInitialized = true;

            // 各レイヤーにフレーム間の時間を設定（ヒットレイヤーは固定値なので更新しない）
            UpdateNormalLayersFrameDelay();

            Debug.Log("NotesRingManagerXylo: テンポ情報を設定しました - OneBeat: " + oneBeat + ", frameDelay: " + frameDelay);
        }

        Debug.Log("NotesRingManagerXylo: イベントリスナーを登録しました");
    }

    private void OnDisable()
    {
        // イベントリスナーの解除
        CRIWARE_conductor.TempoMethodEvent1 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent2 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent3 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent4 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent5 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent6 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent7 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent8 -= TempoMethod;

        // アニメーションを停止
        StopAllAnimations();

        Debug.Log("NotesRingManagerXylo: イベントリスナーを解除しました");
    }

    void Update()
    {
        // マウス位置を取得
        Vector2 mousePosition = Input.mousePosition;

        // オブジェクトの位置を更新
        rectTransform.position = mousePosition;
    }

    /// <summary>
    /// テンポ情報更新時に呼ばれるメソッド
    /// </summary>
    private void TempoMethod()
    {
        if (!isInitialized)
        {
            // テンポ情報が初期化されていない場合、CRIWAREから取得を試みる
            if (CRIWARE_conductor.Instance != null && CRIWARE_conductor.Instance.BasicBeat > 0)
            {
                oneBeat = CRIWARE_conductor.Instance.BasicBeat;
                frameDelay = oneBeat / 8;
                isInitialized = true;

                // 各レイヤーにフレーム間の時間を設定（ヒットレイヤーは固定値なので更新しない）
                UpdateNormalLayersFrameDelay();

                Debug.Log("NotesRingManagerXylo: テンポ情報を設定しました - OneBeat: " + oneBeat + ", frameDelay: " + frameDelay);
            }
            else
            {
                Debug.Log("NotesRingManagerXylo: テンポ情報が初期化されていないため、アニメーションを開始できません");
                return;
            }
        }

        Debug.Log("NotesRingManagerXylo: テンポイベントを受信しました - アニメーションを再起動します");

        // パターンに基づいて第一レイヤーのアニメーションを更新
        UpdateFirstLayerFromPattern();

        // カスケードアニメーションを更新
        UpdateCascadeAnimation();

        // レイヤーの透過度を更新
        UpdateLayerTransparencies();

        // 各レイヤーのアニメーションを再スタート
        RestartVisibleAnimations();

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

        // Hitレイヤーは可視状態の場合のみ再スタート
        if (hitLayer.IsVisible())
            hitLayer.RestartAnimation();
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
        hitLayer.StopAnimation();
    }

    /// <summary>
    /// 通常レイヤーのフレーム間の時間を更新（ヒットレイヤーは固定値なので更新しない）
    /// </summary>
    private void UpdateNormalLayersFrameDelay()
    {
        animLayer1st.SetFrameDelay(frameDelay);
        animLayer2nd.SetFrameDelay(frameDelay);
        animLayer3rd.SetFrameDelay(frameDelay);
        animLayer4th.SetFrameDelay(frameDelay);
        // hitLayerは固定値なので更新しない
    }

    /// <summary>
    /// 指定したレイヤーのアニメーションタイプを変更
    /// </summary>
    /// <param name="layer">変更するレイヤー (1-4, 5=hit)</param>
    /// <param name="type">設定するアニメーションタイプ</param>
    public void ChangeAnimationType(int layer, AnimationType type)
    {
        Sprite[] sprites = GetSpritesForType(type);
        bool isVisible = (type != AnimationType.None);

        switch (layer)
        {
            case 1:
                animLayer1st.ChangeSprites(sprites);
                animLayer1st.SetVisibility(isVisible);
                break;
            case 2:
                animLayer2nd.ChangeSprites(sprites);
                animLayer2nd.SetVisibility(isVisible);
                break;
            case 3:
                animLayer3rd.ChangeSprites(sprites);
                animLayer3rd.SetVisibility(isVisible);
                break;
            case 4:
                animLayer4th.ChangeSprites(sprites);
                animLayer4th.SetVisibility(isVisible);
                break;
            case 5:
                hitLayer.ChangeSprites(sprites);
                break;
        }
    }

    /// <summary>
    /// アニメーションタイプに対応するスプライト配列を取得
    /// </summary>
    private Sprite[] GetSpritesForType(AnimationType type)
    {
        Sprite[] result = null;

        switch (type)
        {
            case AnimationType.Long1st:
                result = longFirstSprites;
                break;
            case AnimationType.Long2nd:
                result = longSecondSprites;
                break;
            case AnimationType.Long3rd:
                result = longThirdSprites;
                break;
            case AnimationType.Long4th:
                result = longFourthSprites;
                break;
            case AnimationType.Short1st:
                result = shortFirstSprites;
                break;
            case AnimationType.Short2nd:
                result = shortSecondSprites;
                break;
            case AnimationType.Short3rd:
                result = shortThirdSprites;
                break;
            case AnimationType.Hit:
                result = hitSprites;
                break;
            case AnimationType.None:
            default:
                // 重要な修正: AnimationType.Noneの場合でも空の配列を返す（nullではない）
                return new Sprite[0];
        }

        // 配列がnullの場合は空の配列を返す
        if (result == null)
        {
            Debug.LogWarning($"GetSpritesForType: {type}のスプライト配列がnullです。空の配列を使用します。");
            return new Sprite[0];
        }

        return result;
    }

    /// <summary>
    /// Hitアニメーションを手動で開始
    /// </summary>
    public void TriggerHitAnimation()
    {
        hitLayer.SetVisibility(true);
        hitLayer.RestartAnimation();
    }
}

/// <summary>
/// 個別のアニメーションレイヤーを管理する基本クラス
/// </summary>
public class UIAnimationLayer : MonoBehaviour
{
    protected Image image;
    protected CanvasGroup canvasGroup;
    protected Sprite[] sprites;
    protected float frameDelay;
    protected Coroutine animationCoroutine;

    /// <summary>
    /// レイヤーを初期化
    /// </summary>
    public virtual void Initialize(Sprite[] initialSprites, float initialFrameDelay)
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("Initialize: Imageコンポーネントが見つかりません");
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("Initialize: CanvasGroupコンポーネントが見つかりません");
        }

        sprites = initialSprites;
        frameDelay = initialFrameDelay;

        if (sprites != null && sprites.Length > 0)
        {
            image.sprite = sprites[0];
            image.enabled = true;
        }
        else
        {
            Debug.LogWarning("Initialize: スプライト配列が無効です");
            image.enabled = false;
        }
    }

    /// <summary>
    /// レイヤーの透明度を設定
    /// </summary>
    public virtual void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    /// <summary>
    /// レイヤーの可視性を設定
    /// </summary>
    public virtual void SetVisibility(bool visible)
    {
        if (image != null)
        {
            image.enabled = visible;
        }
    }

    /// <summary>
    /// レイヤーの可視性を取得
    /// </summary>
    public virtual bool IsVisible()
    {
        return image != null && image.enabled;
    }

    /// <summary>
    /// フレーム間の時間を設定
    /// </summary>
    public virtual void SetFrameDelay(float delay)
    {
        frameDelay = delay;
    }

    /// <summary>
    /// スプライトセットを変更
    /// </summary>
    public virtual void ChangeSprites(Sprite[] newSprites)
    {
        // 新しいスプライト配列がnullの場合は早期リターン
        if (newSprites == null)
        {
            Debug.LogWarning("ChangeSprites: 新しいスプライト配列がnullです");
            sprites = null;
            return;
        }

        sprites = newSprites;

        if (image != null && sprites.Length > 0 && sprites[0] != null)
        {
            image.sprite = sprites[0];
        }

    }

    /// <summary>
    /// アニメーションを再スタート
    /// </summary>
    public virtual void RestartAnimation()
    {
        StopAnimation();
        StartAnimation();
    }

    /// <summary>
    /// アニメーションを開始
    /// </summary>
    public virtual void StartAnimation()
    {
        // スプライトがない、または非表示になっている場合は何もしない
        if (sprites == null || sprites.Length == 0 || image == null || !image.enabled)
        {
            Debug.LogWarning("StartAnimation: sprites または image が無効です");
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateSprites());
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
    }

    /// <summary>
    /// スプライトアニメーションのコルーチン
    /// </summary>
    protected virtual IEnumerator AnimateSprites()
    {
        int currentIndex = 0;

        if (sprites == null || sprites.Length == 0 || image == null)
        {
            Debug.LogWarning("AnimateSprites: sprites または image が null です");
            yield break;
        }

        while (true)
        {
            // ループの各イテレーションでnullチェックを行う
            if (image == null || sprites == null || currentIndex >= sprites.Length || sprites[currentIndex] == null)
            {
                Debug.LogWarning($"AnimateSprites: null参照が検出されました（index: {currentIndex}）");
                break;
            }

            image.sprite = sprites[currentIndex];

            yield return new WaitForSeconds(frameDelay);

            currentIndex++;

            if (currentIndex >= sprites.Length)
            {
                break;
            }
        }

        animationCoroutine = null;
    }
}

/// <summary>
/// ヒットアニメーション専用のレイヤークラス
/// </summary>
public class UIHitAnimationLayer : UIAnimationLayer
{
    /// <summary>
    /// ヒットアニメーション用のスプライトアニメーションコルーチン（速度固定、再生後非表示）
    /// </summary>
    protected override IEnumerator AnimateSprites()
    {
        int currentIndex = 0;

        if (sprites == null || sprites.Length == 0 || image == null)
        {
            Debug.LogWarning("HitAnimateSprites: sprites または image が null です");
            yield break;
        }

        // アニメーション開始時に表示
        if (image != null)
        {
            image.enabled = true;
        }

        while (currentIndex < sprites.Length)  // 条件を明示的に制限
        {
            // ループの各イテレーションでnullチェックを行う
            if (image == null || sprites == null || currentIndex >= sprites.Length || sprites[currentIndex] == null)
            {
                Debug.LogWarning($"HitAnimateSprites: null参照が検出されました（index: {currentIndex}）");
                break;
            }

            image.sprite = sprites[currentIndex];

            yield return new WaitForSeconds(frameDelay);

            currentIndex++;
        }

        // アニメーション終了時に非表示にする
        if (image != null)
        {
            image.enabled = false;
        }

        animationCoroutine = null;
    }
}