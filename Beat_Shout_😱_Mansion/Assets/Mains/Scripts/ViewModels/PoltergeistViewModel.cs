using Mains.Commons;
using Mains.Models;
using ObservableCollections;
using R3;
using R3.Triggers;
using System.Linq;
using UnityEngine;

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
        private InteractionPart _interactionPart;
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public InteractionPart InteractionPart => _interactionPart;
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        private ReactiveCommand<InteractionPart> _interactionPartReactive = new ReactiveCommand<InteractionPart>();
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveCommand<InteractionPart> InteractionPartReactive => _interactionPartReactive;
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        public ObservableList<GhostInStaticObjectStruct> GhostInStaticObjectStructs => _playerModel?.GhostInStaticObjectStructs ?? null;
        /// <summary>プレイヤーのトランスフォーム</summary>
        public Transform PlayerTransform => _playerModel?.PlayerPropertiesStruct.transform ?? null;
        /// <summary>オバケの家具入居管理の構造体トランザクション</summary>
        public GhostInStaticObjectStruct TransactionGhostInStaticObjectStruct => _playerModel?.TransactionGhostInStaticObjectStruct ?? new GhostInStaticObjectStruct();
        /// <summary>プレイヤーのHP</summary>
        public ReactiveProperty<int> PlayerHealthPoint => _playerModel?.PlayerPropertiesStruct.healthPoint ?? null;
        /// <summary>リズムパートが失敗で終了したか</summary>
        private ReactiveCommand<bool> _isBadEndRhythmPart = new ReactiveCommand<bool>();
        /// <summary>リズムパートが失敗で終了したか</summary>
        public ReactiveCommand<bool> IsBadEndRhythmPart => _isBadEndRhythmPart;
        /// <summary>デシベルレベル</summary>
        public ReactiveCommand<float> DbLevel => _playerModel?.InteractionPartTable?.dbLevel ?? null;
        /// <summary>リズムパートでミスした時にハートが減少する演出完了フラグ
        private ReactiveCommand<bool> _isCompletedDirection = new ReactiveCommand<bool>();
        /// <summary>リズムパートでミスした時にハートが減少する演出完了フラグ
        public ReactiveCommand<bool> IsCompletedDirection => _isCompletedDirection;
        /// <summary>ミッションクリアフラグ</summary>
        private ReactiveCommand<bool> _isMissionClear = new ReactiveCommand<bool>();
        /// <summary>ミッションクリアフラグ</summary>
        public ReactiveCommand<bool> IsMissionClear => _isMissionClear;
        /// <summary>家具とプレイヤーがお互い向き合っている状態フラグ</summary>
        private ReactiveCommand<bool> _isPostRhythmFaceOff = new ReactiveCommand<bool>();
        /// <summary>家具とプレイヤーがお互い向き合っている状態フラグ</summary>
        public ReactiveCommand<bool> IsPostRhythmFaceOff => _isPostRhythmFaceOff;
        /// <summary>オバケ移動演出の対象</summary>
        private bool _isMoveGhostDirectionTarget;
        /// <summary>オバケ移動演出の対象</summary>
        public bool IsMoveGhostDirectionTarget => _isMoveGhostDirectionTarget;
        /// <summary>攻撃開始フラグ</summary>
        private ReactiveCommand<Transform> _isStartAttack = new ReactiveCommand<Transform>();
        /// <summary>攻撃開始フラグ</summary>
        public ReactiveCommand<Transform> IsStartAttack => _isStartAttack;
        /// <summary>敵戦パート</summary>
        private ReactiveCommand<EnemyBattlePart> _enemyBattlePartReactive = new ReactiveCommand<EnemyBattlePart>();
        /// <summary>敵戦パート</summary>
        public ReactiveCommand<EnemyBattlePart> EnemyBattlePartReactive => _enemyBattlePartReactive;
        /// <summary>敵戦パート</summary>
        public EnemyBattlePart EnemyBattlePart => _playerModel?.EnemyBattlePart ?? EnemyBattlePart.Normal;
        /// <summary>中ボスオバケ退治率</summary>
        private ReactiveCommand<float> _midBosskillsRateReactive = new ReactiveCommand<float>();
        /// <summary>中ボスオバケ退治率</summary>
        public ReactiveCommand<float> MidBosskillsRateReactive => _midBosskillsRateReactive;
        /// <summary>中ボスオバケ退治率</summary>
        public float MidBosskillsRate => _playerModel?.MidBosskillsRate ?? 0f;
        ///// <summary>レベルオーナー</summary>
        //private LevelOwner _levelOwner;
        ///// <summary>レベルオーナー</summary>
        //private LevelOwner LevelOwner => _levelOwner != null ? _levelOwner : _levelOwner = GameManager.Instance.LevelOwner;

        public PoltergeistViewModel(PoltergeistTable poltergeistTable, Transform startAttackInstance = null)
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
                    _playerModel.IsBadEndRhythmPart.Subscribe(isBadEnd =>
                    {
                        _isBadEndRhythmPart.Execute(isBadEnd);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.IsMissionClear.Subscribe(isMissionClear =>
                    {
                        _isMissionClear.Execute(isMissionClear);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.IsPostRhythmFaceOff.Subscribe(isPostRhythmFaceOff =>
                    {
                        _isPostRhythmFaceOff.Execute(isPostRhythmFaceOff);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.MidBosskillsRateReactive.Subscribe(midBosskillsRate =>
                    {
                        _midBosskillsRateReactive.Execute(midBosskillsRate);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.EnemyBattlePartReactive.Subscribe(enemyBattlePart =>
                    {
                        _enemyBattlePartReactive.Execute(enemyBattlePart);
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
                                _interactionPart = interactionPart;
                                _interactionPartReactive.Execute(interactionPart);
                            })
                            .AddTo(ref _disposableBag);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // オバケの攻撃開始範囲の有効フラグ
            ReactiveProperty<bool> startAttackInstanceEnabled = new ReactiveProperty<bool>();
            // オバケの攻撃開始範囲の監視
            System.IDisposable disposable = null;
            startAttackInstanceEnabled.Subscribe(enabled =>
            {
                if (enabled)
                {
                    disposable = startAttackInstance.OnTriggerEnterAsObservable()
                        .Where(x => x.CompareTag("Player"))
                        .Select(x => x.transform)
                        .Take(1)
                        .Subscribe(player =>
                        {
                            _isStartAttack.Execute(player);
                        })
                        .AddTo(ref _disposableBag);
                }
                else
                {
                    disposable?.Dispose();
                }
            })
                .AddTo(ref _disposableBag);
            // オバケの攻撃開始範囲コライダー
            Collider startAttackInstanceCollider = null;
            if (startAttackInstance != null)
            {
                startAttackInstanceCollider = startAttackInstance.GetComponent<SphereCollider>();
            }
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    if (startAttackInstanceCollider != null &&
                        startAttackInstanceCollider.enabled != startAttackInstanceEnabled.Value)
                    {
                        startAttackInstanceEnabled.Value = startAttackInstanceCollider.enabled;
                    }
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

        public void SetIsMissionClear(bool isMissionClear)
        {
            if (_playerModel != null)
                _playerModel.SetIsMissionClear(isMissionClear);
        }

        public void SetTargetGhost(Transform targetGhost)
        {
            if (_playerModel != null)
                _playerModel.SetTargetGhost(targetGhost);
        }

        public void SetIsCompletedMoveGhostDirection(bool isCompletedMoveGhostDirection)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedMoveGhostDirection(isCompletedMoveGhostDirection);
        }

        /// <summary>
        /// オバケ移動演出の対象をセット
        /// </summary>
        /// <param name="isMoveGhostDirectionTarget">オバケ移動演出の対象</param>
        public void SetIsMoveGhostDirectionTarget(bool isMoveGhostDirectionTarget)
        {
            _isMoveGhostDirectionTarget = isMoveGhostDirectionTarget;
        }

        /// <summary>
        /// 攻撃開始フラグをセット
        /// </summary>
        /// <param name="isStartAttack">攻撃開始フラグ</param>
        public void SetIsStartAttack(Transform isStartAttack)
        {
            _isStartAttack.Execute(isStartAttack);
        }

        /// <summary>
        /// 移動元の家具のポルターガイスト情報を初期化
        /// </summary>
        /// <param name="ghostInStaticObjectStruct">オバケの家具入居管理のデータクラス</param>
        public void ResetStaticObject(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            if (_playerModel != null)
                _playerModel.GhostContainer.Reset(ghostInStaticObjectStruct);
        }

        /// <summary>
        /// オバケの引っ越し
        /// </summary>
        /// <param name="ghostInStaticObjectStruct">オバケの家具入居管理のデータクラス</param>
        public void ShuffleNewStaticObject(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            if (_playerModel != null)
                _playerModel.GhostContainer.Shuffle(ghostInStaticObjectStruct);
        }

        /// <summary>
        /// 通常戦パートとステージ番号を考慮したクリア判定が有効な場合
        /// </summary>
        /// <param name="checkClearStructs"></param>
        /// <returns>クリア判定結果</returns>
        public bool CheckClearAndUpdateEnemyBattlePart()
        {
            var table = _playerModel?.PoltergeistTable ?? null;
            if (table == null)
            {
                Debug.LogWarning("プレイヤーモデルまたはポルターガイストのアニメーション管理テーブルがnull");
                return false;
            }

            var part = EnemyBattlePart;
            var checkClearStruct = table.subSettings.checkClearStruct;
            var result = checkClearStruct.enemyBattlePart.Equals(part);
            if (!result)
            {
                int index = (int)part;
                index++;
                EnemyBattlePart updPart = (EnemyBattlePart)index;
                SetEnemyBattlePart(updPart);
            }

            return result;
        }

        ///// <summary>
        ///// 中ボスオバケ退治率の計算
        ///// </summary>
        ///// <param name="beforeMembersCount">リズムパート前の人数</param>
        ///// <returns>中ボスオバケ退治率</returns>
        //public float CalcMidBosskillsRate(float beforeMembersCount)
        //{
        //    // リズムパート前の人数 - リズムパート後の人数 = 退治数
        //    var afterPoint = beforeMembersCount - TransactionGhostInStaticObjectStruct.membersCount;
        //    float targetkillsRate = (float)afterPoint / beforeMembersCount;
        //    _midBosskillsRate = targetkillsRate;

        //    return _midBosskillsRate;
        //}

        ///// <summary>
        ///// 利用人数合計を取得する
        ///// </summary>
        ///// <returns>利用人数合計</returns>
        ///// <remarks>「通常戦パート」か「中ボス戦パート」かの条件によって、<br/>
        ///// 「通常オバケ」か「中ボスオバケ」の利用人数合計を取得する</remarks>
        //public int GetMembersCountSum()
        //{
        //    var model = _playerModel;
        //    if (model != null)
        //    {
        //        var ghostStructs = GhostInStaticObjectStructs;
        //        switch (model.EnemyBattlePart)
        //        {
        //            case EnemyBattlePart.Normal:
        //                var cnt = ghostStructs.Where(q => q.role.Equals(GhostRole.Normal))
        //                    .Select(q => q.membersCount).Sum();

        //                return cnt;
        //            case EnemyBattlePart.MidBoss:
        //                var cnt1 = ghostStructs.Where(q => q.role.Equals(GhostRole.MidBoss))
        //                    .Select(q => q.membersCount).Sum();

        //                return cnt1;
        //        }
        //    }
        //    Debug.LogWarning("プレイヤーモデルが存在しません");

        //    return 0;
        //}

        public void SetEnemyBattlePart(EnemyBattlePart enemyBattlePart)
        {
            if (_playerModel != null)
                _playerModel.SetEnemyBattlePart(enemyBattlePart);
        }

        public void ReplaceGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            if (_playerModel != null)
                _playerModel.ReplaceGhostInStaticObjectStructs(ghostInStaticObjectStruct);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
