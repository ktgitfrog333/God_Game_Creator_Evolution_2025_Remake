using R3;
using Titles.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Titles.Views
{
    /// <summary>
    /// セーブデータ初期化パネルのビュー
    /// </summary>
    public class SaveDataInitializePanelView : MonoBehaviour
    {
        /// <summary>セーブデータ初期化パネルのビューモデル</summary>
        [SerializeField] private SaveDataInitializePanelViewModel viewModel;
        /// <summary>コマンドボタン</summary>
        [SerializeField] private Button commandButton;
        /// <summary>コマンドID</summary>
        [SerializeField] private int commandId;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (commandButton == null)
                commandButton = GetComponentInChildren<Button>();
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
            _disposableBag.Dispose();
            viewModel.Dispose();
        }

        public void OnActionCommand()
        {
            viewModel.Execute(commandId);
        }
    }
}
