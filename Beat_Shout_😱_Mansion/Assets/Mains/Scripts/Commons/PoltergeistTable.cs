using ObservableCollections;
using R3;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// モデルタイプに応じたプレハブを取得する
        /// </summary>
        /// <param name="mappings">マッピングリスト</param>
        /// <param name="modelType">オバケモデルタイプ</param>
        /// <returns>対応するプレハブ（見つからない場合はnull）</returns>
        public Transform GetPrefabByModelType(List<GhostModelTypePrefabMapping> mappings, GhostModelType modelType)
        {
            var mapping = mappings.FirstOrDefault(m => m.ghostModelType == modelType);
            return mapping != null ? mapping.prefab : null;
        }
    }

    /// <summary>
    /// オバケモデルタイプとプレハブの紐付け
    /// </summary>
    [System.Serializable]
    public class GhostModelTypePrefabMapping
    {
        /// <summary>オバケモデルタイプ</summary>
        public GhostModelType ghostModelType;
        /// <summary>対応するプレハブ</summary>
        public Transform prefab;
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
        /// <summary>笑い声の再生間隔設定</summary>
        public PoltergeistLaughSettings laughSettings = new PoltergeistLaughSettings();

        /// <summary>
        /// オバケ笑い声の再生間隔設定
        /// </summary>
        /// <remarks>1. 最初の笑い声を再生<br/>
        /// 2. 最初の笑い声の再生回数を超えた場合、次点の笑い声再生へ移行<br/>
        /// 3. 次点の笑い声を再生<br/>
        /// 4. 最初の笑い声の再生回数を超えた場合、間隔は最大値になる</remarks>
        [System.Serializable]
        public class PoltergeistLaughSettings
        {
            [Header("ループ間隔設定")]
            [Tooltip("最初の笑い声の再生間隔（秒）")]
            public float firstInterval = 1f;
            [Tooltip("最初の笑い声の再生回数")]
            public int firstStartCount = 1;
            [Tooltip("次点の笑い声の再生間隔（秒）")]
            public float secondInterval = 2.5f;
            [Tooltip("次点の笑い声の再生回数")]
            public int secondStartCount = 2;
            [Tooltip("間隔の最大値（この秒数以上は遅くならない）")]
            public float maxInterval = 5f;
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

        [Header("オバケモデルタイプ別プレハブマッピング")]
        /// <summary>オバケモデルタイプ別プレハブ設定</summary>
        public GhostModelTypePrefabSettings ghostModelTypePrefabSettings;

        /// <summary>
        /// オバケモデルタイプ別プレハブ設定
        /// </summary>
        [System.Serializable]
        public class GhostModelTypePrefabSettings
        {

            [Tooltip("探索パート：モデルタイプ別の逃走用オバケプレハブ")]
            public List<GhostModelTypePrefabMapping> missGhostEscapePrefabMappings = new List<GhostModelTypePrefabMapping>();
        }
    }
}
