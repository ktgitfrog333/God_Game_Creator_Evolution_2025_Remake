using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// オブジェクトをホーミングする処理のカスタマイズテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "HomingObjectCustomizeTable", menuName = "Scriptable Objects/HomingObjectCustomizeTable")]
    public class HomingObjectCustomizeTable : ScriptableObject
    {
        [Tooltip("Assets/Mains/Prefabs/UIs/DurabilityRatePanel.prefabをセットしておく。")]
        public Transform durabilityRatePanelPrefab;
    }
}
