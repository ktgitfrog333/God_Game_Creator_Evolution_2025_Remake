using UnityEngine;
using Rewired;

namespace Mains.Views
{
    /// <summary>
    /// コントローラー振動のビュー
    /// </summary>
    public class VibrationView : MonoBehaviour
    {
        /// <summary>モーターレベル1</summary>
        [SerializeField] private float motorLevel1;
        /// <summary>モーターレベル2</summary>
        [SerializeField] private float motorLevel2;
        /// <summary>終了時間</summary>
        [SerializeField] private float duration;
        /// <summary>振動を開始する最長距離</summary>
        [SerializeField] private float[] maxDistance;

        /// <summary>
        /// コントローラーの振動
        /// </summary>
        /// <param name="player">RewiredのPlayer</param>
        /// <param name="distance">距離</param>
        public void VibrateController(Player player, float distance)
        {
            // TODO: 一定距離に近づいたら振動させる
            if (player == null ||
                maxDistance[1] < distance) return;

            // 近いほど振動が強くなる（遠いと0、近いと1）
            float intensity = Mathf.Clamp01(1f - (distance / maxDistance[1]));
            foreach (var joystick in player.controllers.Joysticks)
            {
                if (joystick == null || !joystick.supportsVibration) continue;

                try
                {
                    if (distance <= maxDistance[0] &&
                        joystick.vibrationMotorCount > 0)
                        player.SetVibration(0, motorLevel1 * intensity, duration); // モーター0を振動

                    if (joystick.vibrationMotorCount > 1)
                        player.SetVibration(1, motorLevel2 * intensity, duration); // モーター1を振動
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"コントローラーの振動に失敗: {e.Message}");
                }
            }
        }
    }
}
