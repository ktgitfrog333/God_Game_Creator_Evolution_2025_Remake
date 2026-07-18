using CriWare;
using R3;
using Titles.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Titles.Views
{
    /// <summary>
    /// マイク入力パネルのビュー
    /// </summary>
    public class InputMicPanelView : MonoBehaviour
    {
        /// <summary>マイクのデバイス名表示用テキスト</summary>
        [SerializeField] private Text micName;
        /// <summary>マイク入力パネルのビューモデル</summary>
        [SerializeField] private InputMicPanelViewModel viewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals("InputMicDeviceName"))
                {
                    if (micName == null)
                        micName = child.GetComponent<Text>();
                }
            }
        }

        private void Start()
        {
            viewModel.Initialize();

            Observable.EveryUpdate()
                .Where(_ => !CriAtomExMic.isInitialized)
                .Subscribe(_ =>
                {
                    CriAtomExMic.InitializeModule();
                })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Where(_ => CriAtomExMic.isInitialized)
                .Take(1)
                .Subscribe(_ =>
                {
                    var devices = CriAtomExMic.GetDevices();
                    if (0 < devices.Length)
                    {
                        RenderUseMicInfo(devices[0].deviceName);
                    }
                    else
                    {
                        RenderUseMicInfo("* No Mic");
                        viewModel.AddMessages("利用可能なマイクデバイスが見つかりません！");
                    }
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            viewModel.Dispose();
        }

        /// <summary>
        /// マイクのデバイス名を表示
        /// </summary>
        private void RenderUseMicInfo(string deviceName)
        {
            micName.text = deviceName;
        }
    }
}
