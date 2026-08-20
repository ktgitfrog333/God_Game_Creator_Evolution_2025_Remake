using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

namespace Titles.Views
{
    /// <summary>
    /// ゲームオブジェクトを有効にするシークエンサーのビュー
    /// </summary>
    /// <remarks>その2</remarks>
    public class ActiveGameObjectsSequencerView1 : MonoBehaviour
    {
        /// <summary>コマンドボタンのビュー</summary>
        [SerializeField] private CommandButtonView commandButtonView;
        /// <summary>使用するマイクパネルのビュー</summary>
        [SerializeField] private UseMicPanelView useMicPanelView;
        /// <summary>セットアップガイドパネルのビュー</summary>
        [SerializeField] private SetupGuidePanelView setupGuidePanelView;
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
            commandButtonView.gameObject.SetActive(true);

            await Observable.EveryUpdate()
                .Where(_ => commandButtonView.IsCompleted.CurrentValue)
                .FirstAsync(token);

            setupGuidePanelView.gameObject.SetActive(true);

            await Observable.EveryUpdate()
                .Where(_ => setupGuidePanelView.IsCompleted.CurrentValue)
                .FirstAsync(token);

            useMicPanelView.gameObject.SetActive(true);
        }
    }
}
