using DG.Tweening;
using R3;
using Titles.ViewModels;
using UnityEngine;

namespace Titles.Views
{
    /// <summary>
    /// セットアップガイドパネルのビュー
    /// </summary>
    public class SetupGuidePanelView : MonoBehaviour
    {
        /// <summary>セットアップガイドパネルのビューモデル</summary>
        [SerializeField] private SetupGuidePanelViewModel viewModel;
        /// <summary>セットアップガイドパネルの設定</summary>
        [SerializeField] private SetupGuidePanelSettings settings;
        /// <summary>下から開始の上下移動アニメーション</summary>
        private Tweener _downUpAnimation;
        /// <summary>下から開始の上下移動アニメーション移動元の座標</summary>
        private Vector2 _downUpAnimationFromPosition;
        /// <summary>目的位置までの移動アニメーション</summary>
        private Tweener _movementAnimation;
        /// <summary>拡大のアニメーション</summary>
        private Tweener _scaleUpAnimation;
        /// <summary>目的位置に瞬間移動してフェード点滅アニメーション</summary>
        private Sequence _moveAndFadeAnimation;
        /// <summary>初期化済みフラグ</summary>
        private readonly ReactiveProperty<bool> _isCompleted = new ReactiveProperty<bool>();
        /// <summary>初期化済みフラグ</summary>
        public ReadOnlyReactiveProperty<bool> IsCompleted => _isCompleted;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            var set = settings;
            foreach (Transform child in transform)
            {
                if (child.name.Equals("SetupGuideText"))
                {
                    if (set.setupGuideTextTrans == null)
                        set.setupGuideTextTrans = child as RectTransform;
                }
                if (child.name.Equals("GoodText"))
                {
                    if (set.goodTextTrans == null)
                        set.goodTextTrans = child as RectTransform;
                }
                if (child.name.Equals("HighlightImage"))
                {
                    if (set.highlightImageTrans == null)
                        set.highlightImageTrans = child as RectTransform;
                    if (set.highlightImageCanvasGroup == null)
                        set.highlightImageCanvasGroup = child.GetComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            var set = settings;
            var vm = viewModel;

            vm.Initialize();

            ReactiveProperty<bool> isOnly = new ReactiveProperty<bool>();
            vm.IsActiveAdminMode.Subscribe(isActiveAdminMode =>
                {
                    Vector2 toPosition = new Vector2();
                    float toWidth = 0f;
                    float toHeight = 0f;
                    Vector2 toFloatPosition = new Vector2();
                    var goodTextTrans = set.goodTextTrans;

                    if (isActiveAdminMode)
                    {
                        toPosition = set.StartButtonPosition;
                        var scaleUpAnimation = _scaleUpAnimation;
                        if (scaleUpAnimation != null && scaleUpAnimation.IsPlaying())
                        {
                            scaleUpAnimation.Kill();
                        }
                        goodTextTrans.gameObject.SetActive(true);
                        scaleUpAnimation = PlayScaleUpAnimation(vm.ScaleUpAnimationDuration, goodTextTrans);

                        _scaleUpAnimation = scaleUpAnimation;

                        var startButtonRect = set.StartButtonRect;
                        toWidth = startButtonRect.width;
                        toHeight = startButtonRect.height;
                    }
                    else
                    {
                        toPosition = set.UseMicDowndownPosition;
                        // 2回目以降はオブジェクトが有効なままなので一度状態をリセットする
                        if (goodTextTrans.gameObject.activeSelf)
                        {
                            goodTextTrans.gameObject.SetActive(false);
                            Vector3 originScale = vm.ScaleUpAnimationFromScale;
                            goodTextTrans.transform.localScale = originScale;
                        }
                        var useMicDowndownRect = set.UseMicDowndownRect;
                        toWidth = useMicDowndownRect.width;
                        toHeight = useMicDowndownRect.height;
                    }
                    toFloatPosition = toPosition + Vector2.up * vm.AnimationOffsetUpDistance;

                    var downUpAnimation = _downUpAnimation;
                    var movementAnimation = _movementAnimation;
                    var moveAndFadeAnimation = _moveAndFadeAnimation;
                    var setupGuideTextTrans = set.setupGuideTextTrans;
                    var highlightImageCanvasGroup = set.highlightImageCanvasGroup;

                    if (downUpAnimation != null && downUpAnimation.IsPlaying())
                    {
                        downUpAnimation.Kill();
                        // 初期位置（from）へ戻す
                        setupGuideTextTrans.anchoredPosition = _downUpAnimationFromPosition;
                    }
                    if (movementAnimation != null && movementAnimation.IsPlaying())
                    {
                        movementAnimation.Kill();
                    }
                    if (moveAndFadeAnimation != null && moveAndFadeAnimation.IsPlaying())
                    {
                        moveAndFadeAnimation.Kill();
                        // アルファ値を初期値へ戻す
                        highlightImageCanvasGroup.alpha = 0f;
                    }

                    movementAnimation = PlayMovementAnimation(isOnly.Value ? vm.MovementAnimationDuration : 0f, toFloatPosition, setupGuideTextTrans)
                        .OnComplete(() =>
                        {
                            var fromPosition = toFloatPosition;
                            _downUpAnimationFromPosition = fromPosition;
                            var toDownPosition = fromPosition + Vector2.down * vm.DownUpAnimationDistance;
                            var downUpAnimation = _downUpAnimation;

                            downUpAnimation = PlayDownUpAnimation(vm.DownUpAnimationDuration, fromPosition, toDownPosition, setupGuideTextTrans);
                            moveAndFadeAnimation = PlayMoveAndFadeAnimation(vm.MoveAndFadeAnimationDuration, toPosition, toWidth, toHeight,
                                set.highlightImageTrans, highlightImageCanvasGroup);

                            _downUpAnimation = downUpAnimation;
                            _moveAndFadeAnimation = moveAndFadeAnimation;
                        });

                    _movementAnimation = movementAnimation;

                    if (!isOnly.Value)
                        isOnly.Value = true;
                })
                .AddTo(ref _disposableBag);

            var completedSceneLoadDirectionType = vm.CompletedSceneLoadDirectionType;
            // シーンのクローズ中はインタラクトを無効にする
            completedSceneLoadDirectionType.Where(x => x == 2 || x == 3)
                .Subscribe(_ =>
                {
                    var downUpAnimation = _downUpAnimation;
                    var movementAnimation = _movementAnimation;
                    var moveAndFadeAnimation = _moveAndFadeAnimation;
                    var setupGuideTextTrans = set.setupGuideTextTrans;
                    var highlightImageCanvasGroup = set.highlightImageCanvasGroup;

                    if (downUpAnimation != null && downUpAnimation.IsPlaying())
                    {
                        downUpAnimation.Kill();
                        // 初期位置（from）へ戻す
                        setupGuideTextTrans.anchoredPosition = _downUpAnimationFromPosition;
                    }
                    if (movementAnimation != null && movementAnimation.IsPlaying())
                    {
                        movementAnimation.Kill();
                    }
                    if (moveAndFadeAnimation != null && moveAndFadeAnimation.IsPlaying())
                    {
                        moveAndFadeAnimation.Kill();
                        // アルファ値を初期値へ戻す
                        highlightImageCanvasGroup.alpha = 0f;
                    }

                    setupGuideTextTrans.gameObject.SetActive(false);
                })
                .AddTo(ref _disposableBag);

            _isCompleted.Value = true;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            var vm = viewModel;
            vm.Dispose();
        }

        /// <summary>
        /// 下から開始の上下移動アニメーションを再生
        /// </summary>
        /// <param name="duration">アニメーション終了時間</param>
        /// <param name="fromPosition">移動元の座標</param>
        /// <param name="toPosition">移動先の座標</param>
        /// <param name="setupGuideTextTrans">セットアップガイドのトランスフォーム</param>
        /// <returns>トゥイナー</returns>
        private Tweener PlayDownUpAnimation(float duration, Vector2 fromPosition, Vector2 toPosition, RectTransform setupGuideTextTrans)
        {
            // まず fromPosition に瞬間移動（開始位置）
            setupGuideTextTrans.anchoredPosition = fromPosition;

            // fromPosition → toPosition への往復アニメーション（Yoyo）
            Tweener tweener = setupGuideTextTrans
                .DOAnchorPos(toPosition, duration)
                .SetEase(Ease.InOutSine)     // 滑らかな往復
                .SetLoops(-1, LoopType.Yoyo) // 無限ループ（往復）
                .SetAutoKill(false);          // 再生後もトゥイナーを保持

            return tweener;
        }

        /// <summary>
        /// 目的位置までの移動アニメーションを再生
        /// </summary>
        /// <param name="duration">アニメーション終了時間</param>
        /// <param name="toPosition">移動先の座標</param>
        /// <param name="setupGuideTextTrans">セットアップガイドのトランスフォーム</param>
        /// <returns>トゥイナー</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private Tweener PlayMovementAnimation(float duration, Vector2 toPosition, RectTransform setupGuideTextTrans)
        {
            // 現在位置から toPosition まで移動
            Tweener tweener = setupGuideTextTrans
                .DOAnchorPos(toPosition, duration)
                .SetEase(Ease.OutQuad)      // 滑らかに減速
                .SetAutoKill(false);         // 再生後もトゥイナーを保持（再利用可能に）

            return tweener;
        }

        /// <summary>
        /// 拡大のアニメーションを再生
        /// </summary>
        /// <param name="duration">アニメーション終了時間</param>
        /// <param name="goodTextTrans">GOODテキストのトランスフォーム</param>
        /// <returns>トゥイナー</returns>
        private Tweener PlayScaleUpAnimation(float duration, RectTransform goodTextTrans)
        {
            // Tweener tweener = goodTextTrans.DOScale(1.2f, 0.3f)
            Tweener tweener = goodTextTrans.DOScale(1f, duration)
                .SetEase(Ease.OutBack)
                .SetAutoKill(false);         // 再生後もトゥイナーを保持（再利用可能に）

            return tweener;
        }

        /// <summary>
        /// 目的位置に瞬間移動してフェード点滅アニメーションを再生
        /// </summary>
        /// <param name="duration">アニメーション終了時間</param>
        /// <param name="toPosition">移動先の座標</param>
        /// <param name="toWidth">変更後の幅</param>
        /// <param name="toHeight">変更後の高さ</param>
        /// <param name="highlightImageTrans">ハイライトイメージのトランスフォーム</param>
        /// <param name="highlightImageCanvasGroup">ハイライトイメージのキャンバスグループ</param>
        /// <returns>シーケンス</returns>
        private Sequence PlayMoveAndFadeAnimation(float duration, Vector2 toPosition, float toWidth, float toHeight,
            RectTransform highlightImageTrans, CanvasGroup highlightImageCanvasGroup)
        {
            // 1. 指定された位置に瞬間移動
            highlightImageTrans.anchoredPosition = toPosition;

            // 2. 指定されたサイズに変更
            highlightImageTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, toWidth);
            highlightImageTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, toHeight);

