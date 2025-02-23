using UnityEngine;
using System.Collections;

namespace Mains.Script_xylo
{

public class Test_BeatObj : MonoBehaviour
{
    private Vector3 originalScale; // 元のサイズを保存
    private Coroutine scaleCoroutine; // コルーチンを管理

    public bool Beat01 = false;
    public bool Beat02 = false;
    public bool Beat03 = false;
    public bool Beat04 = false;

    private void OnEnable()
    {
        // 元のサイズを保存
        originalScale = transform.localScale;

        CRIWARE_conductor.TempoMethodEvent1 += TempoMethod1;
        CRIWARE_conductor.TempoMethodEvent2 += TempoMethod2;
        CRIWARE_conductor.TempoMethodEvent3 += TempoMethod3;
        CRIWARE_conductor.TempoMethodEvent4 += TempoMethod4;
        CRIWARE_conductor.TempoMethodEvent5 += TempoMethod5;
        CRIWARE_conductor.TempoMethodEvent6 += TempoMethod6;
        CRIWARE_conductor.TempoMethodEvent7 += TempoMethod7;
        CRIWARE_conductor.TempoMethodEvent8 += TempoMethod8;
    }

    private void OnDisable()
    {
        CRIWARE_conductor.TempoMethodEvent1 -= TempoMethod1;
        CRIWARE_conductor.TempoMethodEvent2 -= TempoMethod2;
        CRIWARE_conductor.TempoMethodEvent3 -= TempoMethod3;
        CRIWARE_conductor.TempoMethodEvent4 -= TempoMethod4;
        CRIWARE_conductor.TempoMethodEvent5 -= TempoMethod5;
        CRIWARE_conductor.TempoMethodEvent6 -= TempoMethod6;
        CRIWARE_conductor.TempoMethodEvent7 -= TempoMethod7;
        CRIWARE_conductor.TempoMethodEvent8 -= TempoMethod8;
    }

    private void TempoMethod1()
    {
        if(!Beat01)
            return;

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine); // 途中で呼ばれた場合は前のコルーチンを停止
        }
        scaleCoroutine = StartCoroutine(ShrinkAndRestore());
    }
    private void TempoMethod2()
    {
        if (!Beat02)
            return;
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine); // 途中で呼ばれた場合は前のコルーチンを停止
        }
        scaleCoroutine = StartCoroutine(ShrinkAndRestore());
    }
    private void TempoMethod3()
    {
        if (!Beat03)
            return;
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine); // 途中で呼ばれた場合は前のコルーチンを停止
        }
        scaleCoroutine = StartCoroutine(ShrinkAndRestore());
    }
    private void TempoMethod4()
    {
        if (!Beat04)
            return;
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine); // 途中で呼ばれた場合は前のコルーチンを停止
        }
        scaleCoroutine = StartCoroutine(ShrinkAndRestore());
    }
    private void TempoMethod5()
    {
        if (!Beat01)
            return;
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine); // 途中で呼ばれた場合は前のコルーチンを停止
        }
        scaleCoroutine = StartCoroutine(ShrinkAndRestore());
    }
    private void TempoMethod6()
    {
        if (!Beat02)
            return;
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine); // 途中で呼ばれた場合は前のコルーチンを停止
        }
        scaleCoroutine = StartCoroutine(ShrinkAndRestore());
    }
    private void TempoMethod7()
    {
        if (!Beat03)
            return;
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine); // 途中で呼ばれた場合は前のコルーチンを停止
        }
        scaleCoroutine = StartCoroutine(ShrinkAndRestore());
    }
    private void TempoMethod8()
    {
        if (!Beat04)
            return;
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine); // 途中で呼ばれた場合は前のコルーチンを停止
        }
        scaleCoroutine = StartCoroutine(ShrinkAndRestore());
    }


    private IEnumerator ShrinkAndRestore()
    {
        // 一瞬で縮む
        transform.localScale = originalScale * 0.5f; // 50%のサイズに縮小

        // 0.2秒かけて元のサイズに戻す
        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale * 0.5f, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale; // 最後に確実に元のサイズに戻す
    }
}

}
