using DG.Tweening;
using ObservableCollections;
using R3;
using Rewired;
using Selects.Commons;
using Selects.ViewModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Views
{
    /// <summary>
    /// 共通UIのビュー
    /// </summary>
    public class CommonPanelView : MonoBehaviour
    {
        [Tooltip("CommonPanel > HeaderPanel > ControlGuidePanelPC をセット")]
        [SerializeField] private RectTransform controlGuidePanelPC;
        [Tooltip("CommonPanel > HeaderPanel > ControlGuidePanelXbox360Con をセット")]
        [SerializeField] private RectTransform controlGuidePanelXbox360Con;
        [Tooltip("CommonPanel > CenterPanel > EnterRoomPanel をセット")]
        [SerializeField] private RectTransform enterRoomPanel;
        [Tooltip("CommonPanel > CenterPanel > ReturnTitleScenePanel をセット")]
        [SerializeField] private RectTransform returnTitleScenePanel;
        [Tooltip("CommonPanel > CenterPanel > ConfirmPanel をセット")]
        [SerializeField] private RectTransform confirmPanel;
        [Tooltip("CommonPanel > CenterPanel > ConfirmPanel > CursorIconImage をセット")]
        [SerializeField] private RectTransform cursorIconImage;
        [SerializeField] private float cursorIconImageのマージン;
        [SerializeField] private CommonPanelTemplateStruct 共通UIのテンプレート;
        [SerializeField] private LevelStruct[] レベル構造体リスト;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            if (controlGuidePanelPC == null)
                controlGuidePanelPC = transform.GetChild(0).GetChild(0) as RectTransform;
            if (controlGuidePanelXbox360Con == null)
                controlGuidePanelXbox360Con = transform.GetChild(0).GetChild(1) as RectTransform;
            if (enterRoomPanel == null)
                enterRoomPanel = transform.GetChild(1).GetChild(0) as RectTransform;
            if (returnTitleScenePanel == null)
                returnTitleScenePanel = transform.GetChild(1).GetChild(1) as RectTransform;
            if (confirmPanel == null)
                confirmPanel = transform.GetChild(1).GetChild(2) as RectTransform;
            if (cursorIconImage == null)
                cursorIconImage = transform.GetChild(1).GetChild(2).GetChild(2) as RectTransform;
        }

        private void Start()
        {
            controlGuidePanelXbox360Con.gameObject.SetActive(false);
            var player = ReInput.players.GetPlayer(0);
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    var inputType = GetInputType(player);
                    switch (inputType)
                    {
                        case 0:
                            controlGuidePanelPC.gameObject.SetActive(true);
                            controlGuidePanelXbox360Con.gameObject.SetActive(false);

                            break;
                        case 1:
                            controlGuidePanelPC.gameObject.SetActive(false);
                            controlGuidePanelXbox360Con.gameObject.SetActive(true);

                            break;
                    }
                })
                .AddTo(ref _disposableBag);
            CommonPanelViewModel viewModel = new CommonPanelViewModel();
            var enterRoomText = enterRoomPanel.GetComponentInChildren<TextMeshProUGUI>();
            // 移動先のステージ番号
            int targetStageIndex = -1;
            // YesText/NoText それぞれのボタン制御
            YesNoTextView[] yesNoTextViews = confirmPanel.GetComponentsInChildren<YesNoTextView>();
            Vector3 cursorIconImageDefaultPosition = cursorIconImage.transform.position;
            Observable.EveryUpdate()
                .Select(_ => viewModel.SelectedStageIndex)
                .Where(x => x != null)
                .Take(1)
                .Subscribe(selectedStageIndexReactive =>
                {
                    selectedStageIndexReactive.Subscribe(selectedStageIndex =>
                    {
                        switch (selectedStageIndex)
                        {
                            case -1:
                                if (enterRoomPanel.gameObject.activeSelf)
                                    enterRoomPanel.gameObject.SetActive(false);
                                if (returnTitleScenePanel.gameObject.activeSelf)
                                    returnTitleScenePanel.gameObject.SetActive(false);
                                if (confirmPanel.gameObject.activeSelf)
                                    confirmPanel.gameObject.SetActive(false);
                                if (Time.timeScale != 1f)
                                    Time.timeScale = 1f;

                                break;
                            case 5:
                                if (enterRoomPanel.gameObject.activeSelf)
                                    enterRoomPanel.gameObject.SetActive(false);
                                if (!returnTitleScenePanel.gameObject.activeSelf)
                                    returnTitleScenePanel.gameObject.SetActive(true);
                                cursorIconImage.transform.position = cursorIconImageDefaultPosition;
                                if (!confirmPanel.gameObject.activeSelf)
                                    confirmPanel.gameObject.SetActive(true);
                                if (Time.timeScale != 0f)
                                    Time.timeScale = 0f;

                                break;
                            default:
                                RenderEnterRoomText(enterRoomText, selectedStageIndex, 共通UIのテンプレート.enterRoomText, レベル構造体リスト);
                                if (!enterRoomPanel.gameObject.activeSelf)
                                    enterRoomPanel.gameObject.SetActive(true);
                                if (returnTitleScenePanel.gameObject.activeSelf)
                                    returnTitleScenePanel.gameObject.SetActive(false);
                                cursorIconImage.transform.position = cursorIconImageDefaultPosition;
                                if (!confirmPanel.gameObject.activeSelf)
                                    confirmPanel.gameObject.SetActive(true);
                                if (Time.timeScale != 0f)
                                    Time.timeScale = 0f;

                                break;
                        }
                        targetStageIndex = selectedStageIndex;
                    })
                    .AddTo(ref _disposableBag);
                })
                .AddTo(ref _disposableBag);
            FadeImageView fadeImageView = FindAnyObjectByType<FadeImageView>();
            foreach (var yesNoTextView in yesNoTextViews.Select((p, i) => new { Content = p, Index = i }))
            {
                yesNoTextView.Content.EventState.Subscribe(eventSatte =>
                {
                    switch (eventSatte)
                    {
                        case EnumEventCommand.Selected:
                            cursorIconImage.DOMove(yesNoTextViews[yesNoTextView.Index].transform.position + Vector3.left * cursorIconImageのマージン, .35f)
                                .SetUpdate(true);

                            break;
                        case EnumEventCommand.Submited:
                            DoLoadSceneOrResetSelectedStageIndex(yesNoTextView.Index, targetStageIndex, yesNoTextViews, fadeImageView, viewModel);

                            break;
                        case EnumEventCommand.Canceled:
                            ResetSelectedStageIndex(viewModel);

                            break;
                    }
                })
                    .AddTo(ref _disposableBag);
            }
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// 入力タイプを取得
        /// </summary>
        /// <param name="player">Rewiredのplayer</param>
        /// <returns>入力タイプ</returns>
        private int GetInputType(Player player)
        {
            var last = player.controllers.GetLastActiveController();
            if (last != null && last.type == ControllerType.Joystick)
            {
                return 1; // コントローラー
            }
            else
            {
                return 0; // キーボード／マウス
            }
        }

        /// <summary>
        /// ステージ名称を変換して再描画
        /// </summary>
        /// <param name="enterRoomText">ステージ名称表示用のTextMeshPro</param>
        /// <param name="selectedStageIndex">選択ステージ番号</param>
        /// <param name="enterRoomTextTemplate">ステージ名称のテンプレート</param>
        /// <param name="levelStructs">レベル構造体リスト</param>
        private void RenderEnterRoomText(TextMeshProUGUI enterRoomText, int selectedStageIndex, string enterRoomTextTemplate, LevelStruct[] levelStructs)
        {
            var levelStruct = levelStructs.FirstOrDefault(x => x.階層 == selectedStageIndex);
            var stageName = levelStruct.ステージ名称;
            enterRoomText.text = enterRoomTextTemplate.Replace("${stageName}", $"{stageName}");
        }

        /// <summary>
        /// シーンロードまたは、選択ステージ番号をリセット
        /// </summary>
        /// <param name="index">はい／いいえのインデックス</param>
        /// <param name="targetStageIndex">移動先のステージ番号</param>
        /// <param name="yesNoTextViews">YesText/NoText それぞれのボタン制御</param>
        /// <param name="fadeImageView">フェードイメージのビュー</param>
        /// <param name="viewModel">共通UIのビューモデル</param>
        /// <remarks>はい／いいえで処理を分岐<br/>
        /// <br/>
        /// [はい]：<br/>
        /// __●YesText/NoText それぞれのボタン制御を無効<br/>
        /// __●フェード処理<br/>
        /// __●遷移先の情報をセットしてセーブデータを保存<br/>
        /// __●移動先のステージ番号によって各ステージまたはタイトル画面のシーンをロード<br/>
        /// [いいえ]：<br/>
        /// __●ビューモデル経由で選択ステージ番号をリセット</remarks>
        private void DoLoadSceneOrResetSelectedStageIndex(int index, int targetStageIndex, YesNoTextView[] yesNoTextViews, FadeImageView fadeImageView, CommonPanelViewModel viewModel)
        {
            switch (index)
            {
                case 0:
                    foreach (var yesNoTextView in yesNoTextViews)
                    {
                        yesNoTextView.SetEnabled(false);
                    }
                    ReactiveProperty<int> startLoadCnt = new ReactiveProperty<int>();
                    startLoadCnt.Where(x => 1 < x)
                        .Subscribe(_ =>
                        {
                            Observable.Create<bool>(observer =>
                            {
                                string sceneName = string.Empty;
                                switch (targetStageIndex)
                                {
                                    case 5:
                                        sceneName = "TitleScene";

                                        break;
                                    default:
                                        sceneName = "MainScene";

                                        break;
                                }
                                StartCoroutine(LoadSceneCoroutine(observer, sceneName));
                                return Disposable.Empty;
                            })
                                .Subscribe(_ => { })
                                .AddTo(ref _disposableBag);
                        })
                        .AddTo(ref _disposableBag);
                    Observable.Create<bool>(observer =>
                    {
                        StartCoroutine(fadeImageView.PlayFadeInDirection(observer, 1.5f));
                        return Disposable.Empty;
                    })
                        .Subscribe(_ =>
                        {
                            startLoadCnt.Value++;
                        })
                        .AddTo(ref _disposableBag);
                    ResourcesUtility utility = new ResourcesUtility();
                    UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
                    userBean.sceneIdx = targetStageIndex;
                    utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, userBean);
                    startLoadCnt.Value++;

                    break;
                case 1:
                    ResetSelectedStageIndex(viewModel);

                    break;
            }
        }

        /// <summary>
        /// 選択ステージ番号をリセット
        /// </summary>
        /// <param name="viewModel">共通UIのビューモデル</param>
        private void ResetSelectedStageIndex(CommonPanelViewModel viewModel)
        {
            viewModel.SetSelectedStageIndex(-1);
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
