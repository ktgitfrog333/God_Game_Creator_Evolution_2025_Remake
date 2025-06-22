using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// MissileDirectAnimManagerBのカスタマイズ構造体
    /// </summary>
    public struct MissileDirectAnimCustomizeStruct
    {
        /// <summary>角度一致しているかのフラグ</summary>
        public bool isGoodStickDirection;
        /// <summary>トランスフォーム</summary>
        public Transform transform;
        /// <summary>有効になった際のゲーム時間</summary>
        public float onEnabledTime;
    }
}
