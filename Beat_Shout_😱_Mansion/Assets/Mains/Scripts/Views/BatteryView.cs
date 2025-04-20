using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// バッテリーのビュー
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class BatteryView : MonoBehaviour
    {
        /// <summary>カプセルコライダー</summary>
        [SerializeField] private SphereCollider sphereCollider;

        private void Reset()
        {
            if (sphereCollider == null)
                sphereCollider = GetComponent<SphereCollider>();
        }

        private void OnEnable()
        {
            if (sphereCollider.enabled)
                sphereCollider.enabled = false;
        }

        /// <summary>
        /// コライダーへ有効／無効をセット
        /// </summary>
        /// <param name="enabled">有効／無効</param>
        public void SetEnabledCollider(bool enabled)
        {
            if (sphereCollider.enabled != enabled)
                sphereCollider.enabled = enabled;
        }

        /// <summary>
        /// 電池をDestroy
        /// </summary>
        public void GetBattery()
        {
            Destroy(gameObject);
        }
    }
}
