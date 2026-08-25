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
                    vibrationEnableIndex = 1;

                    break;
                case EnumLoadMode.ClearToSceneIdx_1:
                    sceneIdx = 5;
                    state = new int[]
                    {
                        2,
                        2,
                        1,
                        0,
                        0,
                    };
                    InitializeEventProgressList();
                    SetCompleteEventProgress((int)TutorialEventId.ETB0004);

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
                    SetCompleteEventProgress((int)TutorialEventId.ETS0002);

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

        /// <summary>
        /// イベント進捗配列にて指定されたチュートリアルのイベントIDまで完了にする
        /// </summary>
        /// <param name="eventId">チュートリアルのイベントID</param>
        private void SetCompleteEventProgress(int eventId)
        {
            var eventProgressList = this.eventProgressList;
            for (int i = 0; i < eventProgressList.Count; i++)
            {
                var eventProgress = eventProgressList[i];
                if ((TutorialEventId)eventProgress.eventId != TutorialEventId.None)
                {
                    if (eventProgress.eventId <= eventId)
                    {
                        eventProgress.status = 1;
                    }
                }
            }
        }
    }
}
