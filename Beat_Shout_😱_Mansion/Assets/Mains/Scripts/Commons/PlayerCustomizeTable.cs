using System.Collections.Generic;
using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// プレイヤーのカスタマイズテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerCustomizeTable", menuName = "Scriptable Objects/PlayerCustomizeTable")]
    public class PlayerCustomizeTable : ScriptableObject, System.IDisposable
    {
        /// <summary>リズムパートのテーブル</summary>
        [SerializeField] private RhythmPartTable rhythmPartTable;
        /// <summary>プレイヤーへ強制的にヒットしない形にする</summary>
        public bool IsNoHitPlayerForceMode => rhythmPartTable.isNoHitPlayerForceMode;
        /// <summary>接地判定の対象タグデータ</summary>
        public GroundTagData[] groundTagDatas;
        /// <summary>接地判定の対象タグデータ</summary>
        private Dictionary<int, string> _groundTagDictionary;
        /// <summary>接地判定の対象タグデータ</summary>
        public Dictionary<int, string> GroundTagDictionary => _groundTagDictionary;

        public void Initialize()
        {
            _groundTagDictionary = new Dictionary<int, string>();
            foreach (var groundTagData in groundTagDatas)
            {
                _groundTagDictionary.Add(groundTagData.type, groundTagData.tag);
            }
        }

        public void Dispose()
        {
            _groundTagDictionary = null;
        }
    }

    /// <summary>
    /// 接地判定の対象タグデータ
    /// </summary>
    [System.Serializable]
    public class GroundTagData
    {
        /// <summary>地面タイプ</summary>
        public int type;
        /// <summary>地面タグ</summary>
        public string tag;
    }
}
