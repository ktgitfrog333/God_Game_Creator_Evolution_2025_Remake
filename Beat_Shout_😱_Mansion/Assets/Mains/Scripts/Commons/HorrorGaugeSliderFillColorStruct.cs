using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// 恐怖ゲージスライダーのフィルとカラーの構造体
    /// </summary>
    /// <remarks>恐怖値の基準が特定のレベルまで到達したら<br/>
    /// そのフィル、色へ変化させる</remarks>
    [System.Serializable]
    public struct HorrorGaugeSliderFillColorStruct
    {
        /// <summary>恐怖値の割合 〜から</summary>
        [Range(0f, 1f)]
        public float from;
        /// <summary>恐怖値の割合 〜まで</summary>
        [Range(0f, 1f)]
        public float to;
        /// <summary>その色へ変化させる</summary>
        public Color gaugeColor;
        /// <summary>フィル値 〜から</summary>
        [Range(0f, 1f)]
        public float fillAmountFrom;
        /// <summary>フィル値 〜まで</summary>
        [Range(0f, 1f)]
        public float fillAmountTo;
    }
}
