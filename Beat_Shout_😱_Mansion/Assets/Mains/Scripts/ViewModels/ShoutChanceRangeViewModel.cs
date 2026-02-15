using Mains.Commons;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// シャウトチャンスの範囲ビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "ShoutChanceRangeViewModel", menuName = "Scriptable Objects/ShoutChanceRangeViewModel")]
    public class ShoutChanceRangeViewModel : ScriptableObject
    {
        /// <summary>利用ステータス</summary>
        public UseStatus UseStatus { get; set; }

        public void DoInitialize(UseStatus useStatus)
        {
            UseStatus = useStatus;
        }
    }
}
