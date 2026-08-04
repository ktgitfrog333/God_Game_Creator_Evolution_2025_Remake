using Mains.Commons;
using Mains.Models;
using R3;
using Rewired;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// ハートが減少する演出ビューモデル
    /// </summary>
    public class HPDownDirectionViewModel : System.IDisposable, IHPDownDirectionModel
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>リズムパート完了フラグ</summary>
        private ReactiveCommand<int> _isCompletedRhythmPart = new ReactiveCommand<int>();
        /// <summary>リズムパート完了フラグ</summary>
        public ReactiveCommand<int> IsCompletedRhythmPart => _isCompletedRhythmPart;
        /// <summary>Rewiredのプレイヤー</summary>
        private Player _player;
        /// <summary>オバケ攻撃のヒットフラグ</summary>
        private ReactiveCommand<bool> _isHitGhostAttack = new ReactiveCommand<bool>();
        /// <summary>オバケ攻撃のヒットフラグ</summary>
        public ReactiveCommand<bool> IsHitGhostAttack => _isHitGhostAttack;
        /// <summary>敵戦パート</summary>
        private EnemyBattlePart _enemyBattlePart;
        /// <summary>敵戦パート</summary>
        public EnemyBattlePart EnemyBattlePart => _enemyBattlePart;
        /// <summary>電池落下タイプ</summary>
        private readonly ReactiveProperty<BatteryDropType> _batteryDropType = new ReactiveProperty<BatteryDropType>();
        /// <summary>電池落下タイプ</summary>
        public ReadOnlyReactiveProperty<BatteryDropType> BatteryDropType => _batteryDropType;
        /// <summary>MissGhostAttackヒット通知</summary>
        private readonly ReactiveCommand<Unit> _isHitMissGhostAttackTutorial = new ReactiveCommand<Unit>();
        /// <summary>MissGhostAttackヒット通知</summary>
        public ReactiveCommand<Unit> IsHitMissGhostAttackTutorial => _isHitMissGhostAttackTutorial;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public HPDownDirectionViewModel(Player player)
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    _playerModel.IsCompletedRhythmPart.Subscribe(isCompleted =>
                    {
                        _isCompletedRhythmPart.Execute(isCompleted);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.IsHitGhostAttack.Subscribe(isHitGhostAttack =>
                    {
                        _isHitGhostAttack.Execute(isHitGhostAttack);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.EnemyBattlePartReactive.Subscribe(x =>
                    {
                        _enemyBattlePart = x;
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.IsHitMissGhostAttackTutorial.Subscribe(x =>
                    {
                        _isHitMissGhostAttackTutorial.Execute(Unit.Default);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.BatteryDropType.Subscribe(x =>
                    {
                        _batteryDropType.Value = x;
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            _player = player;
        }

        public void SetIsCompletedDirection(bool isCompleted)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedDirection(isCompleted);
        }

        /// <summary>
        /// プレイヤーコントローラー操作の有効かどうかをセット
        /// </summary>
        /// <param name="isEnabled">有効かどうか</param>
        public void SetPlayerControllerEnabled(bool isEnabled)
        {
            var player = _player;
            player.controllers.maps.SetMapsEnabled(isEnabled, "Default");
        }

        public void SubtractionHealthPoint()
        {
            if (_playerModel != null)
                _playerModel.SubtractionHealthPoint();
        }

        public void SetOnHpDecreasedTutorial()
        {
            if (_playerModel != null)
                _playerModel.SetOnHpDecreasedTutorial();
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
