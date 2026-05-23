using System.Linq;

namespace Universal.Commons
{
    [System.Serializable]
    /// <summary>
    /// ユーザー情報を保持するクラス
    /// </summary>
    public class UserBean
    {
        /// <summary>シーンインデックス</summary>
        public int sceneIdx = 5;
        /// <summary>クリアステータス</summary>
        /// <remarks>0:ステージ未開放<br/>
        /// 1:ステージ解放<br/>
        /// 2:ステージクリア済み</remarks>
        public int[] state = new int[]
        {
            1,
            0,
            0,
            0,
            0,
        };
        /// <summary>BGMボリュームインデックス</summary>
        /// <remarks>範囲:0～10<br/>
        /// デフォルト:5</remarks>
        public int bgmVolumeIndex = 5;
        /// <summary>SEボリュームインデックス</summary>
        /// <remarks>範囲:0.0 - 1.0<br/>
        /// デフォルト:1.0</remarks>
        public float seVolumeIndex = 1.0f;
        /// <summary>振動有効インデックス</summary>
        /// <remarks>0:振動オフ<br/>
        /// 1:振動オン</remarks>
        public int vibrationEnableIndex = 1;
        /// <summary>イベント進捗配列</summary>
        public System.Collections.Generic.List<EventProgress> eventProgressList = new System.Collections.Generic.List<EventProgress>();

        /// <summary>
        /// ユーザー情報を保持するクラス
        /// </summary>
        public UserBean()
        {
            InitializeEventProgressList();
        }

        /// <summary>
        /// ユーザー情報を保持するクラス
        /// </summary>
        public UserBean(UserBean userBean)
        {
            sceneIdx = userBean.sceneIdx;
            state = userBean.state.ToArray();
            bgmVolumeIndex = userBean.bgmVolumeIndex;
            seVolumeIndex = userBean.seVolumeIndex;
            vibrationEnableIndex = userBean.vibrationEnableIndex;
            eventProgressList = new System.Collections.Generic.List<EventProgress>();
            if (userBean.eventProgressList != null)
            {
                foreach (var progress in userBean.eventProgressList)
                {
                    eventProgressList.Add(new EventProgress(progress));
                }
            }
        }

        /// <summary>
        /// ユーザー情報を保持するクラス
        /// </summary>
        public UserBean(UserBean userBean, EnumLoadMode enumLoadMode = EnumLoadMode.Continue)
            : this(userBean) // まずコピー
        {
            switch (enumLoadMode)
            {
                case EnumLoadMode.Default:
                    sceneIdx = 5;
                    state = new int[]
                    {
                        1,
                        0,
                        0,
                        0,
                        0,
                    };
                    InitializeEventProgressList();

                    break;
                case EnumLoadMode.Default1:
                    bgmVolumeIndex = 5;
                    seVolumeIndex = 1.0f;
                    vibrationEnableIndex = 1;

                    break;
                case EnumLoadMode.All:
                    sceneIdx = 5;
                    state = new int[]
                    {
                        2,
                        2,
                        2,
                        2,
                        2,
                    };
                    InitializeEventProgressList();

                    break;
            }
        }

        /// <summary>
        /// イベント進捗配列の初期化
        /// </summary>
        private void InitializeEventProgressList()
        {
            eventProgressList = new System.Collections.Generic.List<EventProgress>();
            foreach (TutorialEventId eventId in System.Enum.GetValues(typeof(TutorialEventId)))
            {
                if (eventId != TutorialEventId.None)
                {
                    eventProgressList.Add(new EventProgress((int)eventId, 0));
                }
            }
        }
    }
}
