using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// 恐怖フレームカラー構造体
    /// </summary>
    /// <remarks>恐怖値の基準が特定のレベルまで到達したら<br/>
    /// その色へ変化させる</remarks>
    [System.Serializable]
    public class HorrorMokuFrameColorStruct
    {
        /// <summary>恐怖値の割合 〜から</summary>
        [Range(0f, 1f)]
        public float from;
        /// <summary>恐怖値の割合 〜まで</summary>
        [Range(0f, 1f)]
        public float to;
        /// <summary>その色へ変化させる</summary>
        public Color frameColor;
        /// <summary>変化アニメーション終了時間</summary>
        public float duration;
        /// <summary>変化アニメーションをループさせるか</summary>
        public bool isLoop;
        [Header("ループ用のオプション")]
        /// <summary>その色へ変化させる（ループ用ベースカラー1）</summary>
        public Color frameFromColor;
        /// <summary>その色へ変化させる（ループ用ベースカラー2）</summary>
        public Color frameToColor;
    }
}
