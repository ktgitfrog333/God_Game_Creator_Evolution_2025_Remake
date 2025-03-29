
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ホーミングミサイル自体にアタッチして使用するアニメーションマネージャークラス
/// （追従検索ロジックを排除した簡略版）
/// </summary>
public class MissileDirectAnimManager : MonoBehaviour
{
    [Header("表示設定")]
    [Tooltip("ワールド座標からスクリーン座標へのオフセット")]
    public Vector2 screenOffset = Vector2.zero;

    [Tooltip("UIキャンバスへの参照（自動検索される場合は設定不要）")]
    public Canvas targetCanvas;

    [Tooltip("画像の大きさ")]
    public Vector2 imageSize = new Vector2(100, 100);

    [Tooltip("スプライトサイズを一定に保つ")]
    public bool maintainConstantSize = true;

    [Header("透過度設定")]
    [Range(0, 1)]
    [Tooltip("第一レイヤーの透過度 (0 = 完全透明, 1 = 不透明)")]
    public float layer1Alpha = 0.6f; // 40%不透明（60%透明）

    [Range(0, 1)]
    [Tooltip("第二レイヤーの透過度 (0 = 完全透明, 1 = 不透明)")]
    public float layer2Alpha = 0.3f; // 30%不透明（70%透明）

    [Header("アニメーションパターン")]
    [Tooltip("アニメーションパターン (0=なし, 1=Short系, 2=Long系)")]
    [Range(0, 2)]
    public int animationPattern = 1;

    [Tooltip("アニメーション完了後にオブジェクトをプールに返す")]
    public bool returnToPoolWhenDone = true;

    [Header("インタラクション設定")]
    [Tooltip("クリック検出を有効にする")]
    public bool enableClickDetection = true;

    [Tooltip("クリック判定の猶予時間（秒）")]
    [Range(0.01f, 0.5f)]
    public float clickGracePeriod = 0.1f;

    [Tooltip("クリック成功時のコールバック")]
    public UnityEngine.Events.UnityEvent onClickSuccess;

    [Tooltip("クリック失敗時のコールバック")]
    public UnityEngine.Events.UnityEvent onClickFail;

    [Header("色設定")]
    [Tooltip("通常時の色")]
    public Color normalColor = Color.white;

    [Tooltip("クリック成功時の色")]
    public Color successColor = Color.green;

    [Tooltip("長押し中の色")]
    public Color holdingColor = Color.blue;

    [Tooltip("失敗時の色")]
    public Color failColor = Color.red;

    [Tooltip("色変更の持続時間（秒）")]
    public float colorChangeDuration = 0.5f;

    [Header("プール返却設定")]
    [Tooltip("成功時のプール返却遅延（秒）")]
    public float successReturnDelay = 0.5f;

    [Tooltip("失敗時のプール返却遅延（秒）")]
    public float failReturnDelay = 0.3f;

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

    // UI関連の変数
    private RectTransform canvasRectTransform;
    private GameObject uiContainer;
    private RectTransform uiRectTransform;
    private Camera mainCamera;

    // レイヤー管理用変数
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

    // クリック検出用変数
    private bool clickable = false;
    private bool requireLongPress = false;
    private bool waitingForRelease = false;
    private bool clickPressed = false;
    private float clickTimer = 0f;
    private float clickTargetTime = 0f;
    private float releaseTargetTime = 0f;
    private int currentAnimStage = 0;
    private bool animationStarted = false;
    private bool animationCompleted = false;
    private float objectCreationTime = 0f;

  //  private float clickAvailableTime;   // クリック可能になった時間
  

    // ミサイルの状態フラグ
    private bool isSuccessful = false;
    private bool isFailed = false;
    private bool isReturningToPool = false;

    // 元のミサイル色を保存
    private Color originalColor;

    // レンダラー参照（色変更用）
    private Renderer missileRenderer;

    // アニメーション順序の配列（Short系とLong系）
    private readonly AnimationType[] shortAnimSequence = new AnimationType[]
    {
        AnimationType.Short1st,
        AnimationType.Short2nd,
        AnimationType.Short3rd,
        AnimationType.Hit
    };

