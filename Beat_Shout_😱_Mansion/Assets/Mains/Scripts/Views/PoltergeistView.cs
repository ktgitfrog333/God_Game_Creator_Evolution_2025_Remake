using DG.Tweening;
using Mains.Commons;
using Mains.External;
using Mains.ViewModels;
using ObservableCollections;
using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// ポルターガイストのビュー
    /// </summary>
    public class PoltergeistView : MonoBehaviour
    {
        [Tooltip("Assets/Mains/Scripts/Commons/PoltergeistTable.assetをセットしておく。")]
        [SerializeField] private PoltergeistTable poltergeistTable;
        /// <summary>リズムパート位置（プレイヤー位置をリズムパート用に移動させる）</summary>
        private Vector3 _rhythmPartPosition;
        /// <summary>リズムパート角度（プレイヤー位置をリズムパート用に移動させる）</summary>
        private Vector3 _rhythmPartEulerAngles;
        /// <summary>リズムパート位置（プレイヤー位置をリズムパート用に移動させる）</summary>
        public Vector3 RhythmPartPosition => _rhythmPartPosition;
        /// <summary>リズムパート角度（プレイヤー位置をリズムパート用に移動させる）</summary>
        public Vector3 RhythmPartEulerAngles => _rhythmPartEulerAngles;
        /// <summary>リズムパート位置（家具をリズムパート用に移動させる）</summary>
        private Vector3 _rhythmPartPosition_1;
        /// <summary>リズムパート角度（家具をリズムパート用に移動させる）</summary>
        private Vector3 _rhythmPartEulerAngles_1;
        /// <summary>オバケの家具入居管理の構造体</summary>
        [SerializeField] private GhostInStaticObjectStruct ghostInStaticObjectStruct;
        /// <summary>オバケの家具入居管理の構造体</summary>
        public GhostInStaticObjectStruct GhostInStaticObjectStruct
        {
            get
            {
                return ghostInStaticObjectStruct;
            }
            set
            {
                ghostInStaticObjectStruct = value;
            }
        }
        /// <summary>ポルターガイストの設定</summary>
        [SerializeField] private PoltergeistSettings settings;
        /// <summary>ミサイルテンポスポナー</summary>
        private Transform _missileTempoSpawnerInstance;
        /// <summary>ポルターガイストのビューモデル</summary>
        private PoltergeistViewModel _poltergeistViewModel;
        /// <summary>モーターのビュー</summary>
        private MotorView _motorView;
        /// <summary>トランスフォーム</summary>
        private Transform _transform;
        /// <summary>初期ポジション</summary>
        private Vector3 _initialPosition;
        /// <summary>初期オイラー角度</summary>
        private Vector3 _initialEulerAngles;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>フェードイメージのビュー</summary>
        private FadeImageView _fadeImageView;
        /// <summary>MissileObjectPoolerのカスタマイズビュー</summary>
        private HomingObjectPoolerCustomizeView _homingObjectPoolerCustomizeView;
        /// <summary>ObjectPoolerXyloOtherのカスタマイズビュー</summary>
        private ObjectPoolerXyloOtherCustomizeView _objectPoolerXyloOtherCustomizeView;
        /// <summary>トリガー以外のコライダー</summary>
        private List<Collider> _noTriggerColliders;
        /// <summary>Rigidbody</summary>
        private Rigidbody _rigidbody;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>パトロール（引っ越し）タイマー</summary>
        private System.IDisposable _patrolTimerDisposable = null;
        /// <summary>移動用オバケ生成位置</summary>
        private Vector3 _missGhostEscapePosition = Vector3.zero;
        /// <summary>移動用オバケ生成角度</summary>
        private Vector3 _missGhostEscapeEulerAngles = Vector3.zero;

        /// <summary>パトロール（引っ越し）の残り時間</summary>
        private float _patrolRemainingTime = -1f;

#if UNITY_EDITOR
        /// <summary>エディター表示用：パトロールの残り時間 (-1なら停止中)</summary>
        public float DebugPatrolRemainingTime => _patrolRemainingTime;
        /// <summary>エディター表示用：パトロール間隔</summary>
        public float DebugPatrolInterval { get; private set; } = -1f;
#endif
        /// <summary>監視かつSE再生用のDisposable</summary>
        private System.IDisposable _soundOutputBehaviorDisposable = null;
        /// <summary>オブジェクトプールビュー</summary>
        private ObjectsPoolView _objectsPoolView;
        /// <summary>オブジェクトプールビュー</summary>
        private ObjectsPoolView ObjectsPoolView => _objectsPoolView != null ? _objectsPoolView : _objectsPoolView = GameObject.FindAnyObjectByType<ObjectsPoolView>();
        /// <summary>
        /// 笑い声再生間隔のタイプ
        /// </summary>
        private enum LaughPhase
        {
            /// <summary>初回：1秒間隔で1回</summary>
            First,
            /// <summary>2回目～：2.5秒間隔で3回</summary>
            Second,
            /// <summary>最終：5秒間隔で無制限</summary>
            Final,
        }
        /// <summary>笑い声再生間隔のタイプ</summary>
        private LaughPhase _currentLaughPhase = LaughPhase.First;
        /// <summary>現在のフェーズで残り何回まで連続再生するか（-1は無制限）</summary>
        private int _remainingRepeatsInPhase;
        /// <summary>現在のフェーズでの再生間隔（秒）</summary>
        private float _currentLaughInterval;

        private void Reset()
        {
            // リズムパートポジション（プレイヤー位置をリズムパート用に移動させる）の生成
            FindOrInstanceGameObject("RhythmPartPosition");
            // リズムパートポジション（家具をリズムパート用に移動させる）の生成
            FindOrInstanceGameObject("RhythmPartPosition_1");
            // パーティクル位置の生成
            FindOrInstanceGameObject("DustParticlePosition");
            // 移動用オバケ位置の生成
            FindOrInstanceGameObject("MissGhostEscapePosition");
            // オバケ攻撃（遠距離系）
            FindOrInstanceGameObject("GhostTurretPosition");
            // オバケ攻撃予備動作の停止位置
            FindOrInstanceGameObject("GhostBulletWaitPosition");
            // リズムパート時の移動先情報を設定するためボックスコライダーを必要に応じて生成
            var collider = transform.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = transform.gameObject.AddComponent<BoxCollider>();
                collider.enabled = false;
                Debug.LogWarning($"ボックスコライダーを生成しました。必要に応じてサイズを変更してください。\r\n※PivotとCenter座標にずれがある場合は一度親子関係を解除して修正してください。");
            }
        }

        private void Start()
        {
            var trans = transform;
            _transform = trans;
            _initialPosition = _transform.position;
            _initialEulerAngles = _transform.eulerAngles;
            // Poltergeistの生成
            var originParent = _transform.parent;
            // 初期化
            var motorInstance = Instantiate(poltergeistTable.motorPrefab, _transform.position, Quaternion.identity);
            // 弾との衝突対象外コライダー情報
            /*
             * 攻撃オバケが放つ弾オブジェクトは親子関係を持たずに管理する前提のため、
             * 当該オブジェクト内に自身の親が持つ干渉コライダー情報を渡す
             * 
             * 子側で親のみ対象から外すことで
             *  ●発射時にプレイヤーへ向かう
             *  ●親のコライダーに接触して落下処理へ移行
             * の様な事故を回避する
             */
            List<int> ignorePhysicsGhostBullets = new List<int>();
            // ベースとなるオブジェクトのコライダーのプロパティをMotorへコピー
            motorInstance.GetComponent<BoxCollider>().center = _transform.GetComponent<BoxCollider>().center;
            motorInstance.GetComponent<BoxCollider>().size = _transform.GetComponent<BoxCollider>().size;
            motorInstance.transform.eulerAngles = _transform.eulerAngles;
            motorInstance.transform.SetParent(originParent);
            _transform.SetParent(motorInstance.transform);
            _transform.localPosition = Vector3.zero;
            _noTriggerColliders = new List<Collider>();
            _noTriggerColliders.Add(motorInstance.GetComponent<BoxCollider>());
            _rigidbody = motorInstance.GetComponent<Rigidbody>();
            // 静的コライダー群の生成
            var originParent_1 = motorInstance.transform.parent;
            // 静的コライダー群
            Transform staticColldersInstance = Instantiate(poltergeistTable.staticColldersPrefab, motorInstance.transform.position, Quaternion.identity).transform;
            // シャウトチャンスの範囲
            Transform shoutChanceInstance = null;
            // プレイヤーガード
            Transform playerGuardInstance = null;
            // オバケの攻撃開始範囲
            Transform startAttackInstance = null;
            foreach (Transform child in staticColldersInstance)
            {
                if (child.name.Equals("ShoutChanceRange"))
                {
                    shoutChanceInstance = child;
                }
                if (child.name.Equals("PlayerGuard"))
                {
                    playerGuardInstance = child;
                }
                if (child.name.Equals("StartAttackRange"))
                {
                    startAttackInstance = child;
                }
            }
            // ベースとなるオブジェクトのコライダーのプロパティをPlayerGuardへコピー
            playerGuardInstance.GetComponent<BoxCollider>().center = _transform.GetComponent<BoxCollider>().center;
            playerGuardInstance.GetComponent<BoxCollider>().size = _transform.GetComponent<BoxCollider>().size;
            staticColldersInstance.transform.eulerAngles = _transform.eulerAngles;
            staticColldersInstance.SetParent(originParent_1);
            motorInstance.transform.SetParent(staticColldersInstance);
            _noTriggerColliders.Add(playerGuardInstance.GetComponent<BoxCollider>());
            motorInstance.transform.localPosition = Vector3.zero;
            _transform.SetParent(motorInstance.transform);
            _transform.localPosition = Vector3.zero;
            ignorePhysicsGhostBullets.Add(playerGuardInstance.GetComponent<BoxCollider>().GetInstanceID());
            foreach (Transform child in _transform)
            {
                if (child.name.Equals("RhythmPartPosition"))
                {
                    _rhythmPartPosition = child.position;
                    _rhythmPartEulerAngles = child.eulerAngles;
                    break;
                }
            }
            foreach (Transform child in _transform)
            {
                if (child.name.Equals("RhythmPartPosition_1"))
                {
                    _rhythmPartPosition_1 = child.position;
                    _rhythmPartEulerAngles_1 = child.eulerAngles;
                    break;
                }
            }
            _motorView = motorInstance.GetComponent<MotorView>();
            // 音の出力タイプ
            var soundOutputType = ghostInStaticObjectStruct.soundOutputType;
            switch (soundOutputType)
            {
                case SoundOutputType.TableDefault:
                    soundOutputType = poltergeistTable.subSettings.defaultSoundOutputType;

                    break;
            }
            _motorView.SoundOutput = soundOutputType;
            // ポルターガイストの設定
            var set = settings;
            // 壁掛けオブジェクト用アニメーションSO
            PoltergeistAnimationSO poltergeistAnimationSO = set.poltergeistAnimationSO;
            if (poltergeistAnimationSO != null)
            {
                _motorView.PoltergeistAnimationSO = poltergeistAnimationSO;
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }
            // オバケの攻撃開始範囲の半径は振動を開始する最長距離の値を使用する
            var startAttackCollider = startAttackInstance.GetComponent<SphereCollider>();
            startAttackCollider.radius = _motorView.MaxDistance;
            
            if (shoutChanceInstance != null)
            {
                var shoutChanceCollider = shoutChanceInstance.GetComponent<SphereCollider>();
                shoutChanceCollider.radius = ghostInStaticObjectStruct.customShoutRadius > 0f 
                    ? ghostInStaticObjectStruct.customShoutRadius 
                    : poltergeistTable.subSettings.defaultShoutRadius;
            }

            _poltergeistViewModel = new PoltergeistViewModel(poltergeistTable, startAttackInstance);
            // ポルターガイストのビューモデル
            var viewModel = _poltergeistViewModel;
            // オバケの攻撃タイプ
            ReactiveProperty<GhostAttackType> ghostAttackType = new ReactiveProperty<GhostAttackType>();
            // リストに追加される度にリストへ追加された要素のゴーストIDを見るのはコンポーネントが持つ要素と同じでは？
            // 各コンポーネントのStartイベントにて要素をセットする。ViewModelを経由してModel内にリストを持っておいてそれにAddする。
            // 配列が変更される度に、その要素がゴーストIDと一致するなら、その情報を元のコンポーネントのStructへも反映する
            Observable.EveryUpdate()
                .Select(_ => _poltergeistViewModel.GhostInStaticObjectStructs)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.ObserveReplace()
                        // poltergeistViewIDが一致する場合
                        .Where(x => x.NewValue.poltergeistViewID == ghostInStaticObjectStruct.poltergeistViewID)
                        .Subscribe(x =>
                        {
                            // ghostInStaticObjectStructの内容を変更された内容で更新する
                            ghostInStaticObjectStruct.ghostTeamID = x.NewValue.ghostTeamID;
                            ghostInStaticObjectStruct.useStatus = x.NewValue.useStatus;
                            ghostInStaticObjectStruct.membersCount = x.NewValue.membersCount;
                            ghostInStaticObjectStruct.attackType = x.NewValue.attackType;
                            ghostInStaticObjectStruct.moveType = x.NewValue.moveType;
                            ghostInStaticObjectStruct.customShoutRadius = x.NewValue.customShoutRadius;
                            ghostInStaticObjectStruct.soundOutputType = x.NewValue.soundOutputType;
                            ghostInStaticObjectStruct.role = x.NewValue.role;
                            ghostInStaticObjectStruct.ghostModelType = x.NewValue.ghostModelType;
                            ghostInStaticObjectStruct.ghostVoiceType = x.NewValue.ghostVoiceType;
                            var soundOutputType = ghostInStaticObjectStruct.soundOutputType;
                            switch (soundOutputType)
                            {
                                case SoundOutputType.TableDefault:
                                    soundOutputType = poltergeistTable.subSettings.defaultSoundOutputType;

                                    break;
                            }
                            ghostAttackType.Value = ghostInStaticObjectStruct.attackType;
                            _motorView.SoundOutput = ghostInStaticObjectStruct.soundOutputType;
                            // ghostTeamIDが空なら、motorInstanceへポルターガイストを無効に更新する
                            // 空でないなら、有効に更新する
                            _motorView.IsEnabledPoltergeist = !string.IsNullOrEmpty(x.NewValue.ghostTeamID.Value);
                            _motorView.DustParticlePosition = FindOrInstanceGameObject("DustParticlePosition");

                            StopPatrolTimer();
                            StopSoundOutputBehavior();
                            if (x.NewValue.useStatus == UseStatus.Using)
                            {
                                if (x.NewValue.moveType == MoveType.Patrol)
                                {
                                    StartPatrolTimer();
                                }
                            }
                        })
                        .AddTo(ref _disposableBag);
                    // オブジェクトIDを割り振る
                    var instanceID = GetInstanceID();
                    ghostInStaticObjectStruct.poltergeistViewID = instanceID;
                    // 中ボスオバケの家具入居管理のデータクラス
                    var midBossGhostInStaticObjectStruct = set.midBossGhostInStaticObjectStruct;
                    midBossGhostInStaticObjectStruct.poltergeistViewID = instanceID;
                    switch (ghostInStaticObjectStruct.useStatus)
                    {
                        case UseStatus.Using:
                            // 使用中ならIDを割り振る
                            ghostInStaticObjectStruct.ghostTeamID = new ReactiveProperty<string>();
                            ghostInStaticObjectStruct.ghostTeamID.Value = System.Guid.NewGuid().ToString();
                            _motorView.IsEnabledPoltergeist = true;
                            _motorView.DustParticlePosition = FindOrInstanceGameObject("DustParticlePosition");

                            break;
                        default:
                            ghostInStaticObjectStruct.ghostTeamID = new ReactiveProperty<string>();
                            ghostInStaticObjectStruct.ghostTeamID.Value = string.Empty;

                            break;
                    }
                    switch (midBossGhostInStaticObjectStruct.useStatus)
                    {
                        case UseStatus.Using:
                            // 使用中ならIDを割り振る
                            // オバケ団体IDは基本的に使っていないのでとりあえず値が入っていれば何でもいい
                            midBossGhostInStaticObjectStruct.ghostTeamID = new ReactiveProperty<string>();
                            midBossGhostInStaticObjectStruct.ghostTeamID.Value = System.Guid.NewGuid().ToString();

                            break;
                        default:
                            midBossGhostInStaticObjectStruct.ghostTeamID = new ReactiveProperty<string>();
                            midBossGhostInStaticObjectStruct.ghostTeamID.Value = string.Empty;

                            break;
                    }
                    _poltergeistViewModel.AddGhostInStaticObjectStructs(ghostInStaticObjectStruct);
                    ghostAttackType.Value = ghostInStaticObjectStruct.attackType;
                })
                .AddTo(ref _disposableBag);
            foreach (Transform child in _transform)
            {
                if (child.name.Equals("MissGhostEscapePosition"))
                {
                    _missGhostEscapePosition = child.position;
                    _missGhostEscapeEulerAngles = child.eulerAngles;
                    break;
                }
            }
            
            if (ghostInStaticObjectStruct.useStatus == UseStatus.Using)
            {
                StartSoundOutputBehavior(soundOutputType);
            }

            // パート切り替え時にもタイマーを制御
            _poltergeistViewModel.InteractionPartReactive.Subscribe(part => 
            {
                if (part == InteractionPart.Search || part == InteractionPart.ShoutChance) 
                {
                    if (ghostInStaticObjectStruct.moveType == MoveType.Patrol && ghostInStaticObjectStruct.useStatus == UseStatus.Using)
                    {
                        // 該当パートに戻ったらタイマー再開
                        StartPatrolTimer();
                    }
                }
                else if (part == InteractionPart.Rhythm)
                {
                    // リズムパート時は一時停止
                    PausePatrolTimer();
                    StopSoundOutputBehavior();
                }
            }).AddTo(ref _disposableBag);
            _poltergeistViewModel.InteractionPartReactive.Pairwise()
                .Subscribe(part =>
                {
                    var prev = part.Previous;
                    var current = part.Current;
                    if (prev == InteractionPart.Rhythm &&
                        current != InteractionPart.Rhythm)
                    {
                        StartSoundOutputBehavior(soundOutputType);
                    }
                })
                .AddTo(ref _disposableBag);
            // オバケ移動演出の再生完了を監視する
            System.IDisposable playMoveGhostDirectionDisposable = null;
            // リズムパートが終了⇒フェードインアウト完了⇒家具とプレイヤーがお互い向き合っている状態を監視
            _poltergeistViewModel.IsPostRhythmFaceOff.Where(x => x &&
                // 直前にオバケが隠れていたかの判定をつけて全ての家具が対象にならないようにする
                _poltergeistViewModel.MoveTargetGhostDirection != null)
                .Select(_ => _poltergeistViewModel.MoveTargetGhostDirection)
                .Subscribe(ghostInStaticObjectStruct =>
                {
                    // 移動用オバケプレハブ（生成済み）
                    Transform instanceMissGhostEscape = null;
                    var modelType = ghostInStaticObjectStruct.ghostModelType;
                    var escapePrefab = GetMissGhostEscapePrefab(modelType);
                    var voiceType = ghostInStaticObjectStruct.ghostVoiceType;
                    var direction = PlayMoveGhostDirection(escapePrefab, _missGhostEscapePosition, _missGhostEscapeEulerAngles, _transform, _script_XyloApi, voiceType,
                        _poltergeistViewModel, instanceMissGhostEscape);
                    playMoveGhostDirectionDisposable = direction.Take(1)
                        .Subscribe(_ =>
                        {
                            _poltergeistViewModel.SetTargetGhost(null);
                            _poltergeistViewModel.SetIsCompletedMoveGhostDirection(true);
                            _poltergeistViewModel.SetMoveTargetGhostDirection(null);
                        })
                        .AddTo(ref _disposableBag);
                    // リズムパートへ移行した際に実行中なら中断する（再び呼ばれることがあった場合は最初から再生）
                    _poltergeistViewModel.InteractionPartReactive.Where(x => x.Equals(InteractionPart.Rhythm))
                        .Take(1)
                        .Subscribe(_ =>
                        {
                            if (instanceMissGhostEscape != null &&
                                instanceMissGhostEscape.gameObject.activeSelf)
                                instanceMissGhostEscape.gameObject.SetActive(false);
                            playMoveGhostDirectionDisposable.Dispose();
                            _poltergeistViewModel.SetTargetGhost(null);
                            _poltergeistViewModel.SetMoveTargetGhostDirection(null);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // オバケの攻撃タイプを監視する
            ghostAttackType.Subscribe(attackType =>
            {
                switch (attackType)
                {
                    case GhostAttackType.None:
                        startAttackCollider.enabled = false;

                        break;
                    case GhostAttackType.ThrowBookInstance:
                        startAttackCollider.enabled = true;

                        break;
                    case GhostAttackType.ThrowBookNotInstance:
                        startAttackCollider.enabled = true;

                        break;
                }
            })
                .AddTo(ref _disposableBag);
            // オバケ弾のインスタンス
            Dictionary<GhostAttackType, GhostBulletAbstractView> ghostBulletsInstance = new Dictionary<GhostAttackType, GhostBulletAbstractView>();
            // オバケタレットの位置
            Vector3 ghostTurretPosition = Vector3.zero;
            // オバケタレットの角度
            Vector3 ghostTurretEulerAngles = Vector3.zero;
            foreach (Transform child in trans)
            {
                if (child.name.Equals("GhostTurretPosition"))
                {
                    ghostTurretPosition = child.position;
                    ghostTurretEulerAngles = child.eulerAngles;
                    break;
                }
            }
            // オバケ攻撃予備動作の停止位置
            Vector3 ghostBulletWaitPosition = Vector3.zero;
            Transform ghostBulletWaitPositionTrans = null;
            foreach (Transform child in trans)
            {
                if (child.name.Equals("GhostBulletWaitPosition"))
                {
                    ghostBulletWaitPosition = child.position;
                    ghostBulletWaitPositionTrans = child;
                    break;
                }
            }
            // オバケの攻撃タイプの場合、攻撃開始状態を監視
            var ghostAttack = poltergeistTable.subSettings.ghostAttack;
            _poltergeistViewModel.IsStartAttack
                .Subscribe(player =>
                {
                    var type = ghostAttackType.Value;
                    switch (type)
                    {
                        case GhostAttackType.ThrowBookInstance:
                            DoGhostAttackThrowBookInstance(ghostTurretPosition, ghostTurretEulerAngles, ghostBulletWaitPosition, player, ignorePhysicsGhostBullets,
                                ghostAttack.ghostBulletBookPrefab,
                                ghostBulletsInstance).Take(1)
                                .Subscribe(_ =>
                                {
                                    _poltergeistViewModel.SetIsStartAttack(null);
                                })
                                .AddTo(ref _disposableBag);

                            break;
                        case GhostAttackType.ThrowBookNotInstance:
                            var ghostBulletBookInstance = set.ghostAttack.ghostBulletBookInstance;
                            if (ghostBulletBookInstance != null)
                            {
                                ghostBulletsInstance[type] = ghostBulletBookInstance.GetComponent<GhostBulletBookView>();
                                DoGhostAttackThrowBookNotInstance(ghostBulletsInstance, ghostBulletWaitPosition, player, ignorePhysicsGhostBullets).Take(1)
                                    .Subscribe(_ =>
                                    {
                                        _poltergeistViewModel.SetIsStartAttack(null);
                                    })
                                    .AddTo(ref _disposableBag);
                            }
                            else
                            {
                                // 前提：攻撃オバケも家具を移動する対象であること
                                // 攻撃オバケかつ、生成なし設定の場合、移動先によって対象のオブジェクトがNULLになるため
                                // 逃げの対策として、動的生成のパターンへ切り替える
                                DoGhostAttackThrowBookInstance(ghostTurretPosition, ghostTurretEulerAngles, ghostBulletWaitPosition, player, ignorePhysicsGhostBullets,
                                    ghostAttack.ghostBulletBookPrefab,
                                    ghostBulletsInstance).Take(1)
                                    .Subscribe(_ =>
                                    {
                                        _poltergeistViewModel.SetIsStartAttack(null);
                                    })
                                    .AddTo(ref _disposableBag);
                            }

                            break;
                    }
                })
                .AddTo(ref _disposableBag);
            // 敵戦パートの切替を監視する
            viewModel.EnemyBattlePartReactive.Subscribe(enemyBattlePart =>
            {
                switch (enemyBattlePart)
                {
                    case EnemyBattlePart.MidBoss:
                        viewModel.ReplaceGhostInStaticObjectStructs(set.midBossGhostInStaticObjectStruct);

                        break;
                }
            })
                .AddTo(ref _disposableBag);
            _script_XyloApi = new Script_xyloApi();
            _fadeImageView = FindAnyObjectByType<FadeImageView>();
            _homingObjectPoolerCustomizeView = FindAnyObjectByType<HomingObjectPoolerCustomizeView>();
            _objectPoolerXyloOtherCustomizeView = FindAnyObjectByType<ObjectPoolerXyloOtherCustomizeView>();
        }

        /// <summary>
        /// オバケ攻撃を実行
        /// </summary>
        /// <param name="ghostTurretPosition">オバケタレットの位置</param>
        /// <param name="ghostTurretEulerAngles">オバケタレットの角度</param>
        /// <param name="ghostBulletWaitPosition">オバケ攻撃予備動作の停止位置</param>
        /// <param name="player">追尾の対象</param>
        /// <param name="ignorePhysicsGhostBullets">弾との衝突対象外コライダー情報</param>
        /// <param name="ghostBulletBookPrefab">オバケ弾</param>
        /// <param name="ghostBulletsInstance">オバケ弾のインスタンス</param>
        /// <returns>オブザーバブル</returns>
        /// <remarks>スロー（本）動的生成モード</remarks>
        private Observable<Unit> DoGhostAttackThrowBookInstance(Vector3 ghostTurretPosition, Vector3 ghostTurretEulerAngles, Vector3 ghostBulletWaitPosition, Transform player, List<int> ignorePhysicsGhostBullets,
            Transform ghostBulletBookPrefab,
            Dictionary<GhostAttackType, GhostBulletAbstractView> ghostBulletsInstance)
        {
            return Observable.Create<Unit>(observer =>
            {
                var instance = Instantiate(ghostBulletBookPrefab, ghostTurretPosition, Quaternion.identity);
                instance.eulerAngles = ghostTurretEulerAngles;
                var ghostBulletBookView = instance.GetComponent<Views.GhostBulletBookView>();
                ghostBulletsInstance[GhostAttackType.ThrowBookInstance] = ghostBulletBookView;
                ghostBulletBookView.SetBulletWaitPosition(ghostBulletWaitPosition);
                ghostBulletBookView.SetTarget(player);
                ghostBulletBookView.SetIgnorePhysicsGhostBullets(ignorePhysicsGhostBullets);
                ghostBulletBookView.IsCompletedMoveToTarget.Where(x => x)
                    .Take(1)
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
        /// オバケ攻撃を実行
        /// </summary>
        /// <param name="ghostBulletsInstance">オバケ弾のインスタンス</param>
        /// <param name="ghostBulletWaitPosition">オバケ攻撃予備動作の停止位置</param>
        /// <param name="player">追尾の対象</param>
        /// <param name="ignorePhysicsGhostBullets">弾との衝突対象外コライダー情報</param>
        /// <returns>オブザーバブル</returns>
        /// <remarks>スロー（本）生成なしモード</remarks>
        private Observable<Unit> DoGhostAttackThrowBookNotInstance(Dictionary<GhostAttackType, GhostBulletAbstractView> ghostBulletsInstance, Vector3 ghostBulletWaitPosition, Transform player, List<int> ignorePhysicsGhostBullets)
        {
            return Observable.Create<Unit>(observer =>
            {
                var ghostBulletBook = ghostBulletsInstance[GhostAttackType.ThrowBookNotInstance];
                var ghostBulletBookView = (GhostBulletBookView)ghostBulletBook;
                ghostBulletBookView.SetBulletWaitPosition(ghostBulletWaitPosition);
                ghostBulletBookView.SetTarget(player);
                ghostBulletBookView.SetIgnorePhysicsGhostBullets(ignorePhysicsGhostBullets);
                // 生成なしの場合はイベント依存不可のため明示的に攻撃メソッドを呼び出す
                ghostBulletBookView.DoMoveToTarget();
                ghostBulletBookView.IsCompletedMoveToTarget.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                    .AddTo(ref _disposableBag);

                return Disposable.Empty;
            });
        }

        private void OnDestroy()
        {
            StopPatrolTimer();
            _disposableBag.Dispose();
            _script_XyloApi?.Dispose();
            _poltergeistViewModel?.Dispose();
        }

        /// <summary>
        /// スピードオバケ用のパトロール（引っ越し）タイマーを開始する
        /// </summary>
        private void StartPatrolTimer()
        {
            _patrolTimerDisposable?.Dispose();

            // Inspectorで設定した秒数を取得（例: 30秒）
            float interval = poltergeistTable.subSettings.moveIntervalSeconds;

            // 初回またはリセット後のみ満タンにする
            if (_patrolRemainingTime < 0f)
            {
                _patrolRemainingTime = interval;
            }

#if UNITY_EDITOR
            DebugPatrolInterval = interval;
#endif

            _patrolTimerDisposable = Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    _patrolRemainingTime -= Time.deltaTime;

                    if (_patrolRemainingTime <= 0f)
                    {
                        _patrolTimerDisposable?.Dispose();
                        _patrolRemainingTime = -1f; // 次回のStartでリセットされるようにする
                        
                        // ターゲットを自分に設定し、演出対象であることをViewModelに通知
                        _poltergeistViewModel.SetTargetGhost(_transform);
                        _poltergeistViewModel.SetMoveTargetGhostDirection(ghostInStaticObjectStruct);

                        // 逃げる演出を呼び出し
                        Transform instanceMissGhostEscape = null;
                        var modelType = ghostInStaticObjectStruct.ghostModelType;
                        var escapePrefab = GetMissGhostEscapePrefab(modelType);
                        var voiceType = ghostInStaticObjectStruct.ghostVoiceType;
                        var direction = PlayMoveGhostDirection(
                            escapePrefab, 
                            _missGhostEscapePosition, 
                            _missGhostEscapeEulerAngles, 
                            _transform, 
                            _script_XyloApi,
                            voiceType,
                            _poltergeistViewModel, 
                            instanceMissGhostEscape
                        );

                        direction.Take(1).Subscribe(__ =>
                        {
                            // 演出完了後
                            _poltergeistViewModel.SetTargetGhost(null);
                            _poltergeistViewModel.SetIsCompletedMoveGhostDirection(true);
                            _poltergeistViewModel.SetMoveTargetGhostDirection(null);
                            
                            // 実際の引っ越し処理を実行
                            ShuffleNewStaticObject();
                        }).AddTo(ref _disposableBag);
                    }
                }).AddTo(ref _disposableBag);
        }

        /// <summary>
        /// スピードオバケ用のパトロール（引っ越し）タイマーを一時停止する
        /// </summary>
        private void PausePatrolTimer()
        {
            _patrolTimerDisposable?.Dispose();
        }

        /// <summary>
        /// スピードオバケ用のパトロール（引っ越し）タイマーを停止・リセットする
        /// </summary>
        private void StopPatrolTimer()
        {
            _patrolTimerDisposable?.Dispose();
            _patrolRemainingTime = -1f;
        }

        /// <summary>
        /// 対象オブジェクトを子から探索して該当しない場合はゲームオブジェクトを生成
        /// </summary>
        /// <param name="name">ゲームオブジェクト名</param>
        private Transform FindOrInstanceGameObject(string name)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals(name))
                {
                    return child;
                }
            }
            switch (name)
            {
                case "RhythmPartPosition":
                    var newObj = new GameObject(name).AddComponent<RhythmPartPositionView>();
                    newObj.transform.position = transform.position;
                    newObj.transform.SetParent(transform);

                    return newObj.transform;
                case "RhythmPartPosition_1":
                    var newObj1 = new GameObject(name).AddComponent<RhythmPartPosition_1View>();
                    newObj1.transform.position = transform.position;
                    newObj1.transform.SetParent(transform);

                    return newObj1.transform;
                default:
                    var newObj2 = new GameObject(name);
                    newObj2.transform.position = transform.position;
                    newObj2.transform.SetParent(transform);

                    return newObj2.transform;
            }
        }

        /// <summary>
        /// オバケの家具入居管理の構造体の更新トランザクション開始
        /// </summary>
        /// <remarks>PlayerViewから呼び出される<br/>
        /// ViewModel経由でTransactionGhostInStaticObjectStructをセット<br/>
        /// ●利用人数の更新を監視<br/>
        /// ＿○0になったらリズムパートを終了<br/>
        /// ＿＿・ViewModel経由でコミット<br/>
        /// ●BGMの終了を監視<br/>
        /// ＿○BGMが終了したらリズムパートを終了<br/>
        /// ＿＿・ViewModel経由でコミット<br/>
        /// ●HPの減少（リズムパート失敗）を監視<br/>
        /// ＿○HPが減少したらリズムパートを終了<br/>
        /// ＿＿・ViewModel経由でコミット
        /// </remarks>
        public void BeginTransactionGhostInStaticObjectStruct()
        {
            var viewModel = _poltergeistViewModel;
            switch (viewModel.EnemyBattlePart)
            {
                case EnemyBattlePart.Normal:
                    _script_XyloApi.ChangeBgmB();

                    break;
                case EnemyBattlePart.MidBoss:
                    _script_XyloApi.ChangeBgmC();

                    break;
                case EnemyBattlePart.Tutorial:
                    _script_XyloApi.ChangeBgmB();

                    break;
            }
            var subSettings = poltergeistTable.subSettings;
            List<System.IDisposable> disposables = new List<System.IDisposable>();
            disposables.Add(
                _poltergeistViewModel.IsCompletedDirection.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        _script_XyloApi.ChangeBgmA();
                        CommitTransactionGhostInStaticObjectStruct(_poltergeistViewModel);
                        // 後処理
                        // 各処理をObservableに変換して、全て完了したら次に進む
                        List<Observable<bool>> completionObservables = new List<Observable<bool>>();

                        // 1. 暗幕フェード処理（条件付き）
                        var ghostStructs = _poltergeistViewModel.GhostInStaticObjectStructs;
                        var cnt = ghostStructs.Where(q => q.role.Equals(GhostRole.Normal))
                            .Select(q => q.membersCount).Sum();
                        var midBosskillsRate = viewModel.MidBosskillsRate;
                        var healthPoint = _poltergeistViewModel.PlayerHealthPoint.Value;
                        // ポルターガイストの設定
                        var set = settings;
                        // 壁掛けオブジェクト用アニメーションSO
                        PoltergeistAnimationSO poltergeistAnimationSO = set.poltergeistAnimationSO;
                        switch (viewModel.EnemyBattlePart)
                        {
                            case EnemyBattlePart.Normal:
                                if (0 < cnt && 0 < healthPoint)
                                {
                                    completionObservables.Add(
                                        Observable.Create<bool>(observer =>
                                        {
                                            StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                                            return Disposable.Empty;
                                        })
                                        .Do(_ =>
                                        {
                                            _motorView?.DoStopFloaterAnimation();
                                            ResetMovePosition(_initialPosition, _initialEulerAngles, _noTriggerColliders, _rigidbody, poltergeistAnimationSO);
                                        })
                                    );
                                }
                                else if (cnt < 1 && 0 < healthPoint)
                                {
                                    if (viewModel.CheckClearAndUpdateEnemyBattlePart())
                                    {
                                        completionObservables.Add(
                                            Observable.Create<bool>(observer =>
                                            {
                                                StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                                                return Disposable.Empty;
                                            })
                                            .Do(_ =>
                                            {
                                                _motorView?.DoStopFloaterAnimation();
                                                ResetMovePosition(_initialPosition, _initialEulerAngles, _noTriggerColliders, _rigidbody, poltergeistAnimationSO);
                                                _poltergeistViewModel.SetIsMissionClear(true);
                                            })
                                        );
                                    }
                                    else
                                    {
                                        completionObservables.Add(
                                            Observable.Create<bool>(observer =>
                                            {
                                                StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                                                return Disposable.Empty;
                                            })
                                            .Do(_ =>
                                            {
                                                _motorView?.DoStopFloaterAnimation();
                                                ResetMovePosition(_initialPosition, _initialEulerAngles, _noTriggerColliders, _rigidbody, poltergeistAnimationSO);
                                            })
                                        );
                                    }
                                }
                                else
                                {
                                    // 条件を満たさない場合は即座に完了するObservableを追加（デッドロジック）
                                    completionObservables.Add(Observable.Return(true));
                                }

                                break;
                            case EnemyBattlePart.MidBoss:
                                if (midBosskillsRate < subSettings.targetkillsRate && 0 < healthPoint)
                                {
                                    completionObservables.Add(
                                        Observable.Create<bool>(observer =>
                                        {
                                            StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                                            return Disposable.Empty;
                                        })
                                        .Do(_ =>
                                        {
                                            _motorView?.DoStopFloaterAnimation();
                                            ResetMovePosition(_initialPosition, _initialEulerAngles, _noTriggerColliders, _rigidbody, poltergeistAnimationSO);
                                        })
                                    );
                                }
                                else if (subSettings.targetkillsRate <= midBosskillsRate && 0 < healthPoint)
                                {
                                    completionObservables.Add(
                                        Observable.Create<bool>(observer =>
                                        {
                                            StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                                            return Disposable.Empty;
                                        })
                                        .Do(_ =>
                                        {
                                            _motorView?.DoStopFloaterAnimation();
                                            ResetMovePosition(_initialPosition, _initialEulerAngles, _noTriggerColliders, _rigidbody, poltergeistAnimationSO);
                                            _poltergeistViewModel.SetIsMissionClear(true);
                                        })
                                    );
                                }
                                else
                                {
                                    // 条件を満たさない場合は即座に完了するObservableを追加（デッドロジック）
                                    completionObservables.Add(Observable.Return(true));
                                }

                                break;
                            case EnemyBattlePart.Tutorial:
                                // チュートリアルパートでは人数・HPに関わらず常にフェードアウト→浮遊停止→リセットを実行
                                completionObservables.Add(
                                    Observable.Create<bool>(observer =>
                                    {
                                        StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                                        return Disposable.Empty;
                                    })
                                    .Do(_ =>
                                    {
                                        _motorView?.DoStopFloaterAnimation();
                                        ResetMovePosition(_initialPosition, _initialEulerAngles, _noTriggerColliders, _rigidbody, poltergeistAnimationSO);
                                    })
                                );

                                break;
                        }
                        viewModel.SetMidBosskillsRate(0f);

                        // 2. オバケが残っていたらプールへ戻す（Other）
                        completionObservables.Add(
                            Observable.Create<bool>(observer =>
                            {
                                StartCoroutine(_objectPoolerXyloOtherCustomizeView.AllDisabled(observer));
                                return Disposable.Empty;
                            })
                        );

                        // 3. スポナーの削除（同期的処理をObservableに変換）
                        completionObservables.Add(
                            Observable.Create<bool>(observer =>
                            {
                                _missileTempoSpawnerInstance.gameObject.SetActive(false);
                                _homingObjectPoolerCustomizeView.DoReturnAllMissilesToPool();
                                FindMissileTempoSpawnerInstanceAndDestroy(_missileTempoSpawnerInstance);
                                observer.OnNext(true);
                                observer.OnCompleted();
                                return Disposable.Empty;
                            })
                        );

                        // 全てのObservableが完了したら次に進む
                        // Observable.Createで全てのObservableの完了を待つ
                        disposables.Add(
                            Observable.Create<bool>(observer =>
                            {
                                if (completionObservables.Count == 0)
                                {
                                    observer.OnNext(true);
                                    observer.OnCompleted();
                                    return Disposable.Empty;
                                }

                                int completedCount = 0;
                                int totalCount = completionObservables.Count;
                                List<System.IDisposable> innerDisposables = new List<System.IDisposable>();

                                foreach (var obs in completionObservables)
                                {
                                    innerDisposables.Add(
                                        obs.Take(1)
                                            .Subscribe(_ =>
                                            {
                                                completedCount++;
                                                if (completedCount >= totalCount)
                                                {
                                                    observer.OnNext(true);
                                                    observer.OnCompleted();
                                                }
                                            })
                                    );
                                }

                                return Disposable.Create(() =>
                                {
                                    foreach (var d in innerDisposables)
                                        d?.Dispose();
                                });
                            })
                            .Take(1)
                            .Subscribe(_ =>
                            {
                                // 全て完了したらパート切り替え
                                _poltergeistViewModel.SetInteractionPartToSearch();
                                foreach (var disposable in disposables)
                                    disposable.Dispose();
                            })
                            .AddTo(ref _disposableBag)
                        );
                    })
                    .AddTo(ref _disposableBag)
            );
            disposables.Add(
                _script_XyloApi.BgmCStatus.DistinctUntilChanged()
                    .Where(x => x == 3 &&
                    viewModel.EnemyBattlePart.Equals(EnemyBattlePart.MidBoss))
                    .Subscribe(_ =>
                    {
                        var midBosskillsRate = viewModel.MidBosskillsRate;
                        if (subSettings.targetkillsRate <= midBosskillsRate)
                        {
                            _poltergeistViewModel.SetIsCompletedRhythmPart(1);
                        }
                        else
                        {
                            _poltergeistViewModel.SetIsCompletedRhythmPart(2);
                            _poltergeistViewModel.SetMoveTargetGhostDirection(ghostInStaticObjectStruct);
                        }
                    })
                    .AddTo(ref _disposableBag)
            );
            ReactiveCommand<int> membersCount = new ReactiveCommand<int>();
            disposables.Add(
                membersCount.Where(x => x < 1 &&
                    viewModel.EnemyBattlePart.Equals(EnemyBattlePart.Normal))
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        _poltergeistViewModel.SetIsCompletedRhythmPart(1);
                    })
                    .AddTo(ref _disposableBag)
            );
            disposables.Add(
                _poltergeistViewModel.IsBadEndRhythmPart.Where(x => x &&
                    viewModel.EnemyBattlePart.Equals(EnemyBattlePart.Normal))
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        _poltergeistViewModel.SetIsCompletedRhythmPart(2);
                        _poltergeistViewModel.SetMoveTargetGhostDirection(ghostInStaticObjectStruct);
                    })
                    .AddTo(ref _disposableBag)
            );
            _poltergeistViewModel.SetTransactionGhostInStaticObjectStruct(ghostInStaticObjectStruct);
            disposables.Add(
                Observable.EveryUpdate()
                    .Select(_ => _poltergeistViewModel.TransactionGhostInStaticObjectStruct)
                    .Subscribe(transactionGhostInStaticObjectStruct =>
                    {
                        membersCount.Execute(transactionGhostInStaticObjectStruct.membersCount);
                    })
                    .AddTo(ref _disposableBag)
            );
        }

        /// <summary>
        /// オバケの家具入居管理の構造体の更新トランザクションをコミット
        /// </summary>
        /// <param name="viewModel">ビューモデル</param>
        private void CommitTransactionGhostInStaticObjectStruct(PoltergeistViewModel viewModel)
        {
            var transactionGhostStruct = viewModel.TransactionGhostInStaticObjectStruct;
            var enemyBattlePart = viewModel.EnemyBattlePart;
            var subSettings = poltergeistTable.subSettings;
            switch (enemyBattlePart)
            {
                case EnemyBattlePart.Normal:
                    if (transactionGhostStruct.membersCount < 1)
                    {
                        ExitGhost();
                    }
                    else
                    {
                        ShuffleNewStaticObject();
                    }

                    break;
                case EnemyBattlePart.MidBoss:
                    float midBosskillsRate = viewModel.MidBosskillsRate;
                    if (subSettings.targetkillsRate <= midBosskillsRate)
                    {
                        ExitGhost();
                    }
                    else
                    {
                        ShuffleNewStaticObject();
                    }

                    break;
                case EnemyBattlePart.Tutorial:
                    // チュートリアルパートでは強制的に空室化する
                    ExitGhost();

                    break;
            }
            viewModel.SetDefaultTransactionGhostInStaticObjectStruct();
        }

        /// <summary>
        /// オバケの引っ越し
        /// </summary>
        public void ShuffleNewStaticObject()
        {
            _poltergeistViewModel.ShuffleNewStaticObject(ghostInStaticObjectStruct);
        }

        /// <summary>
        /// 拠点を空室にする
        /// </summary>
        public void ExitGhost()
        {
            _poltergeistViewModel.ResetStaticObject(ghostInStaticObjectStruct);
        }

        /// <summary>
        /// ゴーストを飛び出させる処理を実行
        /// </summary>
        public async void AsyncDoBurstGhosts()
        {
            await Task.Delay(0);
            if (_poltergeistViewModel != null)
                _poltergeistViewModel.SetIsCompletedBurstGhosts(true);
        }

        /// <summary>
        /// ミサイルテンポスポナーを生成
        /// </summary>
        public void InstanceMissileTempoSpawner()
        {
            var originParent = _transform.parent;
            var missileTempoSpawnerInstance = Instantiate(poltergeistTable.missileTempoSpawnerPrefab, _rhythmPartPosition_1, Quaternion.identity);
            missileTempoSpawnerInstance.transform.SetParent(originParent);
            // リズムパートの調整（家具に対して真正面に配置するとオバケがずれることがある？）
            var originAngles = _rhythmPartEulerAngles_1;
            missileTempoSpawnerInstance.eulerAngles = new Vector3(0f, originAngles.y - 125.09f, 0f);
            _missileTempoSpawnerInstance = missileTempoSpawnerInstance;
        }

        /// <summary>
        /// リズムパート位置へ移動する処理
        /// </summary>
        public void MovePosition()
        {
            // ポルターガイストの設定
            var set = settings;
            // 壁掛けオブジェクト用アニメーションSO
            PoltergeistAnimationSO poltergeistAnimationSO = set.poltergeistAnimationSO;
            SetRigidbodyStatus(_rigidbody, false, poltergeistAnimationSO);
            SetNoTriggerColliders(_noTriggerColliders, false);
            _transform.position = _rhythmPartPosition_1;
            _transform.eulerAngles = _rhythmPartEulerAngles_1;
        }

        /// <summary>
        /// リズムパート位置への移動アニメーションを再生
        /// </summary>
        /// <param name="observer">オブザーバー</param>
        /// <param name="playerTransform">プレイヤーのトランスフォーム</param>
        /// <returns>コルーチン</returns>
        public IEnumerator PlayMovePositionAnimation(Observer<bool> observer, Transform playerTransform)
        {
            // ポルターガイストの設定
            var set = settings;
            // 壁掛けオブジェクト用アニメーションSO
            PoltergeistAnimationSO poltergeistAnimationSO = set.poltergeistAnimationSO;
            SetRigidbodyStatus(_rigidbody, false, poltergeistAnimationSO);
            SetNoTriggerColliders(_noTriggerColliders, false);

            Vector3 targetPosition = _rhythmPartPosition_1;
            Vector3 start = _transform.position;
            Vector3 end = targetPosition;

            // プレイヤーとの距離をチェック
            float avoidRadius = 15.0f; // プレイヤーとの距離がこれ以下なら回避
            Vector3 playerPosition = playerTransform.position;

            List<Vector3> path = new List<Vector3>();
            var sequence = DOTween.Sequence();
            if (Vector3.Distance(playerPosition, end) < avoidRadius)
            {
                // プレイヤーを避けるための方向を計算
                Vector3 toTarget = (end - start).normalized;
                Vector3 toPlayer = (playerPosition - start).normalized;
                Vector3 avoidDir = Vector3.Cross(toTarget, Vector3.up).normalized; // Y軸を基準に外側へ回避
                // プレイヤーが右にいれば、avoidDirを反転させる（左回避にする）
                float dot = Vector3.Dot(avoidDir, toPlayer);
                if (dot > 0)
                {
                    avoidDir *= -1f;
                }
                // 中継点の追加（少し外側に避ける）
                Vector3 midPoint = (start + end) * 0.5f + avoidDir * 1.0f;
                path.Add(start);
                path.Add(midPoint);
                path.Add(end);

                sequence
                    .Append(_transform.DOPath(path.ToArray(), 1.2f, PathType.CatmullRom).SetEase(Ease.InOutSine))
                    .Join(_transform.DOLocalRotate(new Vector3(0, 1080f, 0), 1.2f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine))
                    .AppendCallback(() =>
                    {
                        _transform.eulerAngles = _rhythmPartEulerAngles_1; // 最終角度に調整
                        observer.OnNext(true);
                        observer.OnCompleted();
                    });
            }
            else
            {
                // そのまま移動
                DOTween.Sequence()
                    .Append(_transform.DOMove(end, 1f))
                    .Join(_transform.DOLocalRotate(new Vector3(0, 1080f, 0), 1.2f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine))
                    .AppendCallback(() =>
                    {
                        _transform.eulerAngles = _rhythmPartEulerAngles_1; // 最終角度に調整
                        observer.OnNext(true);
                        observer.OnCompleted();
                    });
            }

            yield return null;
        }

        /// <summary>
        /// 浮かせるアニメーション処理を呼び出す
        /// </summary>
        /// <returns>コルーチン</returns>
        public IEnumerator DoPlayFloaterAnimation()
        {
            if (_motorView != null)
            {
                StartCoroutine(_motorView.DoPlayFloaterAnimation());
            }
            
            yield return null;
        }

        /// <summary>
        /// リズムパート終了時にDestroy
        /// </summary>
        /// <param name="missileTempoSpawnerInstance">ミサイルテンポスポナー</param>
        private void FindMissileTempoSpawnerInstanceAndDestroy(Transform missileTempoSpawnerInstance)
        {
            if (missileTempoSpawnerInstance != null &&
                missileTempoSpawnerInstance.gameObject != null)
            {
                Destroy(missileTempoSpawnerInstance.gameObject);
            }
        }

        /// <summary>
        /// Rigidbodyの重力、物理シミュレーションを有効／無効切り替え
        /// </summary>
        /// <param name="rigidbody">Rigidbody</param>
        /// <param name="isEnabled">有効／無効</param>
        /// <param name="poltergeistAnimationSO">壁掛けオブジェクト用アニメーションSO</param>
        private void SetRigidbodyStatus(Rigidbody rigidbody, bool isEnabled, PoltergeistAnimationSO poltergeistAnimationSO)
        {
            if (rigidbody == null)
                return;

            // SO設定済み（壁掛け）の場合は常にKinematic維持（落下防止）
            if (poltergeistAnimationSO != null)
                return;

            if (isEnabled &&
                !rigidbody.useGravity &&
                rigidbody.isKinematic)
            {
                // 有効
                rigidbody.useGravity = true;
                rigidbody.isKinematic = false;
            }
            else if (!isEnabled &&
                rigidbody.useGravity &&
                !rigidbody.isKinematic)
            {
                // 無効
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
            }
        }

        /// <summary>
        /// コライダーを有効／無効切り替え
        /// </summary>
        /// <param name="noTriggerColliders">コライダーリスト</param>
        /// <param name="isEnabled">有効／無効</param>
        private void SetNoTriggerColliders(List<Collider> noTriggerColliders, bool isEnabled)
        {
            if (noTriggerColliders == null ||
                noTriggerColliders.Count < 1)
                return;

            if (isEnabled)
            {
                // 有効
                foreach (var noTriggerCollider in noTriggerColliders.Where(x => !x.enabled))
                {
                    noTriggerCollider.enabled = true;
                }
            }
            else
            {
                // 無効
                foreach (var noTriggerCollider in noTriggerColliders.Where(x => x.enabled))
                {
                    noTriggerCollider.enabled = false;
                }
            }
        }

        /// <summary>
        /// 元の位置へ移動させる処理
        /// </summary>
        /// <param name="initialPosition">初期ポジション</param>
        /// <param name="initialEulerAngles">初期オイラー角度</param>
        /// <param name="noTriggerColliders">コライダーリスト</param>
        /// <param name="rigidbody">Rigidbody</param>
        /// <param name="poltergeistAnimationSO">壁掛けオブジェクト用アニメーションSO</param>
        private void ResetMovePosition(Vector3 initialPosition, Vector3 initialEulerAngles, List<Collider> noTriggerColliders, Rigidbody rigidbody, PoltergeistAnimationSO poltergeistAnimationSO)
        {
            _transform.position = initialPosition;
            _transform.eulerAngles = initialEulerAngles;
            SetNoTriggerColliders(noTriggerColliders, true);
            SetRigidbodyStatus(rigidbody, true, poltergeistAnimationSO);
        }

        /// <summary>
        /// オバケ移動演出を再生
        /// </summary>
        /// <param name="missGhostEscapePrefab">移動用オバケプレハブ</param>
        /// <param name="missGhostEscapePosition">移動用オバケ生成位置</param>
        /// <param name="missGhostEscapeEulerAngles">移動用オバケ生成角度</param>
        /// <param name="trans">トランスフォーム</param>
        /// <param name="script_XyloApi">シロさんのコンポーネントへアクセスするAPI</param>
        /// <param name="ghostVoiceType">オバケボイスタイプ（SE差分）</param>
        /// <param name="poltergeistViewModel">ポルターガイストのビューモデル</param>
        /// <param name="instanceMissGhostEscape">移動用オバケプレハブ（生成済み）</param>
        /// <returns>オブザーバブル</returns>
        private Observable<Unit> PlayMoveGhostDirection(Transform missGhostEscapePrefab, Vector3 missGhostEscapePosition, Vector3 missGhostEscapeEulerAngles, Transform trans, Script_xyloApi script_XyloApi, GhostVoiceType ghostVoiceType,
            PoltergeistViewModel poltergeistViewModel, Transform instanceMissGhostEscape)
        {
            return Observable.Create<Unit>(observer =>
            {
                if (instanceMissGhostEscape == null)
                {
                    Transform instance = Instantiate(missGhostEscapePrefab, missGhostEscapePosition, Quaternion.identity).transform;
                    instance.eulerAngles = missGhostEscapeEulerAngles;
                    instance.SetParent(trans);
                    instanceMissGhostEscape = instance;
                }
                var missGhost = instanceMissGhostEscape;
                if (!missGhost.gameObject.activeSelf)
                    missGhost.gameObject.SetActive(true);
                var missGhostView = missGhost.GetComponent<MissGhostEscapeView>();
                // DOTweenでオバケが家具から出現して他の家具へ移動するアニメーションを追加
                missGhostView.IsEscapeCompleted.Where(x => x)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        // オバケ笑い声SE再生機能の追加（ボイスタイプ分岐）
                        script_XyloApi.PlayGhostLaughByVoiceType(ghostVoiceType);
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    })
                    .AddTo(ref _disposableBag);
                poltergeistViewModel.SetTargetGhost(missGhost);

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// 監視開始かつSE再生
        /// </summary>
        /// <param name="soundOutputType">音の出力タイプ</param>
        private void StartSoundOutputBehavior(SoundOutputType soundOutputType)
        {
            StopSoundOutputBehavior();

            var d = new CompositeDisposable();
            var laughSettings = poltergeistTable.subSettings.laughSettings;
            float shoutRadius = ghostInStaticObjectStruct.customShoutRadius > 0f 
                ? ghostInStaticObjectStruct.customShoutRadius 
                : poltergeistTable.subSettings.defaultShoutRadius;

            // ダウジング時の基本応答（ノリノリオバケ以外）
            if (soundOutputType != SoundOutputType.ReactiveShout_CallAndResponse)
            {
                if (_motorView != null)
                {
                    _motorView.OnActionAsObservable.Where(x => x)
                        .Subscribe(_ =>
                        {
                            PlayLaughSE(laughSettings);
                        })
                        .AddTo(d);
                }
            }

            if (soundOutputType == SoundOutputType.Loop || soundOutputType == SoundOutputType.ReactiveStatic)
            {
                // ★ 状態を初期化（範囲外リセット用）
                void ResetLaughSequence()
                {
                    _currentLaughPhase = LaughPhase.First;
                    _remainingRepeatsInPhase = laughSettings.firstStartCount;               // 初回フェーズ：1回のみ
                    _currentLaughInterval = laughSettings.firstInterval;
                }
                ResetLaughSequence();

                float timer = _currentLaughInterval;
                Vector3 lastPos = Vector3.zero;

                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        if (_motorView == null || _poltergeistViewModel?.PlayerTransform == null || !_motorView.IsEnabledPoltergeist)
                            return;

                        var playerPos = _poltergeistViewModel.PlayerTransform.position;
                        float dist = Vector3.Distance(_motorView.transform.position, playerPos);

                        if (dist <= shoutRadius)
                        {
                            // 静止時のみカウントダウンするか（ReactiveStatic 用）
                            bool canCountDown = true;
                            if (soundOutputType == SoundOutputType.ReactiveStatic)
                            {
                                if (Vector3.Distance(playerPos, lastPos) >= 0.01f)
                                    canCountDown = false; // 動いているとタイマー停止
                            }

                            if (canCountDown)
                            {
                                timer -= Time.deltaTime;
                                if (timer <= 0f)
                                {
                                    // SEを再生
                                    PlayLaughSE(laughSettings);

                                    // 次の再生までの時間と回数を設定
                                    if (_remainingRepeatsInPhase > 0)
                                        _remainingRepeatsInPhase--;

                                    // フェーズ遷移
                                    if (_remainingRepeatsInPhase == 0)
                                    {
                                        switch (_currentLaughPhase)
                                        {
                                            case LaughPhase.First:
                                                _currentLaughPhase = LaughPhase.Second;
                                                _remainingRepeatsInPhase = laughSettings.secondStartCount;      // 第二フェーズ：3回連続
                                                _currentLaughInterval = laughSettings.secondInterval;
                                                break;
                                            case LaughPhase.Second:
                                                _currentLaughPhase = LaughPhase.Final;
                                                _remainingRepeatsInPhase = -1;     // 最終フェーズ：無制限
                                                _currentLaughInterval = laughSettings.maxInterval;
                                                break;
                                            case LaughPhase.Final:
                                                // 最終フェーズは回数無制限なのでそのまま
                                                break;
                                        }
                                    }

                                    timer = _currentLaughInterval;
                                }
                            }
                        }
                        else
                        {
                            // 範囲外に出たら状態をリセット（次に近づいたとき初回から）
                            ResetLaughSequence();
                            timer = _currentLaughInterval;
                        }

                        lastPos = playerPos;
                    })
                    .AddTo(d);
            }
            else if (soundOutputType == SoundOutputType.ReactiveShout_CallAndResponse)
            {
                if (_motorView != null)
                {
                    _motorView.OnActionAsObservable.Where(x => x)
                        .ThrottleFirst(System.TimeSpan.FromSeconds(2f)) // 連続発動を防ぐ
                        .Subscribe(_ =>
                        {
                            // スクラッチ演出 (複数回連続で短く呼ぶ)
                            Observable.Timer(System.TimeSpan.Zero, System.TimeSpan.FromMilliseconds(150))
                                .Take(3)
                                .Subscribe(__ => PlayLaughSE(laughSettings))
                                .AddTo(d);
                        })
                        .AddTo(d);
                }
            }

            _soundOutputBehaviorDisposable = d;
        }

        /// <summary>
        /// 監視停止（SE再生用）
        /// </summary>
        private void StopSoundOutputBehavior()
        {
            _soundOutputBehaviorDisposable?.Dispose();
            _soundOutputBehaviorDisposable = null;
        }

        /// <summary>
        /// オバケの笑い声SEを再生（簡易3Dサウンド）
        /// </summary>
        /// <param name="laughSettings">オバケ笑い声の再生間隔・エコー設定</param>
        private void PlayLaughSE(PoltergeistTableSubSettings.PoltergeistLaughSettings laughSettings)
        {
            ObjectsPoolView objectsPoolView = ObjectsPoolView;
            Se_3D_PickerCustomizeView t3DSoundPlayer = objectsPoolView?.Get3DSoundPlayer();

            if (t3DSoundPlayer != null && _motorView != null && _motorView.IsEnabledPoltergeist && _poltergeistViewModel?.PlayerTransform != null)
            {
                Vector3 motorPos = _motorView.transform.position;
                float dist = Vector3.Distance(motorPos, _poltergeistViewModel.PlayerTransform.position);
                if (dist <= _motorView.MaxDistance)
                {
                    float intensity = Mathf.Clamp01(1f - (dist / _motorView.MaxDistance));
                    string seName = Script_xyloApi.GetGhostLaughSEName(ghostInStaticObjectStruct.ghostVoiceType);
                    t3DSoundPlayer.PlaySound(seName, intensity);
                }
            }
        }

        /// <summary>
        /// モデルタイプ別の逃走用オバケプレハブを取得する
        /// </summary>
        /// <param name="modelType">オバケモデルタイプ（FBX差分）</param>
        /// <returns>対応するプレハブ（マッピングに存在しない場合はデフォルトのmissGhostEscapePrefab）</returns>
        private Transform GetMissGhostEscapePrefab(GhostModelType modelType)
        {
            var mappedPrefab = poltergeistTable.GetPrefabByModelType(poltergeistTable.subSettings.ghostModelTypePrefabSettings.missGhostEscapePrefabMappings, modelType);
            return mappedPrefab != null ? mappedPrefab : poltergeistTable.missGhostEscapePrefab;
        }
    }

    /// <summary>
    /// ポルターガイストの設定
    /// </summary>
    [System.Serializable]
    public class PoltergeistSettings
    {
        /// <summary>オバケの攻撃タイプの設定</summary>
        public GhostAttack ghostAttack;
        [Tooltip("壁掛けオブジェクト用アニメーションSO。セットすると従来の物理揺らしに代わりDOTweenで再生する。")]
        public PoltergeistAnimationSO poltergeistAnimationSO;
        /// <summary>中ボスオバケの家具入居管理のデータクラス</summary>
        public GhostInStaticObjectStruct midBossGhostInStaticObjectStruct;

        /// <summary>
        /// オバケの攻撃タイプの設定
        /// </summary>
        [System.Serializable]
        public class GhostAttack
        {
            /// <summary>オバケ弾（本）</summary>
            /// <see cref="GhostAttackType.ThrowBookNotInstance"/>
            public Transform ghostBulletBookInstance;
        }
    }
}
