using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// コングラチュレーションのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "CongratulationsPanelViewModel", menuName = "Scriptable Objects/CongratulationsPanelViewModel")]
    public class CongratulationsPanelViewModel : ScriptableObject
    {
        /// <summary>コングラチュレーションのアニメーション終了時間</summary>
        [SerializeField] private float[] congratulationsTextDurations;
        /// <summary>コングラチュレーションのアニメーション終了時間</summary>
        public float[] CongratulationsTextDurations => congratulationsTextDurations;
        /// <summary>コングラチュレーションのテキストの移動先</summary>
        [SerializeField] private Vector3 congratulationsTextToPosition = new Vector3(-205f, 430f, 0f);
        /// <summary>コングラチュレーションのテキストの移動先</summary>
        public Vector3 CongratulationsTextToPosition => congratulationsTextToPosition;
        /// <summary>エンディングCGパネルのアニメーション終了時間</summary>
        [SerializeField] private float endingCGPanelDuration;
        /// <summary>エンディングCGパネルのアニメーション終了時間</summary>
        public float EndingCGPanelDuration => endingCGPanelDuration;
    }
}
