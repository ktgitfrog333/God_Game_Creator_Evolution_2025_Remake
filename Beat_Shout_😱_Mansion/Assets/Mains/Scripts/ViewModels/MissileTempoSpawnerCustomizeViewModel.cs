using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// MissileTempoSpawnerのカスタマイズのビューモデル
    /// </summary>
    public class MissileTempoSpawnerCustomizeViewModel : System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>バッテリーのトランスフォーム</summary>
        public Transform BatteryTransform => _playerModel?.BatteryTransform ?? null;
        /// <summary>敵戦パート</summary>
        public EnemyBattlePart EnemyBattlePart => _playerModel?.EnemyBattlePart ?? EnemyBattlePart.Normal;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public MissileTempoSpawnerCustomizeViewModel()
        {
            _playerModel = GameObject.FindAnyObjectByType<PlayerModel>();

            Observable.EveryUpdate()
                .Where(_ => _playerModel == null)
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                })
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
