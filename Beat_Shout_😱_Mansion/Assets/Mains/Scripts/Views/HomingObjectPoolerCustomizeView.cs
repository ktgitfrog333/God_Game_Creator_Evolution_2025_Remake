using Mains.External;
using Mains.ViewModels;
using R3;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// MissileObjectPoolerのカスタマイズビュー
    /// </summary>
    public class HomingObjectPoolerCustomizeView : MonoBehaviour
    {
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            _script_XyloApi = new Script_xyloApi();
            var trans = transform;
            _script_XyloApi.SetMissileObjectPooler(trans);
            HomingObjectPoolerCustomizeViewModel viewModel = new HomingObjectPoolerCustomizeViewModel();
            bool isFoundedBattery = false;
            Observable.EveryUpdate()
                .Select(_ => viewModel.BatteryTransform)
                .Subscribe(batteryTransform =>
                {
                    if (batteryTransform != null)
                    {
                        // NULLじゃない場合はノーツ非表示、ノーツクリック不可状態にする
                        isFoundedBattery = true;
                    }
                    else
                    {
                        // NULLの場合はノーツ表示、ノーツクリック可能状態にする
                        isFoundedBattery = false;
                    }
                })
                .AddTo(ref _disposableBag);
            Transform overlayCanvas = null;
            Observable.EveryUpdate()
                .Select(_ => GameObject.Find("OverlayCanvas"))
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    overlayCanvas = x.transform;
                })
                .AddTo(ref _disposableBag);
            // MissileEffectContainerが中央にいるなら大きさを0にする（有効／無効切り替えにすると既存スクリプトと競合するため暫定処置）
            Observable.EveryUpdate()
                .Select(_ => _script_XyloApi.MissileGameObjects)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(missileGameObjects =>
                {
                    var missileEffectContainers = _script_XyloApi.GetNoteTransforms(missileGameObjects);
                    foreach (var missileEffectContainer in missileEffectContainers)
                    {
                        Observable.EveryUpdate()
                            .Select(_ => missileEffectContainer)
                            .Subscribe(missileEffectContainer =>
                            {
                                if (!isFoundedBattery &&
                                    0f < missileEffectContainer.localPosition.sqrMagnitude)
                                {
                                    missileEffectContainer.localScale = Vector3.one;
                                }
                                else
                                {
                                    missileEffectContainer.localScale = Vector3.zero;
                                }
                                if (overlayCanvas != null &&
                                    !missileEffectContainer.parent.Equals(overlayCanvas))
                                {
                                    missileEffectContainer.SetParent(overlayCanvas);
                                }
                            })
                            .AddTo(ref _disposableBag);
                    }
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// HomingObjectPoolerのReturnAllMissilesToPoolメソッドを呼び出し
        /// </summary>
        public void DoReturnAllMissilesToPool()
        {
            _script_XyloApi.ReturnAllMissilesToPool();
            _script_XyloApi.SetActiveObjectCount(0);
        }
    }
}
