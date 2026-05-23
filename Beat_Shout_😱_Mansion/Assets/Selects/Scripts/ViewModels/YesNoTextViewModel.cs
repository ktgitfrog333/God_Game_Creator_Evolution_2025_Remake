using Mains.Models;
using R3;
using Selects.Commons;
using UnityEngine;

namespace Selects.ViewModels
{
    /// <summary>
    /// YesかNoのボタン制御ビューモデル
    /// </summary>
    public class YesNoTextViewModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;

        public YesNoTextViewModel()
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

        /// <summary>
        /// 実行イベントの状態をセット
        /// </summary>
        /// <param name="eventState">実行イベントの状態</param>
        public void SetEventState(EnumEventCommand eventState)
        {
            if (_playerModel != null)
                _playerModel.SetEventState(eventState);
        }
    }
}
