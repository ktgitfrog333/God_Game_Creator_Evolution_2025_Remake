using R3;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// TutorialSequencerがRewiredに直接依存しないよう、入力操作を抽象化するインターフェース
    /// </summary>
    public interface ITutorialInput
    {
        /// <summary>縦軸移動の入力値（-1〜1）</summary>
        float MoveVertical { get; }

        /// <summary>横軸移動の入力値（-1〜1）</summary>
        float MoveHorizontal { get; }

        /// <summary>水平視点の入力値（-1〜1）</summary>
        float AimMoveHorizontal { get; }

        /// <summary>垂直視点の入力値（-1〜1）</summary>
        float AimMoveVertical { get; }

        /// <summary>サーチボタンが押された瞬間かどうか</summary>
        bool SearchButtonDown { get; }

        /// <summary>マイクスイッチが押された瞬間かどうか</summary>
        bool SwitchPartButtonDown { get; }

        /// <summary>タップライトが押された瞬間かどうか</summary>
        bool TapLightButtonDown { get; }

        /// <summary>指定カテゴリのみコントローラマップを有効化（nullで全無効）</summary>
        /// <param name="categoryName">Mapカテゴリ名</param>
        void EnableOnlyControllerMapCategory(string categoryName);
    }
}
