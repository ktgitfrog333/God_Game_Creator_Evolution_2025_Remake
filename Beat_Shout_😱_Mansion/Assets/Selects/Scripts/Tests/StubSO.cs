using UnityEngine;

namespace Selects.Tests
{
    /// <summary>
    /// スタブ
    /// </summary>
    [CreateAssetMenu(fileName = "StubSO", menuName = "Scriptable Objects/Selects/StubSO")]
    public class StubSO : ScriptableObject
    {
        public Commons commons;
        public Views views;
    }

    [System.Serializable]
    public class Commons
    {
        public CommonPanelCustomizeOfMainViewTest commonPanelCustomizeOfMainViewTest;
        public RhythmPartPanelCustomizeOfMainViewTest rhythmPartPanelCustomizeOfMainViewTest;
        public TutorialPanelViewTest tutorialPanelViewTest;
        public TutorialConditionEvaluatorTest tutorialConditionEvaluatorTest;
        public RewiredControllerMapTest rewiredControllerMapTest;

        [System.Serializable]
        public class RhythmPartPanelCustomizeOfMainViewTest
        {
            public リズムパート状態 _リズムパート状態;

            [System.Serializable]
            public class リズムパート状態
            {
                public Mains.Commons.InteractionPart interactionPart;
            }
        }

        [System.Serializable]
        public class CommonPanelCustomizeOfMainViewTest
        {
            public 最大HPがセットされることを確認 _最大HPがセットされることを確認;
            public 現在HPがセットされることを確認 _現在HPがセットされることを確認;
            public ハートが減少する演出が実行されることを確認 _ハートが減少する演出が実行されることを確認;

            [System.Serializable]
            public class 最大HPがセットされることを確認
            {
                /// <summary>プレイヤーの最大HP</summary>
                public int playerHealthPointMax;
            }

            [System.Serializable]
            public class 現在HPがセットされることを確認
            {
                /// <summary>プレイヤーのHP</summary>
                public int playerHealthPoint;
            }

            [System.Serializable]
            public class ハートが減少する演出が実行されることを確認
            {
                /// <summary>オバケ攻撃のヒットフラグ</summary>
                public bool isHitGhostAttack;
            }
        }

        [System.Serializable]
        public class TutorialPanelViewTest
        {
            public ApplyMessageの確認 _ApplyMessageの確認;
            public ApplyMessageWithProgressの確認 _ApplyMessageWithProgressの確認;
            public FadeInAsyncの確認 _FadeInAsyncの確認;
            public FadeOutAsyncの確認 _FadeOutAsyncの確認;

            [System.Serializable]
            public class ApplyMessageの確認
            {
                public string messageId;
            }

            [System.Serializable]
            public class ApplyMessageWithProgressの確認
            {
                public string messageId;
                public string current;
                public string total;
            }

            [System.Serializable]
            public class FadeInAsyncの確認
            {
                public float duration;
            }

            [System.Serializable]
            public class FadeOutAsyncの確認
            {
                public float duration;
            }
        }

        [System.Serializable]
        public class TutorialConditionEvaluatorTest
        {
            public IsCompletedの確認 _IsCompletedの確認;
            public IsCompletedUpToの確認 _IsCompletedUpToの確認;
            public ShouldSkipの確認 _ShouldSkipの確認;
            public ShouldRunMoveの確認 _ShouldRunMoveの確認;
            public ShouldRunAimMoveの確認 _ShouldRunAimMoveの確認;
            public ShouldRunShoutの確認 _ShouldRunShoutの確認;
            public ShouldRunRhythmの確認 _ShouldRunRhythmの確認;
            public ShouldRunStage1Guideの確認 _ShouldRunStage1Guideの確認;
            public ShouldRunShoutNoteGuideの確認 _ShouldRunShoutNoteGuideの確認;
            public ShouldRunShoutNoteの確認 _ShouldRunShoutNoteの確認;
            public ShouldRunStage3Guideの確認 _ShouldRunStage3Guideの確認;

