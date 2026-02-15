using UnityEngine;
using Mains.Commons;
using Mains.Models;
using ObservableCollections;
using R3;

namespace Mains.ViewModels
{
    /// <summary>
    /// プレイヤーのビューモデル
    /// </summary>
    public class PlayerViewModel : IPlayerModel, System.IDisposable
    {
        /// <summary>プレイヤーのモデル</summary>
        private PlayerModel _playerModel;
        /// <summary>SphereCast用のRaycastHit配列（再利用）</summary>
        private RaycastHit[] _sphereCastHits = new RaycastHit[1];
        /// <summary>オバケの家具入居管理の構造体リスト</summary>
        public ObservableList<GhostInStaticObjectStruct> GhostInStaticObjectStructs => _playerModel?.GhostInStaticObjectStructs ?? null;
        /// <summary>ポルターガイストの発生位置</summary>
        public ReactiveProperty<Vector3> OnActionPoltergeistPosition
        {
            get
            {
                return _playerModel?.PoltergeistTable?.onActionPoltergeistPosition ?? null;
            }
        }
        /// <summary>ゴーストが飛び出してくる演出の完了</summary>
        public ReactiveCommand<bool> IsCompletedBurstGhosts
        {
            get
            {
                return _playerModel?.InteractionPartTable?.isCompletedBurstGhosts ?? null;
            }
        }
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveProperty<InteractionPart> InteractionPart
        {
            get
            {
                return _playerModel?.InteractionPartTable?.interactionPart ?? null;
            }
        }
        /// <summary>ターゲットクロス位置</summary>
        public ReactiveCommand<Vector3> TargetCrossPosition
        {
            get
            {
                return _playerModel?.TargetCrossPosition ?? null;
            }
        }
        /// <summary>バッテリーのトランスフォーム</summary>
        public Transform BatteryTransform => _playerModel?.BatteryTransform ?? null;
        /// <summary>バッテリーが選択状態か</summary>
        public bool IsSelectedBattery => _playerModel?.IsSelectedBattery ?? false;
        /// <summary>[Script_xyloApi.cs]リズムパート失敗</summary>
        public ReactiveCommand<bool> IsFailed => _playerModel?.IsFailed ?? null;
        /// <summary>選択されたMissGhostAttack</summary>
        public Transform SelectedMissGhostAttackTransform => _playerModel?.SelectedMissGhostAttackTransform ?? null;
        /// <summary>ステージ開始演出が完了したか</summary>
        public bool _isCompletedStartDirection;
        /// <summary>ステージ開始演出が完了したか</summary>
        public bool IsCompletedStartDirection => _isCompletedStartDirection;
        /// <summary>ステージ開始演出が完了したか</summary>
        public ReactiveCommand<bool> IsCompletedStartDirectionReactive => _playerModel?.IsCompletedStartDirection ?? null;
        /// <summary>ステージ開始位置トランスフォーム</summary>
        private ReactiveCommand<Transform> _startPointTrans = new ReactiveCommand<Transform>();
        /// <summary>ステージ開始位置トランスフォーム</summary>
        public ReactiveCommand<Transform> StartPointTrans => _startPointTrans;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>ミッションクリアフラグ</summary>
        private bool _isMissionClear;
        /// <summary>ミッションクリアフラグ</summary>
        public bool IsMissionClear => _isMissionClear;
        /// <summary>ミッションクリアフラグ</summary>
        private ReactiveCommand<bool> _isMissionClearReactive = new ReactiveCommand<bool>();
        /// <summary>ミッションクリアフラグ</summary>
        public ReactiveCommand<bool> IsMissionClearReactive => _isMissionClearReactive;
        /// <summary>視界ジャック用ゴースト</summary>
        private ReactiveCommand<Transform> _targetGhost = new ReactiveCommand<Transform>();
        /// <summary>視界ジャック用ゴースト</summary>
        public ReactiveCommand<Transform> TargetGhost => _targetGhost;

        public PlayerViewModel(InteractionPartTable interactionPartTable)
        {
            PlayerModel model = GameObject.FindAnyObjectByType<PlayerModel>();
            if (model == null)
            {
                GameObject gameObject = new GameObject($"{typeof(PlayerModel).Name}");
                _playerModel = gameObject.AddComponent<PlayerModel>();
            }
            else
            {
                _playerModel = model;
            }
            if (_playerModel.InteractionPartTable == null)
                _playerModel.InteractionPartTable = interactionPartTable;
            _playerModel.StartPointTrans.Subscribe(x =>
            {
                _startPointTrans.Execute(x);
            })
                .AddTo(ref _disposableBag);
            _playerModel.IsCompletedStartDirection.Subscribe(x =>
            {
                _isCompletedStartDirection = x;
            })
                .AddTo(ref _disposableBag);
            _playerModel.IsMissionClear.Subscribe(isMissionClear =>
            {
                _isMissionClearReactive.Execute(isMissionClear);
            })
                .AddTo(ref _disposableBag);
            _playerModel.TargetGhost.Subscribe(targetGhost =>
            {
                _targetGhost.Execute(targetGhost);
            })
                .AddTo(ref _disposableBag);
        }

