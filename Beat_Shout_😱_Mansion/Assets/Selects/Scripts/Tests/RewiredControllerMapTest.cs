using R3;
using Rewired;
using Selects.Views;
using UnityEngine;

namespace Selects.Tests
{
    /// <summary>
    /// RewiredのControllerMap制御テスト
    /// </summary>
    public class RewiredControllerMapTest : MonoBehaviour
    {
        [SerializeField] private StubSO stubSO;
        [SerializeField] private float moveVertical;
        [SerializeField] private float moveHorizontal;
        [SerializeField] private float aimMoveHorizontal;
        [SerializeField] private float aimMoveVertical;
        [SerializeField] private bool searchButtonDown;
        [SerializeField] private bool switchPartButtonDown;
        [SerializeField] private bool inhaleHeldButtonDown;
        [SerializeField] private bool inhaleHeldConButtonDown;
        [SerializeField] private bool tapLightButtonDown;
        private RewiredTutorialInput _rewiredTutorialInput;
        private Player _player;
        DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            var rewiredPlayer = ReInput.players.GetPlayer(0);
            _player = ReInput.players.GetPlayer(0);
            _rewiredTutorialInput = new RewiredTutorialInput(rewiredPlayer);
            //Observable.EveryUpdate()
            //    .Subscribe(_ =>
            //    {
            //        searchButtonDown = _player.GetButtonDown("SwitchPart");
            //    })
            //    .AddTo(ref _disposableBag);
        }

        private void Update()
        {
            var input = _rewiredTutorialInput;
            moveVertical = input.MoveVertical;
            moveHorizontal = input.MoveHorizontal;
            aimMoveHorizontal = input.AimMoveHorizontal;
            aimMoveVertical = input.AimMoveVertical;
            //var rewiredPlayer = ReInput.players.GetPlayer(0);
            //if (!searchButtonDown) searchButtonDown = _player.GetButtonDown("Search");
            if (!searchButtonDown) searchButtonDown = input.SearchButtonDown;
            if (!switchPartButtonDown) switchPartButtonDown = input.SwitchPartButtonDown;
            if (!inhaleHeldButtonDown) inhaleHeldButtonDown = input.InhaleHeldButtonDown;
            if (!inhaleHeldConButtonDown) inhaleHeldConButtonDown = input.InhaleHeldConButtonDown;
            if (!tapLightButtonDown) tapLightButtonDown = input.TapLightButtonDown;
        }

        private void OnGUI()
        {
            var input = _rewiredTutorialInput;
            var rewiredControllerMapTest = stubSO.commons.rewiredControllerMapTest;
            int y = 10;
            const int buttonWidth = 500;
            const int buttonHeight = 50;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "マップの更新"))
            {
                //var rewiredPlayer = ReInput.players.GetPlayer(0);
                //rewiredPlayer.controllers.maps.SetMapsEnabled(false, "Default"); // ゲーム操作を無効化
                //rewiredPlayer.controllers.maps.SetMapsEnabled(true, rewiredControllerMapTest.categoryName); // ゲーム操作を無効化
                var categoryName = rewiredControllerMapTest.categoryName;
                input.EnableOnlyControllerMapCategory(categoryName);
                Debug.Log($"マップを [{categoryName}] へ更新しました");
            }
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
