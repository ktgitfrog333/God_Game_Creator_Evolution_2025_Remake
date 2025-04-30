using Mains.Commons;
using Mains.ViewModels;
using R3;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// バッテリーのビュー
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class BatteryView : MonoBehaviour
    {
        /// <summary>カプセルコライダー</summary>
        [SerializeField] private SphereCollider sphereCollider;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (sphereCollider == null)
                sphereCollider = GetComponent<SphereCollider>();
        }

        private void OnEnable()
        {
            if (sphereCollider.enabled)
                sphereCollider.enabled = false;
        }

        private void Start()
        {
            BatteryViewModel viewModel = new BatteryViewModel();
            Observable.EveryUpdate()
                .Select(_ => viewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Where(x => !x.Equals(InteractionPart.Rhythm))
                        .Subscribe(_ =>
                        {
                            Destroy(gameObject);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// コライダーへ有効／無効をセット
        /// </summary>
        /// <param name="enabled">有効／無効</param>
        public void SetEnabledCollider(bool enabled)
        {
            if (sphereCollider.enabled != enabled)
                sphereCollider.enabled = enabled;
        }

        /// <summary>
        /// 電池をDestroy
        /// </summary>
        public void GetBattery()
        {
            Destroy(gameObject);
        }
    }
}
