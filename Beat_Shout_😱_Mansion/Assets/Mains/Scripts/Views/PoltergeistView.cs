using UnityEngine;
using Mains.Commons;
using R3;
using ObservableCollections;
using Mains.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Mains.External;
using Unity.Collections;
using System.Collections.Generic;

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
        /// <summary>リズムパートポジション（プレイヤー位置をリズムパート用に移動させる）</summary>
        private Transform _rhythmPartPosition;
        /// <summary>リズムパートポジション（プレイヤー位置をリズムパート用に移動させる）</summary>
        public Transform RhythmPartPosition => _rhythmPartPosition;
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
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>フェードイメージのビュー</summary>
        private FadeImageView _fadeImageView;
        /// <summary>MissileObjectPoolerのカスタマイズビュー</summary>
        private HomingObjectPoolerCustomizeView _homingObjectPoolerCustomizeView;
        /// <summary>ObjectPoolerXyloOtherのカスタマイズビュー</summary>
        private ObjectPoolerXyloOtherCustomizeView _objectPoolerXyloOtherCustomizeView;
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
                var newObj = new GameObject("RhythmPartPosition");
                newObj.transform.position = transform.position;
                newObj.transform.SetParent(transform);
            }
        }

        private void Start()
        {
            _transform = transform;
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
            // ShoutChanceRangeの生成
            var originParent_1 = motorInstance.transform.parent;
            Transform shoutChanceInstance = Instantiate(shoutChanceRangePrefab, motorInstance.transform.position, Quaternion.identity).transform;
            // ベースとなるオブジェクトのコライダーのプロパティをShoutChanceRangeへコピー
            shoutChanceInstance.GetComponent<BoxCollider>().center = _transform.GetComponent<BoxCollider>().center;
            shoutChanceInstance.GetComponent<BoxCollider>().size = _transform.GetComponent<BoxCollider>().size;
            shoutChanceInstance.transform.eulerAngles = _transform.eulerAngles;
            shoutChanceInstance.SetParent(originParent_1);
            motorInstance.transform.SetParent(shoutChanceInstance);
            motorInstance.transform.localPosition = Vector3.zero;
            _transform.SetParent(motorInstance.transform);
            _transform.localPosition = Vector3.zero;
            foreach (Transform child in transform)
            {
                if (child.name.Equals("RhythmPartPosition"))
                {
                    _rhythmPartPosition = child;
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
                        ReactiveProperty<int> processStepCnt = new ReactiveProperty<int>();
                        disposables.Add(
                            processStepCnt.Where(x => 1 < x)
                                .Subscribe(_ =>
                                {
                                    // パート切り替え
                                    _poltergeistViewModel.SetInteractionPartToSearch();
                                    foreach (var disposable in disposables)
                                        disposable.Dispose();
                                })
                                .AddTo(ref _disposableBag)
                        );
                        // 暗幕フェード
                        // 利用総人数が0なら暗幕以降の演出は実行しない
                        var ghostStructs = _poltergeistViewModel.GhostInStaticObjectStructs;
                        var cnt = ghostStructs.Select(q => q.membersCount).Sum();
                        var healthPoint = _poltergeistViewModel.PlayerHealthPoint.Value;
                        if (0 < cnt &&
                            0 < healthPoint)
                        {
                            disposables.Add(
                                Observable.Create<bool>(observer =>
                                {
                                    StartCoroutine(_fadeImageView.PlayFadeInDirection(observer));
                                    return Disposable.Empty;
                                })
                                    .Subscribe(_ =>
                                    {
                                        processStepCnt.Value++;
                                    })
                                    .AddTo(ref _disposableBag)
                            );
                        }
                        else
                        {
                            foreach (var disposable in disposables)
                                disposable.Dispose();
                        }
                        // スポナーの削除
                        FindMissileTempoSpawnerInstanceAndDestroy(_missileTempoSpawnerInstance);
                        // オバケが残っていたらプールへ戻す
                        _homingObjectPoolerCustomizeView.DoReturnAllMissilesToPool();
                        // オバケが残っていたらプールへ戻す（Other）
                        disposables.Add(
                            Observable.Create<bool>(observer =>
                            {
                                StartCoroutine(_objectPoolerXyloOtherCustomizeView.AllDisabled(observer));
                                return Disposable.Empty;
                            })
                                .Subscribe(_ =>
                                {
                                    processStepCnt.Value++;
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
            BurstGhosts(_ghostBurstsInstance, _transform);
            await Task.Delay(1000);
            if (_poltergeistViewModel != null)
                _poltergeistViewModel.SetIsCompletedBurstGhosts(true);
        }

        /// <summary>
        /// ミサイルテンポスポナーを生成
        /// </summary>
        public void InstanceMissileTempoSpawner()
        {
            var originParent = _transform.parent;
            var missileTempoSpawnerInstance = Instantiate(missileTempoSpawnerPrefab, _transform.position, Quaternion.identity);
            missileTempoSpawnerInstance.transform.SetParent(originParent);
            _missileTempoSpawnerInstance = missileTempoSpawnerInstance;
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
    }
}