    private readonly AnimationType[] longAnimSequence = new AnimationType[]
    {
        AnimationType.Long1st,
        AnimationType.Long2nd,
        AnimationType.Long3rd,
        AnimationType.Long4th,
        AnimationType.Hit
    };

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
        mainCamera = Camera.main;

        // レンダラーコンポーネントを取得
        missileRenderer = GetComponent<Renderer>();
        if (missileRenderer != null)
        {
            // 元の色を保存
            originalColor = missileRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("MissileDirectAnimManager: このオブジェクトにRendererコンポーネントが見つかりません。色変更機能は動作しません。");
        }

        // パターン配列のチェック
        if (animationPattern < 0 || animationPattern > 2)
        {
            animationPattern = 1; // デフォルト値を設定
            Debug.LogWarning("MissileDirectAnimManager: アニメーションパターンが範囲外です。デフォルト値を設定しました。");
        }

        // UIのセットアップ
        SetupUI();

        // アニメーションレイヤーを作成
        CreateAnimationLayers();
    }

    /// <summary>
    /// UIコンテナを設定
    /// </summary>
    private void SetupUI()
    {
        // キャンバスが指定されていない場合は自動で探す
        if (targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
            if (targetCanvas == null)
            {
                // キャンバスが見つからない場合は新しく作成
                GameObject canvasObj = new GameObject("MissileEffectCanvas");
                targetCanvas = canvasObj.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // キャンバススケーラーを追加
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // Raycasterを追加（必要に応じて）
                canvasObj.AddComponent<GraphicRaycaster>();

        
            }
        }

        canvasRectTransform = targetCanvas.GetComponent<RectTransform>();

        // UIコンテナを作成（ミサイルのスプライト表示用）
        uiContainer = new GameObject("MissileEffectContainer");
        uiContainer.transform.SetParent(targetCanvas.transform, false);

        uiRectTransform = uiContainer.AddComponent<RectTransform>();
        uiRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        uiRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        uiRectTransform.sizeDelta = imageSize;
    }

    private void Start()
    {
        // 初期状態では全レイヤー非表示
        SetAllLayersInvisible();

        // クリック判定時間の設定
        if (CRIWARE_conductor.Instance != null && CRIWARE_conductor.Instance.BasicBeat > 0)
        {
            oneBeat = CRIWARE_conductor.Instance.BasicBeat;
        }
        else
        {
            oneBeat = 0.4f; // デフォルト値
        }

        // 初期状態ではオブジェクトの色を元の色に設定
        if (missileRenderer != null)
        {
            missileRenderer.material.color = normalColor;
        }
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
    /// アニメーションを手動でリセットする
    /// </summary>
    public void ResetAnimation()
    {
        animationStarted = false;
        animationCompleted = false;
        currentAnimStage = 0;
        SetAllLayersInvisible();

        // 状態フラグをリセット
        isSuccessful = false;
        isFailed = false;
        isReturningToPool = false;

        // 色を元に戻す
        if (missileRenderer != null)
        {
            missileRenderer.material.color = normalColor;
        }
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
        layerObj.transform.SetParent(uiContainer.transform, false);


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

         }

        // アニメーション状態をリセット
        ResetAnimation();

        // クリック状態をリセット
        clickable = false;
        waitingForRelease = false;
        clickPressed = false;
        clickTimer = 0f;

        // すべてのレイヤーを非表示にする
        if (animLayer1st != null)
        {
            SetAllLayersInvisible();
        }
        // 生成時間を記録

        objectCreationTime = Time.time;

        // アニメーション状態をリセット
        ResetAnimation();

        // クリック状態をリセット
        clickable = false;
        waitingForRelease = false;
        clickPressed = false;
        clickTimer = 0f;
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

     }

    void Update()
    {
        // プールに戻す処理が開始されている場合は更新しない
        if (isReturningToPool) return;

        // ミサイル（このスクリプトがアタッチされているオブジェクト）のワールド座標をスクリーン座標に変換
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

        // 画面背後にある場合は非表示
        if (screenPos.z < 0)
        {
            uiContainer.SetActive(false);
            return;
        }
        else
        {
            uiContainer.SetActive(true);

            // スクリーンオフセットを適用
            screenPos.x += screenOffset.x;
            screenPos.y += screenOffset.y;

            // UI位置を更新
            uiRectTransform.position = screenPos;

            // 遠近に関わらず一定サイズを維持する場合
            if (maintainConstantSize)
            {
                // キャンバススケーラーがあれば考慮する
                if (targetCanvas != null && targetCanvas.scaleFactor > 0)
                {
                    uiRectTransform.sizeDelta = imageSize / targetCanvas.scaleFactor;
                }
                else
                {
                    uiRectTransform.sizeDelta = imageSize;
                }
            }
        }

        // クリック検出が有効で、クリック可能な状態の場合
        if (enableClickDetection && clickable)
        {
      
                // オブジェクト生成からの経過時間をベースにした判定
                // Short系の場合（クリック可能になってからではなく、生成時からの時間をベース）
                if (animationPattern == 1)
                {
                    // 生成時間からの絶対時間を使用
                    HandleShortClickAbsoluteTime();
                }
                else if (animationPattern == 2)
                {
                    // 長押しも絶対時間ベースで判定
                    HandleLongClickAbsoluteTime();
                }
          
           
        }
    }
    // 絶対時間ベースのShortクリック判定
    private void HandleShortClickAbsoluteTime()
    {
        // 生成からの経過時間
        float elapsedTime = Time.time - objectCreationTime;

        // ジャストタイミングの絶対時間（生成から4ビート後）
        float absoluteClickTargetTime = oneBeat * 4;

        // タイミング差と判定
        float timingDifference = elapsedTime - absoluteClickTargetTime;
        bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

        // マウスクリック/タッチ検出
        if (Input.GetMouseButtonDown(0))
        {
            // クリック位置がUI上かチェック
            if (IsPointerOverUI())
            {
                if (inClickWindow)
                {
                    // 成功！
                    Debug.Log($"クリック成功！ ジャストタイミングとの差: {timingDifference:F3}秒（猶予時間: ±{clickGracePeriod:F3}秒）【絶対時間】");
                    TriggerSuccessEvent();
                }
                else
                {
                    // 失敗 - タイミング外
                    Debug.Log($"クリック失敗: ジャストタイミングとの差: {timingDifference:F3}秒（猶予時間: ±{clickGracePeriod:F3}秒）【絶対時間】");

                    // 早すぎるクリックの場合でもアニメーションは継続
                    if (timingDifference < 0)
                    {
                        // 早すぎるクリックだが、アニメーションは停止せずに失敗のみ記録
                        isFailed = true;
                        ChangeObjectColor(failColor);
                        StopAndReturnToPool();

                    }
                    else
                    {
                        // 遅すぎるクリックは通常の失敗処理
                        TriggerFailEvent();
                    }
                }

                // クリック判定を無効化（アニメーションは続行させる）
                clickable = false;
            }
        }
    }
    private void HandleLongClickAbsoluteTime()
    {
        // 生成からの経過時間
        float elapsedTime = Time.time - objectCreationTime;

        // ジャストタイミングの絶対時間（生成から4ビート後に押し、5ビート後に離す）
        float absolutePressTargetTime = oneBeat * 4;
        float absoluteReleaseTargetTime = oneBeat * 5;

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
                if (IsPointerOverUI())
                {
                    if (inPressWindow)
                    {
                        // 押すタイミングOK、離すのを待つ
                        waitingForRelease = true;
                        clickPressed = true;

                        Debug.Log($"長押し開始成功！ ジャストタイミングとの差: {pressTimingDifference:F3}秒（猶予時間: ±{clickGracePeriod:F3}秒）【絶対時間】");

                        // 長押し中の色に変更（青）
                        ChangeObjectColor(holdingColor);
                    }
                    else
                    {
                        // 失敗 - 押すタイミング外
                        Debug.Log($"長押し開始失敗: ジャストタイミングとの差: {pressTimingDifference:F3}秒（猶予時間: ±{clickGracePeriod:F3}秒）【絶対時間】");

                        // 早すぎるクリックの場合でもアニメーションは継続
                        if (pressTimingDifference < 0)
                        {
                            // 早すぎるクリックだが、アニメーションは停止せずに失敗のみ記録
                            isFailed = true;
                            ChangeObjectColor(failColor);
                            StopAndReturnToPool();
                        }
                        else
                        {
                            // 遅すぎるクリックは通常の失敗処理
                            TriggerFailEvent();
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
                    TriggerSuccessEvent();
                }
                else
                {
                    // 失敗 - 離すタイミング外
                    Debug.Log($"長押し解放失敗: ジャストタイミングとの差: {releaseTimingDifference:F3}秒（猶予時間: ±{releaseGracePeriod:F3}秒）【絶対時間】");
                    TriggerFailEvent();
                    StopAndReturnToPool();
                }

                // クリック判定を無効化
                clickable = false;
                waitingForRelease = false;
                clickPressed = false;
            }
        }
    }
    public void StopAndReturnToPool()
    {
        // アニメーション進行状態をリセット
        animationStarted = false;
        animationCompleted = true;
        clickable = false;

        // すべてのレイヤーを非表示にして停止
        StopAllAnimations();
        SetAllLayersInvisible();

        // プールへの返却プロセスが開始されていなければ開始
        if (!isReturningToPool && returnToPoolWhenDone)
        {
            isReturningToPool = true;

            // 即座にプールに返却する（フレーム待ちでコルーチンを使用）
            StartCoroutine(ImmediateReturnToPool());
        }
    }

    /// <summary>
    /// 即座にオブジェクトをプールに返却するコルーチン
    /// </summary>
    private IEnumerator ImmediateReturnToPool()
    {
        // 1フレーム待機して、現在の処理が完了するのを待つ
        yield return null;

        Debug.Log("MissileDirectAnimManager: アニメーションを中断してオブジェクトをプールに即座に返却します");

        // オブジェクトを非アクティブにしてプールに戻す
        gameObject.SetActive(false);
    }



    /// <summary>
    /// ポインターがUIの上にあるか判定
    /// </summary>
    private bool IsPointerOverUI()
    {
        Vector2 mousePos = Input.mousePosition;

        // UIコンテナの位置とサイズを取得
        RectTransform rt = uiRectTransform;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // 矩形の全体サイズを計算
        float totalWidth = corners[2].x - corners[0].x;
        float totalHeight = corners[2].y - corners[0].y;

        // 中心点を計算
        float centerX = (corners[0].x + corners[2].x) / 2f;
        float centerY = (corners[0].y + corners[2].y) / 2f;

        // クリック判定を縮小（X/4, Y/4）
        float clickableWidth = totalWidth / 4f;
        float clickableHeight = totalHeight / 4f;

        // 縮小された矩形の角を計算
        float minX = centerX - clickableWidth / 2f;
        float maxX = centerX + clickableWidth / 2f;
        float minY = centerY - clickableHeight / 2f;
        float maxY = centerY + clickableHeight / 2f;

        // マウス位置が縮小された矩形内かチェック
        if (mousePos.x >= minX && mousePos.x <= maxX &&
            mousePos.y >= minY && mousePos.y <= maxY)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 成功イベントを発火
    /// </summary>
    private void TriggerSuccessEvent()
    {
        Debug.Log("MissileDirectAnimManager: クリック成功！");

        // 成功状態を設定
        isSuccessful = true;
        isFailed = false;

        // 成功時の色に変更（緑）
        ChangeObjectColor(successColor);

        // 成功イベントを実行
        onClickSuccess?.Invoke();

        // 成功エフェクトなどを表示する場合はここに追加
        TriggerHitAnimation();

        // 成功時の遅延でプールに返す
        //if (returnToPoolWhenDone)
        //{
        //    StartCoroutine(ReturnToPoolWithDelay(successReturnDelay));
        //}
    }

    /// <summary>
    /// 失敗イベントを発火
    /// </summary>
    private void TriggerFailEvent()
    {
        Debug.Log("MissileDirectAnimManager: クリック失敗");

        // 失敗状態を設定
        isSuccessful = false;
        isFailed = true;

        // 失敗時の色に変更（赤）
        ChangeObjectColor(failColor);

        // 失敗イベントを実行
        onClickFail?.Invoke();

        // 失敗時の遅延でプールに返す
        //if (returnToPoolWhenDone)
        //{
        //    StartCoroutine(ReturnToPoolWithDelay(failReturnDelay));
        //}
    }

    /// <summary>
    /// オブジェクトの色を変更
    /// </summary>
    private void ChangeObjectColor(Color newColor)
    {
        if (missileRenderer == null) return;

        // 色変更コルーチンを開始
        StartCoroutine(ColorChangeCoroutine(newColor));
    }

    /// <summary>
    /// 色を徐々に変更するコルーチン
    /// </summary>
    private IEnumerator ColorChangeCoroutine(Color targetColor)
    {
        if (missileRenderer == null) yield break;

        // 開始色を取得
        Color startColor = missileRenderer.material.color;
        float elapsedTime = 0f;

        // 色を徐々に変更
        while (elapsedTime < colorChangeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / colorChangeDuration);

            // 色の補間
            missileRenderer.material.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        // 確実に目標色に設定
        missileRenderer.material.color = targetColor;
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

                Debug.Log("MissileDirectAnimManager: テンポ情報を設定しました - OneBeat: " + oneBeat + ", frameDelay: " + frameDelay);
            }
            else
            {
                Debug.Log("MissileDirectAnimManager: テンポ情報が初期化されていないため、アニメーションを開始できません");
                return;
            }
        }

        // オブジェクトが非アクティブならアニメーションを更新しない
        if (!uiContainer.activeInHierarchy) return;

        // プールに戻す処理が開始されている場合は更新しない
        if (isReturningToPool) return;

        // すでにアニメーションが完了している場合は何もしない
        if (animationCompleted) return;

        // アニメーションをまだ開始していない場合は開始する
        if (!animationStarted)
        {
            StartAnimation();
            return;
        }

        // 次のアニメーションステージに進む
        AdvanceAnimationStage();
    }

    /// <summary>
    /// アニメーションを開始する
    /// </summary>
    private void StartAnimation()
    {
        animationStarted = true;
        currentAnimStage = 0;

        // すべてのレイヤーを非表示にする
        SetAllLayersInvisible();

        // パターンに基づいて最初のアニメーションを設定
        PlayCurrentAnimationStage();

        // レイヤーの透過度を更新
        UpdateLayerTransparencies();

        // クリック判定用変数をリセット
        clickable = false;
        waitingForRelease = false;
        clickPressed = false;
        clickTimer = 0f;

        // パターンに基づいてクリックターゲット時間を設定
        if (animationPattern == 1) // Short系
        {
            // Short系は3ビート後にクリック
            clickTargetTime = oneBeat * 3;
            requireLongPress = false;
        }
        else if (animationPattern == 2) // Long系
        {
            // Long系は3ビート後に押し、4ビート後に離す
            clickTargetTime = oneBeat * 3;
            releaseTargetTime = oneBeat * 4;
            requireLongPress = true;
        }
    }

    /// <summary>
    /// 次のアニメーションステージに進む
    /// </summary>
    private void AdvanceAnimationStage()
    {
        currentAnimStage++;

        // 選択されたパターンの配列長をチェック
        int maxStages = (animationPattern == 2) ? longAnimSequence.Length :
                       (animationPattern == 1) ? shortAnimSequence.Length : 1;

        // すべてのステージが終了したかチェック
        if (currentAnimStage >= maxStages)
        {
            CompleteAnimation();
            return;
        }

        // 次のアニメーションを再生
        PlayCurrentAnimationStage();

        // クリック判定を有効化する（Short系は2段階目[index 1]、Long系は3段階目[index 2]の時）
        if (enableClickDetection)
        {
            if (animationPattern == 1 && currentAnimStage == 1) // Short系の3段階目
            {
                clickable = true;
                clickTimer = 0f;
            //    clickAvailableTime = Time.time; // クリック可能になった時間を記録
            }
            else if (animationPattern == 2 && currentAnimStage == 1) // Long系の4段階目
            {
                clickable = true;
                clickTimer = 0f;
          //      clickAvailableTime = Time.time; // クリック可能になった時間を記録
            }
        }
    }

    /// <summary>
    /// 現在のステージのアニメーションを再生する
    /// </summary>
    private void PlayCurrentAnimationStage()
    {
        // すべてのレイヤーを非表示にする
        SetAllLayersInvisible();

        // パターンが0（なし）の場合は何も表示しない
        if (animationPattern == 0) return;

        // 現在のアニメーションタイプを取得
        AnimationType currentType = AnimationType.None;

        if (animationPattern == 1 && currentAnimStage < shortAnimSequence.Length)
        {
            currentType = shortAnimSequence[currentAnimStage];
        }
        else if (animationPattern == 2 && currentAnimStage < longAnimSequence.Length)
        {
            currentType = longAnimSequence[currentAnimStage];
        }

        // アニメーションタイプに応じたレイヤーを表示
        switch (currentType)
        {
            case AnimationType.Short1st:
                animLayer1st.ChangeSprites(shortFirstSprites);
                animLayer1st.SetVisibility(true);
                animLayer1st.RestartAnimation();
                break;
            case AnimationType.Short2nd:
                animLayer2nd.ChangeSprites(shortSecondSprites);
                animLayer2nd.SetVisibility(true);
                animLayer2nd.RestartAnimation();
                break;
            case AnimationType.Short3rd:
                animLayer3rd.ChangeSprites(shortThirdSprites);
                animLayer3rd.SetVisibility(true);
                animLayer3rd.RestartAnimation();
                break;
            case AnimationType.Long1st:
                animLayer1st.ChangeSprites(longFirstSprites);
                animLayer1st.SetVisibility(true);
                animLayer1st.RestartAnimation();
                break;
            case AnimationType.Long2nd:
                animLayer2nd.ChangeSprites(longSecondSprites);
                animLayer2nd.SetVisibility(true);
                animLayer2nd.RestartAnimation();
                break;
            case AnimationType.Long3rd:
                animLayer3rd.ChangeSprites(longThirdSprites);
                animLayer3rd.SetVisibility(true);
                animLayer3rd.RestartAnimation();
                break;
            case AnimationType.Long4th:
                animLayer4th.ChangeSprites(longFourthSprites);
                animLayer4th.SetVisibility(true);
                animLayer4th.RestartAnimation();
                break;
            case AnimationType.Hit:
                hitLayer.ChangeSprites(hitSprites);
                hitLayer.SetVisibility(true);
                hitLayer.RestartAnimation();
                break;
        }
    }

    /// <summary>
    /// アニメーションを完了してプールに戻す
    /// </summary>
    private void CompleteAnimation()
    {
        animationCompleted = true;
        clickable = false;

        // すべてのレイヤーを非表示
        SetAllLayersInvisible();

        // クリックされなかった場合の失敗処理
        if (enableClickDetection && animationPattern > 0 && !clickPressed && !isSuccessful && !isFailed)
        {
            TriggerFailEvent();
        }

        // オブジェクトプールに戻す場合（すでに成功/失敗イベントで返却処理が開始されていない場合のみ）
        if (returnToPoolWhenDone && !isReturningToPool)
        {
            // デフォルトの遅延を使用
            StartCoroutine(ReturnToPoolWithDelay(0f));
        }
    }

    /// <summary>
    /// 遅延付きでオブジェクトをプールに戻す
    /// </summary>
    private IEnumerator ReturnToPoolWithDelay(float delay)
    {
        // 返却処理が開始されていなければ開始
        if (!isReturningToPool)
        {
            isReturningToPool = true;

            // 指定された遅延を待つ
            yield return new WaitForSeconds(delay);

            Debug.Log($"MissileDirectAnimManager: オブジェクトをプールに返却します（遅延: {delay}秒）");

            // オブジェクトを非アクティブにしてプールに戻す
            gameObject.SetActive(false);
        }
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