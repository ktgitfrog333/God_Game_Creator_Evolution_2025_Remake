using UnityEngine;
using System.Collections;

namespace Mains.Script_xylo
{

public class Test_Input : MonoBehaviour
{
    public GameObject targetCube;
    private Material cubeMaterial;
    private Color originalColor;
    private Coroutine colorChangeCoroutine;

    // クールダウン関連の変数を追加
    private float cooldownDuration = 0.2f; // クールダウン時間（秒）
    private float lastInputTime = -1f;     // 最後の入力時間

    private void Start()
    {
        if (targetCube != null)
        {
            Renderer renderer = targetCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                cubeMaterial = renderer.material;
                originalColor = cubeMaterial.color;
            }
        }
    }

    private void Update()
    {
        // スペースキーが押された時のみ判定を行う
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // クールダウン中でないかチェック
            if (Time.time - lastInputTime >= cooldownDuration)
            {
                CheckBeatAndChangeColor();
                lastInputTime = Time.time; // 入力時間を更新
            }
        }
    }

    private void CheckBeatAndChangeColor()
    {
        CRIWARE_conductor.BeatResult result = CRIWARE_conductor.Instance.JustBeatTick();
        if (result != CRIWARE_conductor.BeatResult.Miss)
        {
            ChangeColor(Color.green * 0.8f); // 明るい緑色
        }
        else
        {
            ChangeColor(Color.red); // 赤色
        }
    }

    private void ChangeColor(Color newColor)
    {
        if (cubeMaterial != null)
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }
            colorChangeCoroutine = StartCoroutine(LerpColor(newColor));
        }
    }

    private IEnumerator LerpColor(Color targetColor)
    {
        float elapsedTime = 0f;
        float duration = 0.3f;
        Color startColor = cubeMaterial.color;

        cubeMaterial.color = targetColor;
        yield return null;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            cubeMaterial.color = Color.Lerp(targetColor, originalColor, t);
            yield return null;
        }

        cubeMaterial.color = originalColor;
        colorChangeCoroutine = null;
    }
}
}
