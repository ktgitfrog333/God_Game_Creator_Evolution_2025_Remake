using UnityEngine;
using Mains.Commons;
using R3;

namespace Mains.Models
{
    /// <summary>
    /// プレイヤーのモデル
    /// </summary>
    public class PlayerModel : MonoBehaviour, IPlayerModel
    {
        /// <summary>【探索／シャウトチャンス／リズム】パート情報管理テーブル</summary>
        public InteractionPartTable InteractionPartTable { get; set; }
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

        private void OnDestroy()
        {
            _disposableBag.Dispose();
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
}
