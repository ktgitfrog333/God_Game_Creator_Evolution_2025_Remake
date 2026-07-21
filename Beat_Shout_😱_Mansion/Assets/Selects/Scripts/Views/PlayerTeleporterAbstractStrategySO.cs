using Cysharp.Threading.Tasks;
using Rewired;
using System.Threading;
using UnityEngine;
using R3;
using DG.Tweening;
using Mains.Views;

namespace Selects.Views
{
    /// <summary>
    /// プレイヤー移動演出ストラテジーの抽象クラス
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerTeleporterAbstractStrategySO", menuName = "Scriptable Objects/PlayerTeleporterAbstractStrategySO")]
    public abstract class PlayerTeleporterAbstractStrategySO : ScriptableObject, IPlayerTeleporterStrategySO
    {
        /// <summary>プレイヤー移動演出ストラテジーの設定</summary>
        [SerializeField] protected PlayerTeleporterSettins settins;
        /// <summary>プレイヤー移動演出ストラテジータイプ</summary>
        public PlayerTeleporterStrategyType PlayerTeleporterStrategyType => settins.playerTeleporterStrategyType;

        public virtual UniTask TeleportPlayer(
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
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// プレイヤー制御
        /// </summary>
        /// <param name="characterControllerEnabled">有効／無効</param>
        /// <param name="characterController">キャラクターコントローラー</param>
        /// <param name="playerEnabled">Rewiredプレイヤーの有効／無効</param>
        /// <param name="player">Rewiredプレイヤー</param>
        protected virtual void SetPlayerController(bool characterControllerEnabled, CharacterController characterController, int playerEnabled, Player player)
        {
            characterController.enabled = characterControllerEnabled;
            switch (playerEnabled)
            {
                case 1:
                    player.controllers.maps.SetMapsEnabled(false, "Default");

                    break;
                case 2:
                    player.controllers.maps.SetMapsEnabled(true, "Default");

                    break;
            }
        }

        /// <summary>
        /// プレイヤーを座標位置へ瞬間移動
        /// </summary>
        /// <param name="position">移動先の位置</param>
        /// <param name="angles">移動先の角度</param>
        /// <param name="playerTransform">プレイヤーのTransform</param>
        /// <param name="playerHeadTransform">プレイヤー頭のTransform</param>
        /// <param name="playerView">プレイヤーのビュー</param>
        protected virtual void TeleportPlayer(Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform, PlayerView playerView)
        {
            playerTransform.position = position;
            playerTransform.eulerAngles = angles;
            playerHeadTransform.eulerAngles = angles;
            playerView.SetCurrentYaw(angles.y);
        }

        /// <summary>
        /// プレイヤーを座標位置へTween移動
        /// </summary>
        /// <param name="position">移動先の位置</param>
        /// <param name="angles">移動先の角度</param>
        /// <param name="playerTransform">プレイヤーのTransform</param>
        /// <param name="playerHeadTransform">プレイヤー頭のTransform</param>
        /// <param name="durations">移動と回転の時間配列</param>
        /// <param name="playerView">プレイヤーのビュー</param>
        /// <returns>オブザーバブル</returns>
        protected virtual Observable<Unit> TeleportPlayerAnimation(Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform,
            float[] durations, PlayerView playerView)
        {
            return Observable.Create<Unit>(observer =>
            {
                var seq = DOTween.Sequence()
                    .Append(playerTransform.DOMove(position, durations[0]))
                    .Join(playerTransform.DORotate(angles, durations[1]))
                    .Join(playerHeadTransform.DORotate(angles, durations[1]))
                    .AppendCallback(() =>
                    {
                        playerHeadTransform.eulerAngles = angles;
                        playerView.SetCurrentYaw(angles.y);
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    });

                return Disposable.Empty;
            });
        }
    }

    /// <summary>
    /// プレイヤー移動演出ストラテジーの設定
    /// </summary>
    [System.Serializable]
    public class PlayerTeleporterSettins
    {
        /// <summary>ステージ開始演出のシーケンサ</summary>
        public StartDirectionSequencer startDirectionSequencer;
        /// <summary>プレイヤー移動演出ストラテジータイプ</summary>
        public PlayerTeleporterStrategyType playerTeleporterStrategyType;
    }

    /// <summary>
    /// プレイヤー移動演出ストラテジータイプ
    /// </summary>
    public enum PlayerTeleporterStrategyType
    {
        /// <summary>なし</summary>
        None,
        /// <summary>トゥイーン</summary>
        Tween,
        /// <summary>フェードと瞬間移動</summary>
        FadeAndTeleport,
        /// <summary>瞬間移動</summary>
        Teleport,
    }
}
