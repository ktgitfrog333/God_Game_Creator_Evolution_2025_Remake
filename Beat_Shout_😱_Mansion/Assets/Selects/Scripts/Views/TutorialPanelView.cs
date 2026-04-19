using TMPro;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using System.Linq;

namespace Selects.Views
{
    /// <summary>
    /// チュートリアルパネルのビュー
    /// </summary>
    public class TutorialPanelView : MonoBehaviour
    {
        /// <summary>チュートリアルパネルの設定</summary>
        [SerializeField] private TutorialPanelSettings settings;
        /// <summary>プレイヤー位置</summary>
        public Observable<Vector3> PositionStream => positionStream;
        private Subject<Vector3> positionStream = new();
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            var set = settings;
            set.mainMessageTexts = GetComponentsInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            var set = settings;
            Observable.EveryUpdate()
                .Select(_ => set.player)
                .Where(x => x != null)
                .Subscribe(player =>
                {
                    positionStream.OnNext(player.transform.position);
                })
                .AddTo(ref _disposableBag);
            RunTutorial(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        private async UniTaskVoid RunTutorial(CancellationToken token)
        {
            var set = settings;
            GameObject[] mainMessages = set.mainMessageTexts.Select(x => x.gameObject).ToArray();
            // ① 初回メッセージ表示
            mainMessages[0].SetActive(true);
            mainMessages[1].SetActive(false);

            // ② プレイヤー移動待ち
            await WaitPlayerMoveAsync(token);

            // ③ 切り替え
            mainMessages[0].SetActive(false);
            mainMessages[1].SetActive(true);
        }

        private async UniTask WaitPlayerMoveAsync(CancellationToken token)
        {
            var set = settings;
            var player = set.player.transform;

            // ▼ パターン①：位置変化ベース
            await PositionStream
                .Pairwise() // 前回と比較
                .Where(pair => Vector3.Distance(pair.Previous, pair.Current) > 0.1f)
                .FirstAsync(token);
        }
    }

    /// <summary>
    /// チュートリアルパネルの設定
    /// </summary>
    [System.Serializable]
    public class TutorialPanelSettings
    {
        /// <summary>メインメッセージ</summary>
        public TextMeshProUGUI[] mainMessageTexts;
        /// <summary>プレイヤー</summary>
        public Transform player;
    }
}
