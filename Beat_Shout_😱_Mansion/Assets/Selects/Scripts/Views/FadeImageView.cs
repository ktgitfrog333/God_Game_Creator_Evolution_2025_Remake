using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using Selects.Manager;
using Selects.ViewModels;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Selects.Views
{
    /// <summary>
    /// フェードイメージのビュー
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class FadeImageView : MonoBehaviour
    {
        /// <summary>イメージ</summary>
        [SerializeField] private Image image;
        /// <summary>フェードイメージの設定</summary>
        [SerializeField] private FadeImageSettings settings;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (image == null)
                image = GetComponent<Image>();
        }

        private void Start()
        {
            var set = settings;
            Observable.EveryUpdate()
                .Select(_ => GameManager.Instance)
                .Where(x => x != null)
                .Take(1)
                .Select(x => x.LevelOwner)
                .Subscribe(owner =>
                {
                    FadeImageViewModel viewModel = new FadeImageViewModel();
                    owner.IsCompleted.Where(x => x)
                        .Subscribe(_ =>
                        {
                            var sequencer = set.startDirectionSequencer;
                            sequencer.SetDoPostHideEffectDelegate(
                                (duraction, andFromTweenMode) => PlayFadeOutDirection(duraction, andFromTweenMode),
                                this.GetCancellationTokenOnDestroy(),
                                1.5f,
                                false
                            ).Forget();
                            //Observable.Create<bool>(observer =>
                            //{
                            //    StartCoroutine(PlayFadeOutDirection(observer, 1.5f, false));
                            //    return Disposable.Empty;
                            //})
                            //    .Subscribe(_ =>
                            //    {
                            //        viewModel.SetIsCompletedStartDirection(true);
                            //    })
                            //    .AddTo(ref _disposableBag);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            var set = settings;
            _disposableBag.Dispose();
            set.startDirectionSequencer.Dispose();
        }

        /// <summary>
        /// 暗幕フェードイン演出
        /// </summary>
        /// <param name="observer">オブザーバー</param>
        /// <param name="duration">終了時間</param>
        /// <returns>コルーチン</returns>
        public IEnumerator PlayFadeInDirection(Observer<bool> observer, float duration = .5f)
        {
            Color startColor = image.color;
            startColor.a = 0;
            image.color = startColor;

            // フェードインアニメーション
            image.DOFade(1f, duration)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    observer.OnNext(true);
                    observer.OnCompleted();
                }); // 緩やかなフェードイン

            yield return null;
        }

        /// <summary>
        /// 暗幕フェードイン演出
        /// </summary>
        /// <param name="duration">終了時間</param>
        /// <returns>オブザーバブル</returns>
        public Observable<bool> PlayFadeInDirection(float duration = .5f)
        {
            return Observable.Create<bool>(observer =>
            {
                StartCoroutine(PlayFadeInDirection(observer, duration));

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// 暗幕フェードアウト演出
        /// </summary>
        /// <param name="observer">オブザーバー</param>
        /// <param name="duration">終了時間</param>
        /// <param name="andFromTweenMode">0から1遷移演出を有効</param>
        /// <returns>コルーチン</returns>
        public IEnumerator PlayFadeOutDirection(Observer<bool> observer, float duration = .5f, bool andFromTweenMode = true)
        {
            if (andFromTweenMode)
            {
                // 初期状態で黒にする
                Color startColor = image.color;
                startColor.a = 1;
                image.color = startColor;
            }

            // フェードアウトアニメーション
            image.DOFade(0f, duration)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    observer.OnNext(true);
                    observer.OnCompleted();
                }); // 緩やかなフェードイン
        
            yield return null;
        }

        /// <summary>
        /// 暗幕フェードアウト演出
        /// </summary>
        /// <param name="duration">終了時間</param>
        /// <param name="andFromTweenMode">0から1遷移演出を有効</param>
        /// <returns>オブザーバブル</returns>
        public Observable<bool> PlayFadeOutDirection(float duration = .5f, bool andFromTweenMode = true)
        {
            return Observable.Create<bool>(observer =>
            {
                StartCoroutine(PlayFadeOutDirection(observer, duration, andFromTweenMode));

                return Disposable.Empty;
            });
        }
    }

    /// <summary>
    /// フェードイメージの設定
    /// </summary>
    [System.Serializable]
    public class FadeImageSettings
    {
        /// <summary>ステージ開始演出のシーケンサ</summary>
        public StartDirectionSequencer startDirectionSequencer;
    }
}
