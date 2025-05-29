using R3;
using Rewired;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// MissileDirectAnimManagerBの入力カスタマイズビュー
    /// </summary>
    public class MissileDirectAnimManagerBCustomizeInputView : MonoBehaviour
    {
        /// <summary>角度（180<=・=>0 を+-でセットされる）</summary>
        private float? _angle;
        /// <summary>角度</summary>
        public float? Angle => _angle;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            var player = ReInput.players.GetPlayer(0);
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    float moveX = player.GetAxis("RhythmMoveHorizontal");
                    float moveZ = player.GetAxis("RhythmMoveVertical");
                    // 入力が十分にあるときのみ計算
                    if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
                    {
                        float angle = Mathf.Atan2(moveZ, moveX) * Mathf.Rad2Deg;
                        _angle = angle;
                    }
                    else
                    {
                        _angle = null;
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
