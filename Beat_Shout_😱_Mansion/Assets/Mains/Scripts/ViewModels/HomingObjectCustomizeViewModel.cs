using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// オブジェクトをホーミングする処理のカスタマイズビューモデル
    /// </summary>
    public class HomingObjectCustomizeViewModel : IHomingObjectCustomizeModel, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>プレイヤーのトランスフォーム</summary>
        public Transform PlayerTransform => _playerModel?.PlayerPropertiesStruct.transform ?? null;
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        private ReactiveProperty<InteractionPart> _interactionPart = new ReactiveProperty<InteractionPart>();
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveProperty<InteractionPart> InteractionPart => _interactionPart;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public HomingObjectCustomizeViewModel()
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
            Observable.EveryUpdate()
                .Where(_ =>  _playerModel != null &&
                    _playerModel.InteractionPartTable != null)
                .Select(_ => _playerModel.InteractionPartTable.interactionPart)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Subscribe(interactionPart =>
                    {
                        _interactionPart.Value = interactionPart;
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void SubtractionTransactionGhostInStaticObjectStruct()
        {
            if (_playerModel != null)
                _playerModel.SubtractionTransactionGhostInStaticObjectStruct();
        }

        public void SetIsFailed(bool isFailed)
        {
            if (_playerModel != null)
                _playerModel.SetIsFailed(isFailed);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
