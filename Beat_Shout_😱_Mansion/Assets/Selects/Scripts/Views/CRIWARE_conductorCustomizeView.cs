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

            // Time.timeScale を監視して再生状態を同期
            Observable.EveryUpdate()
                .Select(_ => Time.timeScale)
                .DistinctUntilChanged()
                .Subscribe(timeScale =>
                {
                    bool isPaused = timeScale == 0f;

                    // timeScale == 0 → 音楽を一時停止
                    // timeScale != 0 → 音楽を再開
                    _scriptXyloApi.SetBgmPause(isPaused);

                    Debug.Log($"[CRI Conductor] TimeScale変化 → timeScale: {timeScale}, Pause: {isPaused}, NotesClick: {!isPaused}");
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _scriptXyloApi?.Dispose();
        }

#if UNITY_EDITOR
        //private void OnGUI()
        //{
        //    // エディタ専用：Time.timeScale 切り替えボタン
        //    float buttonWidth = 200f;
        //    float buttonHeight = 40f;
        //    float margin = 10f;
        //    float x = Screen.width - buttonWidth - margin;
        //    float y = margin;

        //    string label = Time.timeScale == 0f
        //        ? "▶ TimeScale = 1 に変更"
        //        : "⏸ TimeScale = 0 に変更";

        //    if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), label))
        //    {
        //        Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
        //        Debug.Log($"[CRI Conductor] OnGUI ボタン押下 → TimeScale: {Time.timeScale}");
        //    }

        //    // 現在の TimeScale 表示
        //    GUI.Label(new Rect(x, y + buttonHeight + 5f, buttonWidth, 20f),
        //        $"現在の TimeScale: {Time.timeScale}");
        //}
#endif
    }
}
