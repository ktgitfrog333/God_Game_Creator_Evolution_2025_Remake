using Mains.Commons;
using Mains.ViewModels;
using R3;
using UnityEngine;

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
                                // デシベルレベルを取得してdB(A)を表示する用のテキストへ反映する処理を追加
                                dbLevelNotNullDisposable = Observable.EveryUpdate()
                                    .Select(_ => _shoutChancePartPanelViewModel.DbLevel)
                                    .Where(x => x != null)
                                    .Take(1)
                                    .Subscribe(x =>
                                    {
                                        dbLevelDisposable = x.Subscribe(dbLevel =>
                                        {
                                            if (iconMicAlertLevel <= dbLevel)
                                            {
                                                StartCoroutine(iconMicShoutImageView.PlayShoutEffect());
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
        }
    }
}
