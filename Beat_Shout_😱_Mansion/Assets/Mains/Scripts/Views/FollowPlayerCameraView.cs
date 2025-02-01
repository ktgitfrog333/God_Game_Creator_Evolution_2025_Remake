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
                    var target = q.transform;
                    cinemachineCamera.Follow = target;
                    cinemachineCamera.LookAt = target;
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
