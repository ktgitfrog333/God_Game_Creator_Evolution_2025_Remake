using UnityEngine;
//using Mains.Commons;
using Mains.Models;
using ObservableCollections;
using R3;

namespace Selects.ViewModels
{
    /// <summary>
    /// 共通UIのビューモデル
    /// </summary>

    public class CommonPanelViewModel : ICommonPanelModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>選択されたステージ番号</summary>
        public ReactiveCommand<int> SelectedStageIndex => _playerModel?.SelectedStageIndex ?? null;

        public CommonPanelViewModel()
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
    }
}
