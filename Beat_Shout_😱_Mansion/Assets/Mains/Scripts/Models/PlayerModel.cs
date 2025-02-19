using UnityEngine;
using Mains.Commons;
using R3;

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

        private void Start()
        {
            Observable.EveryUpdate()
                .Select(_ => InteractionPartTable)
                .Where(q => q != null)
                .Take(1)
                .Subscribe(q =>
                {
                    // TODO:探索パート⇔シャウトチャンスパート切り替えが視覚化されていない間は残す
                    InteractionPartTable.interactionPart.Subscribe(q => Debug.Log(q))
                        .AddTo(ref _disposableBag);
                    InteractionPartTable.interactionPart.Value = InteractionPart.Search;
                })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Select(_ => PoltergeistTable)
                .Where(q => q != null)
                .Take(1)
                .Subscribe(q =>
                {
                    q.isOnActionPoltergeist.Subscribe(q =>
                    {
                        Debug.Log(q);
                        // 有効を無効へ戻す手段がないためここで無効に戻す
                        if (q)
                            PoltergeistTable.isOnActionPoltergeist.Value = false;
                    })
                    .AddTo(ref _disposableBag);
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

        public void SetIsOnActionPoltergeist(bool isOnActionPoltergeist)
        {
            if (PoltergeistTable != null &&
                isOnActionPoltergeist)
                PoltergeistTable.isOnActionPoltergeist.Value = isOnActionPoltergeist;
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
    }

    /// <summary>
    /// ポルターガイストのインターフェース
    /// </summary>
    public interface IPoltergeistModel
    {
        /// <summary>
        /// ポルターガイストが発生をセット
        /// </summary>
        /// <param name="isOnActionPoltergeist">ポルターガイストが発生</param>
        public void SetIsOnActionPoltergeist(bool isOnActionPoltergeist);
    }
}
