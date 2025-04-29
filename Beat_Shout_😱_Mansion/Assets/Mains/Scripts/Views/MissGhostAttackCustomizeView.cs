using Mains.Commons;
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
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();
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
            MissGhostAttackCustomizeViewModel viewModel = new MissGhostAttackCustomizeViewModel();
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
        }

        /// <summary>
        /// プレイヤーのHPを減らす演出
        /// </summary>
        /// <param name="viewModel">ビューモデル</param>
        private void SubtractionPlayerHealth(MissGhostAttackCustomizeViewModel viewModel)
        {
            viewModel.SubtractionHealthPoint();
        }
    }
}
