using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using R3;
using Mains.External;

namespace Mains.Views
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
            Color startColor = image.color;
            if (0f < startColor.a)
            {
                startColor.a = 0;
                image.color = startColor;
            }
            // 1. fade.6～.9 の間をゆったりとしたカーブでYoYoのループDOTweenアニメーション
            image.DOFade(0.9f, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .From(0.6f)
                .SetId("LoopFade");
            Script_xyloApi script_XyloApi = new Script_xyloApi();
            script_XyloApi.FrameRate
                .Where(x => 0f < x)
                .Subscribe(_ =>
                {
                    // 2. このタイミングで一気に fade0にするDOTweenアニメーション
                    DOTween.Kill("LoopFade"); // ループ停止
                    image.DOFade(0f, 0.3f)
                        .SetEase(Ease.OutQuad);
                })
                .AddTo(ref _disposableBag);
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
