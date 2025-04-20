using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// リズムパートパネルのビューモデル
    /// </summary>
    public class RhythmPartPanelViewModel : IRhythmPartPanelModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveProperty<InteractionPart> InteractionPart
        {
            get
            {
                return _playerModel?.InteractionPartTable?.interactionPart ?? null;
            }
        }
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public RhythmPartPanelViewModel()
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    // 1度のみ実行されれば良いので破棄しても問題なし
                    _disposableBag.Dispose();
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
    }
}
