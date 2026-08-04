using Cysharp.Threading.Tasks;
using Mains.Views;
using Rewired;
using System.Threading;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// 移動Tween処理Async。
    /// DOMove + DORotate のシークエンスTweenでプレイヤーを移動させる。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerTeleporterStrategySO", menuName = "Scriptable Objects/PlayerTeleporterStrategySO")]
    public class PlayerTeleporterStrategySO : PlayerTeleporterAbstractStrategySO
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
                    (characterControllerEnabled, characterController, playerEnabled, player) => SetPlayerController(characterControllerEnabled, playerCharacterController, playerEnabled, player),
                    false,
                    playerCharacterController,
                    0,
                    player,
                    token
                ).Forget();
            }

            sequencer.SetDoTweenMoveDelegate(
                (position, angles, playerTransform, playerHeadTransform, durations, playerView) => TeleportPlayerAnimation(position, angles, playerTransform, playerHeadTransform, durations, playerView),
                position,
                angles,
                playerTransform,
                playerHead,
                new float[] { 1f, 1f },
                playerView,
                token
            ).Forget();
            if (isCompletedStartDirection)
            {
                sequencer.SetDoPostProcessDelegate(
                    (characterControllerEnabled, characterController, playerEnabled, player) => SetPlayerController(characterControllerEnabled, playerCharacterController, playerEnabled, player),
                    true,
                    0,
                    token
                ).Forget();
            }

            return UniTask.CompletedTask;
        }
    }
}
