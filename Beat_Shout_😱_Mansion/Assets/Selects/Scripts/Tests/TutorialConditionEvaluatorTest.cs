using Selects.Views;
using Universal.Commons;
using UnityEngine;

namespace Selects.Tests
{
    /// <summary>
    /// TutorialConditionEvaluatorのテスト
    /// </summary>
    /// <remarks>
    /// OnGUIボタンによる目視確認テスト<br/>
    /// パラメータは<see cref="StubSO"/>から取得
    /// </remarks>
    public class TutorialConditionEvaluatorTest : MonoBehaviour
    {
        [SerializeField] private StubSO stubSO;

        private void OnGUI()
        {
            int y = 10;
            const int buttonWidth = 500;
            const int buttonHeight = 50;
            const int spacing = 60;

            // =========================================================
            // IsCompletedの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "IsCompletedの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._IsCompletedの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.IsCompleted(bean, (int)stub.eventId);
                Debug.Log($"[テスト] IsCompleted(eventId:{stub.eventId}) => {result}");
            }
            y += spacing;

            // =========================================================
            // IsCompletedUpToの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "IsCompletedUpToの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._IsCompletedUpToの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.IsCompletedUpTo(bean, stub.upToEventId);
                Debug.Log($"[テスト] IsCompletedUpTo(upToEventId:{stub.upToEventId}) => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldSkipの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldSkipの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldSkipの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldSkip(bean);
                Debug.Log($"[テスト] ShouldSkip => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldRunMoveの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldRunMoveの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldRunMoveの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldRunMove(bean);
                Debug.Log($"[テスト] ShouldRunMove => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldRunAimMoveの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldRunAimMoveの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldRunAimMoveの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldRunAimMove(bean);
                Debug.Log($"[テスト] ShouldRunAimMove => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldRunShoutの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldRunShoutの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldRunShoutの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldRunShout(bean);
                Debug.Log($"[テスト] ShouldRunShout => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldRunRhythmの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldRunRhythmの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldRunRhythmの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldRunRhythm(bean);
                Debug.Log($"[テスト] ShouldRunRhythm => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldRunStage1Guideの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldRunStage1Guideの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldRunStage1Guideの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldRunStage1Guide(bean);
                Debug.Log($"[テスト] ShouldRunStage1Guide => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldRunShoutNoteGuideの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldRunShoutNoteGuideの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldRunShoutNoteGuideの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldRunShoutNoteGuide(bean);
                Debug.Log($"[テスト] ShouldRunShoutNoteGuide => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldRunShoutNoteの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldRunShoutNoteの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldRunShoutNoteの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldRunShoutNote(bean);
                Debug.Log($"[テスト] ShouldRunShoutNote => {result}");
            }
            y += spacing;

            // =========================================================
            // ShouldRunStage3Guideの確認
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "ShouldRunStage3Guideの確認"))
            {
                var stub = stubSO.commons.tutorialConditionEvaluatorTest._ShouldRunStage3Guideの確認;
                var bean = BuildUserBean(stub.userBeanParameter);
                var result = TutorialConditionEvaluator.ShouldRunStage3Guide(bean);
                Debug.Log($"[テスト] ShouldRunStage3Guide => {result}");
            }
        }

        /// <summary>
        /// StubSOのUserBeanParameterからUserBeanを構築する
        /// </summary>
        /// <param name="param">スタブのユーザー情報パラメータ</param>
        /// <returns>構築されたUserBean</returns>
        private UserBean BuildUserBean(Commons.TutorialConditionEvaluatorTest.UserBeanParameter param)
        {
            var bean = new UserBean();

            if (param.state != null && param.state.Length >= 5)
            {
                bean.state = (int[])param.state.Clone();
            }

            if (param.eventProgressList != null)
            {
                bean.eventProgressList = new System.Collections.Generic.List<EventProgress>();
                foreach (var ep in param.eventProgressList)
                {
                    bean.eventProgressList.Add(new EventProgress(ep.eventId, ep.status));
                }
            }

            return bean;
        }
    }
}
