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
        private ReactiveCommand<InteractionPart> _interactionPart = new ReactiveCommand<InteractionPart>();
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveCommand<InteractionPart> InteractionPart => _interactionPart;
        /// <summary>恐怖値のカウントを停止中かのフラグ拡張版</summary>
        private ReactiveCommand<bool> _isStopHorrorCountMore = new ReactiveCommand<bool>();
        /// <summary>恐怖値のカウントを停止中かのフラグ拡張版</summary>
        public ReactiveCommand<bool> IsStopHorrorCountMore => _isStopHorrorCountMore;
        /// <summary>ステージクリア演出完了フラグ</summary>
        private ReactiveCommand<bool> _isCompletedStageClearDirection = new ReactiveCommand<bool>();
        /// <summary>ステージクリア演出完了フラグ</summary>
        public ReactiveCommand<bool> IsCompletedStageClearDirection => _isCompletedStageClearDirection;
        /// <summary>オバケ移動演出の完了フラグ</summary>
        private ReactiveCommand<bool> _isCompletedMoveGhostDirection = new ReactiveCommand<bool>();
        /// <summary>オバケ移動演出の完了フラグ</summary>
        public ReactiveCommand<bool> IsCompletedMoveGhostDirection => _isCompletedMoveGhostDirection;
        /// <summary>敵戦パート</summary>
        public EnemyBattlePart EnemyBattlePart => _playerModel?.EnemyBattlePart ?? EnemyBattlePart.Normal;
        /// <summary>中ボスオバケ退治率</summary>
        public float MidBosskillsRate => _playerModel?.MidBosskillsRate ?? 0f;
        /// <summary>ポルターガイストのアニメーション管理テーブル</summary>
        public PoltergeistTable PoltergeistTable => _playerModel?.PoltergeistTable ?? null;
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
            _playerModel.IsCompletedMoveGhostDirection.Subscribe(x =>
            {
                _isCompletedMoveGhostDirection.Execute(x);
            })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Select(_ => _playerModel.InteractionPartTable)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(table =>
                {
                    table.interactionPart.Subscribe(interactionPart =>
                    {
                        _interactionPart.Execute(interactionPart);
                    })
                    .AddTo(ref _disposableBag);
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
            _interactionPart.DistinctUntilChanged()
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
        }

        public void SetIsMissionClear(bool isMissionClear)
        {
            if (_playerModel != null)
                _playerModel.SetIsMissionClear(isMissionClear);
        }

        /// <summary>
        /// 通常戦パートとステージ番号を考慮したクリア判定が有効な場合
        /// </summary>
        /// <returns>クリア判定結果</returns>
        public bool CheckClearAndUpdateEnemyBattlePart()
        {
            var table = PoltergeistTable;
            if (table == null)
            {
                Debug.LogWarning("プレイヤーモデルまたはポルターガイストのアニメーション管理テーブルがnull");
                return false;
            }

            var part = EnemyBattlePart;
            var checkClearStruct = table.subSettings.checkClearStruct;
            var result = checkClearStruct.enemyBattlePart.Equals(part);

            return result;
        }
    }
}
