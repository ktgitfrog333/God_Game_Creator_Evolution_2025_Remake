using Mains.Commons;
using Mains.External;
using Mains.ViewModels;
using R3;
using R3.Triggers;
using UnityEngine;

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
        /// <summary>オブジェクトをホーミングする処理のカスタマイズ設定</summary>
        [SerializeField] private HomingObjectCustomizeSettins settins;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>オブジェクトをホーミングする処理のカスタマイズビューモデル</summary>
        private HomingObjectCustomizeViewModel _viewModel;
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
            Transform trans = transform;
            var canvasTransform = GameObject.Find("OverlayCanvas").transform;
            _script_XyloApi.SetMissileDirectAnimManagerB(trans);
            // goodPanelPrefabとgoodParticleSysPrefabのインスタンス情報
            (GoodPanelView, ParticleSystem) instancesGoodContents = (null, null);
            _script_XyloApi.IsSuccessfulReactive.Where(x => x)
                .Subscribe(_ =>
                {
                    var instances = ShowUIPanelAtObjectPosition(goodPanelPrefab, canvasTransform, goodParticleSysPrefab, _script_XyloApi.NoteTransform, trans);
                    instancesGoodContents.Item1 = instances.Item1.GetComponent<GoodPanelView>();
                    instancesGoodContents.Item2 = instances.Item2.GetComponent<ParticleSystem>();
                    DoSubtractionTransactionGhostInStaticObjectStruct(_viewModel);
                    _script_XyloApi.PlayHitSuccess3();
                })
                .AddTo(ref _disposableBag);
            _script_XyloApi.IsFailedReactive.Where(x => x &&
                gameObject.activeSelf)
                .Subscribe(_ =>
                {
                    ShowUIPanelAtObjectPosition(badPanelPrefab, canvasTransform, badParticleSysPrefab, _script_XyloApi.NoteTransform, trans);
                    _script_XyloApi.PlayHitMiss3();
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
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _script_XyloApi?.Dispose();
            _viewModel?.Dispose();
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
        /// オバケの家具入居管理の構造体から減算
        /// </summary>
        /// <remarks>ViewModel経由で利用人数が0より大きいなら<br/>
        /// ●利用人数から1名減らす</remarks>
        private void DoSubtractionTransactionGhostInStaticObjectStruct(HomingObjectCustomizeViewModel viewModel)
        {
            viewModel.SubtractionTransactionGhostInStaticObjectStruct();
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
    }
}
