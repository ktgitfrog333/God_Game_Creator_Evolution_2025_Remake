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
                                        Tweener fillTweener = null;
                                        Sequence fillReturnSequence = null;
                                        dbLevelDisposable = x.Subscribe(dbLevel =>
                                            {
                                                // TODO: アラート発生レベルの判定を共通化する
                                                if (iconMicAlertLevel <= dbLevel)
                                                {
                                                    if (!iconMicShoutImageView.IsPlaying)
                                                        StartCoroutine(iconMicShoutImageView.PlayShoutEffect());
                                                }
                                                // fillAmount値の算出（0～1へ正規化）
                                                float normalized = Mathf.Clamp01(dbLevel / シャウトチャンスパートの共通パラメータ管理用テーブル.シャウトゲージスライダー最大値);
                                                if (dbLevel != シャウトチャンスパートの共通パラメータ管理用テーブル.シャウトゲージスライダー最大値)
                                                {
                                                    // すでにTweenが動いているなら終了するまでマイク入力は反映しない
                                                    if (fillTweener != null && fillTweener.IsActive() && fillTweener.IsPlaying())
                                                    {
                                                        return;
                                                    }
                                                    // マイク入力の場合
                                                    shoutPowerGaugeImage.DOFillAmount(normalized, .3f);
                                                }
                                                else
                                                {
                                                    // キーボード／Xbox360コンによるマニュアル入力の場合

                                                    // すでにTweenが動いているなら強制終了して上書き
                                                    if (fillTweener != null && fillTweener.IsActive() && fillTweener.IsPlaying())
                                                    {
                                                        fillTweener.Kill();
                                                        fillReturnSequence = null;
                                                    }
                                                    // DOTweenアニメーションでfillAmountを更新
                                                    fillTweener = shoutPowerGaugeImage.DOFillAmount(normalized, .3f)
                                                        .OnComplete(() =>
                                                        {
                                                            fillReturnSequence?.Play();
                                                        });
                                                    fillReturnSequence = DOTween.Sequence()
                                                        .AppendInterval(.2f)
                                                        .Append(shoutPowerGaugeImage.DOFillAmount(0f, 0.8f).SetEase(Ease.InOutSine))
                                                        ;
                                                    fillTweener.Play();
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
