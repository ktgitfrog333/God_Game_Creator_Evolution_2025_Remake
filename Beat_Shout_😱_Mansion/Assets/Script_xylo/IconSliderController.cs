using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IconSliderController : MonoBehaviour
{
    [Header("UI References")]
    public Slider iconCountSlider;
    public Transform iconContainer; // Horizontal Layout Group が付いているオブジェクト
    public GameObject iconPrefab;   // アイコンのプレハブ

    private HorizontalLayoutGroup layoutGroup;
    private ContentSizeFitter contentSizeFitter;

    [Header("Settings")]
    public int maxIconCount = 10; // 最大10個に変更
    public float animationDuration = 0.3f;

    [Header("Colors")]
    public Color activeColor = Color.white;   // アクティブ時の色
    public Color inactiveColor = Color.gray;  // 非アクティブ時の色（グレー）

    private List<GameObject> allIcons = new List<GameObject>();
    private int currentActiveCount = 0;

    void Start()
    {
        // Layout Group コンポーネントを取得
        layoutGroup = iconContainer.GetComponent<HorizontalLayoutGroup>();
        contentSizeFitter = iconContainer.GetComponent<ContentSizeFitter>();

        // Horizontal Layout Group の初期設定を最適化
        if (layoutGroup != null)
        {
            layoutGroup.spacing = 10f; // 固定間隔
            layoutGroup.childAlignment = TextAnchor.MiddleLeft; // 左寄せ
            layoutGroup.childControlWidth = false;  // 幅制御を無効
            layoutGroup.childControlHeight = false; // 高さ制御を無効
            layoutGroup.childScaleWidth = false;    // スケール制御を無効
            layoutGroup.childScaleHeight = false;   // スケール制御を無効
            layoutGroup.childForceExpandWidth = false;  // 強制拡張を無効
            layoutGroup.childForceExpandHeight = false; // 強制拡張を無効

            // パディングを設定（左端からの開始位置を調整）
            layoutGroup.padding = new RectOffset(0, 0, 0, 0); // 左パディング0で完全に左詰め
        }

        // Content Size Fitter の設定を最適化
        if (contentSizeFitter != null)
        {
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // スライダーの初期設定
        iconCountSlider.minValue = 0;
        iconCountSlider.maxValue = maxIconCount;
        iconCountSlider.value = 5; // 初期値
        iconCountSlider.wholeNumbers = true;

        // スライダーの値変更イベントを登録
        iconCountSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // 予め全てのアイコンを生成（全てグレー）
        CreateAllIcons();

        // 初期状態を設定
        UpdateIconColors((int)iconCountSlider.value);
    }

    void CreateAllIcons()
    {
        for (int i = 0; i < maxIconCount; i++)
        {
            GameObject newIcon = Instantiate(iconPrefab, iconContainer);

            // Layout Element の設定を確実にする
            LayoutElement layoutElement = newIcon.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = newIcon.AddComponent<LayoutElement>();
            }

            // 固定サイズを設定
            layoutElement.preferredWidth = 50f;
            layoutElement.preferredHeight = 50f;
            layoutElement.flexibleWidth = 0f;   // 柔軟な幅を無効
            layoutElement.flexibleHeight = 0f;  // 柔軟な高さを無効
            layoutElement.minWidth = 50f;       // 最小幅を固定
            layoutElement.minHeight = 50f;      // 最小高さを固定

            // 初期状態はグレーに設定
            Image iconImage = newIcon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = inactiveColor;
            }

            allIcons.Add(newIcon);
        }

        // レイアウトを即座に更新
        LayoutRebuilder.MarkLayoutForRebuild(iconContainer as RectTransform);
    }

    public void OnSliderValueChanged(float value)
    {
        int newActiveCount = (int)value;
        UpdateIconColors(newActiveCount);
    }

    void UpdateIconColors(int activeCount)
    {
        if (activeCount == currentActiveCount) return;

        // アニメーション方向を決定
        bool increasing = activeCount > currentActiveCount;

        if (increasing)
        {
            // アクティブ数が増える場合：新しくアクティブになるアイコンをアニメーション
            for (int i = currentActiveCount; i < activeCount; i++)
            {
                StartCoroutine(ColorChangeAnimation(allIcons[i], inactiveColor, activeColor));
            }
        }
        else
        {
            // アクティブ数が減る場合：非アクティブになるアイコンをアニメーション
            for (int i = activeCount; i < currentActiveCount; i++)
            {
                StartCoroutine(ColorChangeAnimation(allIcons[i], activeColor, inactiveColor));
            }
        }

        currentActiveCount = activeCount;
    }

    System.Collections.IEnumerator ColorChangeAnimation(GameObject icon, Color fromColor, Color toColor)
    {
        Image iconImage = icon.GetComponent<Image>();
        if (iconImage == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            // 色の補間
            iconImage.color = Color.Lerp(fromColor, toColor, progress);

            // 軽微なスケールエフェクト
            float scale = 1f + (Mathf.Sin(progress * Mathf.PI) * 0.1f);
            icon.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        // 最終状態を確実に設定
        iconImage.color = toColor;
        icon.transform.localScale = Vector3.one;
    }

    void OnDestroy()
    {
        // イベントリスナーを削除
        if (iconCountSlider != null)
        {
            iconCountSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}