using Mains.Models;
using R3;
using UnityEngine;

namespace Selects.ViewModels
{
    /// <summary>
    /// リスポーン地点ビューモデル
    /// </summary>
    public class PlayerRespawnPositionViewModel : System.IDisposable, IPlayerRespawnPositionModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public PlayerRespawnPositionViewModel()
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

        public void SetStartPointTrans(Transform startPointTrans)
        {
            if (_playerModel != null)
            {
                _playerModel.SetStartPointTrans(startPointTrans);
            }
            else
            {
                Observable.EveryUpdate()
                    .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                    .Where(x => x != null)
                    .Take(1)
                    .Subscribe(x =>
                    {
                        _playerModel = x;
                        _playerModel.SetStartPointTrans(startPointTrans);
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
