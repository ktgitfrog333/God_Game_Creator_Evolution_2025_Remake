using Cysharp.Threading.Tasks;
using Mains.Views;
using R3;
using Rewired;
using System.Threading;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// フェードTween+瞬間移動処理Async。
    /// フェードインで暗転してから瞬間移動し、完了後にフェードアウトで復帰する。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerTeleporterStrategySO1", menuName = "Scriptable Objects/PlayerTeleporterStrategySO1")]
    public class PlayerTeleporterStrategySO1 : PlayerTeleporterAbstractStrategySO
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

            if (isCompletedStartDirection)
            {
                sequencer.SetDoPreHideEffectDelegate(
                    (duraction) => fadeImageView.PlayFadeInDirection(duraction),
                    token,
                    .25f
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
            if (!isCompletedStartDirection)
            {
                // ここの分岐は、FadeImageViewのStartで呼んでいるため実装不要
            }
            else
            {
                sequencer.SetDoPostHideEffectDelegate(
                    (duraction, andFromTweenMode) => fadeImageView.PlayFadeOutDirection(duraction, andFromTweenMode),
                    token,
                    .25f,
                    false
                ).Forget();
            }
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
