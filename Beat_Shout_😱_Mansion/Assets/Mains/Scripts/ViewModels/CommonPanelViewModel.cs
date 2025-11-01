using UnityEngine;
using Mains.Commons;
using Mains.Models;
using ObservableCollections;
using R3;

namespace Mains.ViewModels
{
    /// <summary>
    /// 共通UIのビューモデル
    /// </summary>

    public class CommonPanelViewModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        public ObservableList<GhostInStaticObjectStruct> GhostInStaticObjectStructs => _playerModel?.GhostInStaticObjectStructs ?? null;
        /// <summary>デシベルレベル</summary>
        public ReactiveCommand<float> DbLevel => _playerModel?.InteractionPartTable?.dbLevel ?? null;
        /// <summary>プレイヤーのHP</summary>
        public ReactiveProperty<int> PlayerHealthPoint => _playerModel?.PlayerPropertiesStruct.healthPoint ?? null;
        /// <summary>プレイヤーの最大HP</summary>
        public ReactiveCommand<int> PlayerHealthPointMax => _playerModel?.PlayerPropertiesStruct.healthPointMax ?? null;
        /// <summary>恐怖値</summary>
        public ReactiveCommand<float> HorrorCount => _playerModel?.PlayerPropertiesStruct.horrorCount ?? null;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public CommonPanelViewModel(InteractionPartTable interactionPartTable)
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
            if (_playerModel.InteractionPartTable == null)
                _playerModel.InteractionPartTable = interactionPartTable;
        }
    }
}
