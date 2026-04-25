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
        /// <summary>シャウト範囲のデフォルト値</summary>
        public float defaultShoutRadius = 1.5f;
        /// <summary>音の出力タイプのデフォルト値</summary>
        public SoundOutputType defaultSoundOutputType = SoundOutputType.Loop;
        /// <summary>笑い声の再生間隔・遅延・エコー設定</summary>
        public PoltergeistLaughSettings laughSettings = new PoltergeistLaughSettings();

        /// <summary>
        /// オバケ笑い声の再生間隔・遅延・エコー設定
        /// </summary>
        [System.Serializable]
        public class PoltergeistLaughSettings
        {
            [Header("ループ間隔設定")]
            [Tooltip("ベースとなる笑い声の間隔（秒）")]
            public float baseInterval = 2f;
            [Tooltip("間隔が延び始めるまでの基準再生回数")]
            public int slowDownStartCount = 3;
            [Tooltip("1回ごとに延びる間隔の増加秒数")]
            public float slowDownIncrement = 1f;
            [Tooltip("間隔の最大値（この秒数以上は遅くならない）")]
            public float maxInterval = 5f;

            [Header("エコー設定")]
            [Tooltip("エコーが発生する距離の閾値（シャウトラジアスに対する割合。0.5なら最大距離の半分まで近づいた時）")]
            public float echoDistanceThresholdRatio = 0.5f;
            [Tooltip("エコーの遅延時間（秒）")]
            public float echoDelay = 0.3f;
            [Tooltip("エコーの最大音量（メイン音量を1としたときの割合）")]
            public float maxEchoVolumeRatio = 0.6f;
            [Tooltip("エコーの最小音量（メイン音量を1としたときの割合）")]
            public float minEchoVolumeRatio = 0.2f;
        }

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
        [Tooltip("ノルマ達成率")]
        public float targetkillsRate = .8f;
        [Tooltip("クリア判定")]
        public CheckClearStruct checkClearStruct;

        /// <summary>
        /// クリア判定
        /// </summary>
        [System.Serializable]
        public struct CheckClearStruct
        {
            /// <summary>敵戦パート</summary>
            public EnemyBattlePart enemyBattlePart;
        }
    }
}