        public void SetIsSwitchPart(bool isSwitchPart)
        {
            if (_playerModel != null)
                _playerModel.SetIsSwitchPart(isSwitchPart);
        }

        public void SetPlayerTransform(Transform transform)
        {
            if (_playerModel != null)
                _playerModel.SetPlayerTransform(transform);
        }

        public void SetIsCompletedBurstGhosts(bool isCompletedBurstGhosts)
        {
            if (_playerModel != null)
                _playerModel.SetIsCompletedBurstGhosts(isCompletedBurstGhosts);
        }

        public void SetDbLevel(float dbLevel)
        {
            if (_playerModel != null)
                _playerModel.SetDbLevel(dbLevel);
        }

        public void SetInteractionPart(InteractionPart interactionPart)
        {
            if (_playerModel != null)
                _playerModel.SetInteractionPart(interactionPart);
        }

        public void SetHealthPointMax(int healthPointMax)
        {
            if (_playerModel != null)
                _playerModel.SetHealthPointMax(healthPointMax);
        }

        public void SetHealthPoint(int healthPoint)
        {
            if (_playerModel != null)
                _playerModel.SetHealthPoint(healthPoint);
        }

        public void SetBatteryTransform(Transform batteryTransform)
        {
            if (_playerModel != null)
                _playerModel.SetBatteryTransform(batteryTransform);
        }

        public void SetIsLockedUpdateHealthPoint(bool isLockedUpdateHealthPoint)
        {
            if (_playerModel != null)
                _playerModel.SetIsLockedUpdateHealthPoint(isLockedUpdateHealthPoint);
        }

        public void AddHorrorCount(float horrorCount)
        {
            if (_playerModel != null)
                _playerModel.AddHorrorCount(horrorCount);
        }

        public void SetIsStopHorrorCount(bool isStopHorrorCount)
        {
            if (_playerModel != null)
                _playerModel.SetIsStopHorrorCount(isStopHorrorCount);
        }

        /// <summary>
        /// 地面の接触判定
        /// </summary>
        /// <param name="characterController">キャラクター移動制御</param>
        /// <param name="distanceToGround">地面との距離</param>
        /// <param name="groundLayerMask">接地判定の対象レイヤー</param>
        /// <returns>地面に接触しているか</returns>
        public bool IsGrounded(CharacterController characterController, float distanceToGround, LayerMask groundLayerMask)
        {
            // CharacterControllerのisGroundedプロパティを優先的に使用
            // Move()の後に自動的に更新されるため、より正確
            if (characterController.isGrounded)
            {
                return true;
            }

            // CharacterControllerが無効な場合や、より詳細な判定が必要な場合のフォールバック
            float radius = characterController.radius;
            float skinWidth = characterController.skinWidth;
            // raycastDistanceの計算を改善：固定値0.83fを削除し、より適切な値に調整
            float raycastDistance = characterController.height / 2f - radius + skinWidth + distanceToGround;
            Vector3 rayOrigin = characterController.transform.position + Vector3.up * (radius - skinWidth);

            // SphereCastNonAllocで接地判定（指定されたレイヤーのみを対象）
            int hitCount = Physics.SphereCastNonAlloc(rayOrigin, radius, Vector3.down, _sphereCastHits, raycastDistance, groundLayerMask);
            // デバッグ：SceneビューにSphereCastのレイを描画
            Debug.DrawRay(rayOrigin, Vector3.down * raycastDistance, Color.yellow);

            // より確実な判定のため、追加でRaycastも実行
            if (hitCount == 0)
            {
                float rayDistance = characterController.height / 2f + distanceToGround;
                // デバッグ：SceneビューにRaycastのレイを描画
                Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, Color.cyan);
                return Physics.Raycast(rayOrigin, Vector3.down, rayDistance, groundLayerMask);
            }

            return hitCount > 0;
        }

        /// <summary>
        /// 調べるコマンドが有効かを取得
        /// </summary>
        /// <param name="eulerAngles">カメラ視線用のトランスフォームのオイラー角度</param>
        /// <param name="searchAngleMin">調べるコマンド_角度レベル最小</param>
        /// <param name="searchAngleMax">調べるコマンド_角度レベル最大</param>
        /// <returns>調べるコマンドが有効か</returns>
        public bool IsEnabledSearchAngle(Vector3 eulerAngles, float searchAngleMin, float searchAngleMax)
        {
            return searchAngleMin <= eulerAngles.x &&
                eulerAngles.x <= searchAngleMax;
        }

        public void SetIsPostRhythmFaceOff(bool isPostRhythmFaceOff)
        {
            if (_playerModel != null)
                _playerModel.SetIsPostRhythmFaceOff(isPostRhythmFaceOff);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
        }
    }
}
