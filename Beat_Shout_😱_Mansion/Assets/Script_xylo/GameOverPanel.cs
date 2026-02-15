using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class GameOverPanel : MonoBehaviour
{
    [Header("フェード設定")]
    [SerializeField, Tooltip("フェードイン秒数")]
    private float fadeInDuration = 1.5f;

    [SerializeField, Tooltip("待機秒数")]
    private float waitDuration = 2.0f;

    [Header("シーン遷移設定")]
    [SerializeField, Tooltip("待機後に遷移するシーン名")]
    private string nextSceneName = "Title";

    private Image panelImage;
    private TextMeshProUGUI[] textComponents;

    private void Awake()
    {
        // パネル自身のImageコンポーネントを取得
        panelImage = GetComponent<Image>();
        if (panelImage == null)
        {
            Debug.LogError("GameOverPanel: Imageコンポーネントが見つかりません");
        }

        // 子オブジェクトのTextMeshProコンポーネントを全て取得
        textComponents = GetComponentsInChildren<TextMeshProUGUI>(true);

        // 初期状態を透明に設定
        if (panelImage != null)
        {
            Color color = panelImage.color;
            color.a = 0f;
            panelImage.color = color;
        }

        foreach (var text in textComponents)
        {
            Color color = text.color;
            color.a = 0f;
            text.color = color;
        }
    }

    private void OnEnable()
    {
        CRIWARE_AisacChange.Instance.FadeVolume(0, waitDuration-0.2f);
        // フェードイン処理を開始


        StartFadeIn();
    }

    private void OnDisable()
    {
        // Tweenを確実に停止
        DOTween.Kill(this);
    }

    private void StartFadeIn()
    {
        // パネルのフェードイン
        if (panelImage != null)
        {
            panelImage.DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutQuad)
                .SetId(this);
        }

        // テキストのフェードイン
        foreach (var text in textComponents)
        {
            text.DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutQuad)
                .SetId(this);
        }

        // フェードイン完了後、待機してシーン遷移
        DOVirtual.DelayedCall(fadeInDuration + waitDuration, () =>
        {
            LoadNextScene();
        }).SetId(this);
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("GameOverPanel: 遷移先のシーン名が設定されていません");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}