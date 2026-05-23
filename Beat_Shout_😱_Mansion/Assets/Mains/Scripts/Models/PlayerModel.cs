using UnityEngine;
using Mains.Commons;
using R3;
using ObservableCollections;
using System.Collections.Generic;
using System.Linq;
using Mains.Manager;
using Selects.Commons;

namespace Mains.Models
{
    /// <summary>
    /// プレイヤーのモデル
    /// </summary>
    public class PlayerModel : MonoBehaviour, IPlayerModel, IPoltergeistModel, IRhythmPartPanelModel, IHomingObjectCustomizeModel,
        IMissGhostAttackCustomizeModel, IMissileDirectAnimManagerBCustomizeModel, ICommonPanelModel, IFadeImageModel,
        IPlayerRespawnPositionModel, IHPDownDirectionModel, ICommonPanelModel1, IStageClearDirectionModel, IGhostBulletBookModel,
        ITutorialPanelModel
    {
        /// <summary>【探索／シャウトチャンス／リズム】パート情報管理テーブル</summary>
        public InteractionPartTable InteractionPartTable { get; set; }
        /// <summary>ポルターガイストのアニメーション管理テーブル</summary>
        public PoltergeistTable PoltergeistTable { get; set; }
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>オバケ管理</summary>
        private GhostContainer _ghostContainer;
        /// <summary>オバケ管理</summary>
        public GhostContainer GhostContainer => _ghostContainer != null ? _ghostContainer : _ghostContainer = new GhostContainer(new NormalGhostFactory());
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        public ObservableList<GhostInStaticObjectStruct> GhostInStaticObjectStructs => GhostContainer.Ghosts;
        /// <summary>オバケトランザクション管理</summary>
        private GhostTransaction _ghostTransaction;
        /// <summary>オバケトランザクション管理</summary>
        public GhostTransaction GhostTransaction => _ghostTransaction != null ? _ghostTransaction : _ghostTransaction = new GhostTransaction(new NormalGhostFactory());
        /// <summary>オバケの家具入居管理の構造体トランザクション</summary>
        public GhostInStaticObjectStruct TransactionGhostInStaticObjectStruct => GhostTransaction.TransactionGhostInStaticObjectStruct;
        /// <summary>プレイヤープロパティの構造体</summary>
        private PlayerPropertiesStruct _playerPropertiesStruct = new PlayerPropertiesStruct()
        {
            healthPointMax = new ReactiveCommand<int>(),
            healthPoint = new ReactiveProperty<int>(),
            horrorCount = new ReactiveCommand<float>(),
        };
        /// <summary>プレイヤープロパティの構造体</summary>
        public PlayerPropertiesStruct PlayerPropertiesStruct => _playerPropertiesStruct;
        /// <summary>プレイヤーの懐中電灯</summary>
        private Transform _playerFlashLight;
        /// <summary>プレイヤーの懐中電灯</summary>
        public Transform PlayerFlashLight => _playerFlashLight;
        /// <summary>プレイヤーの頭</summary>
        private Transform _playerHead;
        /// <summary>プレイヤーの頭</summary>
        public Transform PlayerHead => _playerHead;
        /// <summary>ターゲットクロス位置</summary>
        private readonly ReactiveCommand<Vector3> _targetCrossPosition = new();
        /// <summary>ターゲットクロス位置</summary>
        public ReactiveCommand<Vector3> TargetCrossPosition => _targetCrossPosition;
        /// <summary>バッテリーのトランスフォーム</summary>
        private Transform _batteryTransform;
        /// <summary>バッテリーのトランスフォーム</summary>
        public Transform BatteryTransform => _batteryTransform;
        /// <summary>バッテリーが選択状態か</summary>
        private bool _isSelectedBattery;
        /// <summary>バッテリーが選択状態か</summary>
        public bool IsSelectedBattery => _isSelectedBattery;
        /// <summary>[Script_xyloApi.cs]リズムパート失敗</summary>
        private readonly ReactiveCommand<bool> _isFailed = new ReactiveCommand<bool>();
        /// <summary>[Script_xyloApi.cs]リズムパート失敗</summary>
        public ReactiveCommand<bool> IsFailed => _isFailed;
        /// <summary>選択されたMissGhostAttack</summary>
        private Transform _selectedMissGhostAttackTransform;
        /// <summary>選択されたMissGhostAttack</summary>
        public Transform SelectedMissGhostAttackTransform => _selectedMissGhostAttackTransform;
        /// <summary>MissileDirectAnimManagerBのカスタマイズ構造体リスト</summary>
        private MissileDirectAnimCustomizeStruct[] _missileDirectAnimCustomizeStructs;
        /// <summary>MissileDirectAnimManagerBのカスタマイズ構造体リスト</summary>
        public MissileDirectAnimCustomizeStruct[] MissileDirectAnimCustomizeStructs => _missileDirectAnimCustomizeStructs;
        /// <summary>ターゲットクロスアンカー位置</summary>
        private Vector2 _targetCrossAnchoredPosition;
        /// <summary>ターゲットクロスアンカー位置</summary>
        public Vector2 TargetCrossAnchoredPosition => _targetCrossAnchoredPosition;
        private ReactiveCommand<int> _selectedStageIndex = new ReactiveCommand<int>();
        /// <summary>選択されたステージ番号</summary>
        public ReactiveCommand<int> SelectedStageIndex => _selectedStageIndex;
        /// <summary>ステージ開始演出が完了したか</summary>
        private ReactiveCommand<bool> _isCompletedStartDirection = new ReactiveCommand<bool>();
        /// <summary>ステージ開始演出が完了したか</summary>
        public ReactiveCommand<bool> IsCompletedStartDirection => _isCompletedStartDirection;
        /// <summary>恐怖値</summary>
        private float _horrorCount;
        /// <summary>恐怖値のカウントを停止中かのフラグ</summary>
        private ReactiveCommand<bool> _isStopHorrorCount = new ReactiveCommand<bool>();
        /// <summary>恐怖値のカウントを停止中かのフラグ</summary>
        public ReactiveCommand<bool> IsStopHorrorCount => _isStopHorrorCount;
        /// <summary>部屋の扉の前で調べる当たり判定に触れたか</summary>
        private ReactiveCommand<int> _isOnTriggerEnterSearchRangeIndex = new ReactiveCommand<int>();
        /// <summary>部屋の扉の前で調べる当たり判定に触れたか</summary>
        public ReactiveCommand<int> IsOnTriggerEnterSearchRangeIndex => _isOnTriggerEnterSearchRangeIndex;
        /// <summary>ステージ開始位置トランスフォーム</summary>
        private ReactiveCommand<Transform> _startPointTrans = new ReactiveCommand<Transform>();
        /// <summary>ステージ開始位置トランスフォーム</summary>
        public ReactiveCommand<Transform> StartPointTrans => _startPointTrans;
        /// <summary>リズムパート完了フラグ</summary>
        /// <remarks>[0]: 未完了<br/>
        /// [1]: 成功<br/>
        /// [2]: 失敗中断</remarks>
        private ReactiveCommand<int> _isCompletedRhythmPart = new ReactiveCommand<int>();
        /// <summary>リズムパート完了フラグ</summary>
        public ReactiveCommand<int> IsCompletedRhythmPart => _isCompletedRhythmPart;
        /// <summary>リズムパートでミスした時にハートが減少する演出完了フラグ
        private ReactiveCommand<bool> _isCompletedDirection = new ReactiveCommand<bool>();
        /// <summary>リズムパートでミスした時にハートが減少する演出完了フラグ
        public ReactiveCommand<bool> IsCompletedDirection => _isCompletedDirection;
        /// <summary>リズムパートが失敗で終了したか</summary>
        private ReactiveCommand<bool> _isBadEndRhythmPart = new ReactiveCommand<bool>();
        /// <summary>リズムパートが失敗で終了したか</summary>
        public ReactiveCommand<bool> IsBadEndRhythmPart => _isBadEndRhythmPart;
        /// <summary>ミッションクリアフラグ</summary>
        private ReactiveCommand<bool> _isMissionClear = new ReactiveCommand<bool>();
        /// <summary>ミッションクリアフラグ</summary>
        public ReactiveCommand<bool> IsMissionClear => _isMissionClear;
        /// <summary>ステージクリア演出完了フラグ</summary>
        private ReactiveCommand<bool> _isCompletedStageClearDirection = new ReactiveCommand<bool>();
        /// <summary>ステージクリア演出完了フラグ</summary>
        public ReactiveCommand<bool> IsCompletedStageClearDirection => _isCompletedStageClearDirection;
        /// <summary>家具とプレイヤーがお互い向き合っている状態フラグ</summary>
        /// <remarks>リズムパートが終了⇒フェードインアウト完了⇒家具とプレイヤーがお互い向き合っている状態を監視</remarks>
        private ReactiveCommand<bool> _isPostRhythmFaceOff = new ReactiveCommand<bool>();
        /// <summary>家具とプレイヤーがお互い向き合っている状態フラグ</summary>
        public ReactiveCommand<bool> IsPostRhythmFaceOff => _isPostRhythmFaceOff;
        /// <summary>視界ジャック用ゴースト</summary>
        private ReactiveCommand<Transform> _targetGhost = new ReactiveCommand<Transform>();
        /// <summary>視界ジャック用ゴースト</summary>
        public ReactiveCommand<Transform> TargetGhost => _targetGhost;
        /// <summary>オバケ移動演出の完了フラグ</summary>
        private ReactiveCommand<bool> _isCompletedMoveGhostDirection = new ReactiveCommand<bool>();
        /// <summary>オバケ移動演出の完了フラグ</summary>
        public ReactiveCommand<bool> IsCompletedMoveGhostDirection => _isCompletedMoveGhostDirection;
        /// <summary>オバケ攻撃のヒットフラグ</summary>
        private ReactiveCommand<bool> _isHitGhostAttack = new ReactiveCommand<bool>();
        /// <summary>オバケ攻撃のヒットフラグ</summary>
        public ReactiveCommand<bool> IsHitGhostAttack => _isHitGhostAttack;
        /// <summary>シャウトノーツアクティブフラグ</summary>
        private ReactiveCommand<bool> _shoutNoteActiveReactive = new ReactiveCommand<bool>();
        /// <summary>シャウトノーツアクティブフラグ</summary>
        public ReactiveCommand<bool> ShoutNoteActiveReactive => _shoutNoteActiveReactive;
        /// <summary>シャウトノーツアクティブフラグ</summary>
        private bool _shoutNoteActive;
        /// <summary>シャウトノーツアクティブフラグ</summary>
        public bool ShoutNoteActive => _shoutNoteActive;
        /// <summary>敵戦パート</summary>
        private ReactiveCommand<EnemyBattlePart> _enemyBattlePartReactive = new ReactiveCommand<EnemyBattlePart>();
        /// <summary>敵戦パート</summary>
        public ReactiveCommand<EnemyBattlePart> EnemyBattlePartReactive => _enemyBattlePartReactive;
        /// <summary>敵戦パート</summary>
        private EnemyBattlePart _enemyBattlePart;
        /// <summary>敵戦パート</summary>
        public EnemyBattlePart EnemyBattlePart => _enemyBattlePart;
        /// <summary>中ボスオバケ退治率</summary>
        private ReactiveCommand<float> _midBosskillsRateReactive = new ReactiveCommand<float>();
        /// <summary>中ボスオバケ退治率</summary>
        public ReactiveCommand<float> MidBosskillsRateReactive => _midBosskillsRateReactive;
        /// <summary>中ボスオバケ退治率</summary>
        private float _midBosskillsRate;
        /// <summary>中ボスオバケ退治率</summary>
        public float MidBosskillsRate => _midBosskillsRate;
        /// <summary>共通UIのヘッダパネルのトランスフォーム</summary>
        private RectTransform _commonHeaderPanelRectTrans;
        /// <summary>共通UIのヘッダパネルのトランスフォーム</summary>
        public RectTransform CommonHeaderPanelRectTrans => _commonHeaderPanelRectTrans;
        /// <summary>実行イベントの監視</summary>
        private ReactiveCommand<EnumEventCommand> _eventStateReactive = new ReactiveCommand<EnumEventCommand>();
        /// <summary>実行イベントの監視</summary>
        public ReactiveCommand<EnumEventCommand> EventStateReactive => _eventStateReactive;

        private void Start()
        {
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        public void SetIsSwitchPart(bool isSwitchPart)
        {
            if (InteractionPartTable != null &&
                isSwitchPart)
            {
                switch (InteractionPartTable.interactionPart.Value)
                {
                    case InteractionPart.Search:
                        InteractionPartTable.interactionPart.Value = InteractionPart.ShoutChance;

                        break;
                    case InteractionPart.ShoutChance:
                        InteractionPartTable.interactionPart.Value = InteractionPart.Search;

                        break;
                }
            }
        }

        public void SetOnActionPoltergeistPosition(Vector3 onActionPoltergeistPosition)
        {
            if (PoltergeistTable != null)
                PoltergeistTable.onActionPoltergeistPosition.Value = onActionPoltergeistPosition;
        }

        public void AddGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            GhostContainer.Add(ghostInStaticObjectStruct);
        }

        public void SetPlayerTransform(Transform transform)
        {
            _playerPropertiesStruct.transform = transform;
        }

        public void SetIsCompletedBurstGhosts(bool isCompletedBurstGhosts)
        {
            if (InteractionPartTable != null)
                InteractionPartTable.isCompletedBurstGhosts.Execute(isCompletedBurstGhosts);
        }

        public void SetDbLevel(float dbLevel)
        {
            if (InteractionPartTable != null)
                InteractionPartTable.dbLevel.Execute(dbLevel);
        }

        public void SetInteractionPart(InteractionPart interactionPart)
        {
            if (InteractionPartTable != null)
                InteractionPartTable.interactionPart.Value = interactionPart;
        }

        public void SetHealthPointMax(int healthPointMax)
        {
            _playerPropertiesStruct.healthPointMax.Execute(healthPointMax);
            _playerPropertiesStruct.healthPoint.Value = healthPointMax;
        }

        public void SetHealthPoint(int healthPoint)
        {
            _playerPropertiesStruct.healthPoint.Value = healthPoint;
        }

        public void SetTargetCrossPosition(Vector3 targetCrossPosition)
        {
            _targetCrossPosition.Execute(targetCrossPosition);
        }

        public void SetBatteryTransform(Transform batteryTransform)
        {
            if (_batteryTransform == null ||
                batteryTransform == null ||
                !_batteryTransform.Equals(batteryTransform))
            {
                _batteryTransform = batteryTransform;
            }
        }

        public void SetIsSelectedBattery(bool isSelectedBattery)
        {
            if (_isSelectedBattery != isSelectedBattery)
                _isSelectedBattery = isSelectedBattery;
        }

        public void SetTransactionGhostInStaticObjectStruct(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            GhostTransaction.SetTransactionGhostInStaticObjectStruct(ghostInStaticObjectStruct);
        }

        public void SubtractionTransactionGhostInStaticObjectStruct()
        {
            GhostTransaction.SubtractionTransactionGhostInStaticObjectStruct();
            var transaction = TransactionGhostInStaticObjectStruct;
            switch (transaction.role)
            {
                case GhostRole.MidBoss:
                    var ghosts = GhostContainer.Ghosts;
                    var beforeGhost = ghosts.Where(x => x.poltergeistViewID == transaction.poltergeistViewID)
                        .FirstOrDefault();
                    var beforeMembersCount = beforeGhost.membersCount;
                    if (beforeMembersCount <= 0)
                    {
                        _midBosskillsRate = 0f;
                        _midBosskillsRateReactive.Execute(_midBosskillsRate);
                        return;
                    }

                    // リズムパート前の人数 - リズムパート後の人数 = 退治数
                    var afterPoint = beforeMembersCount - transaction.membersCount;
                    float midBosskillsRate = (float)afterPoint / beforeMembersCount;
                    // 切り上げ処理
                    midBosskillsRate = Mathf.Ceil(midBosskillsRate * 100f) / 100f;
                    _midBosskillsRate = midBosskillsRate;
                    _midBosskillsRateReactive.Execute(_midBosskillsRate);

                    break;
            }
        }

        public void SetDefaultTransactionGhostInStaticObjectStruct()
        {
            //var role = TransactionGhostInStaticObjectStruct.role;
            GhostTransaction.SetDefaultTransactionGhostInStaticObjectStruct();
            //switch (role)
            //{
            //    case GhostRole.MidBoss:
            //        _midBosskillsRate = 0f;
            //        _midBosskillsRateReactive.Execute(_midBosskillsRate);

            //        break;
            //}
        }

        public void SetInteractionPartToSearch()
        {
            if (InteractionPartTable != null)
                InteractionPartTable.interactionPart.Value = InteractionPart.Search;
        }

        public void SubtractionHealthPoint()
        {
            if (_playerPropertiesStruct.healthPoint != null)
            {
                _playerPropertiesStruct.healthPoint.Value--;
            }
        }

        public void SetIsLockedUpdateHealthPoint(bool isLockedUpdateHealthPoint)
        {
            _playerPropertiesStruct.isLockedUpdateHealthPoint = isLockedUpdateHealthPoint;
        }

        public void SetIsFailed(bool isFailed)
        {
            _isFailed.Execute(isFailed);
        }

        public void SetSelectedMissGhostAttackTransform(Transform selectedMissGhostAttackTransform)
        {
            _selectedMissGhostAttackTransform = selectedMissGhostAttackTransform;
        }
        
        public void AddMissileDirectAnimCustomizeStructs(MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct)
        {
            List<MissileDirectAnimCustomizeStruct> missileDirectAnimCustomizeStructs = null;
            if (_missileDirectAnimCustomizeStructs == null)
            {
                missileDirectAnimCustomizeStructs = new List<MissileDirectAnimCustomizeStruct>();
                missileDirectAnimCustomizeStructs.Add(missileDirectAnimCustomizeStruct);
            }
            else
            {
                missileDirectAnimCustomizeStructs = _missileDirectAnimCustomizeStructs.ToList();
                var index = -1;
                for (int i = 0; i < missileDirectAnimCustomizeStructs.Count; i++)
                {
                    if (missileDirectAnimCustomizeStructs[i].transform.GetInstanceID() == missileDirectAnimCustomizeStruct.transform.GetInstanceID())
                    {
                        index = i;
                        break;
                    }
                }
                if (-1 < index)
                {
                    // 角度一致しているかのフラグのみ更新する
                    var tmpMissileDirectAnimCustomizeStruct = missileDirectAnimCustomizeStructs[index];
                    tmpMissileDirectAnimCustomizeStruct.isGoodStickDirection = missileDirectAnimCustomizeStruct.isGoodStickDirection;
                    missileDirectAnimCustomizeStructs[index] = tmpMissileDirectAnimCustomizeStruct;
                }
                else
                {
                    missileDirectAnimCustomizeStructs.Add(missileDirectAnimCustomizeStruct);
                }
            }

            _missileDirectAnimCustomizeStructs = missileDirectAnimCustomizeStructs.ToArray();
        }

        public void SetTargetCrossAnchoredPosition(Vector2 targetCrossAnchoredPosition)
        {
            _targetCrossAnchoredPosition = targetCrossAnchoredPosition;
        }

        public void AddOrSetOnEnabledTime(MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct)
        {
            List<MissileDirectAnimCustomizeStruct> missileDirectAnimCustomizeStructs = null;
            if (_missileDirectAnimCustomizeStructs == null)
            {
                missileDirectAnimCustomizeStructs = new List<MissileDirectAnimCustomizeStruct>();
                missileDirectAnimCustomizeStructs.Add(missileDirectAnimCustomizeStruct);
            }
            else
            {
                missileDirectAnimCustomizeStructs = _missileDirectAnimCustomizeStructs.ToList();
                var index = -1;
                for (int i = 0; i < missileDirectAnimCustomizeStructs.Count; i++)
                {
                    if (missileDirectAnimCustomizeStructs[i].transform.GetInstanceID() == missileDirectAnimCustomizeStruct.transform.GetInstanceID())
                    {
                        index = i;
                        break;
                    }
                }
                if (-1 < index)
                {
                    // 有効になった際のゲーム時間のみ更新する
                    var tmpMissileDirectAnimCustomizeStruct = missileDirectAnimCustomizeStructs[index];
                    tmpMissileDirectAnimCustomizeStruct.onEnabledTime = missileDirectAnimCustomizeStruct.onEnabledTime;
                    missileDirectAnimCustomizeStructs[index] = tmpMissileDirectAnimCustomizeStruct;
                }
                else
                {
                    missileDirectAnimCustomizeStructs.Add(missileDirectAnimCustomizeStruct);
                }
            }

            _missileDirectAnimCustomizeStructs = missileDirectAnimCustomizeStructs.ToArray();
        }

        public bool IsFrontMissileDirectAnim(Transform transform)
        {
            if (_missileDirectAnimCustomizeStructs == null ||
                _missileDirectAnimCustomizeStructs.Length < 1)
            {
                return false;
            }
            var orderStructs = _missileDirectAnimCustomizeStructs.Where(x => 0f < x.onEnabledTime)
                .OrderBy(x => x.onEnabledTime)
                .ToArray();
            if (0 < orderStructs.Length)
            {
                var orderStruct = orderStructs[0];

                return transform.GetInstanceID() == orderStruct.transform.GetInstanceID();
            }

            return false;
        }

        public void SetSelectedStageIndex(int selectedStageIndex)
        {
            _selectedStageIndex.Execute(selectedStageIndex);
        }

        public void SetIsCompletedStartDirection(bool isCompleted)
        {
            _isCompletedStartDirection.Execute(isCompleted);
        }

        public void AddHorrorCount(float horrorCount)
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var owner = manager.LevelOwner;
            if (owner == null)
                return;

            var max = owner.HorrorCountMax;
            if (max == null)
                return;

            if (max.Value == _horrorCount)
                // 1度MAXへ到達した後は更新しない
                return;

            var tmpHorrorCount = _horrorCount + horrorCount;
            if (max.Value <= tmpHorrorCount)
            {
                tmpHorrorCount = max.Value;
            }
            _horrorCount = tmpHorrorCount;
            _playerPropertiesStruct.horrorCount.Execute(_horrorCount);
        }

