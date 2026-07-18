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
        [SerializeField] private CommandButtonViewModel viewModel;
        /// <summary>コマンドボタン</summary>
        [SerializeField] private Button commandButton;
        /// <summary>ロード対象シーン</summary>
        [SerializeField] private string loadSceneName;
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

            viewModel.IsActiveAdminMode.Subscribe(_ =>
            {
                commandButton.interactable = true;
            })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            viewModel.Dispose();
            _disposableBag.Dispose();
        }

        public void OnActionCommand()
        {
            commandButton.interactable = false;
            SceneManager.LoadScene(loadSceneName);
        }
    }
}
