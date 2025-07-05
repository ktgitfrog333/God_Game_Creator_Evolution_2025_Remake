using Mains.External;
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

        private void Start()
        {
            _xyloApi = new Script_xyloApi();
            _xyloApi.InitializeSe_3D_Picker(transform);
        }

        private void OnDestroy()
        {
            _xyloApi?.Dispose();
        }

        public void PlaySound(string SeName, float volume)
        {
            if (_xyloApi != null)
            {
                _xyloApi.PlaySound(SeName, volume);
            }
        }
    }
}
