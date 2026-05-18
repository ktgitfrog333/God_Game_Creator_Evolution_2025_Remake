using Mains.ViewModels;
using R3;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// オバケ弾：抽象クラスのビュー
    /// </summary>
    public abstract class GhostBulletAbstractView : MonoBehaviour
    {
        /// <summary>オバケ弾：抽象クラスのビューモデル</summary>
        protected GhostBulletAbstractViewModel _viewModel;
        /// <summary>R3のリソース管理</summary>
        protected DisposableBag _disposableBag = new DisposableBag();

        protected virtual void OnDestroy()
        {
            _disposableBag.Dispose();
            _viewModel?.Dispose();
        }
    }

    /// <summary>
    /// オバケ弾：抽象クラスの設定
    /// </summary>
    [System.Serializable]
    public class GhostBulletAbstractSettings
    {
        /// <summary>オバケ弾：抽象クラスのビューモデル</summary>
        public GhostBulletAbstractViewModel viewModel;
    }
}
