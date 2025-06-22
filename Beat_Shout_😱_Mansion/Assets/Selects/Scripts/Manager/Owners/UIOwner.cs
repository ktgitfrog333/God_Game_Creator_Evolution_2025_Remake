using UnityEngine;

namespace Selects.Manager.Owners
{
    /// <summary>
    /// UIオーナー
    /// </summary>
    public class UIOwner : MonoBehaviour
    {
        private void Update()
        {
            SetCursorVisible();
        }

        /// <summary>
        /// マウスカーソルの見た目をセット
        /// </summary>
        /// <remarks>UnityEditor版はマウスカーソル表示<br/>
        /// ビルド版は非表示</remarks>
        private void SetCursorVisible()
        {
#if UNITY_EDITOR
            Cursor.visible = true;
#else
    Cursor.visible = false;
#endif
        }
    }
}
