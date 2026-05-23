using R3;
using Selects.ViewModels;
using Selects.Views;
using System.Threading;
using UnityEngine;

namespace Selects.Tests
{
    /// <summary>
    /// TutorialPanelViewのテスト
    /// </summary>
    /// <remarks>
    /// OnGUIボタンによる目視確認および、ViewModelプロパティの監視テスト<br/>
    /// パラメータは<see cref="StubSO"/>から取得
    /// </remarks>
    public class TutorialPanelViewTest : MonoBehaviour
    {
        [SerializeField] private StubSO stubSO;
        [SerializeField] private TutorialPanelView view;
        [SerializeField] private TutorialPanelViewModel viewModel;
        
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            if (viewModel != null)
            {
                // FlashLightTriggerStayの監視
                viewModel.FlashLightTriggerStay.Subscribe(x =>
                {
                    Debug.Log($"[監視] FlashLightTriggerStay が {x} に更新されました。");
                }).AddTo(this);

                // BatteryHitPlayerAimの監視
                viewModel.BatteryHitPlayerAim.Subscribe(x =>
                {
                    Debug.Log($"[監視] BatteryHitPlayerAim が {x} に更新されました。");
                }).AddTo(this);

                // MissGhostEscapeNormalHitPlayerAimの監視
                viewModel.MissGhostEscapeNormalHitPlayerAim.Subscribe(x =>
                {
                    Debug.Log($"[監視] MissGhostEscapeNormalHitPlayerAim が {x} に更新されました。");
                }).AddTo(this);

                // LightRing2TriggerStayの監視
                viewModel.LightRing2TriggerStay.Subscribe(x =>
                {
                    Debug.Log($"[監視] LightRing2TriggerStay が {x} に更新されました。");
                }).AddTo(this);

                // VaseAndDeskGroupHitPlayerAimの監視
                viewModel.VaseAndDeskGroupHitPlayerAim.Subscribe(x =>
                {
                    Debug.Log($"[監視] VaseAndDeskGroupHitPlayerAim が {x} に更新されました。");
                }).AddTo(this);

                // RightStairsTrigger1FStayの監視
                viewModel.RightStairsTrigger1FStay.Subscribe(x =>
                {
                    Debug.Log($"[監視] RightStairsTrigger1FStay が {x} に更新されました。");
                }).AddTo(this);

                // LeftStairsTrigger2FStayの監視
                viewModel.LeftStairsTrigger2FStay.Subscribe(x =>
                {
                    Debug.Log($"[監視] LeftStairsTrigger2FStay が {x} に更新されました。");
                }).AddTo(this);
            }
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        private void OnGUI()
        {
            Rect buttonRect1 = new Rect(10, 10, 400, 50);
            if (GUI.Button(buttonRect1, "ApplyMessageの確認"))
            {
                var stub = stubSO.commons.tutorialPanelViewTest._ApplyMessageの確認;
                // MSG0001などを想定
                view.ApplyMessage(stub.messageId);
                Debug.Log($"[テスト] ApplyMessage({stub.messageId}) を実行しました。目視で確認してください。");
            }

            Rect buttonRect2 = new Rect(10, 70, 400, 50);
            if (GUI.Button(buttonRect2, "ApplyMessageWithProgressの確認"))
            {
                var stub = stubSO.commons.tutorialPanelViewTest._ApplyMessageWithProgressの確認;
                // MSG0002などを想定
                view.ApplyMessageWithProgress(stub.messageId, stub.current, stub.total);
                Debug.Log($"[テスト] ApplyMessageWithProgress({stub.messageId}, {stub.current}, {stub.total}) を実行しました。目視で確認してください。");
            }

            Rect buttonRect3 = new Rect(10, 130, 400, 50);
            if (GUI.Button(buttonRect3, "ResetMessagesの確認"))
            {
                view.ResetMessages();
                Debug.Log($"[テスト] ResetMessages() を実行しました。目視で確認してください。");
            }

            Rect buttonRect4 = new Rect(10, 190, 400, 50);
            if (GUI.Button(buttonRect4, "FadeInAsyncの確認"))
            {
                var stub = stubSO.commons.tutorialPanelViewTest._FadeInAsyncの確認;
                _ = view.FadeInAsync(stub.duration, _cts.Token);
                Debug.Log($"[テスト] FadeInAsync({stub.duration}) を実行しました。目視で確認してください。");
            }

            Rect buttonRect5 = new Rect(10, 250, 400, 50);
            if (GUI.Button(buttonRect5, "FadeOutAsyncの確認"))
            {
                var stub = stubSO.commons.tutorialPanelViewTest._FadeOutAsyncの確認;
                _ = view.FadeOutAsync(stub.duration, _cts.Token);
                Debug.Log($"[テスト] FadeOutAsync({stub.duration}) を実行しました。目視で確認してください。");
            }
        }
    }
}
