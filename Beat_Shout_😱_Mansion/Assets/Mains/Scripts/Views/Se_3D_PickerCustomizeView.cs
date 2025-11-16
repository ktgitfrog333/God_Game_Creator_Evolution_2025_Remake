using Mains.External;
using R3;
using Unity.Collections;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// Se_3D_Pickerのカスタマイズビュー
    /// </summary>
    public class Se_3D_PickerCustomizeView : MonoBehaviour
    {
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _xyloApi;
        /// <summary>完了の通知</summary>
        private bool _isCompleted;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            _xyloApi = new Script_xyloApi();
            _xyloApi.InitializeSe_3D_Picker(transform);
            _isCompleted = true;
        }

        private void OnDestroy()
        {
            _xyloApi?.Dispose();
            _disposableBag.Dispose();
        }

        public void PlaySound(string SeName, float volume)
        {
            Observable.EveryUpdate()
                .Select(_ => _isCompleted)
                .Where(x => x)
                .Take(1)
                .Select(_ => _xyloApi)
                .Subscribe(api =>
                {
                    api.PlaySound(SeName, volume);
                })
                .AddTo(ref _disposableBag);
        }
    }
}
