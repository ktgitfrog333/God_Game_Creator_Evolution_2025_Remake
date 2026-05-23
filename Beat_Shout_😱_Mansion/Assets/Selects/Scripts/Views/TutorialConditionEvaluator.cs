using Universal.Commons;

namespace Selects.Views
{
    /// <summary>
    /// チュートリアルの実行条件・スキップ条件を判定する純粋クラス、
    /// Viewにもシーケンサーにも属さないメインロジックをここに記す
    /// </summary>
    public static class TutorialConditionEvaluator
    {
        /// <summary>
        /// チュートリアル全体をスキップにすべきか判定すめ
        /// </summary>
        /// <param name="bean">ユーザー情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldSkip(UserBean bean)
        {
            if (bean?.state == null || bean.state.Length < 5) return false;

            var s = bean.state;
            if (s[0] == 1 && s[1] == 0 && s[2] == 0 && s[3] == 0 && s[4] == 0)
                if (IsCompletedUpTo(bean, TutorialEventId.ETB0004)) return true;

            if (s[0] == 2 && s[1] == 0 && s[2] == 0 && s[3] == 0 && s[4] == 0) return true; // Fix? No, keep existing logic unless specified
            if (s[0] == 2 && s[1] == 1 && s[2] == 0 && s[3] == 0 && s[4] == 0) return true;

            if (s[0] == 2 && s[1] == 2 && s[2] == 1 && s[3] == 0 && s[4] == 0)
                if (IsCompletedUpTo(bean, TutorialEventId.ETS0001)) return true;

            if (s[0] == 2 && s[1] == 2 && s[2] == 2 && s[3] == 1 && s[4] == 0) return true;
            if (s[0] == 2 && s[1] == 2 && s[2] == 2 && s[3] == 2 && s[4] == 1) return true;
            if (s[0] == 2 && s[1] == 2 && s[2] == 2 && s[3] == 2 && s[4] == 2) return true;

            return false;
        }

        // =========================================================
        // チュートリアルの各ステップ実行条件
        // =========================================================

        /// <summary>
        /// チュートリアル_移動の実行判定
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldRunMove(UserBean bean)
            => IsBaseCondition1(bean) && !IsCompleted(bean, (int)TutorialEventId.ETB0000);

        /// <summary>
        /// チュートリアル_視点移動の実行判定
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldRunAimMove(UserBean bean)
            => IsBaseCondition1(bean)
            && IsCompleted(bean, (int)TutorialEventId.ETB0000)
            && !IsCompleted(bean, (int)TutorialEventId.ETB0001);

        /// <summary>
        /// チュートリアル_シャウトの実行判定
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldRunShout(UserBean bean)
            => IsBaseCondition1(bean)
            && IsCompleted(bean, (int)TutorialEventId.ETB0001)
            && !IsCompleted(bean, (int)TutorialEventId.ETB0002);

        /// <summary>
        /// チュートリアル_リズムパートの実行判定
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldRunRhythm(UserBean bean)
            => IsBaseCondition1(bean)
            && IsCompleted(bean, (int)TutorialEventId.ETB0002)
            && !IsCompleted(bean, (int)TutorialEventId.ETB0003);

        /// <summary>
        /// チュートリアル_ステージ1の案内の実行判定
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldRunStage1Guide(UserBean bean)
            => IsBaseCondition1(bean)
            && IsCompleted(bean, (int)TutorialEventId.ETB0003)
            && !IsCompleted(bean, (int)TutorialEventId.ETB0004);

        /// <summary>
        /// チュートリアル_シャウトノーツの案内の実行判定
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldRunShoutNoteGuide(UserBean bean)
            => IsBaseCondition2(bean)
            && IsCompleted(bean, (int)TutorialEventId.ETB0004)
            && !IsCompleted(bean, (int)TutorialEventId.ETS0000);

        /// <summary>
        /// チュートリアル_シャウトノーツの実行判定
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldRunShoutNote(UserBean bean)
            => IsBaseCondition2(bean)
            && IsCompleted(bean, (int)TutorialEventId.ETS0000)
            && !IsCompleted(bean, (int)TutorialEventId.ETS0001);

        /// <summary>
        /// チュートリアル_ステージ3の案内の実行判定
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        public static bool ShouldRunStage3Guide(UserBean bean)
            => IsBaseCondition2(bean)
            && IsCompleted(bean, (int)TutorialEventId.ETS0001)
            && !IsCompleted(bean, (int)TutorialEventId.ETS0002);

        /// <summary>
        /// 視点移動開始位置への移動判定
        /// </summary>
        /// <param name="ctx">コンテキスト</param>
        public static bool ShouldTeleportToMoveCompletePoint(TutorialSequencerContext ctx)
        {
            return true;
        }

        // =========================================================
        // 条件判定ヘルパー
        // =========================================================

        /// <summary>
        /// 条件
        /// </summary>
        /// <remarks>クリアステータス「1,0,0,0,0,」</remarks>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        private static bool IsBaseCondition1(UserBean bean)
            => bean.state[0] == 1 && bean.state[1] == 0
            && bean.state[2] == 0 && bean.state[3] == 0 && bean.state[4] == 0;

        /// <summary>
        /// 条件
        /// </summary>
        /// <remarks>クリアステータス「2,2,1,0,0,」</remarks>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <returns>実行可否</returns>
        private static bool IsBaseCondition2(UserBean bean)
            => bean.state[0] == 2 && bean.state[1] == 2
            && bean.state[2] == 1 && bean.state[3] == 0 && bean.state[4] == 0;

        /// <summary>
        /// イベント進捗が完了しているか
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <param name="eventId">イベントID</param>
        /// <returns>完了しているか</returns>
        public static bool IsCompleted(UserBean bean, int eventId)
        {
            if (bean.eventProgressList == null) return false;
            foreach (var p in bean.eventProgressList)
                if (p.eventId == eventId) return p.status == 1;
            return false;
        }

        /// <summary>
        /// 渡されたイベントIDまでの全てのイベントが完了しているか判定する
        /// ETB0000から指定イベントIDまで、一つでも未完了があればfalseを返す
        /// </summary>
        /// <param name="bean">ユーザ情報を保持するクラス</param>
        /// <param name="upToEventId">判定するイベントIDがETB0000～このIDまでを含む</param>
        /// <returns>全て完了しているか</returns>
        public static bool IsCompletedUpTo(UserBean bean, TutorialEventId upToEventId)
        {
            int start = (int)TutorialEventId.ETB0000;
            int end = (int)upToEventId;
            for (int i = start; i <= end; i++)
            {
                if (!IsCompleted(bean, i))
                    return false;
            }
            return true;
        }
    }
}

