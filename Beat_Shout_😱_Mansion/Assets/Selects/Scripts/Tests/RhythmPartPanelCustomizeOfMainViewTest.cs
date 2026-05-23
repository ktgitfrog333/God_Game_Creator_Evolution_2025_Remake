using Mains.Views;
using Selects.ViewModels;
using UnityEngine;
using R3;
using Mains.Models;

namespace Selects.Tests
{
    /// <summary>
    /// RhythmPartPanelCustomizeOfMainViewのテスト
    /// </summary>
    public class RhythmPartPanelCustomizeOfMainViewTest : MonoBehaviour
    {
        [SerializeField] private StubSO stubSO;
        [SerializeField] private RhythmPartPanelCustomizeOfMainViewModel viewModel;
        [SerializeField] private PlayerView playerView;
        [SerializeField] private FollowPlayerCameraView followPlayerCameraView;
        [SerializeField] private Mains.Models.PlayerModel playerModel;

        private DisposableBag _disposableBag = new DisposableBag();
        private bool _prevIsSelectedBattery;
        private Transform _prevSelectedMissGhostAttackTransform;

        private void Start()
        {
            Observable.EveryUpdate()
                .Select(_ => FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(model =>
                {
                    playerModel = model;
                    playerModel.TargetCrossPosition.Subscribe(x =>
                    {
                        //Debug.Log($"[テスト] TargetCrossPosition が変更されました: {x}");
                    }).AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        private void Update()
        {
            if (playerModel != null)
            {
                if (_prevIsSelectedBattery != playerModel.IsSelectedBattery)
                {
                    _prevIsSelectedBattery = playerModel.IsSelectedBattery;
                    Debug.Log($"[テスト] IsSelectedBattery が変更されました: {_prevIsSelectedBattery}");
                }

                if (_prevSelectedMissGhostAttackTransform != playerModel.SelectedMissGhostAttackTransform)
                {
                    _prevSelectedMissGhostAttackTransform = playerModel.SelectedMissGhostAttackTransform;
                    Debug.Log($"[テスト] SelectedMissGhostAttackTransform が変更されました: {(_prevSelectedMissGhostAttackTransform != null ? _prevSelectedMissGhostAttackTransform.name : "null")}");
                }
            }
        }

        private void OnGUI()
        {
            Rect buttonRect1 = new Rect(10, 10, 400, 50);
            if (GUI.Button(buttonRect1, "リズムパート状態"))
            {
                var stub = stubSO.commons.rhythmPartPanelCustomizeOfMainViewTest._リズムパート状態;

                // PlayerViewのcharacterController.enabled = false
                if (playerView != null)
                {
                    var field = typeof(PlayerView).GetField("characterController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var characterController = field.GetValue(playerView) as CharacterController;
                        if (characterController != null)
                        {
                            characterController.enabled = false;
                            Debug.Log("[テスト] PlayerView の characterController.enabled を false にセットしました。");
                        }
                    }
                }

                // FollowPlayerCameraViewのAsyncDeleteFollowAndLookAt
                if (followPlayerCameraView != null)
                {
                    StartCoroutine(followPlayerCameraView.AsyncDeleteFollowAndLookAt());
                    Debug.Log("[テスト] FollowPlayerCameraView の AsyncDeleteFollowAndLookAt を呼び出しました。");
                }

                // ViewModelのInteractionPartへセット
                if (viewModel != null)
                {
                    viewModel.InteractionPart.Value = stub.interactionPart;
                    Debug.Log($"[テスト] InteractionPart に {stub.interactionPart} をセットしました。");
                }

                Debug.Log("[テスト] centerPanel が有効になることを目視で確認してください。");
                Debug.Log("[テスト] マウス位置に応じて targetCrossImage が動くことを目視で確認してください。");
            }
        }
    }
}
