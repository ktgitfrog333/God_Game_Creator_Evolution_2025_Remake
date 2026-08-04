using Cysharp.Threading.Tasks;
using R3;
using System.Threading;

namespace Selects.Views
{
    /// <summary>
    /// TutorialSequencerがViewに依存しないよう、UI操作を抽象化するインターフェース
    /// </summary>
    public interface ITutorialUI
    {
        /// <summary>メッセージIDに対応するメッセージを表示する</summary>
        /// <param name="messageId">メッセージID</param>
        void ApplyMessage(string messageId);

        /// <summary>進捗付きメッセージを表示する（例: 3/5）</summary>
        /// <param name="messageId">メッセージID</param>
        /// <param name="current">現在値</param>
        /// <param name="total">最大値</param>
        void ApplyMessageWithProgress(string messageId, string current, string total);

        /// <summary>メッセージをリセット（全消去）する</summary>
        void ResetMessages();

        /// <summary>パネルをフェードインする</summary>
        /// <param name="duration">アニメーション終了時間</param>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        UniTask FadeInAsync(float duration, CancellationToken token);

        /// <summary>パネルをフェードアウトする</summary>
        /// <param name="duration">アニメーション終了時間</param>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        UniTask FadeOutAsync(float duration, CancellationToken token);

        /// <summary>
        /// 共通UIのビューの有効／無効を切り替える
        /// </summary>
        /// <param name="enabled">有効／無効</param>
        void SetCommonPanelCustomizeOfMainViewEnabled(bool enabled);

        /// <summary>共通UIのビュー</summary>
        CommonPanelCustomizeOfMainView CommonPanelCustomizeOfMainView { get; }

        /// <summary>ステージ1の案内演出の再生</summary>
        void PlayStage1GuideDirection();

        /// <summary>ステージ1の案内演出の完了フラグ</summary>
        ReadOnlyReactiveProperty<bool> IsCompletedStage1GuideDirection { get; }

        /// <summary>初期処理の完了フラグ</summary>
        ReadOnlyReactiveProperty<bool> IsCompletedStart { get; }

        /// <summary>オブジェクトの有効／無効フラグ</summary>
        ReadOnlyReactiveProperty<bool> IsEnabled { get; }

        /// <summary>オブジェクトの有効／無効をセット</summary>
        void SetEnabledTutorialPanel(bool enabled);
    }
}
