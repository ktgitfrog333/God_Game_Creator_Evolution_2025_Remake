using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIの作成と管理を行うクラス
/// </summary>
public class MissileUIManager
{
    private MissileDirectAnimManagerB parent;
    private Canvas targetCanvas;
    private RectTransform canvasRectTransform;
    private GameObject uiContainer;
    private RectTransform uiRectTransform;

    private float layer1Alpha;
    private float layer2Alpha;

    // エイムサークル関連
    private GameObject aimCircleObj;
    private Image aimCircleImage;
    private RectTransform aimCircleRect;

    // SpectrumCircle関連
    private GameObject spectrumCircleObj;
    private RectTransform spectrumCircleRect;

    public MissileUIManager(MissileDirectAnimManagerB parent, Canvas canvas, Vector2 imageSize, float layer1Alpha, float layer2Alpha)
    {
        this.parent = parent;
        this.targetCanvas = canvas;
        this.layer1Alpha = layer1Alpha;
        this.layer2Alpha = layer2Alpha;

        SetupUI(imageSize);
    }

    private void SetupUI(Vector2 imageSize)
    {
        if (targetCanvas == null)
        {
            targetCanvas = Object.FindAnyObjectByType<Canvas>();
            if (targetCanvas == null)
            {
                GameObject canvasObj = new GameObject("MissileEffectCanvas");
                targetCanvas = canvasObj.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        Debug.Log($"キャンバス情報: RenderMode={targetCanvas.renderMode}, PixelPerfect={targetCanvas.pixelPerfect}, ScaleFactor={targetCanvas.scaleFactor}");

        canvasRectTransform = targetCanvas.GetComponent<RectTransform>();

        uiContainer = new GameObject("MissileEffectContainer_" + parent.gameObject.GetInstanceID());
        uiContainer.transform.SetParent(targetCanvas.transform, false);

        uiRectTransform = uiContainer.AddComponent<RectTransform>();
        uiRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        uiRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        uiRectTransform.sizeDelta = imageSize;

        Debug.Log($"UIコンテナ作成: Name={uiContainer.name}, Size={imageSize}, Active={uiContainer.activeInHierarchy}");

        // SpectrumCircleのセットアップ（エイムサークルより前に作成して背面になるようにする）
        SetupSpectrumCircle();

        // エイムサークルのセットアップ
        SetupAimCircle();
    }

    /// <summary>
    /// SpectrumCircleのセットアップ
    /// </summary>
    private void SetupSpectrumCircle()
    {
        GameObject prefab = parent.GetSpectrumCirclePrefab();
        if (prefab == null)
        {
            Debug.LogWarning("SpectrumCirclePrefabがnullです。SpectrumCircleは表示されません。");
            return;
        }

        // プレハブをuiContainerの子として生成
        spectrumCircleObj = Object.Instantiate(prefab, uiContainer.transform);

        spectrumCircleRect = spectrumCircleObj.GetComponent<RectTransform>();
        if (spectrumCircleRect == null)
        {
            spectrumCircleRect = spectrumCircleObj.AddComponent<RectTransform>();
        }

        // 中央に配置
        spectrumCircleRect.anchorMin = new Vector2(0.5f, 0.5f);
        spectrumCircleRect.anchorMax = new Vector2(0.5f, 0.5f);
        spectrumCircleRect.pivot = new Vector2(0.5f, 0.5f);
        spectrumCircleRect.anchoredPosition = Vector2.zero;

        // サイズをuiContainerに合わせる（スケール倍率を適用）
        UpdateSpectrumCircleSize(parent.GetSpectrumCircleScale());

        // 最背面に配置（エイムサークルより後ろ）
        spectrumCircleObj.transform.SetAsFirstSibling();

        // 初期状態は非表示
        spectrumCircleObj.SetActive(false);

        Debug.Log($"SpectrumCircleを初期化: Size={spectrumCircleRect.sizeDelta}");
    }

    /// <summary>
    /// SpectrumCircleのサイズを更新
    /// </summary>
    public void UpdateSpectrumCircleSize(float scale)
    {
        if (spectrumCircleRect != null && uiRectTransform != null)
        {
            spectrumCircleRect.sizeDelta = uiRectTransform.sizeDelta * scale;
            Debug.Log($"SpectrumCircleサイズを更新: {spectrumCircleRect.sizeDelta}, Scale: {scale}");
        }
    }

    /// <summary>
    /// SpectrumCircleの表示/非表示
    /// </summary>
    public void SetSpectrumCircleActive(bool active)
    {
        if (spectrumCircleObj != null)
        {
            spectrumCircleObj.SetActive(active);
            Debug.Log($"SpectrumCircle: {active}");
        }
    }

    private void CreateTestRedCircle()
    {
        GameObject redCircle = new GameObject("TestRedCircle");
        redCircle.transform.SetParent(uiContainer.transform, false);

        RectTransform rt = redCircle.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(50, 50);

        Image img = redCircle.AddComponent<Image>();
        img.color = Color.red;
        img.sprite = CreateCircleSprite();

        Debug.Log("テスト用の赤いサークルを作成しました");
    }

    private Sprite CreateCircleSprite()
    {
        Texture2D texture = new Texture2D(128, 128);
        Color[] colors = new Color[128 * 128];

        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float dx = x - 64;
                float dy = y - 64;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist < 60)
                {
                    colors[y * 128 + x] = Color.white;
                }
                else
                {
                    colors[y * 128 + x] = new Color(1, 1, 1, 0);
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
    }

    private void SetupAimCircle()
    {
        Sprite aimCircleSprite = parent.GetAimCircleSprite();
        Debug.Log($"AimCircleスプライト: {(aimCircleSprite != null ? aimCircleSprite.name : "null")}");

        if (aimCircleSprite == null)
        {
            Debug.LogWarning("AimCircleスプライトがnullです。AimCircleは表示されません。");
            return;
        }

        aimCircleObj = new GameObject("AimCircle");
        aimCircleObj.transform.SetParent(uiContainer.transform, false);

        aimCircleImage = aimCircleObj.AddComponent<Image>();
        aimCircleImage.sprite = aimCircleSprite;
        aimCircleImage.preserveAspect = true;

        float alpha = parent.GetAimCircleAlpha();
        aimCircleImage.color = new Color(1f, 1f, 1f, alpha);

        Debug.Log($"AimCircleのAlpha値: {alpha}");

        aimCircleRect = aimCircleObj.GetComponent<RectTransform>();
        aimCircleRect.anchorMin = new Vector2(0.5f, 0.5f);
        aimCircleRect.anchorMax = new Vector2(0.5f, 0.5f);
        aimCircleRect.pivot = new Vector2(0.5f, 0.5f);
        aimCircleRect.sizeDelta = uiRectTransform.sizeDelta;
        aimCircleRect.anchoredPosition = Vector2.zero;

        aimCircleObj.transform.SetAsLastSibling();

        Debug.Log($"AimCircleを初期化: Size={aimCircleRect.sizeDelta}, Position={aimCircleRect.anchoredPosition}");
    }

    public void UpdateAimCircleSize(float scale)
    {
        if (aimCircleRect != null && uiRectTransform != null)
        {
            aimCircleRect.sizeDelta = uiRectTransform.sizeDelta * scale;
            Debug.Log($"AimCircleサイズを更新: {aimCircleRect.sizeDelta}, Scale: {scale}");
        }
        else
        {
            Debug.LogWarning("AimCircleのサイズを更新できません: " +
                           (aimCircleRect == null ? "AimCircleRectがnull" : "UIRectTransformがnull"));
        }
    }

    public void ForceShowAimCircle()
    {
        if (aimCircleObj != null && aimCircleImage != null)
        {
            aimCircleObj.SetActive(true);
            aimCircleImage.color = Color.red;
            aimCircleImage.enabled = true;
            aimCircleObj.transform.SetAsLastSibling();

            Debug.Log("AimCircleを強制的に表示しました (赤色)");
        }
        else
        {
            Debug.LogError("ForceShowAimCircle: AimCircleオブジェクトまたはイメージがnullです");
        }
    }

    public void CreateNewTestAimCircle()
    {
        GameObject testCircle = new GameObject("NewTestAimCircle");
        testCircle.transform.SetParent(uiContainer.transform, false);

        RectTransform rt = testCircle.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(100, 100);
        rt.anchoredPosition = new Vector2(0, 0);

        Image img = testCircle.AddComponent<Image>();
        img.sprite = CreateCircleSprite();
        img.color = new Color(0, 1, 0, 1);

        testCircle.transform.SetAsLastSibling();

        Debug.Log("新しいテスト用AimCircle (緑) を作成しました");
    }

    public GameObject CreateLayer(string name)
    {
        GameObject layerObj = new GameObject(name);
        layerObj.transform.SetParent(uiContainer.transform, false);

        Image image = layerObj.AddComponent<Image>();
        image.material = new Material(Shader.Find("UI/Default"));
        image.preserveAspect = true;

        CanvasGroup canvasGroup = layerObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        RectTransform layerRect = layerObj.GetComponent<RectTransform>();
        layerRect.anchorMin = Vector2.zero;
        layerRect.anchorMax = Vector2.one;
        layerRect.offsetMin = Vector2.zero;
        layerRect.offsetMax = Vector2.zero;

        layerObj.SetActive(true);

        return layerObj;
    }

    public void SetUIActive(bool active)
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(active);
            Debug.Log($"UIコンテナのアクティブ状態を {active} に設定しました");

            if (active && aimCircleObj != null)
            {
                aimCircleObj.SetActive(true);
                if (aimCircleImage != null)
                {
                    aimCircleImage.enabled = true;
                }
            }
        }
    }

