using Mains.ViewModels;
using R3;
using Rewired;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// リズムパートパネルのビュー
    /// </summary>
    public class RhythmPartPanelView : MonoBehaviour
    {
        /// <summary>中央パネル</summary>
        [SerializeField] private RectTransform centerPanel;
        /// <summary>リズムパートパネルのビューモデル</summary>
        private RhythmPartPanelViewModel _rhythmPartPanelViewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        [Tooltip("Xbox360コントローラーのみ対象\n値の高さ＝入力感度")]
        [SerializeField] private float ポインター移動速度;
        [SerializeField] private float mousePosZ = 10f;

        private void Reset()
        {
            if (centerPanel == null)
                centerPanel = transform.GetChild(0) as RectTransform;
        }

        private void Start()
        {
            _rhythmPartPanelViewModel = new();
            var player = ReInput.players.GetPlayer(0);
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
                    x.Subscribe(interactionPart =>
                    {
                        switch (interactionPart)
                        {
                            case Commons.InteractionPart.Rhythm:
                                centerPanel.gameObject.SetActive(true);
                                targetCrossDisposable?.Dispose();
                                int layerMaskTerrainObjects = 1 << LayerMask.NameToLayer("TerrainObjects");
                                int layerMaskDropItems = 1 << LayerMask.NameToLayer("DropItems");
                                // マウスポインターへ追従
                                targetCrossDisposable = Observable.EveryUpdate()
                                    .Subscribe(_ =>
                                    {
                                        Vector2 mousePos = Input.mousePosition;
                                        float moveX = player.GetAxis("RhythmMoveHorizontal");
                                        float moveZ = player.GetAxis("RhythmMoveVertical");

                                        bool isInputMouse = Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f;
                                        bool isInputJoystick = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;

                                        // 入力排他制御：同一フレーム内でマウスとジョイスティックが両方有効になることを防ぐ
                                        if (isInputMouse && !isInputJoystick)
                                        {
                                            // マウスで操作
                                            if (0 <= mousePos.x && mousePos.x <= Screen.width &&
                                                0 <= mousePos.y && mousePos.y <= Screen.height)
                                            {
                                                Vector2 localPoint;
                                                // ポインター位置（スクリーン座標）からワールド方向ベクトルを計算
                                                Ray ray = Camera.main.ScreenPointToRay(mousePos);

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
                                            }
                                        }
                                        else if (isInputJoystick && !isInputMouse)
                                        {
                                            Vector2 movePosition = new Vector2(moveX, moveZ) * ポインター移動速度 * Time.deltaTime;
                                            Vector2 calcPosition = targetCrossImage.anchoredPosition + movePosition;

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
                                            Ray ray = Camera.main.ScreenPointToRay(screenPoint);

                                            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMaskTerrainObjects))
                                            {
                                                Vector3 hitPoint = hit.point;
                                                _rhythmPartPanelViewModel.SetTargetCrossPosition(hitPoint);
                                            }
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
                                        }
                                    })
                                    .AddTo(ref _disposableBag);

                                break;
                            default:
                                centerPanel.gameObject.SetActive(false);

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
        }
    }
}
