using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Mains.Views
{
    /// <summary>
    /// BADパネルのビュー
    /// </summary>
    public class BadPanelView : MonoBehaviour
    {
        /// <summary>BADテキスト</summary>
        [SerializeField] private TextMeshProUGUI badText;

        private void Reset()
        {
            if (badText == null)
                badText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            // 左上を基準にスケール・回転を適用するため、Pivotを(0, 1)に設定
            RectTransform rectTransform = badText.rectTransform;
            var originAnchoredPosition = new Vector2(rectTransform.sizeDelta.x * -.5f, rectTransform.sizeDelta.y * .5f);
            rectTransform.anchoredPosition = originAnchoredPosition;
            rectTransform.pivot = new Vector2(0f, 1f);

            // 軽く揺れるアニメーション（例: Y軸方向に回転）
            rectTransform
                .DORotate(new Vector3(0f, 0f, -15f), 0.3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    // 揺れ終わったら縮小して非表示
                    rectTransform
                        .DOScale(Vector3.zero, 0.2f)
                        .SetDelay(0.3f)
                        .SetEase(Ease.InBack);
                });
        }

    }
}
