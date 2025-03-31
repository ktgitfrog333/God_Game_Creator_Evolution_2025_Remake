using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// ホーミングミサイル自体にアタッチして使用するアニメーションマネージャークラス
/// （オブジェクトプーラー対応版）
/// </summary>
public class MissileDirectAnimManagerB : MonoBehaviour
{
    #region Public Variable Definitions

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

    [Header("ノーツ設定")]
    [Tooltip("ノーツの種類")]
    public MissileNoteType noteType = MissileNoteType.Short;

    [Tooltip("アニメーション完了後にオブジェクトをプールに返す")]
    private bool returnToPoolWhenDone = true;

    [Header("プール返却設定")]
    [Tooltip("成功時のプール返却遅延（秒）")]
    private float successReturnDelay = 0f; 

    [Tooltip("失敗時のプール返却遅延（秒）")]
    private float failReturnDelay = 0.2f; // 0.3秒から0.2秒に変更

    [Header("インタラクション設定")]
    [Tooltip("クリック検出を有効にする")]
    private bool enableClickDetection = true;

    [Tooltip("クリック判定の猶予時間（秒）")]
    [Range(0.01f, 0.5f)]
    private float clickGracePeriod = 0.1f;

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

    [Header("基本スプライトセット")]
    [Tooltip("Long1stアニメーション用スプライト")]
    public Sprite[] longFirstSprites;

    [Tooltip("Long2ndアニメーション用スプライト")]
    public Sprite[] longSecondSprites;

    [Tooltip("Long3rdアニメーション用スプライト")]
    public Sprite[] longThirdSprites;

    [Tooltip("Long4thアニメーション用スプライト (1拍長押し用)")]
    public Sprite[] long01_4thSprites;

    [Tooltip("Short1stアニメーション用スプライト")]
    public Sprite[] shortFirstSprites;

    [Tooltip("Short2ndアニメーション用スプライト")]
    public Sprite[] shortSecondSprites;

    [Tooltip("Short3rdアニメーション用スプライト")]
    public Sprite[] shortThirdSprites;

    [Tooltip("Hitアニメーション用スプライト")]
    public Sprite[] hitSprites;

    [Header("2拍長押しスプライトセット")]
    [Tooltip("2拍長押し1段階目スプライト (Long02_01)")]
    public Sprite[] long02_1stSprites;

    [Tooltip("2拍長押し2段階目スプライト (Long02_02)")]
    public Sprite[] long02_2ndSprites;

    [Header("3拍長押しスプライトセット")]
    [Tooltip("3拍長押し1段階目スプライト (Long03_01)")]
    public Sprite[] long03_1stSprites;

    [Tooltip("3拍長押し2段階目スプライト (Long03_02)")]
    public Sprite[] long03_2ndSprites;

    [Tooltip("3拍長押し3段階目スプライト (Long03_03)")]
    public Sprite[] long03_3rdSprites;

    [Header("フレーム設定")]
    [Tooltip("通常アニメーションのフレーム間の時間（秒）")]
    public float frameDelay = 0.1f;

    [Tooltip("Hitアニメーションのフレーム間の時間（秒）固定値")]
    public float hitFrameDelay = 0.07f;

    // Public Variable Definitionsセクションに以下を追加
    [Header("エイムサークル設定")]
    [Tooltip("クリック判定用のエイムサークルスプライト")]
    public Sprite aimCircleSprite;

    [Tooltip("エイムサークルの透明度")]
    [Range(0f, 1f)]
    public float aimCircleAlpha = 0.5f;

    [Tooltip("エイムサークルのサイズ倍率")]
    [Range(0.1f, 2.0f)]
    public float aimCircleScale = 1.0f;

    #endregion

    #region Private Variables

    // コンポーネント参照
    private MissileUIManager uiManager;
    private MissileInputManager inputManager;
    private MissileAnimationManager animManager;
    private Camera mainCamera;
    private Renderer missileRenderer;

    // 状態変数
    private bool isSuccessful = false;
    private bool isFailed = false;
    private bool isReturningToPool = false;
    private float objectCreationTime = 0f;
    private float oneBeat = 0.4f;
    private bool isInitialized = false;

    // オブジェクト管理用
    private GameObject[] uiElements = null;
    private bool isFirstInit = true;

    #endregion

    #region Unity ライフサイクル
    public Sprite GetAimCircleSprite()
    {
        return aimCircleSprite;
    }

