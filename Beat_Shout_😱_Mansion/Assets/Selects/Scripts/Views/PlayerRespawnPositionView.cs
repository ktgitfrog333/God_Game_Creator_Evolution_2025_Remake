using R3;
using Selects.Commons;
using Selects.ViewModels;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Views
{
    /// <summary>
    /// リスポーン地点ビュー
    /// </summary>
    public class PlayerRespawnPositionView : MonoBehaviour, IDidStartProvider
    {
        [SerializeField] private LevelStruct レベル構造体;
        /// <summary>リスポーン地点ビューモデル</summary>
        private PlayerRespawnPositionViewModel _viewModel;
        /// <summary>Start完了を通知するObservable（Trueになったら1度だけ発火）</summary>
        private Subject<Unit> _didStartAsObservable = new Subject<Unit>();
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            var index = userBean.sceneIdx;
            if (5 <= index)
            {
                _didStartAsObservable.OnNext(Unit.Default);
                _didStartAsObservable.OnCompleted();
                // タイトルシーンからの遷移の場合は開始位置はデフォルトのまま
                return;
            }
            if (index == レベル構造体.階層)
            {
                _viewModel = new PlayerRespawnPositionViewModel();
                var trans = transform;
                _viewModel.SetStartPointTrans(trans);
            }
            _didStartAsObservable.OnNext(Unit.Default);
            _didStartAsObservable.OnCompleted();
        }

        private void OnDestroy()
        {
            _viewModel?.Dispose();
            _disposableBag.Dispose();
        }

        public Observable<Unit> DidStartAsObservable()
        {
            return Observable.Create<Unit>(observer =>
            {
                _didStartAsObservable.Take(1)
                    .Subscribe(_ =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                    .AddTo(ref _disposableBag);

                return Disposable.Empty;
            });
        }
    }
}
