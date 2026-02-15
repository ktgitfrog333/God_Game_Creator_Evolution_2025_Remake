using UnityEngine;
using TMPro;
using DG.Tweening;
using R3;

namespace Mains.Views
{
    /// <summary>
    /// GOODパネルのビュー
    /// </summary>
	public class GoodPanelView : MonoBehaviour
	{
		/// <summary>GOODテキスト</summary>
	    [SerializeField] private TextMeshProUGUI goodText;
        /// <summary>再生中アニメーションID</summary>
        private const string GoodTweenId = "GoodPanelTween";
        /// <summary>再生中アニメーション情報</summary>
        public bool IsPlaying => DOTween.IsTweening(GoodTweenId, true);
        /// <summary>初期処理の完了</summary>
        private readonly ReactiveProperty<bool> _isCompleted = new ReactiveProperty<bool>();
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

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
				.SetId(GoodTweenId)
	            .OnComplete(() =>
	            {
	                // 少し時間をおいて非表示にする場合
	                goodText.rectTransform.DOScale(0f, 0.2f).SetDelay(0.3f).SetEase(Ease.InBack).SetId(GoodTweenId);
	            });
            _isCompleted.Value = true;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// 実行中のアニメーションを停止
        /// </summary>
        public void StopAnimation()
		{
            if (_isCompleted.Value)
            {
                DOTween.Complete(GoodTweenId);
            }
            else
            {
                _isCompleted.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        DOTween.Complete(GoodTweenId);
                    })
                    .AddTo(ref _disposableBag);
            }
        }
	}
}
