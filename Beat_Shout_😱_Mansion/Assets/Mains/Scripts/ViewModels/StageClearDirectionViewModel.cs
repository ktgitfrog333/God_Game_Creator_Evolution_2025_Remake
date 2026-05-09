using Mains.Models;
using R3;
using Rewired;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// ステージクリア演出のビューモデル
    /// </summary>
    public class StageClearDirectionViewModel : System.IDisposable, IStageClearDirectionModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>ミッションクリアフラグ</summary>
        private ReactiveCommand<bool> _isMissionClear = new ReactiveCommand<bool>();
        /// <summary>ミッションクリアフラグ</summary>
        public ReactiveCommand<bool> IsMissionClear => _isMissionClear;
        /// <summary>Rewiredのプレイヤー</summary>
        private Player _player;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public StageClearDirectionViewModel(Player player)
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    _playerModel.IsMissionClear.Subscribe(x =>
                    {
                        _isMissionClear.Execute(x);
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            _player = player;
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

        public void Dispose()
        {
            _disposableBag.Dispose();
        }

        public void SetIsCompletedStageClearDirection(bool isCompletedStageClearDirection)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedStageClearDirection(isCompletedStageClearDirection);
        }
    }
}
