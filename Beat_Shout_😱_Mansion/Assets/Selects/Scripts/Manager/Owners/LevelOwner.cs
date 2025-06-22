using Mains.Commons;
using R3;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Manager.Owners
{
    /// <summary>
    /// レベルオーナー
    /// </summary>
    public class LevelOwner : MonoBehaviour
    {
        [SerializeField] private OverridesDirectionalLightStruct DirectionalLightを継承して再設定;
        [SerializeField] private OverridesMainCameraStruct MainCameraを継承して再設定;
        /// <summary>初期処理の完了待ち</summary>
        private readonly ReactiveCommand<bool> _isCompleted = new ReactiveCommand<bool>();
        /// <summary>初期処理の完了待ち</summary>
        public ReactiveCommand<bool> IsCompleted => _isCompleted;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private async void Start()
        {
            var mainCamera = Camera.main;
            mainCamera.enabled = false;
            var directionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
            if (DirectionalLightを継承して再設定.Updateされる度に更新)
            {
                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        directionalLight.intensity = DirectionalLightを継承して再設定.intensity;
                        directionalLight.shadowStrength = DirectionalLightを継承して再設定.shadowStrength;
                        directionalLight.color = DirectionalLightを継承して再設定.lightColor;
                    })
                    .AddTo(ref _disposableBag);
            }
            else
            {
                directionalLight.intensity = DirectionalLightを継承して再設定.intensity;
                directionalLight.shadowStrength = DirectionalLightを継承して再設定.shadowStrength;
                directionalLight.color = DirectionalLightを継承して再設定.lightColor;
            }
            mainCamera.gameObject.layer = Mathf.RoundToInt(Mathf.Log(MainCameraを継承して再設定.PostProcessing用のLayer.value, 2));
            // Post-Process Layerコンポーネントを追加
            PostProcessLayer postProcessLayer = mainCamera.gameObject.GetComponent<PostProcessLayer>();
            if (postProcessLayer == null)
            {
                postProcessLayer = mainCamera.gameObject.AddComponent<PostProcessLayer>();
            }

            // Post-Process Layerの設定を調整
            postProcessLayer.volumeLayer = MainCameraを継承して再設定.PostProcessing用のLayer; // Post-Processing用のLayerを設定
            postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
            postProcessLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Medium;
            await Task.Delay(500);
            mainCamera.enabled = true;
            _isCompleted.Execute(true);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
