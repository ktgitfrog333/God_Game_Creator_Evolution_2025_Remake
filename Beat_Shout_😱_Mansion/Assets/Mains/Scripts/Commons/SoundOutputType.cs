using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// 音の出力タイプ
    /// </summary>
    public enum SoundOutputType
    {
        /// <summary>PoltergeistTableのデフォルト値を使用する</summary>
        TableDefault,
        /// <summary>常に一定間隔で音を出力する</summary>
        Loop,
        /// <summary>プレイヤーが動いていない（静止している）時に音を出力する</summary>
        ReactiveStatic,
        /// <summary>シャウト終了後にコール＆レスポンスとして音と演出を出力する</summary>
        ReactiveShout_CallAndResponse
    }
}
