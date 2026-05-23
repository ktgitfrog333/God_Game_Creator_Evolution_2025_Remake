using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Selects.Views
{
    /// <summary>
    /// インタラクトアイコンのビュー
    /// </summary>
    public class InteractIconImageView : MonoBehaviour
    {
        /// <summary>インタラクトアイコンの設定</summary>
        [SerializeField] private InteractIconImageSettings settings;
        /// <summary>インタラクトアイコンのアニメーション</summary>
        private Tweener _iconTween = null;
        /// <summary>矢印のトランスフォーム</summary>
        private RectTransform _arrowTrans;
        /// <summary>初期位置</summary>
        private Vector2 _originPosition;

        private void Reset()
        {
            var set = settings;
            foreach (Transform child in transform)
            {
                if (child.name.Equals("InteractIconArrowImage"))
                {
                    if (set.interactIconArrowImage == null)
                        set.interactIconArrowImage = child.GetComponent<Image>();
                }
            }
        }

        private void Awake()
        {
            var set = settings;
            _arrowTrans = set.interactIconArrowImage.transform as RectTransform;
            _originPosition = _arrowTrans.anchoredPosition;
        }

        private void OnEnable()
        {
            var set = settings;
            _iconTween = _arrowTrans.DOAnchorPosY(-7.5f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            if (_iconTween != null && _iconTween.IsActive())
            {
                _iconTween.Kill();
                _iconTween = null;
                _arrowTrans.anchoredPosition = _originPosition;
            }
        }
    }


    /// <summary>
    /// インタラクトアイコンの設定
    /// </summary>
    [System.Serializable]
    public class InteractIconImageSettings
    {
        /// <summary>チュートリアル_インタラクトアイコン(矢印)のイメージ</summary>
        public Image interactIconArrowImage;
    }
}