        public void SetIsStopHorrorCount(bool isStopHorrorCount)
        {
            _isStopHorrorCount.Execute(isStopHorrorCount);
        }

        public void SetIsOnTriggerEnterSearchRangeIndex(int isOnTriggerEnterSearchRangeIndex)
        {
            _isOnTriggerEnterSearchRangeIndex.Execute(isOnTriggerEnterSearchRangeIndex);
        }

        public void SetStartPointTrans(Transform startPointTrans)
        {
            _startPointTrans.Execute(startPointTrans);
        }

        public void SetIsCompletedRhythmPart(int isCompletedRhythmPart)
        {
            _isCompletedRhythmPart.Execute(isCompletedRhythmPart);
        }

        public void SetIsCompletedDirection(bool isCompleted)
        {
            _isCompletedDirection.Execute(isCompleted);
        }

        public void SetIsBadEndRhythmPart(bool isBadEndRhythmPart)
        {
            if (!_playerPropertiesStruct.isLockedUpdateHealthPoint)
            {
                // 多段ヒット防止
                _playerPropertiesStruct.isLockedUpdateHealthPoint = true;
                _isBadEndRhythmPart.Execute(isBadEndRhythmPart);
            }
        }

        public void SetIsMissionClear(bool isMissionClear)
        {
            _isMissionClear.Execute(isMissionClear);
        }

