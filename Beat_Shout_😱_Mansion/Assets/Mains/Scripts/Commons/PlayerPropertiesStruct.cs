using R3;
using UnityEngine;

namespace Mains.Commons
{
    [System.Serializable]
    /// <summary>
    /// プレイヤープロパティの構造体
    /// </summary>
    public struct PlayerPropertiesStruct
    {
        /// <summary>プレイヤーのトランスフォーム</summary>
        public Transform transform;
        /// <summary>プレイヤーのHP</summary>
        public ReactiveProperty<int> healthPoint;
        /// <summary>プレイヤーの最大HP</summary>
        public ReactiveCommand<int> healthPointMax;
        /// <summary>プレイヤーのHP更新ロック</summary>
        public bool isLockedUpdateHealthPoint;
        /// <summary>恐怖値</summary>
        public ReactiveCommand<float> horrorCount;
    }
}
