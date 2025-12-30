using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// リズムパート開始位置のビュー
    /// </summary>
    public class RhythmPartPositionView : MonoBehaviour
    {
        [ContextMenu("プレイヤーの高さに揃える")]
        private void DoAdjustToPlayer()
        {
            var level = GameObject.Find("Level").transform;
            Transform target = null;
            foreach (Transform child in level)
            {
                foreach (Transform item in child)
                {
                    if (item.name.Equals("Player"))
                    {
                        target = item;
                    }
                }
            }
            if (target != null)
            {
                var trans = transform;
                var position = trans.position;
                var playerPosition = target.position;
                trans.position = new Vector3(position.x, playerPosition.y, position.z);
            }
            else
            {
                Debug.LogWarning("プレイヤーが見つかりません");
            }
        }

        [ContextMenu("自動で角度を調整")]
        private void DoAutoLookAt()
        {
            var trans = transform;
            var parent = trans.parent;
            Transform rhythmPartPosition_1 = null;
            foreach (Transform child in parent)
            {
                if (child.name.Equals("RhythmPartPosition_1"))
                {
                    rhythmPartPosition_1 = child;
                }
            }
            trans.LookAt(rhythmPartPosition_1 != null ? rhythmPartPosition_1 : parent);
            // X軸だけ0に戻す
            var angle = trans.localEulerAngles;
            trans.localEulerAngles = new Vector3(0f, angle.y, angle.z);
        }

        private void OnDrawGizmosSelected()
        {
            var parent = transform.parent;
            Transform rhythmPartPosition_1 = null;
            foreach (Transform child in parent)
            {
                if (child.name.Equals("RhythmPartPosition_1"))
                {
                    rhythmPartPosition_1 = child;
                }
            }
            var trans = transform;
            Vector3 from = trans.position;
            // スフィアをデバッグ描画
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(from, 0.2f); // 半径0.2のワイヤースフィア
        }
    }
}
