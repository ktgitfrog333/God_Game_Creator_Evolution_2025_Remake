using UnityEngine;

namespace Selects.Commons
{
    /// <summary>
    /// チュートリアルパネルストラテジテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialPanelStrategyTable", menuName = "Scriptable Objects/TutorialPanelStrategyTable")]
    public class TutorialPanelStrategyTable : ScriptableObject
    {
        /// <summary>ストラテジID</summary>
        public int strategyId;
    }
}
