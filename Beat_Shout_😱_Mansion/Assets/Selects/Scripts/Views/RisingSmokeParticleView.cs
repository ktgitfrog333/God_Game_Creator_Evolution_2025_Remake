using Selects.Commons;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Views
{
    /// <summary>
    /// 黒煙パーティクルビュー
    /// </summary>
    public class RisingSmokeParticleView : MonoBehaviour
    {
        [SerializeField] private LevelStruct レベル構造体;

        private void Start()
        {
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            var index = レベル構造体.階層;
            if (index < 5)
            {
                var status = userBean.state[index];
                if (0 < status)
                    // 解放状態なら非表示
                    gameObject.SetActive(false);
            }
        }
    }
}
