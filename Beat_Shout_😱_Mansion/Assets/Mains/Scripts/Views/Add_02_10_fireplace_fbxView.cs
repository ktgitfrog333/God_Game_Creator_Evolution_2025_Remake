using R3;
using UnityEngine;
using UnityEngine.Playables;
using System.Linq;

namespace Mains.Views
{
    /// <summary>
    /// 暖炉FBXモデルの追加処理のビュー
    /// </summary>
    public class Add_02_10_fireplace_fbxView : MonoBehaviour
    {
        /// <summary>暖炉FBXモデルの追加処理の設定</summary>
        [SerializeField] private Add_02_10_fireplace_fbxSettings settings;
        /// <summary>点灯コマンドコールバック</summary>
        private ReactiveCommand<Unit> _onBUB_FireFireplace = new ReactiveCommand<Unit>();
        /// <summary>点灯コマンドコールバック</summary>
        public ReactiveCommand<Unit> OnBUB_FireFireplace => _onBUB_FireFireplace;
        /// <summary>点灯継続コマンドコールバック</summary>
        private ReactiveCommand<Unit> _onBUB_FireBurningFireplace = new ReactiveCommand<Unit>();
        /// <summary>点灯継続コマンドコールバック</summary>
        public ReactiveCommand<Unit> OnBUB_FireBurningFireplace => _onBUB_FireBurningFireplace;
        /// <summary>消灯コマンドコールバック</summary>
        private ReactiveCommand<Unit> _onBUB_FireGoingOut = new ReactiveCommand<Unit>();
        /// <summary>消灯コマンドコールバック</summary>
        public ReactiveCommand<Unit> OnBUB_FireGoingOut => _onBUB_FireGoingOut;
        /// <summary>演出の停止</summary>
        private ReactiveCommand<Unit> _onStoppedDirection = new ReactiveCommand<Unit>();
        /// <summary>演出の停止</summary>
        public ReactiveCommand<Unit> OnStoppedDirection => _onStoppedDirection;
        /// <summary>初期処理の完了フラグ</summary>
        private readonly ReactiveProperty<bool> _isCompletedStarted = new ReactiveProperty<bool>();
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            var set = settings;

            if (set.playableDirector == null)
                set.playableDirector = GetComponent<PlayableDirector>();
            if (set.fireplaceFireParticleSysGameObjects == null || set.fireplaceFireParticleSysGameObjects.Length == 0)
            {
                set.fireplaceFireParticleSysGameObjects = GetComponentsInChildren<ParticleSystem>(true)
                    .Select(p => p.gameObject)
                    .Where(go => go.name.Contains("fireplace_fire"))
                    .ToArray();
            }
        }

        private void Start()
        {
            var set = settings;
            var playableDirector = set.playableDirector;

            playableDirector.stopped += OnTimelineStopped;

            _isCompletedStarted.Value = true;
        }

        private void OnDestroy()
        {
            var set = settings;
            var playableDirector = set.playableDirector;

            playableDirector.stopped -= OnTimelineStopped;
        }

        /// <summary>
        /// 演出の再生
        /// </summary>
        public void PlayDirection()
        {
            var set = settings;
            var playableDirector = set.playableDirector;

            if (_isCompletedStarted.CurrentValue)
            {
                playableDirector.Play();
            }
            else
            {
                Observable.EveryUpdate()
                    .Where(_ => _isCompletedStarted.CurrentValue)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        playableDirector.Play();
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        /// <summary>
        /// 演出の停止
        /// </summary>
        public void StopDirection()
        {
            var set = settings;
            var playableDirector = set.playableDirector;
            var fireplaceFireParticleSysGameObjects = set.fireplaceFireParticleSysGameObjects;

            playableDirector.Stop();

            // パーティクルがリセットされていないので初期状態に戻す
            foreach (var fireplaceFireParticleSysGameObject in fireplaceFireParticleSysGameObjects)
            {
                if (fireplaceFireParticleSysGameObject.activeSelf)
                {
                    fireplaceFireParticleSysGameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 点灯コールバック
        /// </summary>
        /// <remarks><see cref="Assets/Mains/Animations/Add_02_10_fireplace_fbx/OnBUB_FireFireplace.signal"/></remarks>
        public void DoOnBUB_FireFireplace()
        {
            _onBUB_FireFireplace.Execute(Unit.Default);
        }

        /// <summary>
        /// 点灯継続コールバック
        /// </summary>
        /// <remarks><see cref="Assets/Mains/Animations/Add_02_10_fireplace_fbx/OnBUB_FireBurningFireplace.signal"/></remarks>
        public void DoOnBUB_FireBurningFireplace()
        {
            _onBUB_FireBurningFireplace.Execute(Unit.Default);
        }

        /// <summary>
        /// 消灯コールバック
        /// </summary>
        /// <remarks><see cref="Assets/Mains/Animations/Add_02_10_fireplace_fbx/OnBUB_FireGoingOut.signal"/></remarks>
        public void DoOnBUB_FireGoingOut()
        {
            _onBUB_FireGoingOut.Execute(Unit.Default);
        }

        /// <summary>
        /// タイムライン停止
        /// </summary>
        /// <param name="d">演出</param>
        private void OnTimelineStopped(PlayableDirector d)
        {
            _onStoppedDirection.Execute(Unit.Default);
        }
    }

    /// <summary>
    /// 暖炉FBXモデルの追加処理の設定
    /// </summary>
    [System.Serializable]
    public class Add_02_10_fireplace_fbxSettings
    {
        /// <summary>演出</summary>
        public PlayableDirector playableDirector;
        /// <summary>暖炉FBXのパーティクルのビュー配列</summary>
        public GameObject[] fireplaceFireParticleSysGameObjects;
    }
}
