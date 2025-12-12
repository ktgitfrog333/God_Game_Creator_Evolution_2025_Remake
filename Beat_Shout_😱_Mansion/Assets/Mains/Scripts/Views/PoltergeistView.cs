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
using Unity.VisualScripting;
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
        [Tooltip("Assets/Mains/Prefabs/Level/Motor.prefabをセットしておく。")]
        [SerializeField] private GameObject motorPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/ShoutChanceRange.prefabをセットしておく。")]
        [SerializeField] private GameObject shoutChanceRangePrefab;
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
        [Tooltip("Assets/Mains/Prefabs/Effects/GhostBursts.prefabをセットしておく。")]
        [SerializeField] private GameObject ghostBurstsPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/DynamicObjects/MissileTempoSpawner.prefabをセットしておく。")]
        [SerializeField] private Transform missileTempoSpawnerPrefab;
        /// <summary>ミサイルテンポスポナー</summary>
        private Transform _missileTempoSpawnerInstance;
        /// <summary>オバケが飛び出すエフェクト</summary>
        private Transform _ghostBurstsInstance;
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

        private void Reset()
        {
            // リズムパートポジション（プレイヤー位置をリズムパート用に移動させる）の生成
            bool isFound = false;
            foreach (Transform child in transform)
            {
                if (child.name.Equals("RhythmPartPosition"))
                {
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                var newObj = new GameObject("RhythmPartPosition").AddComponent<RhythmPartPositionView>();
                newObj.transform.position = transform.position;
                newObj.transform.SetParent(transform);
            }
            // リズムパートポジション（家具をリズムパート用に移動させる）の生成
            bool isFound1 = false;
            foreach (Transform child in transform)
            {
                if (child.name.Equals("RhythmPartPosition_1"))
                {
                    isFound1 = true;
                    break;
                }
            }
            if (!isFound1)
            {
                var newObj = new GameObject("RhythmPartPosition_1").AddComponent<RhythmPartPosition_1View>();
                newObj.transform.position = transform.position;
                newObj.transform.SetParent(transform);
            }
        }

        private void Start()
        {
            _transform = transform;
            _initialPosition = _transform.position;
            _initialEulerAngles = _transform.eulerAngles;
            // Poltergeistの生成
            var originParent = _transform.parent;
            // 初期化
            var motorInstance = Instantiate(motorPrefab, _transform.position, Quaternion.identity);
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
            // ShoutChanceRangeの生成
            var originParent_1 = motorInstance.transform.parent;
            Transform shoutChanceInstance = Instantiate(shoutChanceRangePrefab, motorInstance.transform.position, Quaternion.identity).transform;
            // ベースとなるオブジェクトのコライダーのプロパティをShoutChanceRangeへコピー
            shoutChanceInstance.GetComponent<BoxCollider>().center = _transform.GetComponent<BoxCollider>().center;
            shoutChanceInstance.GetComponent<BoxCollider>().size = _transform.GetComponent<BoxCollider>().size;
            shoutChanceInstance.transform.eulerAngles = _transform.eulerAngles;
            shoutChanceInstance.SetParent(originParent_1);
            motorInstance.transform.SetParent(shoutChanceInstance);
            _noTriggerColliders.Add(shoutChanceInstance.GetComponent<BoxCollider>());
            motorInstance.transform.localPosition = Vector3.zero;
            _transform.SetParent(motorInstance.transform);
            _transform.localPosition = Vector3.zero;
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
            _poltergeistViewModel = new PoltergeistViewModel(poltergeistTable);
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
                            // ghostTeamIDが空なら、motorInstanceへポルターガイストを無効に更新する
                            // 空でないなら、有効に更新する
                            _motorView.IsEnabledPoltergeist = !string.IsNullOrEmpty(x.NewValue.ghostTeamID.Value);
                        })
                        .AddTo(ref _disposableBag);
                    // オブジェクトIDを割り振る
                    ghostInStaticObjectStruct.poltergeistViewID = GetInstanceID();
                    switch (ghostInStaticObjectStruct.useStatus)
                    {
                        case UseStatus.Using:
                            // 使用中ならIDを割り振る
                            ghostInStaticObjectStruct.ghostTeamID = new ReactiveProperty<string>();
                            ghostInStaticObjectStruct.ghostTeamID.Value = System.Guid.NewGuid().ToString();
                            _motorView.IsEnabledPoltergeist = true;

                            break;
                        default:
                            ghostInStaticObjectStruct.ghostTeamID = new ReactiveProperty<string>();
                            ghostInStaticObjectStruct.ghostTeamID.Value = string.Empty;

                            break;
                    }
                    _poltergeistViewModel.AddGhostInStaticObjectStructs(ghostInStaticObjectStruct);
                })
                .AddTo(ref _disposableBag);
            _script_XyloApi = new Script_xyloApi();
            _fadeImageView = FindAnyObjectByType<FadeImageView>();
            _homingObjectPoolerCustomizeView = FindAnyObjectByType<HomingObjectPoolerCustomizeView>();
            _objectPoolerXyloOtherCustomizeView = FindAnyObjectByType<ObjectPoolerXyloOtherCustomizeView>();
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _script_XyloApi?.Dispose();
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
            _script_XyloApi.ChangeBgmB();
            List<System.IDisposable> disposables = new List<System.IDisposable>();
            ReactiveCommand<bool> isCompletedRhythmPart = new ReactiveCommand<bool>();
            disposables.Add(
                isCompletedRhythmPart.Where(x => x)
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
                        var cnt = ghostStructs.Select(q => q.membersCount).Sum();
                        var healthPoint = _poltergeistViewModel.PlayerHealthPoint.Value;
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
                                    ResetMovePosition(_initialPosition, _initialEulerAngles, _noTriggerColliders, _rigidbody);
                                })
                            );
                        }
                        else
                        {
                            // 条件を満たさない場合は即座に完了するObservableを追加
                            completionObservables.Add(Observable.Return(true));
                        }
                        
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
                _script_XyloApi.BgmBStatus.DistinctUntilChanged()
                    .Where(x => x == 3)
                    .Subscribe(_ =>
                    {
                        isCompletedRhythmPart.Execute(true);
                    })
                    .AddTo(ref _disposableBag)
            );
            ReactiveCommand<int> membersCount = new ReactiveCommand<int>();
            disposables.Add(
                membersCount.Where(x => x < 1)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        isCompletedRhythmPart.Execute(true);
                    })
                    .AddTo(ref _disposableBag)
            );
            disposables.Add(
                Observable.EveryUpdate()
                    .Select(_ => _poltergeistViewModel.PlayerHealthPoint)
                    .Where(x => x != null)
                    .Take(1)
                    .Subscribe(x =>
                    {
                        disposables.Add(
                            x.Pairwise()
                                .Where(x => x.Current < x.Previous)
                                .Subscribe(playerHealthPoint =>
                            {
                                isCompletedRhythmPart.Execute(true);
                            })
                            .AddTo(ref _disposableBag)
                        );
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
            if (transactionGhostStruct.membersCount < 1)
            {
                ExitGhost();
            }
            else
            {
                ShuffleNewStaticObject();
            }
            viewModel.SetDefaultTransactionGhostInStaticObjectStruct();
        }

        /// <summary>
        /// オバケの引っ越し
        /// </summary>
        public void ShuffleNewStaticObject()
        {
            var ghostStructs = _poltergeistViewModel.GhostInStaticObjectStructs.ToList();
            // 空いているポルターガイストのインデックスを取得
            var emptyGhostStructIndices = ghostStructs
                .Select((p, i) => new { Content = p, Index = i })
                .Where(x => x.Content.useStatus.Equals(UseStatus.Empty))
                .Select(x => x.Index)
                .ToList();

            // 空きがある場合のみ処理を続行
            if (0 < emptyGhostStructIndices.Count)
            {
                // インデックスをランダムで選択
                int randomIndex = emptyGhostStructIndices[Random.Range(0, emptyGhostStructIndices.Count)];

                // 移動先の家具へポルターガイスト情報を更新
                var nextGhostInStaticObjectStruct = new GhostInStaticObjectStruct();
                nextGhostInStaticObjectStruct.poltergeistViewID = _poltergeistViewModel.GhostInStaticObjectStructs[randomIndex].poltergeistViewID;
                nextGhostInStaticObjectStruct.ghostTeamID = _poltergeistViewModel.GhostInStaticObjectStructs[randomIndex].ghostTeamID;
                nextGhostInStaticObjectStruct.ghostTeamID.Value = ghostInStaticObjectStruct.ghostTeamID.Value;
                nextGhostInStaticObjectStruct.useStatus = ghostInStaticObjectStruct.useStatus;
                nextGhostInStaticObjectStruct.membersCount = ghostInStaticObjectStruct.membersCount;
                _poltergeistViewModel.GhostInStaticObjectStructs[randomIndex] = nextGhostInStaticObjectStruct;

                ResetStaticObject();
            }
            else
            {
                Debug.Log("移動できる空きがありません。");
            }
        }

        /// <summary>
        /// 拠点を空室にする
        /// </summary>
        public void ExitGhost()
        {
            ResetStaticObject();
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
            var missileTempoSpawnerInstance = Instantiate(missileTempoSpawnerPrefab, _rhythmPartPosition_1, Quaternion.identity);
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
            SetRigidbodyStatus(_rigidbody, false);
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
            SetRigidbodyStatus(_rigidbody, false);
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
        /// 移動元の家具のポルターガイスト情報を初期化
        /// </summary>
        private void ResetStaticObject()
        {
            var ghostStructs = _poltergeistViewModel.GhostInStaticObjectStructs.ToList();

            // 移動元の家具のポルターガイスト情報は初期化
            var prevIndex = ghostStructs
                .Select((p, i) => new { Content = p, Index = i })
                .FirstOrDefault(x => x.Content.poltergeistViewID == ghostInStaticObjectStruct.poltergeistViewID)
                .Index;
            var prevGhostInStaticObjectStruct = new GhostInStaticObjectStruct();
            prevGhostInStaticObjectStruct.poltergeistViewID = ghostInStaticObjectStruct.poltergeistViewID;
            prevGhostInStaticObjectStruct.ghostTeamID = ghostInStaticObjectStruct.ghostTeamID;
            prevGhostInStaticObjectStruct.ghostTeamID.Value = string.Empty;
            prevGhostInStaticObjectStruct.useStatus = UseStatus.Empty;
            prevGhostInStaticObjectStruct.membersCount = 0;
            _poltergeistViewModel.GhostInStaticObjectStructs[prevIndex] = prevGhostInStaticObjectStruct;
        }

        /// <summary>
        /// ゴーストを飛び出させる
        /// </summary>
        /// <param name="ghostBurstsInstance">オバケが飛び出すエフェクト</param>
        /// <param name="transform">トランスフォーム</param>
        private void BurstGhosts(Transform ghostBurstsInstance, Transform transform)
        {
            if (ghostBurstsInstance == null)
            {
                ghostBurstsInstance = Instantiate(ghostBurstsPrefab).transform;
                // 親（ShoutChanceRange） > 親（Motor） > 家具
                ghostBurstsInstance.SetParent(transform.parent.parent);
                ghostBurstsInstance.localPosition = Vector3.zero;
            }
            else
            {
                ghostBurstsInstance.gameObject.SetActive(false);
                ghostBurstsInstance.gameObject.SetActive(true);
            }
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
        private void SetRigidbodyStatus(Rigidbody rigidbody, bool isEnabled)
        {
            if (rigidbody == null)
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
        private void ResetMovePosition(Vector3 initialPosition, Vector3 initialEulerAngles, List<Collider> noTriggerColliders, Rigidbody rigidbody)
        {
            _transform.position = initialPosition;
            _transform.eulerAngles = initialEulerAngles;
            SetNoTriggerColliders(noTriggerColliders, true);
            SetRigidbodyStatus(rigidbody, true);
        }
    }
}
