using UnityEngine;
using R3;

namespace Mains.Commons
{
    /// <summary>
    /// 【探索／シャウトチャンス／リズム】パート情報管理テーブル
    /// </summary>
    [CreateAssetMenu(fileName = "InteractionPartTable", menuName = "Scriptable Objects/InteractionPartTable")]
    public class InteractionPartTable : ScriptableObject
    {
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveProperty<InteractionPart> interactionPart = new ReactiveProperty<InteractionPart>(InteractionPart.None);
        /// <summary>デシベルレベル</summary>
        public ReactiveCommand<float> dbLevel = new ReactiveCommand<float>();
        /// <summary>ゴーストが飛び出してくる演出の完了</summary>
        public ReactiveCommand<bool> isCompletedBurstGhosts = new ReactiveCommand<bool>();
    }
}
