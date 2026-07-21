using Cysharp.Threading.Tasks;
using Mains.Commons;
using Mains.External;
using Mains.Views;
using R3;
using Rewired;
using System.Threading;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// ITutorialSideEffect の Script_xyloApi 実装。
    /// Sequencer を外部BGMシステムから切り離すためのアダプター。
    /// </summary>
    public class XyloApiTutorialSideEffect : ITutorialSideEffect
    {
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private readonly Script_xyloApi _api;
        /// <summary>プレイヤー移動演出ストラテジー</summary>
        private readonly IPlayerTeleporterStrategySO _teleporterStrategy;
        private DisposableBag _disposableBag = new DisposableBag();

        public XyloApiTutorialSideEffect(Script_xyloApi api, IPlayerTeleporterStrategySO teleporterStrategy)
        {
            _api = api;
            _teleporterStrategy = teleporterStrategy;
        }

        /// <summary>
        /// マイク入力の有効／無効切り替え
        /// </summary>
        /// <remarks>ゲームオブジェクトを無効にするのではなく、<br/>
        /// スクリプト側で用意されたメソッドを呼び出す</remarks>
        /// <param name="active">有効／無効</param>
        public void SetMicrophoneActive(bool active) => _api.SetMicrophoneActive(active);
        /// <summary>
        /// 曲の再生時間を一時停止
        /// </summary>
        /// <param name="pause">一時停止／再生</param>
        public void SetBgmPause(bool pause) => _api.SetBgmPause(pause);
        /// <summary>
        /// ノーツ入力の有効／無効切り替え
        /// </summary>
        /// <param name="active">有効／無効</param>
        public void SetAllNotesClickDetection(bool active) => _api.SetAllNotesClickDetection(active);
        /// <summary>
        /// ノーツ生成パターンを更新
        /// </summary>
        /// <summary>ノーツ生成パターンを更新</summary>
        /// <param name="pattern">ノーツ生成パターン</param>
        public void SetMissilePattern(string pattern) => _api.SetMissilePattern(pattern);

        public UniTask TeleportPlayerAsync(Vector3 position, Vector3 angles, bool isCompletedStartDirection, CharacterController playerCharacterController, Player player, Transform playerTransform, Transform playerHead, PlayerView playerView, FadeImageView fadeImageView, CancellationToken token)
        {
            return _teleporterStrategy.TeleportPlayer(position, angles, isCompletedStartDirection, playerCharacterController, player, playerTransform, playerHead, playerView, fadeImageView, token);
        }

        public void PlayGhostLaughV2Normal()
        {
            if (_api.IsInstanceSE_Picker())
            {
                _api.PlayGhostLaughByVoiceType(GhostVoiceType.ghost_voice_normal_type);
            }
            else
            {
                Observable.EveryUpdate()
                    .Where(_ => _api.IsInstanceSE_Picker())
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        _api.PlayGhostLaughByVoiceType(GhostVoiceType.ghost_voice_normal_type);
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        public Observable<Unit> EnabledAnimator(Animator missGhostEscapeNormalAnimator, MissGhostEscapeView missGhostEscapeView)
        {
            return Observable.Create<Unit>(observer =>
            {
                missGhostEscapeView.IsEscapeCompleted.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                    .AddTo(ref _disposableBag);
                missGhostEscapeView.SetTriggerAnimator("Escape");

                return Disposable.Empty;
            });
        }

        public void WatchFirstHomingObjectSpawn() => _api.WatchFirstHomingObjectSpawn();
        public Observable<Unit> OnFirstHomingObjectSpawned => _api.OnFirstHomingObjectSpawned;
        public Observable<Unit> OnGhostHomingStarted() => _api.OnGhostHomingStarted();
        public bool IsAnyShortNoteClickable() => _api.IsAnyShortNoteClickable();
        public bool IsAnyLongNoteClickable() => _api.IsAnyLongNoteClickable();

        public Observable<bool> OnNoteSuccessful => _api.IsSuccessfulReactive;
        public Observable<bool> OnNoteFailed => _api.IsFailedReactive;
        public Observable<Unit> OnHpDecreased => _api.OnHpDecreased;
        public Observable<Unit> OnBatteryPicked => _api.OnBatteryPicked;

        public void ForceClickAnyClickableNote() => _api.ForceClickAnyClickableNote();
        public void ClearAllAttackingGhosts() => _api.ClearAllAttackingGhosts();
        public float GetNoteToCrosshairScreenDistance() => _api.GetNoteToCrosshairScreenDistance();

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
