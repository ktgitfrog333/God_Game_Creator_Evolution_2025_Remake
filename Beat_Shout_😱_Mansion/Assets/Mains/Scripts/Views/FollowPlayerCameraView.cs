using Unity.Cinemachine;
using UnityEngine;
using R3;

namespace Mains.Views
{
    /// <summary>
    /// プレイヤーを追尾するカメラのビュー
    /// </summary>
    public class FollowPlayerCameraView : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;
        /// <summary>ターゲット</summary>
        private Transform _target;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (cinemachineCamera == null)
                cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        private void Start()
        {
            Observable.EveryUpdate()
                .Select(q => FindAnyObjectByType<PlayerView>())
                .Where(q => q != null)
                .Take(1)
                .Subscribe(q =>
                {
                    _target = q.transform;
                    cinemachineCamera.Follow = _target;
                    cinemachineCamera.LookAt = _target;
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// CinemachineCameraのFollowとLookAtを削除
        /// </summary>
        public void DeleteFollowAndLookAt()
        {
            cinemachineCamera.Follow = null;
            cinemachineCamera.LookAt = null;
        }

        /// <summary>
        /// CinemachineCameraのFollowとLookAtを再びセット
        /// </summary>
        public void ResetFollowAndLookAt()
        {
            cinemachineCamera.Follow = _target;
            cinemachineCamera.LookAt = _target;
        }
    }
}
