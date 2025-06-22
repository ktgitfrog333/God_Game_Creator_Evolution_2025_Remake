using System.Collections;
using UnityEngine;

public class SimpleMyakudou : MonoBehaviour
{
    // サイズの設定
    public float minSize = 0.9f; // 最小サイズ
    public float maxSize = 1.1f; // 最大サイズ

    // Y軸のみ拡縮するかどうかのフラグ
    public bool Y_Only = false;

    // 各拍に反応するかどうかのフラグ
    public bool beat1 = true;
    public bool beat2 = true;
    public bool beat3 = true;
    public bool beat4 = true;

    // ビートに合わせて拡縮する速度の設定
    [SerializeField] private float expandDuration = 0.1f; // 拡大にかかる時間
    [SerializeField] private float contractDuration = 0.4f; // 縮小にかかる時間

    // 脈動中かどうかのフラグ
    private bool isPulsing = false;
    private Coroutine pulseCoroutine;

    private Vector3 originalScale; // 元のスケール保存用

    private void Awake()
    {
        // 元のスケールを保存
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // CRIWAREのビートイベントに登録（拍ごとに異なるメソッドを割り当て）
        CRIWARE_conductor.TempoMethodEvent1 += PulseOnBeat1;
        CRIWARE_conductor.TempoMethodEvent2 += PulseOnBeat2;
        CRIWARE_conductor.TempoMethodEvent3 += PulseOnBeat3;
        CRIWARE_conductor.TempoMethodEvent4 += PulseOnBeat4;
        CRIWARE_conductor.TempoMethodEvent5 += PulseOnBeat1;
        CRIWARE_conductor.TempoMethodEvent6 += PulseOnBeat2;
        CRIWARE_conductor.TempoMethodEvent7 += PulseOnBeat3;
        CRIWARE_conductor.TempoMethodEvent8 += PulseOnBeat4;
    }

    private void OnDisable()
    {
        // イベント登録解除
        CRIWARE_conductor.TempoMethodEvent1 -= PulseOnBeat1;
        CRIWARE_conductor.TempoMethodEvent2 -= PulseOnBeat2;
        CRIWARE_conductor.TempoMethodEvent3 -= PulseOnBeat3;
        CRIWARE_conductor.TempoMethodEvent4 -= PulseOnBeat4;
        CRIWARE_conductor.TempoMethodEvent5 -= PulseOnBeat1;
        CRIWARE_conductor.TempoMethodEvent6 -= PulseOnBeat2;
        CRIWARE_conductor.TempoMethodEvent7 -= PulseOnBeat3;
        CRIWARE_conductor.TempoMethodEvent8 -= PulseOnBeat4;

        // コンポーネントが無効になったらコルーチンを停止し、元のスケールに戻す
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            transform.localScale = originalScale;
        }
    }

    // ビート1に合わせて呼び出される関数
    void PulseOnBeat1()
    {
        // beat1がオフならなにもしない
        if (!beat1) return;

        // 脈動処理を実行
        TriggerPulse();
    }

    // ビート2に合わせて呼び出される関数
    void PulseOnBeat2()
    {
        // beat2がオフならなにもしない
        if (!beat2) return;

        // 脈動処理を実行
        TriggerPulse();
    }

    // ビート3に合わせて呼び出される関数
    void PulseOnBeat3()
    {
        // beat3がオフならなにもしない
        if (!beat3) return;

        // 脈動処理を実行
        TriggerPulse();
    }

    // ビート4に合わせて呼び出される関数
    void PulseOnBeat4()
    {
        // beat4がオフならなにもしない
        if (!beat4) return;

        // 脈動処理を実行
        TriggerPulse();
    }

    // 実際に脈動処理を開始する共通関数
    void TriggerPulse()
    {
        // 既に脈動中の場合は新しい脈動をキャンセルする（オプション）
        if (isPulsing)
        {
            // 現在進行中の脈動を停止
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }
        }

        // 新しい脈動を開始
        pulseCoroutine = StartCoroutine(PulseCoroutine());
    }

    private IEnumerator PulseCoroutine()
    {
        isPulsing = true;

        // 現在のスケール
        Vector3 startScale = transform.localScale;
        // 最大スケールを計算
        Vector3 maxScaleVec;

        if (Y_Only)
        {
            // Y軸のみ拡大する場合
            maxScaleVec = new Vector3(
                originalScale.x,
                originalScale.y * maxSize,
                originalScale.z
            );
        }
        else
        {
            // 全方向に拡大する場合（元のコード通り）
            maxScaleVec = originalScale * maxSize;
        }

        // 拡大フェーズ
        float elapsedTime = 0f;
        while (elapsedTime < expandDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Min(elapsedTime / expandDuration, 1.0f);

            // 現在のスケールを線形補間で計算
            transform.localScale = Vector3.Lerp(startScale, maxScaleVec, progress);

            yield return null;
        }

        // 最大サイズに到達したことを確認
        transform.localScale = maxScaleVec;

        // 最小スケールを計算
        Vector3 minScaleVec;

        if (Y_Only)
        {
            // Y軸のみ縮小する場合
            minScaleVec = new Vector3(
                originalScale.x,
                originalScale.y * minSize,
                originalScale.z
            );
        }
        else
        {
            // 全方向に縮小する場合（元のコード通り）
            minScaleVec = originalScale * minSize;
        }

        // 縮小フェーズ
        elapsedTime = 0f;
        while (elapsedTime < contractDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Min(elapsedTime / contractDuration, 1.0f);

            // イージング関数で自然な縮小を演出
            float easedProgress = 1f - (1f - progress) * (1f - progress);
            transform.localScale = Vector3.Lerp(maxScaleVec, minScaleVec, easedProgress);

            yield return null;
        }

        // 最小サイズに確実に設定
        transform.localScale = minScaleVec;

        // 元のスケールに戻る
        elapsedTime = 0f;
        float returnDuration = contractDuration * 0.5f; // 戻る時間は縮小時間の半分

        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Min(elapsedTime / returnDuration, 1.0f);

            // 線形補間で元のスケールに戻る
            transform.localScale = Vector3.Lerp(minScaleVec, originalScale, progress);

            yield return null;
        }

        // 元のスケールに確実に設定
        transform.localScale = originalScale;

        isPulsing = false;
    }
}