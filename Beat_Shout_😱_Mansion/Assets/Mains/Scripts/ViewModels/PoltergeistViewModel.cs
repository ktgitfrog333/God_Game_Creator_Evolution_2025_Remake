using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;
using ObservableCollections;
using R3.Triggers;

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
        private ReactiveCommand<InteractionPart> _interactionPart = new ReactiveCommand<InteractionPart>();
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveCommand<InteractionPart> InteractionPart => _interactionPart;
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

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
