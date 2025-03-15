using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using R3;

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
        }

        /// <summary>
        /// 暗幕フェードイン演出
        /// </summary>
        /// <param name="observer">オブザーバー</param>
        /// <returns>コルーチン</returns>
        public IEnumerator PlayFadeInDirection(Observer<bool> observer, float duration = .5f)
        {
            Color startColor = image.color;
            startColor.a = 0;
            image.color = startColor;

            // フェードインアニメーション
            image.DOFade(1f, duration)
                .SetEase(Ease.InOutQuad)
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
        /// <returns>コルーチン</returns>
        public IEnumerator PlayFadeOutDirection(Observer<bool> observer, float duration = .5f)
        {
            // 初期状態で黒にする
            Color startColor = image.color;
            startColor.a = 1;
            image.color = startColor;

            // フェードアウトアニメーション
            image.DOFade(0f, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    observer.OnNext(true);
                    observer.OnCompleted();
                }); // 緩やかなフェードイン
        
            yield return null;
        }
    }
}
