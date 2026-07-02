using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// リズムパートのテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "RhythmPartTable", menuName = "Scriptable Objects/RhythmPartTable")]
    public class RhythmPartTable : ScriptableObject
    {
        /// <summary>プレイヤーへ強制的にヒットしない形にする</summary>
        public bool isNoHitPlayerForceMode;
    }
}
