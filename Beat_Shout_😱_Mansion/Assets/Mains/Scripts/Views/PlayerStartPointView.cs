using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// プレイヤー生成ポイントのビュー
    /// </summary>
    public class PlayerStartPointView : MonoBehaviour
    {
        [SerializeField] private GameObject プレイヤーのプレハブ;

        private void Start()
        {
            Instantiate(プレイヤーのプレハブ, transform.position, transform.rotation, transform.parent);

            // 自身の見た目を非表示にする
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }
    }
}
