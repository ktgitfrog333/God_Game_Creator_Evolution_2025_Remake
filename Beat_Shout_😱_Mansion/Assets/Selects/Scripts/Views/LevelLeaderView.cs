using Selects.Commons;
using R3;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// レベルリーダー
    /// </summary>
    public class LevelLeaderView : MonoBehaviour
    {
        /// <summary>Startイベント呼び出しを順番通りにするためのレイヤー</summary>
        [SerializeField] private DidStartLayerData didStartLayerData;
        /// <summary>R3のリソース管理</summary>
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            // didStartLayerStructsのインデックスを宣言
            ReactiveProperty<int> index = new ReactiveProperty<int>();
            DidStartLayerStruct[] didStartLayerStructs = didStartLayerData.didStartLayerStructs;
            /*
             * ●インデックスを監視
             * 　○インデックス＜didStartLayerStructsの配列数の場合
             * 　　・インデックスの購読が完了したら下記の処理を実行
             * 　　　●didStartLayerStructsのインデックスに紐づくスクリプト名を取得
             * 　　　●スクリプト名を元にスクリプトのdidStartを監視
             * 　　　　○didStartがTrueとなった場合
             * 　　　　　・インデックスを+1へ更新
             * 　　　●スクリプトのゲームオブジェクトを有効にする
             */
            index.Where(i => i < didStartLayerStructs.Length)       // 範囲内のみ処理
                .Subscribe(i =>
                {
                    var scriptName = didStartLayerStructs[i].scriptName;
                    // スクリプト名に紐づくコンポーネントと didStart Observable を解決
                    if (!TryResolve(scriptName, out var behaviour, out var didStartObs))
                    {
                        Debug.LogWarning($"[LevelLeaderView] 解決できませんでした: {scriptName} (index={i})");

                        index.Value = i + 1; // 見つからない場合はスキップ
                        return;
                    }

                    // ●スクリプトのゲームオブジェクトを有効にする
                    if (behaviour != null && behaviour.gameObject != null && !behaviour.gameObject.activeSelf)
                    {
                        behaviour.gameObject.SetActive(true);
                    }

                    // ●didStartを監視して、Trueになったら次のインデックスへ
                    //    1度だけで良いので、Trueが来た最初の1回で購読を完了させる
                    didStartObs
                        .Take(1)
                        .SelectMany(_ => Observable.NextFrame()) // ★1フレーム待機
                        .Subscribe(_ =>
                        {
                            // ○didStartがTrueとなった場合 → インデックスを+1へ更新
                            index.Value = i + 1;
                        })
                        .AddTo(_disposables);
                })
                .AddTo(_disposables);
            /*
             * ●インデックスを0へ更新
             *   （初期値0だが、明示的にリセット）
             */
            index.Value = 0;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// スクリプト名から対象のMonoBehaviourと didStart Observable を解決する。
        /// </summary>
        private bool TryResolve(string scriptName, out MonoBehaviour behaviour, out Observable<Unit> didStartObs)
        {
            behaviour = null;
            didStartObs = null;

            // 明示的に知っているケース（データから参照をもらう）
            switch (scriptName)
            {
                case "PlayerView":
                    behaviour = didStartLayerData.playerView;

                    break;
                case "PlayerRespawnPositionView_0":
                    behaviour = didStartLayerData.playerRespawnPositionView_0;

                    break;
                case "PlayerRespawnPositionView_1":
                    behaviour = didStartLayerData.playerRespawnPositionView_1;

                    break;
                case "PlayerRespawnPositionView_2":
                    behaviour = didStartLayerData.playerRespawnPositionView_2;

                    break;
                case "PlayerRespawnPositionView_3":
                    behaviour = didStartLayerData.playerRespawnPositionView_3;

                    break;
                case "PlayerRespawnPositionView_4":
                    behaviour = didStartLayerData.playerRespawnPositionView_4;

                    break;
                default:
                    Debug.LogWarning($"未登録スクリプト: [{scriptName}]");

                    return false;
            }

            // didStart の Observable を取得（IDidStartProvider を期待）
            if (behaviour is IDidStartProvider provider)
            {
                didStartObs = provider.DidStartAsObservable();
            }

            return behaviour != null && didStartObs != null;
        }
    }

    /// <summary>
    /// Startイベント呼び出しを順番通りにするためのレイヤー
    /// </summary>
    /// <remarks>通常は使わない想定<br/>
    /// 一部のObserverを使用したコンポーネントのみ<br/>
    /// 個々のコンポーネントに対してStartを待ってから順番に生成したい場合に利用する</remarks>
    [System.Serializable]
    public class DidStartLayerData
    {
        /// <summary>プレイヤーのビュー</summary>
        public Mains.Views.PlayerView playerView;
        /// <summary>リスポーン地点ビュー（ステージ1）</summary>
        public PlayerRespawnPositionView playerRespawnPositionView_0;
        /// <summary>リスポーン地点ビュー（ステージ2）</summary>
        public PlayerRespawnPositionView playerRespawnPositionView_1;
        /// <summary>リスポーン地点ビュー（ステージ3）</summary>
        public PlayerRespawnPositionView playerRespawnPositionView_2;
        /// <summary>リスポーン地点ビュー（ステージ4）</summary>
        public PlayerRespawnPositionView playerRespawnPositionView_3;
        /// <summary>リスポーン地点ビュー（ステージ5）</summary>
        public PlayerRespawnPositionView playerRespawnPositionView_4;
        /// <summary>Startイベント呼び出しを順番通りにするためのレイヤーデータ構造体</summary>
        public DidStartLayerStruct[] didStartLayerStructs;
    }

    /// <summary>
    /// didStartを通知できるコンポーネント用インターフェース
    /// </summary>
    public interface IDidStartProvider
    {
        /// <summary>Start完了を通知するObservable（Trueになったら1度だけ発火）</summary>
        Observable<Unit> DidStartAsObservable();
    }
}
