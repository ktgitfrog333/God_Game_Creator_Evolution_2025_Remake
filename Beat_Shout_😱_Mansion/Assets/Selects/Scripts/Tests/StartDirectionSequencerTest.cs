using Cysharp.Threading.Tasks;
using Mains.Views;
using R3;
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
    /// ステージ開始演出のシーケンサのテスト
    /// </summary>
    public class StartDirectionSequencerTest : MonoBehaviour
    {
        [Header("Stub")]
        [SerializeField] private StubSO stubSO;

        [Header("Hierarchy Assigns")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform _teleportPlayerTransform;
        [SerializeField] private Transform _teleportPlayerHeadTransform;
        [SerializeField] private PlayerView _teleportPlayerView;
        [SerializeField] private Transform _tweenPlayerTransform;
        [SerializeField] private Transform _tweenPlayerHeadTransform;
        [SerializeField] private PlayerView _tweenPlayerView;

        private Player _player;
        private UserBean _userBean;
        private string _targetSceneName;
        private PlayerTeleporterStrategySOsLink _playerTeleporterStrategySOsLink;

        private void Start()
        {
            if (ReInput.isReady)
            {
                _player = ReInput.players.GetPlayer(0);
            }
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            _userBean = userBean;
            var testParams = stubSO.views.startDirectionSequencerTest;
            var sequencer = testParams.startDirectionSequencer;
            var set = GetSettingsField(sequencer);
            _targetSceneName = set.targetSceneName;
            _playerTeleporterStrategySOsLink = set.playerTeleporterStrategySOsLink;
        }

        private void OnGUI()
        {
            int y = 10;
            const int buttonWidth = 600;
            const int buttonHeight = 35;
            const int spacing = 45;

            if (stubSO == null || stubSO.views.startDirectionSequencerTest == null)
            {
                GUI.Label(new Rect(10, y, buttonWidth, buttonHeight), "StubSO is not assigned.");
                return;
            }

            var testParams = stubSO.views.startDirectionSequencerTest;
            var sequencer = testParams.startDirectionSequencer;

            if (sequencer == null)
            {
                GUI.Label(new Rect(10, y, buttonWidth, buttonHeight), "StartDirectionSequencer is not assigned in StubSO.");
                return;
            }

            // =========================================================
            // StartDirectionMode ゲッターテスト
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "条件: targetSceneName=MainScene"))
            {
                ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "MainScene");
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                Debug.Log($"[結果] Mode: {sequencer.StartDirectionMode} (Expected: MAIN_SCENE)");
            }
            if (GUI.Button(new Rect(10 + buttonWidth + 10, y, buttonWidth, buttonHeight), "メインシーケンス"))
            {
                //ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "MainScene");
                SetAllDelegates(sequencer, testParams);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "条件1: SelectScene_Amagata, state=[2,2,2,2,1], sceneIdx=0"))
            {
                ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                var bean = new UserBean { state = new int[] { 2, 2, 2, 2, 1 }, sceneIdx = 0 };
                SetPrivateField(sequencer, "_userBean", bean);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                Debug.Log($"[結果1] Mode: {sequencer.StartDirectionMode} (Expected: SELECT_SCENE_AND_NORMAL_AND_FROM_SCENE_ROOMS)");
            }

            if (GUI.Button(new Rect(10 + buttonWidth + 10, y, buttonWidth, buttonHeight), "ステージ番号0～4シーケンス"))
            {
                //ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                var bean = new UserBean { state = new int[] { 2, 2, 2, 2, 1 }, sceneIdx = 0 };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                SetPrivateField(sequencer, "_isOnly", true);
                SetAllDelegates(sequencer, testParams);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "条件2: SelectScene_Amagata, state=[2,2,2,2,1], sceneIdx=5"))
            {
                ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                var bean = new UserBean { state = new int[] { 2, 2, 2, 2, 1 }, sceneIdx = 5 };
                SetPrivateField(sequencer, "_userBean", bean);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                Debug.Log($"[結果2] Mode: {sequencer.StartDirectionMode} (Expected: SELECT_SCENE_AND_NORMAL_AND_FROM_SCENE_TITLE)");
            }

            if (GUI.Button(new Rect(10 + buttonWidth + 10, y, buttonWidth, buttonHeight), "ステージ番号5シーケンス"))
            {
                //ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                var bean = new UserBean { state = new int[] { 2, 2, 2, 2, 1 }, sceneIdx = 5 };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                SetPrivateField(sequencer, "_isOnly", true);
                SetAllDelegates(sequencer, testParams);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "条件3: EMPTY (エラーログ確認)"))
            {
                ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                SetPrivateField(sequencer, "_userBean", bean);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", null);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                Debug.Log($"[結果3] Mode: {sequencer.StartDirectionMode} (Expected: EMPTY)");
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "条件4: Tween SO"))
            {
                ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                SetPrivateField(sequencer, "_userBean", bean);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkTween);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                Debug.Log($"[結果4] Mode: {sequencer.StartDirectionMode} (Expected: SELECT_SCENE_AND_TUTORIALS)");
            }

            if (GUI.Button(new Rect(10 + buttonWidth + 10, y, buttonWidth, buttonHeight), "チュートリアル①シーケンス"))
            {
                //ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                SetPrivateField(sequencer, "_isOnly", true);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkTween);
                SetAllDelegates(sequencer, testParams);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "条件5: FadeAndTeleport SO"))
            {
                ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                SetPrivateField(sequencer, "_userBean", bean);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkFadeAndTeleport);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                Debug.Log($"[結果5] Mode: {sequencer.StartDirectionMode} (Expected: SELECT_SCENE_AND_TUTORIALS_1)");
            }

            if (GUI.Button(new Rect(10 + buttonWidth + 10, y, buttonWidth, buttonHeight), "チュートリアル②シーケンス"))
            {
                //ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                SetPrivateField(sequencer, "_isOnly", true);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkFadeAndTeleport);
                SetAllDelegates(sequencer, testParams);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "条件6: Teleport SO"))
            {
                ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                SetPrivateField(sequencer, "_userBean", bean);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkTeleport);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                Debug.Log($"[結果6] Mode: {sequencer.StartDirectionMode} (Expected: SELECT_SCENE_AND_TUTORIALS_2)");
            }

            if (GUI.Button(new Rect(10 + buttonWidth + 10, y, buttonWidth, buttonHeight), "チュートリアル③シーケンス"))
            {
                //ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                // 基本操作のみ完了にする
                tmpEventProgressList[0].status = 1;
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                SetPrivateField(sequencer, "_isOnly", true);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkTeleport);
                SetAllDelegates(sequencer, testParams);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "条件7: 基本操作"))
            {
                ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                SetPrivateField(sequencer, "_userBean", bean);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkTeleport);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                Debug.Log($"[結果7] Mode: {sequencer.StartDirectionMode} (Expected: SELECT_SCENE_AND_TUTORIALS_3)");
            }

            if (GUI.Button(new Rect(10 + buttonWidth + 10, y, buttonWidth, buttonHeight), "チュートリアル_基本操作"))
            {
                //ResetSequencerState(sequencer);
                SetSettingsField(sequencer, "targetSceneName", "SelectScene_Amagata");
                System.Collections.Generic.List<EventProgress> tmpEventProgressList = InitializeEventProgressList();
                var bean = new UserBean { state = new int[] { 1, 0, 0, 0, 0 }, eventProgressList = tmpEventProgressList };
                ResourcesUtility utility = new ResourcesUtility();
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, bean);
                var mode = DoInitializeMode(sequencer);
                SetPrivateField(sequencer, "_startDirectionMode", mode);
                SetPrivateField(sequencer, "_isOnly", true);
                SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", testParams.playerTeleporterStrategySOsLinkTeleport);
                SetAllDelegates(sequencer, testParams);
            }
            y += spacing;

            // =========================================================
            // StepDictionary ゲッターテスト
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "StepDictionary 初期状態確認"))
            {
                ResetSequencerState(sequencer);
                var dic = sequencer.StepDictionary;
                Debug.Log($"[StepDictionary] PreProcess: {dic[StartDirectionStep.PreProcess]}");
                Debug.Log($"[StepDictionary] PreHideEffect: {dic[StartDirectionStep.PreHideEffect]}");
                Debug.Log($"[StepDictionary] Teleport: {dic[StartDirectionStep.Teleport]}");
                Debug.Log($"[StepDictionary] PostHideEffect: {dic[StartDirectionStep.PostHideEffect]}");
                Debug.Log($"[StepDictionary] TweenMove: {dic[StartDirectionStep.TweenMove]}");
                Debug.Log($"[StepDictionary] PostProcess: {dic[StartDirectionStep.PostProcess]}");
            }
            y += spacing;

            // =========================================================
            // デリゲート設定テスト
            // =========================================================
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "SetDoPreProcessDelegate 呼び出し確認"))
            {
                ResetSequencerState(sequencer);
                sequencer.SetDoPreProcessDelegate(
                    (a, b, c, d) => { Debug.Log("DoPreProcessDelegateモック処理"); },
                    testParams.preProcessCharacterControllerEnabled,
                    characterController,
                    testParams.preProcessPlayerEnabled,
                    _player,
                    this.GetCancellationTokenOnDestroy()
                ).Forget();

                Debug.Log($"[PreProcess] characterControllerEnabled: {GetPrivateField<bool>(sequencer, "_preProcessCharacterControllerEnabled")} == {testParams.preProcessCharacterControllerEnabled}");
                Debug.Log($"[PreProcess] characterController: {GetPrivateField<CharacterController>(sequencer, "_characterController") == characterController}");
                Debug.Log($"[PreProcess] playerEnabled: {GetPrivateField<int>(sequencer, "_preProcessPlayerEnabled")} == {testParams.preProcessPlayerEnabled}");
                Debug.Log($"[PreProcess] player: {GetPrivateField<Player>(sequencer, "_player") == _player}");
                Debug.Log($"[PreProcess] StepDictionary: {sequencer.StepDictionary[StartDirectionStep.PreProcess]} (Expected: 1)");

                // モック処理の呼び出し確認
                var del = GetPrivateField<StartDirectionSequencer.DoPreProcessDelegate>(sequencer, "_doPreProcessDelegate");
                del?.Invoke(testParams.preProcessCharacterControllerEnabled, characterController, testParams.preProcessPlayerEnabled, _player);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "SetDoPreHideEffectDelegate 呼び出し確認"))
            {
                ResetSequencerState(sequencer);
                sequencer.SetDoPreHideEffectDelegate(
                    (duration) => { Debug.Log("DoPreHideEffectDelegateモック処理"); return default; },
                    this.GetCancellationTokenOnDestroy(),
                    testParams.preHideEffectDuration
                ).Forget();

                Debug.Log($"[PreHideEffect] duration: {GetPrivateField<float>(sequencer, "_preHideEffectDuration")} == {testParams.preHideEffectDuration}");
                Debug.Log($"[PreHideEffect] StepDictionary: {sequencer.StepDictionary[StartDirectionStep.PreHideEffect]} (Expected: 1)");

                var del = GetPrivateField<StartDirectionSequencer.DoPreHideEffectDelegate>(sequencer, "_doPreHideEffectDelegate");
                del?.Invoke(testParams.preHideEffectDuration);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "SetDoTeleportDelegate 呼び出し確認"))
            {
                ResetSequencerState(sequencer);
                sequencer.SetDoTeleportDelegate(
                    (pos, ang, t, ht, pv) => { Debug.Log("DoTeleportDelegateモック処理"); },
                    testParams.teleportPosition,
                    testParams.teleportAngles,
                    _teleportPlayerTransform,
                    _teleportPlayerHeadTransform,
                    _teleportPlayerView,
                    this.GetCancellationTokenOnDestroy()
                ).Forget();

                Debug.Log($"[Teleport] position: {GetPrivateField<Vector3>(sequencer, "_teleportPosition")} == {testParams.teleportPosition}");
                Debug.Log($"[Teleport] angles: {GetPrivateField<Vector3>(sequencer, "_teleportAngles")} == {testParams.teleportAngles}");
                Debug.Log($"[Teleport] playerTransform: {GetPrivateField<Transform>(sequencer, "_teleportPlayerTransform") == _teleportPlayerTransform}");
                Debug.Log($"[Teleport] playerHeadTransform: {GetPrivateField<Transform>(sequencer, "_teleportPlayerHeadTransform") == _teleportPlayerHeadTransform}");
                Debug.Log($"[Teleport] playerView: {GetPrivateField<PlayerView>(sequencer, "_teleportPlayerView") == _teleportPlayerView}");
                Debug.Log($"[Teleport] StepDictionary: {sequencer.StepDictionary[StartDirectionStep.Teleport]} (Expected: 1)");

                var del = GetPrivateField<StartDirectionSequencer.DoTeleportDelegate>(sequencer, "_doTeleportDelegate");
                del?.Invoke(testParams.teleportPosition, testParams.teleportAngles, _teleportPlayerTransform, _teleportPlayerHeadTransform, _teleportPlayerView);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "SetDoPostHideEffectDelegate 呼び出し確認"))
            {
                ResetSequencerState(sequencer);
                sequencer.SetDoPostHideEffectDelegate(
                    (duration, andFrom) => { Debug.Log("DoPostHideEffectDelegateモック処理"); return default; },
                    this.GetCancellationTokenOnDestroy(),
                    testParams.postHideEffectDuration,
                    testParams.andFromTweenMode
                ).Forget();

                Debug.Log($"[PostHideEffect] duration: {GetPrivateField<float>(sequencer, "_postHideEffectDuration")} == {testParams.postHideEffectDuration}");
                Debug.Log($"[PostHideEffect] andFromTweenMode: {GetPrivateField<bool>(sequencer, "_andFromTweenMode")} == {testParams.andFromTweenMode}");
                Debug.Log($"[PostHideEffect] StepDictionary: {sequencer.StepDictionary[StartDirectionStep.PostHideEffect]} (Expected: 1)");

                var del = GetPrivateField<StartDirectionSequencer.DoPostHideEffectDelegate>(sequencer, "_doPostHideEffectDelegate");
                del?.Invoke(testParams.postHideEffectDuration, testParams.andFromTweenMode);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "SetDoTweenMoveDelegate 呼び出し確認"))
            {
                ResetSequencerState(sequencer);
                sequencer.SetDoTweenMoveDelegate(
                    (pos, ang, t, th, dur, pv) => { Debug.Log("DoTweenMoveDelegateモック処理"); return default; },
                    testParams.tweenMovePosition,
                    testParams.tweenMoveAngles,
                    _tweenPlayerTransform,
                    _tweenPlayerHeadTransform,
                    testParams.tweenMoveDurations,
                    _tweenPlayerView,
                    this.GetCancellationTokenOnDestroy()
                ).Forget();

                Debug.Log($"[TweenMove] position: {GetPrivateField<Vector3>(sequencer, "_tweenMovePosition")} == {testParams.tweenMovePosition}");
                Debug.Log($"[TweenMove] angles: {GetPrivateField<Vector3>(sequencer, "_tweenMoveAngles")} == {testParams.tweenMoveAngles}");
                Debug.Log($"[TweenMove] playerTransform: {GetPrivateField<Transform>(sequencer, "_tweenPlayerTransform") == _tweenPlayerTransform}");
                Debug.Log($"[TweenMove] playerHeadTransform: {GetPrivateField<Transform>(sequencer, "_tweenPlayerHeadTransform") == _tweenPlayerHeadTransform}");
                float[] actualTweenMoveDurations = GetPrivateField<float[]>(sequencer, "_tweenMoveDurations");
                float[] expectedTweenMoveDurations = testParams.tweenMoveDurations;
                Debug.Log($"[TweenMove] durations: {string.Join("/", actualTweenMoveDurations)} == {string.Join("/", expectedTweenMoveDurations)}");
                Debug.Log($"[TweenMove] playerView: {GetPrivateField<PlayerView>(sequencer, "_tweenPlayerView") == _tweenPlayerView}");
                Debug.Log($"[TweenMove] StepDictionary: {sequencer.StepDictionary[StartDirectionStep.TweenMove]} (Expected: 1)");

                var del = GetPrivateField<StartDirectionSequencer.DoTweenMoveDelegate>(sequencer, "_doTweenMoveDelegate");
                del?.Invoke(testParams.tweenMovePosition, testParams.tweenMoveAngles, _tweenPlayerTransform, _tweenPlayerHeadTransform, testParams.tweenMoveDurations, _tweenPlayerView);
            }
            y += spacing;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "SetDoPostProcessDelegate 呼び出し確認"))
            {
                ResetSequencerState(sequencer);
                sequencer.SetDoPostProcessDelegate(
                    (a, b, c, d) => { Debug.Log("DoPostProcessDelegateモック処理"); },
                    testParams.postProcessCharacterControllerEnabled,
                    testParams.postProcessPlayerEnabled,
                    this.GetCancellationTokenOnDestroy()
                ).Forget();

                Debug.Log($"[PostProcess] characterControllerEnabled: {GetPrivateField<bool>(sequencer, "_postProcessCharacterControllerEnabled")} == {testParams.postProcessCharacterControllerEnabled}");
                Debug.Log($"[PostProcess] playerEnabled: {GetPrivateField<int>(sequencer, "_postProcessPlayerEnabled")} == {testParams.postProcessPlayerEnabled}");
                Debug.Log($"[PostProcess] StepDictionary: {sequencer.StepDictionary[StartDirectionStep.PostProcess]} (Expected: 1)");

                var del = GetPrivateField<StartDirectionSequencer.DoPreProcessDelegate>(sequencer, "_doPostProcessDelegate");
                del?.Invoke(testParams.postProcessCharacterControllerEnabled, characterController, testParams.postProcessPlayerEnabled, _player);
            }
        }

        private void OnDestroy()
        {
            var testParams = stubSO.views.startDirectionSequencerTest;
            var sequencer = testParams.startDirectionSequencer;
            sequencer.Dispose();
            ResourcesUtility utility = new ResourcesUtility();
            utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, _userBean);
            SetSettingsField(sequencer, "targetSceneName", _targetSceneName);
            SetSettingsField(sequencer, "playerTeleporterStrategySOsLink", _playerTeleporterStrategySOsLink);
        }

        private void ResetSequencerState(StartDirectionSequencer sequencer)
        {
            SetPrivateField(sequencer, "_startDirectionMode", StartDirectionMode.EMPTY);
            SetPrivateField(sequencer, "_stepDictionary", null);
            SetPrivateField(sequencer, "_isRunning", false);
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

        private StartDirectionMode DoInitializeMode(StartDirectionSequencer sequencer)
        {
            var initializeMethod = typeof(StartDirectionSequencer).GetMethod("InitializeMode",
                BindingFlags.NonPublic | BindingFlags.Instance);

            StartDirectionMode mode = StartDirectionMode.EMPTY;
            if (initializeMethod != null)
            {
                var result = initializeMethod.Invoke(sequencer, null);
                mode = (StartDirectionMode)result;
                //Debug.Log("[DoInitializeMode] InitializeModeメソッドを実行しました");
            }
            else
            {
                Debug.LogError("[DoInitializeMode] InitializeModeメソッドが見つかりませんでした");
            }

            return mode;
        }

        private T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field?.GetValue(target);
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private void SetAllDelegates(StartDirectionSequencer sequencer, Selects.Tests.Views.StartDirectionSequencerTest testParams)
        {
            sequencer.SetDoPreProcessDelegate(
                (a, b, c, d) => { Debug.Log("DoPreProcessDelegateモック処理"); },
                testParams.preProcessCharacterControllerEnabled,
                characterController,
                testParams.preProcessPlayerEnabled,
                _player,
                this.GetCancellationTokenOnDestroy()
            ).Forget();

            sequencer.SetDoPreHideEffectDelegate(
                (duration) => DummyPreHideEffect(duration),
                this.GetCancellationTokenOnDestroy(),
                testParams.preHideEffectDuration
            ).Forget();

            sequencer.SetDoTeleportDelegate(
                (pos, ang, t, th, pv) => { Debug.Log("DoTeleportDelegateモック処理"); },
                testParams.teleportPosition,
                testParams.teleportAngles,
                _teleportPlayerTransform,
                _teleportPlayerHeadTransform,
                _teleportPlayerView,
                this.GetCancellationTokenOnDestroy()
            ).Forget();

            sequencer.SetDoPostHideEffectDelegate(
                (duration, andFrom) => DummyPostHideEffect(duration, andFrom),
                this.GetCancellationTokenOnDestroy(),
                testParams.postHideEffectDuration,
                testParams.andFromTweenMode
            ).Forget();

            sequencer.SetDoTweenMoveDelegate(
                (pos, ang, t, th, dur, pv) => DummyTweenMove(pos, ang, t, th, dur, pv),
                testParams.tweenMovePosition,
                testParams.tweenMoveAngles,
                _tweenPlayerTransform,
                _tweenPlayerHeadTransform,
                testParams.tweenMoveDurations,
                _tweenPlayerView,
                this.GetCancellationTokenOnDestroy()
            ).Forget();

            sequencer.SetDoPostProcessDelegate(
                (a, b, c, d) => { Debug.Log("DoPostProcessDelegateモック処理"); },
                testParams.postProcessCharacterControllerEnabled,
                testParams.postProcessPlayerEnabled,
                this.GetCancellationTokenOnDestroy()
            ).Forget();
        }

        private Observable<bool> DummyPreHideEffect(float duration = .5f)
        {
            return Observable.Create<bool>(observer =>
            {
                Debug.Log("DoPreHideEffectDelegateモック処理");
                observer.OnNext(true);
                observer.OnCompleted();

                return Disposable.Empty;
            });
        }

        private Observable<Unit> DummyTweenMove(Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform, float[] durations, PlayerView playerView)
        {
            return Observable.Create<Unit>(observer =>
            {
                Debug.Log("DoTweenMoveDelegateモック処理");
                observer.OnNext(Unit.Default);
                observer.OnCompleted();

                return Disposable.Empty;
            });
        }

        private Observable<bool> DummyPostHideEffect(float duration = .5f, bool andFromTweenMode = true)
        {
            return Observable.Create<bool>(observer =>
            {
                Debug.Log("DoPostHideEffectDelegateモック処理");
                observer.OnNext(true);
                observer.OnCompleted();

                return Disposable.Empty;
            });
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
    }
}
