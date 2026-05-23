using Selects.ViewModels;

namespace Selects.Views
{
    /// <summary>
    /// TutorialSequencerが必要とする全依存をまとめるコンテキスト。
    /// コンストラクタ引数を増やさず、依存を一箇所に集約する。
    /// </summary>
    public class TutorialSequencerContext
    {
        /// <summary>UIへの抽象（フェード・メッセージ操作）</summary>
        public ITutorialUI UI { get; }

        /// <summary>入力への抽象（Rewired）</summary>
        public ITutorialInput Input { get; }

        /// <summary>副作用への抽象（BGM・マイク・ノーツ）</summary>
        public ITutorialSideEffect SideEffect { get; }

        /// <summary>ビューモデル（プレイヤー位置・DbLevel等）</summary>
        public TutorialPanelViewModel ViewModel { get; }

        /// <summary>レベル上のチュートリアル専用オブジェクト群</summary>
        public TutorialPanelSettings.LevelObjects LevelObjects { get; }

        /// <summary>テーブル（メッセージ・ミサイルパターン）</summary>
        public TutorialPanelSettings.Tables Tables { get; }

        public TutorialSequencerContext(
            ITutorialUI ui,
            ITutorialInput input,
            ITutorialSideEffect sideEffect,
            TutorialPanelViewModel viewModel,
            TutorialPanelSettings.LevelObjects levelObjects,
            TutorialPanelSettings.Tables tables)
        {
            UI = ui;
            Input = input;
            SideEffect = sideEffect;
            ViewModel = viewModel;
            LevelObjects = levelObjects;
            Tables = tables;
        }
    }
}
