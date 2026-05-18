using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// レベル構造体管理テーブル
    /// </summary>
    [CreateAssetMenu(fileName = "LevelTable", menuName = "Scriptable Objects/LevelTable")]
    public class LevelTable : ScriptableObject
    {
        public LevelStruct[] レベル構造体リスト;
    }
}
