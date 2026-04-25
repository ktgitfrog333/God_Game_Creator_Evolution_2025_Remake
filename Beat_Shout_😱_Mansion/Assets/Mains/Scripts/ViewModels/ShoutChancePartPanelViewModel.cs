using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// シャウトチャンスパネルのビューモデル
    /// </summary>
    public class ShoutChancePartPanelViewModel
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
        /// <summary>デシベルレベル</summary>
        public ReactiveCommand<float> DbLevel => _playerModel?.InteractionPartTable?.dbLevel ?? null;
        /// <summary>シャウトノーツアクティブフラグ</summary>
        private ReactiveCommand<bool> _shoutNoteActiveReactive = new ReactiveCommand<bool>();
        /// <summary>シャウトノーツアクティブフラグ</summary>
        public ReactiveCommand<bool> ShoutNoteActiveReactive => _shoutNoteActiveReactive;
        /// <summary>シャウトノーツアクティブフラグ</summary>
        public bool ShoutNoteActive => _playerModel?.ShoutNoteActive ?? false;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();


        public ShoutChancePartPanelViewModel()
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    var model = _playerModel;
                    model.ShoutNoteActiveReactive.Subscribe(shoutNoteActive =>
                    {
                        _shoutNoteActiveReactive.Execute(shoutNoteActive);
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }
    }
}
