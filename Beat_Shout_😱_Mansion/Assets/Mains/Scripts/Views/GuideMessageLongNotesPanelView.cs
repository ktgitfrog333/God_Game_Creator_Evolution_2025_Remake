using UnityEngine;
using TMPro;
using DG.Tweening;
using R3;

namespace Mains.Views
{
    /// <summary>
    /// GuideMessageLongNotesパネルのビュー
    /// </summary>
	public class GuideMessageLongNotesPanelView : MonoBehaviour
	{
		/// <summary>GOODテキスト</summary>
	    [SerializeField] private TextMeshProUGUI guideMessageLongNotesText;
        /// <summary>再生中アニメーションID</summary>
        private const string GuideMessageLongNotesTweenId = "GuideMessageLongNotesPanelTween";
        /// <summary>再生中アニメーション情報</summary>
        public bool IsPlaying => DOTween.IsTweening(GuideMessageLongNotesTweenId, true);
        /// <summary>初期位置</summary>
        private Vector2 _originLocalPosition;
        private readonly ReactiveProperty<bool> _isCompletedStart = new ReactiveProperty<bool>();
        /// <summary>初期処理の完了</summary>
        private readonly ReactiveProperty<bool> _isCompleted = new ReactiveProperty<bool>();
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
		{
			if (guideMessageLongNotesText == null)
				guideMessageLongNotesText = GetComponentInChildren<TextMeshProUGUI>();
		}

        private void Start()
        {
            _originLocalPosition = guideMessageLongNotesText.rectTransform.localPosition;

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
            guideMessageLongNotesText.text = message;
        }

        /// <summary>
        /// アニメーションを再生
        /// </summary>
        /// <param name="isDisableOnCompleted">完了後に無効にするか</param>
        /// <param name="directionMode">向きモード</param>
        public void PlayAnimation(bool isDisableOnCompleted, int directionMode = 0)
        {
            void DoMove()
            {
                if (IsPlaying)
                {
                    DOTween.Complete(GuideMessageLongNotesTweenId);
                }

                var originLocalPosition = _originLocalPosition;
                guideMessageLongNotesText.rectTransform.localPosition = originLocalPosition;

                var targetPosition = originLocalPosition + new Vector2(25f * (directionMode == 0 ? -1f : 1f), 0f);
                var isDisabled = isDisableOnCompleted;
                guideMessageLongNotesText.rectTransform.DOLocalMove(targetPosition, .3f)
                    .SetId(GuideMessageLongNotesTweenId)
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
                DOTween.Complete(GuideMessageLongNotesTweenId);
                _isCompleted.Value = false;
            }
            else
            {
                _isCompleted.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        DOTween.Complete(GuideMessageLongNotesTweenId);
                        _isCompleted.Value = false;
                    })
                    .AddTo(ref _disposableBag);
            }
        }
	}
}