    private void Awake()
    {
        // 初回初期化のみ実行する処理
        if (isFirstInit)
        {
            mainCamera = Camera.main;

            // レンダラーコンポーネントを取得
            missileRenderer = GetComponent<Renderer>();
            if (missileRenderer == null)
            {
                Debug.LogWarning($"{gameObject.name}: このオブジェクトにRendererコンポーネントが見つかりません。色変更機能は動作しません。");
            }

            // タイマーを設定（安全対策）
            StartCoroutine(SafetyTimer());

            isFirstInit = false;
        }

        // 毎回実行する初期化処理
        ResetComponentState();
    }
    public float GetAimCircleAlpha()
    {
        return aimCircleAlpha;
    }
    public void UpdateAimCircleSize(float scale)
    {

        aimCircleScale = scale;
        if (uiManager != null)
        {
            uiManager.UpdateAimCircleSize(scale);
        }
    }

    private void Start()
    {
        // 初期状態では全レイヤー非表示
        if (animManager != null)
        {
            animManager.SetAllLayersInvisible();
        }

        // CRIWAREからテンポ情報を取得
        UpdateTempoInfo();
    }

    private void OnEnable()
    {
        // イベントリスナーの登録
        RegisterEventListeners();

        // CRIWAREからテンポ情報を取得
        UpdateTempoInfo();

        // アニメーション状態をリセット
        ResetAnimation();

        // 生成時間を記録
        objectCreationTime = Time.time;

        UpdateAimCircleSize(aimCircleScale);
    }

    private void OnDisable()
    {
        // イベントリスナーの解除
        UnregisterEventListeners();

        // アニメーションを停止
        if (animManager != null)
        {
            animManager.StopAllAnimations();
        }

        // 状態をリセット
        isSuccessful = false;
        isFailed = false;
        isReturningToPool = false;
    }

    // MissileDirectAnimManagerB.cs の Update メソッドを修正
    private void Update()
    {
        // プールに戻す処理が開始されている場合は更新しない
        if (isReturningToPool) return;

        // UI位置の更新
        UpdateUIPosition();

        // クリックデバッグ（任意の場所でのクリック処理をデバッグ用に追加）
        if (Input.GetMouseButtonDown(0))
        {
            bool isOverUI = IsPointerOverUI();

            // UI上でのクリックの場合、ここでも直接処理してみる（緊急対応）
            if (isOverUI && enableClickDetection && !isFailed && !isSuccessful)
            {
                float elapsedTime = Time.time - objectCreationTime;
                // 直接タイミング判定して処理
                ProcessClick(elapsedTime);
            }
        }

        // 入力検出の処理（通常のフロー）
        if (enableClickDetection && !isFailed && !isSuccessful && inputManager != null)
        {
            inputManager.HandleInput(noteType, Time.time - objectCreationTime);
        }

        // アニメーションが完了していたら返却処理
        if (animManager != null && animManager.IsAnimationCompleted() && returnToPoolWhenDone && !isReturningToPool)
        {
            if (!isSuccessful && !isFailed)
            {
                Debug.Log($"{gameObject.name}: アニメーション完了を検出。プールに返却します。");
                StartCoroutine(ReturnToPoolWithDelay(0.2f));
            }
        }
    }
    // 直接クリック処理用の新しいメソッドを追加
    private void ProcessClick(float elapsedTime)
    {
        // ジャストタイミングの絶対時間（生成から4ビート後）
        float absoluteClickTargetTime = oneBeat * 4;

        // ノーツタイプごとの判定
        if (noteType == MissileNoteType.Short)
        {
            // 短押し判定
            ProcessShortClick(elapsedTime, absoluteClickTargetTime);
        }
        else if (noteType == MissileNoteType.Long1Beat ||
                 noteType == MissileNoteType.Long2Beat ||
                 noteType == MissileNoteType.Long3Beat)
        {
            // 長押し判定（マウスボタン押下時）
            ProcessLongClickDown(elapsedTime, absoluteClickTargetTime);
        }
    }
    // 短押し判定処理
    private void ProcessShortClick(float elapsedTime, float targetTime)
    {
        // タイミング差と判定
        float timingDifference = elapsedTime - targetTime;
        bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

        Debug.Log($"{gameObject.name}: 短押し判定 - 経過時間: {elapsedTime:F2}秒, 目標時間: {targetTime:F2}秒, 差: {timingDifference:F2}秒, 猶予: {clickGracePeriod:F2}秒");

        if (inClickWindow)
        {
            // 成功！
            Debug.Log($"{gameObject.name}: クリック成功！ ジャストタイミングとの差: {timingDifference:F3}秒");
            TriggerSuccessEvent();
        }
        else
        {
            // 失敗 - タイミング外
            Debug.Log($"{gameObject.name}: クリック失敗: ジャストタイミングとの差: {timingDifference:F3}秒");
            TriggerFailEvent();
        }
    }

