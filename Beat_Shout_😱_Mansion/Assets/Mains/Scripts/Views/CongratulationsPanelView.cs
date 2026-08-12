using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mains.ViewModels;
using R3;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// コングラチュレーションのビュー
    /// </summary>
    public class CongratulationsPanelView : MonoBehaviour
    {
        /// <summary>コングラチュレーションのビューモデル</summary>
        [SerializeField] private CongratulationsPanelViewModel viewModel;
        /// <summary>コングラチュレーションの設定</summary>
        [SerializeField] private CongratulationsPanelSettings settings;
        /// <summary>初期処理の完了</summary>
        private readonly ReactiveProperty<bool> _isCompletedStart = new ReactiveProperty<bool>();
        /// <summary>初期処理の完了</summary>
        public ReadOnlyReactiveProperty<bool> IsCompletedStart => _isCompletedStart;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            var set = settings;
            if (set.congratulationsPanelTrans == null)
                set.congratulationsPanelTrans = transform as RectTransform;

            foreach (Transform child in transform)
            {
                if (child.name.Equals("CongratulationsText"))
                {
                    if (set.congratulationsText == null)
                        set.congratulationsText = child.GetComponent<TextMeshProUGUI>();
                }
                if (child.name.Equals("EndingCGPanel"))
                {
                    if (set.endingCGPanelCanvasGroup == null)
                        set.endingCGPanelCanvasGroup = child.GetComponent<CanvasGroup>();
                }
                if (child.name.Equals("CreditPanel"))
                {
                    if (set.creditPanelCanvasGroup == null)
                        set.creditPanelCanvasGroup = child.GetComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            _isCompletedStart.Value = true;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// パネルを有効にして、コングラチュレーションのアニメーションを再生する
        /// </summary>
        /// <returns>オブザーバブル</returns>
        public Observable<Unit> EnableAndPlayFadeInCongratulations()
        {
            return Observable.Create<Unit>(observer =>
            {
                EnableAndPlayFadeInAsync(observer, this.GetCancellationTokenOnDestroy())
                    .Forget();

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// パネルを有効にして、アニメーションを再生する
        /// </summary>
        /// <param name="observer">オブザーバ</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask EnableAndPlayFadeInAsync(Observer<Unit> observer, CancellationToken token)
        {
            var set = settings;
            var trans = set.congratulationsPanelTrans;
            trans.gameObject.SetActive(true);
            var vm = viewModel;

            await Observable.EveryUpdate()
                .Where(_ => IsCompletedStart.CurrentValue)
                .FirstAsync(token);

            var congratulationsText = set.congratulationsText;
            var congratulationsTextTrans = congratulationsText.transform as RectTransform;
            var congratulationsTextDurations = vm.CongratulationsTextDurations;

            await DOTween.Sequence()
                .Append(congratulationsText.DOFade(1f, congratulationsTextDurations[0]))
                .Join(congratulationsTextTrans.DOScale(Vector3.one, congratulationsTextDurations[1]).SetEase(Ease.OutBack))
                .Append(congratulationsTextTrans.DOAnchorPos(vm.CongratulationsTextToPosition, congratulationsTextDurations[2]))
                .SetUpdate(true)
                .ToUniTask();

            await DOTween.Sequence()
                .Append(set.endingCGPanelCanvasGroup.DOFade(1f, vm.EndingCGPanelDuration))
                .Join(set.creditPanelCanvasGroup.DOFade(1f, vm.EndingCGPanelDuration))
                .SetUpdate(true)
                .ToUniTask();

            observer.OnNext(Unit.Default);
            observer.OnCompleted();
        }
    }

    /// <summary>
    /// コングラチュレーションの設定
    /// </summary>
    [System.Serializable]
    public class CongratulationsPanelSettings
    {
        /// <summary>コングラチュレーションのトランスフォーム</summary>
        public RectTransform congratulationsPanelTrans;
        /// <summary>コングラチュレーションのテキスト</summary>
        public TextMeshProUGUI congratulationsText;
        /// <summary>エンディングCGパネルのキャンバスグループ</summary>
        public CanvasGroup endingCGPanelCanvasGroup;
        /// <summary>クレジットパネルのキャンバスグループ</summary>
        public CanvasGroup creditPanelCanvasGroup;
    }
}
