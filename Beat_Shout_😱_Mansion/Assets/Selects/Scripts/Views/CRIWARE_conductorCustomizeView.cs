using UnityEngine;
using R3;
using Mains.External;

namespace Selects.Views
{
    /// <summary>
    /// Time.timeScale を監視して CRI の再生状態を同期させるView
    /// </summary>
    public class CRIWARE_conductorCustomizeView : MonoBehaviour
    {
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private Script_xyloApi _scriptXyloApi;

        private void Start()
        {
            _scriptXyloApi = new Script_xyloApi();

            // フォーカス変化をR3で流す
            Observable.FromEvent<bool>(
                    h => Application.focusChanged += h,
                    h => Application.focusChanged -= h)
                .Subscribe(hasFocus =>
                {
                    // フォーカスを失ったら一時停止、戻ったら再開
                    _scriptXyloApi.SetBgmPause(!hasFocus);
                    Debug.Log($"[CRI Conductor] フォーカス変化 → hasFocus: {hasFocus}, Pause: {!hasFocus}");
                })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    _scriptXyloApi.SetAllNotesClickDetection(false);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _scriptXyloApi?.Dispose();
        }
    }
}
