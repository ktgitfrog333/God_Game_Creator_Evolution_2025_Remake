using DG.Tweening;
using Mains.Commons;
using Mains.Models;
using R3;
using R3.Triggers;
using System.Linq;
using UnityEngine;

namespace Mains.ViewModels
{
    /// <summary>
    /// オバケ弾：本のビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "GhostBulletBookViewModel", menuName = "Scriptable Objects/GhostBulletBookViewModel")]
    public class GhostBulletBookViewModel : GhostBulletAbstractViewModel, IGhostBulletBookModel
    {
        /// <summary>オバケ弾：本のビューモデル設定</summary>
        [SerializeField] private GhostBulletBookVMSettings settings;
        /// <summary>オバケ弾：本のビューモデル設定</summary>
        public GhostBulletBookVMSettings Settings => settings;
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        private ReactiveCommand<InteractionPart> _interactionPart = new ReactiveCommand<InteractionPart>();
        /// <summary>【探索／シャウトチャンス／リズム】パート</summary>
        public ReactiveCommand<InteractionPart> InteractionPart => _interactionPart;
        /// <summary>攻撃アニメーションが再生されたか</summary>
        public bool IsDidPlayAttackAnimation { get; set; }
        /// <summary>攻撃アニメーション再生かつリズムパートの購読</summary>
        private Subject<Unit> _isDidPlayAttackAnimationAndChangeRhythm = new Subject<Unit>();
        /// <summary>攻撃アニメーション再生かつリズムパートの購読</summary>
        public Subject<Unit> IsDidPlayAttackAnimationAndChangeRhythm => _isDidPlayAttackAnimationAndChangeRhythm;
        /// <summary>いずれかにヒットする</summary>
        /// <remarks>[0]: 初期値<br/>
        /// [1]: プレイヤー<br/>
        /// [2]: 床、壁</remarks>
        private ReactiveCommand<int> _isHitAny = new ReactiveCommand<int>();
        /// <summary>いずれかにヒットする</summary>
        public ReactiveCommand<int> IsHitAny => _isHitAny;
        /// <summary>接地状態の監視</summary>
        private SerialDisposable _isGroundedDisposable = new SerialDisposable();

        /// <summary>予備動作の停止位置</summary>
        public Vector3? BulletWaitPosition { get; set; }
        /// <summary>追尾の対象</summary>
        public Transform Target { get; set; }
        /// <summary>直進方向</summary>
        public Vector3? MoveDirection { get; set; }
        /// <summary>トランスフォーム</summary>
        public Transform Transform { get; set; }
        /// <summary>モデルのトランスフォーム</summary>
        public Transform ModelFbxTrans { get; set; }
        /// <summary>対象へ向かせるかのフラグ</summary>
        private ReactiveProperty<bool> _isLookAt = new ReactiveProperty<bool>();
        /// <summary>対象へ向かせるかのフラグ</summary>
        public ReactiveProperty<bool> IsLookAt => _isLookAt;
        /// <summary>進行可否フラグ</summary>
        private ReactiveProperty<bool> _isGoing = new ReactiveProperty<bool>();
        /// <summary>進行可否フラグ</summary>
        public ReactiveProperty<bool> IsGoing => _isGoing;
        /// <summary>攻撃時の予備動作（家具が発光する演出）</summary>
        public Sequence Sequence { get; set; }
        /// <summary>FBX側のトゥイーン</summary>
        private ReactiveProperty<Tweener> _modelFbxTransTween = new ReactiveProperty<Tweener>();
        /// <summary>FBX側のトゥイーン</summary>
        public ReactiveProperty<Tweener> ModelFbxTransTween => _modelFbxTransTween;
        /// <summary>FBX側のトゥイーン</summary>
        private ReactiveProperty<Tweener> _modelFbxTransTween1 = new ReactiveProperty<Tweener>();
        /// <summary>FBX側のトゥイーン</summary>
        public ReactiveProperty<Tweener> ModelFbxTransTween1 => _modelFbxTransTween1;
        /// <summary>弾との衝突対象外コライダー情報</summary>
        public int[] IgnorePhysicsGhostBullets { get; set; }
        /// <summary>移動処理を停止が呼び出されたか</summary>
        public bool IsDidStopMovement { get; set; }

        public void DoInitialize(GhostBulletAbstractVMSettings detailSettings, SphereCollider hitTrigger)
        {
            settings = (GhostBulletBookVMSettings)detailSettings;
            base.DoInitialize();
            hitTrigger.OnTriggerEnterAsObservable()
                .Subscribe(trigger =>
                {
                    var layer = trigger.gameObject.layer;
                    if (trigger.CompareTag("Player"))
                    {
                        // ここだけタグ判定のため注意
                        _isHitAny.Execute(1);
                    }
                    else if (layer == LayerMask.NameToLayer("TerrainObjects"))
                    {
                        _isHitAny.Execute(2);
                    }
                    else if(layer == LayerMask.NameToLayer("StaticObjects"))
                    {
                        // 弾との衝突対象外コライダー情報
                        var ignorePhysicsGhostBullets = IgnorePhysicsGhostBullets;
                        if (ignorePhysicsGhostBullets == null ||
                            ignorePhysicsGhostBullets.Length < 1)
                        {
                            _isHitAny.Execute(2);
                        }
                        else
                        {
                            // 衝突イベントは短いサイクルで発生する可能性があり念のためLinq禁止縛りの実装
                            foreach (var ignorePhysicsGhostBullet in ignorePhysicsGhostBullets)
                            {
                                if (ignorePhysicsGhostBullet == trigger.GetInstanceID())
                                {

                                    return;
                                }
                            }
                            _isHitAny.Execute(2);
                        }
                    }
                })
                .AddTo(ref _disposableBag);
        }

