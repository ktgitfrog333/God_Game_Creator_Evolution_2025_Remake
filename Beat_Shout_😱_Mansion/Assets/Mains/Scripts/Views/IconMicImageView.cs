using UnityEngine;
using UnityEngine.UI;

namespace Mains.Views
{
    /// <summary>
    /// マイクアイコンイメージのビュー
    /// </summary>
    public class IconMicImageView : MonoBehaviour
    {
        /// <summary>マイクアイコンイメージ</summary>
        [SerializeField] private Image image;
        [Tooltip("下記二つをセット（初期値が若い方を昇順にする）\nAssets/Mains/Textures/UIs/beatshout_gamemain_icon_mic.png\nAssets/Mains/Textures/UIs/beatshout_gamemain_icon_mic_shout.png")]
        /// <summary>表示差分</summary>
        [SerializeField] private Sprite[] patterns;

        private void Reset()
        {
            if (image == null)
                image = GetComponent<Image>();
        }

        /// <summary>
        /// マイクアイコン (シャウト中)をセット
        /// </summary>
        public void SetSpriteMicShout()
        {
            image.sprite = patterns[1];
        }

        /// <summary>
        /// マイクアイコンをセット
        /// </summary>
        public void SetSpriteMic()
        {
            image.sprite = patterns[0];
        }
    }
}
