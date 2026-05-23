using UnityEngine;
//using Mains.Commons;
using Mains.Models;
using R3;

namespace Selects.ViewModels
{
    /// <summary>
    /// 共通UIのビューモデル
    /// </summary>

    public class CommonPanelViewModel : ICommonPanelModel, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>選択されたステージ番号</summary>
        public ReactiveCommand<int> SelectedStageIndex => _playerModel?.SelectedStageIndex ?? null;
        /// <summary>部屋の扉の前で調べる当たり判定に触れた階層</summary>
        private ReactiveCommand<int> _isOnTriggerEnterSearchRangeIndex = new ReactiveCommand<int>();
        /// <summary>部屋の扉の前で調べる当たり判定に触れた階層</summary>
        public ReactiveCommand<int> IsOnTriggerEnterSearchRangeIndex => _isOnTriggerEnterSearchRangeIndex;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

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
                })
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
                .Select(_ => _playerModel)
                .Where(x => x != null)
                .Select(x => x.IsOnTriggerEnterSearchRangeIndex)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(isOnTriggerEnterSearchRangeIndex =>
                {
                    isOnTriggerEnterSearchRangeIndex.Subscribe(x =>
                    {
                        _isOnTriggerEnterSearchRangeIndex.Execute(x);
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void SetSelectedStageIndex(int selectedStageIndex)
        {
            if (_playerModel != null)
                _playerModel.SetSelectedStageIndex(selectedStageIndex);
        }

        public void SetIsOnTriggerEnterSearchRangeIndex(int isOnTriggerEnterSearchRangeIndex)
        {
            throw new System.NotImplementedException();
        }

        public void SetCommonHeaderPanelRectTrans(RectTransform commonHeaderPanelRectTrans)
        {
            if (_playerModel != null)
                _playerModel.SetCommonHeaderPanelRectTrans(commonHeaderPanelRectTrans);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
