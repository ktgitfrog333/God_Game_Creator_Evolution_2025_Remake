using Rewired;

namespace Selects.Views
{
    /// <summary>
    /// ITutorialInput の Rewired 実装。
    /// Sequencer を Rewired から切り離すためのアダプター。
    /// </summary>
    public class RewiredTutorialInput : ITutorialInput
    {
        /// <summary>RewiredのPlayer</summary>
        private readonly Player _player;

        public RewiredTutorialInput(Player player)
        {
            _player = player;
        }

        public float MoveVertical => _player.GetAxis("MoveVertical");
        public float MoveHorizontal => _player.GetAxis("MoveHorizontal");
        public float AimMoveHorizontal => _player.GetAxis("AimMoveHorizontal");
        public float AimMoveVertical => _player.GetAxis("AimMoveVertical");
        public bool SearchButtonDown => _player.GetButtonDown("Search");
        public bool SwitchPartButtonDown => _player.GetButtonDown("SwitchPart");
        public bool InhaleHeldButtonDown => _player.GetButtonDown("Inhale");
        public bool InhaleHeldConButtonDown => (_player.GetButtonDown("InhaleHalfLeft") && _player.GetButton("InhaleHalfRight")) ||
            (_player.GetButton("InhaleHalfLeft") && _player.GetButtonDown("InhaleHalfRight"));
        public bool TapLightButtonDown => _player.GetButtonDown("TapLight");

        public void EnableOnlyControllerMapCategory(string categoryName)
        {
            _player.controllers.maps.SetAllMapsEnabled(false);
            if (!string.IsNullOrEmpty(categoryName))
                _player.controllers.maps.SetMapsEnabled(true, categoryName);
        }
    }
}
