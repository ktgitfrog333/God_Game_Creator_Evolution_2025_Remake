using DG.Tweening;
using Mains.External;
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
        [Tooltip("CommonPanel > CenterPanel > InteractPanelPC をセット")]
        [SerializeField] private RectTransform interactPanelPC;
        [Tooltip("CommonPanel > CenterPanel > InteractPanelXbox360Con をセット")]
        [SerializeField] private RectTransform interactPanelXbox360Con;
        [Header("フェード設定")]
        /// <summary>前に戻るシーン名</summary>
        [SerializeField] private string gameSceneNameBack;
        /// <summary>共通UIのビューモデル</summary>
        private CommonPanelViewModel _viewModel;
        /// <summary>シロさんのコンポーネントへアクセスするAPI</summary>
        private Script_xyloApi _script_XyloApi;
        /// <summary>R3のリソース管理</summary>
        private readonly SerialDisposable _currentInputTypeDisposable = new SerialDisposable();
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
            foreach (Transform child in transform)
            {
                if (child.name.Equals("CenterPanel"))
                {
                    foreach (Transform item in child)
                    {
                        if (item.name.Equals("InteractPanelPC"))
                        {
                            if (interactPanelPC == null)
                                interactPanelPC = item as RectTransform;
                        }
                        if (item.name.Equals("InteractPanelXbox360Con"))
                        {
                            if (interactPanelXbox360Con == null)
                                interactPanelXbox360Con = item as RectTransform;
                        }
                    }
                }
            }
        }

        private void Start()
        {
            controlGuidePanelXbox360Con.gameObject.SetActive(false);
            interactPanelPC.gameObject.SetActive(false);
            interactPanelXbox360Con.gameObject.SetActive(false);
            var player = ReInput.players.GetPlayer(0);
            // 現在の入力タイプ
            ReactiveProperty<int> currentInputType = new ReactiveProperty<int>();
            Observable.EveryUpdate()
                .Select(_ => GetInputType(player))
                .DistinctUntilChanged()
                .Subscribe(inputType =>
                {
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
                    currentInputType.Value = inputType;
                })
                .AddTo(ref _disposableBag);
            _viewModel = new CommonPanelViewModel();
            var enterRoomText = enterRoomPanel.GetComponentInChildren<TextMeshProUGUI>();
            // 移動先のステージ番号
            int targetStageIndex = -1;
            // YesText/NoText それぞれのボタン制御
            YesNoTextView[] yesNoTextViews = confirmPanel.GetComponentsInChildren<YesNoTextView>();
            Vector3 cursorIconImageDefaultPosition = cursorIconImage.transform.position;
            Observable.EveryUpdate()
                .Select(_ => _viewModel.SelectedStageIndex)
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
                    // インタラクトパネルはステージが選択されたら非表示にする
                    selectedStageIndexReactive.Select(x => -1 < x)
                        .Pairwise()
                        .Select(x =>
                        {
                            if (!x.Previous &&
                                x.Current)
                            {
                                return (1, true);
                            } else if (x.Previous &&
                                !x.Current)
                            {
                                return (1, false);
                            }

                            return (0, false);
                        })
                        .Where(x => 0 < x.Item1)
                        .Select(x => x.Item2)
                        .Subscribe(isSelected =>
                        {
                            if (isSelected)
                            {
                                if (interactPanelPC.gameObject.activeSelf)
                                    interactPanelPC.gameObject.SetActive(false);
                                if (interactPanelXbox360Con.gameObject.activeSelf)
                                    interactPanelXbox360Con.gameObject.SetActive(false);
                            }
                            else
                            {
                                switch (currentInputType.Value)
                                {
                                    case 0:
                                        if (!interactPanelPC.gameObject.activeSelf)
                                            interactPanelPC.gameObject.SetActive(true);

                                        break;
                                    case 1:
                                        if (!interactPanelXbox360Con.gameObject.activeSelf)
                                            interactPanelXbox360Con.gameObject.SetActive(true);

                                        break;
                                }
                            }
                        })
                        .AddTo(ref _disposableBag);
                    selectedStageIndexReactive.Execute(-1);
                })
                .AddTo(ref _disposableBag);
            FadeImageView fadeImageView = FindAnyObjectByType<FadeImageView>();
            _script_XyloApi = new Script_xyloApi();
            foreach (var yesNoTextView in yesNoTextViews.Select((p, i) => new { Content = p, Index = i }))
            {
                yesNoTextView.Content.EventState.Subscribe(eventSatte =>
                {
                    switch (eventSatte)
                    {
                        case EnumEventCommand.Selected:
                            cursorIconImage.DOMove(yesNoTextViews[yesNoTextView.Index].transform.position + Vector3.left * cursorIconImageのマージン, .35f)
                                .SetUpdate(true);
                            _script_XyloApi.PlayBUB_Move3();

                            break;
                        case EnumEventCommand.Submited:
                            DoLoadSceneOrResetSelectedStageIndex(yesNoTextView.Index, targetStageIndex, レベル構造体リスト, gameSceneNameBack,
                                yesNoTextViews, fadeImageView, _viewModel, _script_XyloApi);

                            break;
                        case EnumEventCommand.Canceled:
                            ResetSelectedStageIndex(_viewModel);
                            _script_XyloApi.PlayBUB_Move1();

                            break;
                    }
                })
                    .AddTo(ref _disposableBag);
            }
            // 部屋の扉の前で調べる当たり判定に触れたら調べるコマンドUIを出す
            _viewModel.IsOnTriggerEnterSearchRangeIndex.Select(x => -1 < x)
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    // ステージが選択されていない間だけ切り替えを行う
                    _currentInputTypeDisposable.Disposable = currentInputType.Where(_ => targetStageIndex < 0)
                        .Subscribe(inputType =>
                    {
                        RenderInteractPanel(x, inputType, interactPanelPC, interactPanelXbox360Con);
                    });
                    RenderInteractPanel(x, currentInputType.Value, interactPanelPC, interactPanelXbox360Con);
                })
                .AddTo(ref _disposableBag);
            _viewModel.IsOnTriggerEnterSearchRangeIndex.Execute(-1);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            _viewModel?.Dispose();
            _currentInputTypeDisposable.Disposable = null;
            _script_XyloApi?.Dispose();
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
        /// <param name="levelStructs">レベル構造体リスト</param>
        /// <param name="gameSceneNameBack">前に戻るシーン名</param>
        /// <param name="yesNoTextViews">YesText/NoText それぞれのボタン制御</param>
        /// <param name="fadeImageView">フェードイメージのビュー</param>
        /// <param name="viewModel">共通UIのビューモデル</param>
        /// <param name="script_XyloApi">シロさんのコンポーネントへアクセスするAPI</param>
        /// <remarks>はい／いいえで処理を分岐<br/>
        /// <br/>
        /// [はい]：<br/>
        /// __●YesText/NoText それぞれのボタン制御を無効<br/>
        /// __●フェード処理<br/>
        /// __●遷移先の情報をセットしてセーブデータを保存<br/>
        /// __●移動先のステージ番号によって各ステージまたはタイトル画面のシーンをロード<br/>
        /// [いいえ]：<br/>
        /// __●ビューモデル経由で選択ステージ番号をリセット</remarks>
        private void DoLoadSceneOrResetSelectedStageIndex(int index, int targetStageIndex, LevelStruct[] levelStructs, string gameSceneNameBack,
            YesNoTextView[] yesNoTextViews, FadeImageView fadeImageView, CommonPanelViewModel viewModel,
            Script_xyloApi script_XyloApi)
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
                                        sceneName = gameSceneNameBack;

                                        break;
                                    default:
                                        sceneName = levelStructs.Where(x => x.階層 == targetStageIndex)
                                            .Select(x => x.ステージ名称_物理名)
                                            .FirstOrDefault();
                                        if (string.IsNullOrEmpty(sceneName))
                                        {
                                            Debug.LogWarning("ステージ名称_物理名のマッピング失敗のためステージ1を読み込みます");
                                            sceneName = "MainScene_Stage1_Living";
                                        }

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
                    script_XyloApi.PlayBUB_Submit3();
                    startLoadCnt.Value++;

                    break;
                case 1:
                    ResetSelectedStageIndex(viewModel);
                    script_XyloApi.PlayBUB_Move1();

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

        /// <summary>
        /// インタラクトパネルを描画
        /// </summary>
        /// <param name="isOnTriggerEnterSearchRange">部屋の扉の前で調べる当たり判定に触れたか</param>
        /// <param name="inputType">現在の入力タイプ</param>
        /// <param name="interactPanelPC">インタラクトパネルPC</param>
        /// <param name="interactPanelXbox360Con">インタラクトパネルXbox360コン</param>
        private void RenderInteractPanel(bool isOnTriggerEnterSearchRange, int inputType, RectTransform interactPanelPC, RectTransform interactPanelXbox360Con)
        {
            if (isOnTriggerEnterSearchRange)
            {
                switch (inputType)
                {
                    case 0:
                        if (!interactPanelPC.gameObject.activeSelf)
                            interactPanelPC.gameObject.SetActive(true);
                        if (interactPanelXbox360Con.gameObject.activeSelf)
                            interactPanelXbox360Con.gameObject.SetActive(false);

                        break;
                    case 1:
                        if (interactPanelPC.gameObject.activeSelf)
                            interactPanelPC.gameObject.SetActive(false);
                        if (!interactPanelXbox360Con.gameObject.activeSelf)
                            interactPanelXbox360Con.gameObject.SetActive(true);

                        break;
                }
            }
            else
            {
                if (interactPanelPC.gameObject.activeSelf)
                    interactPanelPC.gameObject.SetActive(false);
                if (interactPanelXbox360Con.gameObject.activeSelf)
                    interactPanelXbox360Con.gameObject.SetActive(false);
            }
        }
    }
}
