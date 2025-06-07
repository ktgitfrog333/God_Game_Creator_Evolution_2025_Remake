using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// リズムパート開始位置_1のビュー
    /// </summary>
    public class RhythmPartPosition_1View : MonoBehaviour
    {
        [Tooltip("Editorモードのみ\nRhythmPartPosition_1をPoltergeistViewがアタッチされているオブジェクトorRhythmPartPositionへLookAtさせる")]
        [SerializeField] private bool 自動で角度を調整;

        private void OnDrawGizmosSelected()
        {
            var parent = transform.parent;
            Transform rhythmPartPosition = null;
            foreach (Transform child in parent)
            {
                if (child.name.Equals("RhythmPartPosition"))
                {
                    rhythmPartPosition = child;
                }
            }
            if (自動で角度を調整)
            {
                transform.LookAt(rhythmPartPosition != null ? rhythmPartPosition : parent);
            }
            var trans = transform;
            var parentSizeMagnitude = trans.parent.GetComponent<BoxCollider>().size.magnitude;
            Vector3 from = new Vector3(trans.position.x, trans.position.y + parentSizeMagnitude / 2f, trans.position.z);
            // キューブをデバッグ描画
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(from, trans.parent.GetComponent<BoxCollider>().size); // 半径0.2のワイヤーキューブ
        }
    }
}
