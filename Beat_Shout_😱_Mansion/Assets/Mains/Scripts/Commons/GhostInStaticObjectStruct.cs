using UnityEngine;
using R3;

namespace Mains.Commons
{
    /// <summary>
    /// オバケの家具入居管理の構造体
    /// </summary>
    [System.Serializable]
    public struct GhostInStaticObjectStruct
    {
        /// <summary>ポルターガイストのビューID</summary>
        public int poltergeistViewID;
        /// <summary>オバケ団体ID</summary>
        public ReactiveProperty<string> ghostTeamID;
        /// <summary>利用ステータス（空室／使用中）</summary>
        public UseStatus useStatus;
        /// <summary>利用人数</summary>
        public int membersCount;
        /// <summary>オバケの攻撃タイプ</summary>
        public GhostAttackType attackType;
    }
}
