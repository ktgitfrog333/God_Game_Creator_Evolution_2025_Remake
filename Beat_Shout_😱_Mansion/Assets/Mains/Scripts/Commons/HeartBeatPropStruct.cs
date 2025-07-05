using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// 心音プロパティ構造体
    /// </summary>
    [System.Serializable]
    public class HeartBeatPropStruct
    {
        /// <summary>恐怖値の割合 〜から</summary>
        [Range(0f, 1f)]
        public float from;
        /// <summary>恐怖値の割合 〜まで</summary>
        [Range(0f, 1f)]
        public float to;
        /// <summary>音量レベル</summary>
        [Range(0f, 1f)]
        public float volumeLevel;
        /// <summary>脈動値</summary>
        public float value;
    }
}
