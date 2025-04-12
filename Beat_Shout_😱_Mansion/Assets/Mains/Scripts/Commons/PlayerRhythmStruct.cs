using UnityEngine;

namespace Mains.Commons
{
    [System.Serializable]
    /// <summary>
    /// リズムパートで使用するプレイヤープロパティの構造体
    /// </summary>
    public struct PlayerRhythmStruct
    {
        /// <summary>Player > Body > Elbow > Arm > FlashLight > Spot Light のLightコンポーネント</summary>
        public Light spotLightLight;
        /// <summary>Player > Body > Elbow > Arm > FlashLight > Spot Light のTransform</summary>
        public Transform spotLightLightTrans;
        /// <summary>Player > Body > Elbow 肘</summary>
        public Transform elbow;
    }
}
