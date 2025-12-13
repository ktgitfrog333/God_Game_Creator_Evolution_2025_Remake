using Mains.Models;
using R3;
using UnityEngine;

namespace Selects.ViewModels
{
    /// <summary>
    /// 黒煙パーティクルビューモデル
    /// </summary>
    public class RisingSmokeParticleViewModel : System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>部屋の扉の前で調べる当たり判定に触れた階層</summary>
        private ReactiveCommand<int> _isOnTriggerEnterSearchRangeIndex = new ReactiveCommand<int>();
        /// <summary>部屋の扉の前で調べる当たり判定に触れた階層</summary>
        public ReactiveCommand<int> IsOnTriggerEnterSearchRangeIndex => _isOnTriggerEnterSearchRangeIndex;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public RisingSmokeParticleViewModel()
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
            Observable.EveryUpdate()
                .Select(_ => _playerModel)
                .Where(x => x != null)
                .Select(x => x.IsOnTriggerEnterSearchRangeIndex)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(isOnTriggerEnterSearchRangeIndex =>
                {
                    isOnTriggerEnterSearchRangeIndex.Subscribe(x =>
                    {
                        _isOnTriggerEnterSearchRangeIndex.Execute(x);
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