        public void SetIsCompletedStageClearDirection(bool isCompletedStageClearDirection)
        {
            _isCompletedStageClearDirection.Execute(isCompletedStageClearDirection);
        }

        public void SetIsPostRhythmFaceOff(bool isPostRhythmFaceOff)
        {
            _isPostRhythmFaceOff.Execute(isPostRhythmFaceOff);
        }

        public void SetTargetGhost(Transform targetGhost)
        {
            _targetGhost.Execute(targetGhost);
        }

        public void SetIsCompletedMoveGhostDirection(bool isCompletedMoveGhostDirection)
        {
            _isCompletedMoveGhostDirection.Execute(isCompletedMoveGhostDirection);
        }

        public void SetIsHitGhostAttack(bool isHitGhostAttack)
        {
            _isHitGhostAttack.Execute(isHitGhostAttack);
        }

        public void SetShoutNoteActive(bool shoutNoteActive)
        {
            if (_shoutNoteActive != shoutNoteActive)
            {
                _shoutNoteActive = shoutNoteActive;
                _shoutNoteActiveReactive.Execute(shoutNoteActive);
            }
        }

        public void SetEnemyBattlePart(EnemyBattlePart enemyBattlePart)
        {
            _enemyBattlePart = enemyBattlePart;
            _enemyBattlePartReactive.Execute(enemyBattlePart);
        }

