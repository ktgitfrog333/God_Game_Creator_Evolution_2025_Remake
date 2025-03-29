using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingObject : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float initialSpeed = 8f;     // 初期速度
    [SerializeField] private float curveStrength = 25f;   // 外側に膨らむ強さ
    [SerializeField] private float maxSpeed = 15f;        // 最大速度
    [SerializeField] private float acceleration = 5f;     // 加速度
    [SerializeField] private float rotationSpeed = 3f;    // 回転速度

    [Header("目標位置設定")]
    [SerializeField] private float targetDistance = 10f;  // カメラからの目標距離
    [SerializeField] private float lifeDuration = 5f;     // オブジェクト寿命（秒）
    [SerializeField] private float curvePhaseTime = 1.2f; // カーブフェーズの時間
    [SerializeField] private float initialPhaseTime = 0.2f; // 初期フェーズの時間
    [SerializeField] private float CountInt = 3f;         // 何ビート生存するか
    private float LifeBeat = 0.5f;                        // 基本ビート長（秒）
    [SerializeField] private float arrivalBuffer = 0.2f;  // 到達の余裕時間（秒）

    // 追加: 到達時刻と到達距離の計算用
    private float midPointTime;                          // 途中停止時刻
    private float targetArrivalTime;                      // 目標到達時刻
    private float homingStartTime;                        // ホーミング開始時刻
    private bool hasReachedTarget = false;                // 目標に到達したかどうか
    private bool hasReachedMidPoint = false;              // 途中点に到達したかどうか
    private Vector3 midPointPosition;                     // 途中停止位置
    private float OneBeat;

    private Transform cameraTransform;
    private Vector3 targetPosition;
    private Vector3 curveDirection;
    private float currentSpeed;
    private float timer = 0f;


    private enum FlightPhase { Initial, Curving, Homing, MidPoint, Arrived }
    private FlightPhase currentPhase = FlightPhase.Initial;

    private void OnEnable()
    {
        // リセット
        ResetObject();
    }

    public void Init()
    {
        OneBeat = CRIWARE_conductor.Instance.BasicBeat;

        LifeBeat = OneBeat * 3;
        curvePhaseTime = OneBeat * 1.8f;

        // 全体の寿命設定
        lifeDuration = LifeBeat + (OneBeat / 2);

        // 途中停止時刻の設定
        midPointTime = LifeBeat;

        // 目標到達時刻の設定
        targetArrivalTime = OneBeat * 2;

        // 変数の初期化
        hasReachedTarget = false;
        hasReachedMidPoint = false;

        // カメラの参照を取得
        cameraTransform = Camera.main.transform;

        // 初期設定
        currentSpeed = initialSpeed;
        timer = 0f;
        currentPhase = FlightPhase.Initial;

        // 現在の前方向を維持
        Vector3 initialDirection = transform.forward;

        // 目標位置を設定
        targetPosition = transform.position + initialDirection * targetDistance;

        // 膨らみの方向をカメラに対して垂直面内のみに制限
        // カメラの前方向と初期方向の両方に垂直なベクトルを計算
        Vector3 right = Vector3.Cross(cameraTransform.forward, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, cameraTransform.forward).normalized;

        // ランダムな角度を生成
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        // 垂直面内のランダムな方向
        curveDirection = (right * Mathf.Cos(randomAngle) + up * Mathf.Sin(randomAngle)).normalized * curveStrength;
    }

    private void ResetObject()
    {
        // カメラの参照を取得
        cameraTransform = Camera.main.transform;

        // 初期設定
        currentSpeed = initialSpeed;
        timer = 0f;
        currentPhase = FlightPhase.Initial;
        hasReachedTarget = false;
        hasReachedMidPoint = false;
        midPointPosition = Vector3.zero;

        // 目標位置を計算 - 現在の前方向に特定距離
        targetPosition = transform.position + transform.forward * targetDistance;

        // 膨らみの方向をカメラに対して垂直面内のみに制限
        Vector3 right = Vector3.Cross(cameraTransform.forward, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, cameraTransform.forward).normalized;

        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        curveDirection = (right * Mathf.Cos(randomAngle) + up * Mathf.Sin(randomAngle)).normalized * curveStrength;

        // ランダムな回転は適用しない（スポナーからの向きを維持）
        // transform.rotation = Random.rotation; <- この行を削除
    }

    private void Update()
    {
        // 寿命を管理
        timer += Time.deltaTime;

        // まず目標位置を常に更新（カメラが動く場合）
        targetPosition = cameraTransform.position + cameraTransform.forward * targetDistance;

        // オブジェクトが完全に非アクティブになるのを防ぐために
        // 寿命チェックを最後に移動し、その間は見えるように処理を続ける

        // 途中停止時間に達したかチェック
        if (timer >= midPointTime && !hasReachedMidPoint && currentPhase != FlightPhase.MidPoint)
        {
            // 途中点に到達
            currentPhase = FlightPhase.MidPoint;
            hasReachedMidPoint = true;

            // 現在位置を途中停止位置として記録
            midPointPosition = transform.position;

        }
        // 最終目標到達時間は途中停止フェーズ内で処理するため、
        // ここでは特別な処理を行わない（瞬間移動させない）
        else if (currentPhase == FlightPhase.Arrived)
        {
            // 到達フェーズには実際には移行しないため、このブロックは実行されない
            // （念のため残しておく）
        }
        else if (currentPhase == FlightPhase.MidPoint)
        {
            // 途中停止フェーズでは、永続的に途中位置に留まる
            // 途中位置を維持
            transform.position = midPointPosition;

            // 最終目標到達時間が来ても移動せず、途中停止位置のままを維持
            if (timer >= targetArrivalTime && !hasReachedTarget)
            {
                hasReachedTarget = true;
       
            }
        }
        else
        {
            // 移動フェーズの更新
            UpdateMovement();

            // 目標に向けて回転
            UpdateRotation();
        }

 
    }

    private void UpdateMovement()
    {
        // フェーズに応じた動き
        switch (currentPhase)
        {
            case FlightPhase.Initial:
                // 初期フェーズ - わずかに加速
                currentSpeed += acceleration * 0.5f * Time.deltaTime;
                transform.position += transform.forward * currentSpeed * Time.deltaTime;

                // 設定した時間後にカーブフェーズへ
                if (timer > initialPhaseTime)
                {
                    currentPhase = FlightPhase.Curving;
                }
                break;

            case FlightPhase.Curving:
                // カーブフェーズ - 外側に膨らむ
                float curveRatio = Mathf.Clamp01((timer - initialPhaseTime) / curvePhaseTime);
                // イージング関数で膨らみを制御（最初は強く、徐々に弱く）
                float easedCurve = 1f - (curveRatio * curveRatio);
                Vector3 curveForce = curveDirection * easedCurve;

                // カーブしながら前進
                transform.position += (transform.forward + curveForce) * currentSpeed * Time.deltaTime;

                // カーブの終了
                if (curveRatio >= 1f)
                {
                    currentPhase = FlightPhase.Homing;
                    homingStartTime = timer; // ホーミング開始時刻を記録
                }
                break;

            case FlightPhase.Homing:
                // 目標へのベクトルと距離を計算
                Vector3 directionToTarget = (targetPosition - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

                // 途中停止時間が近づいているか確認
                float timeToMidPoint = midPointTime - timer;

                // 途中停止時間が近い場合
                if (timeToMidPoint <= 0.01f)
                {
                    // 途中停止フェーズへ移行
                    currentPhase = FlightPhase.MidPoint;
                    hasReachedMidPoint = true;
                    midPointPosition = transform.position;
                    break;
                }

                // 目標に到達したか判定
                if (distanceToTarget < 0.5f)
                {
                    currentPhase = FlightPhase.Arrived;
                    hasReachedTarget = true;
                    break;
                }

                // 途中停止時間までの調整された移動
                if (timeToMidPoint > 0)
                {
                    // 残り時間内に適切な位置まで進むように速度を調整
                    // ここでは途中停止位置を考慮した速度調整
                    float progressRatio = 1.0f - (timeToMidPoint / (midPointTime - homingStartTime));
                    float adjustedSpeed = Mathf.Lerp(initialSpeed, maxSpeed, progressRatio);

                    // 速度の範囲を制限
                    currentSpeed = Mathf.Clamp(adjustedSpeed, initialSpeed * 0.5f, maxSpeed);
                }
                else
                {
                    // 途中停止時間を過ぎた場合は停止
                    currentSpeed = 0f;
                }

                // 移動を適用
                transform.position += directionToTarget * currentSpeed * Time.deltaTime;
                break;

            case FlightPhase.MidPoint:
                // 途中停止フェーズでは位置を維持
                // 移動せず、現在の位置を保持
                currentSpeed = 0f;
                break;

            case FlightPhase.Arrived:
                // 到達後は目標位置にとどまる（カメラの動きに追従）
                transform.position = targetPosition;

                // 到達後は移動しないように速度をゼロに
                currentSpeed = 0f;
                break;
        }
    }

    private void UpdateRotation()
    {
        // 目標方向を取得
        Vector3 direction = (targetPosition - transform.position).normalized;

        switch (currentPhase)
        {
            case FlightPhase.Initial:
                // 初期フェーズでは現在の方向を維持
                break;

            case FlightPhase.Curving:
                // カーブフェーズでは緩やかに目標方向に向け始める
                float curveRotationSpeed = rotationSpeed * 0.3f; // カーブ中は回転速度を抑える
                Quaternion curveRotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    curveRotationSpeed * Time.deltaTime
                );
                transform.rotation = curveRotation;
                break;

            case FlightPhase.Homing:
                // ホーミングフェーズでは目標位置に向けて回転
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
                break;

            case FlightPhase.MidPoint:
                // 途中停止フェーズでは現在の向きを維持
                // 必要に応じてゆっくりと目標方向に向けることも可能
                Quaternion midPointRotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    rotationSpeed * 0.2f * Time.deltaTime
                );
                transform.rotation = midPointRotation;
                break;

            case FlightPhase.Arrived:
                // 到達フェーズでは目標位置に向けて回転
                Quaternion arrivedRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    arrivedRotation,
                    rotationSpeed * Time.deltaTime
                );
                break;
        }
    }

    // デバッグ用の視覚化（エディタでの確認用）
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && cameraTransform != null)
        {
            // 目標位置
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.5f);

            // 途中停止位置
            if (hasReachedMidPoint)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(midPointPosition, 0.4f);
            }

            // 現在の向き
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);

            // 目標方向
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, targetPosition);

            // 到達状態の表示
            if (hasReachedTarget)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, 1.0f);
            }

            // 途中停止状態の表示
            if (hasReachedMidPoint && !hasReachedTarget)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, 0.8f);
            }
        }
    }
}