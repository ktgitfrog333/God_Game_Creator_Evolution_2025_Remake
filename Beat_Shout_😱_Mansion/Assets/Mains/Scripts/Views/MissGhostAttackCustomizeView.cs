using Mains.Commons;
using Mains.ViewModels;
using R3;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// MissGhostAttackのカスタマイズビュー
    /// </summary>
    public class MissGhostAttackCustomizeView : MonoBehaviour
    {
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

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
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
