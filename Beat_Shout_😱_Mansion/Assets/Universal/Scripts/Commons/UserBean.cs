namespace Universal.Commons
{
    [System.Serializable]
    /// <summary>
    /// ユーザー情報を保持するクラス
    /// </summary>
    public class UserBean
    {
        /// <summary>シーンインデックス</summary>
        public int sceneIdx = 0;

        /// <summary>
        /// ユーザー情報を保持するクラス
        /// </summary>
        public UserBean(EnumLoadMode enumLoadMode = EnumLoadMode.Continue)
        {
            switch (enumLoadMode)
            {
                case EnumLoadMode.Continue:
                    break;
                case EnumLoadMode.Default:
                    sceneIdx = 0;

                    break;
                case EnumLoadMode.All:
                    // デッドロジック（ステージ全開放機能はない）
                    sceneIdx = 0;

                    break;
            }
        }

        /// <summary>
        /// ユーザー情報を保持するクラス
        /// </summary>
        public UserBean(UserBean userBean)
        {
            sceneIdx = userBean.sceneIdx;
        }
    }
}
