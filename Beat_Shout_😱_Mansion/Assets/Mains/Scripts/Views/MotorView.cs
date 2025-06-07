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
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

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
            Se_3D_Picker t3DSoundPlayer = objectsPoolView.Get3DSoundPlayer();
            Observable.EveryUpdate()
                .Select(_ => _poltergeistViewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Subscribe(x =>
                    {
                        switch (x)
                        {
                            case InteractionPart.Search:
                            case InteractionPart.ShoutChance:
                                // 探索パートとシャウトチャンスパートはタップを有効
                                animator.SetBool("Tap", true);

                                break;
                            case InteractionPart.Rhythm:
                                // リズムパートはタップを無効
                                animator.SetBool("Tap", false);

                                break;
                        }
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            _onAction.Where(x => x)
                .Subscribe(_ =>
                {
                    if (dustParticleInstance == null)
                    {
                        dustParticleInstance = GameObject.Instantiate(dustParticlePrefab).transform;
                        dustParticleInstance.SetParent(_transform);
                        dustParticleInstance.localPosition = Vector3.zero;
                    }
                    else
                    {
                        dustParticleInstance.gameObject.SetActive(false);
                        dustParticleInstance.gameObject.SetActive(true);
                    }
                    if (_poltergeistViewModel.PlayerTransform != null)
                    {
                        // モーターの現在位置を取得
                        Vector3 playerPosition = _transform.position;
                        // 距離を計算
                        float distance = Vector3.Distance(playerPosition, _poltergeistViewModel.PlayerTransform.position);
                        // 一定距離に近づいたら振動させる
                        if (distance <= maxDistance)
                        {
                            // 近いほど振動が強くなる（遠いと0、近いと1）
                            float intensity = Mathf.Clamp01(1f - (distance / maxDistance));

                            // 3D空間での音の出力
                            t3DSoundPlayer.PlaySound("footstep", intensity);
                        }
                    }
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
            if (!IsEnabledPoltergeist ||
                _transform == null)
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

            var angle = Quaternion.Angle(_initialRotation, _transform.rotation);
            if (tiltThreshold < angle)
            {
                _poltergeistViewModel.SetOnActionPoltergeistPosition(_transform.position);
                // アクション実行を通知
                _onAction.Execute(true);
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