        public void ReplaceGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            GhostContainer.Replace(ghostInStaticObjectStruct);
        }

        public void SetMidBosskillsRate(float midBosskillsRate)
        {
            _midBosskillsRate = midBosskillsRate;
            _midBosskillsRateReactive.Execute(_midBosskillsRate);
        }

        public void SetPlayerFlashLight(Transform playerFlashLight)
        {
            _playerFlashLight = playerFlashLight;
        }

        public void SetCommonHeaderPanelRectTrans(RectTransform commonHeaderPanelRectTrans)
        {
            _commonHeaderPanelRectTrans = commonHeaderPanelRectTrans;
        }

        public void SetEventState(EnumEventCommand eventState)
        {
            _eventStateReactive.Execute(eventState);
        }

        public void SetPlayerHead(Transform playerHead)
        {
            _playerHead = playerHead;
        }
    }

    /// <summary>
    /// プレイヤーのモデルのインターフェース
    /// </summary>
    public interface IPlayerModel
    {
        /// <summary>
        /// パート切り替え入力をセット
        /// </summary>
        /// <param name="isSwitchPart">パート切り替え入力</param>
        public void SetIsSwitchPart(bool isSwitchPart);
        /// <summary>
        /// パート切り替え入力をセット
        /// </summary>
        /// <param name="interactionPart">【探索／シャウトチャンス／リズム】パート</param>
        public void SetInteractionPart(InteractionPart interactionPart);
        /// <summary>
        /// プレイヤーのトランスフォームをセット
        /// </summary>
        /// <param name="transform">プレイヤーのトランスフォーム</param>
        public void SetPlayerTransform(Transform transform);
        /// <summary>
        /// ゴーストが飛び出してくる演出の完了をセット
        /// </summary>
        /// <param name="isCompletedBurstGhosts">ゴーストが飛び出してくる演出の完了</param>
        public void SetIsCompletedBurstGhosts(bool isCompletedBurstGhosts);
        /// <summary>
        /// デシベルレベルをセット
        /// </summary>
        /// <param name="dbLevel">デシベルレベル</param>
        public void SetDbLevel(float dbLevel);
        /// <summary>
        /// プレイヤーの最大HPをセット
        /// </summary>
        /// <param name="healthPointMax">プレイヤーの最大HP</param>
        public void SetHealthPointMax(int healthPointMax);
        /// <summary>
        /// プレイヤーのHPをセット
        /// </summary>
        /// <param name="healthPoint">プレイヤーのHP</param>
        public void SetHealthPoint(int healthPoint);
        /// <summary>
        /// バッテリーのトランスフォームをセット
        /// </summary>
        /// <param name="batteryTransform">バッテリーのトランスフォーム</param>
        public void SetBatteryTransform(Transform batteryTransform);
        /// <summary>
        /// プレイヤーのHP更新ロックをセット
        /// </summary>
        /// <param name="isLockedUpdateHealthPoint">プレイヤーのHP更新ロック</param>
        public void SetIsLockedUpdateHealthPoint(bool isLockedUpdateHealthPoint);
        /// <summary>
        /// 恐怖値を加算
        /// </summary>
        /// <param name="horrorCount">恐怖値</param>
        public void AddHorrorCount(float horrorCount);
        /// <summary>
        /// 恐怖値のカウントを停止中かのフラグをセット
        /// </summary>
        /// <param name="isStopHorrorCount">恐怖値のカウントを停止中かのフラグ</param>
        public void SetIsStopHorrorCount(bool isStopHorrorCount);
        /// <summary>
        /// 家具とプレイヤーがお互い向き合っている状態フラグをセット
        /// </summary>
        /// <param name="isPostRhythmFaceOff">家具とプレイヤーがお互い向き合っている状態フラグ</param>
        public void SetIsPostRhythmFaceOff(bool isPostRhythmFaceOff);
        /// <summary>
        /// プレイヤーの懐中電灯をセット
        /// </summary>
        /// <param name="playerFlashLight">プレイヤーの懐中電灯</param>
        public void SetPlayerFlashLight(Transform playerFlashLight);
        /// <summary>
        /// プレイヤーの頭をセット
        /// </summary>
        /// <param name="playerHead">プレイヤーの頭</param>
        public void SetPlayerHead(Transform playerHead);
    }

    /// <summary>
    /// ポルターガイストのインターフェース
    /// </summary>
    public interface IPoltergeistModel
    {
        /// <summary>
        /// ポルターガイストの発生位置をセット
        /// </summary>
        /// <param name="onActionPoltergeistPosition">ポルターガイストの発生位置</param>
        public void SetOnActionPoltergeistPosition(Vector3 onActionPoltergeistPosition);
        /// <summary>
        /// オバケの家具入居管理の構造体リストへ追加
        /// </summary>
        /// <param name="ghostInStaticObjectStruct">オバケの家具入居管理の構造体</param>
        public void AddGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct);
        /// <summary>
        /// ゴーストが飛び出してくる演出の完了をセット
        /// </summary>
        /// <param name="isCompletedBurstGhosts">ゴーストが飛び出してくる演出の完了</param>
        public void SetIsCompletedBurstGhosts(bool isCompletedBurstGhosts);
        /// <summary>
        /// オバケの家具入居管理の構造体トランザクションをセット
        /// </summary>
        /// <param name="ghostInStaticObjectStruct">オバケの家具入居管理の構造体</param>
        public void SetTransactionGhostInStaticObjectStruct(GhostInStaticObjectStruct ghostInStaticObjectStruct);
        /// <summary>
        /// オバケの家具入居管理の構造体トランザクションへデフォルトをセット
        /// </summary>
        public void SetDefaultTransactionGhostInStaticObjectStruct();
        /// <summary>
        /// 探索パート切り替え入力をセット
        /// </summary>
        public void SetInteractionPartToSearch();
        /// <summary>
        /// リズムパート完了フラグをセット
        /// </summary>
        /// <param name="isCompletedRhythmPart">リズムパート完了フラグ</param>
        public void SetIsCompletedRhythmPart(int isCompletedRhythmPart);
        /// <summary>
        /// ミッションクリアフラグをセット
        /// </summary>
        /// <param name="isMissionClear">ミッションクリアフラグ</param>
        public void SetIsMissionClear(bool isMissionClear);
        /// <summary>
        /// 視界ジャック用ゴーストをセット
        /// </summary>
        /// <param name="targetGhost">視界ジャック用ゴースト</param>
        public void SetTargetGhost(Transform targetGhost);
        /// <summary>
        /// オバケ移動演出の完了フラグをセット
        /// </summary>
        /// <param name="isCompletedMoveGhostDirection">オバケ移動演出の完了フラグ</param>
        public void SetIsCompletedMoveGhostDirection(bool isCompletedMoveGhostDirection);
        /// <summary>
        /// 敵戦パートをセット
        /// </summary>
        /// <param name="enemyBattlePart">敵戦パート</param>
        public void SetEnemyBattlePart(EnemyBattlePart enemyBattlePart);
        /// <summary>
        /// オバケの家具入居管理のデータクラスリストにて対象レコードを更新する
        /// </summary>
        /// <param name="ghostInStaticObjectStruct">オバケの家具入居管理のデータクラス</param>
        public void ReplaceGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct);
        /// <summary>
        /// 中ボスオバケ退治率をセット
        /// </summary>
        /// <param name="midBosskillsRate">中ボスオバケ退治率</param>
        public void SetMidBosskillsRate(float midBosskillsRate);
    }

    /// <summary>
    /// リズムパートパネルのインターフェース
    /// </summary>
    public interface IRhythmPartPanelModel
    {
        /// <summary>
        /// ターゲットクロス位置をセット
        /// </summary>
        /// <param name="targetCrossPosition">ターゲットクロス位置</param>
        public void SetTargetCrossPosition(Vector3 targetCrossPosition);
        ///// <summary>
        ///// バッテリーのトランスフォームをセット
        ///// </summary>
        ///// <param name="batteryTransform">バッテリーのトランスフォーム</param>
        //public void SetBatteryTransform(Transform batteryTransform);
        /// <summary>
        /// バッテリーの選択状態をセット
        /// </summary>
        /// <param name="isSelectedBattery">バッテリーの選択状態</param>
        public void SetIsSelectedBattery(bool isSelectedBattery);
        /// <summary>
        /// 選択されたMissGhostAttackをセット
        /// </summary>
        /// <param name="selectedMissGhostAttackTransform">選択されたMissGhostAttack</param>
        public void SetSelectedMissGhostAttackTransform(Transform selectedMissGhostAttackTransform);
        /// <summary>
        /// ターゲットクロスアンカー位置をセット
        /// </summary>
        /// <param name="targetCrossAnchoredPosition">ターゲットクロスアンカー位置</param>
        public void SetTargetCrossAnchoredPosition(Vector2 targetCrossAnchoredPosition);
    }

    /// <summary>
    /// オブジェクトをホーミングする処理のカスタマイズインターフェース
    /// </summary>
    public interface IHomingObjectCustomizeModel
    {
        /// <summary>
        /// オバケの家具入居管理の構造体から減算
        /// </summary>
        public void SubtractionTransactionGhostInStaticObjectStruct();
        /// <summary>
        /// [Script_xyloApi.cs]リズムパート失敗をセット
        /// </summary>
        public void SetIsFailed(bool isFailed);
    }


    /// <summary>
    /// MissGhostAttackのカスタマイズモデルインターフェース
    /// </summary>
    public interface IMissGhostAttackCustomizeModel
    {
        /// <summary>
        /// プレイヤーのHPを減らす
        /// </summary>
        public void SubtractionHealthPoint();
        /// <summary>
        /// リズムパートが失敗で終了したかをセット
        /// </summary>
        /// <param name="isBadEndRhythmPart">リズムパートが失敗で終了したか</param>
        public void SetIsBadEndRhythmPart(bool isBadEndRhythmPart);
    }
    
    /// <summary>
    /// MissileDirectAnimManagerBのカスタマイズモデルインターフェース
    /// </summary>
    public interface IMissileDirectAnimManagerBCustomizeModel
    {
        /// <summary>
        /// MissileDirectAnimManagerBのカスタマイズ構造体リストへ追加
        /// </summary>
        /// <param name="missileDirectAnimCustomizeStruct">MissileDirectAnimManagerBのカスタマイズ構造体</param>
    	public void AddMissileDirectAnimCustomizeStructs(MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct);
        /// <summary>
        /// 有効になった際のゲーム時間をセット
        /// </summary>
        /// <param name="missileDirectAnimCustomizeStruct">MissileDirectAnimManagerBのカスタマイズ構造体</param>
        public void AddOrSetOnEnabledTime(MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct);
        /// <summary>
        /// MissileDirectAnimManagerBが最前面に存在するか
        /// </summary>
        /// <param name="transform">トランスフォーム</param>
        /// <returns>最前面に存在するか</returns>
        public bool IsFrontMissileDirectAnim(Transform transform);
        /// <summary>
        /// シャウトノーツアクティブフラグをセット
        /// </summary>
        /// <param name="shoutNoteActive">シャウトノーツアクティブフラグ</param>
        public void SetShoutNoteActive(bool shoutNoteActive);
    }

    /// <summary>
    /// 共通UIのモデルインターフェース
    /// </summary>
    /// <remarks>セレクトシーン用</remarks>
    public interface ICommonPanelModel
    {
        /// <summary>
        /// 選択されたステージ番号をセット
        /// </summary>
        /// <param name="selectedStageIndex">選択されたステージ番号</param>
        public void SetSelectedStageIndex(int selectedStageIndex);
        /// <summary>
        /// 部屋の扉の前で調べる当たり判定に触れた階層をセット
        /// </summary>
        /// <param name="isOnTriggerEnterSearchRangeIndex">部屋の扉の前で調べる当たり判定に触れた階層</param>
        public void SetIsOnTriggerEnterSearchRangeIndex(int isOnTriggerEnterSearchRangeIndex);
        /// <summary>
        /// 共通UIのヘッダパネルのトランスフォームをセット
        /// </summary>
        /// <param name="commonHeaderPanelRectTrans">共通UIのヘッダパネルのトランスフォーム</param>
        public void SetCommonHeaderPanelRectTrans(RectTransform commonHeaderPanelRectTrans);
    }

    /// <summary>
    /// フェードイメージのモデルインターフェース
    /// </summary>
    public interface IFadeImageModel
    {
        /// <summary>
        /// ステージ開始演出が完了したかをセット
        /// </summary>
        /// <param name="isCompleted">ステージ開始演出が完了したか</param>
        public void SetIsCompletedStartDirection(bool isCompleted);
    }

    /// <summary>
    /// リスポーン地点モデルインターフェース
    /// </summary>
    public interface IPlayerRespawnPositionModel
    {
        /// <summary>
        /// ステージ開始位置トランスフォームをセット
        /// </summary>
        /// <param name="startPointTrans">ステージ開始位置トランスフォーム</param>
        public void SetStartPointTrans(Transform startPointTrans);
    }

    /// <summary>
    /// ハートが減少する演出モデルインターフェース
    /// </summary>
    public interface IHPDownDirectionModel
    {
        /// <summary>
        /// リズムパートでミスした時にハートが減少する演出完了フラグをセット
        /// </summary>
        /// <param name="isCompleted">リズムパートでミスした時にハートが減少する演出完了フラグ</param>
        public void SetIsCompletedDirection(bool isCompleted);
        /// <summary>
        /// プレイヤーのHPを減らす
        /// </summary>
        public void SubtractionHealthPoint();
    }

    /// <summary>
    /// 共通UIのモデルインターフェース
    /// </summary>
    /// <remarks>メインシーン用</remarks>
    public interface ICommonPanelModel1
    {
        /// <summary>
        /// ミッションクリアフラグをセット
        /// </summary>
        /// <param name="isMissionClear">ミッションクリアフラグ</param>
        public void SetIsMissionClear(bool isMissionClear);
    }

    /// <summary>
    /// ステージクリア演出のモデルインターフェース
    /// </summary>
    public interface IStageClearDirectionModel
    {
        /// <summary>
        /// ステージクリア演出完了フラグをセット
        /// </summary>
        /// <param name="isCompletedStageClearDirection">ステージクリア演出完了フラグ</param>
        public void SetIsCompletedStageClearDirection(bool isCompletedStageClearDirection);
    }

    /// <summary>
    /// オバケ弾：本のモデルインターフェース
    /// </summary>
    public interface IGhostBulletBookModel
    {
        /// <summary>
        /// オバケ攻撃のヒットフラグをセット
        /// </summary>
        /// <param name="isHitGhostAttack">オバケ攻撃のヒットフラグ</param>
        public void SetIsHitGhostAttack(bool isHitGhostAttack);
    }

    /// <summary>
    /// チュートリアルパネルのモデルインターフェース
    /// </summary>
    public interface ITutorialPanelModel
    {
        /// <summary>
        /// 敵戦パートをセット
        /// </summary>
        /// <param name="enemyBattlePart">敵戦パート</param>
        public void SetEnemyBattlePart(EnemyBattlePart enemyBattlePart);
    }
}
