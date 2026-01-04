using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HierarchyMergeEditor : EditorWindow
{
    private GameObject itemA;
    private GameObject itemB;

    [MenuItem("Tools/Hierarchy Merge")]
    private static void Open()
    {
        GetWindow<HierarchyMergeEditor>("Hierarchy Merge");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Hierarchy Merge Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        itemA = (GameObject)EditorGUILayout.ObjectField(
            "Item A (Source)",
            itemA,
            typeof(GameObject),
            true
        );

        itemB = (GameObject)EditorGUILayout.ObjectField(
            "Item B (Target)",
            itemB,
            typeof(GameObject),
            true
        );

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(itemA == null || itemB == null))
        {
            if (GUILayout.Button("Execute Merge", GUILayout.Height(32)))
            {
                ExecuteMerge();
            }
        }
    }

    private void ExecuteMerge()
    {
        // -------------------------
        // Pre Check
        // -------------------------

        var itemAChildren = itemA.transform
            .Cast<Transform>()
            .Select(t => t.gameObject)
            .ToList();

        // itemA 直下の名前ユニークチェック
        var duplicateA = itemAChildren
            .GroupBy(go => go.name)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateA != null)
        {
            EditorUtility.DisplayDialog(
                "Merge Error",
                $"ItemA直下に同名オブジェクトがあります: {duplicateA.Key}",
                "OK"
            );
            return;
        }

        // itemB 全階層キャッシュ
        var itemBTransforms = itemB.GetComponentsInChildren<Transform>(true);

        // itemB 側の複数一致チェック
        foreach (var a in itemAChildren)
        {
            var matches = itemBTransforms
                .Where(t => t.name == a.name)
                .ToList();

            if (matches.Count > 1)
            {
                string detail = string.Join(
                    "\n",
                    matches.Select(m => GetHierarchyPath(m))
                );

                Debug.LogError(
                    $"[MergeError] '{a.name}' が複数存在します:\n{detail}"
                );

                EditorUtility.DisplayDialog(
                    "Merge Error",
                    $"ItemB内で '{a.name}' が複数存在します。\n処理を中断しました。",
                    "OK"
                );
                return;
            }
        }

        // -------------------------
        // Undo Begin
        // -------------------------

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Hierarchy Merge");
        Undo.RegisterFullObjectHierarchyUndo(itemB, "Hierarchy Merge");

        var destroyTargets = new List<GameObject>();

        Scene targetScene = itemB.scene;

        // -------------------------
        // Instance Phase
        // -------------------------

        var instances = new List<(GameObject instance, string originalName, Vector3 worldPosition, Quaternion worldRotation, Vector3 lossyScale)>();

        foreach (var a in itemAChildren)
        {
            Transform matchedB = itemBTransforms
                .FirstOrDefault(t => t.name == a.name);

            Transform parent;

            if (matchedB != null)
            {
                parent = matchedB.parent;
                destroyTargets.Add(matchedB.gameObject);
            }
            else
            {
                parent = itemB.transform;
            }

            // itemAのワールド座標を保存
            Vector3 worldPosition = a.transform.position;
            Quaternion worldRotation = a.transform.rotation;
            Vector3 lossyScale = a.transform.lossyScale;

            GameObject instance;
            GameObject sourcePrefab =
                PrefabUtility.GetCorrespondingObjectFromSource(a);

            if (sourcePrefab != null)
            {
                // Prefab / FBX から Asset を保持したままインスタンス化
                instance = (GameObject)PrefabUtility.InstantiatePrefab(
                    sourcePrefab,
                    itemB.scene
                );
            }
            else
            {
                // 通常の Scene Object のみインスタンス化
                instance = Instantiate(a);
                SceneManager.MoveGameObjectToScene(instance, itemB.scene);
            }

            Undo.RegisterCreatedObjectUndo(instance, "Hierarchy Merge");

            // 親を設定（ワールド座標を保持するためfalse）
            instance.transform.SetParent(parent, false);
            instance.transform.SetSiblingIndex(parent.childCount - 1);

            // 名前は既存オブジェクト削除後に設定するため、ここでは保存のみ
            instances.Add((instance, a.name, worldPosition, worldRotation, lossyScale));
        }

        // -------------------------
        // Destroy Phase
        // -------------------------

        foreach (var target in destroyTargets)
        {
            Undo.DestroyObjectImmediate(target);
        }

        // -------------------------
        // Transform Assignment Phase
        // -------------------------

        // 既存オブジェクトを削除した後、ワールド座標を設定
        foreach (var (instance, originalName, worldPosition, worldRotation, lossyScale) in instances)
        {
            Transform instanceTransform = instance.transform;

            // ワールド位置と回転を設定
            instanceTransform.position = worldPosition;
            instanceTransform.rotation = worldRotation;

            // ワールドスケールを維持するためにローカルスケールを計算
            // lossyScale = parent.lossyScale * localScale
            // localScale = lossyScale / parent.lossyScale
            if (instanceTransform.parent != null)
            {
                Vector3 parentLossyScale = instanceTransform.parent.lossyScale;
                Vector3 localScale = new Vector3(
                    parentLossyScale.x != 0 ? lossyScale.x / parentLossyScale.x : 1f,
                    parentLossyScale.y != 0 ? lossyScale.y / parentLossyScale.y : 1f,
                    parentLossyScale.z != 0 ? lossyScale.z / parentLossyScale.z : 1f
                );
                instanceTransform.localScale = localScale;
            }
            else
            {
                // 親がない場合は直接設定
                instanceTransform.localScale = lossyScale;
            }
        }

        // -------------------------
        // Name Assignment Phase
        // -------------------------

        // 座標設定後に名前を設定
        foreach (var (instance, originalName, _, _, _) in instances)
        {
            instance.name = originalName;
        }

        // -------------------------
        // Undo End
        // -------------------------

        Undo.CollapseUndoOperations(undoGroup);

        Debug.Log("[HierarchyMerge] Merge completed successfully.");
    }

    private static string GetHierarchyPath(Transform t)
    {
        var path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