            // 3. アルファ値を 0 にリセット（非表示）
            highlightImageCanvasGroup.alpha = 0f;

            // 4. フェードイン → フェードアウト のループアニメーション（点滅）
            // シーケンスを作成して制御
            Sequence sequence = DOTween.Sequence();

            // フェードイン（0 → 1）
            Tween fadeIn = highlightImageCanvasGroup
                .DOFade(1f, duration)
                .SetEase(Ease.InOutSine);

            // フェードアウト（1 → 0）
            Tween fadeOut = highlightImageCanvasGroup
                .DOFade(0f, duration)
                .SetEase(Ease.InOutSine);

            // シーケンスに追加（フェードイン → フェードアウト の繰り返し）
            sequence
                .Append(fadeIn)
                .Append(fadeOut)
                .SetLoops(-1, LoopType.Restart)  // 無限ループ
                .SetAutoKill(false);              // 再生後も保持

            return sequence;
        }
    }

    /// <summary>
    /// セットアップガイドパネルの設定
    /// </summary>
    [System.Serializable]
    public class SetupGuidePanelSettings
    {
        /// <summary>セットアップガイドのトランスフォーム</summary>
        public RectTransform setupGuideTextTrans;
        /// <summary>GOODテキストのトランスフォーム</summary>
        public RectTransform goodTextTrans;
        /// <summary>ハイライトイメージのトランスフォーム</summary>
        public RectTransform highlightImageTrans;
        /// <summary>ハイライトイメージのキャンバスグループ</summary>
        public CanvasGroup highlightImageCanvasGroup;
        /// <summary>使用するマイクドロップダウンのトランスフォーム</summary>
        [SerializeField] private RectTransform useMicDowndownTrans;
        /// <summary>使用するマイクドロップダウンの位置</summary>
        public Vector2 UseMicDowndownPosition => useMicDowndownTrans.anchoredPosition;
        /// <summary>使用するマイクドロップダウンのレクト</summary>
        public Rect UseMicDowndownRect => useMicDowndownTrans.rect;
        /// <summary>コマンドボタンのトランスフォーム</summary>
        [SerializeField] private RectTransform startButtonTrans;
        /// <summary>コマンドボタンの位置</summary>
        public Vector2 StartButtonPosition => startButtonTrans.anchoredPosition;
        /// <summary>コマンドボタンのレクト</summary>
        public Rect StartButtonRect => startButtonTrans.rect;
    }
}
