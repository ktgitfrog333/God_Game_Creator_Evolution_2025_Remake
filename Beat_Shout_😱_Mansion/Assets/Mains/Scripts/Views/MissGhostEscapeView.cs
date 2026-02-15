using DG.Tweening;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// 移動用オバケのビュー
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class MissGhostEscapeView : MonoBehaviour
    {
        /// <summary>移動用オバケの設定</summary>
        [SerializeField] private MissGhostEscapeSettings settings;
        /// <summary>逃げるアニメーション再生が完了した</summary>
        private ReactiveCommand<bool> _isEscapeCompleted = new ReactiveCommand<bool>();
        /// <summary>逃げるアニメーション再生が完了した</summary>
        public ReactiveCommand<bool> IsEscapeCompleted => _isEscapeCompleted;
        /// <summary>トランスフォーム</summary>
        private Transform _transform;
        /// <summary>前進するDOTweenアニメーション</summary>
        private Tweener _moveFowardTweener;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            settings.animator = GetComponent<Animator>();
        }

        private void Start()
        {
            var trans = transform;
            // 位置（子）
            Vector3 childPosition = Vector3.zero;
            // 角度（子）
            Vector3 childEulerAngles = Vector3.zero;
            // 大きさ（子）
            Vector3 childScale = Vector3.zero;
            foreach (Transform child in trans)
            {
                if (child.name.Equals("GhostBodyModel"))
                {
                    childPosition = child.position;
                    childEulerAngles = child.eulerAngles;
                    childScale = child.localScale;
                }
            }
            // 位置
            Vector3 position = trans.position;
            _transform = trans;
            this.OnEnableAsObservable()
                .Subscribe(_ =>
                {
                    _isEscapeCompleted.Execute(false);
                })
                .AddTo(ref _disposableBag);
            this.OnDisableAsObservable()
                .Subscribe(_ =>
                {
                    ResetStatus(childPosition, childEulerAngles, childScale, _moveFowardTweener, position, _transform);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// 生成アニメーション再生が完了した
        /// </summary>
        /// <see cref="Assets/Mains/Animations/MissGhostEscapes/Spawn.anim"/>
        public void OnSpawnCompleted()
        {
            var set = settings;
            var trans = _transform;
            // 前進するDOTweenアニメーションを再生。このタイミングでアニメーションのトリガーを有効にする
            _moveFowardTweener = trans.DOLocalMoveZ(1f * set.distance, set.duration)
                .OnComplete(() =>
                {
                    set.animator.SetTrigger("Escape");
                });
        }

        /// <summary>
        /// 逃げるアニメーション再生が完了した
        /// </summary>
        /// <see cref="Assets/Mains/Animations/MissGhostEscapes/Escape.anim"/>
        public void OnEscapeCompleted()
        {
            _isEscapeCompleted.Execute(true);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 状態をリセット
        /// </summary>
        /// <param name="childPosition">位置（子）</param>
        /// <param name="childEulerAngles">角度（子）</param>
        /// <param name="childScale">大きさ（子）</param>
        /// <param name="moveFowardTweener">前進するDOTweenアニメーション</param>
        /// <param name="position">位置</param>
        /// <param name="transform">トランスフォーム</param>
        private void ResetStatus(Vector3 childPosition, Vector3 childEulerAngles, Vector3 childScale, Tweener moveFowardTweener, Vector3 position, Transform transform)
        {
            var trans = transform;
            foreach (Transform child in trans)
            {
                if (child.name.Equals("GhostBodyModel"))
                {
                    if (child.position != childPosition)
                        child.position = childPosition;
                    if (child.eulerAngles != childEulerAngles)
                        child.eulerAngles = childEulerAngles;
                    if (child.localScale != childScale)
                        child.localScale = childScale;
                }
            }
            if (moveFowardTweener != null && moveFowardTweener.IsActive())
            {
                moveFowardTweener.Kill();
                moveFowardTweener = null;
            }
            if (trans.position != position)
                trans.position = position;
        }
    }

    /// <summary>
    /// 移動用オバケの設定
    /// </summary>
    [System.Serializable]
    public class MissGhostEscapeSettings
    {
        /// <summary>アニメーション終了時間</summary>
        public float duration;
        /// <summary>移動距離</summary>
        public float distance;
        /// <summary>アニメータ</summary>
        public Animator animator;
    }
}
