using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// MissGhostAttackのカスタマイズテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "MissGhostAttackCustomizeTable", menuName = "Scriptable Objects/MissGhostAttackCustomizeTable")]
    public class MissGhostAttackCustomizeTable : ScriptableObject
    {
        /// <summary>リズムパートのテーブル</summary>
        [SerializeField] private RhythmPartTable rhythmPartTable;
        /// <summary>プレイヤーへ強制的にヒットしない形にする</summary>
        public bool IsNoHitPlayerForceMode => rhythmPartTable.isNoHitPlayerForceMode;
    }
}
