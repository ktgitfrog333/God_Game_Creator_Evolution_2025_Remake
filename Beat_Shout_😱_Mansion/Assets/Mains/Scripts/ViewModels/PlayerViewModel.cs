using UnityEngine;
using Mains.Commons;
using Mains.Models;
using ObservableCollections;

namespace Mains.ViewModels
{
    /// <summary>
    /// プレイヤーのビューモデル
    /// </summary>
    public class PlayerViewModel : IPlayerModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        public ObservableList<GhostInStaticObjectStruct> GhostInStaticObjectStructs => _playerModel?.GhostInStaticObjectStructs ?? null;

        public PlayerViewModel(InteractionPartTable interactionPartTable)
        {
            GameObject gameObject = new GameObject($"{typeof(PlayerModel).Name}");
            _playerModel = gameObject.AddComponent<PlayerModel>();
            _playerModel.InteractionPartTable = interactionPartTable;
        }

        public void SetIsSwitchPart(bool isSwitchPart)
        {
            if (_playerModel != null)
                _playerModel.SetIsSwitchPart(isSwitchPart);
        }
    }
}
