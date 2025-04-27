using R3;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// MissGhostAttackのカスタマイズビュー
    /// </summary>
    public class MissGhostAttackCustomizeView : MonoBehaviour
    {
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            Transform trans = transform;
            var camera = Camera.main.transform;
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    trans.LookAt(camera.position);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
