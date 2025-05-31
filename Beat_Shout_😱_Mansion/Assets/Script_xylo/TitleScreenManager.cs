using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Rewired;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    // プレイヤー参照
    private Player player;

    // UI画面管理
    [Header("UI Panels")]
    public GameObject startButton;
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject exitConfirmPanel;

    // メインメニュー選択肢
    [Header("Main Menu Items")]
    public Selectable startGameButton;
    public Selectable optionsButton;
    public Selectable exitButton;

    // オプションメニュー項目
    [Header("Options Items")]
    public Slider[] optionSliders; // Vol, Se, Mic の3つのスライダー
    public Selectable backButton;

    // 終了確認
    [Header("Exit Confirmation")]
    public Selectable yesButton;
    public Selectable noButton;

    // ゲーム開始設定
    [Header("Game Settings")]
    public string gameSceneName = "GameScene";

    // 現在のメニュー状態
    public enum MenuState { StartScreen, MainMenu, Options, ExitConfirm }
    private MenuState currentState;

    [Header("Fade Settings")]
    public Image FadeImage;
    public float fadeDuration = 1.5f; // フェード時間

    [Header("Debug Settings")]
    public bool enableDebugMode = true; // デバッグモードの有効/無効

    void Start()
    {
        if (enableDebugMode)
        {
            Debug.Log("=== TitleScreenManager デバッグモード開始 ===");
        }

        // フェードイメージが割り当てられていることを確認
        if (FadeImage == null)
        {
            Debug.LogError("FadeImage is not assigned!");
            return;
        }

        // ゲーム開始時に不透明から透明へフェードイン
        StartCoroutine(FadeIn());

        // Rewiredプレイヤーの初期化
        InitializeRewiredPlayer();

        // 初期状態の設定
        SetMenuState(MenuState.StartScreen);

        // 選択オブジェクトの設定
        EventSystem.current.SetSelectedGameObject(startButton);
    }

    void InitializeRewiredPlayer()
    {
        player = ReInput.players.GetPlayer(0);

        if (player != null)
        {
            if (enableDebugMode)
            {
                Debug.Log("[OK] Rewiredプレイヤー取得成功！");
                Debug.Log("接続中のジョイスティック数: " + player.controllers.joystickCount);
                Debug.Log("キーボードが利用可能: " + (player.controllers.Keyboard != null));
                Debug.Log("マウスが利用可能: " + (player.controllers.Mouse != null));

                // 各ジョイスティックの詳細情報
                for (int i = 0; i < player.controllers.joystickCount; i++)
                {
                    var joystick = player.controllers.Joysticks[i];
                    Debug.Log("ジョイスティック " + i + ": " + joystick.name + " (ボタン数: " + joystick.buttonCount + ", 軸数: " + joystick.axisCount + ")");
                }

                // 起動時にアクションの存在確認
                CheckAllActionsOnStart();
            }
        }
        else
        {
            Debug.LogError("[NG] Rewiredプレイヤーが取得できませんでした！");
        }
    }

    void Update()
    {
        if (player == null)
        {
            if (enableDebugMode)
            {
                Debug.Log("[ERROR] プレイヤーがnullです！");
            }
            return;
        }

        // デバッグモードの場合、詳細な入力チェック
        if (enableDebugMode)
        {
            CheckRawInputs();
            CheckRewiredActions();
        }

        // 各種入力の検出（詳細なデバッグ付き）
        bool confirmPressed = GetConfirmInput();
        bool cancelPressed = GetCancelInput();
        bool upPressed = GetUpInput();
        bool downPressed = GetDownInput();
        bool leftPressed = GetLeftInput();
        bool rightPressed = GetRightInput();

        // 入力が検出された場合のデバッグログ
        if (enableDebugMode)
        {
            if (confirmPressed) Debug.Log("[INPUT DETECTED] Confirm入力が検出されました！");
            if (cancelPressed) Debug.Log("[INPUT DETECTED] Cancel入力が検出されました！");
            if (upPressed) Debug.Log("[INPUT DETECTED] Up入力が検出されました！");
            if (downPressed) Debug.Log("[INPUT DETECTED] Down入力が検出されました！");
            if (leftPressed) Debug.Log("[INPUT DETECTED] Left入力が検出されました！");
            if (rightPressed) Debug.Log("[INPUT DETECTED] Right入力が検出されました！");
        }

        // 現在の状態に応じた入力処理
        switch (currentState)
        {
            case MenuState.StartScreen:
                if (enableDebugMode && (confirmPressed || Input.GetMouseButtonDown(0)))
                {
                    Debug.Log("[STATE] StartScreen → MainMenuへ移行します");
                }
                if (confirmPressed || Input.GetMouseButtonDown(0))
                {
                    SetMenuState(MenuState.MainMenu);
                }
                break;

            case MenuState.MainMenu:
                if (enableDebugMode && (upPressed || downPressed || confirmPressed || cancelPressed))
                {
                    Debug.Log("[STATE] MainMenuで入力処理を開始します");
                }
                HandleMainMenuInput(upPressed, downPressed, confirmPressed, cancelPressed);
                break;

            case MenuState.Options:
                if (enableDebugMode && (upPressed || downPressed || leftPressed || rightPressed || confirmPressed || cancelPressed))
                {
                    Debug.Log("[STATE] Optionsで入力処理を開始します");
                }
                HandleOptionsInput(upPressed, downPressed, leftPressed, rightPressed, confirmPressed, cancelPressed);
                break;

            case MenuState.ExitConfirm:
                if (enableDebugMode && (leftPressed || rightPressed || confirmPressed || cancelPressed))
                {
                    Debug.Log("[STATE] ExitConfirmで入力処理を開始します");
                }
                HandleExitConfirmInput(leftPressed, rightPressed, confirmPressed, cancelPressed);
                break;
        }
    }

    // 各種入力検出メソッド（詳細なデバッグ付き）
    bool GetConfirmInput()
    {
        bool rewiredInput = false;
        bool keyboardInput = false;

        try
        {
            rewiredInput = player.GetButtonDown("UISubmit");
            if (enableDebugMode && rewiredInput)
            {
                Debug.Log("[INPUT SUCCESS] Rewired UISubmit が検出されました！");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugMode)
            {
                Debug.Log("[INPUT FAIL] Rewired UISubmit エラー: " + e.Message);
            }
        }

        keyboardInput = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        if (enableDebugMode && keyboardInput)
        {
            Debug.Log("[INPUT SUCCESS] キーボード Confirm が検出されました！");
        }

        return rewiredInput || keyboardInput;
    }

    bool GetCancelInput()
    {
        bool rewiredInput = false;
        bool keyboardInput = false;

        try
        {
            rewiredInput = player.GetButtonDown("UICancel");
            if (enableDebugMode && rewiredInput)
            {
                Debug.Log("[INPUT SUCCESS] Rewired UICancel が検出されました！");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugMode)
            {
                Debug.Log("[INPUT FAIL] Rewired UICancel エラー: " + e.Message);
            }
        }

        keyboardInput = Input.GetKeyDown(KeyCode.Escape);
        if (enableDebugMode && keyboardInput)
        {
            Debug.Log("[INPUT SUCCESS] キーボード Cancel が検出されました！");
        }

        return rewiredInput || keyboardInput;
    }

    bool GetUpInput()
    {
        bool rewiredInput = false;
        bool keyboardInput = false;

        try
        {
            rewiredInput = player.GetButtonDown("UIUp");
            if (enableDebugMode && rewiredInput)
            {
                Debug.Log("[INPUT SUCCESS] Rewired UIUp が検出されました！");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugMode)
            {
                Debug.Log("[INPUT FAIL] Rewired UIUp エラー: " + e.Message);
            }
        }

        keyboardInput = Input.GetKeyDown(KeyCode.UpArrow);
        if (enableDebugMode && keyboardInput)
        {
            Debug.Log("[INPUT SUCCESS] キーボード Up が検出されました！");
        }

        return rewiredInput || keyboardInput;
    }

    bool GetDownInput()
    {
        bool rewiredInput = false;
        bool keyboardInput = false;

        try
        {
            rewiredInput = player.GetButtonDown("UIDown");
            if (enableDebugMode && rewiredInput)
            {
                Debug.Log("[INPUT SUCCESS] Rewired UIDown が検出されました！");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugMode)
            {
                Debug.Log("[INPUT FAIL] Rewired UIDown エラー: " + e.Message);
            }
        }

        keyboardInput = Input.GetKeyDown(KeyCode.DownArrow);
        if (enableDebugMode && keyboardInput)
        {
            Debug.Log("[INPUT SUCCESS] キーボード Down が検出されました！");
        }

        return rewiredInput || keyboardInput;
    }

    bool GetLeftInput()
    {
        bool rewiredInput = false;
        bool keyboardInput = false;

        try
        {
            rewiredInput = player.GetButtonDown("UILeft");
            if (enableDebugMode && rewiredInput)
            {
                Debug.Log("[INPUT SUCCESS] Rewired UILeft が検出されました！");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugMode)
            {
                Debug.Log("[INPUT FAIL] Rewired UILeft エラー: " + e.Message);
            }
        }

        keyboardInput = Input.GetKeyDown(KeyCode.LeftArrow);
        if (enableDebugMode && keyboardInput)
        {
            Debug.Log("[INPUT SUCCESS] キーボード Left が検出されました！");
        }

        return rewiredInput || keyboardInput;
    }

    bool GetRightInput()
    {
        bool rewiredInput = false;
        bool keyboardInput = false;

        try
        {
            rewiredInput = player.GetButtonDown("UIRight");
            if (enableDebugMode && rewiredInput)
            {
                Debug.Log("[INPUT SUCCESS] Rewired UIRight が検出されました！");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugMode)
            {
                Debug.Log("[INPUT FAIL] Rewired UIRight エラー: " + e.Message);
            }
        }

        keyboardInput = Input.GetKeyDown(KeyCode.RightArrow);
        if (enableDebugMode && keyboardInput)
        {
            Debug.Log("[INPUT SUCCESS] キーボード Right が検出されました！");
        }

        return rewiredInput || keyboardInput;
    }

    // 各メニューの入力処理（詳細なデバッグ付き）
    void HandleMainMenuInput(bool up, bool down, bool confirm, bool cancel)
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (enableDebugMode)
        {
            Debug.Log("[MAIN MENU] 現在選択中: " + (currentSelected != null ? currentSelected.name : "null"));
        }

        if (up)
        {
            if (enableDebugMode) Debug.Log("[MAIN MENU] 上方向の入力を処理中...");

            if (currentSelected == exitButton.gameObject)
            {
                SelectUIElement(optionsButton);
            }
            else if (currentSelected == optionsButton.gameObject)
            {
                SelectUIElement(startGameButton);
            }
            else
            {
                // どのボタンも選択されていない場合の初期化
                SelectUIElement(startGameButton);
            }
        }
        else if (down)
        {
            if (enableDebugMode) Debug.Log("[MAIN MENU] 下方向の入力を処理中...");

            if (currentSelected == startGameButton.gameObject)
            {
                SelectUIElement(optionsButton);
            }
            else if (currentSelected == optionsButton.gameObject)
            {
                SelectUIElement(exitButton);
            }
            else
            {
                // どのボタンも選択されていない場合の初期化
                SelectUIElement(startGameButton);
            }
        }

        if (confirm)
        {
            if (enableDebugMode) Debug.Log("[MAIN MENU] 決定ボタンが押されました");

            if (currentSelected == startGameButton.gameObject)
            {
                if (enableDebugMode) Debug.Log("[MAIN MENU] ゲーム開始を実行");
                OnStartGameClicked();
            }
            else if (currentSelected == optionsButton.gameObject)
            {
                if (enableDebugMode) Debug.Log("[MAIN MENU] オプション画面を実行");
                OnOptionsClicked();
            }
            else if (currentSelected == exitButton.gameObject)
            {
                if (enableDebugMode) Debug.Log("[MAIN MENU] 終了確認を実行");
                OnExitClicked();
            }
            else
            {
                if (enableDebugMode) Debug.Log("[MAIN MENU] 不明なボタンが選択されています: " + (currentSelected != null ? currentSelected.name : "null"));
            }
        }
    }

    void HandleOptionsInput(bool up, bool down, bool left, bool right, bool confirm, bool cancel)
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (enableDebugMode)
        {
            Debug.Log("[OPTIONS] 現在選択中: " + (currentSelected != null ? currentSelected.name : "null"));
        }

        // スライダー間の移動
        if (up || down)
        {
            if (enableDebugMode) Debug.Log("[OPTIONS] 上下方向の入力を処理中...");

            int currentSliderIndex = -1;
            for (int i = 0; i < optionSliders.Length; i++)
            {
                if (currentSelected == optionSliders[i].gameObject)
                {
                    currentSliderIndex = i;
                    break;
                }
            }

            if (enableDebugMode) Debug.Log("[OPTIONS] 現在のスライダーインデックス: " + currentSliderIndex);

            if (up && currentSliderIndex > 0)
            {
                SelectUIElement(optionSliders[currentSliderIndex - 1]);
            }
            else if (down)
            {
                if (currentSliderIndex >= 0 && currentSliderIndex < optionSliders.Length - 1)
                {
                    SelectUIElement(optionSliders[currentSliderIndex + 1]);
                }
                else if (currentSliderIndex == optionSliders.Length - 1 || currentSliderIndex == -1)
                {
                    SelectUIElement(backButton);
                }
            }
            else if (up && currentSelected == backButton.gameObject && optionSliders.Length > 0)
            {
                SelectUIElement(optionSliders[optionSliders.Length - 1]);
            }
        }

        // スライダーの値調整
        if (left || right)
        {
            if (enableDebugMode) Debug.Log("[OPTIONS] 左右方向の入力を処理中...");

            for (int i = 0; i < optionSliders.Length; i++)
            {
                if (currentSelected == optionSliders[i].gameObject)
                {
                    float oldValue = optionSliders[i].value;
                    float change = right ? 0.1f : -0.1f;
                    optionSliders[i].value = Mathf.Clamp01(optionSliders[i].value + change);

                    if (enableDebugMode)
                    {
                        Debug.Log("[OPTIONS] スライダー" + i + " の値を変更: " + oldValue.ToString("F2") + " → " + optionSliders[i].value.ToString("F2"));
                    }
                    break;
                }
            }
        }

        if (confirm && currentSelected == backButton.gameObject)
        {
            if (enableDebugMode) Debug.Log("[OPTIONS] 戻るボタンが押されました");
            OnBackFromOptionsClicked();
        }

        if (cancel)
        {
            if (enableDebugMode) Debug.Log("[OPTIONS] キャンセルボタンが押されました");
            OnBackFromOptionsClicked();
        }
    }

    void HandleExitConfirmInput(bool left, bool right, bool confirm, bool cancel)
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (enableDebugMode)
        {
            Debug.Log("[EXIT CONFIRM] 現在選択中: " + (currentSelected != null ? currentSelected.name : "null"));
        }

        if (left || right)
        {
            if (enableDebugMode) Debug.Log("[EXIT CONFIRM] 左右方向の入力を処理中...");

            if (currentSelected == yesButton.gameObject)
            {
                SelectUIElement(noButton);
            }
            else if (currentSelected == noButton.gameObject)
            {
                SelectUIElement(yesButton);
            }
            else
            {
                // どちらも選択されていない場合の初期化
                SelectUIElement(noButton);
            }
        }

        if (confirm)
        {
            if (enableDebugMode) Debug.Log("[EXIT CONFIRM] 決定ボタンが押されました");

            if (currentSelected == yesButton.gameObject)
            {
                if (enableDebugMode) Debug.Log("[EXIT CONFIRM] ゲーム終了を実行");
                OnYesExitClicked();
            }
            else if (currentSelected == noButton.gameObject)
            {
                if (enableDebugMode) Debug.Log("[EXIT CONFIRM] 終了をキャンセル");
                OnNoExitClicked();
            }
        }

        if (cancel)
        {
            if (enableDebugMode) Debug.Log("[EXIT CONFIRM] キャンセルボタンが押されました");
            OnNoExitClicked();
        }
    }

    // UI要素選択のヘルパーメソッド（詳細なデバッグ付き）
    void SelectUIElement(Selectable element)
    {
        if (element != null)
        {
            EventSystem.current.SetSelectedGameObject(element.gameObject);
            if (enableDebugMode)
            {
                Debug.Log("[FOCUS CHANGE] " + element.name + " を選択しました！");
                Debug.Log("[FOCUS CHANGE] EventSystem.currentSelectedGameObject: " + (EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : "null"));
            }
        }
        else
        {
            if (enableDebugMode)
            {
                Debug.LogError("[FOCUS ERROR] 選択しようとした要素がnullです！");
            }
        }
    }

    // デバッグ用入力チェックメソッド（UiInputSamp250531から移植）
    void CheckRawInputs()
    {
        for (int joyIndex = 0; joyIndex < player.controllers.joystickCount; joyIndex++)
        {
            var joystick = player.controllers.Joysticks[joyIndex];

            for (int buttonIndex = 0; buttonIndex < joystick.buttonCount; buttonIndex++)
            {
                if (joystick.GetButtonDown(buttonIndex))
                {
                    Debug.Log("[RAW INPUT] ジョイスティック" + joyIndex + " ボタン" + buttonIndex + " が押されました！");
                }
            }

            for (int axisIndex = 0; axisIndex < joystick.axisCount; axisIndex++)
            {
                float axisValue = joystick.GetAxis(axisIndex);
                if (Mathf.Abs(axisValue) > 0.5f)
                {
                    Debug.Log("[RAW INPUT] ジョイスティック" + joyIndex + " 軸" + axisIndex + ": " + axisValue.ToString("F2"));
                }
            }
        }
    }

    void CheckRewiredActions()
    {
        string[] actionNames = {
            "UIUp", "UIDown", "UILeft", "UIRight", "UISubmit", "UICancel",
            "UIVertical", "UIHorizontal"
        };

        foreach (string actionName in actionNames)
        {
            try
            {
                if (player.GetButtonDown(actionName))
                {
                    Debug.Log("[REWIRED ACTION] " + actionName + " が押されました！");
                }

                float axisValue = player.GetAxis(actionName);
                if (Mathf.Abs(axisValue) > 0.5f)
                {
                    Debug.Log("[REWIRED AXIS] " + actionName + " 軸の値: " + axisValue.ToString("F2"));
                }
            }
            catch
            {
                // このアクションは存在しない
            }
        }
    }

    void CheckAllActionsOnStart()
    {
        Debug.Log("=== 起動時アクション存在確認 ===");

        string[] actionNames = {
            "UIUp", "UIDown", "UILeft", "UIRight", "UISubmit", "UICancel",
            "UIVertical", "UIHorizontal",
            "Up", "Down", "Left", "Right", "Submit", "Cancel",
            "Vertical", "Horizontal"
        };

        foreach (string actionName in actionNames)
        {
            try
            {
                float value = player.GetAxis(actionName);
                Debug.Log("[存在する] アクション「" + actionName + "」");
            }
            catch
            {
                Debug.Log("[存在しない] アクション「" + actionName + "」");
            }
        }
    }

    // メニュー状態の変更
    public void SetMenuState(MenuState newState)
    {
        currentState = newState;
        if (enableDebugMode)
        {
            Debug.Log("[State Change] 新しいパネル: " + newState);
        }

        // すべてのパネルを非表示
        startButton.SetActive(false);
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        exitConfirmPanel.SetActive(false);

        // 新しい状態に応じたパネルを表示
        switch (newState)
        {
            case MenuState.StartScreen:
                startButton.SetActive(true);
                EventSystem.current.SetSelectedGameObject(startButton);
                break;

            case MenuState.MainMenu:
                mainMenuPanel.SetActive(true);
                SelectUIElement(startGameButton);
                LoadOptions(); // オプション画面から戻った時に設定を反映
                break;

            case MenuState.Options:
                optionsPanel.SetActive(true);
                LoadOptions(); // オプション設定を読み込み
                if (optionSliders.Length > 0)
                {
                    SelectUIElement(optionSliders[0]);
                }
                break;

            case MenuState.ExitConfirm:
                exitConfirmPanel.SetActive(true);
                SelectUIElement(noButton);
                break;
        }
    }

    // UI要素から呼び出すボタン関数
    public void OnStartButtonClicked()
    {
        if (SE_Picker.Instance != null)
        {
            SE_Picker.Instance.PlayFootStep(1);
        }
        SetMenuState(MenuState.MainMenu);
    }

    public void OnStartGameClicked()
    {
        if (SE_Picker.Instance != null)
        {
            SE_Picker.Instance.PlayFootStep(1);
        }
        StartCoroutine(LoadSceneWithFade(gameSceneName));
    }

    public void OnOptionsClicked()
    {
        if (SE_Picker.Instance != null)
        {
            SE_Picker.Instance.PlayFootStep(1);
        }
        SetMenuState(MenuState.Options);
    }

    public void OnExitClicked()
    {
        if (SE_Picker.Instance != null)
        {
            SE_Picker.Instance.PlayFootStep(1);
        }
        SetMenuState(MenuState.ExitConfirm);
    }

    public void OnBackFromOptionsClicked()
    {
        if (SE_Picker.Instance != null)
        {
            SE_Picker.Instance.PlayFootStep(1);
        }
        SaveOptions();
        SetMenuState(MenuState.MainMenu);
    }

    public void OnYesExitClicked()
    {
        if (SE_Picker.Instance != null)
        {
            SE_Picker.Instance.PlayFootStep(1);
        }
        StartCoroutine(QuitWithFade());
    }

    public void OnNoExitClicked()
    {
        if (SE_Picker.Instance != null)
        {
            SE_Picker.Instance.PlayFootStep(1);
        }
        SetMenuState(MenuState.MainMenu);
    }

    // オプション設定の保存
    private void SaveOptions()
    {
        if (optionSliders.Length >= 3)
        {
            PlayerPrefs.SetFloat("VolumeMain", optionSliders[0].value);
            PlayerPrefs.SetFloat("VolumeSE", optionSliders[1].value);
            PlayerPrefs.SetFloat("VolumeMic", optionSliders[2].value);
            PlayerPrefs.Save();

            if (enableDebugMode)
            {
                Debug.Log("[Options] 設定を保存しました");
            }
        }
    }

    // オプション設定の読み込み
    private void LoadOptions()
    {
        if (optionSliders.Length >= 3)
        {
            optionSliders[0].value = PlayerPrefs.GetFloat("VolumeMain", 0.75f);
            optionSliders[1].value = PlayerPrefs.GetFloat("VolumeSE", 0.75f);
            optionSliders[2].value = PlayerPrefs.GetFloat("VolumeMic", 0.75f);

            if (enableDebugMode)
            {
                Debug.Log("[Options] 設定を読み込みました");
            }
        }
    }

    // フェードイン処理（不透明から透明へ）
    private IEnumerator FadeIn()
    {
        FadeImage.gameObject.SetActive(true);
        FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 1f);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 0f);
        FadeImage.gameObject.SetActive(false);
    }

    // フェードアウト処理（透明から不透明へ）
    private IEnumerator FadeOut()
    {
        FadeImage.gameObject.SetActive(true);
        FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 0f);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 1f);
    }

    // シーン読み込みとフェード処理
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }

    // ゲーム終了処理とフェード
    private IEnumerator QuitWithFade()
    {
        yield return StartCoroutine(FadeOut());
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // デバッグ用コンテキストメニュー
    [ContextMenu("今すぐ全入力チェック")]
    void ForceCheckAllInputs()
    {
        if (player != null)
        {
            Debug.Log("=== 強制全入力チェック ===");
            CheckAllActionsOnStart();
            Debug.Log("コントローラーのボタンを押してみて、[RAW INPUT]が出るかチェック！");
        }
        else
        {
            Debug.LogError("プレイヤーが初期化されていません！");
        }
    }
}