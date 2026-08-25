using R3;
using Titles.Models;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// コマンドボタンのビューモデル抽象クラス
    /// </summary>
    [CreateAssetMenu(fileName = "CommandButtonAbstractViewModel", menuName = "Scriptable Objects/CommandButtonAbstractViewModel")]
    public class CommandButtonAbstractViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] protected LauncherModel model;
        /// <summary>初期化済みフラグ</summary>
        private bool _isInitialized;
        /// <summary>管理者モード有効フラグ</summary>
        protected ReactiveCommand<bool> _isActiveAdminMode;
        /// <summary>管理者モード有効フラグ</summary>
        public ReactiveCommand<bool> IsActiveAdminMode => _isActiveAdminMode;
        /// <summary>R3のリソース管理</summary>
        protected DisposableBag _disposableBag;

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _isActiveAdminMode = new ReactiveCommand<bool>();
            _disposableBag = new DisposableBag();

            model.Initialize();

            DoObserver();
        }

        protected virtual void DoObserver()
        {

        }

        public void Dispose()
        {
            model.Dispose();
            _isActiveAdminMode = null;
            DisposeAdditional();
            _disposableBag.Dispose();
            _isInitialized = false;
        }

        protected virtual void DisposeAdditional()
        {

        }
    }
}
