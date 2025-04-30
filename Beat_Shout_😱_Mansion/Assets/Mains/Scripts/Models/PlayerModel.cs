using UnityEngine;
using Mains.Commons;
using R3;
using ObservableCollections;

namespace Mains.Models
{
    /// <summary>
    /// プレイヤーのモデル
    /// </summary>
    public class PlayerModel : MonoBehaviour, IPlayerModel, IPoltergeistModel, IRhythmPartPanelModel, IHomingObjectCustomizeModel, IMissGhostAttackCustomizeModel
    {
        /// <summary>【探索／シャウトチャンス／リズム】パート情報管理テーブル</summary>
        public InteractionPartTable InteractionPartTable { get; set; }
        /// <summary>ポルターガイストのアニメーション管理テーブル</summary>
        public PoltergeistTable PoltergeistTable { get; set; }
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        private ObservableList<GhostInStaticObjectStruct> _ghostInStaticObjectStructs = new ObservableList<GhostInStaticObjectStruct>();
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        public ObservableList<GhostInStaticObjectStruct> GhostInStaticObjectStructs => _ghostInStaticObjectStructs;
        /// <summary>オバケの家具入居管理の構造体トランザクション</summary>
        private GhostInStaticObjectStruct _transactionGhostInStaticObjectStruct;
        /// <summary>オバケの家具入居管理の構造体トランザクション</summary>
        public GhostInStaticObjectStruct TransactionGhostInStaticObjectStruct => _transactionGhostInStaticObjectStruct;
        /// <summary>プレイヤープロパティの構造体</summary>
        private PlayerPropertiesStruct _playerPropertiesStruct = new PlayerPropertiesStruct()
        {
            healthPointMax = new ReactiveCommand<int>(),
            healthPoint = new ReactiveProperty<int>(),
        };
        /// <summary>プレイヤープロパティの構造体</summary>
        public PlayerPropertiesStruct PlayerPropertiesStruct => _playerPropertiesStruct;
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

        private void Start()
        {
            Observable.EveryUpdate()
                .Select(_ => InteractionPartTable)
                .Where(q => q != null)
                .Take(1)
                .Subscribe(q =>
                {
                    InteractionPartTable.interactionPart.Value = InteractionPart.Search;
                })
                .AddTo(ref _disposableBag);
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
            _ghostInStaticObjectStructs.Add(ghostInStaticObjectStruct);
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
            _transactionGhostInStaticObjectStruct = ghostInStaticObjectStruct;
        }

        public void SubtractionTransactionGhostInStaticObjectStruct()
        {
            var ghostStuct = _transactionGhostInStaticObjectStruct;
            if (ghostStuct.ghostTeamID != null &&
                !string.IsNullOrEmpty(ghostStuct.ghostTeamID.Value) &&
                ghostStuct.useStatus.Equals(UseStatus.Using) &&
                0 < ghostStuct.membersCount)
            {
                ghostStuct.membersCount--;
            }
            _transactionGhostInStaticObjectStruct = ghostStuct;
        }

        public void SetDefaultTransactionGhostInStaticObjectStruct()
        {
            _transactionGhostInStaticObjectStruct = new GhostInStaticObjectStruct();
        }

        public void SetInteractionPartToSearch()
        {
            if (InteractionPartTable != null)
                InteractionPartTable.interactionPart.Value = InteractionPart.Search;
        }

        public void SubtractionHealthPoint()
        {
            if (_playerPropertiesStruct.healthPoint != null &&
                !_playerPropertiesStruct.isLockedUpdateHealthPoint)
            {
                // 多段ヒット防止
                _playerPropertiesStruct.isLockedUpdateHealthPoint = true;
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
    }
}
