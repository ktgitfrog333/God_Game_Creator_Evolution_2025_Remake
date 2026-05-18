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
    public Vector2 screenOffset = Vector2.zero;
    public Canvas targetCanvas;
    public Vector2 imageSize = new Vector2(100, 100);
    public bool maintainConstantSize = true;

    [Header("透過度設定")]
    [Range(0, 1)] public float layer1Alpha = 0.6f;
    [Range(0, 1)] public float layer2Alpha = 0.3f;

    [Header("ノーツ設定")]
    public MissileNoteType noteType = MissileNoteType.Short;
    private bool returnToPoolWhenDone = true;

    [Header("プール返却設定")]
    private float successReturnDelay = 0f;
    private float failReturnDelay = 0.2f;

    [Header("インタラクション設定")]
    private bool enableClickDetection = true;
    [Range(0.01f, 0.5f)] private float clickGracePeriod = 0.1f;

    [Header("色設定")]
    public Color normalColor = Color.white;
    public Color holdingColor = Color.blue;
    public float colorChangeDuration = 0.5f;

    [Header("Micノーツ設定")]
    [Tooltip("Long2Beat_Mic用チェックポイント数")]
    public int micCheckpointCount = 4;

    [Tooltip("Long2Beat_Mic用アニメーション色tint")]
    public Color micNoteTintColor = Color.white;

    [Tooltip("SpectrumCircleプレハブ")]
    public GameObject spectrumCirclePrefab;

    [Tooltip("SpectrumCircleのサイズ倍率")]
    [Range(0.1f, 5.0f)]
    public float spectrumCircleScale = 1.0f;

    [Header("基本スプライトセット")]
    public Sprite[] longFirstSprites;
    public Sprite[] longSecondSprites;
    public Sprite[] longThirdSprites;
    public Sprite[] long01_4thSprites;
    public Sprite[] shortFirstSprites;
    public Sprite[] shortSecondSprites;
    public Sprite[] shortThirdSprites;

    [Header("2拍長押しスプライトセット")]
    public Sprite[] long02_1stSprites;
    public Sprite[] long02_2ndSprites;

    [Header("3拍長押しスプライトセット")]
    public Sprite[] long03_1stSprites;
    public Sprite[] long03_2ndSprites;
    public Sprite[] long03_3rdSprites;

    [Header("フレーム設定")]
    public float frameDelay = 0.1f;
    public float hitFrameDelay = 0.07f;

    [Header("エイムサークル設定")]
    public Sprite aimCircleSprite;
    [Range(0f, 1f)] public float aimCircleAlpha = 0.5f;
    [Range(0.1f, 2.0f)] public float aimCircleScale = 1.0f;

    #endregion

    #region Private Variables

    private MissileUIManager uiManager;
    private MissileInputManager inputManager;
    private MissileAnimationManager animManager;
    private MissileMicInputManager micInputManager;
    private Camera mainCamera;
    private Renderer missileRenderer;

    private bool isSuccessful = false;
    private bool isFailed = false;
    private bool isReturningToPool = false;
    private bool isForceReturning = false;
    private float objectCreationTime = 0f;
    private float oneBeat = 0.4f;
    private bool isInitialized = false;
    private bool SetTimeYet = false;

    private GameObject[] uiElements = null;
    private bool isFirstInit = true;
    private List<Coroutine> activeCoroutines = new List<Coroutine>();

    #endregion

    #region Unity ライフサイクル

    public Sprite GetAimCircleSprite() => aimCircleSprite;
    public float GetAimCircleAlpha() => aimCircleAlpha;
    public GameObject GetSpectrumCirclePrefab() => spectrumCirclePrefab;
    public float GetSpectrumCircleScale() => spectrumCircleScale;

    public void UpdateAimCircleSize(float scale)
    {
        aimCircleScale = scale;
        if (uiManager != null)
        {
            uiManager.UpdateAimCircleSize(scale);
        }
    }

    private void Awake()
    {
        if (isFirstInit)
        {
            mainCamera = Camera.main;

            missileRenderer = GetComponent<Renderer>();
            if (missileRenderer == null)
            {
                Debug.LogWarning($"{gameObject.name}: Rendererコンポーネントが見つかりません。");
            }

            activeCoroutines.Add(StartManagedCoroutine(SafetyTimer()));
            isFirstInit = false;
        }

        ResetComponentState();
    }

    private void Start()
    {
        if (animManager != null)
        {
            animManager.SetAllLayersInvisible();
        }

        UpdateTempoInfo();
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.one;

        isReturningToPool = false;
        isForceReturning = false;
        activeCoroutines.Clear();

        RegisterEventListeners();
        UpdateTempoInfo();
        ResetAnimation();

        objectCreationTime = Time.time;

        if (uiManager != null)
        {
            uiManager.EnsureUIContainerActive();
            uiManager.UpdateAimCircleSize(aimCircleScale);
            uiManager.SetAimCircleAlpha(aimCircleAlpha);

            // Long2Beat_Micの場合はSpectrumCircleを表示
            if (noteType == MissileNoteType.Long2Beat_Mic)
            {
                uiManager.SetSpectrumCircleActive(true);
            }
            else
            {
                uiManager.SetSpectrumCircleActive(false);
            }
        }
        else
        {
            Debug.LogError("OnEnable: uiManagerがnullです");
        }

        // Long2Beat_Micの場合はアニメーションに色を設定
        if (noteType == MissileNoteType.Long2Beat_Mic && animManager != null)
        {
            animManager.SetLayerTintColor(micNoteTintColor);
        }
        else if (animManager != null)
        {
            animManager.SetLayerTintColor(Color.white);
        }

        SetTimeYet = true;

        activeCoroutines.Add(StartManagedCoroutine(SafetyTimer()));

        Debug.Log($"OnEnable完了: GameObjectID={gameObject.GetInstanceID()}, Position={transform.position}");
    }

    private void OnDisable()
    {
        UnregisterEventListeners();

        if (animManager != null)
        {
            animManager.StopAllAnimations();
        }

        if (uiManager != null)
        {
            uiManager.CleanupUIContainer();
            uiManager.SetUIActive(false);

            GameObject container = uiManager.GetUIContainer();
            if (container != null)
            {
                container.SetActive(false);
            }
        }

        isSuccessful = false;
        isFailed = false;
        isReturningToPool = false;
        isForceReturning = false;

        StopAllCoroutines();
        activeCoroutines.Clear();
    }

    private void Update()
    {
        if (isReturningToPool || isForceReturning) return;

        UpdateUIPosition();

        if (Input.GetKeyDown(KeyCode.F1) && uiManager != null)
        {
            uiManager.ForceShowAimCircle();
            Debug.Log("F1キー: AimCircleを強制表示");
        }

        if (Input.GetKeyDown(KeyCode.F2) && uiManager != null)
        {
            uiManager.CreateNewTestAimCircle();
            Debug.Log("F2キー: 新しいテスト用AimCircleを作成");
        }

        // クリック処理
        if (Input.GetMouseButtonDown(0))
        {
            bool isOverUI = IsPointerOverUI();

            if (isOverUI && enableClickDetection && !isFailed && !isSuccessful)
            {
                float elapsedTime = Time.time - objectCreationTime;

                if (noteType != MissileNoteType.Long2Beat_Mic)
                {
                    ProcessClick(elapsedTime);
                }
                // Long2Beat_Micはクリック不要なので何もしない
            }
        }

        // リリース処理
        if (Input.GetMouseButtonUp(0))
        {
            bool isOverUI = IsPointerOverUI();

            if (isOverUI && enableClickDetection && !isFailed && !isSuccessful)
            {
                float elapsedTime = Time.time - objectCreationTime;

                if (noteType == MissileNoteType.Long1Beat ||
                    noteType == MissileNoteType.Long2Beat ||
                    noteType == MissileNoteType.Long3Beat)
                {
                    if (inputManager != null && inputManager.IsLongPressStarted())
                    {
                        inputManager.HandleLongPressRelease(elapsedTime);
                    }
                }
            }
        }

        // 通常入力処理
        if (enableClickDetection && !isFailed && !isSuccessful && inputManager != null
            && noteType != MissileNoteType.Long2Beat_Mic)
        {
            inputManager.HandleInput(noteType, Time.time - objectCreationTime);
        }

        // Micノーツ更新処理
        if (noteType == MissileNoteType.Long2Beat_Mic && !isFailed && !isSuccessful)
        {
            UpdateMicInput();
        }

        // アニメーション完了チェック
        if (animManager != null && animManager.IsAnimationCompleted() && returnToPoolWhenDone
            && !isReturningToPool && !isForceReturning)
        {
            if (!isSuccessful && !isFailed)
            {
                StartManagedCoroutine(ReturnToPoolWithDelay(0.2f));
            }
        }

        // 入力期限切れチェック（Long2Beat_Micは対象外）
        if (!isSuccessful && !isFailed && !isReturningToPool && !isForceReturning && enableClickDetection)
        {
            float elapsedTime = Time.time - objectCreationTime;
            float absoluteClickTargetTime = oneBeat * 4;
            float clickDeadlineTime = absoluteClickTargetTime + clickGracePeriod;

            if (noteType == MissileNoteType.Short && elapsedTime > clickDeadlineTime)
            {
                TriggerNoInputFailEvent();
            }
            else if ((noteType == MissileNoteType.Long1Beat ||
                      noteType == MissileNoteType.Long2Beat ||
                      noteType == MissileNoteType.Long3Beat) &&
                     elapsedTime > clickDeadlineTime &&
                     inputManager != null &&
                     !inputManager.IsLongPressStarted())
            {
                TriggerNoInputFailEvent();
            }
        }
    }

    #endregion

    #region Micノーツ処理

    private void UpdateMicInput()
    {
        if (micInputManager == null) return;
        if (micInputManager.IsFinished()) return;

        float elapsedTime = Time.time - objectCreationTime;
        float measureStartTargetTime = oneBeat * 4;

        if (!micInputManager.IsLongPressStarted() && elapsedTime >= measureStartTargetTime)
        {
            if (!isInitialized)
            {
                Debug.LogWarning($"[MicInput] oneBeatが未初期化のため待機中: oneBeat={oneBeat}");
                return;
            }
            micInputManager.StartMeasure(elapsedTime, oneBeat * 2f);
            Debug.Log($"[MicInput] 4拍後に計測開始: elapsedTime={elapsedTime:F3}");
        }

        if (micInputManager.IsLongPressStarted())
        {
            micInputManager.UpdateMeasure(elapsedTime);
        }
    }

    #endregion

    #region 初期化メソッド

    private Coroutine StartManagedCoroutine(IEnumerator routine)
    {
        if (isReturningToPool || isForceReturning) return null;

        Coroutine coroutine = StartCoroutine(routine);
        if (coroutine != null)
        {
            activeCoroutines.Add(coroutine);
        }
        return coroutine;
    }

    private void ProcessClick(float elapsedTime)
    {
        if (isReturningToPool || isForceReturning) return;

        float absoluteClickTargetTime = oneBeat * 4;

        if (noteType == MissileNoteType.Short)
        {
            ProcessShortClick(elapsedTime, absoluteClickTargetTime);
        }
        else if (noteType == MissileNoteType.Long1Beat ||
                 noteType == MissileNoteType.Long2Beat ||
                 noteType == MissileNoteType.Long3Beat)
        {
            ProcessLongClickDown(elapsedTime, absoluteClickTargetTime);
        }
    }

    private void ProcessShortClick(float elapsedTime, float targetTime)
    {
        if (isReturningToPool || isForceReturning) return;

        float timingDifference = elapsedTime - targetTime;
        bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

        if (inClickWindow)
        {
            TriggerSuccessEvent();
        }
        else
        {
            TriggerFailEvent();
        }
    }

    private void ProcessLongClickDown(float elapsedTime, float targetTime)
    {
        if (isReturningToPool || isForceReturning) return;

        float timingDifference = elapsedTime - targetTime;
        bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

        if (inClickWindow)
        {
            SetHoldingColor();

            if (inputManager != null)
            {
                inputManager.StartLongPress(targetTime);
            }
        }
        else
        {
            TriggerFailEvent();
        }
    }

    private void OnDestroy()
    {
        if (uiManager != null)
        {
            GameObject container = uiManager.GetUIContainer();
            if (container != null)
            {
                Destroy(container);
            }
        }
    }

    private void ResetComponentState()
    {
        ValidateSettings();

        isSuccessful = false;
        isFailed = false;
        isReturningToPool = false;
        isForceReturning = false;

        if (missileRenderer != null)
        {
            missileRenderer.material.color = normalColor;
        }

        CreateManagers();
    }

    private void CreateManagers()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        uiManager = new MissileUIManager(this, targetCanvas, imageSize, layer1Alpha, layer2Alpha);
        animManager = new MissileAnimationManager(this, uiManager);
        inputManager = new MissileInputManager(this, clickGracePeriod, oneBeat);

        float tA = 0.3f, tB = 0.6f, tC = 1.0f;
        if (MicInput_Criware.Instance != null && MicInput_Criware.Instance.volumeThresholds != null
            && MicInput_Criware.Instance.volumeThresholds.Length >= 3)
        {
            tA = MicInput_Criware.Instance.volumeThresholds[0];
            tB = MicInput_Criware.Instance.volumeThresholds[1];
            tC = MicInput_Criware.Instance.volumeThresholds[2];
        }

        micInputManager = new MissileMicInputManager(this, oneBeat, micCheckpointCount, tA, tB, tC);
    }

    private void ValidateSettings()
    {
        if ((int)noteType < 0 || (int)noteType > 5)
        {
            noteType = MissileNoteType.Short;
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

            if (animManager != null)
            {
                animManager.UpdateFrameDelays(frameDelay);
            }

            if (inputManager != null)
            {
                inputManager.UpdateTempoInfo(oneBeat, noteType);
            }

            // micInputManagerはインスタンスを再生成せずoneBeatのみ更新
            if (micInputManager != null)
            {
                micInputManager.UpdateOneBeat(oneBeat);
            }
        }
        else
        {
            oneBeat = 0.4f;
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

        if (screenPos.z < 0)
        {
            uiManager.SetUIActive(false);
            return;
        }
        else
        {
            uiManager.SetUIActive(true);

            screenPos.x += screenOffset.x;
            screenPos.y += screenOffset.y;

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

        isSuccessful = false;
        isFailed = false;
        isReturningToPool = false;
        isForceReturning = false;

        if (missileRenderer != null)
        {
            missileRenderer.material.color = normalColor;
        }

        if (inputManager != null)
        {
            inputManager.Reset();
        }

        if (micInputManager != null)
        {
            micInputManager.Reset();
        }

        StopAllCoroutines();
        activeCoroutines.Clear();
    }

    public void TriggerSuccessEvent()
    {
        if (isReturningToPool || isForceReturning) return;

        isSuccessful = true;
        isFailed = false;

        GameObject obj = ObjectPoolerXyloOther.Instance.SpawnFromPool("SuccessGhostDown", transform.position, Quaternion.identity);
        MissGhostAttack missGhostAttack = obj.GetComponent<MissGhostAttack>();
        if (missGhostAttack != null)
        {
            missGhostAttack.InitSuccess();
        }
        else
        {
            Debug.LogError("MissGhostAttack component not found on spawned object!");
        }

        if (returnToPoolWhenDone)
        {
            StartManagedCoroutine(ReturnToPoolWithDelay(successReturnDelay));
        }
    }

    public void TriggerFailEvent()
    {
        if (isReturningToPool || isForceReturning) return;

        Debug.Log("失敗");

        GameObject obj = ObjectPoolerXyloOther.Instance.SpawnFromPool("MissGhostAttack", transform.position, Quaternion.identity);

        MissGhostAttack missGhostAttack = obj.GetComponent<MissGhostAttack>();
        if (missGhostAttack != null)
        {
            missGhostAttack.InitFailed(noteType);
        }
        else
        {
            Debug.LogError("MissGhostAttack component not found on spawned object!");
        }

        isSuccessful = false;
        isFailed = true;

        if (returnToPoolWhenDone)
        {
            StartManagedCoroutine(ReturnToPoolWithDelay(failReturnDelay));
        }
    }

    public void TriggerNoInputFailEvent()
    {
        if (isReturningToPool || isForceReturning) return;

        isSuccessful = false;
        isFailed = true;

        GameObject obj = ObjectPoolerXyloOther.Instance.SpawnFromPool("MissGhostAttack", transform.position, Quaternion.identity);

        MissGhostAttack missGhostAttack = obj.GetComponent<MissGhostAttack>();
        if (missGhostAttack != null)
        {
            missGhostAttack.InitFailed(noteType);
        }
        else
        {
            Debug.LogError("MissGhostAttack component not found on spawned object!");
        }

        if (returnToPoolWhenDone)
        {
            StartManagedCoroutine(ReturnToPoolWithDelay(failReturnDelay));
        }
    }

    private void ChangeObjectColor(Color newColor)
    {
        if (missileRenderer == null) return;
        if (isReturningToPool || isForceReturning) return;

        StartManagedCoroutine(ColorChangeCoroutine(newColor));
    }

    private IEnumerator ColorChangeCoroutine(Color targetColor)
    {
        if (missileRenderer == null) yield break;

        Color startColor = missileRenderer.material.color;
        float elapsedTime = 0f;

        while (elapsedTime < colorChangeDuration && !isReturningToPool && !isForceReturning)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / colorChangeDuration);
            missileRenderer.material.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        if (!isReturningToPool && !isForceReturning && missileRenderer != null)
        {
            missileRenderer.material.color = targetColor;
        }
    }

    public void TempoMethod()
    {
        if (isReturningToPool || isForceReturning) return;

        if (!isInitialized)
        {
            UpdateTempoInfo();
            if (!isInitialized) return;
        }

        if (animManager == null) return;
        if (uiManager == null || !uiManager.IsUIActive()) return;

        animManager.HandleTempoTick(noteType);

        if (animManager.IsAnimationCompleted() && !isSuccessful && !isFailed
            && returnToPoolWhenDone && !isReturningToPool && !isForceReturning)
        {
            StartManagedCoroutine(ReturnToPoolWithDelay(0f));
        }
    }

    public IEnumerator ReturnToPoolWithDelay(float delay)
    {
        if (!isReturningToPool && !isForceReturning)
        {
            isReturningToPool = true;

            foreach (Coroutine coroutine in activeCoroutines.ToArray())
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            activeCoroutines.Clear();

            yield return new WaitForSeconds(delay);

            CleanupBeforePoolReturn();
            gameObject.SetActive(false);
        }
    }

    private void CleanupBeforePoolReturn()
    {
        if (animManager != null)
        {
            animManager.SetAllLayersInvisible();
            animManager.StopAllAnimations();
        }

        if (missileRenderer != null)
        {
            missileRenderer.material.color = normalColor;
        }

        HomingObject homingComponent = GetComponent<HomingObject>();
        if (homingComponent != null)
        {
            transform.localScale = homingComponent.InitialScale;
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        if (uiManager != null)
        {
            // SpectrumCircleを非表示に
            uiManager.SetSpectrumCircleActive(false);

            uiManager.SetUIActive(false);

            GameObject container = uiManager.GetUIContainer();
            if (container != null)
            {
                container.SetActive(false);
            }
        }
    }

    private IEnumerator SafetyTimer()
    {
        float maxLifetime = 10f;
        yield return new WaitForSeconds(maxLifetime);

        if (gameObject.activeInHierarchy && !isReturningToPool && !isForceReturning)
        {
            Debug.LogWarning($"{gameObject.name}: 安全タイマーによる強制返却を実行します（{maxLifetime}秒経過）");
            ForceReturnToPool();
        }
    }

    public void ForceReturnToPool()
    {
        if (isReturningToPool || isForceReturning) return;

        isForceReturning = true;

        StopAllCoroutines();
        activeCoroutines.Clear();

        CleanupBeforePoolReturn();

        MissileObjectPooler pooler = FindAnyObjectByType<MissileObjectPooler>();
        if (pooler != null)
        {
            int estimatedId = 1;
            string objName = gameObject.name.ToLower();

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
            gameObject.SetActive(false);
        }
    }

    #endregion

    #region 公開プロパティとメソッド

    public Canvas GetTargetCanvas() => targetCanvas;

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
            case MissileAnimationType.None:
            default:
                return new Sprite[0];
        }
    }

    public float GetHitFrameDelay() => hitFrameDelay;
    public float GetNormalFrameDelay() => frameDelay;
    public float GetOneBeat() => oneBeat;
    public MissileNoteType GetNoteType() => noteType;

    public bool IsPointerOverUI() => uiManager != null && uiManager.IsPointerOverUI();
    public void SetHoldingColor() => ChangeObjectColor(holdingColor);

    public bool IsClickDetectionEnabled() => enableClickDetection;
    public bool IsSuccessful() => isSuccessful;
    public bool IsFailed() => isFailed;

    #endregion
}