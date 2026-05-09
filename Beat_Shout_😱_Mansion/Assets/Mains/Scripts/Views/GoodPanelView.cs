using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Mains.Views
{
    /// <summary>
    /// GOODパネルのビュー
    /// </summary>
	public class GoodPanelView : MonoBehaviour
	{
		/// <summary>GOODテキスト</summary>
	    [SerializeField] private TextMeshProUGUI goodText;
		
		private void Reset()
		{
			if (goodText == null)
				goodText = GetComponentInChildren<TextMeshProUGUI>();
		}
		
		private void Start()
		{
	        // 初期スケール設定
	        goodText.rectTransform.localScale = Vector3.one * 0.5f;

	        // 縮小→拡大のアニメーション
	        goodText.rectTransform
	            .DOScale(1.2f, 0.3f)
	            .SetEase(Ease.OutBack)
	            .OnComplete(() =>
	            {
	                // 少し時間をおいて非表示にする場合
	                goodText.rectTransform.DOScale(0f, 0.2f).SetDelay(0.3f).SetEase(Ease.InBack);
	            });
		}
	}
}
