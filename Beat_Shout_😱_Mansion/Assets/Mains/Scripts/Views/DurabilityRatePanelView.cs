using DG.Tweening;
using Mains.Commons;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// シャウトノーツの耐久率パネルビュー
    /// </summary>
    public partial class DurabilityRatePanelView : MonoBehaviour
    {
        /// <summary>シャウトノーツの耐久率パネル設定</summary>
        [SerializeField] private DurabilityRatePanelSettings settings;
        /// <summary>シークエンス</summary>
        private Sequence _durabilitySequence;
        /// <summary>スケール初期値</summary>
        private Vector3 _initLocalScale;
        /// <summary>テキスト初期値</summary>
        private string _initText;
        /// <summary>カラー初期値</summary>
        private Color32 _initColor;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (settings.durabilityRatePanel == null)
                settings.durabilityRatePanel = transform as RectTransform;

            foreach (Transform child in transform)
            {
                if (child.name.Equals("DurabilityRateText"))
                {
                    if (settings.durabilityRateText == null)
                        settings.durabilityRateText = child.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        private void Start()
        {
            var set = settings;
            var panel = set.durabilityRatePanel;
            var text = set.durabilityRateText;
            _initLocalScale = panel.localScale;
            _initText = text.text;
            _initColor = text.color;

            this.OnDisableAsObservable()
                .Subscribe(_ =>
                {
                    ResetDurability();
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// 耐久率演出（拡大→縮小 + 値減少 + 色変化）
        /// </summary>
        /// <param name="from">現在値</param>
        /// <param name="to">更新値</param>
        /// <param name="duration">アニメーション終了時間</param>
        public void PlayDurabilityAnimation(int from = 100, int to = 0, float duration = 0.5f)
        {
            _durabilitySequence?.Complete();

            var set = settings;
            var panel = set.durabilityRatePanel;
            var text = set.durabilityRateText;
            int currentValue = from;

            _durabilitySequence = DOTween.Sequence()
                .SetLink(gameObject);

            // =========================
            // ① パネル拡大 → 元に戻る
            // =========================
            _durabilitySequence.Join(
                panel.DOScale(1.2f, duration * 0.5f)
                     .SetEase(Ease.OutQuad)
                     .OnComplete(() =>
                     {
                         panel.DOScale(1.0f, duration * 0.5f)
                              .SetEase(Ease.InQuad);
                     })
            );

            // =========================
            // ② 値減少（100 → 0）
            // =========================
            _durabilitySequence.Join(
                DOTween.To(
                    () => currentValue,
                    x =>
                    {
                        currentValue = x;

                        // テキスト更新
                        text.text = currentValue.ToString();

                        // 色更新
                        text.color = EvaluateColor(currentValue);
                    },
                    to,
                    duration
                )
                .SetEase(Ease.Linear)
            );
        }

        /// <summary>
        /// 耐久率表示をリセット
        /// </summary>
        private void ResetDurability()
        {
            _durabilitySequence?.Kill();
            var set = settings;
            var panel = set.durabilityRatePanel;
            var text = set.durabilityRateText;
            panel.localScale = _initLocalScale;
            text.text = _initText;
            text.color = _initColor;
        }

        /// <summary>
        /// 値に応じた色を返す
        /// </summary>
        /// <param name="value">値</param>
        /// <returns>値に応じた色</returns>
        private Color32 EvaluateColor(int value)
        {
            float t = Mathf.Clamp01((float)value / 100f);
            DurabilityRatePanelTable table = settings.durabilityRatePanelTable;

            Color32[] colors = table.textColors;

            // 0〜1をセグメント分割
            float scaled = t * (colors.Length - 1);
            int index = Mathf.FloorToInt(scaled);
            int nextIndex = Mathf.Clamp(index + 1, 0, colors.Length - 1);

            float lerpT = scaled - index;

            return Color32.Lerp(colors[index], colors[nextIndex], lerpT);
        }
    }

    /// <summary>
    /// シャウトノーツの耐久率パネル設定
    /// </summary>
    [System.Serializable]
    public class DurabilityRatePanelSettings
    {
        /// <summary>パネルのトランスフォーム</summary>
        public RectTransform durabilityRatePanel;
        /// <summary>テキストのトランスフォーム</summary>
        public TextMeshProUGUI durabilityRateText;
        /// <summary>シャウトノーツの耐久率パネルテーブル</summary>
        public DurabilityRatePanelTable durabilityRatePanelTable;
    }
}
