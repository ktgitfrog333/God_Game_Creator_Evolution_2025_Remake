using DG.Tweening;
using Mains.Commons;
using Mains.ViewModels;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Mains.Views
{
    /// <summary>
    /// シャウトチャンスパネルのビュー
    /// </summary>
    public class ShoutChancePartPanelView : MonoBehaviour
    {
        /// <summary>中央パネル</summary>
        [SerializeField] private RectTransform centerPanel;
        /// <summary>マイクアイコンイメージのビュー</summary>
        [SerializeField] private IconMicShoutImageView iconMicShoutImageView;
        /// <summary>マイク点灯させるレベル</summary>
        [SerializeField] private float iconMicAlertLevel;
        /// <summary>シャウトパワーゲージ</summary>
        [SerializeField] private Image shoutPowerGaugeImage;
        [SerializeField] private PlayerShoutChanceTable シャウトチャンスパートの共通パラメータ管理用テーブル;
        /// <summary>シャウトチャンスパネルのビューモデル</summary>
        private ShoutChancePartPanelViewModel _shoutChancePartPanelViewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (centerPanel == null)
                centerPanel = transform.GetChild(0) as RectTransform;
            if (iconMicShoutImageView == null)
                iconMicShoutImageView = GetComponentInChildren<IconMicShoutImageView>();
            if (shoutPowerGaugeImage == null)
                shoutPowerGaugeImage = transform.GetChild(0).GetChild(3).GetChild(1).GetComponent<Image>();
        }

        /// <see cref="IconMicShoutImageView.PlayShoutEffect(Observer{bool})">centerPanelを無効にするもう一つの方法</see>
        private void Start()
        {
            _shoutChancePartPanelViewModel = new();
            // シャウトチャンスパート用のUI表示切り替え
            Observable.EveryUpdate()
                .Select(_ => _shoutChancePartPanelViewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    System.IDisposable dbLevelNotNullDisposable = null;
                    System.IDisposable dbLevelDisposable = null;
                    x.Subscribe(interactionPart =>
                    {
                        switch (interactionPart)
                        {
                            case InteractionPart.ShoutChance:
                                centerPanel.gameObject.SetActive(true);
                                dbLevelNotNullDisposable?.Dispose();
                                dbLevelDisposable?.Dispose();
                                // デシベルレベルを取得してマイクアイコン切り替え表示する処理を追加
                                dbLevelNotNullDisposable = Observable.EveryUpdate()
                                    .Select(_ => _shoutChancePartPanelViewModel.DbLevel)
                                    .Where(x => x != null)
                                    .Take(1)
                                    .Subscribe(x =>
                                    {
                                        Sequence fillSequence = null;
                                        float? normalizedPrev = null;
                                        dbLevelDisposable = x.Pairwise()
                                            .Subscribe(dbLevel =>
                                            {
                                                if (iconMicAlertLevel <= dbLevel.Current)
                                                {
                                                    if (!iconMicShoutImageView.IsPlaying)
                                                        StartCoroutine(iconMicShoutImageView.PlayShoutEffect());
                                                }
                                                if (dbLevel.Previous < dbLevel.Current)
                                                {
                                                    // fillAmount値の算出（0～1へ正規化）
                                                    float normalized = Mathf.Clamp01(dbLevel.Current / シャウトチャンスパートの共通パラメータ管理用テーブル.シャウト達成デシベル);

                                                    // すでにTweenが動いていて、目標値が近いならスキップ（不要な上書き防止）
                                                    if (fillSequence != null && fillSequence.IsActive() && fillSequence.IsPlaying())
                                                    {
                                                        float currentTarget = normalizedPrev == null ? -1f : normalizedPrev.Value;
                                                        if (Mathf.Approximately(currentTarget, normalized)) return;

                                                        fillSequence.Kill(); // 上書きしたい場合は Kill
                                                    }

                                                    // DOTweenアニメーションでfillAmountを更新
                                                    fillSequence = DOTween.Sequence()
                                                        .Append(shoutPowerGaugeImage.DOFillAmount(normalized, 0.3f))
                                                        .AppendInterval(.05f)
                                                        .Append(shoutPowerGaugeImage.DOFillAmount(0f, 0.8f).SetEase(Ease.InOutSine));
                                                    fillSequence.Play();
                                                    normalizedPrev = normalized;
                                                }
                                            })
                                            .AddTo(ref _disposableBag);
                                    })
                                    .AddTo(ref _disposableBag);

                                break;
                            case InteractionPart.Rhythm:
                                // リズムパート移行時は別の方法でcenterPanelを無効にする

                                break;
                            default:
                                centerPanel.gameObject.SetActive(false);

                                break;
                        }
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // シャウトパワーゲージの初期化
            shoutPowerGaugeImage.fillAmount = 0f;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
