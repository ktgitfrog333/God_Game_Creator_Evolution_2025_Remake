using UnityEngine;
using Mains.Commons;
using Mains.Models;
using ObservableCollections;
using R3;

namespace Mains.ViewModels
{
    /// <summary>
    /// 共通UIのビューモデル
    /// </summary>

    public class CommonPanelViewModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        public ObservableList<GhostInStaticObjectStruct> GhostInStaticObjectStructs => _playerModel?.GhostInStaticObjectStructs ?? null;
        /// <summary>デシベルレベル</summary>
        public ReactiveCommand<float> DbLevel => _playerModel?.InteractionPartTable?.dbLevel ?? null;
        /// <summary>プレイヤーのHP</summary>
        public ReactiveProperty<int> PlayerHealthPoint => _playerModel?.PlayerPropertiesStruct.healthPoint ?? null;
        /// <summary>プレイヤーの最大HP</summary>
        public ReactiveCommand<int> PlayerHealthPointMax => _playerModel?.PlayerPropertiesStruct.healthPointMax ?? null;
        /// <summary>恐怖値</summary>
        public ReactiveCommand<float> HorrorCount => _playerModel?.PlayerPropertiesStruct.horrorCount ?? null;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public CommonPanelViewModel()
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
    }
}
