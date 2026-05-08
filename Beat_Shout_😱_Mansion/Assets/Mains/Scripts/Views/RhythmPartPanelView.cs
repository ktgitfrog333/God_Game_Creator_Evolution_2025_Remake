using Mains.ViewModels;
using R3;
using Rewired;
using UnityEngine;
using Mains.Commons;
using System.Linq;
using UnityEngine.UI;

namespace Mains.Views
{
    /// <summary>
    /// リズムパートパネルのビュー
    /// </summary>
    public class RhythmPartPanelView : MonoBehaviour
    {
        /// <summary>中央パネル</summary>
        [SerializeField] private RectTransform centerPanel;
        /// <summary>リズムパートパネルの設定</summary>
        [SerializeField] private RhythmPartPanelSettings settings;
        /// <summary>リズムパートパネルのビューモデル</summary>
        private RhythmPartPanelViewModel _rhythmPartPanelViewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        [Tooltip("Xbox360コントローラーのみ対象\n値の高さ＝入力感度")]
        [SerializeField] private float ポインター移動距離;

        private void Reset()
        {
            if (centerPanel == null)
                centerPanel = transform.GetChild(0) as RectTransform;
            foreach (Transform child in transform)
            {
                var set = settings;
                if (child.name.Equals("HeaderPanel"))
                {
                    if (set.headerPanel == null)
                        set.headerPanel = child as RectTransform;
                    foreach (Transform item in child)
                    {
                        if (item.name.Equals("MidbossQuotaGaugePanel"))
                        {
                            if (set.midbossQuotaGaugePanel == null)
                                set.midbossQuotaGaugePanel = item as RectTransform;
                            foreach (Transform item1 in item)
                            {
                                if (item1.name.Equals("MidbossQuotaGaugeFillImage"))
                                {
                                    if (set.midbossQuotaGaugeFillImage == null)
                                        set.midbossQuotaGaugeFillImage = item1.GetComponent<Image>();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Start()
        {
            _rhythmPartPanelViewModel = new();
            var player = ReInput.players.GetPlayer(0);
            var viewModel = _rhythmPartPanelViewModel;
            var set = settings;
            // リズムパート用のUI表示切り替え
            Observable.EveryUpdate()
                .Select(_ => _rhythmPartPanelViewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    // Rewiredによる入力管理
                    // Assets/Universal/Scripts/Prefabs/Rewired Input Manager.prefab
                    // プレイヤーへキーボードやコントローラー操作を割り当てる場合は
                    // 当該プレハブから実施すること（シーンからの変更は適用されない）
                    RectTransform targetCrossImage = centerPanel.GetChild(0) as RectTransform;
                    System.IDisposable targetCrossDisposable = null;
                    System.IDisposable midBosskillsRateDisposable = null;
                    // ゲージUIイメージのマテリアル
                    Material runtimeMaterial = Instantiate(set.midbossQuotaGaugeFillImage.material);
                    set.midbossQuotaGaugeFillImage.material = runtimeMaterial;
                    x.Subscribe(interactionPart =>
                    {
                        switch (interactionPart)
                        {
                            case Commons.InteractionPart.Rhythm:
                                centerPanel.gameObject.SetActive(true);
                                set.headerPanel.gameObject.SetActive(true);
                                targetCrossDisposable?.Dispose();
                                midBosskillsRateDisposable?.Dispose();
                                int layerMaskTerrainObjects = 1 << LayerMask.NameToLayer("TerrainObjects");
                                int layerMaskDropItems = 1 << LayerMask.NameToLayer("DropItems");
                                int layerMaskGhost = 1 << LayerMask.NameToLayer("Ghost");
                                Camera mainCamera = Camera.main;
                                // マウスポインターへ追従
                                targetCrossDisposable = Observable.EveryUpdate()
                                    .Select(_ =>
                                    {
                                        bool isDefaultMapEnabled =
                                            player.controllers.maps.GetAllMapsInCategory("Default")
                                                  .Any(m => m.enabled);

                                        return isDefaultMapEnabled;
                                    })
                                    .Where(x => x)
                                    .Subscribe(_ =>
                                    {
                                        Vector2 mousePos = Input.mousePosition;
                                        float moveX = player.GetAxis("RhythmMoveHorizontal");
                                        float moveZ = player.GetAxis("RhythmMoveVertical");
                                        bool isGetBattery = player.GetButtonDown("GetBattery");

                                        bool isInputMouse = Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f;
                                        bool isInputJoystick = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f || isGetBattery;
                                        Vector2 movePosition = isInputJoystick && !isInputMouse ? new Vector2(moveX, moveZ) : Vector2.zero;

                                        if (mainCamera == null)
                                            // ゲームオーバー時などカメラが取得できない場合の対策用
                                            return;

                                        // 入力排他制御：同一フレーム内でマウスとジョイスティックが両方有効になることを防ぐ
                                        if (isInputMouse && !isInputJoystick)
                                        {
                                            // マウスで操作
                                            if (0 <= mousePos.x && mousePos.x <= Screen.width &&
                                                0 <= mousePos.y && mousePos.y <= Screen.height)
                                            {
                                                Vector2 localPoint;
                                                // ポインター位置（スクリーン座標）からワールド方向ベクトルを計算
                                                Ray ray = mainCamera.ScreenPointToRay(mousePos);

                                                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMaskTerrainObjects))
                                                {
                                                    Vector3 hitPoint = hit.point;
                                                    _rhythmPartPanelViewModel.SetTargetCrossPosition(hitPoint);

                                                    // デバッグ表示
                                                    Debug.DrawLine(ray.origin, hitPoint, Color.red);
                                                }
                                                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                                    centerPanel, mousePos, null, out localPoint);

                                                // 必要なら制限処理もここで
                                                targetCrossImage.anchoredPosition = localPoint;
                                                // 選択状態の電池を検知する
                                                if (Physics.Raycast(ray, out RaycastHit hit1, Mathf.Infinity, layerMaskDropItems))
                                                {
                                                    Transform hitTrans = hit1.transform;
                                                    _rhythmPartPanelViewModel.SetIsSelectedBattery(true);

                                                    // デバッグ表示
                                                    Debug.DrawLine(ray.origin, hitTrans.position, Color.red);
                                                }
                                                else
                                                {
                                                    _rhythmPartPanelViewModel.SetIsSelectedBattery(false);
                                                }
                                                // 選択状態のMissGhostAttackを検知する
                                                if (_rhythmPartPanelViewModel.BatteryTransform == null &&
                                                    Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, layerMaskGhost))
                                                {
                                                    Transform hitTrans = hit2.transform;
                                                    _rhythmPartPanelViewModel.SetSelectedMissGhostAttackTransform(hitTrans);

                                                    // デバッグ表示
                                                    Debug.DrawLine(ray.origin, hitTrans.position, Color.red);
                                                }
                                                else
                                                {
                                                    _rhythmPartPanelViewModel.SetSelectedMissGhostAttackTransform(null);
                                                }
                                            }
                                        }
                                        else if (isInputJoystick && !isInputMouse)
                                        {
                                            Vector2 calcPosition = movePosition * ポインター移動距離;
                                            _rhythmPartPanelViewModel.SetTargetCrossAnchoredPosition(calcPosition);

                                            Rect panelRect = centerPanel.rect;
                                            float halfWidth = targetCrossImage.rect.width * 0.5f;
                                            float halfHeight = targetCrossImage.rect.height * 0.5f;

                                            // UI内の移動制限
                                            calcPosition.x = Mathf.Clamp(calcPosition.x, panelRect.xMin + halfWidth, panelRect.xMax - halfWidth);
                                            calcPosition.y = Mathf.Clamp(calcPosition.y, panelRect.yMin + halfHeight, panelRect.yMax - halfHeight);
                                            targetCrossImage.anchoredPosition = calcPosition;

                                            Vector3 worldPosition = targetCrossImage.transform.TransformPoint(calcPosition);
                                            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
                                            // ポインター位置（スクリーン座標）からワールド方向ベクトルを計算
                                            Ray ray = mainCamera.ScreenPointToRay(screenPoint);

                                            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMaskTerrainObjects))
                                            {
                                                Vector3 hitPoint = hit.point;
                                                _rhythmPartPanelViewModel.SetTargetCrossPosition(hitPoint);
                                            }
                                            // Xbox360コントローラーはBボタンで電池を取得する
                                            if (isGetBattery)
                                            {
                                                _rhythmPartPanelViewModel.SetIsSelectedBattery(true);
                                                MissGhostAttackCustomizeView missGhostAttackCustomizeView = FindAnyObjectByType<MissGhostAttackCustomizeView>();
                                                if (missGhostAttackCustomizeView != null)
                                                {
	                                                Transform hitTrans = missGhostAttackCustomizeView.transform;
	                                                _rhythmPartPanelViewModel.SetSelectedMissGhostAttackTransform(hitTrans);
                                                }
                                            }
                                            else
                                            {
                                                _rhythmPartPanelViewModel.SetIsSelectedBattery(false);
                                                _rhythmPartPanelViewModel.SetSelectedMissGhostAttackTransform(null);
                                            }
                                        }
                                    })
                                    .AddTo(ref _disposableBag);
                                // 敵戦パート
                                var enemyBattlePart = viewModel.EnemyBattlePart;
                                // ゲージUIのパネル
                                var midbossQuotaGaugePanel = set.midbossQuotaGaugePanel;
                                switch (enemyBattlePart)
                                {
                                    case EnemyBattlePart.Normal:
                                        if (midbossQuotaGaugePanel.gameObject.activeSelf)
                                            midbossQuotaGaugePanel.gameObject.SetActive(false);

                                        break;
                                    case EnemyBattlePart.MidBoss:
                                        if (!midbossQuotaGaugePanel.gameObject.activeSelf)
                                            midbossQuotaGaugePanel.gameObject.SetActive(true);
                                        // 中ボスオバケ退治率を監視
                                        midBosskillsRateDisposable = viewModel.MidBosskillsRateReactive.Subscribe(midBosskillsRate =>
                                        {
                                            SetFloatFill(midBosskillsRate, runtimeMaterial);
                                        })
                                            .AddTo(ref _disposableBag);
                                        // 中ボスオバケ退治率
                                        var midBosskillsRate = viewModel.MidBosskillsRate;
                                        SetFloatFill(midBosskillsRate, runtimeMaterial);

                                        break;
                                }

                                break;
                            default:
                                centerPanel.gameObject.SetActive(false);
                                set.headerPanel.gameObject.SetActive(false);

                                break;
                        }
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _rhythmPartPanelViewModel?.Dispose();
        }

        /// <summary>
        /// Imageのマテリアル.SetFloat("_Fill", 値);を更新
        /// </summary>
        /// <param name="midBosskillsRate">中ボスオバケ退治率</param>
        /// <param name="runtimeMaterial">ゲージUIイメージのマテリアル</param>
        /// <see cref="Assets/Mains/Materials/UIs/GaugeMaterial.mat"/>
        private void SetFloatFill(float midBosskillsRate, Material runtimeMaterial)
        {
            runtimeMaterial.SetFloat("_Fill", midBosskillsRate);
        }
    }

    /// <summary>
    /// リズムパートパネルの設定
    /// </summary>
    [System.Serializable]
    public class RhythmPartPanelSettings
    {
        [Tooltip("RhythmPartPanel > HeaderPanel をセット")]
        /// <summary>ヘッダパネル</summary>
        public RectTransform headerPanel;
        /// <summary>ゲージUIのパネル</summary>
        public RectTransform midbossQuotaGaugePanel;
        /// <summary>ゲージUIのイメージ</summary>
        public Image midbossQuotaGaugeFillImage;
    }
}
