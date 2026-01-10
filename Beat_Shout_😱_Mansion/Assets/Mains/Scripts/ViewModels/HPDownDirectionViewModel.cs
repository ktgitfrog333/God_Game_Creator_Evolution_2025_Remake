using Mains.Models;
using R3;
using Rewired;
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
        /// <summary>Rewiredのプレイヤー</summary>
        private Player _player;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public HPDownDirectionViewModel(Player player)
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
            _player = player;
        }

        public void SetIsCompletedDirection(bool isCompleted)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedDirection(isCompleted);
        }

        /// <summary>
        /// プレイヤーコントローラー操作の有効かどうかをセット
        /// </summary>
        /// <param name="isEnabled">有効かどうか</param>
        public void SetPlayerControllerEnabled(bool isEnabled)
        {
            var player = _player;
            player.controllers.maps.SetMapsEnabled(isEnabled, "Default");
        }

        public void SubtractionHealthPoint()
        {
            if (_playerModel != null)
                _playerModel.SubtractionHealthPoint();
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
