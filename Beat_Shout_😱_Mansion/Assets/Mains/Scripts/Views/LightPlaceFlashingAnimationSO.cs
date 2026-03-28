using DG.Tweening;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// 暖炉、壁ランプ用の点滅アニメーション
    /// </summary>
    [CreateAssetMenu(fileName = "LightPlaceFlashingAnimation", menuName = "Scriptable Objects/LightPlaceFlashingAnimation")]
    public class LightPlaceFlashingAnimationSO : PoltergeistAnimationSO
    {
        /// <summary>暖炉、壁ランプ用の点滅アニメーション設定</summary>
        [SerializeField] private LightPlaceFlashingAnimationSOSettings settings;
        /// <summary>クローンされたSO専用の状態保持_ライト</summary>
        private Light _cachedLight;
        /// <summary>クローンされたSO専用の状態保持_明るさ</summary>
        private float _initialIntensity;

        public override Sequence Play(Transform target, Quaternion initialLocalRotation)
        {
            // 初回のみTransform以下からLightコンポーネントを探してキャッシュ
            if (_cachedLight == null)
            {
                _cachedLight = target.GetComponentInChildren<Light>();
                if (_cachedLight != null)
                {
                    _initialIntensity = _cachedLight.intensity;
                }
                else
                {
                    return null; // ライトが見つからない場合はアニメーションしない
                }
            }

            // 何らかの要因でターゲットが破棄されていたら中断
            if (_cachedLight == null) return null;

            // 暖炉、壁ランプ用の点滅アニメーション設定
            var set = settings;
            // 明るさを点滅させるDOTween Sequence
            return DOTween.Sequence()
                .Append(_cachedLight.DOIntensity(set.minIntensity, set.duration).SetEase(Ease.Flash, set.loops))
                .OnComplete(() => _cachedLight.intensity = _initialIntensity);
        }
    }

    /// <summary>
    /// 暖炉、壁ランプ用の点滅アニメーション設定
    /// </summary>
    [System.Serializable]
    public class LightPlaceFlashingAnimationSOSettings
    {
        /// <summary>アニメーション終了時間</summary>
        public float duration = 0.1f;
        /// <summary>明るさ最小値</summary>
        public float minIntensity = 0.2f;
        /// <summary>ループ回数</summary>
        public int loops = 6;
    }
}