    public bool IsUIActive()
    {
        return uiContainer != null && uiContainer.activeInHierarchy;
    }

    public void UpdateUITransform(Vector3 position, Vector2 size, bool maintainConstantSize)
    {
        if (uiRectTransform == null) return;

        uiRectTransform.position = position;

        if (maintainConstantSize)
        {
            if (targetCanvas != null && targetCanvas.scaleFactor > 0)
            {
                uiRectTransform.sizeDelta = size / targetCanvas.scaleFactor;
            }
            else
            {
                uiRectTransform.sizeDelta = size;
            }
        }
    }

    public bool IsPointerOverUI()
    {
        Vector2 mousePos = Input.mousePosition;

        if (uiContainer == null || !uiContainer.activeInHierarchy)
        {
            return false;
        }

        if (aimCircleObj == null || aimCircleRect == null)
        {
            RectTransform rt = uiRectTransform;
            if (rt == null) return false;

            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            float centerX = (corners[0].x + corners[2].x) / 2f;
            float centerY = (corners[0].y + corners[2].y) / 2f;

            float clickableWidth = (corners[2].x - corners[0].x) / 3f;
            float clickableHeight = (corners[2].y - corners[0].y) / 3f;

            return (Mathf.Abs(mousePos.x - centerX) <= clickableWidth / 2f &&
                   Mathf.Abs(mousePos.y - centerY) <= clickableHeight / 2f);
        }
        else
        {
            Vector3[] corners = new Vector3[4];
            aimCircleRect.GetWorldCorners(corners);

            Vector2 center = new Vector2(
                (corners[0].x + corners[2].x) / 2f,
                (corners[0].y + corners[2].y) / 2f
            );

            float radiusX = (corners[2].x - corners[0].x) / 2f;
            float radiusY = (corners[2].y - corners[0].y) / 2f;
            float radius = Mathf.Min(radiusX, radiusY);

            float distanceSquared = (mousePos.x - center.x) * (mousePos.x - center.x) +
                                   (mousePos.y - center.y) * (mousePos.y - center.y);

            return distanceSquared <= radius * radius;
        }
    }