    // 長押し開始判定処理
    private void ProcessLongClickDown(float elapsedTime, float targetTime)
    {
        // タイミング差と判定
        float timingDifference = elapsedTime - targetTime;
        bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

        Debug.Log($"{gameObject.name}: 長押し開始判定 - 経過時間: {elapsedTime:F2}秒, 目標時間: {targetTime:F2}秒, 差: {timingDifference:F2}秒, 猶予: {clickGracePeriod:F2}秒");

        if (inClickWindow)
        {
            // 長押し開始成功 - 色を変更して長押し中状態に
            Debug.Log($"{gameObject.name}: 長押し開始成功！");
            SetHoldingColor();

            // 長押し情報を保存（inputManagerに通知する代わりに直接処理）
            if (inputManager != null)
            {
                inputManager.StartLongPress(targetTime);
            }
        }
        else
        {
            // 失敗 - タイミング外
            Debug.Log($"{gameObject.name}: 長押し開始失敗: ジャストタイミングとの差: {timingDifference:F3}秒");
            TriggerFailEvent();
        }
    }

    private void OnDestroy()
    {
        // UIコンテナを削除（必要な場合）
        if (uiManager != null)
        {
            GameObject container = uiManager.GetUIContainer();
            if (container != null)
            {
                Destroy(container);
            }
        }
    }

    #endregion

    #region 初期化メソッド

    /// <summary>
    /// コンポーネントの状態をリセットする（プールから再利用時）
    /// </summary>
    private void ResetComponentState()
    {
        // 設定値の検証
        ValidateSettings();

        // 変数の初期化
        isSuccessful = false;
        isFailed = false;
        isReturningToPool = false;

        // レンダラーの色をリセット
        if (missileRenderer != null)
        {
            missileRenderer.material.color = normalColor;
        }

        // サブマネージャーの初期化
        CreateManagers();
    }

    private void CreateManagers()
    {
        // メインカメラの参照を更新
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // UIマネージャーの作成
        uiManager = new MissileUIManager(this, targetCanvas, imageSize, layer1Alpha, layer2Alpha);

        // アニメーションマネージャーの作成
        animManager = new MissileAnimationManager(this, uiManager);

        // 入力マネージャーの作成
        inputManager = new MissileInputManager(this, clickGracePeriod, oneBeat);
    }

    private void ValidateSettings()
    {
        // 有効なノーツタイプかチェック
        if ((int)noteType < 0 || (int)noteType > 4)
        {
            noteType = MissileNoteType.Short; // デフォルト値を設定
            Debug.LogWarning($"{gameObject.name}: ノーツタイプが範囲外です。デフォルト値を設定しました。");
        }

        if (frameDelay <= 0)
        {
            Debug.LogWarning($"{gameObject.name}: frameDelay が 0 以下です。デフォルト値 0.1 を使用します。");
            frameDelay = 0.1f;
        }
    }

    private void RegisterEventListeners()
    {
        CRIWARE_conductor.TempoMethodEvent1 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent2 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent3 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent4 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent5 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent6 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent7 += TempoMethod;
        CRIWARE_conductor.TempoMethodEvent8 += TempoMethod;
    }

    private void UnregisterEventListeners()
    {
        CRIWARE_conductor.TempoMethodEvent1 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent2 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent3 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent4 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent5 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent6 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent7 -= TempoMethod;
        CRIWARE_conductor.TempoMethodEvent8 -= TempoMethod;
    }

    private void UpdateTempoInfo()
    {
        if (CRIWARE_conductor.Instance != null && CRIWARE_conductor.Instance.BasicBeat > 0)
        {
            oneBeat = CRIWARE_conductor.Instance.BasicBeat;
            frameDelay = oneBeat / 8;
            isInitialized = true;

            // 各レイヤーにフレーム間の時間を設定
            if (animManager != null)
            {
                animManager.UpdateFrameDelays(frameDelay);
            }

            if (inputManager != null)
            {
                inputManager.UpdateTempoInfo(oneBeat, noteType);
            }
        }
        else
        {
            oneBeat = 0.4f; // デフォルト値
        }
    }

    #endregion

    #region UIポジショニング

    private void UpdateUIPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (uiManager == null) return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

