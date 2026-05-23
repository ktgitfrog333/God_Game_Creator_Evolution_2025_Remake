using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Selects.ViewModels
{
    /// <summary>
    /// リズムパートパネルのビューモデル
    /// </summary>
    /// <remarks>をステージセレクト用に移植してきた版</remarks>
    /// <see cref="Mains.ViewModels.RhythmPartPanelViewModel"/>
    [CreateAssetMenu(fileName = "RhythmPartPanelCustomizeOfMainViewModel", menuName = "Scriptable Objects/RhythmPartPanelCustomizeOfMainViewModel")]
    public class RhythmPartPanelCustomizeOfMainViewModel : ScriptableObject, IRhythmPartPanelModel, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        private ReactiveProperty<InteractionPart> _interactionPart = new ReactiveProperty<InteractionPart>();
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveProperty<InteractionPart> InteractionPart => _interactionPart;
        /// <summary>バッテリーのトランスフォーム</summary>
        public Transform BatteryTransform => _playerModel?.BatteryTransform ?? null;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public void Initialize()
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    _playerModel.InteractionPartTable.interactionPart.Subscribe(interactionPart =>
                    {
                        _interactionPart.Value = interactionPart;
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void SetTargetCrossPosition(Vector3 targetCrossPosition)
        {
            if (_playerModel != null)
                _playerModel.SetTargetCrossPosition(targetCrossPosition);
        }

        public void SetIsSelectedBattery(bool isSelectedBattery)
        {
            if (_playerModel != null)
                _playerModel.SetIsSelectedBattery(isSelectedBattery);
        }

        public void SetSelectedMissGhostAttackTransform(Transform selectedMissGhostAttackTransform)
        {
            if (_playerModel != null)
                _playerModel.SetSelectedMissGhostAttackTransform(selectedMissGhostAttackTransform);
        }

        public void SetTargetCrossAnchoredPosition(Vector2 targetCrossAnchoredPosition)
        {
            if (_playerModel != null)
                _playerModel.SetTargetCrossAnchoredPosition(targetCrossAnchoredPosition);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
