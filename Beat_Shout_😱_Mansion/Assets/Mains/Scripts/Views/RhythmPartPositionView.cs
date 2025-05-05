using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// リズムパート開始位置のビュー
    /// </summary>
    public class RhythmPartPositionView : MonoBehaviour
    {
        [Tooltip("Editorモードのみ\nRhythmPartPositionをPoltergeistViewがアタッチされているオブジェクトへLookAtさせる")]
        [SerializeField] private bool 自動で角度を調整;

        private void OnDrawGizmosSelected()
        {
            var parent = transform.parent;
            var trans = transform;
            if (自動で角度を調整)
            {
                trans.LookAt(parent);
            }
            Vector3 from = trans.position;
            var ceiling003obj = GameObject.Find("ceiling.003").transform;
            Vector3[] tos = new Vector3[]
            {
                parent.position,
                new Vector3(from.x, ceiling003obj.position.y, from.z)
            };
            Vector3[] directions = new Vector3[]
            {
                tos[0] - from,
                tos[1] - from,
            };
            float[] distances = new float[]
            {
                Vector3.Distance(from, tos[0]),
                Vector3.Distance(from, tos[1]),
            };
            // ベストな距離+-0.05以外は警告ログ＋色を赤に。それ以外ならログ出力なし＋色を緑に。
            bool[] isLimiteds = new bool[]
            {
                !(6.61f <= distances[0] && distances[0] <= 6.71f),
                !(1.45f <= distances[1] && distances[1] <= 1.55f),
            };
            if (isLimiteds[0] ||
                isLimiteds[1])
            {
                Debug.LogWarning($"家具との距離: [{distances[0]}]_天井との距離: [{distances[1]}]");
            }
            Debug.DrawRay(from, directions[0], isLimiteds[0] ? Color.red : Color.green);
            Debug.DrawRay(from, directions[1], isLimiteds[1] ? Color.red : Color.green);
            // スフィアをデバッグ描画
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(from, 0.2f); // 半径0.2のワイヤースフィア
        }
    }
}
