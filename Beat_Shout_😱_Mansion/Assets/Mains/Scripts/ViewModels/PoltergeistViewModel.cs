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
    public class PoltergeistViewModel : IPoltergeistModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>ポルターガイストが発生</summary>
        public ReactiveProperty<bool> IsOnActionPoltergeist
        {
            get
            {
                return _playerModel?.PoltergeistTable?.isOnActionPoltergeist ?? null;
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
                    // 1度のみ実行されれば良いので破棄しても問題なし
                    _disposableBag.Dispose();
                })
                .AddTo(ref _disposableBag);
        }

        public void SetIsOnActionPoltergeist(bool isOnActionPoltergeist)
        {
            if (_playerModel != null)
                _playerModel.SetIsOnActionPoltergeist(isOnActionPoltergeist);
        }

        public void AddGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            _playerModel.AddGhostInStaticObjectStructs(ghostInStaticObjectStruct);
        }
    }
}
