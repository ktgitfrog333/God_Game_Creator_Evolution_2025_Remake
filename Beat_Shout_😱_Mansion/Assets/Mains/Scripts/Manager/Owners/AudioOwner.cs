using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Mains.Manager.Owners
{
    /// <summary>
    /// オーディオオーナー
    /// </summary>
    public class AudioOwner : MonoBehaviour
    {
        /// <summary>SEボリュームインデックス</summary>
        private float? _seVolumeIndex;

        private void Start()
        {
            var temp = new ResourcesUtility();
            var userBean = temp.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            if (userBean == null)
            {
                Debug.LogWarning($"JSONデータ読み込み失敗のためデフォルト値をセット");
            }
            _seVolumeIndex = userBean != null ? userBean.seVolumeIndex : 1f;
        }

        /// <summary>
        /// SEボリュームインデックスを取得
        /// </summary>
        /// <returns>SEボリュームインデックス</returns>
        public float GetSeVolumeIndex()
        {
            var seVolumeIndex = _seVolumeIndex;
            if (seVolumeIndex.HasValue)
            {
                return seVolumeIndex.Value;
            }
            else
            {
                var temp = new ResourcesUtility();
                var userBean = temp.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
                if (userBean == null)
                {
                    Debug.LogWarning($"JSONデータ読み込み失敗のためデフォルト値をセット");
                }
                _seVolumeIndex = userBean != null ? userBean.seVolumeIndex : 1f;

                return _seVolumeIndex.Value;
            }
        }
    }
}
