using Mains.External;
using UnityEngine;
using R3;

namespace Mains.Views
{
    /// <summary>
    /// オブジェクトをホーミングする処理のビュー
    /// </summary>
	public class HomingObjectCustomizeView : MonoBehaviour
	{
        [Tooltip("Assets/Mains/Prefabs/UIs/GoodPanel.prefabをセットしておく。")]
        [SerializeField] private Transform goodPanelPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/GoodParticleSys.prefabをセットしておく。")]
        [SerializeField] private Transform goodParticleSysPrefab;
        [Tooltip("Assets/Mains/Prefabs/UIs/BadPanel.prefabをセットしておく。")]
        [SerializeField] private Transform badPanelPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/BadParticleSys.prefabをセットしておく。")]
        [SerializeField] private Transform badParticleSysPrefab;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
		{
			Script_xyloApi script_XyloApi = new();
            Transform trans = transform;
            var canvasTransform = GameObject.Find("OverlayCanvas").transform;
            script_XyloApi.SetMissileDirectAnimManagerB(trans);
            script_XyloApi.IsSuccessful.Where(x => x)
                .Subscribe(_ =>
                {
                    ShowUIPanelAtObjectPosition(goodPanelPrefab, canvasTransform, goodParticleSysPrefab, script_XyloApi.NoteTransform, trans);
                })
                .AddTo(ref _disposableBag);
            script_XyloApi.IsFailed.Where(x => x)
                .Subscribe(_ =>
                {
                    ShowUIPanelAtObjectPosition(badPanelPrefab, canvasTransform, badParticleSysPrefab, script_XyloApi.NoteTransform, trans);
                })
                .AddTo(ref _disposableBag);
        }

        /// <summary>
        /// オブジェクトをベースにCanvasの座標へ変換して、プレハブをインスタンス
        /// </summary>
        /// <param name="uiPrefab">UIプレハブ</param>
        /// <param name="canvasTransform">キャンバストランスフォーム</param>
        /// <param name="particleSysPrefab">パーティクルプレハブ</param>
        /// <param name="noteTransform">UIトランスフォーム</param>
        /// <param name="trans">トランスフォーム</param>
        private void ShowUIPanelAtObjectPosition(Transform uiPrefab, Transform canvasTransform, Transform particleSysPrefab, Transform noteTransform, Transform trans)
        {
            // UIを生成
            Transform instance = Instantiate(uiPrefab, canvasTransform);
            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            var noteAnchoredPosition = (noteTransform as RectTransform).anchoredPosition;
            rectTransform.anchoredPosition = noteAnchoredPosition;

            // パーティクルをワールド空間に生成
            // プレハブの元の回転を取得
            Quaternion prefabRotation = particleSysPrefab.rotation;

            // trans（例：UIや他オブジェクト）のY回転だけ適用
            Vector3 euler = prefabRotation.eulerAngles;
            euler.y = trans.rotation.eulerAngles.y;

            // 新しい回転を作成
            Quaternion finalRotation = Quaternion.Euler(euler);
            Transform particleInstance = Instantiate(particleSysPrefab, trans.position, finalRotation);
        }
    }
}
