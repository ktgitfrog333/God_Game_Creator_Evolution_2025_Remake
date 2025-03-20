using UnityEngine;
using Mains.Commons;
using R3;
using ObservableCollections;
using Mains.ViewModels;
using System.Linq;

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
        // TODO:シャウトチャンスパートでポルターガイストが発生した時のエフェクト
        [SerializeField] private GameObject dustParticlePrefab;
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
        /// <summary>ポルターガイストのビューモデル</summary>
        private PoltergeistViewModel _poltergeistViewModel;
        /// <summary>モーターのビュー</summary>
        private MotorView _motorView;
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
            var trans = transform;
            // Poltergeistの生成
            var originParent = trans.parent;
            // 初期化
            var motorInstance = Instantiate(motorPrefab, trans.position, Quaternion.identity);
            // ベースとなるオブジェクトのコライダーのプロパティをMotorへコピー
            motorInstance.GetComponent<BoxCollider>().center = trans.GetComponent<BoxCollider>().center;
            motorInstance.GetComponent<BoxCollider>().size = trans.GetComponent<BoxCollider>().size;
            motorInstance.transform.eulerAngles = trans.eulerAngles;
            motorInstance.transform.SetParent(originParent);
            trans.SetParent(motorInstance.transform);
            trans.localPosition = Vector3.zero;
            // ShoutChanceRangeの生成
            var originParent_1 = motorInstance.transform.parent;
            Transform shoutChanceInstance = Instantiate(shoutChanceRangePrefab, motorInstance.transform.position, Quaternion.identity).transform;
            // ベースとなるオブジェクトのコライダーのプロパティをShoutChanceRangeへコピー
            shoutChanceInstance.GetComponent<BoxCollider>().center = trans.GetComponent<BoxCollider>().center;
            shoutChanceInstance.GetComponent<BoxCollider>().size = trans.GetComponent<BoxCollider>().size;
            shoutChanceInstance.transform.eulerAngles = trans.eulerAngles;
            shoutChanceInstance.SetParent(originParent_1);
            motorInstance.transform.SetParent(shoutChanceInstance);
            motorInstance.transform.localPosition = Vector3.zero;
            trans.SetParent(motorInstance.transform);
            trans.localPosition = Vector3.zero;
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
    }
}
