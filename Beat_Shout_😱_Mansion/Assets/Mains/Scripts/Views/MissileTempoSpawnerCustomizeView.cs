using Mains.ViewModels;
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
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            MissileTempoSpawnerCustomizeViewModel missileTempoSpawnerCustomizeViewModel = new MissileTempoSpawnerCustomizeViewModel();
            Transform[] missileEffectContainers = null;
            Observable.EveryUpdate()
                .Select(_ => missileTempoSpawnerCustomizeViewModel.BatteryTransform)
                .Where(_ => missileEffectContainers != null &&
                    0 < missileEffectContainers.Length)
                .Subscribe(batteryTransform =>
                {
                    if (batteryTransform != null)
                    {
                        // NULLじゃない場合はノーツ非表示、ノールクリック不可状態にする
                        foreach (var missileEffectContainer in missileEffectContainers)
                        {
                            missileEffectContainer.localScale = Vector3.zero;
                        }
                    }
                    else
                    {
                        // NULLの場合はノーツ表示、ノールクリック可能状態にする
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
        }
    }
}
