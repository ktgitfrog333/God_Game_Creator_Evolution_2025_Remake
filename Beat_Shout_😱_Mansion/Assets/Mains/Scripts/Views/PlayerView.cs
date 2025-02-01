using UnityEngine;
using Rewired;
using R3;

namespace Mains.Views
{
    /// <summary>
    /// プレイヤーのビュー
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerView : MonoBehaviour
    {
        /// <summary>キャラクター移動制御</summary>
        [SerializeField] private CharacterController characterController;
        /// <summary>
        /// Rewiredによる入力管理
        /// </summary>
        /// <see cref="Assets/Universal/Scripts/Prefabs/Rewired Input Manager.prefab"/>
        /// <remarks>
        /// プレイヤーへキーボードやコントローラー操作を割り当てる場合は<br/>
        /// 当該プレハブから実施すること（シーンからの変更は適用されない）
        /// </remarks>
        private Player player;
        [SerializeField] private float 移動速度;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            player = ReInput.players.GetPlayer(0);
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    // プレイヤーの移動入力
                    float moveX = player.GetAxis("MoveHorizontal"); // 横方向入力 (A/D または ←/→)
                    float moveZ = player.GetAxis("MoveVertical");
                    Vector3 move = transform.right * moveX + transform.forward * moveZ;
                    characterController.Move(move * 移動速度 * Time.deltaTime);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
