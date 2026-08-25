using R3;
using Titles.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Titles.Views
{
    /// <summary>
    /// コマンドボタンのビュー
    /// </summary>
    public class CommandButtonView : MonoBehaviour
    {
        /// <summary>コマンドボタンのビューモデル</summary>
        [SerializeField] private CommandButtonAbstractViewModel viewModel;
        /// <summary>コマンドボタン</summary>
        [SerializeField] private Button commandButton;
        /// <summary>ロード対象シーン</summary>
        [SerializeField] private string loadSceneName;
        /// <summary>演出待機フラグ</summary>
        [SerializeField] private bool isWaitDirection;
        /// <summary>フェードイメージのビュー</summary>
        [SerializeField] private FadeImageView fadeImageView;
        /// <summary>初期化済みフラグ</summary>
        private readonly ReactiveProperty<bool> _isCompleted = new ReactiveProperty<bool>();
        /// <summary>初期化済みフラグ</summary>
        public ReadOnlyReactiveProperty<bool> IsCompleted => _isCompleted;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (commandButton == null)
                commandButton = GetComponent<Button>();
        }

        private void Start()
        {
            viewModel.Initialize();

            viewModel.IsActiveAdminMode.Subscribe(x =>
            {
                // 演出を考慮する場合、フェードアウト完了まで待機してインタラクトを有効にする
                var commandButton = this.commandButton;
                if (isWaitDirection)
                {
                    var vm = (CommandButtonSwitchableViewModel)viewModel;
                    if (x)
                    {
                        var completedSceneLoadDirectionType = vm.CompletedSceneLoadDirectionType;
                        switch (completedSceneLoadDirectionType.CurrentValue)
                        {
                            case 0:
                                completedSceneLoadDirectionType.Where(x => x == 1)
                                    .Subscribe(_ =>
                                    {
                                        commandButton.interactable = x;
                                    })
                                    .AddTo(ref _disposableBag);

                                break;
                            case 1:
                                commandButton.interactable = x;

                                break;
                        }
                    }
                    else
                    {
                        commandButton.interactable = x;
                    }
                }
                else
                {
                    commandButton.interactable = x;
                }
            })
                .AddTo(ref _disposableBag);

            _isCompleted.Value = true;
        }

        private void OnDestroy()
        {
            viewModel.Dispose();
            _disposableBag.Dispose();
        }

        public void OnActionCommand()
        {
            commandButton.interactable = false;
            // 演出を考慮する場合、フェードイン完了まで待機してシーンロードを実施
            if (isWaitDirection && fadeImageView != null)
            {
                var vm = (CommandButtonSwitchableViewModel)viewModel;
                vm.SetCompletedSceneLoadDirectionType(2);
                fadeImageView.PlayFadeInDirection(1f).Where(x => x)
                    .Subscribe(_ =>
                    {
                        vm.SetCompletedSceneLoadDirectionType(3);
                        SceneManager.LoadScene(loadSceneName);
                    })
                    .AddTo(ref _disposableBag);
            }
            else
            {
                SceneManager.LoadScene(loadSceneName);
            }
        }
    }
}
