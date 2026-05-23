using Mains.Commons;
using Mains.Views;
using R3;
using Rewired;
using Selects.ViewModels;
using System.Linq;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// リズムパートパネルのビュー
    /// </summary>
    /// <remarks>をステージセレクト用に移植してきた版</remarks>
    /// <see cref="Mains.Views.RhythmPartPanelView"/>
    public class RhythmPartPanelCustomizeOfMainView : MonoBehaviour
    {
        /// <summary>リズムパートパネルの設定</summary>
        [SerializeField] private RhythmPartPanelCustomizeOfMainSettings settings;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            var set = settings;
            foreach (Transform child in transform)
            {
                if (child.name.Equals("CenterPanel"))
                {
                    if (set.centerPanel == null)
                        set.centerPanel = child as RectTransform;
                }
            }
        }

        private void Start()
        {
            var player = ReInput.players.GetPlayer(0);
            var set = settings;
            var viewModel = set.viewModel;
            viewModel.Initialize();
            viewModel.InteractionPart.Subscribe(interactionPart =>
            {
                // Rewiredによる入力管理
                // Assets/Universal/Scripts/Prefabs/Rewired Input Manager.prefab
                // プレイヤーへキーボードやコントローラー操作を割り当てる場合は
                // 当該プレハブから実施すること（シーンからの変更は適用されない）
                RectTransform targetCrossImage = set.centerPanel.GetChild(0) as RectTransform;
                System.IDisposable targetCrossDisposable = null;
                switch (interactionPart)
                {
                    case InteractionPart.Rhythm:
                        set.centerPanel.gameObject.SetActive(true);
                        targetCrossDisposable?.Dispose();
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
                                MoveTargetCross(mainCamera, player, layerMaskTerrainObjects, viewModel, set.centerPanel, targetCrossImage,
                                    layerMaskDropItems, layerMaskGhost, set.ポインター移動距離);
                            })
                            .AddTo(ref _disposableBag);

                        break;
                    default:
                        set.centerPanel.gameObject.SetActive(false);

                        break;
                }
            })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            var set = settings;
            set.viewModel.Dispose();
            _disposableBag.Dispose();
        }

        /// <summary>
        /// ターゲットクロスを移動させる
        /// </summary>
        /// <param name="mainCamera">メインカメラ</param>
        /// <param name="player">RewiredのPlayer</param>
        /// <param name="layerMaskTerrainObjects">地形レイヤー</param>
        /// <param name="viewModel">リズムパートパネルのビューモデル</param>
        /// <param name="centerPanel">中央パネル</param>
        /// <param name="targetCrossImage">ターゲットクロスのイメージ</param>
        /// <param name="layerMaskDropItems">取得可アイテムのレイヤー</param>
        /// <param name="layerMaskGhost">オバケのレイヤー</param>
        /// <param name="ポインター移動距離">ポインター移動距離</param>
        private void MoveTargetCross(Camera mainCamera, Player player, int layerMaskTerrainObjects, RhythmPartPanelCustomizeOfMainViewModel viewModel,
            RectTransform centerPanel, RectTransform targetCrossImage, int layerMaskDropItems, int layerMaskGhost, float ポインター移動距離)
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
                        viewModel.SetTargetCrossPosition(hitPoint);

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
                        viewModel.SetIsSelectedBattery(true);

                        // デバッグ表示
                        Debug.DrawLine(ray.origin, hitTrans.position, Color.red);
                    }
                    else
                    {
                        viewModel.SetIsSelectedBattery(false);
                    }
                    // 選択状態のMissGhostAttackを検知する
                    if (viewModel.BatteryTransform == null &&
                        Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, layerMaskGhost))
                    {
                        Transform hitTrans = hit2.transform;
                        viewModel.SetSelectedMissGhostAttackTransform(hitTrans);

                        // デバッグ表示
                        Debug.DrawLine(ray.origin, hitTrans.position, Color.red);
                    }
                    else
                    {
                        viewModel.SetSelectedMissGhostAttackTransform(null);
                    }
                }
            }
            else if (isInputJoystick && !isInputMouse)
            {
                Vector2 calcPosition = movePosition * ポインター移動距離;
                viewModel.SetTargetCrossAnchoredPosition(calcPosition);

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
                    viewModel.SetTargetCrossPosition(hitPoint);
                }
                // Xbox360コントローラーはBボタンで電池を取得する
                if (isGetBattery)
                {
                    viewModel.SetIsSelectedBattery(true);
                    MissGhostAttackCustomizeView missGhostAttackCustomizeView = FindAnyObjectByType<MissGhostAttackCustomizeView>();
                    if (missGhostAttackCustomizeView != null)
                    {
                        Transform hitTrans = missGhostAttackCustomizeView.transform;
                        viewModel.SetSelectedMissGhostAttackTransform(hitTrans);
                    }
                }
                else
                {
                    viewModel.SetIsSelectedBattery(false);
                    viewModel.SetSelectedMissGhostAttackTransform(null);
                }
            }
        }
    }

    /// <summary>
    /// リズムパートパネルの設定
    /// </summary>
    [System.Serializable]
    public class RhythmPartPanelCustomizeOfMainSettings
    {
        /// <summary>中央パネル</summary>
        public RectTransform centerPanel;
        /// <summary>リズムパートパネルのビューモデル</summary>
        public RhythmPartPanelCustomizeOfMainViewModel viewModel;
        [Tooltip("Xbox360コントローラーのみ対象\n値の高さ＝入力感度")]
        public float ポインター移動距離;
    }
}
