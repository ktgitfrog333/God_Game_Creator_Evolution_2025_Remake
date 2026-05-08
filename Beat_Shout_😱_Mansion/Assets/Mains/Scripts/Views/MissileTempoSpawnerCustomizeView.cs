using Mains.ViewModels;
using Mains.External;
using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// MissileTempoSpawnerのカスタマイズのビュー
    /// </summary>
    public class MissileTempoSpawnerCustomizeView : MonoBehaviour
    {
        /// <summary>MissileTempoSpawnerのカスタマイズのビューモデル</summary>
        private MissileTempoSpawnerCustomizeViewModel _viewModel;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            _viewModel = new MissileTempoSpawnerCustomizeViewModel();
            _script_XyloApi = new Script_xyloApi();
            
            var spawner = Object.FindAnyObjectByType<MissileTempoSpawner>();
            if (spawner != null)
            {
                _script_XyloApi.SetMissileTempoSpawner(spawner.transform);
            }
            Transform[] missileEffectContainers = null;
            Observable.EveryUpdate()
                .Select(_ => _viewModel.BatteryTransform)
                .Where(_ => missileEffectContainers != null &&
                    0 < missileEffectContainers.Length)
                .Subscribe(batteryTransform =>
                {
                    if (batteryTransform != null)
                    {
                        // NULLじゃない場合はノーツ非表示、ノーツクリック不可状態にする
                        foreach (var missileEffectContainer in missileEffectContainers)
                        {
                            missileEffectContainer.localScale = Vector3.zero;
                        }
                    }
                    else
                    {
                        // NULLの場合はノーツ表示、ノーツクリック可能状態にする
                        foreach (var missileEffectContainer in missileEffectContainers)
                        {
                            missileEffectContainer.localScale = Vector3.one;
                        }
                    }
                })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Select(_ => GameObject.Find("MissileEffectContainer"))
                .Where(x => x != null)
                .Take(1)
                .Subscribe(missileEffectContainer =>
                {
                    if (missileEffectContainer != null)
                    {
                        var parent = missileEffectContainer.transform.parent;
                        List<Transform> containers = new List<Transform>();
                        foreach (Transform child in parent)
                        {
                            if (child.name.Equals("MissileEffectContainer"))
                            {
                                containers.Add(child);
                            }
                        }
                        missileEffectContainers = containers.ToArray();
                    }

                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _viewModel?.Dispose();
            _script_XyloApi?.Dispose();
        }
    }
}
