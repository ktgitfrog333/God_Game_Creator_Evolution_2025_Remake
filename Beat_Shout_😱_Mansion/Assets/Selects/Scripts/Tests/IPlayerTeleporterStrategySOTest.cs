using Cysharp.Threading.Tasks;
using Mains.Views;
using Rewired;
using Selects.Views;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Tests
{
    /// <summary>
    /// プレイヤー移動演出ストラテジーテスト
    /// </summary>
    public class IPlayerTeleporterStrategySOTest : MonoBehaviour
    {
        [Header("Stub")]
        [SerializeField] private StubSO stubSO;

        [Header("Hierarchy Assigns")]
        [SerializeField] private CharacterController playerCharacterController;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform playerHead;
        [SerializeField] private PlayerView playerView;
        [SerializeField] private Selects.Views.FadeImageView fadeImageView;

        // インターフェースのフィールド
        private IPlayerTeleporterStrategySO playerTeleporterStrategySO;
        private Player _player;
        private UserBean _userBean;
        private PlayerTeleporterStrategySOsLink _playerTeleporterStrategySOsLink;

        private void Start()
        {
            _player = ReInput.players.GetPlayer(0);
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            _userBean = userBean;
            var testParams = stubSO.views.startDirectionSequencerTest;
            var sequencer = testParams.startDirectionSequencer;
            var set = GetSettingsField(sequencer);
            _playerTeleporterStrategySOsLink = set.playerTeleporterStrategySOsLink;
        }

        private void OnGUI()
        {
            int y = 10;
            const int buttonWidth = 500;
            const int buttonHeight = 50;
            const int spacing = 60;

            if (stubSO == null)
            {
                GUI.Label(new Rect(10, y, buttonWidth, buttonHeight), "StubSO is not assigned.");
                return;
            }

            var testParams = stubSO.views.playerTeleporterStrategySOTest;
            var sequencer = testParams.startDirectionSequencer;

            if (sequencer == null)
            {
                GUI.Label(new Rect(10, y, buttonWidth, buttonHeight), "StartDirectionSequencer is not assigned in StubSO.");
                return;
            }

            // =========================================================
            // 移動Tween処理Async (PlayerTeleporterStrategySO)
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "移動Tween処理Async"))
            {
                // インターフェースのフィールドへセット
                playerTeleporterStrategySO = testParams.playerTeleporterStrategySO;

                Debug.Log("[テスト] 移動Tween処理Asyncの実行を開始します");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkTween);

                // TeleportPlayerを実行
                playerTeleporterStrategySO?.TeleportPlayer(
                    testParams.position,
                    testParams.angles,
                    testParams.isCompletedStartDirection,
                    playerCharacterController,
                    _player,
                    playerTransform,
                    playerHead,
                    playerView,
                    fadeImageView,
                    this.GetCancellationTokenOnDestroy()
                ).Forget();
            }
            y += spacing;

            // =========================================================
            // フェードTween+瞬間移動処理Async (PlayerTeleporterStrategySO1)
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "フェードTween+瞬間移動処理Async"))
            {
                // インターフェースのフィールドへセット
                playerTeleporterStrategySO = testParams.playerTeleporterStrategySO1;

                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkFadeAndTeleport);

                Debug.Log("[テスト] フェードTween+瞬間移動処理Asyncの実行を開始します");
                // TeleportPlayerを実行
                playerTeleporterStrategySO?.TeleportPlayer(
                    testParams.position,
                    testParams.angles,
                    testParams.isCompletedStartDirection,
                    playerCharacterController,
                    _player,
                    playerTransform,
                    playerHead,
                    playerView,
                    fadeImageView,
                    this.GetCancellationTokenOnDestroy()
                ).Forget();
            }
            y += spacing;

            // =========================================================
            // 瞬間移動処理 (PlayerTeleporterStrategySO2)
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "瞬間移動処理"))
            {
                // インターフェースのフィールドへセット
                playerTeleporterStrategySO = testParams.playerTeleporterStrategySO2;

                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkTeleport);

                Debug.Log("[テスト] 瞬間移動処理の実行を開始します");
                // TeleportPlayerを実行
                playerTeleporterStrategySO?.TeleportPlayer(
                    testParams.position,
                    testParams.angles,
                    testParams.isCompletedStartDirection,
                    playerCharacterController,
                    _player,
                    playerTransform,
                    playerHead,
                    playerView,
                    fadeImageView,
                    this.GetCancellationTokenOnDestroy()
                ).Forget();
            }
        }

        private void OnDestroy()
        {
            var testParams = stubSO.views.startDirectionSequencerTest;
            var sequencer = testParams.startDirectionSequencer;
            sequencer.Dispose();
            ResourcesUtility utility = new ResourcesUtility();
            utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, _userBean);
            SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", _playerTeleporterStrategySOsLink);
        }

        /// <summary>
        /// イベント進捗配列の初期化
        /// </summary>
        private System.Collections.Generic.List<EventProgress> InitializeEventProgressList()
        {
            var eventProgressList = new System.Collections.Generic.List<EventProgress>();
            foreach (TutorialEventId eventId in System.Enum.GetValues(typeof(TutorialEventId)))
            {
                if (eventId != TutorialEventId.None)
                {
                    eventProgressList.Add(new EventProgress((int)eventId, 0));
                }
            }

            return eventProgressList;
        }

        private void SetSettingsField(StartDirectionSequencer sequencer, string fieldName, object value)
        {
            var settings = GetPrivateField<StartDirectionSettings>(sequencer, "settings");
            if (settings != null)
            {
                var field = typeof(StartDirectionSettings).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                field?.SetValue(settings, value);
            }
        }

        private StartDirectionSettings GetSettingsField(StartDirectionSequencer sequencer)
        {
            var settings = GetPrivateField<StartDirectionSettings>(sequencer, "settings");
            if (settings != null)
            {
                return settings;
            }

            return null;
        }

        private T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field?.GetValue(target);
        }
    }
}