    public GameObject GetUIContainer() => uiContainer;
    public Canvas GetTargetCanvas() => targetCanvas;
    public float GetLayer1Alpha() => layer1Alpha;
    public float GetLayer2Alpha() => layer2Alpha;

    public void SetAimCircleAlpha(float alpha)
    {
        if (aimCircleImage != null)
        {
            Color color = aimCircleImage.color;
            color.a = alpha;
            aimCircleImage.color = color;
            Debug.Log($"AimCircleのアルファ値を {alpha} に設定しました");
        }
    }

    public void CleanupUIContainer()
    {
        if (uiContainer != null)
        {
            for (int i = 0; i < uiContainer.transform.childCount; i++)
            {
                uiContainer.transform.GetChild(i).gameObject.SetActive(false);
            }

            uiContainer.SetActive(false);

            Debug.Log("UIコンテナをクリーンアップしました");
        }
    }

    public void EnsureUIContainerActive()
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(true);

            for (int i = 0; i < uiContainer.transform.childCount; i++)
            {
                GameObject child = uiContainer.transform.GetChild(i).gameObject;
                child.SetActive(true);

                if (child == aimCircleObj && aimCircleImage != null)
                {
                    aimCircleImage.enabled = true;
                }
            }

            Debug.Log("UIコンテナとすべての子オブジェクトをアクティブにしました");
        }
    }
}