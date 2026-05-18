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
        /// <summary>ポルターガイストのビュー</summary>
        private PoltergeistView _poltergeistView;
        /// <summary>ポルターガイストのビュー</summary>
        public PoltergeistView PoltergeistView => _poltergeistView;
        public UseStatus UseStatus => _poltergeistView.GhostInStaticObjectStruct.useStatus;

        private void Start()
        {
            var trans = transform;
            PoltergeistView poltergeistView = trans.parent.GetComponentInChildren<PoltergeistView>();
            _poltergeistView = poltergeistView;
        }
    }
}
