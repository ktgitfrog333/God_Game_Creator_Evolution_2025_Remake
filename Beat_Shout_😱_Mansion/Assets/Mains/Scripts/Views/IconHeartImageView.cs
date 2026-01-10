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
        [SerializeField] private Image[] images;
        [Tooltip("下記二つをセット（初期値が若い方を昇順にする）\nAssets/Mains/Textures/UIs/beatshout_gamemain_icon_heart_not.png\nAssets/Mains/Textures/UIs/beatshout_gamemain_icon_heart.png")]
        /// <summary>
        /// ハートが有効か
        /// </summary>
        public bool IsEnabledHeart => images[1].transform.localScale == Vector3.one;
        /// <summary>トランスフォーム</summary>
        private Transform _trans;
        /// <summary>トランスフォーム</summary>
        public Transform Trans => _trans != null ? _trans : _trans = transform;
        /// <summary>トランスフォーム</summary>
        private Transform _enabledTrans;
        /// <summary>トランスフォーム</summary>
        public Transform EnabledTrans => _enabledTrans != null ? _enabledTrans : _enabledTrans = images[1].transform;

        private void Reset()
        {
            if (images == null)
                images = GetComponentsInChildren<Image>();
        }

        /// <summary>
        /// ハートアイコンをセット
        /// </summary>
        public void SetSpriteHeart()
        {
            var trans = Trans;
            if (trans.localScale != Vector3.one)
                trans.localScale = Vector3.one;
            var enabledTrans = EnabledTrans;
            if (enabledTrans.localScale != Vector3.one)
                enabledTrans.localScale = Vector3.one;
        }

        /// <summary>
        /// ハートアイコン (空)
        /// </summary>
        public void SetSpriteHeartNot()
        {
            var trans = Trans;
            if (trans.localScale != Vector3.one)
                trans.localScale = Vector3.one;
            var enabledTrans = EnabledTrans;
            if (enabledTrans.localScale != Vector3.zero)
                enabledTrans.localScale = Vector3.zero;
        }
    }
}
