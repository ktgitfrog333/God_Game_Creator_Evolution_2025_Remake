using UnityEngine;

namespace Selects.Commons
{
    /// <summary>
    /// 生成パターンデータ
    /// </summary>
    [System.Serializable]
    public class MissilePatternData
    {
        /// <summary>生成パターンID</summary>
        public string patternId;
        /// <summary>生成パターン</summary>
        public string pattern;
        /// <summary>成功数</summary>
        public int successCount;
    }

    /// <summary>
    /// ノーツ生成パターン制御
    /// </summary>
    [CreateAssetMenu(fileName = "MissilePatternTable", menuName = "Scriptable Objects/MissilePatternTable")]
    public class MissilePatternTable : ScriptableObject
    {
        /// <summary>生成パターンデータ配列</summary>
        public MissilePatternData[] list;

        /// <summary>
        /// 生成パターンIDから生成パターンデータを取得
        /// </summary>
        /// <param name="patternId">生成パターンID</param>
        /// <returns>生成パターンデータ</returns>
        public MissilePatternData Get(string patternId)
        {
            if (list == null) return null;
            foreach (var data in list)
            {
                if (data.patternId == patternId) return data;
            }
            return null;
        }
    }
}
