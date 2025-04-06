using UnityEngine;
using Mains.Commons;
using R3;
using ObservableCollections;

namespace Mains.Models
{
    /// <summary>
    /// プレイヤーのモデル
    /// </summary>
    public class PlayerModel : MonoBehaviour, IPlayerModel, IPoltergeistModel
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
        /// <summary>プレイヤープロパティの構造体</summary>
        private PlayerPropertiesStruct _playerPropertiesStruct = new PlayerPropertiesStruct()
        {
            healthPointMax = new ReactiveCommand<int>(),
            healthPoint = new ReactiveCommand<int>(),
        };
        /// <summary>プレイヤープロパティの構造体</summary>
        public PlayerPropertiesStruct PlayerPropertiesStruct => _playerPropertiesStruct;

        private void Start()
        {
            Observable.EveryUpdate()
                .Select(_ => InteractionPartTable)
                .Where(q => q != null)
                .Take(1)
                .Subscribe(q =>
                {
                    // TODO:探索パート⇔シャウトチャンスパート切り替えが視覚化されていない間は残す
                    InteractionPartTable.interactionPart.Subscribe(x => Debug.Log($"interactionPart: [{x}]"))
                        .AddTo(ref _disposableBag);
                    InteractionPartTable.dbLevel.Subscribe(x => Debug.Log($"dbLevel: [{x}]"))
                        .AddTo(ref _disposableBag);
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
            _playerPropertiesStruct.healthPoint.Execute(healthPointMax);
        }

        public void SetHealthPoint(int healthPoint)
        {
            _playerPropertiesStruct.healthPoint.Execute(healthPoint);
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
    }
}
