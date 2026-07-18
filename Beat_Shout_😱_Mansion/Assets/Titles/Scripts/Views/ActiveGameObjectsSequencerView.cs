using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

namespace Titles.Views
{
    /// <summary>
    /// ゲームオブジェクトを有効にするシークエンサーのビュー
    /// </summary>
    public class ActiveGameObjectsSequencerView : MonoBehaviour
    {
        /// <summary>メッセージパネルのビュー</summary>
        [SerializeField] private MessagePanelView messagePanelView;
        /// <summary>マイク入力パネルのビュー</summary>
        [SerializeField] private InputMicPanelView inputMicPanelView;
        /// <summary>キャンセラレーショントークンソース</summary>
        private CancellationTokenSource _cts;

        private void Start()
        {
            _cts = new CancellationTokenSource();

            LoadAsync(_cts.Token).Forget();
        }

        private void OnDestroy()
        {
            _cts.Dispose();
        }

        /// <summary>
        /// ロード処理
        /// </summary>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask LoadAsync(CancellationToken token)
        {
            messagePanelView.gameObject.SetActive(true);

            await Observable.EveryUpdate()
                .Where(_ => messagePanelView.IsCompleted.CurrentValue)
                .FirstAsync(token);

            inputMicPanelView.gameObject.SetActive(true);
        }
    }
}
