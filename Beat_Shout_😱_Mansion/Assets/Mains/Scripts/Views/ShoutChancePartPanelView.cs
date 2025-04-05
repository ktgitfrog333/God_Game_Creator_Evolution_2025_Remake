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
        [SerializeField] private IconMicImageView iconMicImageView;
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
            if (iconMicImageView == null)
                iconMicImageView = GetComponentInChildren<IconMicImageView>();
        }

        private void Start()
        {
            _shoutChancePartPanelViewModel = new();
            // シャウトチャンスパート用のUI表示切り替え
            Observable.EveryUpdate()
                .Select(_ => _shoutChancePartPanelViewModel.InteractionPart)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    System.IDisposable dbLevelNotNulldisposable = null;
                    System.IDisposable dbLeveldisposable = null;
                    x.Subscribe(interactionPart =>
                    {
                        switch (interactionPart)
                        {
                            case InteractionPart.ShoutChance:
                                centerPanel.gameObject.SetActive(true);
                                dbLevelNotNulldisposable?.Dispose();
                                dbLeveldisposable?.Dispose();
                                // デシベルレベルを取得してdB(A)を表示する用のテキストへ反映する処理を追加
                                dbLevelNotNulldisposable = Observable.EveryUpdate()
                                    .Select(_ => _shoutChancePartPanelViewModel.DbLevel)
                                    .Where(x => x != null)
                                    .Take(1)
                                    .Subscribe(x =>
                                    {
                                        dbLeveldisposable = x.Subscribe(dbLevel =>
                                        {
                                            if (iconMicAlertLevel <= dbLevel)
                                            {
                                                iconMicImageView.SetSpriteMicShout();
                                            }
                                            else
                                            {
                                                iconMicImageView.SetSpriteMic();
                                            }
                                        })
                                        .AddTo(ref _disposableBag);
                                    })
                                    .AddTo(ref _disposableBag);

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
