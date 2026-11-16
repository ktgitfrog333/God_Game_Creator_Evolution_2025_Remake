using UnityEngine;
using TMPro;
using DG.Tweening;
using R3;

namespace Mains.Views
{
    /// <summary>
    /// GuideMessageShortNotesパネルのビュー
    /// </summary>
	public class GuideMessageShortNotesPanelView : MonoBehaviour
	{
		/// <summary>GOODテキスト</summary>
	    [SerializeField] private TextMeshProUGUI guideMessageShortNotesText;
        /// <summary>再生中アニメーションID</summary>
        private const string GuideMessageShortNotesTweenId = "GuideMessageShortNotesPanelTween";
        /// <summary>再生中アニメーション情報</summary>
        public bool IsPlaying => DOTween.IsTweening(GuideMessageShortNotesTweenId, true);
        /// <summary>初期位置</summary>
        private Vector2 _originLocalPosition;
        private readonly ReactiveProperty<bool> _isCompletedStart = new ReactiveProperty<bool>();
        /// <summary>初期処理の完了</summary>
        private readonly ReactiveProperty<bool> _isCompleted = new ReactiveProperty<bool>();
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
		{
			if (guideMessageShortNotesText == null)
                guideMessageShortNotesText = GetComponentInChildren<TextMeshProUGUI>();
		}

        private void Start()
        {
            _originLocalPosition = guideMessageShortNotesText.rectTransform.localPosition;

            _isCompletedStart.Value = true;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// メッセージをセット
        /// </summary>
        /// <param name="message">メッセージ</param>
        public void SetMessage(string message)
        {
            guideMessageShortNotesText.text = message;
        }

        /// <summary>
        /// アニメーションを再生
        /// </summary>
        /// <param name="isDisableOnCompleted">完了後に無効にするか</param>
        public void PlayAnimation(bool isDisableOnCompleted)
        {
            void DoMove()
            {
                if (IsPlaying)
                {
                    DOTween.Complete(GuideMessageShortNotesTweenId);
                }

                var originLocalPosition = _originLocalPosition;
                guideMessageShortNotesText.rectTransform.localPosition = originLocalPosition;

                var targetPosition = originLocalPosition + new Vector2(25f * -1f, 0f);
                var isDisabled = isDisableOnCompleted;
                guideMessageShortNotesText.rectTransform.DOLocalMove(targetPosition, .15f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetId(GuideMessageShortNotesTweenId)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        if (isDisabled)
                            transform.gameObject.SetActive(false);
                    });

                _isCompleted.Value = true;
            }

            if (_isCompletedStart.CurrentValue)
            {
                DoMove();
            }
            else
            {
                Observable.EveryUpdate()
                    .Select(_ => _isCompletedStart.CurrentValue)
                    .Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        DoMove();
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        /// <summary>
        /// 実行中のアニメーションを停止
        /// </summary>
        public void StopAnimation()
		{
            if (_isCompleted.Value)
            {
                DOTween.Complete(GuideMessageShortNotesTweenId);
                _isCompleted.Value = false;
            }
            else
            {
                _isCompleted.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        DOTween.Complete(GuideMessageShortNotesTweenId);
                        _isCompleted.Value = false;
                    })
                    .AddTo(ref _disposableBag);
            }
        }
	}
}
