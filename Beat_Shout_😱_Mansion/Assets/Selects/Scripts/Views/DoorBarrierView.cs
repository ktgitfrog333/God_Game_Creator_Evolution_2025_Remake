using DG.Tweening;
using R3;
using Selects.Commons;
using Selects.ViewModels;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Views
{
    /// <summary>
    /// ドアの通行禁止マテリアル用ビュー
    /// </summary>
    public class DoorBarrierView : MonoBehaviour
    {
        [SerializeField] private LevelStruct レベル構造体;
        /// <summary>ドアの通行禁止マテリアル用ビューモデル</summary>
        private DoorBarrierViewModel _viewModel;
        /// <summary>マテリアルのレンダラー</summary>
        [SerializeField] private Renderer doorRenderer;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            doorRenderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            var index = レベル構造体.階層;
            _viewModel = new DoorBarrierViewModel();
            Material material = doorRenderer.material;
            int FadeID = Shader.PropertyToID("_Fade");
            _viewModel.IsOnTriggerEnterSearchRangeIndex.Where(x => x == index)
                .Take(1)
                .Subscribe(_ =>
                {
                    if (index < 5)
                    {
                        var status = userBean.state[index];
                        if (0 < status)
                        {
                            material.DOFloat(0f, FadeID, .5f)
                                .From(1f);
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
