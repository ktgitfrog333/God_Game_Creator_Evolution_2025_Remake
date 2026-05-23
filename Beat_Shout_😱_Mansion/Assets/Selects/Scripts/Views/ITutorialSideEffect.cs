using Cysharp.Threading.Tasks;
using System.Threading;

namespace Selects.Views
{
    /// <summary>
    /// TutorialSequencerが外部サービス（BGM・マイク等）に直接依存しないよう、
    /// 副作用操作を抽象化するインターフェース
    /// </summary>
    public interface ITutorialSideEffect
    {
        /// <summary>マイクの有効／無効を切り替える</summary>
        void SetMicrophoneActive(bool active);

        /// <summary>BGMの一時停止／再開を切り替える</summary>
        void SetBgmPause(bool pause);

        /// <summary>ノーツのクリック検出を有効／無効にする</summary>
        void SetAllNotesClickDetection(bool active);

        /// <summary>ミサイルパターンを設定する</summary>
        void SetMissilePattern(string pattern);

        /// <summary>プレイヤーをテレポートさせる</summary>
        UniTask TeleportPlayerAsync(UnityEngine.Vector3 position, UnityEngine.Vector3 angles, CancellationToken token);

        /// <summary>最初のHomingObjectスポーン監視を開始する</summary>
        void WatchFirstHomingObjectSpawn();

        /// <summary>最初のHomingObjectスポーン通知</summary>
        R3.Observable<R3.Unit> OnFirstHomingObjectSpawned { get; }

        /// <summary>1体目のオバケがHoming状態になった通知</summary>
        R3.Observable<R3.Unit> OnGhostHomingStarted();

        /// <summary>アクティブなショートノーツのクリック受付判定</summary>
        bool IsAnyShortNoteClickable();

        /// <summary>アクティブなロングノーツのクリック受付判定</summary>
        bool IsAnyLongNoteClickable();

        /// <summary>ノーツの成功判定通知</summary>
        R3.Observable<bool> OnNoteSuccessful { get; }

        /// <summary>ノーツの失敗判定通知</summary>
        R3.Observable<bool> OnNoteFailed { get; }

        /// <summary>判定可能状態のノーツに対して強制的にGOOD判定（クリック）を行う</summary>
        void ForceClickAnyClickableNote();

        /// <summary>HP減少通知</summary>
        R3.Observable<R3.Unit> OnHpDecreased { get; }

        /// <summary>落下した電池の取得通知</summary>
        R3.Observable<R3.Unit> OnBatteryPicked { get; }

        /// <summary>攻撃に向かってきているオバケをすべて消去する</summary>
        void ClearAllAttackingGhosts();

        /// <summary>ターゲットクロスと直近ノーツのスクリーン距離を取得する</summary>
        float GetNoteToCrosshairScreenDistance();
    }
}
