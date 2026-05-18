using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// MissGhostAttackのカスタマイズビューモデル
    /// </summary>
    public class MissGhostAttackCustomizeViewModel : IMissGhostAttackCustomizeModel, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
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
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                })
                .AddTo(ref _disposableBag);
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

        /// <summary>
        /// 現在のトランザクション中のオバケモデルタイプを取得する
        /// </summary>
        public GhostModelType CurrentGhostModelType =>
            _playerModel?.TransactionGhostInStaticObjectStruct.ghostModelType ?? GhostModelType.ghost_model_normal_type;

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
