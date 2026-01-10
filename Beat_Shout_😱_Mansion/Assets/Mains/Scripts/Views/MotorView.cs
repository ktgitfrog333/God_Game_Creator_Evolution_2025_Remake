using DG.Tweening;
using Mains.Commons;
using Mains.External;
using Mains.ViewModels;
using R3;
using System.Collections;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// モーターのビュー
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    public class MotorView : MonoBehaviour
    {
        [Tooltip("Assets/Mains/Scripts/Commons/PoltergeistTable.assetをセットしておく。")]
        [SerializeField] private PoltergeistTable poltergeistTable;
        /// <summary>アニメータ</summary>
        [SerializeField] private Animator animator;
        /// <summary>Rigidbody</summary>
        [SerializeField] private new Rigidbody rigidbody;
        /// <summary>基本の力の強さ</summary>
        [SerializeField] private float forceMagnitude;
        /// <summary>ランダム補正</summary>
        [SerializeField] private float forceVariation;
        /// <summary>回転の強さ</summary>
        [SerializeField] private float torqueMagnitude;
        [Tooltip("Assets/Mains/Prefabs/Effects/DustParticle.prefabをセットしておく。")]
        [SerializeField] private GameObject dustParticlePrefab;
        /// <summary>ポルターガイストのビューモデル</summary>
        private PoltergeistViewModel _poltergeistViewModel;
        /// <summary>ポルターガイストが有効か</summary>
        public bool IsEnabledPoltergeist { get; set; }
        /// <summary>トランスフォーム</summary>
        private Transform _transform;
        /// <summary>アクション発火の監視</summary>
        private ReactiveCommand<bool> _onAction = new ReactiveCommand<bool>();
        /// <summary>初期回転値</summary>
        private Quaternion _initialRotation;
        /// <summary>初期ローカル回転値</summary>
        private Quaternion _initialLocalRotation;
        /// <summary>角度しきい値（°）</summary>
        [SerializeField] private float tiltThreshold;
        [Tooltip("Assets/Mains/Prefabs/Level/ObjectsPoolView.prefabをセットしておく。")]
        [SerializeField] private GameObject objectsPoolViewPrefab;
        /// <summary>振動を開始する最長距離</summary>
        [SerializeField] private float maxDistance;
        ///// <summary>アニメーションをループするか</summary>
        //private bool _isLoopAnimation = false;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>浮かせるアニメーション処理の監視</summary>
        private System.IDisposable _onTempoSetDisposable = null;
        /// <summary>初期ローカルポジション</summary>
        private Vector3 _initialLocalPosition;
        /// <summary>初期ローカルオイラー角度</summary>
        private Vector3 _initialLocalEulerAngles;
        /// <summary>オブジェクトプールビュー</summary>
        private ObjectsPoolView _objectsPoolView;
        [SerializeField] private PlayerShoutChanceTable シャウトチャンスパートの共通パラメータ管理用テーブル;
        /// <summary>驚くアニメーションを再生中</summary>
        private bool _isPlayingFreakout;
        /// <summary>土煙パーティクルのトランスフォーム</summary>
        public Transform DustParticlePosition { get; set; }
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>R3のリソース管理</summary>
        private readonly CompositeDisposable _disposables = new();

        private void Reset()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (rigidbody == null)
                rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _transform = transform;
            _initialLocalPosition = transform.localPosition;
            _initialLocalEulerAngles = transform.localEulerAngles;
            Transform dustParticleInstance = null;
            _poltergeistViewModel = new PoltergeistViewModel(poltergeistTable);
            // オブジェクトプールビュー
            var objectsPoolView = GameObject.FindAnyObjectByType<ObjectsPoolView>();
            if (objectsPoolView == null)
                objectsPoolView = Instantiate(objectsPoolViewPrefab).GetComponent<ObjectsPoolView>();
            Se_3D_PickerCustomizeView t3DSoundPlayer = objectsPoolView.Get3DSoundPlayer();
            _objectsPoolView = objectsPoolView;
            Observable.EveryUpdate()
                .Select(_ => _poltergeistViewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    CompositeDisposable disposables = null;
                    x.DistinctUntilChanged()
                        .Subscribe(x =>
                        {
                            switch (x)
                            {
                                case InteractionPart.Search:
                                    // 探索パートはタップを有効
                                    animator.SetBool("Tap", true);

                                    break;
                                case InteractionPart.ShoutChance:
                                    // シャウトチャンスパートとリズムパートはタップを無効
                                    animator.SetBool("Tap", false);
                                    // デシベルレベルを取得してマイクアイコン切り替え表示する処理を追加
                                    disposables = new();
                                    Observable.EveryUpdate()
                                        .Select(_ => _poltergeistViewModel.DbLevel)
                                        .Where(x => x != null)
                                        .Take(1)
                                        .Subscribe(x =>
                                        {
                                            // TODO: アラート発生レベルの判定を共通化する
                                            x.Where(dbLevel => シャウトチャンスパートの共通パラメータ管理用テーブル.シャウト達成デシベル <= dbLevel &&
                                                !_isPlayingFreakout)
                                                .Subscribe(_ =>
                                                {
                                                    _isPlayingFreakout = true;
                                                    animator.SetTrigger("Freakout");
                                                })
                                                .AddTo(disposables);
                                        })
                                        .AddTo(disposables);

                                    break;
                                case InteractionPart.Rhythm:
                                    // シャウトチャンスパートとリズムパートはタップを無効
                                    animator.SetBool("Tap", false);

                                    break;
                            }
                        })
                        .AddTo(ref _disposableBag);
                    // シャウトチャンスパート⇒シャウトチャンスパート以外へ遷移
                    x.Pairwise()
                        .Where(x => x.Previous.Equals(InteractionPart.ShoutChance) &&
                            !x.Current.Equals(InteractionPart.ShoutChance))
                        .Subscribe(_ =>
                        {
                            disposables?.Dispose();
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            _onAction.Where(x => x)
                .Subscribe(_ =>
                {
                    DoInstanceDustAndPlaySE(IsEnabledPoltergeist, dustParticlePrefab, _transform,
                        dustParticleInstance,
                        DustParticlePosition,
                        _poltergeistViewModel.PlayerTransform, maxDistance,
                        t3DSoundPlayer,
                        _poltergeistViewModel,
                        true);

                    // 実行後はリセットする
                    _onAction.Execute(false);
                })
                .AddTo(ref _disposableBag);
            _initialRotation = _transform.rotation;
            _initialLocalRotation = _transform.localRotation;
            _script_XyloApi = new Script_xyloApi();
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _script_XyloApi?.Dispose();
            _disposables.Dispose();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }

        /// <summary>
        /// AnimarionClipからトリガー（アクション）の受信
        /// </summary>
        /// <see cref="Assets/Mains/Animations/Poltergeists/Poltergeist.controller"/>
        public void OnAction()
        {
            var objectsPoolView = _objectsPoolView;
            if (objectsPoolView == null)
                return;

            Se_3D_PickerCustomizeView t3DSoundPlayer = objectsPoolView.Get3DSoundPlayer();
            Transform dustParticleInstance = null;
            DoInstanceDustAndPlaySE(IsEnabledPoltergeist, dustParticlePrefab, _transform,
                dustParticleInstance,
                DustParticlePosition,
                _poltergeistViewModel.PlayerTransform, maxDistance,
                t3DSoundPlayer,
                _poltergeistViewModel);
        }

        /// <summary>
        /// AnimarionClipからトリガー（アクション）の受信
        /// </summary>
        /// <see cref="Assets/Mains/Animations/Poltergeists/Poltergeist.controller"/>
        public void OnActionFreakout()
        {
            PlayActionAnimation(IsEnabledPoltergeist, _transform, forceVariation, torqueMagnitude, _initialRotation, tiltThreshold,
                rigidbody, _poltergeistViewModel, _onAction);
        }

        /// <summary>
        /// AnimarionClipからトリガー（アクション）の受信
        /// </summary>
        /// <see cref="Assets/Mains/Animations/Poltergeists/Poltergeist.controller"/>
        public void OnCompletedFreakout()
        {
            _isPlayingFreakout = false;
        }

        /// <summary>
        /// アクションアニメーションを再生
        /// </summary>
        /// <param name="isEnabledPoltergeist">ポルターガイストが有効か</param>
        /// <param name="transform">トランスフォーム</param>
        /// <param name="forceVariation">ランダム補正</param>
        /// <param name="torqueMagnitude">回転の強さ</param>
        /// <param name="initialRotation">初期回転値</param>
        /// <param name="tiltThreshold">角度しきい値（°）</param>
        /// <param name="rigidbody">Rigidbody</param>
        /// <param name="poltergeistViewModel">ポルターガイストのビューモデル</param>
        /// <param name="onAction">アクション発火の監視</param>
        private void PlayActionAnimation(bool isEnabledPoltergeist, Transform transform, float forceVariation, float torqueMagnitude, Quaternion initialRotation, float tiltThreshold,
            Rigidbody rigidbody, PoltergeistViewModel poltergeistViewModel, ReactiveCommand<bool> onAction)
        {
            if (!isEnabledPoltergeist ||
                transform == null)
                return;

            // ランダムな方向への力
            Vector3 randomDirection = Random.insideUnitSphere.normalized; // 方向
            float randomForce = forceMagnitude + Random.Range(-forceVariation, forceVariation); // 強さ
            rigidbody.AddForce(randomDirection * randomForce, ForceMode.Impulse);

            // ランダムな回転を追加
            Vector3 randomTorque = new Vector3(
                Random.Range(-torqueMagnitude, torqueMagnitude),
                Random.Range(-torqueMagnitude, torqueMagnitude),
                Random.Range(-torqueMagnitude, torqueMagnitude)
            );
            rigidbody.AddTorque(randomTorque, ForceMode.Impulse);

            var angle = Quaternion.Angle(initialRotation, transform.rotation);
            if (tiltThreshold < angle)
            {
                // アクション実行を通知
                onAction.Execute(true);
            }
        }

        /// <summary>
        /// パーティクル生成及びSE再生
        /// </summary>
        /// <param name="isEnabledPoltergeist">ポルターガイストが有効か</param>
        /// <param name="dustParticlePrefab">Assets/Mains/Prefabs/Effects/DustParticle.prefabをセットしておく。</param>
        /// <param name="transform">トランスフォーム</param>
        /// <param name="dustParticleInstance">DustParticleのInstance用</param>
        /// <param name="dustParticlePosition">土煙パーティクルのトランスフォーム</param>
        /// <param name="playerTransform">プレイヤーのトランスフォーム</param>
        /// <param name="maxDistance">振動を開始する最長距離</param>
        /// <param name="t3DSoundPlayer">Se_3D_Pickerのカスタマイズビュー</param>
        /// <param name="poltergeistViewModel">ポルターガイストのビューモデル</param>
        /// <param name="isInstanceDust">DustParticleを生成するか</param>
        private void DoInstanceDustAndPlaySE(bool isEnabledPoltergeist, GameObject dustParticlePrefab, Transform transform,
            Transform dustParticleInstance,
            Transform dustParticlePosition,
            Transform playerTransform, float maxDistance,
            Se_3D_PickerCustomizeView t3DSoundPlayer,
            PoltergeistViewModel poltergeistViewModel,
            bool isInstanceDust = false)
        {
            if (!isEnabledPoltergeist)
                return;

            if (isInstanceDust)
            {
                if (dustParticleInstance == null)
                {
                    dustParticleInstance = GameObject.Instantiate(dustParticlePrefab).transform;
                    dustParticleInstance.SetParent(transform);
                    dustParticleInstance.position = dustParticlePosition?.position ?? Vector3.zero;
                }
                else
                {
                    dustParticleInstance.gameObject.SetActive(false);
                    dustParticleInstance.gameObject.SetActive(true);
                }
            }
            if (playerTransform != null)
            {
                // モーターの現在位置を取得
                Vector3 motorPosition = transform.position;
                // 距離を計算
                float distance = Vector3.Distance(motorPosition, playerTransform.position);
                // 一定距離に近づいたら振動させる
                if (distance <= maxDistance)
                {
                    // 近いほど振動が強くなる（遠いと0、近いと1）
                    float intensity = Mathf.Clamp01(1f - (distance / maxDistance));

                    // 3D空間での音の出力
                    t3DSoundPlayer.PlaySound("footstep", intensity);
                }
                poltergeistViewModel.SetOnActionPoltergeistPosition(motorPosition);
            }
        }

        /// <summary>
        /// AnimarionClipからトリガー（アクション）の受信
        /// </summary>
        /// <see cref="Assets/Mains/Animations/Poltergeists/Poltergeist.controller"/>
        public void OnDummy() { }

        /// <summary>
        /// 浮かせるアニメーション処理を呼び出す
        /// </summary>
        /// <returns>コルーチン</returns>
        /// <remarks>クラゲの様にふわふわ空中に漂うDOTweenアニメーション</remarks>
        public IEnumerator DoPlayFloaterAnimation()
        {
            if (_transform != null &&
                _script_XyloApi != null)
            {
                var fromPosition = _transform.position;
                _onTempoSetDisposable = _script_XyloApi.IsOnTempoMethodEventAny.Subscribe(_ =>
                {
                    PlayFloaterAnimation(_transform, _script_XyloApi.BasicBeat, fromPosition);
                })
                    .AddTo(ref _disposableBag);
            }

            yield return null;
        }

        /// <summary>
        /// 浮かせるアニメーション処理
        /// </summary>
        /// <param name="trans">トランスフォーム</param>
        /// <param name="basicBeat">BasicBeat</param>
        /// <param name="fromPosition">ポジション初期値</param>
        private void PlayFloaterAnimation(Transform trans, float basicBeat, Vector3 fromPosition)
        {
            trans.position = fromPosition; // 毎回初期化（必要なら）
            // 上下にふわふわ移動（ワールドY軸方向）
            trans.DOMove(trans.position + Vector3.up * 0.5f, basicBeat / 2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// 浮かせるアニメーション処理を停止を呼び出す
        /// </summary>
        public void DoStopFloaterAnimation()
        {
            StopFloaterAnimation(_transform, _initialLocalPosition, _initialLocalEulerAngles, _onTempoSetDisposable);
        }

        /// <summary>
        /// 浮かせるアニメーション処理を停止
        /// </summary>
        /// <param name="trans">トランスフォーム</param>
        /// <param name="initialLocalPosition">ローカルポジション初期値</param>
        /// <param name="initialLocalEulerAngles">ローカルオイラー角度初期値</param>
        /// <param name="onTempoSetDisposable">浮かせるアニメーション処理の監視</param>
        private void StopFloaterAnimation(Transform trans, Vector3 initialLocalPosition, Vector3 initialLocalEulerAngles, System.IDisposable onTempoSetDisposable)
        {
            // 既にループが無効なら return
            if (onTempoSetDisposable == null)
                return;

            // PlayFloaterAnimation / AnimateRandomTiltLoop で発生した Tween をすべて停止
            if (trans != null)
            {
                // この Transform に紐づく全 Tween を停止・削除
                trans.DOKill(complete: true);

                trans.localPosition = initialLocalPosition;

                // 傾きを初期角度に戻す（必要に応じて）
                trans.localEulerAngles = initialLocalEulerAngles;
            }
            onTempoSetDisposable.Dispose();
            onTempoSetDisposable = null;
        }
    }
}
