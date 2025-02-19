using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// ポルターガイストのビューモデル
    /// </summary>
    public class PoltergeistViewModel : IPoltergeistModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public PoltergeistViewModel(PoltergeistTable poltergeistTable)
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    _playerModel.PoltergeistTable = poltergeistTable;
                    // 1度のみ実行されれば良いので破棄しても問題なし
                    _disposableBag.Dispose();
                })
                .AddTo(ref _disposableBag);
        }

        public void SetIsOnActionPoltergeist(bool isOnActionPoltergeist)
        {
            if (_playerModel != null)
                _playerModel.SetIsOnActionPoltergeist(isOnActionPoltergeist);
        }
    }
}
