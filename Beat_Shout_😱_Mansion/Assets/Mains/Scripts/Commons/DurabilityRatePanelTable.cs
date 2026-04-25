using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// シャウトノーツの耐久率パネルテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "DurabilityRatePanelTable", menuName = "Scriptable Objects/DurabilityRatePanelTable")]
    public class DurabilityRatePanelTable : ScriptableObject
    {
        /// <summary>テキストカラー</summary>
        public Color32[] textColors;
    }
}
