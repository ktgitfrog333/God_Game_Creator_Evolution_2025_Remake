using R3;
using Titles.Models;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// コマンドボタンのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "CommandButtonViewModel", menuName = "Scriptable Objects/CommandButtonViewModel")]
    public class CommandButtonViewModel : CommandButtonAbstractViewModel
    {
        protected override void DoObserver()
        {
            model.IsActiveAdminMode.Where(x => x)
                .Subscribe(isActiveAdminMode =>
                {
                    _isActiveAdminMode.Execute(isActiveAdminMode);
                })
                .AddTo(ref _disposableBag);
        }
    }
}
