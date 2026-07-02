using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// プレイヤーのカスタマイズテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerCustomizeTable", menuName = "Scriptable Objects/PlayerCustomizeTable")]
    public class PlayerCustomizeTable : ScriptableObject
    {
        /// <summary>リズムパートのテーブル</summary>
        [SerializeField] private RhythmPartTable rhythmPartTable;
        /// <summary>プレイヤーへ強制的にヒットしない形にする</summary>
        public bool IsNoHitPlayerForceMode => rhythmPartTable.isNoHitPlayerForceMode;
    }
}