            [System.Serializable]
            public class UserBeanParameter
            {
                /// <summary>クリアステータス</summary>
                public int[] state = new int[] { 1, 0, 0, 0, 0 };
                /// <summary>イベント進捗配列</summary>
                public Universal.Commons.EventProgress[] eventProgressList;
            }

            [System.Serializable]
            public class IsCompletedの確認
            {
                public UserBeanParameter userBeanParameter;
                public Universal.Commons.TutorialEventId eventId;
            }

            [System.Serializable]
            public class IsCompletedUpToの確認
            {
                public UserBeanParameter userBeanParameter;
                public Universal.Commons.TutorialEventId upToEventId;
            }

            [System.Serializable]
            public class ShouldSkipの確認
            {
                public UserBeanParameter userBeanParameter;
            }

            [System.Serializable]
            public class ShouldRunMoveの確認
            {
                public UserBeanParameter userBeanParameter;
            }

            [System.Serializable]
            public class ShouldRunAimMoveの確認
            {
                public UserBeanParameter userBeanParameter;
            }

            [System.Serializable]
            public class ShouldRunShoutの確認
            {
                public UserBeanParameter userBeanParameter;
            }

            [System.Serializable]
            public class ShouldRunRhythmの確認
            {
                public UserBeanParameter userBeanParameter;
            }

            [System.Serializable]
            public class ShouldRunStage1Guideの確認
            {
                public UserBeanParameter userBeanParameter;
            }

            [System.Serializable]
            public class ShouldRunShoutNoteGuideの確認
            {
                public UserBeanParameter userBeanParameter;
            }

            [System.Serializable]
            public class ShouldRunShoutNoteの確認
            {
                public UserBeanParameter userBeanParameter;
            }

            [System.Serializable]
            public class ShouldRunStage3Guideの確認
            {
                public UserBeanParameter userBeanParameter;
            }
        }

        [System.Serializable]
        public class RewiredControllerMapTest
        {
            public string categoryName;
        }
    }

    [System.Serializable]
    public class Views
    {
        public PlayerTeleporterStrategySOTest playerTeleporterStrategySOTest;
        public StartDirectionSequencerTest startDirectionSequencerTest;

        [System.Serializable]
        public class StartDirectionSequencerTest
        {
            public Selects.Views.StartDirectionSequencer startDirectionSequencer;
            public string targetSceneName;
            public Universal.Commons.UserBean userBean;
            public Selects.Views.PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLink;
            public Selects.Views.PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLinkTween;
            public Selects.Views.PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLinkFadeAndTeleport;
            public Selects.Views.PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLinkTeleport;
            public bool preProcessCharacterControllerEnabled;
            public int preProcessPlayerEnabled;
            public float preHideEffectDuration;
            public Vector3 teleportPosition;
            public Vector3 teleportAngles;
            public float postHideEffectDuration;
            public bool andFromTweenMode;
            public Vector3 tweenMovePosition;
            public Vector3 tweenMoveAngles;
            public float[] tweenMoveDurations;
            public bool postProcessCharacterControllerEnabled;
            public int postProcessPlayerEnabled;
        }

        [System.Serializable]
        public class PlayerTeleporterStrategySOTest
        {
            public Selects.Views.PlayerTeleporterStrategySO playerTeleporterStrategySO;
            public Selects.Views.PlayerTeleporterStrategySO1 playerTeleporterStrategySO1;
            public Selects.Views.PlayerTeleporterStrategySO2 playerTeleporterStrategySO2;
            public Vector3 position;
            public Vector3 angles;
            public bool isCompletedStartDirection;
            public Selects.Views.StartDirectionSequencer startDirectionSequencer;
            public Selects.Views.PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLinkTween;
            public Selects.Views.PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLinkFadeAndTeleport;
            public Selects.Views.PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLinkTeleport;
        }
    }
}
