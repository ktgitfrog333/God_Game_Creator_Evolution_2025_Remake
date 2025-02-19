using R3;
using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// ポルターガイストのアニメーション管理テーブル
    /// </summary>
    [CreateAssetMenu(fileName = "PoltergeistTable", menuName = "Scriptable Objects/PoltergeistTable")]
    public class PoltergeistTable : ScriptableObject
    {
        [Tooltip("Assets/Mains/Animations/Poltergeists配下にあるAnimator.Controllerを全てセットしておく。ランダムでいずれかのアニメータが呼ばれる仕組み。")]
        public RuntimeAnimatorController[] poltergeistAnimatorControllers; // ポルターガイスト用アニメーション
        /// <summary>ポルターガイストが発生</summary>
        public ReactiveProperty<bool> isOnActionPoltergeist = new ReactiveProperty<bool>();
    }
}
