using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// 暖炉FBXのパーティクルのビュー
    /// </summary>
    public class FireplaceFireParticleSysView : MonoBehaviour
    {
        /// <summary>暖炉FBXのパーティクルの設定</summary>
        [SerializeField] private FireplaceFireParticleSysSettings settings;

        private void Reset()
        {
            var set = settings;

            if (set.particleSystems == null || set.particleSystems.Length < 1)
            {
                set.particleSystems = GetComponentsInChildren<ParticleSystem>();
            }
        }

        private void OnEnable()
        {
            var set = settings;
            var particleSystems = set.particleSystems;

            foreach (var particleSystem in particleSystems)
            {
                particleSystem.Play();
            }
        }

        private void OnDisable()
        {
            var set = settings;
            var particleSystems = set.particleSystems;

            foreach (var particleSystem in particleSystems)
            {
                particleSystem.Clear();
            }
        }
    }

    /// <summary>
    /// 暖炉FBXのパーティクルの設定
    /// </summary>
    [System.Serializable]
    public class FireplaceFireParticleSysSettings
    {
        /// <summary>パーティクルシステム配列</summary>
        public ParticleSystem[] particleSystems;
    }
}
