using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// プレイヤーのテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerTable", menuName = "Scriptable Objects/PlayerTable")]
    public class PlayerTable : ScriptableObject
    {
        /// <summary>プレイヤーのシャウト判定用レイの距離（デフォルト）</summary>
        public float rayLengthDefault;
        /// <summary>目線の距離</summary>
        public float aimDistance = 10f;
        /// <summary>プレイヤーのシャウト判定用レイの距離（チュートリアル）_シャウト練習</summary>
        public float rayLengthTutorialShoutPractice;
        /// <summary>プレイヤーのシャウト判定用レイの距離（チュートリアル）_シャウト本番</summary>
        public float rayLengthTutorialShoutPerformance;
    }
}
