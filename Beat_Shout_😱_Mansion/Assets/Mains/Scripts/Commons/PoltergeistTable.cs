using ObservableCollections;
using R3;
using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// ポルターガイストの管理テーブル
    /// </summary>
    [CreateAssetMenu(fileName = "PoltergeistTable", menuName = "Scriptable Objects/PoltergeistTable")]
    public class PoltergeistTable : ScriptableObject
    {
        [Tooltip("Assets/Mains/Animations/Poltergeists配下にあるAnimator.Controllerを全てセットしておく。ランダムでいずれかのアニメータが呼ばれる仕組み。")]
        public RuntimeAnimatorController[] poltergeistAnimatorControllers; // ポルターガイスト用アニメーション
        /// <summary>ポルターガイストの発生位置</summary>
        public ReactiveProperty<Vector3> onActionPoltergeistPosition = new ReactiveProperty<Vector3>();
    }
}
