using R3;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// コマンドボタンのビューモデル
    /// </summary>
    /// <remarks>有効／無効を切り替える</remarks>
    [CreateAssetMenu(fileName = "CommandButtonSwitchableViewModel", menuName = "Scriptable Objects/CommandButtonSwitchableViewModel")]
    public class CommandButtonSwitchableViewModel : CommandButtonAbstractViewModel
    {
        /// <summary>シーンロード演出の完了タイプ</summary>
        private ReactiveProperty<int> _CompletedSceneLoadDirectionType;
        /// <summary>シーンロード演出の完了タイプ</summary>
        public ReadOnlyReactiveProperty<int> CompletedSceneLoadDirectionType => _CompletedSceneLoadDirectionType;

        protected override void DoObserver()
        {
            _CompletedSceneLoadDirectionType = new ReactiveProperty<int>();

            model.IsActiveAdminMode.Subscribe(isActiveAdminMode =>
                {
                    _isActiveAdminMode.Execute(isActiveAdminMode);
                })
                .AddTo(ref _disposableBag);
            model.CompletedSceneLoadDirectionType.Subscribe(x =>
                {
                    _CompletedSceneLoadDirectionType.Value = x;
                })
                .AddTo(ref _disposableBag);
        }

        protected override void DisposeAdditional()
        {
            _CompletedSceneLoadDirectionType = null;
        }

        public void SetCompletedSceneLoadDirectionType(int completedSceneLoadDirectionType)
        {
            model.SetCompletedSceneLoadDirectionType(completedSceneLoadDirectionType);
        }
    }
}
