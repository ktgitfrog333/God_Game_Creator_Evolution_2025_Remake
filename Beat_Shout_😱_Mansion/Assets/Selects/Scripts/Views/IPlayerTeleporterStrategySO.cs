using Cysharp.Threading.Tasks;
using Mains.Views;
using Rewired;
using System.Threading;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// プレイヤー移動演出ストラテジーのインターフェース。
    /// ScriptableObject で実装し、Inspector からアタッチを切り替えるだけで演出を差し替え可能にする。
    /// </summary>
    public interface IPlayerTeleporterStrategySO
    {
        /// <summary>
        /// 汎用移動処理
        /// </summary>
        /// <param name="position">移動先の位置</param>
        /// <param name="angles">移動先の角度</param>
        /// <param name="isCompletedStartDirection">ステージ開始演出が完了したか</param>
        /// <param name="playerCharacterController">プレイヤーのキャラクターコントローラー</param>
        /// <param name="player">Rewiredプレイヤー</param>
        /// <param name="playerTransform">プレイヤーのTransform</param>
        /// <param name="playerHead">プレイヤーの頭のTransform</param>
        /// <param name="playerView">プレイヤーのビュー</param>
        /// <param name="fadeImageView">フェードイメージのビュー</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        UniTask TeleportPlayer(
            Vector3 position,
            Vector3 angles,
            bool isCompletedStartDirection,
            CharacterController playerCharacterController,
            Player player,
            Transform playerTransform,
            Transform playerHead,
            PlayerView playerView,
            FadeImageView fadeImageView,
            CancellationToken token
        );
    }
}
