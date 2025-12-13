using R3;
using R3.Triggers;
using Rewired;
using Selects.Commons;
using Selects.ViewModels;
using System.Threading.Tasks;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Views
{
    /// <summary>
    /// 部屋の扉の前で調べる当たり判定ビュー
    /// </summary>
    public class SearchRangeView : MonoBehaviour
    {
        [SerializeField] private LevelStruct レベル構造体;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            var index = レベル構造体.階層;
            if (index < 5)
            {
                var status = userBean.state[index];
                if (status == 0)
                    // 未開放なら調べるは不可
                    return;
            }

            SearchRangeViewModel viewModel = new SearchRangeViewModel();
            bool isOnTriggerEnter = false;
            var player = ReInput.players.GetPlayer(0);
            bool isLock = false;
            Observable.EveryUpdate()
                .Select(_ => Time.timeScale)
                .Pairwise()
                .Where(x => x.Previous < x.Current)
                .Subscribe(async _ =>
                {
                    isLock = true;
                    await Task.Delay(100);
                    isLock = false;
                })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Select(_ => isOnTriggerEnter)
                .Where(x => x &&
                    Time.timeScale != 0f)
                .Subscribe(_ =>
                {
                    if (IsInputSearch(player, isLock))
                    {
                        viewModel.SetSelectedStageIndex(index);
                    }
                })
                .AddTo(ref _disposableBag);
            this.OnTriggerStayAsObservable()
                .Subscribe(_ =>
                {
                    if (!isOnTriggerEnter)
                    {
                        isOnTriggerEnter = true;
                        viewModel.SetIsOnTriggerEnterSearchRangeIndex(index);
                    }
                })
                .AddTo(ref _disposableBag);
            this.OnTriggerExitAsObservable()
                .Subscribe(_ =>
                {
                    if (isOnTriggerEnter)
                    {
                        isOnTriggerEnter = false;
                        viewModel.SetIsOnTriggerEnterSearchRangeIndex(-1);
                    }
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// 調べる入力があったか
        /// </summary>
        /// <param name="player">Rewiredのplayer</param>
        /// <param name="isLock">操作ロック</param>
        /// <returns>調べる入力があったか</returns>
        private bool IsInputSearch(Player player, bool isLock)
        {
            if (isLock)
                // ロック中は常にfalse
                return false;

            return player.GetButtonDown("Search");
        }
    }
}
