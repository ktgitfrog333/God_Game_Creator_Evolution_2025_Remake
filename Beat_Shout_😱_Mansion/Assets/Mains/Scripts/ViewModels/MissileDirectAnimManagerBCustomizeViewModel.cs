using Mains.Models;
using UnityEngine;
using R3;
using Mains.Commons;

namespace Mains.ViewModels
{
    /// <summary>
    /// MissileDirectAnimManagerBのカスタマイズビューモデル
    /// </summary>
    public class MissileDirectAnimManagerBCustomizeViewModel : IMissileDirectAnimManagerBCustomizeModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>ターゲットクロスアンカー位置</summary>
        public Vector2 TargetCrossAnchoredPosition => _playerModel?.TargetCrossAnchoredPosition ?? Vector2.zero;

        public MissileDirectAnimManagerBCustomizeViewModel()
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

        public void AddMissileDirectAnimCustomizeStructs(MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct)
        {
        	if (_playerModel != null)
        	{
        		_playerModel.AddMissileDirectAnimCustomizeStructs(missileDirectAnimCustomizeStruct);
        	}
        }

        public void AddOrSetOnEnabledTime(MissileDirectAnimCustomizeStruct missileDirectAnimCustomizeStruct)
        {
            if (_playerModel != null)
            {
                _playerModel.AddOrSetOnEnabledTime(missileDirectAnimCustomizeStruct);
            }
        }

        public bool IsFrontMissileDirectAnim(Transform transform)
        {
            if (_playerModel != null)
            {
                return _playerModel.IsFrontMissileDirectAnim(transform);
            }

            return false;
        }
    }
}
