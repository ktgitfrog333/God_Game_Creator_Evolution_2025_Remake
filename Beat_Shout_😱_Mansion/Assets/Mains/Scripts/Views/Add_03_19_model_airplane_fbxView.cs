using DG.Tweening;
using R3;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// 木製の飛行機FBXモデルの追加処理のビュー
    /// </summary>
    [CreateAssetMenu(fileName = "Add_03_19_model_airplane_fbxView", menuName = "Scriptable Objects/Add_03_19_model_airplane_fbxView")]
    public class Add_03_19_model_airplane_fbxView : ScriptableObject, System.IDisposable
    {
        /// <summary>演出の停止</summary>
        private ReactiveCommand<Unit> _onStoppedDirection;
        /// <summary>演出の停止</summary>
        public ReactiveCommand<Unit> OnStoppedDirection => _onStoppedDirection;
        /// <summary>初期化完了フラグ</summary>
        private bool _isCompletedInitialized;
        /// <summary>スキンメッシュレンダラー</summary>
        private SkinnedMeshRenderer _airplaneRenderer;
        /// <summary>キーのインデックス</summary>
        private int _airplaneKeyIndex;
        /// <summary>キーの値</summary>
        private float _airplaneKeyValue;
        /// <summary>移動の終了時間</summary>
        [SerializeField] private float moveDuration = 1.0f;
        /// <summary>初期位置移動の終了時間</summary>
        [SerializeField] private float returnDuration = .5f;
        /// <summary>実行中アニメーションTweener</summary>
        private Sequence _sequence;

        public void Initialize(SkinnedMeshRenderer airplaneRenderer)
        {
            if (_isCompletedInitialized) return;

            _onStoppedDirection = new ReactiveCommand<Unit>();

            _airplaneRenderer = airplaneRenderer;
            _airplaneKeyIndex = airplaneRenderer.sharedMesh.GetBlendShapeIndex("AirplaneKey");

            if (_airplaneKeyIndex == -1)
            {
                Debug.LogError("BlendShape 'AirplaneKey' が見つかりません。");
            }

            _airplaneKeyValue = airplaneRenderer.GetBlendShapeWeight(_airplaneKeyIndex);
            _isCompletedInitialized = true;
        }

        /// <summary>
        /// 演出の再生
        /// </summary>
        public void PlayDirection()
        {
            var airplaneKeyIndex = _airplaneKeyIndex;

            if (airplaneKeyIndex < 0) return;

            // ワイヤーの長さを変化させるアニメーションの実装
            var sequence = _sequence;
            var airplaneRenderer = _airplaneRenderer;
            float toAirplaneKeyValue = 100f;
            var airplaneKeyValue = _airplaneKeyValue;

            // 100%下降
            sequence = DOTween.Sequence()
                .Append(DOTween.To(
                    () => airplaneRenderer.GetBlendShapeWeight(airplaneKeyIndex),
                    value => airplaneRenderer.SetBlendShapeWeight(airplaneKeyIndex, value),
                    toAirplaneKeyValue,
                    moveDuration
                ).SetEase(Ease.OutCubic))
                .Append(DOTween.To(
                    () => airplaneRenderer.GetBlendShapeWeight(airplaneKeyIndex),
                    value => airplaneRenderer.SetBlendShapeWeight(airplaneKeyIndex, value),
                    airplaneKeyValue,
                    returnDuration
                ).SetEase(Ease.InOutSine))
                .AppendCallback(() => _onStoppedDirection.Execute(Unit.Default));

            _sequence = sequence;
        }

        /// <summary>
        /// 演出の停止
        /// </summary>
        public void StopDirection()
        {
            var airplaneKeyIndex = _airplaneKeyIndex;

            if (airplaneKeyIndex < 0)
            {
                _onStoppedDirection.Execute(Unit.Default);

                return;
            }

            var tweener = _sequence;
            var airplaneRenderer = _airplaneRenderer;

            // アニメーションが実行中なら停止
            if (tweener != null && tweener.IsPlaying())
            {
                tweener.Kill();
                airplaneRenderer.SetBlendShapeWeight(airplaneKeyIndex, _airplaneKeyValue);
            }

            _onStoppedDirection.Execute(Unit.Default);
        }

        public void Dispose()
        {
            if (-1 < _airplaneKeyIndex)
            {
                // 初期値へ戻す
                _airplaneRenderer.SetBlendShapeWeight(_airplaneKeyIndex, _airplaneKeyValue);
            }

            _sequence = null;
            _airplaneKeyValue = 0f;
            _airplaneRenderer = null;
            _airplaneKeyIndex = 0;

            _onStoppedDirection = null;
            _isCompletedInitialized = false;
        }
    }
}
