using UnityEngine;
using Mains.Commons;

namespace Selects.Commons
{
    /// <summary>
    /// レベル構造体
    /// </summary>
    [System.Serializable]
    public struct LevelStruct
    {
        [Tooltip("ステージごとに一意のLevelID（Enum）を指定")]
        public LevelID レベルID;
        public string ステージ名称;
        public string ステージ名称_物理名;
        [Tooltip("ステージのインデックス（何番目に攻略するか）を指定")]
        public int 階層;
    }
}
