using UnityEditor;
using UnityEngine;
using Mains.Views;
using Mains.Commons;

namespace Mains.Editor
{
    [CustomEditor(typeof(PoltergeistView))]
    public class PoltergeistViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // デフォルトのInspectorを表示
            base.OnInspectorGUI();

            var view = (PoltergeistView)target;

            // ゲーム実行中のみ表示
            if (!Application.isPlaying) return;

            // パトロールオバケ（スピードオバケ）の場合のみ拡張表示
            if (view.GhostInStaticObjectStruct.moveType == MoveType.Patrol)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("【スピードオバケ情報（デバッグ）】", EditorStyles.boldLabel);
                
                if (view.DebugPatrolRemainingTime >= 0f)
                {
                    float remaining = view.DebugPatrolRemainingTime;
                    float elapsed = view.DebugPatrolInterval - remaining;

                    // プログレスバー風に表示
                    Rect rect = GUILayoutUtility.GetRect(18f, 18f, "TextField");
                    float progress = elapsed / view.DebugPatrolInterval;
                    EditorGUI.ProgressBar(rect, progress, $"移動まで残り: {remaining:F1} 秒");
                    
                    // Inspectorを毎フレーム更新して残時間を滑らかに表示させる
                    Repaint();
                }
                else
                {
                    EditorGUILayout.HelpBox("現在タイマーは停止中です（探索パート外または空室）", MessageType.Info);
                }
            }
        }
    }
}
