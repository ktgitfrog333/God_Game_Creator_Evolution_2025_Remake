using Mains.Models;
using UnityEngine;
using R3;

namespace Mains.ViewModels
{
    /// <summary>
    /// フェードイメージのビューモデル
    /// </summary>
    public class FadeImageViewModel : IFadeImageModel, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public FadeImageViewModel()
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                })
                .AddTo(ref _disposableBag);
        }

        public void SetIsCompletedStartDirection(bool isCompleted)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedStartDirection(isCompleted);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
