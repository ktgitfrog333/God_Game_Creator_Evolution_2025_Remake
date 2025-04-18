using UnityEngine;
using System.Collections;
public class MissGhostAttack : MonoBehaviour
{
    [SerializeField] private float initialSpeed = 10f; // 初期速度（最大）
    [SerializeField] private float minSpeed = 3f; // 最小速度
    [SerializeField] private float deceleration = 0.2f; // 減速度
    [SerializeField] private int frameDistance = 60; // N フレームの距離
    [SerializeField] private float gravity = 9.8f; // 重力
    [SerializeField] private float arcHeight = 5f; // 放物線の高さ（インスペクターで設定可能）

    private Vector3 startPosition; // 開始位置
    private Vector3 targetPosition; // 目標位置
    private Vector3 moveDirection; // 移動方向
    private float journeyLength; // 移動距離
    private float startTime; // 開始時間
    private bool isActive = false; // アクティブフラグ
    private float currentSpeed; // 現在の速度
    private Vector3 initialScale; // 初期スケール

    public float jumpHeight = 50f; // 飛び上がる高さ
    public float fallDistance = 120f; // 落下する距離

    // 動作モード
    private enum MoveMode
    {
        Failed, // 失敗時の動き
        Success // 成功時の動き
    }

    private MoveMode currentMode;
    private float animationTime = 1.5f; // 成功アニメーションの時間
    private float failedMovementDuration = 1.5f; // 失敗時の移動時間

    private void Awake()
    {
        // 初期スケールを保存
        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        // スケールを初期値に戻す
        transform.localScale = initialScale;
    }

    public void InitFailed(MissileNoteType noteType = MissileNoteType.Short)
    {
        Debug.Log($"Missile Init Failed生成 - ノーツタイプ: {noteType}");
        currentMode = MoveMode.Failed;

        // スケールを確実にリセット
        transform.localScale = initialScale;

        // メインカメラを取得
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("メインカメラが見つかりません！");
            ReturnToPool();
            return;
        }

        // 開始位置を現在の位置に設定
        startPosition = transform.position;

        // カメラの位置と向きから目標位置を計算
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;

        // カメラからNフレーム分の距離を計算
        float distanceToTravel = initialSpeed * (frameDistance / 60f);
        targetPosition = cameraPosition + (cameraForward * distanceToTravel);

        // 移動距離と方向を計算
        journeyLength = Vector3.Distance(startPosition, targetPosition);
        moveDirection = (targetPosition - startPosition).normalized;

        // 初期速度を最大に設定
        currentSpeed = initialSpeed;

        // 開始時間を記録
        startTime = Time.time;
        isActive = true;

        // オブジェクトの初期方向を設定
        transform.forward = moveDirection;
    }

    public void InitSuccess()
    {
        Debug.Log("Missile InitSuccess生成");
        currentMode = MoveMode.Success;

        // スケールを確実にリセット
        transform.localScale = initialScale;

        // 開始位置を現在の位置に設定
        startPosition = transform.position;

        // 落下の目標位置（現在位置から下に落ちる）
        targetPosition = transform.position + new Vector3(0, -fallDistance, 0);

        // 開始時間を記録
        startTime = Time.time;
        isActive = true;
    }

    private void Update()
    {
        if (!isActive) return;

        if (currentMode == MoveMode.Failed)
        {
            UpdateFailedMovement();
        }
        else
        {
            UpdateSuccessMovement();
        }
    }

    private void UpdateFailedMovement()
    {
        // 経過時間を計算
        float timeSinceStart = Time.time - startTime;

        // 時間ベースの進行度計算 (0～1)
        float fractionOfJourney = Mathf.Clamp01(timeSinceStart / failedMovementDuration);

        // X,Z平面上の位置を線形補間で計算
        Vector3 horizontalPosition = Vector3.Lerp(
            new Vector3(startPosition.x, 0, startPosition.z),
            new Vector3(targetPosition.x, 0, targetPosition.z),
            fractionOfJourney
        );

        // Y座標を放物線に沿って計算
        float height = arcHeight * 4 * fractionOfJourney * (1 - fractionOfJourney);

        // カメラ方向への逆向きの移動を加算
        Vector3 reversedDirection = -moveDirection * journeyLength * 0.3f * fractionOfJourney;

        // 最終的な位置を計算
        Vector3 newPosition = new Vector3(
            horizontalPosition.x + reversedDirection.x,
            Mathf.Lerp(startPosition.y, targetPosition.y, fractionOfJourney) + height,
            horizontalPosition.z + reversedDirection.z
        );

        // 新しい位置を設定
        transform.position = newPosition;

        // 進行方向を計算（次のフレームでの位置に向ける）
        Vector3 velocity = (newPosition - transform.position) / Time.deltaTime;
        if (velocity.magnitude > 0.01f)
        {
            // Y軸方向の動きを加味した自然な回転
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // 指定時間が経過したらプールに返却
        if (fractionOfJourney >= 1.0f)
        {
            Debug.Log($"Failed Movement complete at {Time.time}, duration: {timeSinceStart:F2}s");
            ReturnToPool();
        }
    }

    private void UpdateSuccessMovement()
    {
        float timeSinceStart = Time.time - startTime;
        float normalizedTime = Mathf.Clamp01(timeSinceStart / animationTime); // 0から1の範囲に正規化

        // 成功時のアニメーション（上下動）
        Vector3 position = transform.position;

        if (normalizedTime < 0.4f)
        {
            // 最初の40%の時間で上昇
            float upProgress = normalizedTime / 0.4f; // 0から1の範囲に再正規化
            position.y = startPosition.y + jumpHeight * Mathf.Sin(upProgress * Mathf.PI * 0.5f); // Sinカーブで上昇（0からπ/2）
        }
        else if (normalizedTime < 1.0f)
        {
            // 残りの60%の時間で下降
            float downProgress = (normalizedTime - 0.4f) / 0.6f; // 0から1の範囲に再正規化
            float startFall = startPosition.y + jumpHeight; // 落下開始位置
            float endFall = startPosition.y - fallDistance; // 落下終了位置

            // 加速しながら落下するための二次関数
            position.y = startFall - (startFall - endFall) * (downProgress * downProgress);
        }

        // 位置を更新
        transform.position = position;

        // アニメーション完了時
        if (normalizedTime >= 1.0f)
        {
            Debug.Log($"Success Movement complete at {Time.time}, duration: {timeSinceStart:F2}s");
            ReturnToPool();
        }
    }

    // 衝突時にプールに返却
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision detected with {collision.gameObject.name}, returning to pool");
        ReturnToPool();
    }

    // プールに返却する前に状態をリセット
    private void ResetState()
    {
        // スケールを初期値に戻す
        transform.localScale = initialScale;
        isActive = false;
    }

    // プールに返却する
    private void ReturnToPool()
    {
        // 状態をリセット
        ResetState();

        // 成功/失敗に応じて異なるプールに返却
        if (currentMode == MoveMode.Success)
        {
            ObjectPoolerXyloOther.Instance.ReturnToPool("SuccessGhostDown", gameObject);
        }
        else
        {
            ObjectPoolerXyloOther.Instance.ReturnToPool("MissGhostAttack", gameObject);
        }
    }

    // 公開メソッド：外部からプールに返却するため
    public void ForceReturnToPool()
    {
        Debug.Log($"Force return to pool: {gameObject.name}, mode: {currentMode}");
        ReturnToPool();
    }
}