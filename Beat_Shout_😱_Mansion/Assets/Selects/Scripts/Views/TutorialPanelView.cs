using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mains.Commons;
using Mains.External;
using Mains.Views;
using R3;
using R3.Triggers;
using Rewired;
using Selects.Commons;
using Selects.ViewModels;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Views
{
    /// <summary>
    /// チュートリアルパネルのビュー。
    /// 責務：UI操作（フェード・メッセージ・アイコンアニメ）のみ。
    /// フロー制御は TutorialSequencer へ委譲する。
    /// </summary>
    public class TutorialPanelView : MonoBehaviour, ITutorialUI
    {
        /// <summary>チュートリアルパネルの設定</summary>
        [SerializeField] private TutorialPanelSettings settings;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>プレイヤーの視線ヒット対象</summary>
        private RaycastHit[] _hitsPlayerAimToAny = new RaycastHit[10]; // 適当なバッファサイズ;
        /// <summary>ITutorialSideEffect の Script_xyloApi 実装</summary>
        private XyloApiTutorialSideEffect _sideEffect;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        // ------------------------------------------------------------------
        // ITutorialUI 実装
        // ------------------------------------------------------------------

        public void ApplyMessage(string messageId)
        {
            var msgData = settings.tables.messageTable.Get(messageId);
            if (msgData == null) return;
            SetMainMessage(msgData.mainMessage);
            SetSubMessage(msgData.subMessage);
            SetGuideText(msgData.progressText);
            SetIsInteractIconVisible(msgData.hasInteract);
        }

        public void ApplyMessageWithProgress(string messageId, string current, string total)
        {
            var msgData = settings.tables.messageTable.Get(messageId);
            if (msgData == null) return;
            SetMainMessage(msgData.mainMessage);
            SetSubMessage(msgData.subMessage);
            SetGuideText(string.IsNullOrEmpty(msgData.progressText) ? "" :
                msgData.progressText
                    .Replace("${ghostExitMembersCount}", current)
                    .Replace("${ghostAllMembersCount}", total));
            SetIsInteractIconVisible(msgData.hasInteract);
        }

        public void ResetMessages()
        {
            SetMainMessage("");
            SetSubMessage("");
            SetGuideText("");
            SetIsInteractIconVisible(false);
        }

        public async UniTask FadeInAsync(float duration, CancellationToken token)
        {
            var ui = settings.tutorialUIPanels;
            if (ui.tutorialPanelCanvasGroup != null)
            {
                ui.tutorialPanelCanvasGroup.gameObject.SetActive(true);
                ui.tutorialPanelCanvasGroup.alpha = 1f;
            }

            var seq = DOTween.Sequence();
            if (ui.mainMessagePanelCanvasGroup != null && ui.mainMessagePanelCanvasGroup.gameObject.activeSelf)
                _ = seq.Join(ui.mainMessagePanelCanvasGroup.DOFade(1f, duration));
            if (ui.subMessageCanvasGroup != null && ui.subMessageCanvasGroup.gameObject.activeSelf)
                _ = seq.Join(ui.subMessageCanvasGroup.DOFade(1f, duration));
            if (ui.guideTextCanvasGroup != null && ui.guideTextCanvasGroup.gameObject.activeSelf)
                _ = seq.Join(ui.guideTextCanvasGroup.DOFade(1f, duration));

            await seq.ToUniTask(cancellationToken: token);
        }

        public async UniTask FadeOutAsync(float duration, CancellationToken token)
        {
            var ui = settings.tutorialUIPanels;
            var seq = DOTween.Sequence();
            if (ui.mainMessagePanelCanvasGroup != null && ui.mainMessagePanelCanvasGroup.gameObject.activeSelf)
                _ = seq.Join(ui.mainMessagePanelCanvasGroup.DOFade(0f, duration));
            if (ui.subMessageCanvasGroup != null && ui.subMessageCanvasGroup.gameObject.activeSelf)
                _ = seq.Join(ui.subMessageCanvasGroup.DOFade(0f, duration));
            if (ui.guideTextCanvasGroup != null && ui.guideTextCanvasGroup.gameObject.activeSelf)
                _ = seq.Join(ui.guideTextCanvasGroup.DOFade(0f, duration));

            await seq.ToUniTask(cancellationToken: token);
        }

        // ------------------------------------------------------------------
        // Unity ライフサイクル
        // ------------------------------------------------------------------

        private void Reset()
        {
            var uIPanels = settings.tutorialUIPanels;
            if (uIPanels.tutorialPanelCanvasGroup == null)
                uIPanels.tutorialPanelCanvasGroup = GetComponent<CanvasGroup>();
            foreach (Transform child in transform)
            {
                if (child.name.Equals("MainMessagePanel"))
                {
                    if (uIPanels.mainMessagePanelCanvasGroup == null)
                        uIPanels.mainMessagePanelCanvasGroup = child.GetComponent<CanvasGroup>();
                    foreach (Transform item in child)
                    {
                        if (item.name.Equals("MainMessageText"))
                        {
                            if (uIPanels.mainMessageText == null)
                                uIPanels.mainMessageText = item.GetComponent<TextMeshProUGUI>();
                        }
                        if (item.name.Equals("InteractIconImage"))
                        {
                            if (uIPanels.interactIconImage == null)
                                uIPanels.interactIconImage = item.GetComponent<InteractIconImageView>();
                        }
                    }
                }
                if (child.name.Equals("SubMessagePanel"))
                {
                    if (uIPanels.subMessageCanvasGroup == null)
                        uIPanels.subMessageCanvasGroup = child.GetComponent<CanvasGroup>();
                    foreach (Transform item in child)
                    {
                        if (item.name.Equals("SubMessageText"))
                        {
                            if (uIPanels.subMessageText == null)
                                uIPanels.subMessageText = item.GetComponent<TextMeshProUGUI>();
                        }
                    }
                }
                if (child.name.Equals("GuidePanel"))
                {
                    if (uIPanels.guideTextCanvasGroup == null)
                        uIPanels.guideTextCanvasGroup = child.GetComponent<CanvasGroup>();
                    foreach (Transform item in child)
                    {
                        if (item.name.Equals("GuideText"))
                        {
                            if (uIPanels.guideText == null)
                                uIPanels.guideText = item.GetComponent<TextMeshProUGUI>();
                        }
                    }
                }
            }
        }

        private void OnValidate()
        {
            var set = settings;
            PreviewMessages(set);
        }

        private void Start()
        {
            InitializeUIState();

            var set = settings;
            set.viewModel.Initialize();
            var viewModel = set.viewModel;
            var levelObjects = set.levelObjects;
            var details = set.details;

            // 懐中電灯トリガーの接触監視
            var flashLightTrigger = levelObjects.flashLightItemTrigger;
            if (flashLightTrigger != null)
            {
                flashLightTrigger.OnTriggerStayAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetFlashLightTriggerStay(true);
                    })
                    .AddTo(ref _disposableBag);
                flashLightTrigger.OnTriggerExitAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetFlashLightTriggerStay(false);
                    })
                    .AddTo(ref _disposableBag);
            }

            // 電池トリガーの接触監視
            var batteryTrigger = levelObjects.batteryItemTrigger;
            if (batteryTrigger != null)
            {
                batteryTrigger.OnTriggerStayAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetBatteryTriggerStay(true);
                    })
                    .AddTo(ref _disposableBag);
                batteryTrigger.OnTriggerExitAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetBatteryTriggerStay(false);
                    })
                    .AddTo(ref _disposableBag);
            }

            // 光のリング2トリガーの接触監視
            var lightRing2Trigger = levelObjects.lightRingParticleSys2Trigger;
            if (lightRing2Trigger != null)
            {
                lightRing2Trigger.OnTriggerStayAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetLightRing2TriggerStay(true);
                    })
                    .AddTo(ref _disposableBag);
                lightRing2Trigger.OnTriggerExitAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetLightRing2TriggerStay(false);
                    })
                    .AddTo(ref _disposableBag);
            }

            // 1階の右階段トリガーの接触監視
            var rightStairsTrigger1F = levelObjects.rightStairsTrigger1F;
            if (rightStairsTrigger1F != null)
            {
                rightStairsTrigger1F.OnTriggerStayAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetRightStairsTrigger1FStay(true);
                    })
                    .AddTo(ref _disposableBag);
                rightStairsTrigger1F.OnTriggerExitAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetRightStairsTrigger1FStay(false);
                    })
                    .AddTo(ref _disposableBag);
            }

            // 2階の左階段トリガー接触監視
            var leftStairsTrigger2F = levelObjects.leftStairsTrigger2F;
            if (leftStairsTrigger2F != null)
            {
                leftStairsTrigger2F.OnTriggerStayAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetLeftStairsTrigger2FStay(true);
                    })
                    .AddTo(ref _disposableBag);
                leftStairsTrigger2F.OnTriggerExitAsObservable()
                    .Subscribe(_ =>
                    {
                        viewModel.SetLeftStairsTrigger2FStay(false);
                    })
                    .AddTo(ref _disposableBag);
            }

            // プレイヤーの頭
            Transform headTrans = null;
            var batteryItemAimRangeTrigger = levelObjects.batteryItemAimRangeTrigger;
            var missGhostEscapeNormalAimRangeTrigger = levelObjects.missGhostEscapeNormalAimRangeTrigger;
            var vaseAndDeskGroupAimRangeTrigger = levelObjects.vaseAndDeskGroupAimRangeTrigger;
            PlayerView playerView = null;
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    // 目線の距離
                    var aimDistance = 0f;
                    headTrans = viewModel.PlayerHead;
                    if (playerView == null)
                    {
                        var view = viewModel.PlayerTransform?.GetComponent<PlayerView>();
                        if (view != null)
                        {
                            playerView = view;
                        }
                    }
                    else
                    {
                        aimDistance = playerView.PlayerTable.aimDistance;
                    }
                    if (headTrans != null)
                    {
                        // プレイヤーの視線が電池を捉える
                        if (batteryItemAimRangeTrigger != null)
                        {
                            var isHit = IsHitPlayerAimToAny(headTrans, batteryItemAimRangeTrigger.transform, aimDistance);
                            viewModel.SetBatteryHitPlayerAim(isHit);
                        }
                        // プレイヤーの視線が移動オバケ（ノーマル）を捉える
                        if (missGhostEscapeNormalAimRangeTrigger != null)
                        {
                            var isHit = IsHitPlayerAimToAny(headTrans, missGhostEscapeNormalAimRangeTrigger.transform, aimDistance);
                            viewModel.SetMissGhostEscapeNormalHitPlayerAim(isHit);
                        }
                        // プレイヤーの視線が花瓶と机を捉える
                        if (vaseAndDeskGroupAimRangeTrigger != null)
                        {
                            var isHit = IsHitPlayerAimToAny(headTrans, vaseAndDeskGroupAimRangeTrigger.transform, aimDistance);
                            viewModel.SetVaseAndDeskGroupHitPlayerAim(isHit);
                        }
                    }
                })
                .AddTo(ref _disposableBag);

            // 依存オブジェクトを組み立てて Sequencer に渡す
            var rewiredPlayer = ReInput.players.GetPlayer(0);
            _script_XyloApi = new Script_xyloApi();
            _sideEffect = new XyloApiTutorialSideEffect(_script_XyloApi, set.tables.playerTeleporterStrategySOsLink.PlayerTeleporterStrategy);

            var context = new TutorialSequencerContext(
                ui:          this,                                  // ITutorialUI = 自分自身
                input:       new RewiredTutorialInput(rewiredPlayer), // ITutorialInput
                sideEffect: _sideEffect,  // ITutorialSideEffect
                viewModel:   set.viewModel,
                levelObjects: set.levelObjects,
                uiObjects:   set.uIObjects,
                tables:       set.tables
            );

            var sequencer = new TutorialSequencer(context);

            ResourcesUtility utility = new ResourcesUtility();
            var userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);

            sequencer.RunAsync(userBean, this.GetCancellationTokenOnDestroy())
                .Forget();

            // xyloApiのDisposeはOnDestroyで行う（Sequencer外で管理）
            // 必要であれば DisposableBag に追加する
        }

        /// <summary>
        /// プレイヤーの視線が対象オブジェクトを捉えたか
        /// </summary>
        /// <param name="headTrans">プレイヤーの頭</param>
        /// <param name="target">対象オブジェクト</param>
        /// <param name="aimDistance">目線の距離</param>
        /// <returns>対象オブジェクトを捉えたか</returns>
        public bool IsHitPlayerAimToAny(Transform headTrans, Transform target, float aimDistance)
        {
            Vector3 origin = headTrans.position; // 目線の高さ
            Vector3 direction = headTrans.forward;
            int layerMaskAimRange = 1 << LayerMask.NameToLayer("AimRange");

            // デバッグ：Sceneビューに赤線を描画
            Debug.DrawRay(origin, direction * aimDistance, Color.red);

            int hitCount = Physics.RaycastNonAlloc(origin, direction, _hitsPlayerAimToAny, aimDistance, layerMaskAimRange);
            if (0 < hitCount)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    var hit = _hitsPlayerAimToAny[i];
                    if (hit.collider != null && hit.collider.transform.Equals(target))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            settings.viewModel.Dispose();
            _script_XyloApi?.Dispose();
            _sideEffect?.Dispose();
        }

        // ------------------------------------------------------------------
        // UI 内部操作（private）
        // ------------------------------------------------------------------

        /// <summary>
        /// メッセージのプレビュー
        /// </summary>
        /// <param name="set">チュートリアルパネルの設定</param>
        private void PreviewMessages(TutorialPanelSettings set)
        {
            var messageId = set.preview.messageId;
            // メッセージIDが空なら以降は実行しない
            if (string.IsNullOrEmpty(messageId))
                return;

            var msgData = set.tables.messageTable.Get(messageId);
            if (msgData == null)
            {
                Debug.LogWarning($"メッセージIDに該当するメッセージデータを取得できませんでした: [{messageId}]");

                return;
            }
            var isProgressText = !string.IsNullOrEmpty(msgData.progressText);
            if (!isProgressText)
            {
                ApplyMessage(messageId);
            }
            else
            {
                ApplyMessageWithProgress(messageId, "0", "5");
            }
        }

        /// <summary>
        /// UIの状態を初期化
        /// </summary>
        private void InitializeUIState()
        {
            var ui = settings.tutorialUIPanels;

            void InitCanvasGroup(CanvasGroup cg)
            {
                if (cg == null) return;
                cg.alpha = 0f;
                cg.gameObject.SetActive(false);
            }

            InitCanvasGroup(ui.tutorialPanelCanvasGroup);
            InitCanvasGroup(ui.mainMessagePanelCanvasGroup);
            InitCanvasGroup(ui.subMessageCanvasGroup);
            InitCanvasGroup(ui.guideTextCanvasGroup);

            if (ui.interactIconImage != null)
                ui.interactIconImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// メインメッセージを設定
        /// </summary>
        /// <param name="message">メインメッセージ</param>
        private void SetMainMessage(string message)
        {
            var ui = settings.tutorialUIPanels;
            if (ui.mainMessageText != null) ui.mainMessageText.text = message;
            if (ui.mainMessagePanelCanvasGroup != null)
                ui.mainMessagePanelCanvasGroup.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        /// <summary>
        /// サブメッセージを設定
        /// </summary>
        /// <param name="message">サブメッセージ</param>
        private void SetSubMessage(string message)
        {
            var ui = settings.tutorialUIPanels;
            if (ui.subMessageText != null) ui.subMessageText.text = message;
            if (ui.subMessageCanvasGroup != null)
                ui.subMessageCanvasGroup.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        /// <summary>
        /// 進捗テキストを設定
        /// </summary>
        /// <param name="message">進捗テキスト</param>
        private void SetGuideText(string message)
        {
            var ui = settings.tutorialUIPanels;
            if (ui.guideText != null) ui.guideText.text = message;
            if (ui.guideTextCanvasGroup != null)
                ui.guideTextCanvasGroup.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        /// <summary>
        /// インタラクトアイコン表示フラグを設定
        /// </summary>
        /// <param name="isVisible">インタラクトアイコン表示フラグ</param>
        private void SetIsInteractIconVisible(bool isVisible)
        {
            var icon = settings.tutorialUIPanels.interactIconImage;
            if (icon == null) return;
            icon.gameObject.SetActive(isVisible);
        }
    }

    /// <summary>
    /// チュートリアルパネルの設定
    /// </summary>
    [System.Serializable]
    public class TutorialPanelSettings
    {
        /// <summary>チュートリアルパネルのビューモデル</summary>
        public TutorialPanelViewModel viewModel;
        /// <summary>プレビュー用パラメータ</summary>
        public Preview preview;
        /// <summary>チュートリアルインタラクションガイドUI</summary>
        public TutorialUIPanels tutorialUIPanels;
        /// <summary>レベル内のチュートリアル専用オブジェクト</summary>
        public LevelObjects levelObjects;
        /// <summary>UIオブジェクト</summary>
        public UIObjects uIObjects;
        /// <summary>テーブル情報</summary>
        public Tables tables;
        /// <summary>詳細パラメータ</summary>
        public Details details;

        /// <summary>
        /// チュートリアルインタラクションガイドUI
        /// </summary>
        [System.Serializable]
        public class TutorialUIPanels
        {
            /// <summary>チュートリアルパネルのキャンバスグループ</summary>
            public CanvasGroup tutorialPanelCanvasGroup;
            /// <summary>メインメッセージパネルのキャンバスグループ</summary>
            public CanvasGroup mainMessagePanelCanvasGroup;
            /// <summary>メインメッセージテキスト</summary>
            public TextMeshProUGUI mainMessageText;
            /// <summary>インタラクトアイコンイメージ</summary>
            public InteractIconImageView interactIconImage;
            /// <summary>サブメッセージパネルのキャンバスグループ</summary>
            public CanvasGroup subMessageCanvasGroup;
            /// <summary>サブメッセージテキスト</summary>
            public TextMeshProUGUI subMessageText;
            /// <summary>進捗テキストキャンバスグループ</summary>
            public CanvasGroup guideTextCanvasGroup;
            /// <summary>進捗テキスト</summary>
            public TextMeshProUGUI guideText;
        }

        /// <summary>
        /// レベル内のチュートリアル専用オブジェクト
        /// </summary>
        [System.Serializable]
        public class LevelObjects
        {
            /// <summary>光のリングエフェクト</summary>
            public GameObject lightRingParticleSys;
            /// <summary>光のリングエフェクト1</summary>
            public GameObject lightRingParticleSys1;
            /// <summary>光のリングエフェクト2</summary>
            public GameObject lightRingParticleSys2;
            /// <summary>光のリングエフェクト2トリガー</summary>
            public Collider lightRingParticleSys2Trigger;
            /// <summary>落ちている懐中電灯</summary>
            public GameObject flashLightItem;
            /// <summary>落ちている懐中電灯トリガー</summary>
            public Collider flashLightItemTrigger;
            /// <summary>落ちている電池</summary>
            public GameObject batteryItem;
            /// <summary>落ちている電池トリガー</summary>
            public Collider batteryItemTrigger;
            /// <summary>落ちている電池トリガー（視線補足用）</summary>
            public Collider batteryItemAimRangeTrigger;
            /// <summary>移動完了ポイント</summary>
            public Transform moveCompletePoint;
            /// <summary>移動完了ポイント</summary>
            public Transform aimMoveCompletePoint;
            /// <summary>移動オバケ（ノーマル）</summary>
            public GameObject missGhostEscapeNormal;
            /// <summary>移動オバケ（ノーマル）トリガー（視線補足用）</summary>
            public Collider missGhostEscapeNormalAimRangeTrigger;
            /// <summary>移動オバケ（ノーマル）アニメータ</summary>
            public Animator missGhostEscapeNormalAnimator;
            /// <summary>移動オバケ（ノーマル）移動用オバケのビュー</summary>
            public MissGhostEscapeView missGhostEscapeView;
            /// <summary>花瓶と机</summary>
            public GameObject vaseAndDeskGroup;
            /// <summary>花瓶と机トリガー（視線補足用）</summary>
            public Collider vaseAndDeskGroupAimRangeTrigger;
            /// <summary>1階の右階段トリガー</summary>
            public Collider rightStairsTrigger1F;
            /// <summary>2階の左階段トリガー</summary>
            public Collider leftStairsTrigger2F;
        }

        /// <summary>
        /// UIオブジェクト
        /// </summary>
        [System.Serializable]
        public class UIObjects
        {
            /// <summary>フェードイメージのビュー</summary>
            public FadeImageView fadeImageView;
        }

        /// <summary>
        /// テーブル情報
        /// </summary>
        [System.Serializable]
        public class Tables
        {
            /// <summary>メッセージの設定</summary>
            public MessageTable messageTable;
            /// <summary>ノーツ生成パターン制御</summary>
            public MissilePatternTable missilePatternTable;
            /// <summary>プレイヤー移動演出ストラテジーの設定を同期管理させる</summary>
            public PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLink;
            /// <summary>シャウトチャンスパートの共通パラメータ管理用テーブル</summary>
            public PlayerShoutChanceTable playerShoutChanceTable;
        }

        /// <summary>
        /// 詳細パラメータ
        /// </summary>
        /// <remarks>調整用のパラメータ<br/>
        /// 確定次第ハードコード</remarks>
        [System.Serializable]
        public class Details
        {
            ///// <summary>目線の距離</summary>
            //public float aimDistance = 10f;
        }

        /// <summary>
        /// プレビュー用パラメータ
        /// </summary>
        [System.Serializable]
        public class Preview
        {
            /// <summary>メッセージID</summary>
            /// <remarks>対象IDはseeを参照</remarks>
            /// <see cref="Assets/Selects/Scripts/Commons/MessageTable.asset"/>
            public string messageId;
        }
    }
}
