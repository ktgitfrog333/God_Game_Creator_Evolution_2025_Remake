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
    }

    [System.Serializable]
    public class Commons
    {
        public CommonPanelCustomizeOfMainViewTest commonPanelCustomizeOfMainViewTest;
        public RhythmPartPanelCustomizeOfMainViewTest rhythmPartPanelCustomizeOfMainViewTest;
        public TutorialPanelViewTest tutorialPanelViewTest;

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
    }
}
