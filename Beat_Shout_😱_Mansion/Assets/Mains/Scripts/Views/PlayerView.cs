using UnityEngine;
using Rewired;
using R3;
using R3.Triggers;
using Mains.Commons;
using Mains.ViewModels;
using System.Linq;
using Mains.External;

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
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        [SerializeField] private InteractionPartTable 探索_シャウトチャンス_リズムパート情報管理テーブル;
        [SerializeField] private float ロー_切り替え時間_秒;
        [SerializeField] private float ロー_歩幅;
        [SerializeField] private float トップ_歩幅;
        /// <summary>シャウトチャンスレンジ位置</summary>
        private Transform _shoutChanceRange;
        /// <summary>シャウト成功</summary>
        private ReactiveProperty<bool> _isSuccessShout = new ReactiveProperty<bool>();
        /// <summary>プレイヤーのビューモデル</summary>
        private PlayerViewModel _playerViewModel;
        /// <summary>フェードイメージのビュー</summary>
        private FadeImageView _fadeImageView;

        private void Reset()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
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
            // リズムパートの位置まで移動する
            _isSuccessShout.Where(x => x)
                .Subscribe(x =>
                {
                    Observable.Create<bool>(observer =>
                    {
                        StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                        return Disposable.Empty;
                    })
                        .Subscribe(_ =>
                        {
                            // プレイヤー移動させる処理
                            if (_shoutChanceRange != null)
                            {
                                var poltergeistView = _shoutChanceRange.GetComponentInChildren<PoltergeistView>();
                                if (poltergeistView != null)
                                {
                                    characterController.enabled = false;
                                    trans.position = poltergeistView.RhythmPartPosition.position;
                                    trans.eulerAngles = poltergeistView.RhythmPartPosition.eulerAngles;
                                    currentYaw = trans.eulerAngles.y;
                                    characterController.enabled = true;
                                }
                            }
                            Observable.Create<bool>(observer =>
                            {
                                StartCoroutine(_fadeImageView.PlayFadeOutDirection(observer));
                                return Disposable.Empty;
                            })
                                .Subscribe(_ =>
                                {
                                    _isSuccessShout.Value = false;
                                })
                                .AddTo(ref _disposableBag);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // 重力管理用のVelocity
            Vector3 velocity = Vector3.zero;
            _playerViewModel = new(探索_シャウトチャンス_リズムパート情報管理テーブル);
            Observable.EveryUpdate()
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
            // シャウトチャンスレンジ検知
            this.OnTriggerStayAsObservable()
                .Where(x => x.name.StartsWith("ShoutChanceRange"))
                .Select(x => x.transform)
                .Subscribe(x => _shoutChanceRange = x)
                .AddTo(ref _disposableBag);
            this.OnTriggerExitAsObservable()
                .Where(x => x.name.StartsWith("ShoutChanceRange"))
                .Subscribe(x => _shoutChanceRange = null)
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
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// リズムパートの位置まで移動する
        /// </summary>
        private void MoveToShoutChanceRange()
        {
            _isSuccessShout.Value = true;
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
            float raycastDistance = characterController.height / 2f - radius + skinWidth + 0.1f + 0.35f; // 余裕を持たせる
            Vector3 rayOrigin = characterController.transform.position + Vector3.up * (radius - skinWidth);

            return Physics.SphereCast(rayOrigin, radius, Vector3.down, out RaycastHit hit, raycastDistance);
        }
    }
}
