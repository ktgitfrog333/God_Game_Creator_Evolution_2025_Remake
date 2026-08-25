using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Manager.Owners
{
    /// <summary>
    /// オーディオオーナー
    /// </summary>
    public class AudioOwner : MonoBehaviour
    {
        /// <summary>
        /// SEボリュームインデックスを取得
        /// </summary>
        /// <returns>SEボリュームインデックス</returns>
        public float GetSeVolumeIndex()
        {
            // 個別設定は不要のため固定値を返す
            return 1f;
        }
    }
}
