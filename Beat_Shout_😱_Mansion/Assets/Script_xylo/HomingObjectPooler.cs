using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 複数種類のミサイルを管理するオブジェクトプーラー
/// </summary>
public class MissileObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class MissileType
    {
        public GameObject prefab;                   // ミサイルのプレハブ
        public int poolSize = 10;                   // プールサイズ

        // プレハブ名をタグとして使用するためのプロパティ
        public string Name
        {
            get
            {
                return prefab != null ? prefab.name : "Unknown";
            }
        }

        [HideInInspector]
        public int missileId;                       // 自動割り当て用ID（1-9）
    }

    [Header("ミサイル設定")]
    [SerializeField] private List<MissileType> missileTypes = new List<MissileType>();

    [Header("生成設定")]
    [SerializeField] private float spawnRadius = 20f;       // 生成半径
    [SerializeField] private float minSpawnDistance = 15f;  // 最小生成距離

    // プール管理用の辞書
    private Dictionary<int, Queue<GameObject>> missilePoolDict;
    private int activeObjectCount = 0;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        // ミサイルIDの割り当て
        for (int i = 0; i < missileTypes.Count; i++)
        {
            missileTypes[i].missileId = i + 1;  // 1から始まるIDを割り当て
        }

        // プールの初期化
        InitializePools();
    }

    /// <summary>
    /// プールを初期化する
    /// </summary>
    private void InitializePools()
    {
        missilePoolDict = new Dictionary<int, Queue<GameObject>>();

        // 各ミサイルタイプごとにプールを作成
        foreach (MissileType missileType in missileTypes)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // プレハブが存在するか確認
            if (missileType.prefab == null)
            {
                Debug.LogWarning($"ミサイルプーラー: ミサイル ID {missileType.missileId} のプレハブが設定されていません。");
                continue;
            }

            // プール内のオブジェクトを生成
            for (int i = 0; i < missileType.poolSize; i++)
            {
                GameObject obj = Instantiate(missileType.prefab);

                // MissileDirectAnimManagerコンポーネントの確認（アニメーション設定はプレハブ側に依存）
                MissileDirectAnimManager animManager = obj.GetComponent<MissileDirectAnimManager>();
                if (animManager == null)
                {
                    Debug.LogWarning($"ミサイルプーラー: プレハブ「{missileType.Name}」に MissileDirectAnimManager コンポーネントがありません。");
                }

                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            missilePoolDict.Add(missileType.missileId, objectPool);
            Debug.Log($"ミサイルプーラー: ID {missileType.missileId} 「{missileType.Name}」のプールを初期化しました（サイズ: {missileType.poolSize}）");
        }
    }

    /// <summary>
    /// ランダムな位置にミサイルを生成する
    /// </summary>
    /// <param name="missileId">ミサイルID (1-9)</param>
    /// <returns>生成されたミサイル、失敗した場合はnull</returns>
    public GameObject SpawnMissile(int missileId)
    {
        return SpawnMissileAtPosition(missileId, GetRandomPositionWithDistance(minSpawnDistance, spawnRadius), Quaternion.identity);
    }

    /// <summary>
    /// 指定位置にミサイルを生成する
    /// </summary>
    /// <param name="missileId">ミサイルID (1-9)</param>
    /// <param name="position">生成位置</param>
    /// <param name="rotation">生成時の回転</param>
    /// <returns>生成されたミサイル、失敗した場合はnull</returns>
    public GameObject SpawnMissileAtPosition(int missileId, Vector3 position, Quaternion rotation)
    {
        // IDの範囲チェック
        if (missileId < 1 || missileId > 9)
        {
            Debug.LogWarning($"ミサイルプーラー: 無効なID {missileId} です。ID は 1-9 の範囲で指定してください。");
            return null;
        }

        // プール辞書にIDが存在するか確認
        if (!missilePoolDict.ContainsKey(missileId))
        {
            Debug.LogWarning($"ミサイルプーラー: ID {missileId} のミサイルプールが存在しません。");
            return null;
        }

        // プールからオブジェクトを取得
        Queue<GameObject> objectPool = missilePoolDict[missileId];

        // プールが空の場合
        if (objectPool.Count == 0)
        {
            Debug.LogWarning($"ミサイルプーラー: ID {missileId} のミサイルプールのすべてのオブジェクトが使用中です。");

            // 該当するミサイルタイプを検索してプールサイズを取得
            int poolSize = 0;
            GameObject prefab = null;

            foreach (MissileType type in missileTypes)
            {
                if (type.missileId == missileId)
                {
                    poolSize = type.poolSize;
                    prefab = type.prefab;
                    break;
                }
            }

            // プールサイズを1.5倍に拡張する（オプション）
            if (prefab != null)
            {
                int expansionSize = Mathf.CeilToInt(poolSize * 0.5f);
                Debug.Log($"ミサイルプーラー: ID {missileId} のプールを拡張します (+{expansionSize})");

                for (int i = 0; i < expansionSize; i++)
                {
                    GameObject obj = Instantiate(prefab);

                    // MissileDirectAnimManagerコンポーネントの有無確認（設定はプレハブ側に依存）
                    MissileDirectAnimManager animManagerC = obj.GetComponent<MissileDirectAnimManager>();
                    if (animManagerC == null)
                    {
                        Debug.LogWarning($"ミサイルプーラー: プレハブに MissileDirectAnimManager コンポーネントがありません。");
                    }

                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
            }

            // それでもプールが空の場合はnullを返す
            if (objectPool.Count == 0)
            {
                return null;
            }
        }

        // プールからオブジェクトを取り出す
        GameObject objectToSpawn = objectPool.Dequeue();

        // 位置と回転を設定
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // アニメーション状態をリセット（必要に応じて）
        MissileDirectAnimManager animManager = objectToSpawn.GetComponent<MissileDirectAnimManager>();
        if (animManager != null)
        {
            animManager.ResetAnimation();
        }

        // オブジェクトをアクティブ化
        objectToSpawn.SetActive(true);
        activeObjectCount++;

        // オブジェクトが非アクティブになった時のイベントを登録
        StartCoroutine(ReturnToPoolWhenInactive(objectToSpawn, missileId));

        return objectToSpawn;
    }

    /// <summary>
    /// オブジェクトがプールに戻るのを監視するコルーチン
    /// </summary>
    private IEnumerator ReturnToPoolWhenInactive(GameObject obj, int missileId)
    {
        // オブジェクトがアクティブでなくなるまで待機
        yield return new WaitUntil(() => !obj.activeInHierarchy);

        // プールに戻す
        if (missilePoolDict.ContainsKey(missileId))
        {
            missilePoolDict[missileId].Enqueue(obj);
            activeObjectCount--;
        }
    }

    /// <summary>
    /// 指定した距離範囲内のランダムな位置にミサイルを生成する
    /// </summary>
    /// <param name="missileId">ミサイルID (1-9)</param>
    /// <param name="minDistance">カメラからの最小距離</param>
    /// <param name="maxDistance">カメラからの最大距離</param>
    /// <returns>生成されたミサイル、失敗した場合はnull</returns>
    public GameObject SpawnMissileWithDistance(int missileId, float minDistance, float maxDistance)
    {
        return SpawnMissileAtPosition(missileId, GetRandomPositionWithDistance(minDistance, maxDistance), Quaternion.identity);
    }

    /// <summary>
    /// 指定した距離範囲内のランダム位置を取得する
    /// </summary>
    public Vector3 GetRandomPositionWithDistance(float minDistance, float maxDistance)
    {
        // カメラが存在しない場合は原点を返す
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("ミサイルプーラー: カメラが見つかりません。原点を使用します。");
                return Vector3.zero;
            }
        }

        // カメラの周囲のランダムな位置を計算
        Vector3 randomDirection = Random.onUnitSphere;

        // カメラの前方180度内に制限する（背後からは出現しないように）
        if (Vector3.Dot(randomDirection, mainCamera.transform.forward) < 0)
        {
            randomDirection = -randomDirection;
        }

        // 距離をランダムに決定（指定された最小距離から最大距離まで）
        float distance = Random.Range(minDistance, maxDistance);

        return mainCamera.transform.position + randomDirection * distance;
    }

    /// <summary>
    /// 使用可能なミサイルIDの配列を取得
    /// </summary>
    public int[] GetAvailableMissileIds()
    {
        List<int> ids = new List<int>();
        foreach (var type in missileTypes)
        {
            ids.Add(type.missileId);
        }
        return ids.ToArray();
    }

    /// <summary>
    /// ミサイルIDに対応するミサイル名を取得
    /// </summary>
    public string GetMissileNameById(int missileId)
    {
        foreach (var type in missileTypes)
        {
            if (type.missileId == missileId)
            {
                return type.Name;
            }
        }
        return "Unknown";
    }

    /// <summary>
    /// アクティブなオブジェクト数を取得
    /// </summary>
    public int GetActiveObjectCount()
    {
        return activeObjectCount;
    }
}