using Mains.Manager.Owners;
using UnityEngine;

namespace Mains.Manager
{
    /// <summary>
    /// ゲームマネージャー
    /// </summary>
    [RequireComponent(typeof(LevelOwner))]
    [RequireComponent(typeof(UIOwner))]
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
        /// <summary>UIオーナー</summary>
        [SerializeField] private UIOwner uiOwner;
        /// <summary>UIオーナー</summary>
        public UIOwner UIOwner => uiOwner;

        private void Reset()
        {
            if (levelOwner == null)
                levelOwner = GetComponent<LevelOwner>();
            if (uiOwner == null)
                uiOwner = GetComponent<UIOwner>();
        }

        private void Start()
        {
            if (_instance == null)
                _instance = this;
        }
    }
}
