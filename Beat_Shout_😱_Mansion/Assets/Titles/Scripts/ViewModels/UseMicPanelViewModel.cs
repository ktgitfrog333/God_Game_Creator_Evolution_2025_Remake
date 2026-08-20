using CriWare;
using Cysharp.Threading.Tasks;
using Mains.External;
using R3;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Titles.Models;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// 使用するマイクパネルのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "UseMicPanelViewModel", menuName = "Scriptable Objects/UseMicPanelViewModel")]
    public class UseMicPanelViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] private LauncherModel model;
        /// <summary>「※マイクを使用しない」場合のID</summary>
        [SerializeField] private string doNotUseDeviceId = "*Do not use the microphone";
        /// <summary>マイクデバイス情報構造体</summary>
        private ReactiveProperty<List<CriAtomExMic.DeviceInfo>> _configs;
        /// <summary>マイクデバイス情報構造体</summary>
        public ReadOnlyReactiveProperty<List<CriAtomExMic.DeviceInfo>> Configs => _configs;
        /// <summary>オーディオ設定データ</summary>
        private Dictionary<int, string> _audioSettingsDataDic;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_xyloApi;
        /// <summary>ドロップダウン要素のインデックス初期値</summary>
        public int OptionsIndex { get; private set; }
        /// <summary>シーンロード演出の完了タイプ</summary>
        private ReactiveProperty<int> _CompletedSceneLoadDirectionType;
        /// <summary>シーンロード演出の完了タイプ</summary>
        public ReadOnlyReactiveProperty<int> CompletedSceneLoadDirectionType => _CompletedSceneLoadDirectionType;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag;

        public void Initialize(CancellationToken token)
        {
            _disposableBag = new DisposableBag();
            _configs = new ReactiveProperty<List<CriAtomExMic.DeviceInfo>>();
            _configs.Value = new List<CriAtomExMic.DeviceInfo>();
            _script_xyloApi = new Script_xyloApi();
            _CompletedSceneLoadDirectionType = new ReactiveProperty<int>();

            model.Initialize();
            var api = _script_xyloApi;
            _audioSettingsDataDic = api.GetAudioSettingsDataDic();
            DoObserverConfigsAsync(token)
                .Forget();
            var m = model;
            m.CompletedSceneLoadDirectionType.Subscribe(x =>
            {
                _CompletedSceneLoadDirectionType.Value = x;
            })
                .AddTo(ref _disposableBag);
        }

        /// <summary>
        /// マイクデバイス更新のための監視を開始
        /// </summary>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask DoObserverConfigsAsync(CancellationToken token)
        {
            if (!CriAtomExMic.isInitialized)
            {
                CriAtomExMic.InitializeModule();

                await Observable.EveryUpdate()
                    .Where(_ => CriAtomExMic.isInitialized)
                    .FirstAsync(token);
            }
            var m = model;
            Observable.EveryUpdate()
                .Select(_ => CriAtomExMic.GetDevices())
                .Pairwise()
                .Where(x => !isEqualsDevicesPair(x.Previous, x.Current))
                .Select(x => x.Current)
                .Subscribe(devices =>
                {
                    // デバイス情報が更新された場合は内部保存しているデバイス情報もリセットする
                    var api = _script_xyloApi;
                    var audioSettingsDataDic = _audioSettingsDataDic;
                    audioSettingsDataDic[3] = string.Empty;

                    api.SetAudioSettingsData(audioSettingsDataDic);
                    api.SaveSettings();
                    m.SetIsActiveAdminMode(false);
                    OptionsIndex = 0;
                    _configs.Value = devices.ToList();
                })
                .AddTo(ref _disposableBag);
            var devices = CriAtomExMic.GetDevices().ToList();
            var currentDeviceId = _audioSettingsDataDic[3];
            // 「※マイクを使用しない」場合は要素数最大+1する
            var optionsIndex = !string.IsNullOrEmpty(currentDeviceId) && currentDeviceId.Equals(doNotUseDeviceId) ? devices.Count + 1 : 0;
            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                if (!string.IsNullOrEmpty(currentDeviceId) && device.deviceId.Equals(currentDeviceId))
                {
                    // ドロップダウンリスト要素[0]は「使用するマイクを選択してください」が入る想定のため+1する
                    optionsIndex = i + 1;
                }
            }
            // 既に選択済みなら有効にする
            if (0 < optionsIndex)
            {
                m.SetIsActiveAdminMode(true);
            }
            else
            {
                m.SetIsActiveAdminMode(false);
            }
            OptionsIndex = optionsIndex;
            _configs.Value = devices;
        }

        /// <summary>
        /// マイクデバイス情報構造体リストをデバイスID単位で比較して一致しているか
        /// </summary>
        /// <param name="prevDevices">前のマイクデバイス情報構造体リスト</param>
        /// <param name="currentDevices">現在のマイクデバイス情報構造体リスト</param>
        /// <returns>デバイスID単位で比較して一致しているか</returns>
        private bool isEqualsDevicesPair(CriAtomExMic.DeviceInfo[] prevDevices, CriAtomExMic.DeviceInfo[] currentDevices)
        {
            if (prevDevices.Length != currentDevices.Length)
                return false;

            for (int i = 0; i < prevDevices.Length; i++)
            {
                if (prevDevices[i].deviceId != currentDevices[i].deviceId)
                    return false;
            }

            return true;
        }

        public void SetIsActiveAdminMode(bool isActiveAdminMode)
        {
            model.SetIsActiveAdminMode(isActiveAdminMode);
        }

        public void SetCurrentDeviceId(int index)
        {
            var audioSettingsDataDic = _audioSettingsDataDic;
            var configs = _configs.Value;
            var api = _script_xyloApi;

            // audioSettingsDataDicにインデックスからデバイスIDを取得してセット
            // ドロップダウンは要素[0]と要素[要素数最大-1]にはダミー値が入っている
            int configsIndex = index - 1;
            string currentDeviceId = string.Empty;
            if (-1 < configsIndex && configsIndex < configs.Count)
            {
                currentDeviceId = configs[configsIndex].deviceId;
            }
            else if (-1 < configsIndex)
            {
                currentDeviceId = doNotUseDeviceId;
            }
            audioSettingsDataDic[3] = currentDeviceId;

            api.SetAudioSettingsData(audioSettingsDataDic);
            api.SaveSettings();
        }

        public void Dispose()
        {
            model.Dispose();
            _CompletedSceneLoadDirectionType = null;
            _disposableBag.Dispose();
            _configs = null;
            _audioSettingsDataDic = null;
            _script_xyloApi.Dispose();
            _script_xyloApi = null;
            OptionsIndex = 0;
        }
    }
}
