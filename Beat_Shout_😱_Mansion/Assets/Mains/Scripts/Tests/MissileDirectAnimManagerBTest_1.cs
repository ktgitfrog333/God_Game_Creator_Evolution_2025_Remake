using UnityEngine;
using UnityEngine.UI;

namespace Mains.Tests
{
    /// <summary>
    /// ホーミングミサイルテスト
    /// </summary>
    public class MissileDirectAnimManagerBTest_1 : MonoBehaviour
    {
        private MissileUIAnimationLayerCustomize animLayer1st;
        [SerializeField] private GameObject uiContainer;
        [SerializeField] private MissileUIAnimationLayerCustomizeTable missileUIAnimationLayerCustomizeTable;
        [SerializeField] private MissileAnimationType missileAnimationType;

        private void Start()
        {
            GameObject layer1st = CreateLayer("AnimLayer1st");
            layer1st.SetActive(true);
            animLayer1st = layer1st.AddComponent<MissileUIAnimationLayerCustomize>();
            animLayer1st.SetMissileUIAnimationLayerCustomizeTable(missileUIAnimationLayerCustomizeTable);
            animLayer1st.Initialize();
            //animLayer1st.SetVisibility(false);
            animLayer1st.ChangeSprites(missileAnimationType);
        }

        private GameObject CreateLayer(string name)
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
    }
}
