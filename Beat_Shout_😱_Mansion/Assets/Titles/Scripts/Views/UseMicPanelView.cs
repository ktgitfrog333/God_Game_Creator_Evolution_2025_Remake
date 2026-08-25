using CriWare;
using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using Titles.ViewModels;
using TMPro;
using UnityEngine;

namespace Titles.Views
{
    /// <summary>
    /// 使用するマイクパネルのビュー
    /// </summary>
    public class UseMicPanelView : MonoBehaviour
    {
        /// <summary>使用するマイクパネルのビューモデル</summary>
        [SerializeField] private UseMicPanelViewModel viewModel;
        /// <summary>使用するマイクのドロップダウン</summary>
        [SerializeField] private TMP_Dropdown useMicDropdown;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals("UseMicDowndown"))
                {
                    if (useMicDropdown == null)
                        useMicDropdown = child.GetComponent<TMP_Dropdown>();
                }
            }
        }

        private void Start()
        {
            var vm = viewModel;

            vm.Initialize(this.GetCancellationTokenOnDestroy());
            vm.Configs.Subscribe(x =>
            {
                int optionsIndex = vm.OptionsIndex;
                RenderDropdown(x, optionsIndex);
            })
                .AddTo(ref _disposableBag);
            var dropdown = useMicDropdown;
            dropdown.onValueChanged.AsObservable()
                .Subscribe(index =>
                {
                    if (0 < index)
                    {
                        vm.SetIsActiveAdminMode(true);
                    }
                    else
                    {
                        vm.SetIsActiveAdminMode(false);
                    }
                    // indexに応じて内部でマイクのインデックスを保存
                    vm.SetCurrentDeviceId(index);
                })
                .AddTo(ref _disposableBag);
            // フェードアウト完了まで待機してインタラクトを有効にする
            var completedSceneLoadDirectionType = vm.CompletedSceneLoadDirectionType;
            switch (completedSceneLoadDirectionType.CurrentValue)
            {
                case 0:
                    completedSceneLoadDirectionType.Where(x => x == 1)
                        .Subscribe(_ =>
                        {
                            var dropdown = useMicDropdown;
                            dropdown.interactable = true;
                        })
                        .AddTo(ref _disposableBag);

                    break;
                case 1:
                    dropdown.interactable = true;

                    break;
            }
            // シーンのクローズ中はインタラクトを無効にする
            completedSceneLoadDirectionType.Where(x => x == 2 || x == 3)
                .Subscribe(_ =>
                {
                    var dropdown = useMicDropdown;
                    dropdown.interactable = false;
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            var vm = viewModel;
            vm.Dispose();
        }

        /// <summary>
        /// ドロップダウンを描画
        /// </summary>
        /// <param name="configs">マイクデバイス情報構造体</param>
        /// <param name="optionsIndex">ドロップダウン要素のインデックス初期値</param>
        private void RenderDropdown(List<CriAtomExMic.DeviceInfo> configs, int optionsIndex)
        {
            var dropdown = useMicDropdown;
            dropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>();
            options.Add(new TMP_Dropdown.OptionData("<color=#989898>使用するマイクを選択してください</color>"));
            foreach (var config in configs)
            {
                options.Add(new TMP_Dropdown.OptionData($"<color=#0000B4>{config.deviceName}</color>"));
            }
            options.Add(new TMP_Dropdown.OptionData("<color=#B40000>※マイクを使用しない</color>"));
            dropdown.AddOptions(options);
            dropdown.SetValueWithoutNotify(optionsIndex);
        }
    }
}
