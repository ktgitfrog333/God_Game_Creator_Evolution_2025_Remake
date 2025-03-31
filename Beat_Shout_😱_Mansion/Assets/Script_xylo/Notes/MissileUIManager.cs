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

    // UI関連の透過度
    private float layer1Alpha;
    private float layer2Alpha;

    // エイムサークル関連
    private GameObject aimCircleObj;
    private Image aimCircleImage;
    private RectTransform aimCircleRect;

    /// <summary>
    /// UIManagerのコンストラクタ
    /// </summary>
    public MissileUIManager(MissileDirectAnimManagerB parent, Canvas canvas, Vector2 imageSize, float layer1Alpha, float layer2Alpha)
    {
        this.parent = parent;
        this.targetCanvas = canvas;
        this.layer1Alpha = layer1Alpha;
        this.layer2Alpha = layer2Alpha;

        SetupUI(imageSize);
    }

    /// <summary>
    /// UIコンテナを設定
    /// </summary>
    private void SetupUI(Vector2 imageSize)
    {
        // キャンバスが指定されていない場合は自動で探す
        if (targetCanvas == null)
        {
            targetCanvas = Object.FindAnyObjectByType<Canvas>();
            if (targetCanvas == null)
            {
                // キャンバスが見つからない場合は新しく作成
                GameObject canvasObj = new GameObject("MissileEffectCanvas");
                targetCanvas = canvasObj.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // キャンバススケーラーを追加
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // Raycasterを追加（必要に応じて）
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        canvasRectTransform = targetCanvas.GetComponent<RectTransform>();

        // UIコンテナを作成（ミサイルのスプライト表示用）
        uiContainer = new GameObject("MissileEffectContainer");
        uiContainer.transform.SetParent(targetCanvas.transform, false);

        uiRectTransform = uiContainer.AddComponent<RectTransform>();
        uiRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        uiRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        uiRectTransform.sizeDelta = imageSize;

        // エイムサークルの初期化
        SetupAimCircle();
    }

    /// <summary>
    /// エイムサークルのセットアップ
    /// </summary>
    private void SetupAimCircle()
    {
        Sprite aimCircleSprite = parent.GetAimCircleSprite();
        if (aimCircleSprite == null) return;

        // エイムサークルオブジェクトを作成
        aimCircleObj = new GameObject("AimCircle");
        aimCircleObj.transform.SetParent(uiContainer.transform, false);

        // イメージコンポーネントを追加
        aimCircleImage = aimCircleObj.AddComponent<Image>();
        aimCircleImage.sprite = aimCircleSprite;
        aimCircleImage.preserveAspect = true;

        // 透明度の設定
        aimCircleImage.color = new Color(1f, 1f, 1f, parent.GetAimCircleAlpha());

        // RectTransformの設定
        aimCircleRect = aimCircleObj.GetComponent<RectTransform>();
        aimCircleRect.anchorMin = new Vector2(0.5f, 0.5f);
        aimCircleRect.anchorMax = new Vector2(0.5f, 0.5f);
        aimCircleRect.pivot = new Vector2(0.5f, 0.5f);

        // サイズ設定 - 親のUIコンテナと同じサイズを使用
        // ノーツUI全体の大きさに合わせる
        aimCircleRect.sizeDelta = uiRectTransform.sizeDelta;

        // 中央に配置
        aimCircleRect.anchoredPosition = Vector2.zero;
    }

    public void UpdateAimCircleSize(float scale)
    {
 
        if (aimCircleRect != null && uiRectTransform != null)
        {
            // 親コンテナのサイズに対するスケール
            aimCircleRect.sizeDelta = uiRectTransform.sizeDelta * scale;
 
        }
    }
    /// <summary>
    /// レイヤー用のGameObjectを作成
    /// </summary>
    public GameObject CreateLayer(string name)
    {
        GameObject layerObj = new GameObject(name);
        layerObj.transform.SetParent(uiContainer.transform, false);

        // Image コンポーネントを追加
        Image image = layerObj.AddComponent<Image>();

        // アルファブレンドを有効にする設定
        image.material = new Material(Shader.Find("UI/Default"));
        image.preserveAspect = true;

        // 透過を可能にするためのCanvasGroupを追加
        CanvasGroup canvasGroup = layerObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // RectTransformの設定
        RectTransform layerRect = layerObj.GetComponent<RectTransform>();
        layerRect.anchorMin = Vector2.zero;
        layerRect.anchorMax = Vector2.one;
        layerRect.offsetMin = Vector2.zero;
        layerRect.offsetMax = Vector2.zero;

        return layerObj;
    }

    /// <summary>
    /// UIコンテナのアクティブ状態を設定
    /// </summary>
    public void SetUIActive(bool active)
    {
        if (uiContainer != null)
        {
            uiContainer.SetActive(active);
        }
    }

    /// <summary>
    /// UIコンテナのアクティブ状態を取得
    /// </summary>
    public bool IsUIActive()
    {
        return uiContainer != null && uiContainer.activeInHierarchy;
    }

    /// <summary>
    /// UIの位置とサイズを更新
    /// </summary>
    public void UpdateUITransform(Vector3 position, Vector2 size, bool maintainConstantSize)
    {
        if (uiRectTransform == null) return;

        uiRectTransform.position = position;

        // 遠近に関わらず一定サイズを維持する場合
        if (maintainConstantSize)
        {
            // キャンバススケーラーがあれば考慮する
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

    /// <summary>
    /// ポインターがUIの上にあるか判定
    /// </summary>
    public bool IsPointerOverUI()
    {
        Vector2 mousePos = Input.mousePosition;

        // UIコンテナがないか非アクティブなら常にfalse
        if (uiContainer == null || !uiContainer.activeInHierarchy)
        {
            return false;
        }

        // エイムサークルがない場合はデフォルトの方法で判定
        if (aimCircleObj == null || aimCircleRect == null)
        {
            // UIコンテナの位置とサイズを取得
            RectTransform rt = uiRectTransform;
            if (rt == null) return false;

            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            // 中心点を計算
            float centerX = (corners[0].x + corners[2].x) / 2f;
            float centerY = (corners[0].y + corners[2].y) / 2f;

            // デフォルトの矩形判定（コンテナサイズの1/3）
            float clickableWidth = (corners[2].x - corners[0].x) / 3f;
            float clickableHeight = (corners[2].y - corners[0].y) / 3f;

            // 範囲チェック
            return (Mathf.Abs(mousePos.x - centerX) <= clickableWidth / 2f &&
                   Mathf.Abs(mousePos.y - centerY) <= clickableHeight / 2f);
        }
        else
        {
            // エイムサークルの中心を取得
            Vector3[] corners = new Vector3[4];
            aimCircleRect.GetWorldCorners(corners);

            Vector2 center = new Vector2(
                (corners[0].x + corners[2].x) / 2f,
                (corners[0].y + corners[2].y) / 2f
            );

            // エイムサークルの半径を取得
            float radiusX = (corners[2].x - corners[0].x) / 2f;
            float radiusY = (corners[2].y - corners[0].y) / 2f;
            float radius = Mathf.Min(radiusX, radiusY);

            // 円形判定（距離ベース）
            float distanceSquared = (mousePos.x - center.x) * (mousePos.x - center.x) +
                                   (mousePos.y - center.y) * (mousePos.y - center.y);

            return distanceSquared <= radius * radius;
        }
    }

    /// <summary>
    /// UIコンテナの取得
    /// </summary>
    public GameObject GetUIContainer()
    {
        return uiContainer;
    }

    /// <summary>
    /// ターゲットキャンバスの取得
    /// </summary>
    public Canvas GetTargetCanvas()
    {
        return targetCanvas;
    }

    /// <summary>
    /// レイヤー1のアルファ値を取得
    /// </summary>
    public float GetLayer1Alpha()
    {
        return layer1Alpha;
    }

    /// <summary>
    /// レイヤー2のアルファ値を取得
    /// </summary>
    public float GetLayer2Alpha()
    {
        return layer2Alpha;
    }

    /// <summary>
    /// エイムサークルのアルファ値を設定
    /// </summary>
    public void SetAimCircleAlpha(float alpha)
    {
        if (aimCircleImage != null)
        {
            Color color = aimCircleImage.color;
            color.a = alpha;
            aimCircleImage.color = color;
        }
    }
}