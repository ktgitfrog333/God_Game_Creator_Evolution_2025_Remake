using Mains.Commons;
using Mains.ViewModels;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// シャウトチャンスの範囲ビュー
    /// </summary>
    public class ShoutChanceRangeView : MonoBehaviour
    {
        /// <summary>シャウトチャンスの範囲ビューモデル</summary>
        private ShoutChanceRangeViewModel _viewModel;
        /// <summary>ポルターガイストのビュー</summary>
        private PoltergeistView _poltergeistView;
        /// <summary>ポルターガイストのビュー</summary>
        public PoltergeistView PoltergeistView => _poltergeistView;
        public UseStatus UseStatus => _viewModel.UseStatus;

        private void Start()
        {
            ShoutChanceRangeViewModel viewModel = ScriptableObject.CreateInstance<ShoutChanceRangeViewModel>();
            var trans = transform;
            PoltergeistView poltergeistView = trans.parent.GetComponentInChildren<PoltergeistView>();
            viewModel.DoInitialize(poltergeistView.GhostInStaticObjectStruct.useStatus);
            _viewModel = viewModel;
            _poltergeistView = poltergeistView;
        }
    }
}
