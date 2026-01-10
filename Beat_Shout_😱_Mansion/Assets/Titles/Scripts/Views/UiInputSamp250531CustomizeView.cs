using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Titles.Views
{
    /// <summary>
    /// タイトル画面のUI制御とナビゲーション管理カスタマイズビュー
    /// </summary>
    public class UiInputSamp250531CustomizeView : MonoBehaviour
    {
        /// <summary>タイトル画面のUI制御とナビゲーション管理カスタマイズ設定</summary>
        [SerializeField] private UiInputSamp250531CustomizeSettings settings;

        private void Start()
        {
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            var index = userBean.sceneIdx;
            if (index != settings.defaultSceneIdx)
            {
                index = settings.defaultSceneIdx;
                userBean.sceneIdx = index;
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, userBean);
            }
        }
    }

    /// <summary>
    /// タイトル画面のUI制御とナビゲーション管理カスタマイズ設定
    /// </summary>
    [System.Serializable]
    public class UiInputSamp250531CustomizeSettings
    {
        /// <summary>シーンインデックス初期値</summary>
        public int defaultSceneIdx;
    }
}
