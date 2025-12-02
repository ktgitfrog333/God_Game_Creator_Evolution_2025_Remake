using UnityEngine;
using Selects.Manager.Owners;

namespace Selects.Manager
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
        /// <summary>オーディオオーナー</summary>
        [SerializeField] private AudioOwner audioOwner;
        /// <summary>オーディオオーナー</summary>
        public AudioOwner AudioOwner => audioOwner;

        private void Reset()
        {
            if (levelOwner == null)
                levelOwner = GetComponent<LevelOwner>();
            if (uiOwner == null)
                uiOwner = GetComponent<UIOwner>();
            if (audioOwner == null)
                audioOwner = GetComponent<AudioOwner>();
        }

        private void Start()
        {
            if (_instance == null)
                _instance = this;
        }
    }
}
