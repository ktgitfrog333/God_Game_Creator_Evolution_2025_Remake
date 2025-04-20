using UnityEngine;
using Mains.Commons;
using R3;
using ObservableCollections;
using Mains.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;

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
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>トランスフォーム</summary>
        private Transform _transform;

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
                            Debug.Log($"guid: [{ghostInStaticObjectStruct.ghostTeamID.Value}]");
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
            // リズムパートから探索パートへ切り替わった時の処理
            Observable.EveryUpdate()
                .Select(x => _poltergeistViewModel.InteractionPart)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Pairwise()
                        .Where(x => x.Previous.Equals(InteractionPart.Rhythm) &&
                            x.Current.Equals(InteractionPart.Search))
                        .Subscribe(_ =>
                        {
                            FindMissileTempoSpawnerInstanceAndDestroy(_missileTempoSpawnerInstance);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
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
            if (missileTempoSpawnerInstance != null)
            {
                Destroy(missileTempoSpawnerInstance);
            }
        }
    }
}
