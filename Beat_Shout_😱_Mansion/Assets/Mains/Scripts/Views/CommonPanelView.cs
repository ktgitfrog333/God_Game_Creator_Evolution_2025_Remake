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
using UnityEngine.UI;
using Mains.Manager;
using Mains.External;

namespace Mains.Views
{
    /// <summary>
    /// 共通UIのビュー
    /// </summary>
    public class CommonPanelView : MonoBehaviour
    {
        [Tooltip("CommonPanel > CenterFront1Panel > HorrorMokumokuFramePanel > HorrorMokumokuFrameImage をセット")]
        /// <summary>恐怖もくもくフレームのイメージ</summary>
        [SerializeField] private Image horrorMokumokuFrameImage;
        /// <summary>フレームのアルファ値最小</summary>
        [SerializeField] private float frameAlphaMin;
        /// <summary>フレームのアルファ値最大</summary>
        [SerializeField] private float frameAlphaMax;
        /// <summary>心音最小</summary>
        // [SerializeField] private float heartBeatMin;
        /// <summary>心音最大</summary>
        // [SerializeField] private float heartBeatMax;
        /// <summary>心音プロパティ構造体</summary>
        [SerializeField] private HeartBeatPropStruct[] heartBeatPropStructs;
        [Tooltip("CommonPanel > HeaderPanel > IconAndGuidePanel > GuideText をセット")]
        /// <summary>ミッションガイド概要のテキスト</summary>
        [SerializeField] private TextMeshProUGUI guideText;
        [Tooltip("CommonPanel > HeaderPanel > MissionText をセット")]
        /// <summary>ミッションガイド詳細のテキスト</summary>
        [SerializeField] private TextMeshProUGUI missionText;
        [Tooltip("CommonPanel > FooterPanel > IconHeartsPanel をセット")]
        /// <summary>ハートアイコンを表示する用のトランスフォーム</summary>
        [SerializeField] private RectTransform iconHeartsPanel;
        [Tooltip("CommonPanel > FooterPanel > HorrorGaugeSlider をセット")]
        /// <summary>恐怖ゲージのスライダー</summary>
        [SerializeField] private Slider horrorGaugeSlider;
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
        [Tooltip("Assets/Mains/Prefabs/Level/ObjectsPoolView.prefabをセットしておく。")]
        [SerializeField] private GameObject objectsPoolViewPrefab;
        /// <summary>共通UIのビューモデル</summary>
        private CommonPanelViewModel _commonPanelViewModel;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (horrorMokumokuFrameImage == null)
                horrorMokumokuFrameImage = transform.GetChild(0).GetChild(0).GetComponentInChildren<Image>();
            if (guideText == null)
                guideText = transform.GetChild(1).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
            if (missionText == null)
                missionText = transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
            if (iconHeartsPanel == null)
                iconHeartsPanel = transform.GetChild(2).GetChild(0) as RectTransform;
            if (horrorGaugeSlider == null)
                horrorGaugeSlider = transform.GetChild(2).GetChild(1).GetComponent<Slider>();
            if (stageClearPanel == null)
                stageClearPanel = transform.GetChild(3).GetChild(0) as RectTransform;
            if (stageClearText == null)
                stageClearText = transform.GetChild(3).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
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
                                    CheckPlayerHealthPointAndDoDirectionGameOver(currentHp, player, fadeImageView);
                                })
                                .AddTo(ref _disposableBag);
                            }
                        })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            stageClearPanel.gameObject.SetActive(false);
            _script_XyloApi = new Script_xyloApi();
            // 恐怖もくもくフレームUIと恐怖ゲージUIを制御
            var heartBeatPropStruct = heartBeatPropStructs[0];
            Observable.EveryUpdate()
                .Select(_ => _commonPanelViewModel.HorrorCount)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
                {
                    x.Subscribe(horrorCount =>
                    {
                        var max = GameManager.Instance.LevelOwner.HorrorCountMax;
                        if (max != null)
                        {
                            RenderFrameImageAlpha(horrorCount, max.Value, frameAlphaMin, frameAlphaMax, horrorMokumokuFrameImage);
                            SetHorrorGaugeSlider(horrorCount, max.Value, horrorGaugeSlider);
                            heartBeatPropStruct = GetHeartBeatPropStruct(horrorCount, max.Value, heartBeatPropStructs);
                            CheckHorrorCountAndDoDirectionGameOver(horrorCount, max.Value, player, fadeImageView, _script_XyloApi);
                        }
                    })
                        .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            // 一定のリズムでSEを再生。horrorCountが残り少ないほどビートが短くなっていく。
            float heartBeatElapsedTime = 0f;
            // オブジェクトプールビュー
            var objectsPoolView = GameObject.FindAnyObjectByType<ObjectsPoolView>();
            if (objectsPoolView == null)
                objectsPoolView = Instantiate(objectsPoolViewPrefab).GetComponent<ObjectsPoolView>();
            Se_3D_PickerCustomizeView t3DSoundPlayer = objectsPoolView.Get3DSoundPlayer();
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                	if (heartBeatPropStruct.value <= heartBeatElapsedTime)
                	{
                        t3DSoundPlayer.PlaySound("footstep", heartBeatPropStruct.volumeLevel);
                        heartBeatElapsedTime = 0f;

                		return;
                	}
                	heartBeatElapsedTime += Time.deltaTime;
                })
                	.AddTo(ref _disposableBag);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _script_XyloApi.Dispose();
        }

        /// <summary>
        /// プレイヤーのHPを監視してゲームオーバー演出を実行
        /// </summary>
        /// <param name="healthPoint">プレイヤーのHP</param>
        /// <param name="player">RewiredのPlayer</param>
        /// <param name="fadeImageView">フェードイメージのビュー</param>
        private void CheckPlayerHealthPointAndDoDirectionGameOver(int healthPoint, Player player, FadeImageView fadeImageView)
        {
            if (healthPoint < 1)
            {
                DirectionGameOver(player, fadeImageView);
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
        /// 恐怖もくもくフレームのイメージを描画
        /// </summary>
        /// <param name="horrorCount">恐怖値</param>
        /// <param name="horrorCountMax">恐怖値最大</param>
        /// <param name="frameAlphaMin">フレームのアルファ値最小</param>
        /// <param name="frameAlphaMax">フレームのアルファ値最大</param>
        /// <param name="horrorMokumokuFrameImage">恐怖もくもくフレームのイメージ</param>
        private void RenderFrameImageAlpha(float horrorCount, float horrorCountMax, float frameAlphaMin, float frameAlphaMax, Image horrorMokumokuFrameImage)
        {
            var horror = horrorCount / horrorCountMax;
            var frameAlpha = frameAlphaMin + (frameAlphaMax - frameAlphaMin) * horror;
            var color = horrorMokumokuFrameImage.color;
            var tmpColor = new Color(color.r, color.g, color.b, frameAlpha);
            horrorMokumokuFrameImage.color = tmpColor;
        }

        /// <summary>
        /// 恐怖ゲージのスライダーの値をセット
        /// </summary>
        /// <param name="horrorCount">恐怖値</param>
        /// <param name="horrorCountMax">恐怖値最大</param>
        /// <param name="horrorGaugeSlider">恐怖ゲージのスライダー</param>
        private void SetHorrorGaugeSlider(float horrorCount, float horrorCountMax, Slider horrorGaugeSlider)
        {
            var horror = horrorCount / horrorCountMax;
            horrorGaugeSlider.value = horror;
        }

        /// <summary>
        /// 心音プロパティ構造体を取得
        /// </summary>
        /// <param name="horrorCount">恐怖値</param>
        /// <param name="horrorCountMax">恐怖値最大</param>
        /// <param name="heartBeatPropStructs">心音プロパティ構造体</param>
        /// <return>心音プロパティ構造体</return>
        private HeartBeatPropStruct GetHeartBeatPropStruct(float horrorCount, float horrorCountMax, HeartBeatPropStruct[] heartBeatPropStructs)
        {
            // 小数点以下2桁に丸め込む
            var horror = Mathf.Round((horrorCount / horrorCountMax) * 100f) / 100f;
            var heartBeatPropStruct = heartBeatPropStructs.FirstOrDefault(x => x.from <= horror &&
                horror <= x.to);

            return heartBeatPropStruct;
        }

        /// <summary>
        /// 恐怖値を監視してゲームオーバー演出を実行
        /// </summary>
        /// <param name="horrorCount">恐怖値</param>
        /// <param name="horrorCountMax">恐怖値最大</param>
        /// <param name="player">RewiredのPlayer</param>
        /// <param name="fadeImageView">フェードイメージのビュー</param>
        /// <param name="_script_XyloApi">シロさんのコンポーネントへアクセスするAPI</param>
        private void CheckHorrorCountAndDoDirectionGameOver(float horrorCount, float horrorCountMax, Player player, FadeImageView fadeImageView, Script_xyloApi _script_XyloApi)
        {
            if (horrorCountMax <= horrorCount)
            {
                DirectionGameOver(player, fadeImageView, _script_XyloApi);
            }
        }

        /// <summary>
        /// ゲームオーバー演出
        /// </summary>
        /// <param name="player">RewiredのPlayer</param>
        /// <param name="fadeImageView">フェードイメージのビュー</param>
        /// <param name="_script_XyloApi">シロさんのコンポーネントへアクセスするAPI</param>
        private void DirectionGameOver(Player player, FadeImageView fadeImageView, Script_xyloApi _script_XyloApi = null)
        {
            // 時間を停止
            Time.timeScale = 0f;
            player.controllers.maps.SetMapsEnabled(false, "Default"); // ゲーム操作を無効化
            if (_script_XyloApi != null)
            {
                _script_XyloApi.SetMicrophoneActive(false);
            }
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
