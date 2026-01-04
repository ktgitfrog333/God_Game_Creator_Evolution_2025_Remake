using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// ハートが減少する演出ビューモデル
    /// </summary>
    public class HPDownDirectionViewModel : System.IDisposable, IHPDownDirectionModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>リズムパート完了フラグ</summary>
        private ReactiveCommand<int> _isCompletedRhythmPart = new ReactiveCommand<int>();
        /// <summary>リズムパート完了フラグ</summary>
        public ReactiveCommand<int> IsCompletedRhythmPart => _isCompletedRhythmPart;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public HPDownDirectionViewModel()
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    _playerModel.IsCompletedRhythmPart.Subscribe(isCompleted =>
                    {
                        _isCompletedRhythmPart.Execute(isCompleted);
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void SetIsCompletedDirection(bool isCompleted)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedDirection(isCompleted);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
