using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// オバケ弾：抽象クラスのビューモデル
    /// </summary>
    public abstract class GhostBulletAbstractViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        protected PlayerModel _playerModel;
        /// <summary>R3のリソース管理</summary>
        protected DisposableBag _disposableBag = new DisposableBag();

        public void DoInitialize()
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    Initialize(_playerModel);
                })
                .AddTo(ref _disposableBag);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="playerModel">プレイヤーのモデル</param>
        protected virtual void Initialize(PlayerModel playerModel)
        {

        }

        public void Dispose()
        {
            _disposableBag.Dispose();
            DisposeAdd();
        }

        /// <summary>
        /// 破棄処理の追加分
        /// </summary>
        protected virtual void DisposeAdd()
        {

        }
    }

    /// <summary>
    /// オバケ弾：抽象クラスのビューモデル設定
    /// </summary>
    [System.Serializable]
    public class GhostBulletAbstractVMSettings
    {
        /// <summary>開始時に停止</summary>
        public bool isStartStop;
    }
}
