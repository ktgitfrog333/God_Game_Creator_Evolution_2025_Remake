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

        private void Start()
		{
            _script_XyloApi = new Script_xyloApi();
            Transform trans = transform;
            var canvasTransform = GameObject.Find("OverlayCanvas").transform;
            _script_XyloApi.SetMissileDirectAnimManagerB(trans);
            _viewModel = new HomingObjectCustomizeViewModel();
            _script_XyloApi.IsSuccessfulReactive.Where(x => x)
                .Subscribe(_ =>
                {
                    ShowUIPanelAtObjectPosition(goodPanelPrefab, canvasTransform, goodParticleSysPrefab, _script_XyloApi.NoteTransform, trans);
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
            _script_XyloApi.SetMissileRendererColor();
            // MissileDirectAnimManagerBのmissileRendererを監視して、それをオバケのモデル側のRendererへ反映
            _script_XyloApi.MissileRendererColor.Subscribe(color =>
            {
                SetRendererColor(modelRenderer, color);
            })
                .AddTo(ref _disposableBag);
            this.OnEnableAsObservable()
                .Subscribe(_ =>
                {
                    float dice = Random.value; // 0.0〜1.0 の乱数
                    // 30%の確率で再生
                    if (dice <= .3f)
                    {
                        _script_XyloApi.PlayGhostLaugh3();
                    }
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
        private void ShowUIPanelAtObjectPosition(Transform uiPrefab, Transform canvasTransform, Transform particleSysPrefab, Transform noteTransform, Transform trans)
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
        /// レンダラーの色をセット
        /// </summary>
        /// <param name="modelRenderer">オバケの3Dモデルレンダラー</param>
        /// <param name="color">色</param>
        private void SetRendererColor(Renderer modelRenderer, Color color)
        {
            modelRenderer.material.color = color;
        }
    }
}
