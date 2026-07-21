using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// プレイヤー移動演出ストラテジーの設定を同期管理させる
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerTeleporterStrategySOsLink", menuName = "Scriptable Objects/PlayerTeleporterStrategySOsLink")]
    public class PlayerTeleporterStrategySOsLink : ScriptableObject
    {
        /// <summary>プレイヤー移動演出ストラテジー</summary>
        [SerializeField] private PlayerTeleporterAbstractStrategySO playerTeleporterStrategy;
        /// <summary>プレイヤー移動演出ストラテジー</summary>
        public PlayerTeleporterAbstractStrategySO PlayerTeleporterStrategy => playerTeleporterStrategy;
    }
}
