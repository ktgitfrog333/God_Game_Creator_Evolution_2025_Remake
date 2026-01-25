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
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Mains.Views
{
    /// <summary>
    /// プレイヤーのビュー
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerView : MonoBehaviour, IDidStartProvider, Selects.Views.IDidStartProvider
    {
        [Header("全パート共通")]
        [SerializeField] private InteractionPartTable 探索_シャウトチャンス_リズムパート情報管理テーブル;
        /// <summary>プレイヤーの設定</summary>
        [SerializeField] private PlayerSettrings settings;
        [Header("探索パート／シャウトチャンスパート")]
        /// <summary>キャラクター移動制御</summary>
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float トップ_移動速度;
        [SerializeField] private float 視点速度補正;
        [SerializeField] private float 重力 = 9.81f;
        [SerializeField] private float ロー_切り替え時間_秒;
        [SerializeField] private float ロー_歩幅;
        [SerializeField] private float トップ_歩幅;
        [SerializeField] private PlayerShoutChanceTable シャウトチャンスパートの共通パラメータ管理用テーブル;
        /// <summary>プレイヤーのビューモデル</summary>
        private PlayerViewModel _playerViewModel;
        /// <summary>フェードイメージのビュー</summary>
        private FadeImageView _fadeImageView;
        /// <summary>地面との距離</summary>
        [SerializeField] private float distanceToGround;
        /// <summary>接地判定の対象レイヤー</summary>
        [SerializeField] private LayerMask groundLayerMask;
        /// <summary>カメラ視線用のトランスフォーム</summary>
        [SerializeField] private Transform headTrans;
        /// <summary>カメラ視線用のトランスフォーム</summary>
        public Transform HeadTrans => headTrans;
        [Header("リズムパート")]
        [SerializeField] private PlayerRhythmStruct リズムパートで使用するプレイヤープロパティ;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>Start完了を通知するObservable（Trueになったら1度だけ発火）</summary>
        private Subject<Unit> _didStartAsObservable = new Subject<Unit>();
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
            if (リズムパートで使用するプレイヤープロパティ.hitTrigger == null)
                リズムパートで使用するプレイヤープロパティ.hitTrigger = transform.GetChild(1).GetComponent<SphereCollider>();
            foreach (Transform child in transform)
            {
                if (child.name.Equals("Head"))
                {
                    if (headTrans == null)
                        headTrans = child;
                }
            }
        }

        private void Start()
        {
            _script_XyloApi = new Script_xyloApi();
            // 着地した瞬間も足音を鳴らす
            ReactiveProperty<bool> isGrounded = new();
            isGrounded.DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ =>
                {
                    _script_XyloApi.PlayFootStep();
                })
                .AddTo(ref _disposableBag);
            _playerViewModel = new(探索_シャウトチャンス_リズムパート情報管理テーブル);
            float lastFixedTimeForGrounded = 0f;
            Observable.EveryUpdate()
                .Where(_ => Time.time - lastFixedTimeForGrounded >= Time.fixedDeltaTime) // FixedUpdateと同じタイミング
                .Subscribe(_ =>
                {
                    lastFixedTimeForGrounded = Time.time; // 次の実行タイミングを記録
                    isGrounded.Value = _playerViewModel.IsGrounded(characterController, distanceToGround, groundLayerMask);
                })
                .AddTo(ref _disposableBag);
            // 正面移動かどうかのステータス管理
            ReactiveProperty<bool> isMovingForward = new ReactiveProperty<bool>();
            // 移動距離が一定の長さを超えた場合に足音を鳴らす
            ReactiveProperty<float> walkingDistance = new();
            walkingDistance.Where(x => (isMovingForward.Value ? トップ_歩幅 : ロー_歩幅) < x)
                .Subscribe(_ =>
                {
                    _script_XyloApi.PlayFootStep();
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
            player.controllers.maps.SetMapsEnabled(true, "Default"); // ゲーム操作を無効化
            // 現在のY軸回転角度 (左右回転)
            float currentYaw = headTrans.rotation.eulerAngles.y;
            // 現在のX軸回転角度 (上下回転)
            float currentPitch = headTrans.rotation.eulerAngles.x;
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
            ReactiveProperty<int> movePlayerAndpoltergeistProcessCnt = new ReactiveProperty<int>();
            // [シャウト成功インタラクション] 下記の3つの購読を監視
            //  3-a. プレイヤー移動アニメーション
            //  3-b. プレイヤー回転アニメーション
            //  3-c. 家具移動アニメーション
            movePlayerAndpoltergeistProcessCnt.Where(x => 3 == x)
                .Subscribe(_ =>
                {
                    // [シャウト成功インタラクション] 4. 後処理
                    characterController.enabled = true;
                    StartCoroutine(poltergeistView.DoPlayFloaterAnimation());
                    _playerViewModel.SetIsCompletedBurstGhosts(false);
                    poltergeistView = null;
                    if (followPlayerCameraView != null)
                    {
                        StartCoroutine(followPlayerCameraView.AsyncDeleteFollowAndLookAt());
                    }
                    movePlayerAndpoltergeistProcessCnt.Value = 0;
                })
                .AddTo(ref _disposableBag);
            // 頭の高さローカル位置を保存
            Vector3 originHeadTransLocalPosition = headTrans.localPosition;
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
                            // [シャウト成功インタラクション] 3. プレイヤー移動させる処理
                            if (poltergeistView != null)
                            {
                                characterController.enabled = false;
                                // [シャウト成功インタラクション] 3-a. プレイヤー移動アニメーション
                                trans.DOMove(poltergeistView.RhythmPartPosition, 1f)
                                    .OnComplete(() =>
                                    {
                                        movePlayerAndpoltergeistProcessCnt.Value++;
                                    });
                                // [シャウト成功インタラクション] 3-b. プレイヤー回転アニメーション
                                Observable.Create<bool>(observer =>
                                {
                                    StartCoroutine(LookAtLoopPoltergeist(observer, headTrans, poltergeistView.transform, 1f));

                                    return Disposable.Empty;
                                })
                                    .Subscribe(_ =>
                                    {
                                        headTrans.eulerAngles = poltergeistView.RhythmPartEulerAngles;
                                        currentYaw = headTrans.eulerAngles.y;
                                        movePlayerAndpoltergeistProcessCnt.Value++;
                                    })
                                    .AddTo(ref _disposableBag);
                                Observable.Create<bool>(observer =>
                                {
                                    // [シャウト成功インタラクション] 3-c. 家具移動アニメーション
                                    StartCoroutine(poltergeistView.PlayMovePositionAnimation(observer, trans));

                                    return Disposable.Empty;
                                })
                                    .Subscribe(_ =>
                                    {
                                        movePlayerAndpoltergeistProcessCnt.Value++;
                                    })
                                    .AddTo(ref _disposableBag);
                                // [シャウト成功インタラクション] 3-d. プレイヤーの頭の高さをリズムパート用に変更
                                var localPosition = headTrans.localPosition;
                                headTrans.localPosition = new Vector3(localPosition.x, リズムパートで使用するプレイヤープロパティ.headHeight, localPosition.z);
                            }
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // 重力管理用のVelocity
            Vector3 velocity = Vector3.zero;
            _playerViewModel.SetPlayerTransform(transform);
            // イントロが完了するまではプレイヤー操作禁止
            characterController.enabled = false;
            // フェード処理が完了するまではプレイヤー操作禁止
            Observable.EveryUpdate()
                .Select(_ => _playerViewModel.IsCompletedStartDirectionReactive)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Where(x => x)
                        .Subscribe(_ =>
                        {
                            if (!characterController.enabled)
                                characterController.enabled = true;
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // シャウトが成功したポジション
            Vector3? successShoutPosition = null;
            // シャウトが成功したオイラー角度
            Vector3? successShoutEulerAngles = null;
            _script_XyloApi.InitVolumeLevelReactive();
            // 恐怖値のカウントを停止する
            bool isStopHorrorCount = false;
            // 視界ジャック用ゴースト
            Transform targetGhost = null;
            _playerViewModel.TargetGhost.Subscribe(x =>
            {
                targetGhost = x;
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
                    System.IDisposable observableUpdateIsFailedDisposable = null;
                    System.IDisposable observableIsFailedDisposable = null;
                    System.IDisposable observableTargetCrossPositionDisposable = null;
                    System.IDisposable volumeLevelReactiveDisposable = null;
                    System.IDisposable interactionPartDisposable = null;
                    Camera mainCamera = null;
                    interactionPartDisposable = x.Pairwise()
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
                                observableUpdateIsFailedDisposable?.Dispose();
                                observableIsFailedDisposable?.Dispose();
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
                                        if (mainCamera == null)
                                        {
                                            mainCamera = Camera.main;
                                        }
                                        Vector3 cameraForward = mainCamera.transform.forward;
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
                                        // 重力処理（Move()の前に適用）
                                        if (!isGrounded.Value)
                                        {
                                            velocity.y -= 重力 * Time.deltaTime;
                                        }
                                        else
                                        {
                                            velocity.y = 0; // 地面にいる場合、Y方向の速度をリセット
                                        }
                                        characterController.Move((move + velocity) * Time.deltaTime);
                                        // Move()の後にCharacterControllerの接地状態を確認
                                        // CharacterController.isGroundedはMove()の後に自動的に更新されるため、より正確
                                        if (!characterController.isGrounded)
                                        {
                                            // 実際に接地していない場合は、次のフレームで確実に落下させるためvelocity.yを更新
                                            // 接地していない場合、velocity.yが正の値（上昇中）でない限り、重力を適用
                                            if (velocity.y <= 0)
                                            {
                                                velocity.y -= 重力 * Time.deltaTime;
                                            }
                                        }
                                        else
                                        {
                                            // 接地している場合は速度をリセット（下向きの速度のみ）
                                            if (velocity.y < 0)
                                            {
                                                velocity.y = 0;
                                            }
                                        }
                                        // 視点移動入力
                                        AjustHeadEulerAnglesXY(targetGhost, headTrans, player, ref currentYaw, ref currentPitch, 視点速度補正);

                                        // 回転を適用
                                        headTrans.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

                                        bool isSwitchPart = player.GetButtonDown("SwitchPart");
                                        _playerViewModel.SetIsSwitchPart(isSwitchPart);
                                        if (!isStopHorrorCount)
                                        {
                                            _playerViewModel.AddHorrorCount(1f * Time.deltaTime);
                                        }
                                    })
                                    .AddTo(ref _disposableBag);
                                _playerViewModel.SetIsLockedUpdateHealthPoint(false);
                            }
                            else if (isRhythmCtrl)
                            {
                                // 1. 探索、シャウト用の操作の監視を破棄
                                observablePlayerControllerDisposable?.Dispose();
                                volumeLevelReactiveDisposable?.Dispose();
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
                                                    _script_XyloApi.PlayBatteryGet3();
                                                }
                                            }
                                            if (_playerViewModel.BatteryTransform == null &&
                                                _playerViewModel.SelectedMissGhostAttackTransform != null)
                                            {
                                                _playerViewModel.SelectedMissGhostAttackTransform.gameObject.SetActive(false);
                                            }
                                        }
                                        // Xbox360コントローラー専用
                                        if (player.GetButtonDown("GetBattery"))
                                        {
                                            if (_playerViewModel.IsSelectedBattery)
                                            {
                                                // ViewModel経由で電池のTransformを取得
                                                var batteryTransform = _playerViewModel.BatteryTransform;
                                                if (batteryTransform != null)
                                                {
                                                    BatteryView batteryView = batteryTransform.GetComponent<BatteryView>();
                                                    batteryView.GetBattery();
                                                    _script_XyloApi.PlayBatteryGet3();
                                                }
                                            }
                                            if (_playerViewModel.BatteryTransform == null &&
                                                _playerViewModel.SelectedMissGhostAttackTransform != null)
                                            {
                                                _playerViewModel.SelectedMissGhostAttackTransform.gameObject.SetActive(false);
                                            }
                                        }
                                    })
                                    .AddTo(ref _disposableBag);
                                observableUpdateIsFailedDisposable = Observable.EveryUpdate()
                                    .Select(_ => _playerViewModel.IsFailed)
                                    .Where(x => x != null)
                                    .Take(1)
                                    .Subscribe(x =>
                                    {
                                        observableIsFailedDisposable = x.Where(x => x)
                                            .Subscribe(_ =>
                                            {
                                                // [Miss]失敗を購読した場合は電池を落とす
                                                if (_playerViewModel.BatteryTransform == null)
                                                {
                                                    Transform battery = DropBattery(headTrans, リズムパートで使用するプレイヤープロパティ.spotLightLightTrans);
                                                    _playerViewModel.SetBatteryTransform(battery);
                                                }
                                            })
                                            .AddTo(ref _disposableBag);
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
                                            // ここだけクリア状態を直接参照しないと修正が困難なため
                                            if (_playerViewModel.IsMissionClear)
                                            {
                                                // クリアなら後続処理は中断
                                                return;
                                            }

                                            var localPosition = originHeadTransLocalPosition;
                                            headTrans.localPosition = localPosition;
                                            if (followPlayerCameraView != null)
                                                followPlayerCameraView.ResetFollowAndLookAt();
                                            // Rewairedで操作を禁止にする。着地したら暗幕フェードの透明度を元に戻す。
                                            player.controllers.maps.SetMapsEnabled(false, "Default");
                                            if (isGrounded.Value)
                                            {
                                                Observable.Create<bool>(observer =>
                                                {
                                                    StartCoroutine(_fadeImageView.PlayFadeOutDirection(observer, default, false));
                                                    return Disposable.Empty;
                                                })
                                                    .Take(1)
                                                    .Subscribe(_ =>
                                                    {
                                                        _playerViewModel.SetIsPostRhythmFaceOff(true);
                                                    })
                                                    .AddTo(ref _disposableBag);
                                                player.controllers.maps.SetMapsEnabled(true, "Default");
                                            }
                                            else
                                            {
                                                isGrounded.DistinctUntilChanged()
                                                    .Where(x => x)
                                                    .Subscribe(_ =>
                                                    {
                                                        Observable.Create<bool>(observer =>
                                                        {
                                                            StartCoroutine(_fadeImageView.PlayFadeOutDirection(observer, default, false));
                                                            return Disposable.Empty;
                                                        })
                                                            .Take(1)
                                                            .Subscribe(_ =>
                                                            {
                                                                _playerViewModel.SetIsPostRhythmFaceOff(true);
                                                            })
                                                            .AddTo(ref _disposableBag);
                                                        player.controllers.maps.SetMapsEnabled(true, "Default");
                                                    })
                                                    .AddTo(ref _disposableBag);
                                            }
                                            // 元の位置へ移動させる処理
                                            characterController.enabled = false;
                                            if (successShoutPosition.HasValue &&
                                                successShoutEulerAngles.HasValue)
                                            {
                                                trans.position = successShoutPosition.Value;
                                                trans.eulerAngles = successShoutEulerAngles.Value;
                                                currentYaw = trans.eulerAngles.y;
                                            }
                                            // 初期値へリセット
                                            successShoutPosition = null;
                                            successShoutEulerAngles = null;
                                            characterController.enabled = true;

                                            break;
                                    }

                                    break;
                            }
                        })
                        .AddTo(ref _disposableBag);
                    // クリア時には購読停止
                    _playerViewModel.IsMissionClearReactive.Where(x => x)
                        .Take(1)
                        .Subscribe(_ =>
                        {
                            interactionPartDisposable?.Dispose();
                            observablePlayerControllerDisposable?.Dispose();
                        })
                        .AddTo(ref _disposableBag);
                    // 自力でExecute
                    x.Value = InteractionPart.None;
                    x.Value = InteractionPart.Search;
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
            // シャウト成功判定が既に実行されたかどうかを追跡
            bool isShoutSuccessProcessed = false;
            Observable.EveryUpdate()
                .Select(_ => _playerViewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Subscribe(x =>
                    {
                        disposableShoutChanceRangesSetter?.Dispose();
                        disposableDbLevel?.Dispose();
                        // パートが変わったら成功判定フラグをリセット
                        isShoutSuccessProcessed = false;
                        switch (x)
                        {
                            case InteractionPart.ShoutChance:
                                float rayLength = 1f; // 正面に飛ばす長さ（必要に応じて調整）

                                disposableShoutChanceRangesSetter = Observable.EveryUpdate()
                                    .Subscribe(_ =>
                                    {
                                        shoutChanceRanges.Clear();

                                        Vector3 origin = headTrans.position; // 目線の高さ
                                        Vector3 direction = headTrans.forward;
                                        
                                        // デバッグ：Sceneビューに赤線を描画
                                        Debug.DrawRay(origin, direction * rayLength, Color.red);

                                        RaycastHit[] hits = Physics.RaycastAll(origin, direction, rayLength);
                                        foreach (RaycastHit hit in hits)
                                        {
                                            if (hit.collider != null && hit.collider.name.StartsWith("ShoutChanceRange"))
                                            {
                                                Transform t = hit.collider.transform;
                                                if (!shoutChanceRanges.Contains(t) &&
                                                    t.GetComponentInChildren<PoltergeistView>().GhostInStaticObjectStruct.useStatus.Equals(UseStatus.Using))
                                                {
                                                    shoutChanceRanges.Add(t);
                                                }
                                            }
                                        }
                                    })
                                    .AddTo(ref _disposableBag);

                                // 毎フレーム条件をチェックする方式に変更（タイミングずれ問題を解決）
                                disposableDbLevel = Observable.EveryUpdate()
                                    .Where(_ => !isShoutSuccessProcessed)
                                    .Where(_ => シャウトチャンスパートの共通パラメータ管理用テーブル.シャウト達成デシベル <= dbLevel.Value &&
                                        0 < shoutChanceRanges.Count)
                                    .Take(1)
                                    .Subscribe(_ =>
                                    {
                                        isShoutSuccessProcessed = true;
                                        successShoutPosition = trans.position;
                                        successShoutEulerAngles = trans.eulerAngles;
                                        // [シャウト成功インタラクション] 1. シャウトチャンスレンジの中でオバケが潜んでいる家具かつ、一番近いコライダーからポルターガイストビューを取得
                                        poltergeistView = shoutChanceRanges
                                            .OrderBy(t => Vector3.SqrMagnitude(t.position - headTrans.position))
                                            .Select(q => q.GetComponentInChildren<PoltergeistView>())
                                            .FirstOrDefault();
                                        // [シャウト成功インタラクション] 2. オバケが飛び出すエフェクト生成
                                        poltergeistView.AsyncDoBurstGhosts();
                                        poltergeistView.InstanceMissileTempoSpawner();
                                        // BGM再生はトランザクション開始処理の中で実施
                                        poltergeistView.BeginTransactionGhostInStaticObjectStruct();
                                        _playerViewModel.SetInteractionPart(InteractionPart.Rhythm);
                                    })
                                    .AddTo(ref _disposableBag);

                                break;
                        }
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // ブレイブシャウト用
            // 一定のレベルを超えた際に一定時間減少を止める
            dbLevel.Where(x => シャウトチャンスパートの共通パラメータ管理用テーブル.シャウト達成デシベル <= x &&
                !isStopHorrorCount)
                .Subscribe(async _ =>
                {
                    isStopHorrorCount = true;
                    _playerViewModel.SetIsStopHorrorCount(isStopHorrorCount);
                    int time = (int)(シャウトチャンスパートの共通パラメータ管理用テーブル.恐怖値のカウント停止時間 * 1000f);
                    await Task.Delay(time);
                    isStopHorrorCount = false;
                    _playerViewModel.SetIsStopHorrorCount(isStopHorrorCount);
                })
                .AddTo(ref _disposableBag);

            // 吸気入力監視用
            bool isInhaling = false;
            bool isDualInhaling = false;

            Observable.EveryUpdate()
                .Where(_ => characterController.enabled)
                .Subscribe(_ =>
                {
                    bool inhaleHeld = player.GetButtonDown("Inhale");
                    bool inhaleHeldCon = (player.GetButtonDown("InhaleHalfLeft") && player.GetButton("InhaleHalfRight")) ||
                        (player.GetButton("InhaleHalfLeft") && player.GetButtonDown("InhaleHalfRight"));
                    bool isMicInput = _script_XyloApi.IsMicInput();

                    // Inhale 単体の入力
                    if (inhaleHeld &&
                        !inhaleHeldCon &&
                        !isMicInput)
                    {
                        if (!isInhaling)
                        {
                            isInhaling = true;
                            // キー／トリガー入力のためそれっぽいMAX値をセット
                            dbLevel.Value = シャウトチャンスパートの共通パラメータ管理用テーブル.シャウトゲージスライダー最大値;
                        }
                    }
                    else
                    {
                        if (isInhaling)
                        {
                            isInhaling = false;
                        }
                    }

                    // 両方のHalfInhaleを長押し
                    if (inhaleHeldCon &&
                        !inhaleHeld &&
                        !isMicInput)
                    {
                        if (!isDualInhaling)
                        {
                            isDualInhaling = true;
                            // キー／トリガー入力のためそれっぽいMAX値をセット
                            dbLevel.Value = シャウトチャンスパートの共通パラメータ管理用テーブル.シャウトゲージスライダー最大値;
                        }
                    }
                    else
                    {
                        if (isDualInhaling)
                        {
                            isDualInhaling = false;
                        }
                    }

                    // マイク入力の取得
                    if (!inhaleHeld &&
                        !inhaleHeldCon &&
                        isMicInput)
                    {
                        dbLevel.Value = _script_XyloApi.GetDBLevel();
                    }

                    _playerViewModel.SetDbLevel(dbLevel.Value);
                    // どちらも押されていない場合も毎フレーム 0 に戻す（押し直しに備える）
                    if (!inhaleHeld && !inhaleHeldCon &&
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
            // 当たり判定用のトリガーは常にカメラを追従する
            var hitTriggerTrans = リズムパートで使用するプレイヤープロパティ.hitTrigger.transform;
            Observable.EveryUpdate()
                .Select(_ => followPlayerCameraView)
                .Where(x => x != null)
                .Subscribe(followPlayerCameraView =>
                {
                    hitTriggerTrans.position = followPlayerCameraView.transform.position;
                })
                .AddTo(ref _disposableBag);
            // セレクトシーンのみ
            var currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName.Equals(settings.targetSceneName))
            {
                _playerViewModel.StartPointTrans.Where(x => x != null)
                    .Take(1)
                    .Subscribe(startTrans =>
                    {
                        MoveToPoint(startTrans, _playerViewModel.IsCompletedStartDirection, trans, characterController, ref currentYaw);
                    })
                    .AddTo(ref _disposableBag);
            }
            _didStartAsObservable.OnNext(Unit.Default);
            _didStartAsObservable.OnCompleted();
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
            _script_XyloApi?.Dispose();
            _playerViewModel?.Dispose();
        }

        public Observable<Unit> DidStartAsObservable()
        {
            return Observable.Create<Unit>(observer =>
            {
                _didStartAsObservable.Take(1)
                    .Subscribe(_ =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                    .AddTo(ref _disposableBag);

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// ポルターガイストの方へ向きを調整
        /// </summary>
        /// <param name="observer">オブザーバー</param>
        /// <param name="trans">トランスフォーム</param>
        /// <param name="poltergeistTransform">ポルターガイストのトランスフォーム</param>
        /// <param name="duration">終了時間</param>
        /// <returns>コルーチン</returns>
        private IEnumerator LookAtLoopPoltergeist(Observer<bool> observer, Transform trans, Transform poltergeistTransform, float duration)
        {
            float elapsedTimeSec = 0f;
            while (elapsedTimeSec < duration)
            {
                elapsedTimeSec += Time.deltaTime;
                trans.LookAt(poltergeistTransform);

                yield return null;
            }
            observer.OnNext(true);
            observer.OnCompleted();

            yield return null;
        }

        /// <summary>
        /// 頭の角度XYを調整
        /// </summary>
        /// <param name="targetGhost">視界ジャック用ゴースト</param>
        /// <param name="headTrans">カメラ視線用のトランスフォーム</param>
        /// <param name="player">Rewiredのプレイヤー</param>
        /// <param name="currentYaw">視点</param>
        /// <param name="currentPitch">ピッチ</param>
        /// <param name="aimSensitivity">視点速度補正</param>
        private void AjustHeadEulerAnglesXY(Transform targetGhost, Transform headTrans, Player player, ref float currentYaw, ref float currentPitch, float aimSensitivity)
        {
            if (targetGhost != null)
            {
                Vector3 lookDir = targetGhost.position - headTrans.position;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                    var euler = lookRotation.eulerAngles;
                    currentPitch = euler.x;
                    currentYaw = euler.y;
                }
            }
            else
            {
                float aimX = player.GetAxis("AimMoveHorizontal");
                float aimY = player.GetAxis("AimMoveVertical");
                // 視点変更 (角度を直接加算)
                currentYaw += aimX * aimSensitivity * Time.deltaTime;
                currentPitch -= aimY * aimSensitivity * Time.deltaTime;

                // ピッチ角度を制限 (-90度～90度)
                // TODO: 外的要因（シャウト成功による自動移動等）で取得角度がエッジケースに該当することがある
                //       currentPitchが0⇒359.3694⇒90（※Mathf.Clampの補間）⇒唐突にプレイヤーが土下座する
                currentPitch = Mathf.Clamp(currentPitch, -90f, 90f);
            }
        }

        /// <summary>
        /// 電池を落とす
        /// </summary>
        /// <param name="headTrans">カメラ視線用のトランスフォーム</param>
        /// <param name="spotLightLightTrans">スポットライトのトランスフォーム</param>
        /// <returns>バッテリーのトランスフォーム</returns>
        private Transform DropBattery(Transform headTrans, Transform spotLightLightTrans)
        {
            if (リズムパートで使用するプレイヤープロパティ.batteryPrefab == null)
            {
                Debug.LogWarning("BatteryPrefab が設定されていません。");
                return null;
            }

            // プレイヤーの正面方向
            Vector3 forward = headTrans.forward;

            // プレイヤーの向き（Y軸）に基づく角度にランダム±20度を加える
            float baseYaw = headTrans.rotation.eulerAngles.y;
            float randomOffset = Random.Range(-20f, 20f);
            float finalYaw = baseYaw + randomOffset;

            Quaternion rotation = Quaternion.Euler(0, finalYaw, 0);
            Vector3 offsetDirection = rotation * Vector3.forward;

            Vector3 spawnPosition = headTrans.position + offsetDirection.normalized * 1f + Vector3.down * 0.2f;

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
                    _script_XyloApi.PlayBatteryLost1();
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
        /// 指定位置まで移動
        /// </summary>
        /// <param name="startPointTrans">ステージ開始位置</param>
        /// <param name="isCompletedStartDirection">ステージ開始演出が完了したか</param>
        /// <param name="trans">トランスフォーム</param>
        /// <param name="characterController">キャラクター移動制御</param>
        /// <param name="currentYaw">現在のY軸回転角度 (左右回転)</param>
        private void MoveToPoint(Transform startPointTrans, bool isCompletedStartDirection, Transform trans, CharacterController characterController, ref float currentYaw)
        {
            if (characterController.enabled)
                characterController.enabled = false;
            trans.position = startPointTrans.position;
            trans.eulerAngles = startPointTrans.eulerAngles;
            currentYaw = trans.eulerAngles.y;
            if (isCompletedStartDirection)
            {
                if (!characterController.enabled)
                    characterController.enabled = true;
            }
        }
    }

    /// <summary>
    /// プレイヤーの設定
    /// </summary>
    [System.Serializable]
    public class PlayerSettrings
    {
        /// <summary>対象シーン名</summary>
        /// <remarks>セレクトシーンを指定する</remarks>
        public string targetSceneName;
    }
}
