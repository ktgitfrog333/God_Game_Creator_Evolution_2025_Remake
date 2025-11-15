using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// オーディオの仮実装用スタブ
    /// </summary>
    [CreateAssetMenu(fileName = "AudioStubTable", menuName = "Scriptable Objects/AudioStubTable")]
    public class AudioStubTable : ScriptableObject
    {
        /// <summary>心拍_短</summary>
        public AudioClip heartbeatFast;
        /// <summary>心拍_長</summary>
        public AudioClip heartbeatSlow;
    }
}
