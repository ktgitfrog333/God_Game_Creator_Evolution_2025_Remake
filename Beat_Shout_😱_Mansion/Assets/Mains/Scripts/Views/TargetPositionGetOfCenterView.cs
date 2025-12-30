using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// ターゲットのCenter位置を取得するビュー
    /// </summary>
    public class TargetPositionGetOfCenterView : MonoBehaviour
    {
        [Header("メイン設定")]
        [SerializeField] private TargetPositionGetOfCenterData メイン設定;

        [ContextMenu("センター位置を取得")]
        private void GetPositionCenter()
        {
            var renderer = メイン設定.targetRenderer;
            if (renderer == null)
            {
                Debug.LogWarning("Renderer is missing");
                return;
            }
            Debug.Log($"座標: [{renderer.bounds.center}]");
        }

        /// <summary>
        /// ターゲットのCenter位置を取得するデータ
        /// </summary>
        [System.Serializable]
        public class TargetPositionGetOfCenterData
        {
            /// <summary>対象オブジェクトが持つレンダラー</summary>
            public Renderer targetRenderer;
        }
    }
}
