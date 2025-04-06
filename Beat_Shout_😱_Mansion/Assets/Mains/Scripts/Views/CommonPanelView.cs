using Mains.Commons;
using Mains.ViewModels;
using TMPro;
using UnityEngine;
using R3;
using ObservableCollections;
using System.Linq;
using System.Collections.Generic;

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
        [Tooltip("CommonPanel > HeaderPanel > dBLevelText をセット")]
        /// <summary>dB(A)を表示する用のテキスト</summary>
        [SerializeField] private TextMeshProUGUI dBLevelText;
        [Tooltip("CommonPanel > FooterPanel > IconHeartsPanel をセット")]
        /// <summary>ハートアイコンを表示する用のトランスフォーム</summary>
        [SerializeField] private RectTransform iconHeartsPanel;
        [Tooltip("Assets/Mains/Prefabs/UIs/CommonPanels/IconHeartImage.prefab をセット")]
        /// <summary>iconHeartImageのプレハブ</summary>
        [SerializeField] private Transform iconHeartImagePrefab;
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
            if (dBLevelText == null)
                dBLevelText = transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>();
            if (iconHeartsPanel == null)
                iconHeartsPanel = transform.GetChild(1).GetChild(0) as RectTransform;
        }

        private void Start()
        {
            _commonPanelViewModel = new CommonPanelViewModel();
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
                            var ghostAllMembersCount = x.Select(q => q.membersCount).Sum();
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
                        })
                        .AddTo(ref _disposableBag); 
                })
                .AddTo(ref _disposableBag);
            // デシベルレベルを取得してdB(A)を表示する用のテキストへ反映する処理を追加
            Observable.EveryUpdate()
                .Select(_ => _commonPanelViewModel.DbLevel)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Subscribe(x =>
                    {
                        dBLevelText.text = 共通UIのテンプレート.dBLevelText.Replace("${dBLevel}", $"{x}");
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
                            }
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
        }
    }
}
