using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// Directional Lightをオーバーライドする構造体
    /// </summary>
    [System.Serializable]
    public struct OverridesDirectionalLightStruct
    {
        public bool 独自Directional_Light設定を有効にする;
        // 設定値
        public float intensity;
        public float shadowStrength;
        public Color lightColor;
        [Tooltip("オンで毎フレーム実行／オフで一度のみ")]
        public bool Updateされる度に更新;
        public Light Directional_Lightのライト情報;
    }
}
