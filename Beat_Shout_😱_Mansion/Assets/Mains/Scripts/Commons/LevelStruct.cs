using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// レベル構造体
    /// </summary>
    [System.Serializable]
    public struct LevelStruct
    {
        [Tooltip("ステージごとに一意のLevelID（Enum）を指定")]
        public LevelID レベルID;
        [Tooltip("ステージのインデックス（何番目に攻略するか）を指定")]
        public int 階層;
        [Tooltip("Assets/Mains/Prefabs/Level/Stages/Stage_0.prefab 等を指定")]
        public GameObject Stage_xと書かれたプレハブ;
    }
}
