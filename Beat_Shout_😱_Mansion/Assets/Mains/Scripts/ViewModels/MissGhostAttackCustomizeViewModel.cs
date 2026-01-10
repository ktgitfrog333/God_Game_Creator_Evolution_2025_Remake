using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// MissGhostAttackのカスタマイズビューモデル
    /// </summary>
    public class MissGhostAttackCustomizeViewModel : IMissGhostAttackCustomizeModel
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

        public MissGhostAttackCustomizeViewModel()
        {
            System.IDisposable disposable = null;
            disposable = Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    // 1度のみ実行されれば良いので破棄しても問題なし
                    disposable.Dispose();
                });
        }

        public void SubtractionHealthPoint()
        {
            if (_playerModel != null)
                _playerModel.SubtractionHealthPoint();
        }

        public void SetIsBadEndRhythmPart(bool isBadEndRhythmPart)
        {
            if (_playerModel != null)
                _playerModel.SetIsBadEndRhythmPart(isBadEndRhythmPart);
        }
    }
}
