using DG.Tweening;
using R3;
using System.Collections;
using Titles.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Titles.Views
{
    /// <summary>
    /// フェードイメージのビュー
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class FadeImageView : MonoBehaviour
    {
        /// <summary>フェードイメージのビューモデル</summary>
        [SerializeField] private FadeImageViewModel viewModel;
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
            // フェードアウトを呼び出して、完了したらシーンロード完了タイプを1へ更新
            var vm = viewModel;

            vm.Initialize();

            PlayFadeOutDirection(1f).Where(x => x)
                .Subscribe(_ =>
                {
                    vm.SetCompletedSceneLoadDirectionType(1);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            viewModel.Dispose();
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
}
