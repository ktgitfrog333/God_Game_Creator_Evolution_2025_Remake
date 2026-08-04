using Mains.Models;
using R3;
using UnityEngine;

namespace Selects.ViewModels
{
    /// <summary>
    /// ステージ開始演出のビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "StartDirectionViewModel", menuName = "Scriptable Objects/StartDirectionViewModel")]
    public class StartDirectionViewModel : ScriptableObject, System.IDisposable, IStartDirectionModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>ステージ開始演出が完了したか</summary>
        private readonly ReactiveProperty<bool> _isCompletedStartDirection = new ReactiveProperty<bool>(false);
        /// <summary>ステージ開始演出が完了したか</summary>
        public ReadOnlyReactiveProperty<bool> IsCompletedStartDirection => _isCompletedStartDirection;
        /// <summary>一度のみ初期化フラグ</summary>
        private bool _isOnly;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public void Initialize()
        {
            if (!_isOnly)
            {
                _isOnly = true;
                Observable.EveryUpdate()
                    .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                    .Where(x => x != null)
                    .Take(1)
                    .Subscribe(x =>
                    {
                        _playerModel = x;
                        var model = _playerModel;
                        model.IsCompletedStartDirection.Subscribe(isCompletedStartDirection =>
                        {
                            _isCompletedStartDirection.Value = isCompletedStartDirection;
                        })
                            .AddTo(ref _disposableBag);
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        public void SetIsCompletedStartDirection(bool isCompleted)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedStartDirection(isCompleted);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
            _isOnly = false;
        }
    }
}
