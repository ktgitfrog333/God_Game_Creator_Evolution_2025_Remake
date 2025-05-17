using Mains.Commons;
using Mains.ViewModels;
using TMPro;
using UnityEngine;
using R3;
using ObservableCollections;
using System.Linq;
using System.Collections.Generic;
using Rewired;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

namespace Mains.Views
{
    /// <summary>
    /// 共通UIのビュー
    /// </summary>
    public class CommonPanelView : MonoBehaviour
    {
        [Tooltip("CommonPanel > HeaderPanel > IconAndGuidePanel > GuideText をセット")]
        /// <summary>ミッションガイド概要のテキスト</summary>
        [SerializeField] private TextMeshProUGUI guideText;
        [Tooltip("CommonPanel > HeaderPanel > MissionText をセット")]
        /// <summary>ミッションガイド詳細のテキスト</summary>
        [SerializeField] private TextMeshProUGUI missionText;
        [Tooltip("CommonPanel > FooterPanel > IconHeartsPanel をセット")]
        /// <summary>ハートアイコンを表示する用のトランスフォーム</summary>
        [SerializeField] private RectTransform iconHeartsPanel;
        [Tooltip("Assets/Mains/Prefabs/UIs/CommonPanels/IconHeartImage.prefab をセット")]
        /// <summary>iconHeartImageのプレハブ</summary>
        [SerializeField] private Transform iconHeartImagePrefab;
        [Tooltip("CommonPanel > CenterPanel > StageClearPanel をセット")]
        /// <summary>STAGE CLEARのパネル</summary>
        [SerializeField] private RectTransform stageClearPanel;
        [Tooltip("CommonPanel > CenterPanel > StageClearPanel > StageClearText をセット")]
        /// <summary>STAGE CLEARのテキスト</summary>
        [SerializeField] private TextMeshProUGUI stageClearText;
        [SerializeField] private CommonPanelTemplateStruct 共通UIのテンプレート;
        /// <summary>共通UIのビューモデル</summary>
        private CommonPanelViewModel _commonPanelViewModel;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (guideText == null)
                guideText = transform.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
            if (missionText == null)
                missionText = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
            if (iconHeartsPanel == null)
                iconHeartsPanel = transform.GetChild(1).GetChild(0) as RectTransform;
            if (stageClearPanel == null)
                stageClearPanel = transform.GetChild(2).GetChild(0) as RectTransform;
            if (stageClearText == null)
                stageClearText = transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            _commonPanelViewModel = new CommonPanelViewModel();
            var player = ReInput.players.GetPlayer(0);
            FadeImageView fadeImageView = FindAnyObjectByType<FadeImageView>();
            // オバケの家具入居管理の構造体リストから、オバケの数を全て取得してその合計をミッションガイド概要／詳細へ反映する処理を実装
            Observable.EveryUpdate()
                .Select(_ => _commonPanelViewModel.GhostInStaticObjectStructs)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    // 初回の合計を表示
                    var ghostAllMembersCount = x.Select(q => q.membersCount).Sum();
                    guideText.text = 共通UIのテンプレート.guideText.Replace("${ghostAllMembersCount}", $"{ghostAllMembersCount}");
                    var ghostExitMembersCount = 0;
                    missionText.text = 共通UIのテンプレート.missionText.Replace("${ghostAllMembersCount}", $"{ghostAllMembersCount}")
                        .Replace("${ghostExitMembersCount}", $"{ghostExitMembersCount}");
                    // 追加されたら再度合計を更新
                    x.ObserveAdd()
                        .Subscribe(_ =>
                        {
                            ghostAllMembersCount = x.Select(q => q.membersCount).Sum();
                            guideText.text = 共通UIのテンプレート.guideText.Replace("${ghostAllMembersCount}", $"{ghostAllMembersCount}");
                            var ghostExitMembersCount = 0;
                            missionText.text = 共通UIのテンプレート.missionText.Replace("${ghostAllMembersCount}", $"{ghostAllMembersCount}")
                                .Replace("${ghostExitMembersCount}", $"{ghostExitMembersCount}");
                        })
                        .AddTo(ref _disposableBag);
                    // オバケが減ったら再度合計を更新
                    x.ObserveReplace()
                        .Subscribe(_ =>
                        {
                            var ghostAllMembersUpdCount = x.Select(q => q.membersCount).Sum();
                            // 初回の合計 - 減った後の合計 = 倒した数
                            var ghostExitMembersCount = ghostAllMembersCount - ghostAllMembersUpdCount;
                            missionText.text = 共通UIのテンプレート.missionText.Replace("${ghostAllMembersCount}", $"{ghostAllMembersCount}")
                                .Replace("${ghostExitMembersCount}", $"{ghostExitMembersCount}");
                            CheckMissionStatusAndDirectionClear(ghostAllMembersUpdCount, stageClearPanel, stageClearText, player, fadeImageView);
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // プレイヤーの体力を取得してハートアイコンへ反映する処理を実装
            List<IconHeartImageView> iconHeartImageViews = new List<IconHeartImageView>();
            Observable.EveryUpdate()
                .Select(_ => _commonPanelViewModel.PlayerHealthPointMax)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    // 体力ゼロより大きい値がセットされる前提
                    x.Where(x => 0 < x)
                        .Take(1)
                        .Subscribe(maxHp =>
                        {
                            // 最大HPの数だけ体力UIを拡張
                            //  ●ハートアイコン表示の幅を広げる
                            //  ●ハートアイコンを生成
                            if (iconHeartsPanel.childCount < maxHp)
                            {
                                // 足りない分だけ生成（必要に応じて全削除→再生成も可）
                                for (int i = iconHeartsPanel.childCount; i < maxHp; i++)
                                {
                                    var heartGO = Instantiate(iconHeartImagePrefab, iconHeartsPanel);
                                    var heartView = heartGO.GetComponent<IconHeartImageView>();
                                    if (heartView != null)
                                    {
                                        iconHeartImageViews.Add(heartView);
                                    }
                                }
                                _commonPanelViewModel.PlayerHealthPoint.Subscribe(currentHp =>
                                {
                                    // 最大（右）から減らしていく
                                    for (int i = 0; i < iconHeartImageViews.Count; i++)
                                    {
                                        if (i >= currentHp)
                                            iconHeartImageViews[i].SetSpriteHeartNot(); // ♡
                                        else
                                            iconHeartImageViews[i].SetSpriteHeart();    // ♥
                                    }
                                })
                                .AddTo(ref _disposableBag);
                                // HP減少のみを監視
                                _commonPanelViewModel.PlayerHealthPoint.Pairwise()
                                    .Where(x => x.Current < x.Previous)
                                    .Select(x => x.Current)
                                    .Subscribe(currentHp =>
                                {
                                    CheckPlayerHealthPointAndDirectionGameOver(currentHp, player, fadeImageView);
                                })
                                .AddTo(ref _disposableBag);
                            }
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            stageClearPanel.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// プレイヤーのHPを監視してゲームオーバー演出を実行
        /// </summary>
        private void CheckPlayerHealthPointAndDirectionGameOver(int healthPoint, Player player, FadeImageView fadeImageView)
        {
            if (healthPoint < 1)
            {
                // 時間を停止
                Time.timeScale = 0f;
                player.controllers.maps.SetMapsEnabled(false, "Default"); // ゲーム操作を無効化
                Observable.Create<bool>(observer =>
                {
                    StartCoroutine(fadeImageView.PlayFadeInDirection(observer, 1.5f));
                    return Disposable.Empty;
                })
                    .Subscribe(_ =>
                    {
                        Observable.Create<bool>(observer =>
                        {
                            StartCoroutine(LoadSceneCoroutine(observer, "MainScene"));
                            return Disposable.Empty;
                        })
                            .Subscribe(_ => { })
                            .AddTo(ref _disposableBag);
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        /// <summary>
        /// ミッション情報を監視してクリア演出を実行
        /// </summary>
        /// <param name="ghostAllMembersUpdCount">利用総人数（更新後）</param>
        /// <param name="stageClearPanel">STAGE CLEARのパネル</param>
        /// <param name="stageClearText">STAGE CLEARのテキスト</param>
        /// <param name="player">ReInputのPlayer</param>
        /// <param name="fadeImageView">フェードイメージのビュー</param>
        private void CheckMissionStatusAndDirectionClear(int ghostAllMembersUpdCount, RectTransform stageClearPanel, TextMeshProUGUI stageClearText, Player player, FadeImageView fadeImageView)
        {
            if (ghostAllMembersUpdCount < 1)
            {
                // 時間を停止
                Time.timeScale = 0f;
                player.controllers.maps.SetMapsEnabled(false, "Default"); // ゲーム操作を無効化

                // TextMeshProを取得して、クリア演出の様なDOTweenアニメーションをつける。完了を通知する。
                stageClearPanel.gameObject.SetActive(true);
                stageClearText.transform.localScale = Vector3.zero;
                stageClearText.DOFade(0f, 0f);
                DOTween.Sequence()
                    .Append(stageClearText.DOFade(1f, 0.5f))
                    .Join(stageClearText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack))
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        // 必要ならここでさらに次の処理を繋ぐ
                        player.controllers.maps.SetMapsEnabled(true, "CategoryUI");       // UI操作だけ有効化
                        Observable.EveryUpdate()
                            .Select(_ => player.GetButtonDown("Submit"))
                            .DistinctUntilChanged()
                            .Where(x => x)
                            .Take(1)
                            .Subscribe(_ =>
                            {
                                player.controllers.maps.SetMapsEnabled(false, "CategoryUI");
                                Observable.Create<bool>(observer =>
                                {
                                    StartCoroutine(fadeImageView.PlayFadeInDirection(observer));
                                    return Disposable.Empty;
                                })
                                    .Subscribe(_ =>
                                    {
                                        Observable.Create<bool>(observer =>
                                        {
                                            StartCoroutine(LoadSceneCoroutine(observer, "MainScene"));
                                            return Disposable.Empty;
                                        })
                                            .Subscribe(_ => { })
                                            .AddTo(ref _disposableBag);
                                    })
                                    .AddTo(ref _disposableBag);
                            })
                            .AddTo(ref _disposableBag);
                    });
            }
        }

        /// <summary>
        /// シーンをロード
        /// </summary>
        /// <param name="observer">オブザーバー</param>
        /// <param name="sceneName">シーン名</param>
        /// <returns>コルーチン</returns>
        /// <see cref="Assets/Mains/Scenes/MainScene.unity"/>
        private IEnumerator LoadSceneCoroutine(Observer<bool> observer, string sceneName)
        {
            // 時間を再生
            Time.timeScale = 1f;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // 読み込みが終わるまで待機
            while (!asyncLoad.isDone)
            {
                // ここでasyncLoad.progressを見てローディング演出もできる！
                yield return null;
            }
            observer.OnNext(true);
            observer.OnCompleted();
        }
    }
}
