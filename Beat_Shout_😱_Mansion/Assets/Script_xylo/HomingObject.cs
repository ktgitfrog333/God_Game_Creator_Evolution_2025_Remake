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
 public Vector3 InitialScale; // 初期スケール

    [Header("サイズ設定")]
    [SerializeField] private float finalScale = 3.0f;     // 最終的な拡大倍率
    [SerializeField] private float continuousScaleFactor = 1f; // 静止後の拡大速度係数（1/5）
    [SerializeField] private float maxAdditionalScale = 2.0f;    // 静止後の最大追加拡大倍率

    [Header("タイミング設定")]
    [SerializeField] private float initialPhaseTime = 0.2f; // 初期フェーズの時間
    [SerializeField] private float curvePhaseTime = 1.2f;   // カーブフェーズの時間
    [SerializeField] private float CountInt = 3f;           // 何ビート生存するか
    private float LifeBeat = 0.5f;                          // 基本ビート長（秒）

    // 追加: 到達時刻と到達距離の計算用
    private float midPointTime;                          // 途中停止時刻
    private float targetArrivalTime;                      // 目標到達時刻
    private float homingStartTime;                        // ホーミング開始時刻
    private float midPointStartTime;                      // 静止開始時刻
    private bool hasReachedTarget = false;                // 目標に到達したかどうか
    private bool hasReachedMidPoint = false;              // 途中点に到達したかどうか
    private Vector3 midPointPosition;                     // 途中停止位置
    private float OneBeat;

    private Transform cameraTransform;
    private Vector3 targetPosition;
    private Vector3 curveDirection;
    private float currentSpeed;
    private float timer = 0f;
    private Vector3 initialDirection;                     // 初期の前方向を保持
 //   private Vector3 originalScale;                       // 元のスケール

    private enum FlightPhase { Initial, Curving, Homing, MidPoint, Arrived }
    private FlightPhase currentPhase = FlightPhase.Initial;

    private void OnEnable()
    {
    //    InitialScale = transform.localScale; // 初期スケールを保存
        // リセット
        ResetObject();
    }

    private void OnDisable()
    {
        transform.localScale = InitialScale; // 初期スケールを復元
    }

    public void Init()
    {
        OneBeat = CRIWARE_conductor.Instance.BasicBeat;

        LifeBeat = OneBeat * 3;
        curvePhaseTime = OneBeat * 1.8f;

        // 全体の寿命設定
        float lifeDuration = LifeBeat + (OneBeat / 2);

        // 途中停止時刻の設定
        midPointTime = LifeBeat;

        // 目標到達時刻の設定
        targetArrivalTime = OneBeat * 2;

        // 変数の初期化
        hasReachedTarget = false;
        hasReachedMidPoint = false;
        midPointStartTime = 0f;

        // カメラの参照を取得
        cameraTransform = Camera.main.transform;

        // 初期設定
        currentSpeed = initialSpeed;
        timer = 0f;
        currentPhase = FlightPhase.Initial;



        // 現在の前方向を保存（スポナーで設定された角度）
        initialDirection = transform.forward;

        // 生成位置とカメラを結ぶ軸方向を計算
        Vector3 axisToCameraDirection = (cameraTransform.position - transform.position).normalized;

        // 最終目標位置はカメラの位置
        targetPosition = cameraTransform.position;

        // 外側に膨らむ方向を計算
        // 0度が上方向（12時）でスポナーと同じ角度解釈をするために調整
        Vector3 worldUp = Vector3.up;

        // 初期方向と中心軸の両方に垂直な方向を計算
        Vector3 outwardDirection = Vector3.Cross(initialDirection, axisToCameraDirection).normalized;

        // もし外側方向が計算できない場合（ほぼ平行な場合）は代替方向を使用
        if (outwardDirection.magnitude < 0.01f)
        {
            // 代替として上方向と初期方向の垂直成分を使用
            outwardDirection = Vector3.Cross(initialDirection, worldUp).normalized;
            if (outwardDirection.magnitude < 0.01f)
            {
                // それでも難しい場合は前方向を基準にする
                outwardDirection = Vector3.Cross(initialDirection, Vector3.forward).normalized;
            }
        }

        // カーブの膨らみ方向を設定
        curveDirection = outwardDirection * curveStrength;

        // デバッグログ
        //   Debug.Log($"HomingObject: 初期化完了 - 初期方向: {initialDirection}, 軸方向: {axisToCameraDirection}, 膨らみ方向: {curveDirection.normalized}");
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
        midPointStartTime = 0f;


        // スケールをリセット
        transform.localScale = InitialScale;

        // 現在の前方向を保存（スポナーで設定された角度）
        initialDirection = transform.forward;

        // 生成位置とカメラを結ぶ軸方向を計算
        Vector3 axisToCameraDirection = (cameraTransform.position - transform.position).normalized;

        // 最終目標位置はカメラの位置
        targetPosition = cameraTransform.position;

        // 外側に膨らむ方向を計算
        // 0度が上方向（12時）でスポナーと同じ角度解釈をするために調整
        Vector3 worldUp = Vector3.up;

        // 初期方向と中心軸の両方に垂直な方向を計算
        Vector3 outwardDirection = Vector3.Cross(initialDirection, axisToCameraDirection).normalized;

        // もし外側方向が計算できない場合（ほぼ平行な場合）は代替方向を使用
        if (outwardDirection.magnitude < 0.01f)
        {
            // 代替として上方向と初期方向の垂直成分を使用
            outwardDirection = Vector3.Cross(initialDirection, worldUp).normalized;
            if (outwardDirection.magnitude < 0.01f)
            {
                // それでも難しい場合は前方向を基準にする
                outwardDirection = Vector3.Cross(initialDirection, Vector3.forward).normalized;
            }
        }

        // カーブの膨らみ方向を設定
        curveDirection = outwardDirection * curveStrength;
    }

    private void Update()
    {
        // 寿命を管理
        timer += Time.deltaTime;

        // カメラが動く場合、目標位置をカメラ位置に更新
        targetPosition = cameraTransform.position;

        // オブジェクトが完全に非アクティブになるのを防ぐために
        // 寿命チェックを最後に移動し、その間は見えるように処理を続ける

        // 途中停止時間に達したかチェック
        if (timer >= midPointTime && !hasReachedMidPoint && currentPhase != FlightPhase.MidPoint)
        {
            // 途中点に到達
            currentPhase = FlightPhase.MidPoint;
            hasReachedMidPoint = true;
            midPointStartTime = timer; // 静止開始時間を記録

            // 現在位置を途中停止位置として記録
            midPointPosition = transform.position;

            // サイズを途中停止時のサイズに設定（ホーミング終了時点）
            // ただし拡大は続ける
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

            // 停止後もスケール更新を続ける
            UpdateScaleAfterStop();

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

            // スケールの更新
            UpdateScale();

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
                    midPointStartTime = timer; // 静止開始時間を記録
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
                // 到達後は目標位置にとどまる
                transform.position = targetPosition;

                // 到達後は移動しないように速度をゼロに
                currentSpeed = 0f;
                break;
        }
    }

    /// <summary>
    /// スケールを更新する（移動中）
    /// </summary>
    private void UpdateScale()
    {
        // ホーミングフェーズでのみスケールを更新
        if (currentPhase == FlightPhase.Homing)
        {
            // ホーミング開始からの経過時間
            float homingTime = timer - homingStartTime;
            // ホーミングの総時間
            float totalHomingTime = midPointTime - homingStartTime;

            // 進行度に応じてスケールを変更（0～1の範囲）
            float scaleProgress = Mathf.Clamp01(homingTime / totalHomingTime);

            // イージング関数を適用して滑らかに拡大（徐々に加速する拡大）
            float easedProgress = scaleProgress * scaleProgress;

            // スケールを線形に補間
            float currentScaleFactor = Mathf.Lerp(1.0f, finalScale, easedProgress);
            transform.localScale = InitialScale * currentScaleFactor;
        }
        // 初期フェーズとカーブフェーズでは元のサイズ
        else if (currentPhase == FlightPhase.Initial || currentPhase == FlightPhase.Curving)
        {
            transform.localScale = InitialScale;
        }
    }

    /// <summary>
    /// 停止後もスケールを継続的に更新
    /// </summary>
    private void UpdateScaleAfterStop()
    {
        // 静止開始からの経過時間
        float timeAfterStop = timer - midPointStartTime;

        // ホーミング終了時点での拡大率
        float scaleAtStop = finalScale;

        // 継続的な拡大（元の拡大速度の1/5）
        float additionalScaleFactor = timeAfterStop * continuousScaleFactor;

        // 最大追加拡大率を制限
        additionalScaleFactor = Mathf.Min(additionalScaleFactor, maxAdditionalScale);

        // 最終スケールを計算
        float totalScale = scaleAtStop + additionalScaleFactor;

        // スケールを適用
        transform.localScale = InitialScale * totalScale;
    }

    private void UpdateRotation()
    {
        // シンプルな実装 - 現在の移動方向に向けて回転
        if (currentPhase != FlightPhase.MidPoint && currentPhase != FlightPhase.Arrived)
        {
            // 現在のフレームでの移動方向
            Vector3 moveDirection;

            if (currentPhase == FlightPhase.Initial)
            {
                moveDirection = initialDirection;
            }
            else if (currentPhase == FlightPhase.Curving)
            {
                // カーブ中は混合方向
                float curveRatio = Mathf.Clamp01((timer - initialPhaseTime) / curvePhaseTime);
                Vector3 toCameraDirection = (targetPosition - transform.position).normalized;
                moveDirection = Vector3.Lerp(initialDirection, toCameraDirection, curveRatio) + curveDirection * (1f - curveRatio * curveRatio);
                moveDirection.Normalize();
            }
            else // Homing
            {
                moveDirection = (targetPosition - transform.position).normalized;
            }

            // 向きを移動方向に合わせる
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
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

            // 初期方向
            if (initialDirection != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + initialDirection.normalized * 3);
            }

            // 生成位置とカメラを結ぶ軸
            if (cameraTransform != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, cameraTransform.position);
            }

            // カーブの膨らみ方向
            if (curveDirection != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + curveDirection.normalized * 2);
            }

            // 途中停止位置
            if (hasReachedMidPoint)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(midPointPosition, 0.4f);
            }

            // 現在の移動フェーズを表示
            string phaseText = currentPhase.ToString();
            switch (currentPhase)
            {
                case FlightPhase.Initial:
                    Gizmos.color = Color.white;
                    break;
                case FlightPhase.Curving:
                    Gizmos.color = Color.yellow;
                    break;
                case FlightPhase.Homing:
                    Gizmos.color = Color.blue;
                    break;
                case FlightPhase.MidPoint:
                    Gizmos.color = Color.magenta;
                    break;
                case FlightPhase.Arrived:
                    Gizmos.color = Color.green;
                    break;
            }
            Gizmos.DrawWireSphere(transform.position, 0.6f);
        }
    }
}