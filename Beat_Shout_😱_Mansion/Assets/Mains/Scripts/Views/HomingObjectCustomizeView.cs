using Mains.Commons;
using Mains.External;
using Mains.ViewModels;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mains.Views
{
    /// <summary>
    /// オブジェクトをホーミングする処理のカスタマイズビュー
    /// </summary>
	public class HomingObjectCustomizeView : MonoBehaviour
	{
        [Tooltip("Assets/Mains/Prefabs/UIs/GoodPanel.prefabをセットしておく。")]
        [SerializeField] private Transform goodPanelPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/GoodParticleSys.prefabをセットしておく。")]
        [SerializeField] private Transform goodParticleSysPrefab;
        [Tooltip("Assets/Mains/Prefabs/UIs/BadPanel.prefabをセットしておく。")]
        [SerializeField] private Transform badPanelPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/BadParticleSys.prefabをセットしておく。")]
        [SerializeField] private Transform badParticleSysPrefab;
        [Tooltip("オバケの3Dモデルレンダラー GhostBodyModel/ghost_normal")]
        [SerializeField] private Renderer modelRenderer;
        [Tooltip("Assets/Mains/Prefabs/UIs/GuideMessageLongNotesPanel.prefabをセットしておく。")]
        [SerializeField] private Transform guideMessageLongNotesPanelPrefab;
        [Tooltip("Assets/Mains/Prefabs/UIs/GuideMessageShortNotesPanel.prefabをセットしておく。")]
        [SerializeField] private Transform guideMessageShortNotesPanelPrefab;
        /// <summary>オブジェクトをホーミングする処理のカスタマイズ設定</summary>
        [SerializeField] private HomingObjectCustomizeSettins settins;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>オブジェクトをホーミングする処理のカスタマイズビューモデル</summary>
        private HomingObjectCustomizeViewModel _viewModel;
        private string _guideMessageShortNote;
        private Transform _canvasTransform;
        /// <summary>生成されたショートノーツのメッセージ</summary>
        private readonly ReactiveProperty<Transform> _guideMessageShortNotesPanelInstance = new ReactiveProperty<Transform>();
        /// <summary>生成されたロングノーツのメッセージ</summary>
        private readonly ReactiveProperty<Transform> _guideMessageLongNotesPanelInstance = new ReactiveProperty<Transform>();
        /// <summary>初期処理の完了フラグ</summary>
        private ReactiveProperty<bool> _isCompletedStart = new ReactiveProperty<bool>();
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (modelRenderer == null)
                modelRenderer = transform.GetChild(0).GetChild(0).GetComponent<Renderer>();
        }

        private void Awake()
        {
            _script_XyloApi = new Script_xyloApi();
            _viewModel = new HomingObjectCustomizeViewModel();
            this.OnEnableAsObservable()
                .Subscribe(_ =>
                {
                    float dice = Random.value; // 0.0〜1.0 の乱数
                                               // 30%の確率で再生
                    if (dice <= .3f)
                    {
                        // ボイスタイプに応じた笑い声SEを再生
                        _script_XyloApi.PlayGhostLaughByVoiceType(_viewModel.CurrentGhostVoiceType);
                    }
                    // モデルタイプに応じたFBX表示切替
                    SwitchGhostModel(_viewModel.CurrentGhostModelType);
                })
                .AddTo(ref _disposableBag);
        }

        private void Start()
		{
            var set = settins;
            Transform trans = transform;
            var canvasTransform = GameObject.Find("OverlayCanvas").transform;
            var api = _script_XyloApi;
            _script_XyloApi.SetMissileDirectAnimManagerB(trans);
            // goodPanelPrefabとgoodParticleSysPrefabのインスタンス情報
            (GoodPanelView, ParticleSystem) instancesGoodContents = (null, null);
            // ノーツタイプ管理
            int notesType = set.notesType;
            var homingObjectCustomizeTable = set.homingObjectCustomizeTable;
            var currentScneName = SceneManager.GetActiveScene().name;
            var guideMessageLongNotes = homingObjectCustomizeTable.GetGuideMessageLongNote(currentScneName);
            var guideMessageShortNote = homingObjectCustomizeTable.GetGuideMessageShortNote(currentScneName);
            ReactiveProperty<Transform> guideMessageLongNotesPanelInstance = _guideMessageLongNotesPanelInstance;
            ReactiveProperty<Transform> guideMessageShortNotesPanelInstance = _guideMessageShortNotesPanelInstance;
            _script_XyloApi.IsSuccessfulReactive.Where(x => x)
                .Subscribe(_ =>
                {
                    var instances = ShowUIPanelAtObjectPosition(goodPanelPrefab, canvasTransform, goodParticleSysPrefab, _script_XyloApi.NoteTransform, trans);
                    instancesGoodContents.Item1 = instances.Item1.GetComponent<GoodPanelView>();
                    instancesGoodContents.Item2 = instances.Item2.GetComponent<ParticleSystem>();
                    DoSubtractionTransactionGhostInStaticObjectStruct(_viewModel);
                    DoSetOnNoteSuccessful(_viewModel);
                    _script_XyloApi.PlayHitSuccess3(notesType);
                })
                .AddTo(ref _disposableBag);
            _script_XyloApi.IsFailedReactive.Where(x => x &&
                gameObject.activeSelf)
                .Subscribe(_ =>
                {
                    ShowUIPanelAtObjectPosition(badPanelPrefab, canvasTransform, badParticleSysPrefab, _script_XyloApi.NoteTransform, trans);
                    _script_XyloApi.PlayHitMiss3();
                    DoSetOnNoteFailed(_viewModel);
                })
                .AddTo(ref _disposableBag);
            _script_XyloApi.IsFailedReactive.Where(_ => gameObject.activeSelf)
                .Subscribe(isFailed =>
                {
                    _viewModel.SetIsFailed(isFailed);
                })
                .AddTo(ref _disposableBag);
            // クリア済みなら再生中の演出を無効にする
            _viewModel.IsMissionClear.Where(x => x)
                .Subscribe(_ =>
                {
                    Observable.EveryUpdate()
                        .Select(_ => instancesGoodContents.Item1)
                        .Where(x => x != null &&
                            x.gameObject.activeSelf &&
                            x.IsPlaying)
                        .Take(1)
                        .Subscribe(view =>
                        {
                            view.StopAnimation();
                        })
                        .AddTo(ref _disposableBag);
                    Observable.EveryUpdate()
                        .Select(_ => instancesGoodContents.Item2)
                        .Where(x => x != null &&
                            x.gameObject.activeSelf &&
                                (x.isPlaying || x.IsAlive()))
                        .Take(1)
                        .Subscribe(particleSys =>
                        {
                            particleSys.gameObject.SetActive(false);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // Script_xyloApi.耐久率UI表示フラグが有効の場合、DurabilityRatePanelプレハブのクローンを生成する（子要素として生成）
            _script_XyloApi.DurabilityRateTarget.Subscribe(_ =>
            {
                var table = settins.homingObjectCustomizeTable;
                var noteTransform = _script_XyloApi.NoteTransform as RectTransform;
                RectTransform durabilityRatePanelInstance = ShowUIPanelAtObjectPosition(table.durabilityRatePanelPrefab, canvasTransform, noteTransform, trans);
                DurabilityRatePanelView durabilityRatePanelView = durabilityRatePanelInstance.GetComponent<DurabilityRatePanelView>();
                _script_XyloApi.Score.Pairwise()
                    .Subscribe(score =>
                    {
                        int from = 0 <= score.Previous ? score.Previous : 0;
                        int to = 0 <= score.Current ? score.Current : 0;
                        durabilityRatePanelView.PlayDurabilityAnimation(from, to, .25f);
                    })
                    .AddTo(ref _disposableBag);
                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        var noteAnchoredPosition = noteTransform.anchoredPosition;
                        durabilityRatePanelInstance.anchoredPosition = noteAnchoredPosition;
                        var noteLocalScale = noteTransform.localScale;
                        durabilityRatePanelInstance.localScale = noteLocalScale;
                    })
                    .AddTo(ref _disposableBag);
                noteTransform.OnEnableAsObservable()
                    .Subscribe(_ =>
                    {
                        if (!durabilityRatePanelInstance.gameObject.activeSelf)
                            durabilityRatePanelInstance.gameObject.SetActive(true);
                    })
                    .AddTo(ref _disposableBag);
                noteTransform.OnDisableAsObservable()
                    .Subscribe(_ =>
                    {
                        if (durabilityRatePanelInstance.gameObject.activeSelf)
                            durabilityRatePanelInstance.gameObject.SetActive(false);
                    })
                    .AddTo(ref _disposableBag);
            })
                .AddTo(ref _disposableBag);
            // ロングノーツの押下開始、離すのテキスト表示（シーンごとに表示、非表示を制御）
            if (guideMessageLongNotes.Item1 != null && guideMessageLongNotes.Item2 != null)
            {
                // クリック開始メッセージ
                ReactiveProperty<string> clickStartMessage = new ReactiveProperty<string>(guideMessageLongNotes.Item1);
                // 離すメッセージ
                ReactiveProperty<string> releaseMessage = new ReactiveProperty<string>(guideMessageLongNotes.Item2);
                ReactiveProperty<int> clickStartDirectionMode = new ReactiveProperty<int>(0);
                ReactiveProperty<int> releaseDirectionMode = new ReactiveProperty<int>(1);
                ReactiveProperty<Transform> guideMessageLongNotesPanelPrefab = new ReactiveProperty<Transform>(this.guideMessageLongNotesPanelPrefab);
                var noteTransform = _script_XyloApi.NoteTransform as RectTransform;

                api.IsLongNoteClickable.Where(x => x)
                    .Subscribe(_ =>
                    {
                        ShowGuideMessageLongNotes(true, guideMessageLongNotesPanelPrefab.CurrentValue, canvasTransform, noteTransform,
                            clickStartMessage.CurrentValue, clickStartDirectionMode.CurrentValue, guideMessageLongNotesPanelInstance);
                    })
                    .AddTo(ref _disposableBag);

                api.IsLongNoteRelease.Where(x => x)
                    .Subscribe(_ =>
                    {
                        ShowGuideMessageLongNotes(true, guideMessageLongNotesPanelPrefab.CurrentValue, canvasTransform, noteTransform,
                            releaseMessage.CurrentValue, releaseDirectionMode.CurrentValue, guideMessageLongNotesPanelInstance);
                    })
                    .AddTo(ref _disposableBag);
            }
            // ショートノーツのガイドメッセージ表示
            if (!string.IsNullOrEmpty(guideMessageShortNote))
            {
                ReactiveProperty<string> shortNoteMessage = new ReactiveProperty<string>(guideMessageShortNote);
                ReactiveProperty<Transform> guideMessageShortNotesPanelPrefab = new ReactiveProperty<Transform>(this.guideMessageShortNotesPanelPrefab);
                var noteTransform = _script_XyloApi.NoteTransform as RectTransform;

                api.IsShortNoteClickable.Where(x => x)
                    .Subscribe(_ =>
                    {
                        ShowGuideMessageShortNotes(true, guideMessageShortNotesPanelPrefab.CurrentValue, canvasTransform, noteTransform,
                            shortNoteMessage.CurrentValue, guideMessageShortNotesPanelInstance);
                    })
                    .AddTo(ref _disposableBag);
            }

            _guideMessageShortNote = guideMessageShortNote;
            _canvasTransform = canvasTransform;
            _isCompletedStart.Value = true;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _script_XyloApi?.Dispose();
            _viewModel?.Dispose();
        }

        /// <summary>
        /// ショートノーツのメッセージを表示する処理を呼び出す
        /// </summary>
        public void DoShowGuideMessageShortNotes()
        {
            var isCompletedStart = _isCompletedStart;

            void DoShow()
            {
                var tmpGuideMessageShortNotesPanelPrefab = guideMessageShortNotesPanelPrefab;
                string shortNoteMessage = _guideMessageShortNote;
                var canvasTransform = _canvasTransform;
                var noteTransform = _script_XyloApi.NoteTransform as RectTransform;
                ReactiveProperty<Transform> guideMessageShortNotesPanelInstance = _guideMessageShortNotesPanelInstance;

                // 外部呼び出しの場合はアニメーションが停止しても表示されたまま。2回目以降の呼び出し時に前回の状態が完了状態にされる想定。
                ShowGuideMessageShortNotes(true, tmpGuideMessageShortNotesPanelPrefab, canvasTransform, noteTransform, shortNoteMessage, guideMessageShortNotesPanelInstance,
                    false);
            }

            if (isCompletedStart.Value)
            {
                DoShow();
            }
            else
            {
                isCompletedStart.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        DoShow();
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        /// <summary>
        /// ロングノーツのメッセージを表示する処理を呼び出す
        /// </summary>
        /// <param name="directionMode">向きモード</param>
        public void DoShowGuideMessageLongNotes(int directionMode)
        {
            var isCompletedStart = _isCompletedStart;

            void DoShow()
            {
                var set = settins;
                var homingObjectCustomizeTable = set.homingObjectCustomizeTable;
                var currentScneName = SceneManager.GetActiveScene().name;
                var guideMessageLongNotes = homingObjectCustomizeTable.GetGuideMessageLongNote(currentScneName);
                var tmpGuideMessageLongNotesPanelPrefab = guideMessageLongNotesPanelPrefab;
                string shortNoteMessage = _guideMessageShortNote;
                var canvasTransform = _canvasTransform;
                var noteTransform = _script_XyloApi.NoteTransform as RectTransform;
                // クリック開始メッセージ
                ReactiveProperty<string> clickStartMessage = new ReactiveProperty<string>(guideMessageLongNotes.Item1);
                // 離すメッセージ
                ReactiveProperty<string> releaseMessage = new ReactiveProperty<string>(guideMessageLongNotes.Item2);
                ReactiveProperty<int> clickStartDirectionMode = new ReactiveProperty<int>(0);
                ReactiveProperty<int> releaseDirectionMode = new ReactiveProperty<int>(1);
                ReactiveProperty<Transform> guideMessageLongNotesPanelInstance = _guideMessageLongNotesPanelInstance;

                switch (directionMode)
                {
                    case 1:
                        ShowGuideMessageLongNotes(true, tmpGuideMessageLongNotesPanelPrefab, canvasTransform, noteTransform,
                            releaseMessage.CurrentValue, releaseDirectionMode.CurrentValue, guideMessageLongNotesPanelInstance);

                        break;
                    default:
                        ShowGuideMessageLongNotes(true, tmpGuideMessageLongNotesPanelPrefab, canvasTransform, noteTransform,
                            clickStartMessage.CurrentValue, clickStartDirectionMode.CurrentValue, guideMessageLongNotesPanelInstance,
                            false);

                        break;
                }
            }

            if (isCompletedStart.Value)
            {
                DoShow();
            }
            else
            {
                isCompletedStart.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        DoShow();
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        /// <summary>
        /// オブジェクトをベースにCanvasの座標へ変換して、プレハブをインスタンス
        /// </summary>
        /// <param name="uiPrefab">UIプレハブ</param>
        /// <param name="canvasTransform">キャンバストランスフォーム</param>
        /// <param name="particleSysPrefab">パーティクルプレハブ</param>
        /// <param name="noteTransform">UIトランスフォーム</param>
        /// <param name="trans">トランスフォーム</param>
        private (RectTransform, Transform) ShowUIPanelAtObjectPosition(Transform uiPrefab, Transform canvasTransform, Transform particleSysPrefab, Transform noteTransform, Transform trans)
        {
            // UIを生成
            Transform instance = Instantiate(uiPrefab, canvasTransform);
            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            var noteAnchoredPosition = (noteTransform as RectTransform).anchoredPosition;
            rectTransform.anchoredPosition = noteAnchoredPosition;

            // パーティクルをワールド空間に生成
            // プレハブの元の回転を取得
            Quaternion prefabRotation = particleSysPrefab.rotation;

            // trans（例：UIや他オブジェクト）のY回転だけ適用
            Vector3 euler = prefabRotation.eulerAngles;
            euler.y = trans.rotation.eulerAngles.y;

            // 新しい回転を作成
            Quaternion finalRotation = Quaternion.Euler(euler);
            Transform particleInstance = Instantiate(particleSysPrefab, trans.position, finalRotation);

            (RectTransform, Transform) instances = (rectTransform, particleInstance);
            return instances;
        }

        /// <summary>
        /// オブジェクトをベースにCanvasの座標へ変換して、プレハブをインスタンス
        /// </summary>
        /// <param name="uiPrefab">UIプレハブ</param>
        /// <param name="canvasTransform">キャンバストランスフォーム</param>
        /// <param name="particleSysPrefab">パーティクルプレハブ</param>
        /// <param name="noteTransform">UIトランスフォーム</param>
        /// <param name="trans">トランスフォーム</param>
        private RectTransform ShowUIPanelAtObjectPosition(Transform uiPrefab, Transform canvasTransform, Transform noteTransform, Transform trans)
        {
            // UIを生成
            Transform instance = Instantiate(uiPrefab, canvasTransform);
            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            var noteAnchoredPosition = (noteTransform as RectTransform).anchoredPosition;
            rectTransform.anchoredPosition = noteAnchoredPosition;

            return rectTransform;
        }

        /// <summary>
        /// ロングノーツのメッセージを表示する
        /// </summary>
        /// <param name="isShow">表示／非表示</param>
        /// <param name="uiPrefab">UIプレハブ</param>
        /// <param name="canvasTransform">キャンバストランスフォーム</param>
        /// <param name="noteTransform">UIトランスフォーム</param>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="directionMode">向きモード</param>
        /// <param name="uiInstance">UIインスタンス</param>
        /// <param name="isDisableOnCompleted">完了後に無効にするか</param>
        private void ShowGuideMessageLongNotes(bool isShow, Transform uiPrefab, Transform canvasTransform, Transform noteTransform, string message, int directionMode, ReactiveProperty<Transform> uiInstance, bool isDisableOnCompleted = true)
        {
            Transform instance = uiInstance.CurrentValue;

            if (instance == null)
            {
                // メッセージ非表示の場合は新規生成せずに終了
                if (!isShow) return;

                // UIを生成
                instance = Instantiate(uiPrefab, canvasTransform);
            }

            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            var noteAnchoredPosition = (noteTransform as RectTransform).anchoredPosition;
            rectTransform.anchoredPosition = noteAnchoredPosition;
            var guideMessageLongNotesPanelView = instance.GetComponent<GuideMessageLongNotesPanelView>();
            guideMessageLongNotesPanelView.SetRectTransTo(noteTransform as RectTransform);
            guideMessageLongNotesPanelView.SetMessage(message);
            if (instance.gameObject.activeSelf != isShow)
            {
                instance.gameObject.SetActive(isShow);
            }
            guideMessageLongNotesPanelView.PlayAnimation(isDisableOnCompleted, directionMode);

            uiInstance.Value = instance;
            _guideMessageLongNotesPanelInstance.Value = instance;
        }

        /// <summary>
        /// ショートノーツのメッセージを表示する
        /// </summary>
        /// <param name="isShow">表示／非表示</param>
        /// <param name="uiPrefab">UIプレハブ</param>
        /// <param name="canvasTransform">キャンバストランスフォーム</param>
        /// <param name="noteTransform">UIトランスフォーム</param>
        /// <param name="message">表示するメッセージ</param>
        /// <param name="uiInstance">UIインスタンス</param>
        /// <param name="isDisableOnCompleted">完了後に無効にするか</param>
        private void ShowGuideMessageShortNotes(bool isShow, Transform uiPrefab, Transform canvasTransform, Transform noteTransform, string message, ReactiveProperty<Transform> uiInstance, bool isDisableOnCompleted = true)
        {
            Transform instance = uiInstance.CurrentValue;

            if (instance == null)
            {
                // メッセージ非表示の場合は新規生成せずに終了
                if (!isShow) return;

                // UIを生成
                instance = Instantiate(uiPrefab, canvasTransform);
            }

            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            var noteAnchoredPosition = (noteTransform as RectTransform).anchoredPosition;
            rectTransform.anchoredPosition = noteAnchoredPosition;
            var guideMessageShortNotesPanelView = instance.GetComponent<GuideMessageShortNotesPanelView>();
            guideMessageShortNotesPanelView.SetRectTransTo(noteTransform as RectTransform);
            guideMessageShortNotesPanelView.SetMessage(message);
            if (instance.gameObject.activeSelf != isShow)
            {
                instance.gameObject.SetActive(isShow);
            }
            guideMessageShortNotesPanelView.PlayAnimation(isDisableOnCompleted);

            uiInstance.Value = instance;
            _guideMessageShortNotesPanelInstance.Value = instance;
        }

        /// <summary>
        /// オバケの家具入居管理の構造体から減算
        /// </summary>
        /// <remarks>ViewModel経由で利用人数が0より大きいなら<br/>
        /// ●利用人数から1名減らす</remarks>
        private void DoSubtractionTransactionGhostInStaticObjectStruct(HomingObjectCustomizeViewModel viewModel)
        {
            viewModel.SubtractionTransactionGhostInStaticObjectStruct();
        }

        /// <summary>
        /// ノーツの成功判定通知をセットを呼び出す
        /// </summary>
        /// <param name="viewModel">オブジェクトをホーミングする処理のカスタマイズビューモデル</param>
        private void DoSetOnNoteSuccessful(HomingObjectCustomizeViewModel viewModel)
        {
            viewModel.SetOnNoteSuccessful(true);
        }

        /// <summary>
        /// ノーツの成功判定通知をセットを呼び出す
        /// </summary>
        /// <param name="viewModel">オブジェクトをホーミングする処理のカスタマイズビューモデル</param>
        private void DoSetOnNoteFailed(HomingObjectCustomizeViewModel viewModel)
        {
            viewModel.SetOnNoteFailed(true);
        }

        /// <summary>
        /// モデルタイプに応じてGhostBodyModel配下の子モデルを切り替える
        /// </summary>
        /// <param name="modelType">オバケモデルタイプ</param>
        /// <remarks>
        /// プレハブ内に各タイプのモデルを配置しておき、名前が一致するものだけ表示する方式。
        /// 例: GhostBodyModel/ghost_model_normal_type, GhostBodyModel/ghost_model_fat_type など
        /// 該当するモデルが見つからない場合はデフォルト（最初の子）を表示する。
        /// </remarks>
        private void SwitchGhostModel(Commons.GhostModelType modelType)
        {
            // GhostBodyModelを探す
            Transform ghostBodyModel = null;
            foreach (Transform child in transform)
            {
                if (child.name.Equals("GhostBodyModel"))
                {
                    ghostBodyModel = child;

                    break;
                }
            }
            if (ghostBodyModel == null || ghostBodyModel.childCount < 1)
                return;

            string targetName = modelType.ToString();
            bool found = false;

            for (int i = 0; i < ghostBodyModel.childCount; i++)
            {
                var child = ghostBodyModel.GetChild(i);
                bool isTarget = child.name.Equals(targetName);
                child.gameObject.SetActive(isTarget);
                if (isTarget)
                {
                    found = true;
                }
            }

            // 該当モデルが見つからない場合はデフォルト（最初の子）を表示
            if (!found && ghostBodyModel.childCount > 0)
            {
                ghostBodyModel.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// オブジェクトをホーミングする処理のカスタマイズ設定
    /// </summary>
    [System.Serializable]
    public class HomingObjectCustomizeSettins
    {
        /// <summary>オブジェクトをホーミングする処理のカスタマイズテーブル</summary>
        public HomingObjectCustomizeTable homingObjectCustomizeTable;
        /// <summary>ノーツタイプ管理</summary>
        /// <remarks>0: ショートノーツ<br/>
        /// 1: ロングノーツ</remarks>
        public int notesType;
    }
}
