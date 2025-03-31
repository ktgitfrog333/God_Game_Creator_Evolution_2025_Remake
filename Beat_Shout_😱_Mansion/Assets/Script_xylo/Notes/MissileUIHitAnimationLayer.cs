using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ヒットエフェクト専用のアニメーションレイヤー
/// 一度再生して終了する特殊なアニメーション処理
/// </summary>
public class MissileUIHitAnimationLayer : MissileUIAnimationLayer
{
    /// <summary>
    /// ヒットアニメーション用のコルーチン（1回再生して終了）
    /// </summary>
    protected override IEnumerator AnimationCoroutine()
    {
        isPlaying = true;

        // スプライトがない場合は何もしない
        if (sprites == null || sprites.Length == 0)
        {
            isPlaying = false;
            yield break;
        }

        // 最初のフレームを表示
        currentFrame = 0;
        image.sprite = sprites[currentFrame];

        // 全フレームを再生
        for (int i = 1; i < sprites.Length; i++)
        {
            // フレーム間の時間を待機
            yield return new WaitForSeconds(frameDelay);

            // 次のフレームを表示
            currentFrame = i;
            image.sprite = sprites[currentFrame];
        }

        // アニメーション終了後に非表示にする
        yield return new WaitForSeconds(frameDelay);
        SetVisibility(false);

        isPlaying = false;
        animationCoroutine = null;
    }
}