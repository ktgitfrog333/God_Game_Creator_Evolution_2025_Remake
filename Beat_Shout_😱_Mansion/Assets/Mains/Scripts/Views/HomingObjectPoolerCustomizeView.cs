using Mains.External;
using R3;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// MissileObjectPoolerのカスタマイズビュー
    /// </summary>
    public class HomingObjectPoolerCustomizeView : MonoBehaviour
    {
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            Script_xyloApi script_XyloApi = new Script_xyloApi();
            var trans = transform;
            script_XyloApi.SetMissileObjectPooler(trans);
            // MissileEffectContainerが中央にいるなら大きさを0にする（有効／無効切り替えにすると既存スクリプトと競合するため暫定処置）
            Observable.EveryUpdate()
                .Select(_ => script_XyloApi.MissileGameObjects)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(missileGameObjects =>
                {
                    var missileEffectContainers = script_XyloApi.GetNoteTransforms(missileGameObjects);
                    foreach (var missileEffectContainer in missileEffectContainers)
                    {
                        Observable.EveryUpdate()
                            .Select(_ => missileEffectContainer)
                            .Subscribe(missileEffectContainer =>
                            {
                                missileEffectContainer.localScale = 0f < missileEffectContainer.localPosition.sqrMagnitude ? Vector3.one : Vector3.zero;
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
    }
}