        protected override void Initialize(PlayerModel playerModel)
        {
            Observable.EveryUpdate()
                .Select(_ => playerModel.InteractionPartTable)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(table =>
                {
                    table.interactionPart.Subscribe(interactionPart =>
                    {
                        _interactionPart.Execute(interactionPart);
                        switch (interactionPart)
                        {
                            case Commons.InteractionPart.Rhythm:
                                if (IsDidPlayAttackAnimation)
                                {
                                    _isDidPlayAttackAnimationAndChangeRhythm.OnNext(Unit.Default);
                                    _isDidPlayAttackAnimationAndChangeRhythm.OnCompleted();
                                }

                                break;
                        }
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        public void SetIsHitGhostAttack(bool isHitGhostAttack)
        {
            if (_playerModel != null)
                _playerModel.SetIsHitGhostAttack(isHitGhostAttack);
        }

        /// <summary>
        /// 接地状態か
        /// </summary>
        /// <param name="floatingTimeLimit">最長浮遊時間</param>
        /// <param name="hitGroundedTriggerDummy">接地判定用のカプセル型トリガー</param>
        /// <returns>オブザーバブル</returns>
        public Observable<Unit> IsGrounded(float floatingTimeLimit, CapsuleCollider hitGroundedTriggerDummy)
        {
            return Observable.Create<Unit>(observer =>
            {
                float floatingTime = 0f;
                float lastFixedTime = 0f;
                RaycastHit[] sphereCastHits = new RaycastHit[1];
                bool isUnLoop = false;
                // 接地状態の監視
                SerialDisposable disposable = _isGroundedDisposable;
                disposable.Disposable = Observable.EveryUpdate()
                    .Where(_ => !isUnLoop)
                    .Where(_ => Time.time - lastFixedTime >= Time.fixedDeltaTime) // FixedUpdateと同じタイミング
                    .Subscribe(_ =>
                    {
                        lastFixedTime = Time.time; // 次の実行タイミングを記録
                        if (floatingTimeLimit <= floatingTime)
                        {
                            observer.OnNext(Unit.Default);
                            observer.OnCompleted();
                            isUnLoop = true;

                            return;
                        }

                        var trans = hitGroundedTriggerDummy.transform;
                        var scale = trans.lossyScale;
                        var radius = hitGroundedTriggerDummy.radius * Mathf.Max(scale.x, scale.z);
                        var height = hitGroundedTriggerDummy.height * Mathf.Abs(scale.y);
                        var halfHeight = Mathf.Max(height * 0.5f, radius);
                        var worldCenter = trans.TransformPoint(hitGroundedTriggerDummy.center);
                        var bottom = worldCenter + Vector3.down * (halfHeight - radius);
                        var origin = bottom + Vector3.up * 0.01f;
                        var distance = radius;
                        var layerMask = LayerMask.GetMask("TerrainObjects");

                        int hitCount = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, sphereCastHits, distance, layerMask);
                        Debug.DrawRay(origin, Vector3.down * distance, Color.yellow);
                        if (0 < hitCount)
                        {
                            floatingTime = 0f;
                            observer.OnNext(Unit.Default);
                            observer.OnCompleted();
                            isUnLoop = true;

                            return;
                        }
                        else
                        {
                            Debug.DrawRay(origin, Vector3.down * distance, Color.cyan);
                            if (Physics.Raycast(origin, Vector3.down, distance, layerMask))
                            {
                                floatingTime = 0f;
                                observer.OnNext(Unit.Default);
                                observer.OnCompleted();
                                isUnLoop = true;

                                return;
                            }
                            else
                            {
                                floatingTime += Time.fixedDeltaTime;
                            }
                        }
                    })
                    .AddTo(ref _disposableBag);

                return Disposable.Empty;
            });
        }

        protected override void DisposeAdd()
        {
            base.DisposeAdd();
            _isGroundedDisposable.Dispose();
        }
    }

    /// <summary>
    /// オバケ弾：本のビューモデル設定
    /// </summary>
    [System.Serializable]
    public class GhostBulletBookVMSettings : GhostBulletAbstractVMSettings
    {
        /// <summary>アニメーション終了時間</summary>
        public float[] durations;
        /// <summary>移動速度</summary>
        public float moveSpeed;
        /// <summary>浮遊距離</summary>
        public float modelFbxFloatDistance;
        /// <summary>後退距離</summary>
        public float modelFbxBackStepDistance;
        /// <summary>回転速度</summary>
        public float modelFbxSpinSpeed;
        /// <summary>土埃パーティクルプレハブ</summary>
        public Transform dustParticlePrefab;
    }
}
