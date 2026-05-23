using DG.Tweening;
using Mains.Views;
using R3;
using Selects.ViewModels;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Selects.Views
{
    /// <summary>
    /// 共通UIのビュー
    /// </summary>
    /// <remarks>をステージセレクト用に移植してきた版</remarks>
    /// <see cref="Mains.Views.CommonPanelView"/>
    public class CommonPanelCustomizeOfMainView : MonoBehaviour
    {
        /// <summary>共通UIの設定</summary>
        [SerializeField] private CommonPanelCustomizeOfMainSettins settins;
        [Header("その他オプション")]
        /// <summary>ハートアイコン設定</summary>
        [SerializeField] private IconHeartSettings iconHeartSettings;
        /// <summary>プレイヤーの体力を取得してハートアイコンへ反映する処理を実装</summary>
        private List<IconHeartImageView> _iconHeartImageViews;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals("FooterPanel"))
                {
                    foreach (Transform item in child)
                    {
                        if (item.name.Equals("IconHeartsPanel"))
                        {
                            if (iconHeartSettings.iconHeartsPanel == null)
                                iconHeartSettings.iconHeartsPanel = item as RectTransform;
                        }
                    }
                }
            }
        }

        private void Start()
        {
            var set = settins;
            set.viewModel.Initialize();
            var viewModel = settins.viewModel;

            // プレイヤーの体力を取得してハートアイコンへ反映する処理を実装
            ReactiveProperty<List<IconHeartImageView>> iconHeartImageViews = new ReactiveProperty<List<IconHeartImageView>>();
            iconHeartImageViews.Value = new List<IconHeartImageView>();
            _iconHeartImageViews = iconHeartImageViews.Value;
            viewModel.PlayerHealthPointMax.Subscribe(maxHp =>
            {
                // 最大HPの数だけ体力UIを拡張
                //  ●ハートアイコン表示の幅を広げる
                //  ●ハートアイコンを生成
                if (iconHeartSettings.iconHeartsPanel.childCount < maxHp)
                {
                    // 足りない分だけ生成（必要に応じて全削除→再生成も可）
                    RenderIconHeartImageViews(iconHeartSettings, maxHp,
                        iconHeartImageViews);
                    viewModel.PlayerHealthPoint.Subscribe(currentHp =>
                    {
                        RenderIconHeartImageViewsCurrent(iconHeartImageViews, currentHp);
                    })
                        .AddTo(ref _disposableBag);
                }
            })
                .AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            var set = settins;
            _disposableBag.Dispose();
            set.viewModel.Dispose();
        }

        /// <summary>
        /// HPアイコンを縮小アニメーションを再生
        /// </summary>
        /// <remarks>タイムライン上のシグナルから呼び出す<br/>
        /// インスペクタのSignal Receiverを参照</remarks>
        /// <see cref="Assets/Mains/TimeLines/HPDownDirection.playable"/>
        /// <see cref="Assets/Mains/TimeLines/ShrinkageHeartIconAnimation.signal"/>
        public void PlayShrinkageHeartIconAnimation()
        {
            var iconHeartImageViews = _iconHeartImageViews;
            if (iconHeartImageViews != null &&
                0 < iconHeartImageViews.Count)
            {
                var iconHeartImageView = iconHeartImageViews.Select((p, i) => new { Content = p, Index = i })
                    .OrderByDescending(q => q.Index)
                    .FirstOrDefault(x => x.Content.IsEnabledHeart)
                    .Content;
                if (iconHeartImageView == null)
                {
                    Debug.LogWarning($"該当のハートアイコン無し");

                    return;
                }
                // 縮小アニメーション
                var rectTransform = iconHeartImageView.EnabledTrans as RectTransform;
                if (rectTransform != null)
                {
                    // 既存のTweenがあれば停止
                    rectTransform.DOKill();
                    // スケールを0に縮小するアニメーション
                    rectTransform.DOScale(Vector3.zero, iconHeartSettings.iconHeartDuration)
                        .SetEase(Ease.InBack)
                        .SetUpdate(true);
                }
            }
        }

        /// <summary>
        /// ハートアイコン位置まで移動して星を光らせるアニメーションを再生
        /// </summary>
        /// <see cref="Assets/Mains/TimeLines/HPDownDirection.playable"/>
        /// <see cref="Assets/Mains/TimeLines/ShrinkageHeartIconAnimation.signal"/>
        public void PlayMoveIconHeartAndPlayLightingStarAnimation()
        {
            var iconHeartImageViews = _iconHeartImageViews;
            if (iconHeartImageViews != null)
            {
                int index = 0;
                for (int i = 0; i < iconHeartImageViews.Count; i++)
                {
                    if (!iconHeartImageViews[i].IsEnabledHeart)
                    {
                        // 直前に縮小されたハートアイコンを対象にする
                        index = i;

                        break;
                    }
                }
                var particleSys = iconHeartSettings.iconHeartLoststarPerticleSys;
                particleSys.transform.position = iconHeartImageViews[index].transform.position;
                particleSys.Play();
            }
        }

        /// <summary>
        /// ハートアイコン最大数を描画
        /// </summary>
        /// <param name="iconHeartSettings">ハートアイコン設定</param>
        /// <param name="maxHp">プレイヤーの最大HP</param>
        /// <param name="iconHeartImageViews">プレイヤーの体力を取得してハートアイコンへ反映する処理を実装</param>
        private void RenderIconHeartImageViews(IconHeartSettings iconHeartSettings, int maxHp,
            ReactiveProperty<List<IconHeartImageView>> iconHeartImageViews)
        {
            for (int i = iconHeartSettings.iconHeartsPanel.childCount; i < maxHp; i++)
            {
                var heartGO = Instantiate(iconHeartSettings.iconHeartImagePrefab, iconHeartSettings.iconHeartsPanel);
                var heartView = heartGO.GetComponent<IconHeartImageView>();
                if (heartView != null)
                {
                    iconHeartImageViews.Value.Add(heartView);
                }
            }
        }

        /// <summary>
        /// ハートアイコン現在数を描画
        /// </summary>
        /// <param name="iconHeartImageViews">プレイヤーの体力を取得してハートアイコンへ反映する処理を実装</param>
        /// <param name="currentHp">プレイヤーのHP</param>
        private void RenderIconHeartImageViewsCurrent(ReactiveProperty<List<IconHeartImageView>> iconHeartImageViews, int currentHp)
        {
            // 最大（右）から減らしていく
            for (int i = 0; i < iconHeartImageViews.Value.Count; i++)
            {
                if (i >= currentHp)
                    iconHeartImageViews.Value[i].SetSpriteHeartNot(); // ♡
                else
                    iconHeartImageViews.Value[i].SetSpriteHeart();    // ♥
            }
        }
    }

    /// <summary>
    /// 共通UIの設定
    /// </summary>
    [System.Serializable]
    public class CommonPanelCustomizeOfMainSettins
    {
        /// <summary>共通UIのビューモデル</summary>
        public CommonPanelCustomizeOfMainViewModel viewModel;
    }
}
