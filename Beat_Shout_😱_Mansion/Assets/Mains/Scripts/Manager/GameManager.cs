using Mains.Manager.Owners;
using UnityEngine;

namespace Mains.Manager
{
    /// <summary>
    /// ゲームマネージャー
    /// </summary>
    [RequireComponent(typeof(LevelOwner))]
    public class GameManager : MonoBehaviour
    {
        /// <summary>ゲームマネージャー</summary>
        private static GameManager _instance;
        /// <summary>ゲームマネージャー</summary>
        public static GameManager Instance => _instance;
        /// <summary>レベルオーナー</summary>
        [SerializeField] private LevelOwner levelOwner;
        /// <summary>レベルオーナー</summary>
        public LevelOwner LevelOwner => levelOwner;

        private void Reset()
        {
            if (levelOwner == null)
                levelOwner = GetComponent<LevelOwner>();
        }

        private void Start()
        {
            if (_instance == null)
                _instance = this;
        }
    }
}
