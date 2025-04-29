using UnityEngine;
using Mains.Commons;
using Mains.Models;
using ObservableCollections;
using R3;

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
        /// <summary>ポルターガイストの発生位置</summary>
        public ReactiveProperty<Vector3> OnActionPoltergeistPosition
        {
            get
            {
                return _playerModel?.PoltergeistTable?.onActionPoltergeistPosition ?? null;
            }
        }
        /// <summary>ゴーストが飛び出してくる演出の完了</summary>
        public ReactiveCommand<bool> IsCompletedBurstGhosts
        {
            get
            {
                return _playerModel?.InteractionPartTable?.isCompletedBurstGhosts ?? null;
            }
        }
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveProperty<InteractionPart> InteractionPart
        {
            get
            {
                return _playerModel?.InteractionPartTable?.interactionPart ?? null;
            }
        }
        /// <summary>ターゲットクロス位置</summary>
        public ReactiveCommand<Vector3> TargetCrossPosition
        {
            get
            {
                return _playerModel?.TargetCrossPosition ?? null;
            }
        }
        /// <summary>バッテリーのトランスフォーム</summary>
        public Transform BatteryTransform => _playerModel?.BatteryTransform ?? null;
        /// <summary>バッテリーが選択状態か</summary>
        public bool IsSelectedBattery => _playerModel?.IsSelectedBattery ?? false;

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

        public void SetPlayerTransform(Transform transform)
        {
            if (_playerModel != null)
                _playerModel.SetPlayerTransform(transform);
        }

        public void SetIsCompletedBurstGhosts(bool isCompletedBurstGhosts)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedBurstGhosts(isCompletedBurstGhosts);
        }

        public void SetDbLevel(float dbLevel)
        {
            if (_playerModel != null)
                _playerModel.SetDbLevel(dbLevel);
        }

        public void SetInteractionPart(InteractionPart interactionPart)
        {
            if (_playerModel != null)
                _playerModel.SetInteractionPart(interactionPart);
        }

        public void SetHealthPointMax(int healthPointMax)
        {
            if (_playerModel != null)
                _playerModel.SetHealthPointMax(healthPointMax);
        }

        public void SetHealthPoint(int healthPoint)
        {
            if (_playerModel != null)
                _playerModel.SetHealthPoint(healthPoint);
        }

        public void SetBatteryTransform(Transform batteryTransform)
        {
            if (_playerModel != null)
                _playerModel.SetBatteryTransform(batteryTransform);
        }

        public void SetIsLockedUpdateHealthPoint(bool isLockedUpdateHealthPoint)
        {
            if (_playerModel != null)
                _playerModel.SetIsLockedUpdateHealthPoint(isLockedUpdateHealthPoint);
        }
    }
}
