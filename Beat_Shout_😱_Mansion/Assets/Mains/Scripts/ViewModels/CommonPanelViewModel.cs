using DG.Tweening;
using Mains.Commons;
using Mains.Models;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// 共通UIのビューモデル
    /// </summary>

    public class CommonPanelViewModel : ICommonPanelModel1
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
        /// <summary>恐怖値のカウントを停止中かのフラグ</summary>
        public ReactiveCommand<bool> IsStopHorrorCount => _playerModel?.IsStopHorrorCount ?? null;
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveProperty<InteractionPart> InteractionPart
        {
            get
            {
                return _playerModel?.InteractionPartTable?.interactionPart ?? null;
            }
        }
        /// <summary>恐怖値のカウントを停止中かのフラグ拡張版</summary>
        private ReactiveCommand<bool> _isStopHorrorCountMore = new ReactiveCommand<bool>();
        /// <summary>恐怖値のカウントを停止中かのフラグ拡張版</summary>
        public ReactiveCommand<bool> IsStopHorrorCountMore => _isStopHorrorCountMore;
        /// <summary>ステージクリア演出完了フラグ</summary>
        private ReactiveCommand<bool> _isCompletedStageClearDirection = new ReactiveCommand<bool>();
        /// <summary>ステージクリア演出完了フラグ</summary>
        public ReactiveCommand<bool> IsCompletedStageClearDirection => _isCompletedStageClearDirection;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public CommonPanelViewModel(InteractionPartTable interactionPartTable)
        {
            PlayerModel model = GameObject.FindAnyObjectByType<PlayerModel>();
            if (model == null)
            {
                GameObject gameObject = new GameObject($"{typeof(PlayerModel).Name}");
                _playerModel = gameObject.AddComponent<PlayerModel>();
            }
            else
            {
                _playerModel = model;
            }
            if (_playerModel.InteractionPartTable == null)
                _playerModel.InteractionPartTable = interactionPartTable;
            _playerModel.IsCompletedStageClearDirection.Subscribe(x =>
            {
                _isCompletedStageClearDirection.Execute(x);
            })
                .AddTo(ref _disposableBag);
            bool isStopOfRhythm = false;
            // ブレイブシャウト成功中は点滅させる
            Observable.EveryUpdate()
                .Select(_ => IsStopHorrorCount)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.DistinctUntilChanged()
                        .Where(_ => !isStopOfRhythm)
                        .Subscribe(x => _isStopHorrorCountMore.Execute(x))
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // リズムパート中は停止させる
            Observable.EveryUpdate()
                .Select(_ => InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.DistinctUntilChanged()
                        .Select(x =>
                        {
                            switch (x)
                            {
                                case Commons.InteractionPart.Search:
                                case Commons.InteractionPart.ShoutChance:
                                    // 探索パートとシャウトチャンスパートは恐怖ゲージ加算中
                                    isStopOfRhythm = false;

                                    return (0, false);
                                case Commons.InteractionPart.Rhythm:
                                    // リズムパートは停止中
                                    isStopOfRhythm = true;

                                    return (0, true);
                                default:

                                    return (-1, false);
                            }
                        })
                        .Where(x => -1 < x.Item1)
                        .Select(x => x.Item2)
                        .Subscribe(x => _isStopHorrorCountMore.Execute(x))
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void SetIsMissionClear(bool isMissionClear)
        {
            if (_playerModel != null)
                _playerModel.SetIsMissionClear(isMissionClear);
        }
    }
}
