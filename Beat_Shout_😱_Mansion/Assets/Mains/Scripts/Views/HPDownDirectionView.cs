using Mains.ViewModels;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.Playables;

namespace Mains.Views
{
    /// <summary>
    /// ハートが減少する演出ビュー
    /// </summary>
    public class HPDownDirectionView : MonoBehaviour
    {
        /// <summary>ハートが減少する演出設定</summary>
        [SerializeField] private HPDownDirectionSettings settings;
        /// <summary>ハートが減少する演出ビューモデル</summary>
        private HPDownDirectionViewModel _viewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            settings.playableDirector.stopped += OnTimelineStopped;
            _viewModel = new HPDownDirectionViewModel();
            _viewModel.IsCompletedRhythmPart.Where(x => 0 < x)
                .Subscribe(isCompleted =>
                {
                    if (!settings.isEnabledDirection)
                    {
                        _viewModel.SetIsCompletedDirection(true);

                        return;
                    }

                    switch (isCompleted)
                    {
                        case 1:
                            _viewModel.SetIsCompletedDirection(true);

                            break;
                        case 2:
                            PlayHPDownDirection(settings.playableDirector);

                            break;
                    }
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _viewModel?.Dispose();
            settings.playableDirector.stopped -= OnTimelineStopped;
        }

        /// <summary>
        /// タイムライン停止
        /// </summary>
        /// <param name="d">演出</param>
        private void OnTimelineStopped(PlayableDirector d)
        {
            _viewModel.SetIsCompletedDirection(true);
        }

        /// <summary>
        /// ハートが減少する演出の再生
        /// </summary>
        /// <param name="playableDirector">演出</param>
        /// <returns>オブザーバブル</returns>
        private Observable<Unit> PlayHPDownDirection(PlayableDirector playableDirector)
        {
            return Observable.Create<Unit>(observer =>
            {
                playableDirector.Play();

                return Disposable.Empty;
            });
        }
    }

    /// <summary>
    /// ハートが減少する演出設定
    /// </summary>
    [System.Serializable]
    public class HPDownDirectionSettings
    {
        /// <summary>演出</summary>
        public PlayableDirector playableDirector;
        /// <summary>演出を有効</summary>
        public bool isEnabledDirection;
    }
}
