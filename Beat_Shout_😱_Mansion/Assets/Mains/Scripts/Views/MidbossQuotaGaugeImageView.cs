using UnityEngine;
using UnityEngine.UI;

namespace Mains.Views
{
    /// <summary>
    /// 中ボスノルマゲージUIのビュー
    /// </summary>
    public class MidbossQuotaGaugeImageView : MonoBehaviour
    {
        /// <summary>中ボスノルマゲージUIの設定</summary>
        [SerializeField] private MidbossQuotaGaugeImageSettings settings;
        /// <summary>マテリアル</summary>
        private Material _runtimeMaterial;
        /// <summary>マテリアル</summary>
        public Material RuntimeMaterial => _runtimeMaterial != null ? _runtimeMaterial : _runtimeMaterial = Instantiate(settings.gaugeImage.material);

        private void Start()
        {
            settings.gaugeImage.material = RuntimeMaterial;
        }

        private void Update()
        {
            RuntimeMaterial.SetFloat("_Fill", settings.fill);
        }
    }


    /// <summary>
    /// 中ボスノルマゲージUIの設定
    /// </summary>
    [System.Serializable]
    public class MidbossQuotaGaugeImageSettings
    {
        /// <summary>ゲージUIのイメージ</summary>
        public Image gaugeImage;
        /// <summary>フィルの値</summary>
        [Range(0f, 1f)]
        public float fill;
    }
}
