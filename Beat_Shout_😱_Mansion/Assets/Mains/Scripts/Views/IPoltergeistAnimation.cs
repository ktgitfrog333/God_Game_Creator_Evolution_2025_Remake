using DG.Tweening;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// ポルターガイストアニメーションのインターフェース
    /// </summary>
    public interface IPoltergeistAnimation
    {
        /// <summary>
        /// アニメーション再生
        /// </summary>
        /// <param name="target">揺らし対象のTransform</param>
        /// <param name="initialLocalRotation">初期ローカル回転値（復帰用）</param>
        /// <returns>DOTween Sequence</returns>
        Sequence Play(Transform target, Quaternion initialLocalRotation);
    }
}
