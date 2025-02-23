using UnityEngine;
using Rewired;
using R3;
using Mains.Commons;
using Mains.ViewModels;
using CriWare;
using System.Linq;
using Mains.External;

namespace Mains.Views
{
    /// <summary>
    /// プレイヤーのビュー
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    //[RequireComponent(typeof(Animator))]
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
        ///// <summary>歩行SE</summary>
        //private CriAtomSource _criAtomSourceFootStep;
        ///// <summary>SEのトリガー制御</summary>
        //[SerializeField] private Animator animator;
        [SerializeField] private float 連続入力許容時間_秒;
        
        private void Reset()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            //if (animator == null)
            //    animator = GetComponent<Animator>();
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
            //_criAtomSourceFootStep = GetComponentsInChildren<CriAtomSource>().FirstOrDefault(q => q.cueSheet.Equals("CueSheet_SFX") &&
            //q.cueName.Equals("FootStep"));
            Script_xyloApi script_XyloApi = new();
            ReactiveCommand<float> moveAcceleration = new ReactiveCommand<float>();
            moveAcceleration.Pairwise()
                // プラスからマイナス（0へ近づいた）になった場合にSFXをStop
                //.Where(x => x.Current < x.Previous)
                .Subscribe(x =>
                {
                    Debug.Log($"前: [{x.Current}]後: [{x.Previous}]");
                    // 連打すると足踏みしてしまう挙動をなんとかする
                    if (0f < x.Current)
                    {
                        // 足音を鳴らす
                        script_XyloApi.StartFootsteps();
                    }
                    else
                    {
                        // 足音を止める
                        script_XyloApi.StopFootsteps();
                    }
                })
                .AddTo(ref _disposableBag);

            ReactiveProperty<bool> isCoolTimeMode = new();
            ReactiveCommand<int> moveState = new ReactiveCommand<int>();
            moveState.Subscribe(x =>
            {
                Debug.Log($"moveState: [{x}]");
                if (!isCoolTimeMode.Value)
                {
                    isCoolTimeMode.Value = true;
                    Debug.Log($"isCoolTimeMode0: [{isCoolTimeMode.Value}]");
                }
            })
            .AddTo(ref _disposableBag);
            float moveInputTimeSec = 0f;
            Vector3 moveLasted = Vector3.zero;
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    Debug.Log($"isCoolTimeMode1: [{isCoolTimeMode.Value}]");
                    if (isCoolTimeMode.Value)
                    {
                        if (moveInputTimeSec <= 連続入力許容時間_秒)
                        {
                            moveInputTimeSec += Time.deltaTime;
                        }
                        else
                        {
                            moveInputTimeSec = 0f;
                            isCoolTimeMode.Value = false;
                            Debug.Log($"isCoolTimeMode2: [{isCoolTimeMode.Value}]");
                        }
                    }
                    // プレイヤーの移動入力
                    float moveX = player.GetAxis("MoveHorizontal");
                    float moveZ = player.GetAxis("MoveVertical");
                    moveState.Execute(0f < new Vector2(moveX, moveZ).sqrMagnitude ? 1 : 0);
                    // 連続入力を防止
                    Vector3 move = Quaternion.Euler(0f, currentYaw, 0f) * new Vector3(moveX, 0, moveZ);
                    //if (0f < new Vector2(moveX, moveZ).sqrMagnitude)
                    //{
                    //    moveInputTimeSec += Time.deltaTime;
                    //}
                    //else
                    //{
                    //    moveInputTimeSec = 0f;
                    //}
                    //Debug.Log($"moveInputTimeSec:[{moveInputTimeSec}]");
                    // 重力処理
                    if (!characterController.isGrounded)
                    {
                        velocity.y -= 重力 * Time.deltaTime;
                    }
                    else
                    {
                        velocity.y = 0; // 地面にいる場合、Y方向の速度をリセット
                    }
                    //if (moveInputTimeSec <= 連続入力許容時間_秒)
                    //{
                    //    move = Quaternion.Euler(0f, currentYaw, 0f) * new Vector3(0f, 0f, 0f);
                    //}
                    if (!isCoolTimeMode.Value)
                    {
                        Debug.Log("movepattern 0");
                        moveLasted = move * 移動速度;
                        characterController.Move((moveLasted + velocity) * Time.deltaTime);
                        moveAcceleration.Execute(new Vector2(moveX, moveZ).sqrMagnitude);
                    }
                    else
                    {
                        Debug.Log("movepattern 1");
                        characterController.Move((moveLasted + velocity) * Time.deltaTime);
                    }
                    //animator.SetFloat("Walk", move.sqrMagnitude);
                    // 視点移動入力
                    float aimX = player.GetAxis("AimMoveHorizontal");
                    float aimY = player.GetAxis("AimMoveVertical");
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

        ///// <summary>
        ///// 足音を鳴らすSEを再生する
        ///// </summary>
        //public void PlayFootstepSound()
        //{
        //    if (_criAtomSourceFootStep != null)
        //    {
        //        _criAtomSourceFootStep.Play();
        //    }
        //}

        ///// <summary>
        ///// 足音を鳴らすSEを停止する
        ///// </summary>
        //public void StopFootstepSound()
        //{
        //    if (_criAtomSourceFootStep != null)
        //    {
        //        _criAtomSourceFootStep.Stop();
        //    }
        //}

        //public void PlayDummy() { }
    }
}
