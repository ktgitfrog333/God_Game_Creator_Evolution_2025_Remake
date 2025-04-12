using DG.Tweening;
using Mains.ViewModels;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Mains.Views
{
    /// <summary>
    /// マイクアイコン (シャウト中)イメージのビュー
    /// </summary>
    public class IconMicShoutImageView : MonoBehaviour
    {
        /// <summary>マイクアイコン (シャウト中)イメージ</summary>
        [SerializeField] private Image image;
        /// <summary>中央パネル</summary>
        [SerializeField] private RectTransform centerPanel;
        /// <summary>シャウトチャンスパネルのビューモデル</summary>
        private ShoutChancePartPanelViewModel _shoutChancePartPanelViewModel;
        /// <summary>実行中</summary>
        private bool _isPlaying = false;
        /// <summary>実行中</summary>
        public bool IsPlaying => _isPlaying;

        private void Reset()
        {
            if (image == null)
                image = GetComponent<Image>();
            if (centerPanel == null)
                centerPanel = transform.parent as RectTransform;
        }

        private void Start()
        {
            _shoutChancePartPanelViewModel = new ShoutChancePartPanelViewModel();
        }

        /// <summary>
        /// マイクアイコン (シャウト中)をセット
        /// </summary>
        /// <returns>コルーチン</returns>
        public IEnumerator PlayShoutEffect()
        {
            if (_shoutChancePartPanelViewModel == null ||
                !this || image == null || !image.gameObject.activeSelf ||
                _isPlaying)
            {
                yield return null;
            }
            else
            {
                _isPlaying = true;
                Color color = new Color(image.color.r, image.color.g, image.color.b, image.color.a);
                color = new Color(255, 255, 255, 255);
                image.color = color;

                // 点滅：1秒間、0.25秒ごとに透明⇄表示を繰り返し（4回）
                for (int i = 0; i < 4; i++)
                {
                    image.DOFade(0f, 0.125f);
                    yield return new WaitForSeconds(0.125f);
                    image.DOFade(1f, 0.125f);
                    yield return new WaitForSeconds(0.125f);
                }

                switch (_shoutChancePartPanelViewModel.InteractionPart.Value)
                {
                    case Commons.InteractionPart.Rhythm:
                        centerPanel.gameObject.SetActive(false);

                        break;
                }
                _isPlaying = false;
            }
        }
    }
}
