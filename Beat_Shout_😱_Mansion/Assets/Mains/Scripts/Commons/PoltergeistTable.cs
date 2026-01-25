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
        [Tooltip("Assets/Mains/Prefabs/Level/Motor.prefabをセットしておく。")]
        public GameObject motorPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/ShoutChanceRange.prefabをセットしておく。")]
        public GameObject shoutChanceRangePrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/MissileTempoSpawner.prefabをセットしておく。")]
        public Transform missileTempoSpawnerPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/SearchParts/MissGhostEscape.prefabをセットしておく。")]
        public Transform missGhostEscapePrefab;
    }
}
