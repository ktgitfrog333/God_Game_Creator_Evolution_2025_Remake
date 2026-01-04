using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;
using ObservableCollections;

namespace Mains.ViewModels
{
    /// <summary>
    /// ポルターガイストのビューモデル
    /// </summary>
    public class PoltergeistViewModel : IPoltergeistModel, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>ポルターガイストが発生</summary>
        public ReactiveProperty<Vector3> OnActionPoltergeistPosition
        {
            get
            {
                return _playerModel?.PoltergeistTable?.onActionPoltergeistPosition ?? null;
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
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        public ObservableList<GhostInStaticObjectStruct> GhostInStaticObjectStructs => _playerModel?.GhostInStaticObjectStructs ?? null;
        /// <summary>プレイヤーのトランスフォーム</summary>
        public Transform PlayerTransform => _playerModel?.PlayerPropertiesStruct.transform ?? null;
        /// <summary>オバケの家具入居管理の構造体トランザクション</summary>
        public GhostInStaticObjectStruct TransactionGhostInStaticObjectStruct => _playerModel?.TransactionGhostInStaticObjectStruct ?? new GhostInStaticObjectStruct();
        /// <summary>プレイヤーのHP</summary>
        public ReactiveProperty<int> PlayerHealthPoint => _playerModel?.PlayerPropertiesStruct.healthPoint ?? null;
        /// <summary>デシベルレベル</summary>
        public ReactiveCommand<float> DbLevel => _playerModel?.InteractionPartTable?.dbLevel ?? null;
        /// <summary>リズムパートでミスした時にハートが減少する演出完了フラグ
        private ReactiveCommand<bool> _isCompletedDirection = new ReactiveCommand<bool>();
        /// <summary>リズムパートでミスした時にハートが減少する演出完了フラグ
        public ReactiveCommand<bool> IsCompletedDirection => _isCompletedDirection;

        public PoltergeistViewModel(PoltergeistTable poltergeistTable)
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    _playerModel.PoltergeistTable = poltergeistTable;
                    _playerModel.IsCompletedDirection.Subscribe(isCompleted =>
                    {
                        _isCompletedDirection.Execute(isCompleted);
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void SetOnActionPoltergeistPosition(Vector3 onActionPoltergeistPosition)
        {
            if (_playerModel != null)
                _playerModel.SetOnActionPoltergeistPosition(onActionPoltergeistPosition);
        }

        public void AddGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            _playerModel.AddGhostInStaticObjectStructs(ghostInStaticObjectStruct);
        }

        public void SetIsCompletedBurstGhosts(bool isCompletedBurstGhosts)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedBurstGhosts(isCompletedBurstGhosts);
        }

        public void SetTransactionGhostInStaticObjectStruct(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            if (_playerModel != null)
                _playerModel.SetTransactionGhostInStaticObjectStruct(ghostInStaticObjectStruct);
        }

        public void SetDefaultTransactionGhostInStaticObjectStruct()
        {
            if (_playerModel != null)
                _playerModel.SetDefaultTransactionGhostInStaticObjectStruct();
        }

        public void SetInteractionPartToSearch()
        {
            if (_playerModel != null)
                _playerModel.SetInteractionPartToSearch();
        }

        public void SetIsCompletedRhythmPart(int isCompletedRhythmPart)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedRhythmPart(isCompletedRhythmPart);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
