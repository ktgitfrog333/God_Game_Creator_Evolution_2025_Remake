using DG.Tweening;
using Mains.Commons;
using Mains.External;
using Mains.ViewModels;
using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// オバケ弾：本のビュー
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GhostBulletBookView : GhostBulletAbstractView
    {
        /// <summary>オバケ弾：本の設定</summary>
        [SerializeField] private GhostBulletBookSettings settings;
        /// <summary>対象の追尾が完了</summary>
        private ReactiveCommand<bool> _isCompletedMoveToTarget = new ReactiveCommand<bool>();
        /// <summary>対象の追尾が完了</summary>
        public ReactiveCommand<bool> IsCompletedMoveToTarget => _isCompletedMoveToTarget;
        /// <summary>オバケ弾：本のビューモデル</summary>
        public GhostBulletBookViewModel ViewModel => (GhostBulletBookViewModel)(_viewModel != null ? _viewModel : _viewModel = ScriptableObject.CreateInstance<GhostBulletBookViewModel>());
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;

        private void Reset()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals("ModelFbx"))
                {
                    foreach (Transform item in child)
                    {
                        if (item.name.Equals("HaloParticleSys"))
                        {
                            if (settings.haloParticleSys == null)
                                settings.haloParticleSys = item.GetComponent<ParticleSystem>();

                            foreach (Transform item1 in item)
                            {
                                if (item1.name.Equals("RamenSteam"))
                                {
                                    if (settings.ramenSteamParticleSys == null)
                                        settings.ramenSteamParticleSys = item.GetComponent<ParticleSystem>();
                                }
                            }
                        }
                    }
                }
            }
            settings.hitTrigger = GetComponentInChildren<SphereCollider>();
            settings.hitGroundedTriggerDummy = GetComponentInChildren<CapsuleCollider>();
            settings.rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // オバケ弾：本のビューモデル
            GhostBulletBookViewModel viewModel = ViewModel;
            viewModel.DoInitialize(((GhostBulletBookViewModel)settings.viewModel).Settings, settings.hitTrigger);
            // トランスフォーム
            var trans = transform;
            // ModelFbxのトランスフォーム
            Transform modelFbxTrans = null;
            foreach (Transform child in trans)
            {
                if (child.name.Equals("ModelFbx"))
                {
                    modelFbxTrans = child;
                }
            }
            // 対象の追尾が完了
            ReactiveCommand<bool> isCompletedMoveToTarget = _isCompletedMoveToTarget;
            // オバケ弾：本のビューモデル設定
            var detailSettings = viewModel.Settings;
            viewModel.ModelFbxTrans = modelFbxTrans;
            viewModel.Transform = trans;
            if (!detailSettings.isStartStop)
            {
                // 最初は存在せず、登場してから動くパターン
                PlayAttackAnimationAndInputMove(settings, viewModel)
                    .Subscribe(_ =>
                    {
                        StopMovement(settings, viewModel).Subscribe(_ =>
                        {
                            PlayVanishAnimation(settings, viewModel, trans, modelFbxTrans);
                            isCompletedMoveToTarget.Execute(true);
                        })
                            .AddTo(ref _disposableBag);
                    })
                    .AddTo(ref _disposableBag);
            }
            isCompletedMoveToTarget.Where(x => x)
                .Take(1)
                .Subscribe(_ =>
                {
                    ResetStatus();
                })
                .AddTo(ref _disposableBag);
            viewModel.IsDidPlayAttackAnimationAndChangeRhythm.Subscribe(_ =>
            {
                StopMovement(settings, viewModel)
                    .Subscribe(_ =>
                    {
                        PlayVanishAnimation(settings, viewModel, trans, modelFbxTrans);
                        isCompletedMoveToTarget.Execute(true);
                    })
                    .AddTo(ref _disposableBag);
            })
                .AddTo(ref _disposableBag);
            // 1度のみ実行させる
            bool oneceSetIsHitGhostAttack = false;
            _script_XyloApi = new Script_xyloApi();
            viewModel.IsHitAny.Subscribe(isHitAny =>
            {
                switch (isHitAny)
                {
                    case 1:
                        if (!oneceSetIsHitGhostAttack)
                        {
                            oneceSetIsHitGhostAttack = true;
                            viewModel.SetIsHitGhostAttack(true);
                        }

                        break;
                    case 2:
                        StopMovement(settings, viewModel)
                            .Subscribe(_ =>
                            {
                                PlayVanishAnimation(settings, viewModel, trans, modelFbxTrans);
                                isCompletedMoveToTarget.Execute(true);
                            })
                            .AddTo(ref _disposableBag);

                        break;
                }
            })
                .AddTo(ref _disposableBag);
            // Rigidbody
            var rigidbody = settings.rigidbody;
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    MovementNotRB(viewModel);
                })
                .AddTo(ref _disposableBag);
            float lastFixedTime = 0f;
            Observable.EveryUpdate()
                .Where(_ => Time.time - lastFixedTime >= Time.fixedDeltaTime) // FixedUpdateと同じタイミング
                .Subscribe(_ =>
                {
                    lastFixedTime = Time.time; // 次の実行タイミングを記録
                    Movement(viewModel, settings);
                })
                .AddTo(ref _disposableBag);
         }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _script_XyloApi?.Dispose();
        }

        /// <summary>
        /// 予備動作の停止位置をセット
        /// </summary>
        /// <param name="bulletWaitPosition">予備動作の停止位置</param>
        public void SetBulletWaitPosition(Vector3 bulletWaitPosition)
        {
            // オバケ弾：本のビューモデル
            GhostBulletBookViewModel viewModel = ViewModel;
            viewModel.BulletWaitPosition = bulletWaitPosition;
        }

        /// <summary>
        /// 追尾の対象をセット
        /// </summary>
        /// <param name="target">追尾の対象</param>
        public void SetTarget(Transform target)
        {
            // オバケ弾：本のビューモデル
            GhostBulletBookViewModel viewModel = ViewModel;
            viewModel.Target = target;
        }

        /// <summary>
        /// 弾との衝突対象外コライダー情報をセット
        /// </summary>
        /// <param name="ignorePhysicsGhostBullets">弾との衝突対象外コライダー情報</param>
        public void SetIgnorePhysicsGhostBullets(List<int> ignorePhysicsGhostBullets)
        {
            // オバケ弾：本のビューモデル
            GhostBulletBookViewModel viewModel = ViewModel;
            viewModel.IgnorePhysicsGhostBullets = ignorePhysicsGhostBullets.ToArray();
        }

        /// <summary>
        /// 対象へ移動する処理を呼び出す
        /// </summary>
        public void DoMoveToTarget()
        {
            // オバケ弾：本のビューモデル
            GhostBulletBookViewModel viewModel = ViewModel;
            // トランスフォーム
            var trans = viewModel.Transform;
            // モデルのトランスフォーム
            var modelFbxTrans = viewModel.ModelFbxTrans;
            // 対象の追尾が完了
            ReactiveCommand<bool> isCompletedMoveToTarget = _isCompletedMoveToTarget;
            PlayAttackAnimationAndInputMove(settings, viewModel)
                .Subscribe(_ =>
                {
                    StopMovement(settings, viewModel).Subscribe(_ =>
                    {
                        PlayVanishAnimation(settings, viewModel, trans, modelFbxTrans);
                        isCompletedMoveToTarget.Execute(true);
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        /// <summary>
        /// 攻撃アニメーション再生及び移動の入力制御
        /// </summary>
        /// <param name="settings">オバケ弾：本の設定</param>
        /// <param name="viewModel">オバケ弾：本のビューモデル</param>
        /// <returns>オブザーバブル</returns>
        private Observable<Unit> PlayAttackAnimationAndInputMove(GhostBulletBookSettings settings, GhostBulletBookViewModel viewModel)
        {
            return Observable.Create<Unit>((System.Func<Observer<Unit>, System.IDisposable>)(observer =>
            {
                viewModel.IsDidPlayAttackAnimation = true;
                // 予備動作の停止位置
                Vector3? bulletWaitPosition = viewModel.BulletWaitPosition;
                // 追尾の対象
                Transform target = viewModel.Target;
                if (!bulletWaitPosition.HasValue ||
                    target == null)
                {

                    return Disposable.Empty;
                }

                // 攻撃時の予備動作（家具が発光する演出）を追加
                Sequence seq = DOTween.Sequence();
                // オバケ弾：本のビューモデル設定
                var detailSettings = viewModel.Settings;
                // アニメーション終了時間
                var durations = detailSettings.durations;
                // トランスフォーム
                Transform trans = viewModel.Transform;
                if (!detailSettings.isStartStop)
                {
                    seq.Join(trans.DOScale(1f, durations[0]));
                }
                var haloParticleSys = settings.haloParticleSys;
                var ramenSteamParticleSys = settings.ramenSteamParticleSys;
                // モデルのトランスフォーム
                Transform modelFbxTrans = viewModel.ModelFbxTrans;
                // 対象へ向かせるかのフラグ
                ReactiveProperty<bool> isLookAt = viewModel.IsLookAt;
                seq.AppendCallback(() =>
                {
                    haloParticleSys.Play();
                    ramenSteamParticleSys.Play();
                    isLookAt.Value = true;
                })
                    .Append(trans.DOMove(bulletWaitPosition.Value, durations[1]))
                    .Append(modelFbxTrans.DOLocalMoveY(detailSettings.modelFbxFloatDistance, durations[2] / 2f)
                        .SetLoops(2, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine))
                    .AppendCallback(() =>
                    {
                        var main = haloParticleSys.main;
                        var mainColor = main.startColor.color;
                        var r = new Color(255f, 0f, 87f, 255f);
                        var updColor = new Color(r.r, r.g, r.b, mainColor.a);
                        var particles = new ParticleSystem.Particle[haloParticleSys.main.maxParticles];
                        int count = haloParticleSys.GetParticles(particles);

                        for (int i = 0; i < count; i++)
                        {
                            particles[i].startColor = updColor;
                        }

                        haloParticleSys.SetParticles(particles, count);
                    })
                    .Append(modelFbxTrans.DOLocalMoveY(detailSettings.modelFbxFloatDistance, durations[3] / 2f)
                        .SetLoops(2, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine))
                    .Append(modelFbxTrans.DOLocalMove(Vector3.back * detailSettings.modelFbxBackStepDistance, durations[4])
                        .SetEase(Ease.InOutSine))
                    .Join(modelFbxTrans.DOBlendableLocalMoveBy(Vector3.up * detailSettings.modelFbxFloatDistance, durations[5] / 2f)
                        .SetLoops(2, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine))
                    ;
                // 攻撃用のオブジェクトをプレイヤーへ移動させる（直線、追尾ではない）機能を追加
                Tweener fbxTransTween = null;
                Tweener fbxTransTween1 = null;
                // 進行可否フラグ
                ReactiveProperty<bool> isGoing = viewModel.IsGoing;
                // FBX側のトゥイーン
                ReactiveProperty<Tweener> modelFbxTransTween = viewModel.ModelFbxTransTween;
                ReactiveProperty<Tweener> modelFbxTransTween1 = viewModel.ModelFbxTransTween1;
                seq.AppendCallback(() =>
                {
                    modelFbxTrans.eulerAngles = Vector3.zero;
                    isGoing.Value = true;
                    settings.hitTrigger.enabled = true;
                    isLookAt.Value = false;
                    fbxTransTween = modelFbxTrans.DOBlendableLocalMoveBy(Vector3.zero, durations[6]);
                    fbxTransTween1 = modelFbxTrans.DOBlendableLocalRotateBy(
                        new Vector3(
                            Random.Range(-180f, 180f),
                            Random.Range(-180f, 180f),
                            Random.Range(-180f, 180f)),
                        detailSettings.modelFbxSpinSpeed)
                        .SetEase(Ease.Linear)
                        .SetSpeedBased(true)
                        .SetLoops(-1, LoopType.Incremental);
                    modelFbxTransTween.Value = fbxTransTween;
                    modelFbxTransTween1.Value = fbxTransTween1;
                })
                    .AppendInterval(durations[7]);
                seq.OnComplete(() =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                });
                viewModel.Sequence = seq;

                return Disposable.Empty;
            }));
        }

        /// <summary>
        /// 状態の初期化
        /// </summary>
        private void ResetStatus()
        {
            // オバケ弾：本のビューモデル
            GhostBulletBookViewModel viewModel = (GhostBulletBookViewModel)_viewModel;
            viewModel.BulletWaitPosition = null;
            viewModel.Target = null;
            viewModel.MoveDirection = null;
            viewModel.IsDidPlayAttackAnimation = false;
            viewModel.IsDidStopMovement = false;
        }

        /// <summary>
        /// 移動処理を停止
        /// </summary>
        /// <param name="settings">オバケ弾：本の設定</param>
        /// <param name="viewModel">オバケ弾：本のビューモデル</param>
        /// <returns>オブザーバブル</returns>
        private Observable<Unit> StopMovement(GhostBulletBookSettings settings, GhostBulletBookViewModel viewModel)
        {
            return Observable.Create<Unit>(observer =>
            {
                if (viewModel.IsDidStopMovement)
                {

                    return Disposable.Empty;
                }
                viewModel.IsDidStopMovement = true;

                // 移動停止、演出を停止、オブジェクトは無効化
                settings.hitTrigger.enabled = false;
                // 対象へ向かせるかのフラグ
                ReactiveProperty<bool> isLookAt = viewModel.IsLookAt;
                isLookAt.Value = false;
                // 進行可否フラグ
                ReactiveProperty<bool> isGoing = viewModel.IsGoing;
                isGoing.Value = false;
                // 攻撃時の予備動作（家具が発光する演出）
                Sequence sequence = viewModel.Sequence;
                if (sequence != null &&
                    sequence.IsActive())
                {
                    sequence.Kill();
                }
                // FBX側のシークエンス
                ReactiveProperty<Tweener> modelFbxTransTween = viewModel.ModelFbxTransTween;
                if (modelFbxTransTween.Value != null &&
                    modelFbxTransTween.Value.IsActive())
                {
                    modelFbxTransTween.Value.Kill();
                }
                ReactiveProperty<Tweener> modelFbxTransTween1 = viewModel.ModelFbxTransTween1;
                if (modelFbxTransTween1.Value != null &&
                    modelFbxTransTween1.Value.IsActive())
                {
                    modelFbxTransTween1.Value.Kill();
                }
                var haloParticleSys = settings.haloParticleSys;
                haloParticleSys.gameObject.SetActive(false);
                // 飛んでいったオブジェクトが「壁」や「床」へ当たった場合に消す演出
                var rigidbody = settings.rigidbody;
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                var detailSettings = viewModel.Settings;
                viewModel.IsGrounded(detailSettings.durations[8], settings.hitGroundedTriggerDummy).Take(1)
                    .Subscribe(_ =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                    .AddTo(ref _disposableBag);

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// 消失アニメーションを再生
        /// </summary>
        /// <param name="settings">オバケ弾：本の設定</param>
        /// <param name="viewModel">オバケ弾：本のビューモデル</param>
        /// <param name="trans">トランスフォーム</param>
        /// <param name="modelFbxTrans">ModelFbxのトランスフォーム</param>
        private void PlayVanishAnimation(GhostBulletBookSettings settings, GhostBulletBookViewModel viewModel, Transform trans, Transform modelFbxTrans)
        {
            var rigidbody = settings.rigidbody;
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            var detailSettings = viewModel.Settings;
            var durations = detailSettings.durations;
            var dustParticle = settings.dustParticle;
            var dustParticlePrefab = detailSettings.dustParticlePrefab;
            // 縮小して消えるDOTween
            trans.DOScale(0f, durations[9])
                .OnComplete(() =>
                {
                    // 消失時の土埃発生パーティクル再生
                    dustParticle.Play();
                    Instantiate(dustParticlePrefab, trans.position, Quaternion.identity);
                    // モデルの向きをリセット
                    modelFbxTrans.eulerAngles = Vector3.zero;
                    var haloParticleSys = settings.haloParticleSys;
                    haloParticleSys.gameObject.SetActive(true);
                    gameObject.SetActive(false);
                });
        }

        /// <summary>
        /// 移動制御
        /// </summary>
        /// <param name="viewModel">オバケ弾：本のビューモデル</param>
        private void MovementNotRB(GhostBulletBookViewModel viewModel)
        {
            // 追尾の対象
            Transform target = viewModel.Target;
            // 対象へ向かせるかのフラグ
            bool isLookAt = viewModel.IsLookAt.Value;
            // モデルのトランスフォーム
            Transform modelFbxTrans = viewModel.ModelFbxTrans;
            if (target != null)
            {
                if (isLookAt)
                {
                    modelFbxTrans.LookAt(target);
                }
            }
        }

        /// <summary>
        /// 移動制御
        /// </summary>
        /// <param name="viewModel">オバケ弾：本のビューモデル</param>
        /// <param name="settings">オバケ弾：本の設定</param>
        private void Movement(GhostBulletBookViewModel viewModel, GhostBulletBookSettings settings)
        {
            Transform target = viewModel.Target;
            if (target != null)
            {
                bool isGoing = viewModel.IsGoing.Value;
                if (isGoing)
                {
                    Rigidbody rigidbody = settings.rigidbody;
                    Vector3? moveDirection = viewModel.MoveDirection;
                    if (!moveDirection.HasValue)
                    {
                        var delta = target.position - rigidbody.position;
                        if (delta.sqrMagnitude < 0.0001f)
                        {
                            return;
                        }
                        moveDirection = delta.normalized;
                        viewModel.MoveDirection = moveDirection;
                    }
                    GhostBulletBookVMSettings detailSettings = viewModel.Settings;
                    rigidbody.MovePosition(rigidbody.position + moveDirection.Value * detailSettings.moveSpeed * Time.fixedDeltaTime);
                }
            }
        }
    }

    /// <summary>
    /// オバケ弾：本の設定
    /// </summary>
    [System.Serializable]
    public class GhostBulletBookSettings : GhostBulletAbstractSettings
    {
        /// <summary>発光パーティクル</summary>
        public ParticleSystem haloParticleSys;
        /// <summary>煙パーティクル</summary>
        public ParticleSystem ramenSteamParticleSys;
        /// <summary>土埃パーティクル</summary>
        public ParticleSystem dustParticle;
        /// <summary>衝突判定用の球体トリガー</summary>
        public SphereCollider hitTrigger;
        /// <summary>接地判定用のカプセル型トリガー</summary>
        public CapsuleCollider hitGroundedTriggerDummy;
        /// <summary>Rigidbody</summary>
        public Rigidbody rigidbody;
    }
}
