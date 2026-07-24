using R3;
using Titles.Models;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// コマンドボタンのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "CommandButtonViewModel", menuName = "Scriptable Objects/CommandButtonViewModel")]
    public class CommandButtonViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] private LauncherModel model;
        /// <summary>初期化済みフラグ</summary>
        private bool _isInitialized;
        /// <summary>管理者モード有効フラグ</summary>
        private ReactiveCommand<bool> _isActiveAdminMode;
        /// <summary>管理者モード有効フラグ</summary>
        public ReactiveCommand<bool> IsActiveAdminMode => _isActiveAdminMode;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag;

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _isActiveAdminMode = new ReactiveCommand<bool>();
            _disposableBag = new DisposableBag();

            model.Initialize();

            model.IsActiveAdminMode.Where(x => x)
                .Subscribe(isActiveAdminMode =>
                {
                    _isActiveAdminMode.Execute(isActiveAdminMode);
                })
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            model.Dispose();
            _isActiveAdminMode = null;
            _disposableBag.Dispose();
            _isInitialized = false;
        }
    }
}
