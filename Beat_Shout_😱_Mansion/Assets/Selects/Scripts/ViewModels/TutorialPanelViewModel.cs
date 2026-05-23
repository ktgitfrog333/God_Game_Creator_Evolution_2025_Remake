using Mains.Commons;
using Mains.Models;
using R3;
using Selects.Commons;
using UnityEngine;

namespace Selects.ViewModels
{
    /// <summary>
    /// チュートリアルパネルのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialPanelViewModel", menuName = "Scriptable Objects/TutorialPanelViewModel")]
    public class TutorialPanelViewModel : ScriptableObject, System.IDisposable, ITutorialPanelModel
    {
        /// <summary>チュートリアルパネルの設定</summary>
        [SerializeField] private TutorialPanelSettings settings;
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>プレイヤーのトランスフォーム</summary>
        public Transform PlayerTransform => _playerModel?.PlayerPropertiesStruct.transform ?? null;
        /// <summary>プレイヤーの頭</summary>
        public Transform PlayerHead => _playerModel?.PlayerHead ?? null;
        /// <summary>プレイヤーの懐中電灯</summary>
        public Transform PlayerFlashLight => _playerModel?.PlayerFlashLight ?? null;
        /// <summary>デシベルレベル</summary>
        private ReactiveCommand<float> _dbLevelReactive = new ReactiveCommand<float>();
        /// <summary>デシベルレベル</summary>
        public ReactiveCommand<float> DbLevelReactive => _dbLevelReactive;
        /// <summary>デシベルレベル</summary>
        private float _dbLevel;
        /// <summary>デシベルレベル</summary>
        public float DbLevel => _dbLevel;
        /// <summary>家具とプレイヤーがお互い向き合っている状態フラグ</summary>
        private ReactiveCommand<bool> _isPostRhythmFaceOff = new ReactiveCommand<bool>();
        /// <summary>家具とプレイヤーがお互い向き合っている状態フラグ</summary>
        public ReactiveCommand<bool> IsPostRhythmFaceOff => _isPostRhythmFaceOff;
        /// <summary>共通UIのヘッダパネルのトランスフォーム</summary>
        public RectTransform CommonHeaderPanelRectTrans => _playerModel?.CommonHeaderPanelRectTrans ?? null;
        /// <summary>選択されたステージ番号</summary>
        public ReactiveCommand<int> SelectedStageIndex => _playerModel?.SelectedStageIndex ?? null;
        /// <summary>実行イベントの監視</summary>
        private ReactiveCommand<EnumEventCommand> _eventStateReactive = new ReactiveCommand<EnumEventCommand>();
        /// <summary>実行イベントの監視</summary>
        public ReactiveCommand<EnumEventCommand> EventStateReactive => _eventStateReactive;
        /// <summary>共通パネルのフッターパネル</summary>
        public RectTransform CommonFooterPanelRectTrans => throw new System.NotImplementedException();
        /// <summary>懐中電灯トリガー接触状態</summary>
        private ReactiveProperty<bool> _flashLightTriggerStay = new ReactiveProperty<bool>(false);
        /// <summary>懐中電灯トリガー接触状態</summary>
        public ReadOnlyReactiveProperty<bool> FlashLightTriggerStay => _flashLightTriggerStay;
        /// <summary>プレイヤーの視線が電池を捉える</summary>
        private ReactiveProperty<bool> _batteryHitPlayerAim = new ReactiveProperty<bool>(false);
        /// <summary>プレイヤーの視線が電池を捉える</summary>
        public ReadOnlyReactiveProperty<bool> BatteryHitPlayerAim => _batteryHitPlayerAim;
        /// <summary>電池トリガー接触状態</summary>
        private ReactiveProperty<bool> _batteryTriggerStay = new ReactiveProperty<bool>(false);
        /// <summary>電池トリガー接触状態</summary>
        public ReadOnlyReactiveProperty<bool> BatteryTriggerStay => _batteryTriggerStay;
        /// <summary>プレイヤーの視線が移動オバケ（ノーマル）を捉える</summary>
        private ReactiveProperty<bool> _missGhostEscapeNormalHitPlayerAim = new ReactiveProperty<bool>(false);
        /// <summary>プレイヤーの視線が移動オバケ（ノーマル）を捉える</summary>
        public ReadOnlyReactiveProperty<bool> MissGhostEscapeNormalHitPlayerAim => _missGhostEscapeNormalHitPlayerAim;
        /// <summary>光のリング2トリガー接触状態</summary>
        private ReactiveProperty<bool> _lightRing2TriggerStay = new ReactiveProperty<bool>(false);
        /// <summary>光のリング2トリガー接触状態</summary>
        public ReadOnlyReactiveProperty<bool> LightRing2TriggerStay => _lightRing2TriggerStay;
        /// <summary>プレイヤーの視線が花瓶と机を捉える</summary>
        private ReactiveProperty<bool> _vaseAndDeskGroupHitPlayerAim = new ReactiveProperty<bool>(false);
        /// <summary>プレイヤーの視線が花瓶と机を捉える</summary>
        public ReadOnlyReactiveProperty<bool> VaseAndDeskGroupHitPlayerAim => _vaseAndDeskGroupHitPlayerAim;
        /// <summary>1階の右階段トリガー接触状態</summary>
        private ReactiveProperty<bool> _rightStairsTrigger1FStay = new ReactiveProperty<bool>(false);
        /// <summary>1階の右階段トリガー接触状態</summary>
        public ReadOnlyReactiveProperty<bool> RightStairsTrigger1FStay => _rightStairsTrigger1FStay;
        /// <summary>2階の左階段トリガー接触状態</summary>
        private ReactiveProperty<bool> _leftStairsTrigger2FStay = new ReactiveProperty<bool>(false);
        /// <summary>2階の左階段トリガー接触状態</summary>
        public ReadOnlyReactiveProperty<bool> LeftStairsTrigger2FStay => _leftStairsTrigger2FStay;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        /// <summary>
        /// 初期処理
        /// </summary>
        public void Initialize()
        {
            var set = settings;
            Observable.EveryUpdate()
                .Select(_ => GameObject.FindAnyObjectByType<PlayerModel>())
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    _playerModel = x;
                    _playerModel.InteractionPartTable.dbLevel.Subscribe(x =>
                    {
                        _dbLevelReactive.Execute(x);
                        _dbLevel = x;
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.IsPostRhythmFaceOff.Subscribe(x =>
                    {
                        _isPostRhythmFaceOff.Execute(x);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.EventStateReactive.Subscribe(x =>
                    {
                        _eventStateReactive.Execute(x);
                    })
                        .AddTo(ref _disposableBag);
                    _playerModel.SetHealthPointMax(set.開始時のプレイヤーの最大体力);
                })
                .AddTo(ref _disposableBag);
        }

        public void SetEnemyBattlePart(EnemyBattlePart enemyBattlePart)
        {
            if (_playerModel != null)
                _playerModel.SetEnemyBattlePart(enemyBattlePart);
        }

        public void SetIsStartAttack(Transform isStartAttack)
        {
            if (_playerModel != null)
            {
                // PoltergeistViewModelのIsStartAttackをExecuteするため、
                // PlayerModel経由でポルターガイストの攻撃開始を通知する
                // 注意: チュートリアル用の簡易実装
            }
        }

        public void SetFlashLightTriggerStay(bool flashLightTriggerStay)
        {
            _flashLightTriggerStay.Value = flashLightTriggerStay;
        }

        public void SetBatteryHitPlayerAim(bool batteryHitPlayerAim)
        {
            _batteryHitPlayerAim.Value = batteryHitPlayerAim;
        }

        public void SetBatteryTriggerStay(bool  batteryTriggerStay)
        {
            _batteryTriggerStay.Value = batteryTriggerStay;
        }

        public void SetMissGhostEscapeNormalHitPlayerAim(bool missionGhostEscapeNormalHitPlayerAim)
        {
            _missGhostEscapeNormalHitPlayerAim.Value = missionGhostEscapeNormalHitPlayerAim;
        }

        public void SetLightRing2TriggerStay(bool lightRing2TriggerStay)
        {
            _lightRing2TriggerStay.Value = lightRing2TriggerStay;
        }

        public void SetVaseAndDeskGroupHitPlayerAim(bool vaseAndDeskGroupHitPlayerAim)
        {
            _vaseAndDeskGroupHitPlayerAim.Value = vaseAndDeskGroupHitPlayerAim;
        }

        public void SetRightStairsTrigger1FStay(bool rightStairsTrigger1FStay)
        {
            _rightStairsTrigger1FStay.Value = rightStairsTrigger1FStay;
        }

        public void SetLeftStairsTrigger2FStay(bool leftStairsTrigger2FStay)
        {
            _leftStairsTrigger2FStay.Value= leftStairsTrigger2FStay;
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }


    /// <summary>
    /// チュートリアルパネルの設定
    /// </summary>
    [System.Serializable]
    public class TutorialPanelSettings
    {
        public int 開始時のプレイヤーの最大体力;
    }
}
