using Cysharp.Threading.Tasks;
using Mains.Views;
using Rewired;
using System.Threading;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// TutorialSequencerが外部サービス（BGM・マイク等）に直接依存しないよう、
    /// 副作用操作を抽象化するインターフェース
    /// </summary>
    public interface ITutorialSideEffect : System.IDisposable
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
        UniTask TeleportPlayerAsync(Vector3 position, Vector3 angles, bool isCompletedStartDirection, CharacterController playerCharacterController, Player player, Transform playerTransform, Transform playerHead, PlayerView playerView, FadeImageView fadeImageView, CancellationToken token);

        /// <summary>オバケ笑い声SEを再生する</summary>
        void PlayGhostLaughV2Normal();

        /// <summary>オバケ笑い声SEを再生する</summary>
        R3.Observable<R3.Unit> EnabledAnimator(Animator missGhostEscapeNormalAnimator, MissGhostEscapeView missGhostEscapeView);

        void SetMissileTempoSpawner(Transform transform);

        /// <summary>最初のHomingObjectスポーン監視を開始する</summary>
        void WatchFirstHomingObjectSpawn(int targetIndex);

        /// <summary>最初のHomingObjectスポーン通知</summary>
        R3.Observable<R3.Unit> OnFirstHomingObjectSpawned { get; }

        /// <summary>アクティブなショートノーツのクリック受付判定</summary>
        bool IsAnyShortNoteClickable(int targetIndex);

        /// <summary>アクティブなロングノーツのクリック受付判定</summary>
        bool IsAnyLongNoteClickable(int targetIndex);

        /// <summary>判定可能状態のノーツに対して強制的にGOOD判定（クリック）を行う</summary>
        void ForceClickAnyClickableNote(int targetIndex);

        /// <summary>自動成功モードへ強制的に切り替える処理</summary>
        /// <param name="targetIndex">対象のインデックス</param>
        /// <param name="autoMode">自動成功モード有効／無効</param>
        void ForceSetAutoMode(int targetIndex, bool autoMode);

        /// <summary>成功／失敗時の生成オバケのプーラーをセット</summary>
        /// <param name="transform">成功／失敗時の生成オバケのプーラー</param>
        public void SetObjectPoolerXyloOther(Transform transform);

        /// <summary>ターゲットクロスと直近ノーツのスクリーン距離を取得する</summary>
        float GetNoteToCrosshairScreenDistance(int targetIndex);
    }
}
