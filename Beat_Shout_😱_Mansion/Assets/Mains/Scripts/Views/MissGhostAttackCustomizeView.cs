using Mains.Commons;
using Mains.External;
using Mains.ViewModels;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// MissGhostAttackのカスタマイズビュー
    /// </summary>
    public class MissGhostAttackCustomizeView : MonoBehaviour
    {
        /// <summary>ボックスコライダー</summary>
        [SerializeField] private BoxCollider boxCollider;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>MissGhostAttackのカスタマイズビューモデル</summary>
        private MissGhostAttackCustomizeViewModel _viewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();
        }

        private void Awake()
        {
            _viewModel = new MissGhostAttackCustomizeViewModel();
            MissGhostAttackCustomizeViewModel viewModel = _viewModel;
            // OnEnable時にモデルタイプに応じたFBX表示切替
            this.OnEnableAsObservable()
                .Subscribe(_ =>
                {
                    SwitchGhostModel(viewModel.CurrentGhostModelType);
                })
                .AddTo(ref _disposableBag);
        }

        private void Start()
        {
            Transform trans = transform;
            var camera = Camera.main.transform;
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    trans.LookAt(camera.position);
                })
                .AddTo(ref _disposableBag);
            MissGhostAttackCustomizeViewModel viewModel = _viewModel;
            Observable.EveryUpdate()
                .Select(_ => viewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    System.IDisposable disposable = null;
                    x.Subscribe(interactionPart =>
                    {
                        switch (interactionPart)
                        {
                            case InteractionPart.None:
                            case InteractionPart.Search:
                            case InteractionPart.ShoutChance:
                                disposable?.Dispose();
                                disposable = Observable.EveryUpdate()
                                    .Where(x => gameObject.activeSelf)
                                    .Subscribe(_ =>
                                    {
                                        gameObject.SetActive(false);
                                    })
                                    .AddTo(ref _disposableBag);
                                
                                break;
                        }
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // boxColliderのOnTriggerStayでプレイヤーのHPを減らす
            _script_XyloApi = new Script_xyloApi();
            if (boxCollider != null)
            {
                bool isOnTriggerEnter = false;
                boxCollider.OnTriggerStayAsObservable()
                    .Where(x => x.CompareTag("Player") &&
                        !isOnTriggerEnter)
                    .DistinctUntilChanged()
                    .Subscribe(_ =>
                    {
                        isOnTriggerEnter = true;
                        _script_XyloApi.PlayDamage1();
                        SubtractionPlayerHealth(viewModel);
                    })
                    .AddTo(ref _disposableBag);
                trans.gameObject.OnDisableAsObservable()
                    .Where(_ => isOnTriggerEnter)
                    .Subscribe(_ =>
                    {
                        isOnTriggerEnter = false;
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _script_XyloApi?.Dispose();
            _viewModel?.Dispose();
        }

        /// <summary>
        /// プレイヤーのHPを減らす演出
        /// </summary>
        /// <param name="viewModel">ビューモデル</param>
        private void SubtractionPlayerHealth(MissGhostAttackCustomizeViewModel viewModel)
        {
            viewModel.SetIsBadEndRhythmPart(true);
        }

        /// <summary>
        /// モデルタイプに応じて子モデルを切り替える
        /// </summary>
        /// <param name="modelType">オバケモデルタイプ</param>
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
}
