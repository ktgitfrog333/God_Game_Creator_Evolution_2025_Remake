using Mains.External;
using Mains.ViewModels;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.Playables;

namespace Mains.Views
{
    /// <summary>
    /// ステージクリア演出のビュー
    /// </summary>
    public class StageClearDirectionView : MonoBehaviour
    {
        /// <summary>ステージクリア演出の設定</summary>
        [SerializeField] private StageClearDirectionSettings settings;
        /// <summary>ステージクリア演出のビューモデル</summary>
        private StageClearDirectionViewModel _viewModel;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            settings.playableDirector.stopped += OnTimelineStopped;
            var player = ReInput.players.GetPlayer(0);
            _viewModel = new StageClearDirectionViewModel(player);
            _viewModel.IsMissionClear.Where(x => x)
                .Take(1)
                .Subscribe(_ =>
                {
                    PlayStageClearDirection(settings.playableDirector, _viewModel);
                })
                .AddTo(ref _disposableBag);
            _script_XyloApi = new Script_xyloApi();
        }

        private void OnDestroy()
        {
            _viewModel?.Dispose();
            settings.playableDirector.stopped -= OnTimelineStopped;
            _disposableBag.Dispose();
            _script_XyloApi?.Dispose();
        }

        /// <summary>
        /// ③ドアを開ける3を再生
        /// </summary>
        /// <see cref="Assets/Mains/TimeLines/StageClearDirection.playable"/>
        /// <see cref="Assets/Mains/TimeLines/DoorOpen3.signal"/>
        public void PlayDoorOpen3()
        {
            _script_XyloApi.PlayDoorOpen3();
        }

        /// <summary>
        /// タイムライン停止
        /// </summary>
        /// <param name="d">演出</param>
        private void OnTimelineStopped(PlayableDirector d)
        {
            var viewModel = _viewModel;
            viewModel.SetIsCompletedStageClearDirection(true);
            viewModel.SetPlayerControllerEnabled(true);
        }

        /// <summary>
        /// ステージクリア演出を再生
        /// </summary>
        /// <param name="playableDirector">演出</param>
        /// <param name="viewModel">ステージクリア演出のビューモデル</param>
        private void PlayStageClearDirection(PlayableDirector playableDirector, StageClearDirectionViewModel viewModel)
        {
            viewModel.SetPlayerControllerEnabled(false);
            playableDirector.Play();
        }
    }

    /// <summary>
    /// ステージクリア演出の設定
    /// </summary>
    [System.Serializable]
    public class StageClearDirectionSettings
    {
        /// <summary>演出</summary>
        public PlayableDirector playableDirector;
    }
}
