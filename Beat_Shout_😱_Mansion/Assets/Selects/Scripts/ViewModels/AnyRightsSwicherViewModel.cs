using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Selects.ViewModels
{
    /// <summary>
    /// 複数のライトを切り替えるビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "AnyRightsSwicherViewModel", menuName = "Scriptable Objects/AnyRightsSwicherViewModel")]
    public class AnyRightsSwicherViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>敵戦パート</summary>
        private ReactiveProperty<EnemyBattlePart> _enemyBattlePart;
        /// <summary>敵戦パート</summary>
        public ReadOnlyReactiveProperty<EnemyBattlePart> EnemyBattlePart => _enemyBattlePart;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag;

        public void Initialize()
        {
            _disposableBag = new DisposableBag();
            _enemyBattlePart = new ReactiveProperty<EnemyBattlePart>();

            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;

                    var model = _playerModel;
                    Observable.EveryUpdate()
                        .Select(_ => model.EnemyBattlePart)
                        .DistinctUntilChanged()
                        .Subscribe(x =>
                        {
                            _enemyBattlePart.Value = x;
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
       }

        public void Dispose()
        {
            _disposableBag.Dispose();
            _enemyBattlePart = null;
        }
    }
}
