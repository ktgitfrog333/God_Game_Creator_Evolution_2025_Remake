using UnityEngine;
using Rewired;
using R3;
using Mains.Commons;
using Mains.ViewModels;
using System.Linq;
using Mains.External;
using System.Collections.Generic;
using Mains.Manager;
using DG.Tweening;

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
        [SerializeField] private float トップ_移動速度;
        [SerializeField] private float 視点速度補正;
        [SerializeField] private float 重力 = 9.81f;
        [SerializeField] private InteractionPartTable 探索_シャウトチャンス_リズムパート情報管理テーブル;
        [SerializeField] private float ロー_切り替え時間_秒;
        [SerializeField] private float ロー_歩幅;
        [SerializeField] private float トップ_歩幅;
        [SerializeField] private PlayerShoutChanceTable シャウトチャンスパートの共通パラメータ管理用テーブル;
        /// <summary>プレイヤーのビューモデル</summary>
        private PlayerViewModel _playerViewModel;
        /// <summary>フェードイメージのビュー</summary>
        private FadeImageView _fadeImageView;
        [SerializeField] private PlayerRhythmStruct リズムパートで使用するプレイヤープロパティ;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            if (リズムパートで使用するプレイヤープロパティ.spotLightLight == null)
                リズムパートで使用するプレイヤープロパティ.spotLightLight = GetComponentInChildren<Light>();
            if (リズムパートで使用するプレイヤープロパティ.elbow == null)
                リズムパートで使用するプレイヤープロパティ.elbow = transform.GetChild(0).GetChild(0);
            if (リズムパートで使用するプレイヤープロパティ.spotLightLightTrans == null)
                リズムパートで使用するプレイヤープロパティ.spotLightLightTrans = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        }

        private void Start()
        {
            Script_xyloApi script_XyloApi = new();
            // 着地した瞬間も足音を鳴らす
            ReactiveProperty<bool> isGrounded = new();
            isGrounded.Pairwise()
                .Where(x => x.Previous != x.Current &&
                    x.Current)
                .Subscribe(_ => script_XyloApi.PlayFootStep())
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Select(_ => IsGrounded())
                .DistinctUntilChanged() // 連続で同じ値が来たら無視
                .Subscribe(grounded => isGrounded.Value = grounded)
                .AddTo(ref _disposableBag);
            // 正面移動かどうかのステータス管理
            ReactiveProperty<bool> isMovingForward = new ReactiveProperty<bool>();
            // 移動距離が一定の長さを超えた場合に足音を鳴らす
            ReactiveProperty<float> walkingDistance = new();
            walkingDistance.Where(x => (isMovingForward.Value ? トップ_歩幅 : ロー_歩幅) < x)
                .Subscribe(_ =>
                {
                    script_XyloApi.PlayFootStep();
                    walkingDistance.Value = 0f;
                })
                .AddTo(ref _disposableBag);
            // ステータスに応じて移動速度を変更する
            ReactiveCommand<int> moveState = new ReactiveCommand<int>();
            moveState.Pairwise()
                .Where(x => x.Previous != x.Current)
                .Subscribe(x =>
                {
                    switch (x.Current)
                    {
                        case 0:
                            walkingDistance.Value = 0f;

                            break;
                    }
                })
                .AddTo(ref _disposableBag);
            // 歩いている間（移動入力）の時間
            ReactiveProperty<float> walkingTime = new ReactiveProperty<float>();
            walkingTime.Subscribe(x =>
            {
                // 計測時間に応じてステータスを変更する
                if (ロー_切り替え時間_秒 < x)
                    // ロー
                    moveState.Execute(1);
                else
                    // 停止
                    moveState.Execute(0);
            })
            .AddTo(ref _disposableBag);
            // CharacterControllerからの位置情報を監視して移動距離を更新する
            float lastFixedTime = 0f;
            Vector3 previousPosition = Vector3.zero;
            Observable.EveryUpdate()
                .Where(_ => characterController.enabled &&
                    Time.time - lastFixedTime >= Time.fixedDeltaTime) // FixedUpdateと同じタイミング
                .Subscribe(_ =>
                {
                    lastFixedTime = Time.time; // 次の実行タイミングを記録
        
                    Vector3 currentPosition = characterController.transform.position;
                    float deltaDistance = (previousPosition - currentPosition).sqrMagnitude;
        
                    if (deltaDistance > 0.0001f && isGrounded.Value) // 微小な変化を無視
                    {
                        walkingDistance.Value += deltaDistance;
                    }
                    else
                    {
                        walkingDistance.Value = 0f;
                    }

                    previousPosition = currentPosition;
                })
                .AddTo(ref _disposableBag);
            var trans = transform;
            // Rewiredによる入力管理
            // Assets/Universal/Scripts/Prefabs/Rewired Input Manager.prefab
            // プレイヤーへキーボードやコントローラー操作を割り当てる場合は
            // 当該プレハブから実施すること（シーンからの変更は適用されない）
            var player = ReInput.players.GetPlayer(0);
            // 現在のY軸回転角度 (左右回転)
            float currentYaw = trans.rotation.eulerAngles.y;
            // 現在のX軸回転角度 (上下回転)
            float currentPitch = trans.rotation.eulerAngles.x;
            // ターゲットとなるポルターガイストビュー
            PoltergeistView poltergeistView = null;
            FollowPlayerCameraView followPlayerCameraView = null;
            Observable.EveryUpdate()
                .Select(_ => FindAnyObjectByType<FollowPlayerCameraView>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    followPlayerCameraView = x;
                })
                .AddTo(ref _disposableBag);
            // リズムパートの位置まで移動する
            Observable.EveryUpdate()
                .Select(_ => _playerViewModel.IsCompletedBurstGhosts)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Where(x => x)
                        .Subscribe(x =>
                        {
                            Observable.Create<bool>(observer =>
                            {
                                StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                                return Disposable.Empty;
                            })
                                .Subscribe(_ =>
                                {
                                    // [シャウト成功インタラクション] 3. プレイヤー移動させる処理
                                    if (poltergeistView != null)
                                    {
                                        characterController.enabled = false;
                                        trans.position = poltergeistView.RhythmPartPosition.position;
                                        trans.eulerAngles = poltergeistView.RhythmPartPosition.eulerAngles;
                                        currentYaw = trans.eulerAngles.y;
                                        characterController.enabled = true;
                                    }
                                    Observable.Create<bool>(observer =>
                                    {
                                        StartCoroutine(_fadeImageView.PlayFadeOutDirection(observer));
                                        return Disposable.Empty;
                                    })
                                        .Subscribe(_ =>
                                        {
                                            // [シャウト成功インタラクション] 4. 後処理
                                            _playerViewModel.SetIsCompletedBurstGhosts(false);
                                            poltergeistView = null;
                                            if (followPlayerCameraView != null)
                                                followPlayerCameraView.DeleteFollowAndLookAt();
                                        })
                                        .AddTo(ref _disposableBag);
                                })
                                .AddTo(ref _disposableBag);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // 重力管理用のVelocity
            Vector3 velocity = Vector3.zero;
            _playerViewModel = new(探索_シャウトチャンス_リズムパート情報管理テーブル);
            _playerViewModel.SetPlayerTransform(transform);
            // イントロが完了するまではプレイヤー操作禁止
            characterController.enabled = false;
            script_XyloApi.FrameRate
                .Where(x => 0f < x)
                .Subscribe(_ =>
                {
                    characterController.enabled = true;
                })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Select(_ => _playerViewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    System.IDisposable observablePlayerControllerDisposable = null;
                    System.IDisposable observableTake1PlayerControllerDisposable = null;
                    System.IDisposable observableLightControllerDisposable = null;
                    System.IDisposable observableJustBeatTickDisposable = null;
                    System.IDisposable observableTargetCrossPositionDisposable = null;
                    x.Pairwise()
                        .Do(x => Debug.Log($"part_prev: [{x.Previous}]_part_curr: [{x.Current}]"))
                        .Subscribe(part =>
                        {
                            // None⇒探索（1. 探索、シャウト用の操作）
                            // リズム⇒探索（1. 探索、シャウト用の操作）
                            bool isNormalCtrl = part.Previous.Equals(InteractionPart.None) &&
                                    part.Current.Equals(InteractionPart.Search) ||
                                part.Previous.Equals(InteractionPart.Rhythm) &&
                                    part.Current.Equals(InteractionPart.Search);
                            // シャウトチャンス⇒リズム（2. リズムパート用の操作）
                            bool isRhythmCtrl = part.Previous.Equals(InteractionPart.ShoutChance) &&
                                part.Current.Equals(InteractionPart.Rhythm);
                            // 上記以外 探索⇔シャウトチャンス（変更無し）

                            if (isNormalCtrl &&
                                !isRhythmCtrl)
                            {
                                // 2. リズムパート用の操作の監視を破棄
                                observableTake1PlayerControllerDisposable?.Dispose();
                                observableLightControllerDisposable?.Dispose();
                                observableJustBeatTickDisposable?.Dispose();
                                observableTargetCrossPositionDisposable?.Dispose();
                                // 1. 探索、シャウト用の操作
                                observablePlayerControllerDisposable = Observable.EveryUpdate()
                                    .Where(_ => characterController.enabled)
                                    .Subscribe(_ =>
                                    {
                                        // プレイヤーの移動入力
                                        float moveX = player.GetAxis("MoveHorizontal");
                                        float moveZ = player.GetAxis("MoveVertical");
                                        if (0f < Mathf.Abs(moveX) ||
                                            0f < Mathf.Abs(moveZ))
                                        {
                                            walkingTime.Value += Time.deltaTime;
                                        }
                                        else
                                        {
                                            walkingTime.Value = 0f;
                                        }
                                        // 移動方向のベクトル
                                        Vector3 moveInput = new Vector3(moveX, 0, moveZ).normalized;
                                        Vector3 moveDirection = Quaternion.Euler(0f, currentYaw, 0f) * moveInput;

                                        // カメラの正面方向
                                        Vector3 cameraForward = Camera.main.transform.forward;
                                        cameraForward.y = 0; // 水平成分のみ使用
                                        cameraForward.Normalize();

                                        // 進行方向とカメラの向きの角度を求める
                                        float angle = Vector3.Angle(cameraForward, moveDirection);
                                        // 正面移動判定（30度以内なら正面移動）
                                        isMovingForward.Value = (angle < 130f);
                                        // 角度に応じた移動速度の補正値（0°のとき1倍、180°のとき0.5倍など）
                                        float speedMultiplier = Mathf.Lerp(1f, 0.5f, angle / 180f);

                                        // 移動速度に補正をかける
                                        float adjustedMoveSpeed = トップ_移動速度 * speedMultiplier;

                                        // 実際の移動ベクトル
                                        Vector3 move = moveDirection * adjustedMoveSpeed;
                                        // 重力処理
                                        if (!isGrounded.Value)
                                        {
                                            velocity.y -= 重力 * Time.deltaTime;
                                        }
                                        else
                                        {
                                            velocity.y = 0; // 地面にいる場合、Y方向の速度をリセット
                                        }
                                        characterController.Move((move + velocity) * Time.deltaTime);
                                        // 視点移動入力
                                        float aimX = player.GetAxis("AimMoveHorizontal");
                                        float aimY = player.GetAxis("AimMoveVertical");
                                        // 視点変更 (角度を直接加算)
                                        currentYaw += aimX * 視点速度補正 * Time.deltaTime;
                                        currentPitch -= aimY * 視点速度補正 * Time.deltaTime;

                                        // ピッチ角度を制限 (-90度～90度)
                                        // TODO: 外的要因（シャウト成功による自動移動等）で取得角度がエッジケースに該当することがある
                                        //       currentPitchが0⇒359.3694⇒90（※Mathf.Clampの補間）⇒唐突にプレイヤーが土下座する
                                        currentPitch = Mathf.Clamp(currentPitch, -90f, 90f);

                                        // 回転を適用
                                        trans.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

                                        bool isSwitchPart = player.GetButtonDown("SwitchPart");
                                        _playerViewModel.SetIsSwitchPart(isSwitchPart);
                                    })
                                    .AddTo(ref _disposableBag);
                            }
                            else if (isRhythmCtrl)
                            {
                                // 1. 探索、シャウト用の操作の監視を破棄
                                observablePlayerControllerDisposable?.Dispose();
                                // 2. リズムパート用の操作
                                //  X軸、Y軸（重力）、Z軸の位置移動は不可。
                                //  コントローラーの場合は右スティック操作が左スティック操作に変わる。カメラの位置、角度の自動追尾が不可となり、固定となる。
                                observableTake1PlayerControllerDisposable = Observable.EveryUpdate()
                                    .Select(_ => _playerViewModel.TargetCrossPosition)
                                    .Where(x => x != null)
                                    .Take(1)
                                    .Subscribe(x =>
                                    {
                                        observableTargetCrossPositionDisposable = x.Subscribe(position =>
                                        {
                                            Vector3 playerPos = リズムパートで使用するプレイヤープロパティ.spotLightLightTrans.position;
                                            Vector3 lookDirection = position - playerPos;

                                            if (lookDirection.sqrMagnitude > 0.001f)
                                            {
                                                Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                                                リズムパートで使用するプレイヤープロパティ.elbow.rotation = targetRotation;
                                            }
                                        })
                                        .AddTo(ref _disposableBag);
                                    })
                                    .AddTo(ref _disposableBag);
                                Sequence spotLightLightSeqence = null;
                                observableLightControllerDisposable = Observable.EveryUpdate()
                                    .Subscribe(_ =>
                                    {
                                        // クリックした際にライトを点灯させるがすぐ消す
                                        if (player.GetButtonDown("TapLight"))
                                        {
                                            リズムパートで使用するプレイヤープロパティ.spotLightLight.enabled = true;
                                            リズムパートで使用するプレイヤープロパティ.spotLightLight.intensity = 0f;
                                            if (spotLightLightSeqence != null &&
                                                spotLightLightSeqence.IsActive())
                                            {
                                                spotLightLightSeqence.Kill();
                                            }
                                            // 新しくシーケンスを作り直す
                                            spotLightLightSeqence = DOTween.Sequence()
                                                .Append(DOTween.To(
                                                    () => リズムパートで使用するプレイヤープロパティ.spotLightLight.intensity,
                                                    x => リズムパートで使用するプレイヤープロパティ.spotLightLight.intensity = x,
                                                    1f, 0.2f))
                                                .Append(DOTween.To(
                                                    () => リズムパートで使用するプレイヤープロパティ.spotLightLight.intensity,
                                                    x => リズムパートで使用するプレイヤープロパティ.spotLightLight.intensity = x,
                                                    0f, 0.2f))
                                                .OnComplete(() =>
                                                {
                                                    リズムパートで使用するプレイヤープロパティ.spotLightLight.enabled = false;
                                                });
                                            spotLightLightSeqence.Play();
                                            if (_playerViewModel.IsSelectedBattery)
                                            {
                                                // ViewModel経由で電池のTransformを取得
                                                var batteryTransform = _playerViewModel.BatteryTransform;
                                                if (batteryTransform != null)
                                                {
                                                    BatteryView batteryView = batteryTransform.GetComponent<BatteryView>();
                                                    batteryView.GetBattery();
                                                }
                                            }
                                        }
                                    })
                                    .AddTo(ref _disposableBag);
                                // APIを使用してCRIWARE_conductor.csのJustBeatTickを監視
                                observableJustBeatTickDisposable = Observable.EveryUpdate()
                                    .Select(_ => script_XyloApi.JustBeatTick())
                                    .Pairwise()
                                    .Where(x => x.Previous != x.Current)
                                    .Select(x => x.Current)
                                    .Subscribe(justBeatTick =>
                                    {
                                        switch (justBeatTick)
                                        {
                                            case 4:
                                                // [Miss]失敗を購読した場合は電池を落とす
                                                if (_playerViewModel.BatteryTransform == null)
                                                {
                                                    Transform battery = DropBattery(trans, リズムパートで使用するプレイヤープロパティ.spotLightLightTrans);
                                                    _playerViewModel.SetBatteryTransform(battery);
                                                }

                                                break;
                                        }
                                    })
                                    .AddTo(ref _disposableBag);
                            }
                            // リズム⇒探索：カメラをリセット
                            switch (part.Previous)
                            {
                                case InteractionPart.Rhythm:
                                    switch (part.Current)
                                    {
                                        case InteractionPart.Search:
                                            if (followPlayerCameraView != null)
                                                followPlayerCameraView.ResetFollowAndLookAt();

                                            break;
                                    }

                                    break;
                            }
                        })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // オバケが隠れている位置に接近した時に振動
            VibrationView vibrationView = GetComponent<VibrationView>();
            Observable.EveryUpdate()
                .Select(_ => _playerViewModel.OnActionPoltergeistPosition)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Subscribe(x =>
                    {
                        // プレイヤーの現在位置を取得
                        Vector3 playerPosition = trans.position;
                        // 距離を計算
                        float distance = Vector3.Distance(playerPosition, x);
                        vibrationView.VibrateController(player, distance);
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Select(_ => FindAnyObjectByType<FadeImageView>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _fadeImageView = x;
                })
                .AddTo(ref _disposableBag);
            // シャウトチャンスレンジ検知
            List<Transform> shoutChanceRanges = new List<Transform>();
            System.IDisposable disposableShoutChanceRangesSetter = null;
            // シャウト成功となる条件設定処理の実装
            // シャウトチャンスパート
            // + コライダー内
            // + マイク音量 or キーボード長押し⇒解放 or コントローラー長押し⇒解放
            ReactiveProperty<float> dbLevel = new ReactiveProperty<float>();
            System.IDisposable disposableDbLevel = null;
            Observable.EveryUpdate()
                .Select(_ => _playerViewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Subscribe(x =>
                    {
                        disposableShoutChanceRangesSetter?.Dispose();
                        switch (x)
                        {
                            case InteractionPart.ShoutChance:
                                float rayLength = 1f; // 正面に飛ばす長さ（必要に応じて調整）

                                disposableShoutChanceRangesSetter = Observable.EveryUpdate()
                                    .Subscribe(_ =>
                                    {
                                        shoutChanceRanges.Clear();

                                        Vector3 origin = transform.position + Vector3.up * 0.5f; // 目線の高さ
                                        Vector3 direction = transform.forward;
                                        
                                        // デバッグ：Sceneビューに赤線を描画
                                        Debug.DrawRay(origin, direction * rayLength, Color.red);

                                        RaycastHit[] hits = Physics.RaycastAll(origin, direction, rayLength);
                                        foreach (RaycastHit hit in hits)
                                        {
                                            if (hit.collider != null && hit.collider.name.StartsWith("ShoutChanceRange"))
                                            {
                                                Transform t = hit.collider.transform;
                                                if (!shoutChanceRanges.Contains(t))
                                                {
                                                    shoutChanceRanges.Add(t);
                                                }
                                            }
                                        }
                                    })
                                    .AddTo(ref _disposableBag);

                                disposableDbLevel?.Dispose();
                                disposableDbLevel = dbLevel.Where(x => シャウトチャンスパートの共通パラメータ管理用テーブル.シャウト達成デシベル <= x &&
                                    0 < shoutChanceRanges.Count)
                                    .Subscribe(_ =>
                                    {
                                        // [シャウト成功インタラクション] 1. シャウトチャンスレンジの中でオバケが潜んでいる家具かつ、一番近いコライダーからポルターガイストビューを取得
                                        poltergeistView = shoutChanceRanges
                                            .Where(q => q.GetComponentInChildren<PoltergeistView>().GhostInStaticObjectStruct.useStatus.Equals(UseStatus.Using))
                                            .OrderBy(t => Vector3.SqrMagnitude(t.position - transform.position))
                                            .Select(q => q.GetComponentInChildren<PoltergeistView>())
                                            .FirstOrDefault();

                                        // [シャウト成功インタラクション] 2. オバケが飛び出すエフェクト生成
                                        if (poltergeistView != null)
                                        {
                                            poltergeistView.AsyncDoBurstGhosts();
                                            poltergeistView.InstanceMissileTempoSpawner();
                                            _playerViewModel.SetInteractionPart(InteractionPart.Rhythm);
                                        }
                                        script_XyloApi.ChangeBgmB();
                                    })
                                    .AddTo(ref _disposableBag);

                                break;
                        }
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);

            // 吸気入力監視用
            bool isInhaling = false;
            bool isDualInhaling = false;
            float inhaleStartTime = 0f;
            float inhaleDurationThreshold = 1.0f; // 例：1秒以上

            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    bool inhaleHeld = player.GetButton("Inhale");
                    bool inhaleLeftHeld = player.GetButton("InhaleHalfLeft");
                    bool inhaleRightHeld = player.GetButton("InhaleHalfRight");
                    bool isMicInput = script_XyloApi.IsMicInput();

                    // Inhale 単体の入力
                    if (inhaleHeld &&
                        !inhaleLeftHeld &&
                        !inhaleRightHeld &&
                        !isMicInput)
                    {
                        if (!isInhaling)
                        {
                            isInhaling = true;
                            inhaleStartTime = Time.time;
                        }
                    }
                    else
                    {
                        if (isInhaling)
                        {
                            float duration = Time.time - inhaleStartTime;
                            if (duration >= inhaleDurationThreshold)
                            {
                                // キー／トリガー入力のためそれっぽいMAX値をセット
                                dbLevel.Value = シャウトチャンスパートの共通パラメータ管理用テーブル.シャウト達成デシベル;
                            }
                            else
                            {
                                dbLevel.Value = 0f;
                            }
                            isInhaling = false;
                        }
                    }

                    // 両方のHalfInhaleを長押し
                    if (inhaleLeftHeld &&
                        inhaleRightHeld &&
                        !inhaleHeld &&
                        !isMicInput)
                    {
                        if (!isDualInhaling)
                        {
                            isDualInhaling = true;
                            inhaleStartTime = Time.time;
                        }
                    }
                    else
                    {
                        if (isDualInhaling)
                        {
                            float duration = Time.time - inhaleStartTime;
                            if (duration >= inhaleDurationThreshold)
                            {
                                // キー／トリガー入力のためそれっぽいMAX値をセット
                                dbLevel.Value = シャウトチャンスパートの共通パラメータ管理用テーブル.シャウト達成デシベル;
                            }
                            else
                            {
                                dbLevel.Value = 0f;
                            }
                            isDualInhaling = false;
                        }
                    }

                    // マイク入力の取得
                    if (!inhaleHeld &&
                        !inhaleLeftHeld &&
                        !inhaleRightHeld &&
                        isMicInput)
                    {
                        dbLevel.Value = script_XyloApi.GetDBLevel();
                    }

                    _playerViewModel.SetDbLevel(dbLevel.Value);
                    // どちらも押されていない場合も毎フレーム 0 に戻す（押し直しに備える）
                    if (!inhaleHeld && !(inhaleLeftHeld && inhaleRightHeld) &&
                        !isMicInput)
                    {
                        if (!isInhaling && !isDualInhaling)
                        {
                            dbLevel.Value = 0f;
                        }
                    }
                })
                .AddTo(ref _disposableBag);
            // ライトは一旦、消す
            リズムパートで使用するプレイヤープロパティ.spotLightLight.enabled = false;
            // プレイヤーの体力初期設定
            Observable.EveryUpdate()
                .Select(_ => GameManager.Instance)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(manager =>
                {
                    Observable.EveryUpdate()
                        .Select(_ => manager.LevelOwner.PlayerHealthPointMax)
                        .Where(x => x != null)
                        .Take(1)
                        .Subscribe(hpMax =>
                        {
                            _playerViewModel.SetHealthPointMax(hpMax.Value);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnGUI()
        {
#if UNITY_EDITOR
            // ボタンの位置とサイズ (x, y, width, height)
            Rect buttonRect = new Rect(10, 10, 150, 50);
            if (GUI.Button(buttonRect, "InteractionPart を初期化"))
            {
                _playerViewModel.SetInteractionPart(InteractionPart.None);
                _playerViewModel.SetInteractionPart(InteractionPart.Search);
            }
#endif
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// 電池を落とす
        /// </summary>
        /// <param name="playerTransform">プレイヤーのトランスフォーム</param>
        /// <param name="spotLightLightTrans">スポットライトのトランスフォーム</param>
        /// <returns>バッテリーのトランスフォーム</returns>
        private Transform DropBattery(Transform playerTransform, Transform spotLightLightTrans)
        {
            if (リズムパートで使用するプレイヤープロパティ.batteryPrefab == null)
            {
                Debug.LogWarning("BatteryPrefab が設定されていません。");
                return null;
            }

            // プレイヤーの正面方向
            Vector3 forward = playerTransform.forward;

            // ランダムな角度（-20度〜+20度）をY軸回転で加える
            float angle = Random.Range(-20f, 20f);
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 offsetDirection = rotation * forward;

            Vector3 spawnPosition = playerTransform.position + offsetDirection.normalized * 0.5f + Vector3.up * 0.8f;

            // 電池を生成
            Transform battery = Instantiate(リズムパートで使用するプレイヤープロパティ.batteryPrefab, spotLightLightTrans.position, Quaternion.identity);
            BatteryView batteryView = battery.GetComponent<BatteryView>();
            // 回転と移動を組み合わせたシーケンスを作成
            Sequence batterySeq = DOTween.Sequence();
            // ターゲットの向き（移動方向を向く）
            Vector3 direction = spawnPosition - spotLightLightTrans.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            // 移動しながら回転（0.5秒程度で調整）
            batterySeq
                .Append(battery.DOMove(spawnPosition, 0.5f).SetEase(Ease.OutQuad))
                .Join(battery.DORotate(new Vector3(360f, 0f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad))
                .OnComplete(() =>
                {
                    Vector3 direction = spawnPosition - battery.position;
                    if (direction.sqrMagnitude > 0.001f)
                    {
                        battery.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    }
                    batteryView.SetEnabledCollider(true);
                })
                .Play();

            return battery;
        }

        /// <summary>
        /// オバケの引っ越し
        /// </summary>
        /// <param name="ghostTeamID">オバケ団体ID</param>
        private void DoShuffleNewStaticObject(string ghostTeamID)
        {
            var poltergeistView = SearchPoltergeistView(ghostTeamID);
            poltergeistView.ShuffleNewStaticObject();
        }

        /// <summary>
        /// 拠点を空室にする
        /// </summary>
        /// <param name="ghostTeamID">オバケ団体ID</param>
        private void DoExitGhost(string ghostTeamID)
        {
            var poltergeistView = SearchPoltergeistView(ghostTeamID);
            poltergeistView.ExitGhost();
        }

        /// <summary>
        /// ポルターガイストのビューを検索
        /// </summary>
        /// <param name="ghostTeamID">オバケ団体ID</param>
        /// <returns>ポルターガイストのビュー</returns>
        private PoltergeistView SearchPoltergeistView(string ghostTeamID)
        {
            // ghostTeamIDからの逆引きでViewのIDを取得
            int targetPoltergeistViewID = 0;
            var ghostInStaticObjectStructs = _playerViewModel.GhostInStaticObjectStructs;
            if (ghostInStaticObjectStructs != null &&
                0 < ghostInStaticObjectStructs.Count)
            {
                var ghostInStaticObjectStruct = ghostInStaticObjectStructs.FirstOrDefault(q => q.ghostTeamID != null &&
                        q.ghostTeamID.Value.Equals(ghostTeamID));
                targetPoltergeistViewID = ghostInStaticObjectStruct.poltergeistViewID;
            }
            // 対象のポルターガイストを取得 
            var poltergeistViews = FindObjectsByType<PoltergeistView>(FindObjectsSortMode.None);
            // ViewのIDとpoltergeistViewsのGetInstanceIdを検索
            var poltergeistView = poltergeistViews.FirstOrDefault(q => q.GhostInStaticObjectStruct.poltergeistViewID == targetPoltergeistViewID);
            return poltergeistView;
        }

        /// <summary>
        /// 地面の接触判定
        /// </summary>
        /// <returns>地面に接触しているか</returns>
        private bool IsGrounded()
        {
            float radius = characterController.radius;
            float skinWidth = characterController.skinWidth;
            float raycastDistance = characterController.height / 2f - radius + skinWidth + 0.1f + .83f; // 余裕を持たせる
            Vector3 rayOrigin = characterController.transform.position + Vector3.up * (radius - skinWidth);

            return Physics.SphereCast(rayOrigin, radius, Vector3.down, out RaycastHit hit, raycastDistance);
        }
    }
}
