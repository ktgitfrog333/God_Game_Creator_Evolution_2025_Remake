using R3;
using Selects.Commons;
using Selects.ViewModels;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Views
{
    /// <summary>
    /// 黒煙パーティクルビュー
    /// </summary>
    public class RisingSmokeParticleView : MonoBehaviour
    {
        [SerializeField] private LevelStruct レベル構造体;
        /// <summary>黒煙パーティクルビューモデル</summary>
        private RisingSmokeParticleViewModel _viewModel;
        /// <summary>パーティクルシステム</summary>
        [SerializeField] private ParticleSystem risingParticleSystem;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            risingParticleSystem = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            var index = レベル構造体.階層;
            _viewModel = new RisingSmokeParticleViewModel();
            _viewModel.IsOnTriggerEnterSearchRangeIndex.Where(x => x == index)
                .Take(1)
                .Subscribe(_ =>
                {
                    if (index < 5)
                    {
                        var status = userBean.state[index];
                        if (0 < status)
                        {
                            risingParticleSystem.Stop();
                        }
                    }
                })
                .AddTo(ref _disposableBag);
            var status = userBean.state[index];
            if (1 < status)
                // クリア状態なら非表示
                gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _viewModel?.Dispose();
            _disposableBag.Dispose();
        }
    }
}
