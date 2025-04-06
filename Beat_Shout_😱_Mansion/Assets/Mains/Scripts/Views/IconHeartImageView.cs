using UnityEngine;
using UnityEngine.UI;

namespace Mains.Views
{
    /// <summary>
    /// ハートアイコンのビュー
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class IconHeartImageView : MonoBehaviour
    {
        /// <summary>ハートアイコンイメージ</summary>
        [SerializeField] private Image image;
        [Tooltip("下記二つをセット（初期値が若い方を昇順にする）\nAssets/Mains/Textures/UIs/beatshout_gamemain_icon_heart_not.png\nAssets/Mains/Textures/UIs/beatshout_gamemain_icon_heart.png")]
        /// <summary>表示差分</summary>
        [SerializeField] private Sprite[] patterns;

        private void Reset()
        {
            if (image == null)
                image = GetComponent<Image>();
        }

        /// <summary>
        /// ハートアイコンをセット
        /// </summary>
        public void SetSpriteHeart()
        {
            image.sprite = patterns[1];
        }

        /// <summary>
        /// ハートアイコン (空)
        /// </summary>
        public void SetSpriteHeartNot()
        {
            image.sprite = patterns[0];
        }
    }
}
