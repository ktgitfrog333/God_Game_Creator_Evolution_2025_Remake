using DG.Tweening;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// 壁掛けオブジェクト用の揺らしアニメーション（DOShakeRotation）
    /// </summary>
    [CreateAssetMenu(fileName = "WallHangingShakeAnimation", menuName = "Scriptable Objects/WallHangingShakeAnimation")]
    public class WallHangingShakeAnimationSO : PoltergeistAnimationSO
    {
        /// <summary>壁掛けオブジェクト用の揺らしアニメーション（DOShakeRotation）設定</summary>
        [SerializeField] private WallHangingShakeAnimationSOSettings settings;

        public override Sequence Play(Transform target, Quaternion initialLocalRotation)
        {
            // 壁掛けオブジェクト用の揺らしアニメーション（DOShakeRotation）設定
            var set = settings;
            return DOTween.Sequence()
                .Append(target.DOShakeRotation(set.duration, set.strength, set.vibrato, set.randomness, set.fadeOut))
                .OnComplete(() => target.localRotation = initialLocalRotation);
        }
    }

    /// <summary>
    /// 壁掛けオブジェクト用の揺らしアニメーション（DOShakeRotation）設定
    /// </summary>
    [System.Serializable]
    public class WallHangingShakeAnimationSOSettings
    {
        /// <summary>アニメーション終了時間</summary>
        public float duration = 0.6f;
        /// <summary>振動ベクター</summary>
        public Vector3 strength = new Vector3(3f, 0f, 8f);
        /// <summary>振動数</summary>
        public int vibrato = 12;
        /// <summary>手ブレ値</summary>
        public float randomness = 90f;
        /// <summary>フェードアウト</summary>
        public bool fadeOut = true;
    }
}
