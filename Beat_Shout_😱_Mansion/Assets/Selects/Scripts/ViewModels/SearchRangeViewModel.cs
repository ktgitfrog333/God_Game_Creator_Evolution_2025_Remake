using Mains.Models;
using UnityEngine;
using R3;

namespace Selects.ViewModels
{
    /// <summary>
    /// 部屋の扉の前で調べる当たり判定ビューモデル
    /// </summary>
    public class SearchRangeViewModel : ICommonPanelModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;

        public SearchRangeViewModel()
        {
            System.IDisposable disposable = null;
            disposable = Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    // 1度のみ実行されれば良いので破棄しても問題なし
                    disposable.Dispose();
                });
        }

        public void SetSelectedStageIndex(int selectedStageIndex)
        {
            if (_playerModel != null)
                _playerModel.SetSelectedStageIndex(selectedStageIndex);
        }

        public void SetIsOnTriggerEnterSearchRangeIndex(int isOnTriggerEnterSearchRangeIndex)
        {
            if (_playerModel != null)
                _playerModel.SetIsOnTriggerEnterSearchRangeIndex(isOnTriggerEnterSearchRangeIndex);
        }
    }
}
