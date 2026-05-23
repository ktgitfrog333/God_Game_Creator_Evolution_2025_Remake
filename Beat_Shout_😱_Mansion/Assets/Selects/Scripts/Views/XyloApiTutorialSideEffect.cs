using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mains.External;
using Mains.Views;
using System.Threading;

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
        private readonly UnityEngine.CharacterController _playerController;
        private readonly UnityEngine.Transform _playerTransform;

        public XyloApiTutorialSideEffect(Script_xyloApi api, UnityEngine.CharacterController playerController, UnityEngine.Transform playerTransform)
        {
            _api = api;
            _playerController = playerController;
            _playerTransform = playerTransform;
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

        public async UniTask TeleportPlayerAsync(UnityEngine.Vector3 position, UnityEngine.Vector3 angles, CancellationToken token)
        {
            var seq = DOTween.Sequence();
            if (_playerController != null) _playerController.enabled = false;
            if (_playerTransform != null)
            {
                //_playerTransform.SetPositionAndRotation(position, rotation);
                _ = seq.Append(_playerTransform.DOMove(position, 1f))
                    .Join(_playerTransform.DORotate(angles, 1f))
                    .AppendCallback(() =>
                    {
                        if (_playerController != null) _playerController.enabled = true;
                    });
            }

            await seq.ToUniTask(cancellationToken: token);
        }

        public void WatchFirstHomingObjectSpawn() => _api.WatchFirstHomingObjectSpawn();
        public R3.Observable<R3.Unit> OnFirstHomingObjectSpawned => _api.OnFirstHomingObjectSpawned;
        public R3.Observable<R3.Unit> OnGhostHomingStarted() => _api.OnGhostHomingStarted();
        public bool IsAnyShortNoteClickable() => _api.IsAnyShortNoteClickable();
        public bool IsAnyLongNoteClickable() => _api.IsAnyLongNoteClickable();

        public R3.Observable<bool> OnNoteSuccessful => _api.IsSuccessfulReactive;
        public R3.Observable<bool> OnNoteFailed => _api.IsFailedReactive;
        public R3.Observable<R3.Unit> OnHpDecreased => _api.OnHpDecreased;
        public R3.Observable<R3.Unit> OnBatteryPicked => _api.OnBatteryPicked;

        public void ForceClickAnyClickableNote() => _api.ForceClickAnyClickableNote();
        public void ClearAllAttackingGhosts() => _api.ClearAllAttackingGhosts();
        public float GetNoteToCrosshairScreenDistance() => _api.GetNoteToCrosshairScreenDistance();
    }
}
