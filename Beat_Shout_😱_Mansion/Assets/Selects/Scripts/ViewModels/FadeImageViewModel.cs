using Mains.Models;
using UnityEngine;
using R3;

namespace Selects.ViewModels
{
    /// <summary>
    /// フェードイメージのビューモデル
    /// </summary>
    public class FadeImageViewModel : IFadeImageModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;

        public FadeImageViewModel()
        {
            System.IDisposable disposable = null;
            disposable = Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    // 1度のみ実行されれば良いので破棄しても問題なし
                    disposable.Dispose();
                });
        }

        public void SetIsCompletedStartDirection(bool isCompleted)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedStartDirection(isCompleted);
        }
    }
}
