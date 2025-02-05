using UnityEngine;
using Rewired;
using R3;
using Mains.Commons;
using Mains.ViewModels;

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
        [SerializeField] private float 移動速度;
        [SerializeField] private float 視点速度補正;
        [SerializeField] private float 重力 = 9.81f;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        [SerializeField] private InteractionPartTable 探索_シャウトチャンス_リズムパート情報管理テーブル;

        private void Reset()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            // Rewiredによる入力管理
            // Assets/Universal/Scripts/Prefabs/Rewired Input Manager.prefab
            // プレイヤーへキーボードやコントローラー操作を割り当てる場合は
            // 当該プレハブから実施すること（シーンからの変更は適用されない）
            var player = ReInput.players.GetPlayer(0);
            var trans = transform;
            float currentYaw = trans.rotation.eulerAngles.y; // 現在のY軸回転角度 (左右回転)
            float currentPitch = trans.rotation.eulerAngles.x; // 現在のX軸回転角度 (上下回転)
            Vector3 velocity = Vector3.zero; // 現在の速度
            PlayerViewModel playerViewModel = new(探索_シャウトチャンス_リズムパート情報管理テーブル);
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    // プレイヤーの移動入力
                    float moveX = player.GetAxis("MoveHorizontal");
                    float moveZ = player.GetAxis("MoveVertical");
                    Vector3 move = Quaternion.Euler(0f, currentYaw, 0f) * new Vector3(moveX, 0, moveZ);
                    // 重力処理
                    if (!characterController.isGrounded)
                    {
                        velocity.y -= 重力 * Time.deltaTime;
                    }
                    else
                    {
                        velocity.y = 0; // 地面にいる場合、Y方向の速度をリセット
                    }
                    // 視点移動入力
                    float aimX = player.GetAxis("AimMoveHorizontal");
                    float aimY = player.GetAxis("AimMoveVertical");

                    characterController.Move((move * 移動速度 + velocity) * Time.deltaTime);
                    // 視点変更 (角度を直接加算)
                    currentYaw += aimX * 視点速度補正 * Time.deltaTime;
                    currentPitch -= aimY * 視点速度補正 * Time.deltaTime;

                    // ピッチ角度を制限 (-90度～90度)
                    currentPitch = Mathf.Clamp(currentPitch, -90f, 90f);

                    // 回転を適用
                    transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

                    bool isSwitchPart = player.GetButtonDown("SwitchPart");
                    playerViewModel.SetIsSwitchPart(isSwitchPart);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
