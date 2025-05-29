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
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            Script_xyloApi script_XyloApi = new Script_xyloApi();
            Transform trans = transform;
            script_XyloApi.SetMissileDirectAnimManagerB(trans);
            var player = ReInput.players.GetPlayer(0);
            MissileDirectAnimManagerBCustomizeInputView inputView = null;
            Transform mainCamera = Camera.main.transform;
            MissileDirectAnimManagerBCustomizeViewModel viewModel = new MissileDirectAnimManagerBCustomizeViewModel();
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    // プールに戻す処理が開始されている場合は更新しない
                    if (script_XyloApi.IsReturningToPool || script_XyloApi.IsForceReturning) return;

                    script_XyloApi.CheckUpdateUIPosition();
                    bool isOverUIOfLine = IsOverUIOfAngleLine(viewModel.TargetCrossAnchoredPosition, script_XyloApi.NoteTransform as RectTransform, 接触判定の範囲);

                    if (player.GetButtonDown("TapLight"))
                    {
                        bool isOverUI = script_XyloApi.IsPointerOverUI;
                        // UI上でのクリックの場合、ここでも直接処理してみる（緊急対応）
                        if (isOverUIOfLine && isOverUI && script_XyloApi.EnableClickDetection && !script_XyloApi.IsFailed && !script_XyloApi.IsSuccessful)
                        {
                            float elapsedTime = Time.time - script_XyloApi.ObjectCreationTime;
                            // 直接タイミング判定して処理
                            script_XyloApi.ProcessClick(elapsedTime);
                        }
                    }
                    // 2. 離した瞬間の判定（フレーム内で1回だけtrue）
                    if (player.GetButtonUp("TapLight"))
                    {
                        // 長押し終了やHoldノーツ解除判定などに使う
                        bool isOverUI = script_XyloApi.IsPointerOverUI;
                        // UI上でのリリースで、かつ長押しノーツの場合
                        if (isOverUIOfLine && isOverUI && script_XyloApi.EnableClickDetection && !script_XyloApi.IsFailed && !script_XyloApi.IsSuccessful &&
                            (script_XyloApi.NoteType == 2 ||
                                script_XyloApi.NoteType == 3 ||
                                script_XyloApi.NoteType == 4))
                        {
                            float elapsedTime = Time.time - script_XyloApi.ObjectCreationTime;

                            // 入力マネージャーにリリース処理を委任
                            if (script_XyloApi.IsLongPressStarted)
                            {
                                script_XyloApi.HandleLongPressRelease(elapsedTime);
                            }
                        }
                    }
                    MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct = new MissileDirectAnimCustomizeStruct()
                    {
                        isGoodStickDirection = isOverUIOfLine,
                        transform = script_XyloApi.NoteTransform,
                    };
                    viewModel.AddMissileDirectAnimCustomizeStructs(missileDirectAnimCustomizeStruct);
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
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
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
