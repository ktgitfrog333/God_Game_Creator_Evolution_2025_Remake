using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// リズムパート開始位置_1のビュー
    /// </summary>
    public class RhythmPartPosition_1View : MonoBehaviour
    {
        [ContextMenu("コライダー情報をコピー")]
        private void DoCopyCollider()
        {
            var trans = transform;
            var parentCollider = trans.parent.GetComponent<BoxCollider>();
            if (parentCollider == null)
            {
                Debug.LogWarning("親オブジェクトにBoxColliderが存在しません。");
                return;
            }

            var collider = trans.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = trans.gameObject.AddComponent<BoxCollider>();
            }

            // 親オブジェクトと子オブジェクトのスケールを取得
            Vector3 parentScale = trans.parent.localScale;
            Vector3 childScale = trans.localScale;
            collider.size = Vector3.one;
            Vector3 scaledSize = new Vector3(
                collider.size.x * parentCollider.size.x * parentScale.x,
                collider.size.y * parentCollider.size.y * parentScale.y,
                collider.size.z * parentCollider.size.z * parentScale.z
            );

            // 親コライダーのプロパティを個別にコピー
            collider.size = scaledSize;
            collider.center = parentCollider.center;
            collider.enabled = parentCollider.enabled;
            collider.isTrigger = parentCollider.isTrigger;
            collider.material = parentCollider.material;
            collider.contactOffset = parentCollider.contactOffset;

            Debug.Log($"親コライダーの情報をコピーしました。");
        }

        [ContextMenu("自動で角度を調整")]
        private void DoAutoLookAt()
        {
            var trans = transform;
            var parent = trans.parent;
            Transform rhythmPartPosition = null;
            foreach (Transform child in parent)
            {
                if (child.name.Equals("RhythmPartPosition"))
                {
                    rhythmPartPosition = child;
                }
            }
            trans.LookAt(rhythmPartPosition != null ? rhythmPartPosition : parent);
            // X軸だけ0に戻す
            var angle = trans.localEulerAngles;
            trans.localEulerAngles = new Vector3(0f, angle.y, angle.z);
        }

        private void OnDrawGizmosSelected()
        {
            var trans = transform;
            var parent = trans.parent;
            Vector3 from = trans.position;
            Transform rhythmPartPosition = null;
            foreach (Transform child in parent)
            {
                if (child.name.Equals("RhythmPartPosition"))
                {
                    rhythmPartPosition = child;
                }
            }
            Vector3[] tos = new Vector3[]
            {
                rhythmPartPosition != null ? rhythmPartPosition.position : parent.position,
            };
            Vector3[] directions = new Vector3[]
            {
                tos[0] - from,
            };
            float[] distances = new float[]
            {
                Vector3.Distance(from, tos[0]),
            };
            // ベストな距離+-0.05以外は警告ログ＋色を赤に。それ以外ならログ出力なし＋色を緑に。
            bool[] isLimiteds = new bool[]
            {
                !(6.61f <= distances[0] && distances[0] <= 6.71f),
            };
            Debug.DrawRay(from, directions[0], isLimiteds[0] ? Color.red : Color.green);
        }
    }
}
