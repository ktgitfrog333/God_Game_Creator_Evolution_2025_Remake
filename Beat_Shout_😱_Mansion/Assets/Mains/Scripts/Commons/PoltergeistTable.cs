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
        [Tooltip("Assets/Mains/Prefabs/Level/StaticObjects/SearchParts/StaticCollders.prefabをセットしておく。")]
        public Transform staticColldersPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/MissileTempoSpawner.prefabをセットしておく。")]
        public Transform missileTempoSpawnerPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/SearchParts/MissGhostEscape.prefabをセットしておく。")]
        public Transform missGhostEscapePrefab;
        /// <summary>ポルターガイストの発生位置</summary>
        public PoltergeistTableSubSettings subSettings;
    }

    /// <summary>
    /// ポルターガイストの管理テーブルのサブ設定
    /// </summary>
    [System.Serializable]
    public class PoltergeistTableSubSettings
    {
        /// <summary>オバケの攻撃タイプの設定</summary>
        public GhostAttack ghostAttack;

        /// <summary>
        /// オバケの攻撃タイプの設定
        /// </summary>
        [System.Serializable]
        public class GhostAttack
        {
            [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/SearchParts/GhostBulletBook.prefabをセットしておく。")]
            public Transform ghostBulletBookPrefab;
        }

        [Tooltip("スピードオバケが別の家具に移動するまでの時間（秒）")]
        public float moveIntervalSeconds = 30f;
    }
}
