using Mains.Models;
using R3;
using UnityEngine;

namespace Selects.ViewModels
{
    /// <summary>
    /// 共通UIのビューモデル
    /// </summary>
    /// <remarks>をステージセレクト用に移植してきた版</remarks>
    /// <see cref="Mains.ViewModels.CommonPanelViewModel"/>
    [CreateAssetMenu(fileName = "CommonPanelCustomizeOfMainViewModel", menuName = "Scriptable Objects/CommonPanelCustomizeOfMainViewModel")]
    public class CommonPanelCustomizeOfMainViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>プレイヤーの最大HP</summary>
        private ReactiveProperty<int> _playerHealthPointMax = new ReactiveProperty<int>();
        /// <summary>プレイヤーの最大HP</summary>
        public ReactiveProperty<int> PlayerHealthPointMax => _playerHealthPointMax;
        /// <summary>プレイヤーのHP</summary>
        private ReactiveProperty<int> _playerHealthPoint = new ReactiveProperty<int>();
        /// <summary>プレイヤーのHP</summary>
        public ReactiveProperty<int> PlayerHealthPoint => _playerHealthPoint;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public void Initialize()
        {
            PlayerModel model = GameObject.FindAnyObjectByType<PlayerModel>();
            if (model == null)
            {
                GameObject gameObject = new GameObject($"{typeof(PlayerModel).Name}");
                _playerModel = gameObject.AddComponent<PlayerModel>();
            }
            else
            {
                _playerModel = model;
            }
            _playerModel.PlayerPropertiesStruct.healthPointMax.Where(x => 0 < x)
                .Take(1)
                .Subscribe(healthPointMax =>
            {
                _playerHealthPointMax.Value = healthPointMax;
            })
                .AddTo(ref _disposableBag);
            _playerModel.PlayerPropertiesStruct.healthPoint.Subscribe(healthPoint =>
            {
                _playerHealthPoint.Value = healthPoint;
            })
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
