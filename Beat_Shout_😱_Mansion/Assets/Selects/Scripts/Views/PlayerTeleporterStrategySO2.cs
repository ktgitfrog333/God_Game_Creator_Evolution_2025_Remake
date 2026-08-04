using Cysharp.Threading.Tasks;
using Mains.Views;
using Rewired;
using System.Threading;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// 瞬間移動処理。
    /// 演出なしで即座にプレイヤーの位置・角度を変更する。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerTeleporterStrategySO2", menuName = "Scriptable Objects/PlayerTeleporterStrategySO2")]
    public class PlayerTeleporterStrategySO2 : PlayerTeleporterAbstractStrategySO
    {
        /// <inheritdoc/>
        public override UniTask TeleportPlayer(
            Vector3 position,
            Vector3 angles,
            bool isCompletedStartDirection,
            CharacterController playerCharacterController,
            Player player,
            Transform playerTransform,
            Transform playerHead,
            PlayerView playerView,
            FadeImageView fadeImageView,
            CancellationToken token)
        {
            playerHead.localEulerAngles = Vector3.zero;
            var sequencer = settins.startDirectionSequencer;
            if (isCompletedStartDirection)
            {
                sequencer.SetDoPreProcessDelegate(
                    (characterControllerEnabled, characterController, playerEnabled, player) => SetPlayerController(characterControllerEnabled, characterController, playerEnabled, player),
                    false,
                    playerCharacterController,
                    0,
                    player,
                    token
                ).Forget();
            }

            sequencer.SetDoTeleportDelegate(
                (position, angles, playerTransform, playerHeadTransform, playerView) => TeleportPlayer(position, angles, playerTransform, playerHeadTransform, playerView),
                position,
                angles,
                playerTransform,
                playerHead,
                playerView,
                token
            ).Forget();

            if (isCompletedStartDirection)
            {
                sequencer.SetDoPostProcessDelegate(
                    (characterControllerEnabled, characterController, playerEnabled, player) => SetPlayerController(characterControllerEnabled, characterController, playerEnabled, player),
                    true,
                    0,
                    token
                ).Forget();
            }

            return UniTask.CompletedTask;
        }
    }
}
