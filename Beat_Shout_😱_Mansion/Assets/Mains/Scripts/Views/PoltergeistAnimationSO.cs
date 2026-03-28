using DG.Tweening;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// ポルターガイストアニメーションの抽象基底ScriptableObject
    /// </summary>
    public abstract class PoltergeistAnimationSO : ScriptableObject, IPoltergeistAnimation
    {
        public abstract Sequence Play(Transform target, Quaternion initialLocalRotation);
    }
}
