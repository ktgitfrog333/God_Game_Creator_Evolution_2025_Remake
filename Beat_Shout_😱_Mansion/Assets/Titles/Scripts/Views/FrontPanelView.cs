using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using Titles.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Titles.Views
{
    /// <summary>
    /// 手前に表示するパネルのビュー
    /// </summary>
    public class FrontPanelView : MonoBehaviour
    {
        /// <summary>手前に表示するパネルのビューモデル</summary>
        [SerializeField] private FrontPanelViewModel viewModel;
        /// <summary>キャンバスグループ</summary>
        [SerializeField] private CanvasGroup canvasGroup;
        /// <summary>読み込み中のゲージ</summary>
        [SerializeField] private Image progressGaugeImage;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();
        /// <summary>キャンセル用トークンソース</summary>
        private CancellationTokenSource _cts;

        private void Reset()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            foreach (Transform child in transform)
            {
                if (child.name.Equals("ProgressGaugeImage"))
                {
                    if (progressGaugeImage == null)
                        progressGaugeImage = child.GetComponent<Image>();
                }
            }
        }

        private void Start()
        {
            viewModel.Initialize();

            StartCountDown().Forget();
        }

        /// <summary>
        /// カウントダウン開始
        /// </summary>
        /// <returns>UniTask</returns>
        private async UniTaskVoid StartCountDown()
        {
            // キャンセルトークンを生成
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // Qキー押下を監視（押されたら即座にキャンセル）
            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Q))
                .Subscribe(_ =>
                {
                    // 管理者モード有効化
                    viewModel.SetIsActiveAdminMode(true);
                    // パネルを透明化
                    canvasGroup.alpha = 0f;
                    // タイマーキャンセル
                    _cts?.Cancel();
                })
                .AddTo(ref _disposableBag);

            try
            {
                float waitTime = 0f;
                float timeLimit = viewModel.autoLoadTimeLimit;

                await Observable.EveryUpdate()
                    .Do(_ =>
                    {
                        waitTime += Time.deltaTime;
                        // プログレスゲージ更新
                        progressGaugeImage.fillAmount = Mathf.Clamp01(waitTime / timeLimit);
                    })
                    .Where(_ => timeLimit <= waitTime)
                    .FirstAsync(token);

                // ここに到達 = 制限時間超過（Qキー未押下）
                // タイトルシーンへ遷移
                await SceneManager.LoadSceneAsync(viewModel.loadSceneName);
            }
            catch (System.OperationCanceledException)
            {
                // Qキー押下によるキャンセル → 何もしない（既に管理者モード処理済み）
                Debug.Log("Admin mode activated by Q key.");
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _cts?.Cancel();
            _cts?.Dispose();
            viewModel.Dispose();
        }
    }
}
