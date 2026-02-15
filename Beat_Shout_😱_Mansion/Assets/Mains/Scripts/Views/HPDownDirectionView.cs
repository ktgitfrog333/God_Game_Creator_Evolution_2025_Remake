using Mains.ViewModels;
using R3;
using R3.Triggers;
using Rewired;
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
            var player = ReInput.players.GetPlayer(0);
            _viewModel = new HPDownDirectionViewModel(player);
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
                            PlayHPDownDirection(settings.playableDirector, _viewModel);

                            break;
                    }
                })
                .AddTo(ref _disposableBag);
            // 攻撃用オブジェクトがプレイヤーへヒットしたかを監視
            _viewModel.IsHitGhostAttack.Where(x => x)
                .Subscribe(_ =>
                {
                    PlayHPDownDirection(settings.playableDirector, _viewModel);
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
            var viewModel = _viewModel;
            viewModel.SetIsCompletedDirection(true);
            viewModel.SetPlayerControllerEnabled(true);
            Time.timeScale = 1f;
            viewModel.SubtractionHealthPoint();
        }

        /// <summary>
        /// ハートが減少する演出の再生
        /// </summary>
        /// <param name="playableDirector">演出</param>
        /// <param name="viewModel">ハートが減少する演出ビューモデル</param>
        private void PlayHPDownDirection(PlayableDirector playableDirector, HPDownDirectionViewModel viewModel)
        {
            viewModel.SetPlayerControllerEnabled(false);
            Time.timeScale = 0f;
            playableDirector.Play();
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
