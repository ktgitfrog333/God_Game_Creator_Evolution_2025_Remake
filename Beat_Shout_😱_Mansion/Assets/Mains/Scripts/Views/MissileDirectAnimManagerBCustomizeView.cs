using Mains.Commons;
using Mains.External;
using R3;
using Rewired;
using UnityEngine;
using Mains.ViewModels;

namespace Mains.Views
{
    /// <summary>
    /// MissileDirectAnimManagerBのカスタマイズビュー
    /// </summary>
    public class MissileDirectAnimManagerBCustomizeView : MonoBehaviour
    {
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/MissileDirectAnimManagerBCustomizeInput.prefab をアタッチしておく")]
        [SerializeField] private Transform missileDirectAnimManagerBCustomizeInputPrefab;
        [SerializeField] private float 角度の許容範囲;
        [SerializeField] private float 接触判定の範囲;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>MissileDirectAnimManagerBのカスタマイズビューモデル</summary>
        private MissileDirectAnimManagerBCustomizeViewModel _viewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Awake()
        {
            _viewModel = new MissileDirectAnimManagerBCustomizeViewModel();
            _script_XyloApi = new Script_xyloApi();
            _script_XyloApi.SetMissileDirectAnimManagerB(transform);
        }

        private void OnEnable()
        {
            MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct = new MissileDirectAnimCustomizeStruct()
            {
                transform = _script_XyloApi.NoteTransform,
                onEnabledTime = Time.time,
            };
            _viewModel.AddOrSetOnEnabledTime(missileDirectAnimCustomizeStruct);
        }

        private void Start()
        {
            Transform trans = transform;
            var player = ReInput.players.GetPlayer(0);
            MissileDirectAnimManagerBCustomizeInputView inputView = null;
            Transform mainCamera = Camera.main.transform;
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    // プールに戻す処理が開始されている場合は更新しない
                    if (_script_XyloApi.IsReturningToPool || _script_XyloApi.IsForceReturning) return;

                    _script_XyloApi.CheckUpdateUIPosition();
                    bool isOverUIOfLine = IsOverUIOfAngleLine(_viewModel.TargetCrossAnchoredPosition, _script_XyloApi.NoteTransform as RectTransform, 接触判定の範囲);

                    if (player.GetButtonDown("TapLight"))
                    {
                        bool isOverUI = _script_XyloApi.IsPointerOverUI;
                        // UI上でのクリックの場合、ここでも直接処理してみる（緊急対応）
                        if (isOverUIOfLine && isOverUI && _script_XyloApi.EnableClickDetection && !_script_XyloApi.IsFailed && !_script_XyloApi.IsSuccessful)
                        {
                            float elapsedTime = Time.time - _script_XyloApi.ObjectCreationTime;
                            // 直接タイミング判定して処理
                            _script_XyloApi.ProcessClick(elapsedTime);
                        }
                    }
                    // 2. 離した瞬間の判定（フレーム内で1回だけtrue）
                    if (player.GetButtonUp("TapLight"))
                    {
                        // 長押し終了やHoldノーツ解除判定などに使う
                        bool isOverUI = _script_XyloApi.IsPointerOverUI;
                        // UI上でのリリースで、かつ長押しノーツの場合
                        if (isOverUIOfLine && isOverUI && _script_XyloApi.EnableClickDetection && !_script_XyloApi.IsFailed && !_script_XyloApi.IsSuccessful &&
                            (_script_XyloApi.NoteType == 2 ||
                                _script_XyloApi.NoteType == 3 ||
                                _script_XyloApi.NoteType == 4))
                        {
                            float elapsedTime = Time.time - _script_XyloApi.ObjectCreationTime;

                            // 入力マネージャーにリリース処理を委任
                            if (_script_XyloApi.IsLongPressStarted)
                            {
                                _script_XyloApi.HandleLongPressRelease(elapsedTime);
                            }
                        }
                    }
                    MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct = new MissileDirectAnimCustomizeStruct()
                    {
                        isGoodStickDirection = isOverUIOfLine,
                        transform = _script_XyloApi.NoteTransform,
                    };
                    _viewModel.AddMissileDirectAnimCustomizeStructs(missileDirectAnimCustomizeStruct);
                })
                .AddTo(ref _disposableBag);
            var findInputView = FindAnyObjectByType<MissileDirectAnimManagerBCustomizeInputView>();
            if (findInputView == null)
            {
                Transform instanced = Instantiate(missileDirectAnimManagerBCustomizeInputPrefab);
                inputView = instanced.GetComponent<MissileDirectAnimManagerBCustomizeInputView>();
            }
            else
            {
                inputView = findInputView;
            }
            Observable.EveryUpdate()
                .Select(_ => _viewModel.IsFrontMissileDirectAnim(_script_XyloApi.NoteTransform))
                .Subscribe(isFrontMissileDirectAnim =>
                {
                    _script_XyloApi.SetEnableClickDetection(isFrontMissileDirectAnim);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDisable()
        {
            MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct = new MissileDirectAnimCustomizeStruct()
            {
                transform = _script_XyloApi.NoteTransform,
                onEnabledTime = 0f,
            };
            _viewModel.AddOrSetOnEnabledTime(missileDirectAnimCustomizeStruct);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _script_XyloApi?.Dispose();
            _viewModel?.Dispose();
        }

        /// <summary>
        /// 中心点からある角度へ向かって伸びるラインがUIの上にあるか判定
        /// </summary>
        /// <param name="targetCrossAnchoredPosition">ターゲットクロスアンカー位置</param>
        /// <param name="noteTransform">ノーツのトランスフォーム</param>
        /// <param name="scopeRange">接触判定の範囲</param>
        /// <returns>UIの上にあるか</returns>
        private bool IsOverUIOfAngleLine(Vector2 targetCrossAnchoredPosition, RectTransform noteTransform, float scopeRange)
        {
            if (noteTransform == null)
                return false;

            // noteTransform の anchoredPosition（UI空間内）を取得
            Vector2 noteAnchoredPos = noteTransform.anchoredPosition;

            // 中心 (0,0) から targetCrossAnchoredPosition の角度を算出（反時計回りで0度は右）
            float targetAngle = Mathf.Atan2(targetCrossAnchoredPosition.y, targetCrossAnchoredPosition.x) * Mathf.Rad2Deg;

            // 同様に noteTransform の角度を算出
            float noteAngle = Mathf.Atan2(noteAnchoredPos.y, noteAnchoredPos.x) * Mathf.Rad2Deg;

            // 角度差を求めて、360度をまたいでも正しく比較できるようにする
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(targetAngle, noteAngle));

            return angleDiff <= scopeRange;
        }
    }
}
