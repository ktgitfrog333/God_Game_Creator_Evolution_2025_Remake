using Mains.Commons;
using Mains.External;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Mains.Views
{
    /// <summary>
    /// MicInput_Detectorのカスタマイズビュー
    /// </summary>
    public class MicInput_DetectorCustomizeView : MonoBehaviour
    {
        /// <summary>スライダーの再設定</summary>
        /// <remarks>詳細はAPIを確認</remarks>
        /// <see cref="Script_xyloApi._isVolumeSliderMaxValueToTwo"/>
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;
        [SerializeField] private bool wholeNumbers;
        [SerializeField] private bool interactable;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            _script_XyloApi = new Script_xyloApi();
            _script_XyloApi.IsVolumeSliderMaxValueToTwo.Where(x => x)
                .Take(1)
                .Subscribe(_ =>
                {
                    _script_XyloApi.SetVolumeSliderMaxValue(minValue, maxValue, wholeNumbers, interactable);
                })
                .AddTo(ref _disposableBag);
            _script_XyloApi.SetVolumeSliderMaxValueToTwo();
        }

        private void OnDestroy()
        {
            _script_XyloApi?.Dispose();
        }
    }
}
