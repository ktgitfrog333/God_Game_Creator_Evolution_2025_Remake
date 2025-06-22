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
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (image == null)
                image = GetComponent<Image>();
        }

        private void Start()
        {
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
                            Observable.Create<bool>(observer =>
                            {
                                StartCoroutine(PlayFadeOutDirection(observer, 1.5f, false));
                                return Disposable.Empty;
                            })
                                .Subscribe(_ =>
                                {
                                    viewModel.SetIsCompletedStartDirection(true);
                                })
                                .AddTo(ref _disposableBag);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
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
    }
}