        // 画面背後にある場合は非表示
        if (screenPos.z < 0)
        {
            uiManager.SetUIActive(false);
            return;
        }
        else
        {
            uiManager.SetUIActive(true);

            // スクリーンオフセットを適用
            screenPos.x += screenOffset.x;
            screenPos.y += screenOffset.y;

            // UI位置とサイズを更新
            uiManager.UpdateUITransform(screenPos, imageSize, maintainConstantSize);
        }
    }

    #endregion

    #region アニメーションと状態管理

    public void ResetAnimation()
    {
        if (animManager != null)
        {
            animManager.ResetAllAnimations();
        }

        // 状態フラグをリセット
        isSuccessful = false;
        isFailed = false;
        isReturningToPool = false;

        // 色を元に戻す
        if (missileRenderer != null)
        {
            missileRenderer.material.color = normalColor;
        }

        // 入力マネージャーをリセット
        if (inputManager != null)
        {
            inputManager.Reset();
        }
    }

    public void TriggerSuccessEvent()
    {
        Debug.Log($"{gameObject.name}: クリック成功！");

        // 成功状態を設定
        isSuccessful = true;
        isFailed = false;

        // 成功時の色に変更（緑）
        ChangeObjectColor(successColor);

        // 成功イベントを実行
        onClickSuccess?.Invoke();

        // 成功エフェクトなどを表示する場合はここに追加
        if (animManager != null)
        {
            animManager.TriggerHitAnimation();
        }

        // 成功時の遅延でプールに返す
        if (returnToPoolWhenDone)
        {
            // この行を確実に実行するため、コルーチンを直接開始する
            StartCoroutine(ReturnToPoolWithDelay(successReturnDelay));
            Debug.Log($"{gameObject.name}: 成功後のプール返却コルーチンを開始しました");
        }
    }

    public void TriggerFailEvent()
    {
        Debug.Log($"{gameObject.name}: クリック失敗");

        // 失敗状態を設定
        isSuccessful = false;
        isFailed = true;

        // 失敗時の色に変更（赤）
        ChangeObjectColor(failColor);

        // 失敗イベントを実行
        onClickFail?.Invoke();

        // 失敗時の遅延でプールに返す
        if (returnToPoolWhenDone)
        {
            // この行を確実に実行するため、コルーチンを直接開始する
            StartCoroutine(ReturnToPoolWithDelay(failReturnDelay));
            Debug.Log($"{gameObject.name}: 失敗後のプール返却コルーチンを開始しました");
        }
    }

    public void TriggerNoInputFailEvent()
    {
        Debug.Log($"{gameObject.name}: 入力なしで失敗しました");

        // 失敗状態を設定
        isSuccessful = false;
        isFailed = true;

        // 失敗時の色に変更（赤）
        ChangeObjectColor(failColor);

        // 失敗イベントを実行
        onClickFail?.Invoke();

        // 失敗時の遅延でプールに返す
        if (returnToPoolWhenDone)
        {
            StartCoroutine(ReturnToPoolWithDelay(failReturnDelay));
            Debug.Log($"{gameObject.name}: 入力なしの失敗後のプール返却コルーチンを開始しました");
        }
    }

    private void ChangeObjectColor(Color newColor)
    {
        if (missileRenderer == null) return;

        // 色変更コルーチンを開始
        StartCoroutine(ColorChangeCoroutine(newColor));
    }

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

    public void TempoMethod()
    {
        // テンポ情報が初期化されていない場合は初期化を試みる
        if (!isInitialized)
        {
            UpdateTempoInfo();
            if (!isInitialized) return;
        }

        // アニメーションマネージャーが無効な場合は何もしない
        if (animManager == null) return;

        // オブジェクトが非アクティブならアニメーションを更新しない
        if (uiManager == null || !uiManager.IsUIActive()) return;

        // プールに戻す処理が開始されている場合は更新しない
        if (isReturningToPool) return;

        // アニメーションを開始または進行
        animManager.HandleTempoTick(noteType);

        // アニメーションが完了していて、かつ成功/失敗のいずれも発生していない場合は自動的にプールに返却
        if (animManager.IsAnimationCompleted() && !isSuccessful && !isFailed && returnToPoolWhenDone && !isReturningToPool)
        {
              StartCoroutine(ReturnToPoolWithDelay(0f)); // 0.5秒後に返却
        }
    }

    public IEnumerator ReturnToPoolWithDelay(float delay)
    {
        // 返却処理が開始されていなければ開始
        if (!isReturningToPool)
        {
            isReturningToPool = true;
          
            // 指定された遅延を待つ
            yield return new WaitForSeconds(delay);

         
            // プール返却前に状態をクリーンアップ
            CleanupBeforePoolReturn();

            // オブジェクトを非アクティブにしてプールに戻す
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// プールに返却する前にクリーンアップ処理を行う
    /// </summary>
    private void CleanupBeforePoolReturn()
    {
        // UIレイヤーを非表示に
        if (animManager != null)
        {
            animManager.SetAllLayersInvisible();
            animManager.StopAllAnimations();
        }

        // 色をリセット
        if (missileRenderer != null)
        {
            missileRenderer.material.color = normalColor;
        }

        // 動的生成したUIリソースをクリア（プール返却時）
        if (uiManager != null)
        {
            GameObject container = uiManager.GetUIContainer();
            if (container != null)
            {
                // 非アクティブに設定（実際に破棄はせず、次回使用時に再利用）
                container.SetActive(false);
            }
        }
    }

    // 安全対策としてのタイムアウトタイマー
    private IEnumerator SafetyTimer()
    {
        // 最大存在時間（10秒）経過後に強制的に返却
        float maxLifetime = 10f;
        yield return new WaitForSeconds(maxLifetime);

        // まだアクティブで、かつ返却処理が開始されていない場合
        if (gameObject.activeInHierarchy && !isReturningToPool)
        {
            Debug.LogWarning($"{gameObject.name}: 安全タイマーによる強制返却を実行します（{maxLifetime}秒経過）");
            ForceReturnToPool();
        }
    }

    // 強制的にプールに返却する
    public void ForceReturnToPool()
    {
        // すでに返却処理中なら何もしない
        if (isReturningToPool) return;

        isReturningToPool = true;
        Debug.Log($"{gameObject.name}: 強制的にプールに返却します");

        // アニメーションとUI要素をクリーンアップ
        CleanupBeforePoolReturn();

        // オブジェクトプーラーを直接検索して使用（緊急時の処理）
        MissileObjectPooler pooler = FindAnyObjectByType<MissileObjectPooler>();
        if (pooler != null)
        {
            // オブジェクト名から可能ならIDを推測（緊急用）
            int estimatedId = 1; // デフォルト
            string objName = gameObject.name.ToLower();

            // 名前から数字を抽出する簡易的な方法
            for (int i = 1; i <= 9; i++)
            {
                if (objName.Contains(i.ToString()))
                {
                    estimatedId = i;
                    break;
                }
            }

            pooler.ReturnMissileToPool(gameObject, estimatedId);
        }
        else
        {
            // プーラーが見つからない場合は単に非アクティブにする
            gameObject.SetActive(false);
        }
    }

    #endregion

    #region 公開プロパティとメソッド (サブマネージャー用)

    // UIマネージャー用
    public Canvas GetTargetCanvas() => targetCanvas;

    // アニメーションマネージャー用
    public Sprite[] GetSpritesForType(MissileAnimationType type)
    {
        switch (type)
        {
            case MissileAnimationType.Long1st: return longFirstSprites;
            case MissileAnimationType.Long2nd: return longSecondSprites;
            case MissileAnimationType.Long3rd: return longThirdSprites;
            case MissileAnimationType.Long4th: return long01_4thSprites;
            case MissileAnimationType.Short1st: return shortFirstSprites;
            case MissileAnimationType.Short2nd: return shortSecondSprites;
            case MissileAnimationType.Short3rd: return shortThirdSprites;
            case MissileAnimationType.Long02_01: return long02_1stSprites;
            case MissileAnimationType.Long02_02: return long02_2ndSprites;
            case MissileAnimationType.Long03_01: return long03_1stSprites;
            case MissileAnimationType.Long03_02: return long03_2ndSprites;
            case MissileAnimationType.Long03_03: return long03_3rdSprites;
            case MissileAnimationType.Hit: return hitSprites;
            case MissileAnimationType.None:
            default:
                return new Sprite[0];
        }
    }

    public float GetHitFrameDelay() => hitFrameDelay;
    public float GetNormalFrameDelay() => frameDelay;
    public float GetOneBeat() => oneBeat;
    public MissileNoteType GetNoteType() => noteType;

    // 入力マネージャー用
    public bool IsPointerOverUI() => uiManager != null && uiManager.IsPointerOverUI();
    public void SetHoldingColor() => ChangeObjectColor(holdingColor);

    // 状態確認用
    public bool IsClickDetectionEnabled()
    {
        return enableClickDetection;
    }

    public bool IsSuccessful()
    {
        return isSuccessful;
    }

    public bool IsFailed()
    {
        return isFailed;
    }

    #endregion
}