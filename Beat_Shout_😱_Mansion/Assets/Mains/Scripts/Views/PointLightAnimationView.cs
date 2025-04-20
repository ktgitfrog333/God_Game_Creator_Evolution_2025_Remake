using UnityEngine;
using DG.Tweening;

namespace Mains.Views
{
    /// <summary>
    /// Point Light用のアニメーション
    /// </summary>
    public class PointLightAnimationView : MonoBehaviour
    {
        /// <summary>ライト</summary>
        [SerializeField] private Light lights;
        /// <summary>点滅の間隔（シリアライズ可能）</summary>
        [SerializeField] private float blinkInterval = 1.0f;

        private void Reset()
        {
            if (lights == null)
                lights = GetComponent<Light>();
        }

        private void Start()
        {
            // DOTweenでIntensityを70%まで減らし、元に戻すYoYoループ
            float initialIntensity = lights.intensity;
            float targetIntensity = initialIntensity * 0.3f; // 70%減衰

            lights.DOIntensity(targetIntensity, blinkInterval / 2f)
                  .SetLoops(-1, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine);
        }
    }
}
