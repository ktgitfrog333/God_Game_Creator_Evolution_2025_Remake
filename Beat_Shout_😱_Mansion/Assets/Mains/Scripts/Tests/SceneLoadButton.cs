using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mains.Tests
{
    /// <summary>
    /// シーンロードボタン
    /// </summary>
    public class SceneLoadButton : MonoBehaviour
    {
        [Header("シーン設定")]
        [SerializeField] private string sceneName = "SampleScene";
        [SerializeField] private int sceneBuildIndex = 0;
        [SerializeField] private bool useBuildIndex = false;

        [Header("ボタン表示設定")]
        [SerializeField] private string buttonText = "シーンをロード";
        [SerializeField] private float buttonWidth = 200f;
        [SerializeField] private float buttonHeight = 50f;
        [SerializeField] private Color buttonColor = Color.white;

        private void OnGUI()
        {
            // ボタンの位置を設定（画面中央）
            float x = (Screen.width - buttonWidth) / 2f;
            float y = (Screen.height - buttonHeight) / 2f;
            Rect buttonRect = new Rect(x, y, buttonWidth, buttonHeight);

            // ボタンの色を設定
            Color originalColor = GUI.color;
            GUI.color = buttonColor;

            // ボタンを描画
            if (GUI.Button(buttonRect, buttonText))
            {
                LoadScene();
            }

            // 色を戻す
            GUI.color = originalColor;
        }

        private void LoadScene()
        {
            if (useBuildIndex)
            {
                // ビルドインデックスでロード
                SceneManager.LoadScene(sceneBuildIndex);
                Debug.Log($"シーン (BuildIndex: {sceneBuildIndex}) をロードします");
            }
            else
            {
                // シーン名でロード
                SceneManager.LoadScene(sceneName);
                Debug.Log($"シーン '{sceneName}' をロードします");
            }
        }
    }
}
