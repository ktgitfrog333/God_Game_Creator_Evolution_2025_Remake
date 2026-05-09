using Mains.Commons;
using Mains.Models;
using R3;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// リズムパートパネルのビューモデル
    /// </summary>
    public class RhythmPartPanelViewModel : IRhythmPartPanelModel, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveProperty<InteractionPart> InteractionPart
        {
            get
            {
                return _playerModel?.InteractionPartTable?.interactionPart ?? null;
            }
        }
        /// <summary>バッテリーのトランスフォーム</summary>
        public Transform BatteryTransform => _playerModel?.BatteryTransform ?? null;
        /// <summary>MissileDirectAnimManagerBのカスタマイズ構造体</summary>
        public MissileDirectAnimCustomizeStruct[] MissileDirectAnimCustomizeStructs => _playerModel?.MissileDirectAnimCustomizeStructs ?? new MissileDirectAnimCustomizeStruct[0];
        /// <summary>中ボスオバケ退治率</summary>
        private ReactiveCommand<float> _midBosskillsRateReactive = new ReactiveCommand<float>();
        /// <summary>中ボスオバケ退治率</summary>
        public ReactiveCommand<float> MidBosskillsRateReactive => _midBosskillsRateReactive;
        /// <summary>中ボスオバケ退治率</summary>
        public float MidBosskillsRate => _playerModel?.MidBosskillsRate ?? 0f;
        /// <summary>敵戦パート</summary>
        public EnemyBattlePart EnemyBattlePart => _playerModel?.EnemyBattlePart ?? EnemyBattlePart.Normal;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public RhythmPartPanelViewModel()
        {
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    _playerModel.MidBosskillsRateReactive.Subscribe(midBosskillsRate =>
                    {
                        _midBosskillsRateReactive.Execute(midBosskillsRate);
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void SetTargetCrossPosition(Vector3 targetCrossPosition)
        {
            if (_playerModel != null)
                _playerModel.SetTargetCrossPosition(targetCrossPosition);
        }

        public void SetIsSelectedBattery(bool isSelectedBattery)
        {
            if (_playerModel != null)
                _playerModel.SetIsSelectedBattery(isSelectedBattery);
        }

        public void SetSelectedMissGhostAttackTransform(Transform selectedMissGhostAttackTransform)
        {
            if (_playerModel != null)
                _playerModel.SetSelectedMissGhostAttackTransform(selectedMissGhostAttackTransform);
        }

        public void SetTargetCrossAnchoredPosition(Vector2 targetCrossAnchoredPosition)
        {
            if (_playerModel != null)
                _playerModel.SetTargetCrossAnchoredPosition(targetCrossAnchoredPosition);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
