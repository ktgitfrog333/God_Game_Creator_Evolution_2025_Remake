using R3;
using Titles.Models;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// セットアップガイドパネルのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "SetupGuidePanelViewModel", menuName = "Scriptable Objects/SetupGuidePanelViewModel")]
    public class SetupGuidePanelViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] private LauncherModel model;
        /// <summary>目的位置までの移動アニメーション終了時間</summary>
        [SerializeField] private float movementAnimationDuration = 1f;
        /// <summary>目的位置までの移動アニメーション終了時間</summary>
        public float MovementAnimationDuration => movementAnimationDuration;
        /// <summary>下から開始の上下移動アニメーション終了時間</summary>
        [SerializeField] private float downUpAnimationDuration = .5f;
        /// <summary>下から開始の上下移動アニメーション終了時間</summary>
        public float DownUpAnimationDuration => downUpAnimationDuration;
        /// <summary>下から開始の上下移動アニメーション移動距離</summary>
        [SerializeField] private float downUpAnimationDistance = 16f;
        /// <summary>下から開始の上下移動アニメーション移動距離</summary>
        public float DownUpAnimationDistance => downUpAnimationDistance;
        /// <summary>アニメーション補正値</summary>
        [SerializeField] private float animationOffsetUpDistance = 80f;
        /// <summary>アニメーション補正値</summary>
        public float AnimationOffsetUpDistance => animationOffsetUpDistance;
        /// <summary>拡大のアニメーション終了時間</summary>
        [SerializeField] private float scaleUpAnimationDuration = .3f;
        /// <summary>拡大のアニメーション終了時間</summary>
        public float ScaleUpAnimationDuration => scaleUpAnimationDuration;
        /// <summary>拡大のアニメーションスケール初期値</summary>
        [SerializeField] private Vector3 scaleUpAnimationFromScale = Vector3.one * 0.5f;
        /// <summary>拡大のアニメーションスケール初期値</summary>
        public Vector3 ScaleUpAnimationFromScale => scaleUpAnimationFromScale;
        /// <summary>目的位置に瞬間移動してフェード点滅アニメーション終了時間</summary>
        public float MoveAndFadeAnimationDuration => downUpAnimationDuration;
        /// <summary>管理者モード有効フラグ</summary>
        private ReactiveProperty<bool> _isActiveAdminMode;
        /// <summary>管理者モード有効フラグ</summary>
        public ReadOnlyReactiveProperty<bool> IsActiveAdminMode => _isActiveAdminMode;
        /// <summary>シーンロード演出の完了タイプ</summary>
        private ReactiveProperty<int> _CompletedSceneLoadDirectionType;
        /// <summary>シーンロード演出の完了タイプ</summary>
        public ReadOnlyReactiveProperty<int> CompletedSceneLoadDirectionType => _CompletedSceneLoadDirectionType;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag;

        public void Initialize()
        {
            _disposableBag = new DisposableBag();
            _CompletedSceneLoadDirectionType = new ReactiveProperty<int>();
            _isActiveAdminMode = new ReactiveProperty<bool>();
            var m = model;

            m.Initialize();

            model.IsActiveAdminMode.Subscribe(isActiveAdminMode =>
            {
                _isActiveAdminMode.Value = isActiveAdminMode;
            })
                .AddTo(ref _disposableBag);
            m.CompletedSceneLoadDirectionType.Subscribe(x =>
            {
                _CompletedSceneLoadDirectionType.Value = x;
            })
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            model.Dispose();
            _isActiveAdminMode = null;
            _CompletedSceneLoadDirectionType = null;
            _disposableBag.Dispose();
        }
    }
}
