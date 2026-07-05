using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// インフォメーションパネルのビュー
    /// </summary>
    public class InfoPanelView : MonoBehaviour
    {
        /// <summary>キャンバスグループ</summary>
        [SerializeField] private CanvasGroup canvasGroup;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            // ★ Escキー押下で非表示にする（一度のみ）
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 既に非表示の場合は何もしない
                if (canvasGroup != null && canvasGroup.alpha > 0f)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;
                }
            }
        }
    }
}
