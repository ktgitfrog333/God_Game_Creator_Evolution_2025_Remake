using Mains.Commons;
using Mains.ViewModels;
using R3;
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
        // TODO:シャウトチャンスパートでポルターガイストが発生した時のエフェクト
        [SerializeField] private GameObject dustParticlePrefab;
        /// <summary>ポルターガイストのビューモデル</summary>
        private PoltergeistViewModel _poltergeistViewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>ポルターガイストが有効か</summary>
        public bool IsEnabledPoltergeist { get; set; }
        /// <summary>トランスフォーム</summary>
        private Transform _transform;
        /// <summary>アクション発火の監視</summary>
        private ReactiveCommand<bool> _onAction = new ReactiveCommand<bool>();
        /// <summary>初期回転値</summary>
        private Quaternion _initialRotation;
        /// <summary>角度しきい値（°）</summary>
        [SerializeField] private float tiltThreshold;

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
            Transform dustParticleInstance = null;
            _poltergeistViewModel = new PoltergeistViewModel(poltergeistTable);
            // オブジェクトプールビュー
            var objectsPoolView = GameObject.FindAnyObjectByType<ObjectsPoolView>();
            if (objectsPoolView == null)
            {
                GameObject gameObject = new GameObject($"{typeof(ObjectsPoolView).Name}");
                objectsPoolView = gameObject.AddComponent<ObjectsPoolView>();
            }
            Transform t3DSoundPlayer = null;
            try
            {
                t3DSoundPlayer = objectsPoolView.Get3DSoundPlayer();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
            _onAction.Where(x => x)
                .Subscribe(_ =>
                {
                    if (dustParticleInstance == null)
                    {
                        dustParticleInstance = GameObject.Instantiate(dustParticlePrefab, _transform.position, Quaternion.identity).transform;
                        dustParticleInstance.SetParent(_transform);
                    }
                    else
                    {
                        dustParticleInstance.gameObject.SetActive(false);
                        dustParticleInstance.gameObject.SetActive(true);
                    }
                    // 3D空間での音の出力
                    //t3DSoundPlayer.Play?();
                    // 実行後はリセットする
                    _onAction.Execute(false);
                })
                .AddTo(ref _disposableBag);
            _initialRotation = _transform.rotation;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
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
    }
}
