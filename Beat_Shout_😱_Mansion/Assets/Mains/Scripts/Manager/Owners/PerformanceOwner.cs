using UnityEngine;

namespace Mains.Manager.Owners
{
    /// <summary>
    /// パフォーマンスオーナー
    /// </summary>
    public class PerformanceOwner : MonoBehaviour
    {
        /// <summary>パフォーマンスオーナー設定</summary>
        [SerializeField] private PerformanceSettings performanceSettings;

        private void Start()
        {
#if !UNITY_EDITOR
            // ビルド時のみフレームレートを設定
            Application.targetFrameRate = performanceSettings.targetFrameRate;
#endif
        }
    }

    /// <summary>
    /// パフォーマンスオーナー設定
    /// </summary>
    [System.Serializable]
    public class PerformanceSettings
    {
        /// <summary>固定フレームレート</summary>
        public int targetFrameRate;
    }
}
