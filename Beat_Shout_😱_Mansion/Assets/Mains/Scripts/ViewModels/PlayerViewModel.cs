using UnityEngine;
using Mains.Commons;
using Mains.Models;

namespace Mains.ViewModels
{
    /// <summary>
    /// プレイヤーのビューモデル
    /// </summary>
    public class PlayerViewModel : IPlayerModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;

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
