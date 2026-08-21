using Mains.Commons;
using R3;
using Selects.ViewModels;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// 複数のライトを切り替えるビュー
    /// </summary>
    public class AnyRightsSwicherView : MonoBehaviour
    {
        /// <summary>複数のライトを切り替えるビューモデル</summary>
        [SerializeField] private AnyRightsSwicherViewModel viewModel;
        /// <summary>複数のライトを切り替える設定</summary>
        [SerializeField] private AnyRightsSwicherSettings settings;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            var vm = viewModel;
            vm.Initialize();

            vm.EnemyBattlePart.DistinctUntilChanged()
                .Subscribe(x =>
                {
                    switch (x)
                    {
                        case EnemyBattlePart.Tutorial:
                            SwitchNight();

                            break;

                        default:
                            SwitchDay();

                            break;
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
        /// 夜へ切り替える
        /// </summary>
        private void SwitchNight()
        {
            var set = settings;
            var dayLevels = set.dayLevels;
            SetEnableObjects(false, dayLevels.targetGameObjects);

            var nightLevels = set.nightLevels;
            SetEnableObjects(true, nightLevels.targetGameObjects);
        }

        /// <summary>
        /// 昼へ切り替える
        /// </summary>
        private void SwitchDay()
        {
            var set = settings;
            var nightLevels = set.nightLevels;
            SetEnableObjects(false, nightLevels.targetGameObjects);

            var dayLevels = set.dayLevels;
            SetEnableObjects(true, dayLevels.targetGameObjects);
        }

        /// <summary>
        /// 対象のゲームオブジェクトリストへ有効／無効をセット
        /// </summary>
        /// <param name="isEnabled">有効／無効</param>
        /// <param name="targetGameObjects">対象のゲームオブジェクトリスト</param>
        private void SetEnableObjects(bool isEnabled, GameObject[] targetGameObjects)
        {
            foreach (var targetGameObject in targetGameObjects)
            {
                if (targetGameObject.activeSelf != isEnabled)
                    targetGameObject.SetActive(isEnabled);
            }
        }
    }

    /// <summary>
    /// 複数のライトを切り替える設定
    /// </summary>
    [System.Serializable]
    public class AnyRightsSwicherSettings
    {
        /// <summary>夜モードのレベル</summary>
        public NightLevels nightLevels;
        /// <summary>昼モードのレベル</summary>
        public DayLevels dayLevels;

        /// <summary>
        /// 夜モードのレベル
        /// </summary>
        [System.Serializable]
        public class NightLevels
        {
            /// <summary>対象のゲームオブジェクトリスト</summary>
            public GameObject[] targetGameObjects;
        }

        /// <summary>
        /// 昼モードのレベル
        /// </summary>
        [System.Serializable]
        public class DayLevels
        {
            /// <summary>対象のゲームオブジェクトリスト</summary>
            public GameObject[] targetGameObjects;
        }
    }
}
