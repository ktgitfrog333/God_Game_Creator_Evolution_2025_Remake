using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// オバケの攻撃タイプ
    /// </summary>
    public enum GhostAttackType
    {
        /// <summary>なし</summary>
        None,
        /// <summary>スロー（本）動的生成モード</summary>
        ThrowBookInstance,
        /// <summary>スロー（本）生成なしモード</summary>
        /// <remarks>レベルデザインによって予めステージ内に配置されたオブジェクトを動かす場合</remarks>
        ThrowBookNotInstance,
    }
}
