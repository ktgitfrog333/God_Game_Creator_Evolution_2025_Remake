using Rewired;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenController : MonoBehaviour
{
    [Header("初期画面")]
    public Button gameStartButton; // "Game Start"ボタン

    [Header("メインメニュー")]
    public Button beginGameButton;  // "ゲーム開始"ボタン
    public Button optionsButton;    // "オプション"ボタン
    public Button exitGameButton;   // "ゲーム終了"ボタン

    [Header("オプション画面")]
    public Slider bgmVolumeSlider;      // BGMボリュームスライダー
    public Slider seVolumeSlider;       // SEボリュームスライダー
    public Slider vibrationSlider;      // 振動ON/OFFスライダー
    public Slider microphoneSlider;     // マイクON/OFFスライダー
    public Slider micVolumeSlider;      // マイクボリューム表示用スライダー（選択不可）
    public Button backButton;           // 戻るボタン

    [Header("オプション表示UI")]
    public GameObject VibOn1;          // 振動OFF時にハイライト
    public GameObject VibOn2;          // 振動ON時にハイライト
    public GameObject MicOn1;          // マイクOFF時にハイライト
    public GameObject MicOn2;          // マイクON時にハイライト

    [Header("フォーカス表示Obi")]
    public GameObject FirstObi1;       // 初期画面フォーカス表示
    public GameObject TitleObi1;       // メインメニュー：ゲーム開始フォーカス表示
    public GameObject TitleObi2;       // メインメニュー：オプションフォーカス表示
    public GameObject TitleObi3;       // メインメニュー：ゲーム終了フォーカス表示
    public GameObject Option1;         // オプション：BGMボリュームフォーカス表示
    public GameObject Option2;         // オプション：SEボリュームフォーカス表示
    public GameObject Option3;         // オプション：振動設定フォーカス表示
    public GameObject Option4;         // オプション：マイク設定フォーカス表示
    public GameObject Option5;         // オプション：戻るボタンフォーカス表示

    [Header("フェード設定")]
    public Image White;                 // フェード用の白いイメージ
    public float fadeDuration = 1.0f;   // フェード時間（秒）
    public string gameSceneName = "GameScene"; // ゲーム開始時に遷移するシーン名

    [Header("ハイライト設定")]
    public Color highlightColor = Color.yellow;     // ハイライト時の色
    public Color normalColor = Color.white;         // 通常時の色
    public float highlightScale = 1.1f;             // ハイライト時のスケール
    public bool useScaleEffect = true;              // スケール効果を使用するか
    public bool useCustomColors = false;            // カスタムカラーを使用するか（falseの場合はColorBlockを使用）

    [Header("メニューパネル")]
    public GameObject initialPanel;     // 初期画面のパネル
    public GameObject mainMenuPanel;    // メインメニューのパネル
    public GameObject optionsPanel;     // オプション画面のパネル

    [Header("マイク入力システム連携")]
    public MicInput_Criware micInputSystem; // マイク入力システムの参照

    [Header("デバッグ設定")]
    public bool enableDebugMode = true; // デバッグログの有効/無効

    private Player player; // プレイヤーの入力
    private int currentFocus = 0; // 現在フォーカスされているボタン/スライダーのインデックス
    private Selectable[] currentMenuItems; // 現在アクティブなメニューの選択可能要素配列
    private MenuState currentMenuState = MenuState.Initial; // 現在のメニュー状態
    private bool isFading = false; // フェード中かどうかのフラグ

    // メニュー状態の列挙型
    public enum MenuState
    {
        Initial,    // 初期画面
        MainMenu,   // メインメニュー
        Options     // オプション画面
    }

    void Start()
    {
        if (enableDebugMode)
        {
            Debug.Log("=== タイトル画面システム開始 ===");
        }

        // フェードイン開始
        StartCoroutine(FadeInOnStart());

        // Rewiredプレイヤーを取得
        player = ReInput.players.GetPlayer(0);

        if (player != null)
        {
            if (enableDebugMode)
            {
                Debug.Log("[OK] Rewiredプレイヤー取得成功！");
                LogControllerInfo();
            }
        }
        else
        {
            Debug.LogError("[NG] Rewiredプレイヤーが取得できませんでした！");
            return;
        }

        // オプション画面の初期設定
        SetupOptionsSliders();

        // セーブデータから設定を読み込み
        LoadOptions();

        // 初期状態を設定
        SetupInitialState();

        // 起動時にアクションの存在確認
        if (enableDebugMode)
        {
            CheckAllActionsOnStart();
        }
    }

    void Update()
    {
        if (player == null || isFading) return; // フェード中は入力を無効化

        // デバッグ用の生入力チェック
        if (enableDebugMode)
        {
            CheckRawInputs();
            CheckRewiredActions();
        }

        // Rewiredアクションをチェック
        CheckRewiredActionsForNavigation();

        // キーボード入力も確認
        CheckKeyboardInputs();
    }

    #region フェード機能

    /// <summary>
    /// ゲーム開始時のフェードイン処理
    /// </summary>
    private IEnumerator FadeInOnStart()
    {
        if (White == null)
        {
            Debug.LogError("[Fade] White Imageが設定されていません！");
            yield break;
        }

        isFading = true;

        // 最初は完全に白（不透明）
        Color startColor = White.color;
        startColor.a = 1.0f;
        White.color = startColor;
        White.gameObject.SetActive(true);

        if (enableDebugMode)
        {
            Debug.Log("[Fade] フェードイン開始");
        }

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / fadeDuration);
            Color currentColor = White.color;
            currentColor.a = alpha;
            White.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 完全に透明にして非表示
        Color finalColor = White.color;
        finalColor.a = 0.0f;
        White.color = finalColor;
        White.gameObject.SetActive(false);

        isFading = false;

        if (enableDebugMode)
        {
            Debug.Log("[Fade] フェードイン完了");
        }
    }

    /// <summary>
    /// フェードアウト処理（シーン遷移用）
    /// </summary>
    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (White == null)
        {
            Debug.LogError("[Fade] White Imageが設定されていません！");
            yield break;
        }

        isFading = true;

        // フェードアウト開始
        White.gameObject.SetActive(true);
        Color startColor = White.color;
        startColor.a = 0.0f;
        White.color = startColor;

        if (enableDebugMode)
        {
            Debug.Log("[Fade] フェードアウト開始 - シーン遷移: " + sceneName);
        }

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / fadeDuration);
            Color currentColor = White.color;
            currentColor.a = alpha;
            White.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 完全に白（不透明）
        Color finalColor = White.color;
        finalColor.a = 1.0f;
        White.color = finalColor;

        if (enableDebugMode)
        {
            Debug.Log("[Fade] フェードアウト完了 - シーンを読み込み中...");
        }

        // シーン遷移
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// フェードアウト処理（ゲーム終了用）
    /// </summary>
    private IEnumerator FadeOutAndQuitGame()
    {
        if (White == null)
        {
            Debug.LogError("[Fade] White Imageが設定されていません！");
            yield break;
        }

        isFading = true;

        // フェードアウト開始
        White.gameObject.SetActive(true);
        Color startColor = White.color;
        startColor.a = 0.0f;
        White.color = startColor;

        if (enableDebugMode)
        {
            Debug.Log("[Fade] フェードアウト開始 - ゲーム終了");
        }

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / fadeDuration);
            Color currentColor = White.color;
            currentColor.a = alpha;
            White.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 完全に白（不透明）
        Color finalColor = White.color;
        finalColor.a = 1.0f;
        White.color = finalColor;

        if (enableDebugMode)
        {
            Debug.Log("[Fade] フェードアウト完了 - ゲーム終了");
        }

        // ゲーム終了
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    void SetupOptionsSliders()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Setup] オプションスライダーを初期設定中...");
        }

        // BGMボリューム（0-10の11段階）
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.minValue = 0;
            bgmVolumeSlider.maxValue = 10;
            bgmVolumeSlider.wholeNumbers = true;
            bgmVolumeSlider.value = 7; // デフォルト値
        }

        // SEボリューム（0-10の11段階）
        if (seVolumeSlider != null)
        {
            seVolumeSlider.minValue = 0;
            seVolumeSlider.maxValue = 10;
            seVolumeSlider.wholeNumbers = true;
            seVolumeSlider.value = 7; // デフォルト値
        }

        // 振動ON/OFF（0か1の2段階）
        if (vibrationSlider != null)
        {
            vibrationSlider.minValue = 0;
            vibrationSlider.maxValue = 1;
            vibrationSlider.wholeNumbers = true;
            vibrationSlider.value = 1; // デフォルトはON
        }

        // マイクON/OFF（0か1の2段階）
        if (microphoneSlider != null)
        {
            microphoneSlider.minValue = 0;
            microphoneSlider.maxValue = 1;
            microphoneSlider.wholeNumbers = true;
            microphoneSlider.value = 0; // デフォルトはOFF
        }

        // マイクボリューム表示用（選択不可・値の設定は外部に委譲）
        if (micVolumeSlider != null)
        {
            micVolumeSlider.minValue = 0;
            micVolumeSlider.maxValue = 10;
            micVolumeSlider.wholeNumbers = true;
            // 値の設定は外部のマイクシステムに委譲
            micVolumeSlider.interactable = false; // 選択不可に設定
        }

        if (enableDebugMode)
        {
            Debug.Log("[Setup] オプションスライダー初期設定完了");
        }
    }

    // オプション設定の保存（TitleScreenManagerから移植）
    private void SaveOptions()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Options] 設定を保存中...");
        }

        // BGMボリュームを保存
        if (bgmVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("VolumeMain", bgmVolumeSlider.value / 10f); // 0-1の範囲に変換
            if (enableDebugMode)
                Debug.Log("[Save] BGMボリューム: " + bgmVolumeSlider.value);
        }

        // SEボリュームを保存
        if (seVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("VolumeSE", seVolumeSlider.value / 10f); // 0-1の範囲に変換
            if (enableDebugMode)
                Debug.Log("[Save] SEボリューム: " + seVolumeSlider.value);
        }

        // 振動設定を保存
        if (vibrationSlider != null)
        {
            PlayerPrefs.SetInt("VibrationEnabled", (int)vibrationSlider.value);
            if (enableDebugMode)
                Debug.Log("[Save] 振動設定: " + (vibrationSlider.value == 1 ? "ON" : "OFF"));
        }

        // マイク設定を保存
        if (microphoneSlider != null)
        {
            PlayerPrefs.SetInt("MicrophoneEnabled", (int)microphoneSlider.value);
            if (enableDebugMode)
                Debug.Log("[Save] マイク設定: " + (microphoneSlider.value == 1 ? "ON" : "OFF"));
        }

        // マイクボリュームの保存は外部のマイクシステムに委譲

        PlayerPrefs.Save();

        if (enableDebugMode)
        {
            Debug.Log("[Options] 設定を保存しました");
        }
    }

    // オプション設定の読み込み（TitleScreenManagerから移植）
    private void LoadOptions()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Options] 設定を読み込み中...");
        }

        // BGMボリュームを読み込み
        if (bgmVolumeSlider != null)
        {
            float savedBGM = PlayerPrefs.GetFloat("VolumeMain", 0.7f); // デフォルト0.7
            bgmVolumeSlider.value = savedBGM * 10f; // 0-10の範囲に変換
            if (enableDebugMode)
                Debug.Log("[Load] BGMボリューム: " + bgmVolumeSlider.value);
        }

        // SEボリュームを読み込み
        if (seVolumeSlider != null)
        {
            float savedSE = PlayerPrefs.GetFloat("VolumeSE", 0.7f); // デフォルト0.7
            seVolumeSlider.value = savedSE * 10f; // 0-10の範囲に変換
            if (enableDebugMode)
                Debug.Log("[Load] SEボリューム: " + seVolumeSlider.value);
        }

        // 振動設定を読み込み
        if (vibrationSlider != null)
        {
            int savedVibration = PlayerPrefs.GetInt("VibrationEnabled", 1); // デフォルトON
            vibrationSlider.value = savedVibration;
            if (enableDebugMode)
                Debug.Log("[Load] 振動設定: " + (vibrationSlider.value == 1 ? "ON" : "OFF"));
        }

        // マイク設定を読み込み
        if (microphoneSlider != null)
        {
            int savedMicrophone = PlayerPrefs.GetInt("MicrophoneEnabled", 0); // デフォルトOFF
            microphoneSlider.value = savedMicrophone;
            if (enableDebugMode)
                Debug.Log("[Load] マイク設定: " + (microphoneSlider.value == 1 ? "ON" : "OFF"));
        }

        // マイクボリュームの読み込みは外部のマイクシステムに委譲

        // UI表示を更新
        UpdateVibrationDisplay();
        UpdateMicrophoneDisplay();

        // マイク入力システムを初期化
        InitializeMicrophoneSystem();

        if (enableDebugMode)
        {
            Debug.Log("[Options] 設定を読み込みました");
        }
    }

    /// <summary>
    /// フォーカス表示Obiの更新
    /// </summary>
    private void UpdateFocusObiDisplay()
    {
        // すべてのObiを非表示にする
        HideAllObiObjects();

        // 現在の状態とフォーカスに応じてObiを表示
        switch (currentMenuState)
        {
            case MenuState.Initial:
                if (FirstObi1 != null)
                    FirstObi1.SetActive(true);
                if (enableDebugMode)
                    Debug.Log("[Obi Display] FirstObi1 を表示");
                break;

            case MenuState.MainMenu:
                GameObject[] titleObis = { TitleObi1, TitleObi2, TitleObi3 };
                if (currentFocus >= 0 && currentFocus < titleObis.Length && titleObis[currentFocus] != null)
                {
                    titleObis[currentFocus].SetActive(true);
                    if (enableDebugMode)
                        Debug.Log("[Obi Display] " + titleObis[currentFocus].name + " を表示 (フォーカス: " + currentFocus + ")");
                }
                break;

            case MenuState.Options:
                GameObject[] optionObis = { Option1, Option2, Option3, Option4, Option5 };
                if (currentFocus >= 0 && currentFocus < optionObis.Length && optionObis[currentFocus] != null)
                {
                    optionObis[currentFocus].SetActive(true);
                    if (enableDebugMode)
                        Debug.Log("[Obi Display] " + optionObis[currentFocus].name + " を表示 (フォーカス: " + currentFocus + ")");
                }
                break;
        }
    }

    /// <summary>
    /// すべてのObiオブジェクトを非表示にする
    /// </summary>
    private void HideAllObiObjects()
    {
        GameObject[] allObis = { FirstObi1, TitleObi1, TitleObi2, TitleObi3, Option1, Option2, Option3, Option4, Option5 };

        foreach (GameObject obi in allObis)
        {
            if (obi != null)
                obi.SetActive(false);
        }
    }

    /// <summary>
    /// 振動設定の表示を更新（ハイライト表示）
    /// </summary>
    private void UpdateVibrationDisplay()
    {
        if (vibrationSlider != null)
        {
            bool vibrationOn = vibrationSlider.value == 1;

            // VibOn1とVibOn2のハイライト切り替え
            SetButtonHighlight(VibOn1, !vibrationOn); // 振動OFFの時にVibOn1をハイライト
            SetButtonHighlight(VibOn2, vibrationOn);   // 振動ONの時にVibOn2をハイライト

            if (enableDebugMode)
            {
                Debug.Log("[Vibration Display] " + (vibrationOn ? "VibOn2ハイライト" : "VibOn1ハイライト"));
            }
        }
    }

    /// <summary>
    /// マイク設定の表示を更新（ハイライト表示）
    /// </summary>
    private void UpdateMicrophoneDisplay()
    {
        if (microphoneSlider != null)
        {
            bool microphoneOn = microphoneSlider.value == 1;

            // MicOn1とMicOn2のハイライト切り替え
            SetButtonHighlight(MicOn1, !microphoneOn); // マイクOFFの時にMicOn1をハイライト
            SetButtonHighlight(MicOn2, microphoneOn);   // マイクONの時にMicOn2をハイライト

            // マイクボリュームスライダーの表示/非表示
            if (micVolumeSlider != null)
                micVolumeSlider.gameObject.SetActive(microphoneOn);

            if (enableDebugMode)
            {
                Debug.Log("[Microphone Display] " + (microphoneOn ? "MicOn2ハイライト" : "MicOn1ハイライト"));
            }
        }
    }

    /// <summary>
    /// ボタンのハイライト状態を設定するメソッド
    /// </summary>
    /// <param name="buttonGameObject">ボタンのGameObject</param>
    /// <param name="highlight">ハイライトするかどうか</param>
    private void SetButtonHighlight(GameObject buttonGameObject, bool highlight)
    {
        if (buttonGameObject == null) return;

        Button button = buttonGameObject.GetComponent<Button>();
        if (button == null) return;

        if (useCustomColors)
        {
            // カスタムカラーを使用
            if (highlight)
            {
                // ハイライト状態
                if (button.image != null)
                    button.image.color = highlightColor;

                // スケール効果
                if (useScaleEffect)
                    buttonGameObject.transform.localScale = Vector3.one * highlightScale;
            }
            else
            {
                // 通常状態
                if (button.image != null)
                    button.image.color = normalColor;

                // スケールをリセット
                if (useScaleEffect)
                    buttonGameObject.transform.localScale = Vector3.one;
            }
        }
        else
        {
            // ボタンのColorBlockを使用
            ColorBlock colors = button.colors;

            if (highlight)
            {
                // ハイライト状態：selectedColorを使用
                if (button.image != null)
                    button.image.color = colors.selectedColor;

                // スケール効果
                if (useScaleEffect)
                    buttonGameObject.transform.localScale = Vector3.one * highlightScale;
            }
            else
            {
                // 通常状態：normalColorを使用
                if (button.image != null)
                    button.image.color = colors.normalColor;

                // スケールをリセット
                if (useScaleEffect)
                    buttonGameObject.transform.localScale = Vector3.one;
            }
        }
    }

    /// <summary>
    /// マイク設定が変更された時にマイク入力システムに通知
    /// </summary>
    private void NotifyMicrophoneSettingChanged()
    {
        if (micInputSystem != null && microphoneSlider != null)
        {
            bool micEnabled = microphoneSlider.value == 1;
            micInputSystem.SetMicrophoneActive(micEnabled);

            if (enableDebugMode)
            {
                Debug.Log("[Mic System] マイク設定を通知: " + (micEnabled ? "ON" : "OFF"));
            }
        }
    }

    /// <summary>
    /// ゲーム開始時にマイク入力システムを初期化
    /// </summary>
    private void InitializeMicrophoneSystem()
    {
        if (micInputSystem != null && microphoneSlider != null)
        {
            bool micEnabled = microphoneSlider.value == 1;
            micInputSystem.SetMicrophoneActive(micEnabled);

            if (enableDebugMode)
            {
                Debug.Log("[Mic System] 初期化時のマイク設定: " + (micEnabled ? "ON" : "OFF"));
            }
        }
    }

    void SetupInitialState()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Setup] 初期状態を設定中...");
        }

        // 初期画面を表示、他を非表示
        if (initialPanel != null)
            initialPanel.SetActive(true);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // 初期状態では"Game Start"ボタンのみフォーカス
        currentMenuState = MenuState.Initial;
        currentMenuItems = new Selectable[] { gameStartButton };
        currentFocus = 0;

        SetFocusToCurrentItem();
        if (enableDebugMode)
        {
            Debug.Log("[Setup] 初期状態設定完了 - Game Startボタンがフォーカス中");
        }
    }

    void ShowMainMenu()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Menu] メインメニューを表示中...");
        }

        // パネルの切り替え
        if (initialPanel != null)
            initialPanel.SetActive(false);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // メインメニューの状態に変更
        currentMenuState = MenuState.MainMenu;
        currentMenuItems = new Selectable[] { beginGameButton, optionsButton, exitGameButton };
        currentFocus = 0; // "ゲーム開始"ボタンにフォーカス

        SetFocusToCurrentItem();
        if (enableDebugMode)
        {
            Debug.Log("[Menu] メインメニュー表示完了 - ゲーム開始ボタンがフォーカス中");
        }
    }

    void ShowOptionsMenu()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Menu] オプション画面を表示中...");
        }

        // パネルの切り替え
        if (initialPanel != null)
            initialPanel.SetActive(false);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        // オプション画面の状態に変更
        currentMenuState = MenuState.Options;
        currentMenuItems = new Selectable[] {
           bgmVolumeSlider,
           seVolumeSlider,
           vibrationSlider,
           microphoneSlider, 
           // micVolumeSliderは選択不可なので除外
           backButton
       };
        currentFocus = 0; // BGMボリュームスライダーにフォーカス

        // オプション設定を読み込み（最新の値を反映）
        LoadOptions();

        SetFocusToCurrentItem();
        if (enableDebugMode)
        {
            Debug.Log("[Menu] オプション画面表示完了 - BGMボリュームスライダーがフォーカス中");
        }
    }

    void CheckRawInputs()
    {
        // デバッグ用：生のボタン入力をチェック
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
            }
            catch
            {
                // このアクションは存在しない
            }
        }
    }

    void CheckRewiredActionsForNavigation()
    {
        // UIUp - 上方向の移動
        if (player.GetButtonDown("UIUp"))
        {
            if (enableDebugMode)
            {
                Debug.Log("[REWIRED ACTION] UIUp が押されました！");
            }
            MoveFocusUp();
        }

        // UIDown - 下方向の移動
        if (player.GetButtonDown("UIDown"))
        {
            if (enableDebugMode)
            {
                Debug.Log("[REWIRED ACTION] UIDown が押されました！");
            }
            MoveFocusDown();
        }

        // UILeft - 左方向（スライダー値を減らす）
        if (player.GetButtonDown("UILeft"))
        {
            if (enableDebugMode)
            {
                Debug.Log("[REWIRED ACTION] UILeft が押されました！");
            }
            AdjustSliderValue(-1);
        }

        // UIRight - 右方向（スライダー値を増やす）
        if (player.GetButtonDown("UIRight"))
        {
            if (enableDebugMode)
            {
                Debug.Log("[REWIRED ACTION] UIRight が押されました！");
            }
            AdjustSliderValue(1);
        }

        // UISubmit - 決定
        if (player.GetButtonDown("UISubmit"))
        {
            if (enableDebugMode)
            {
                Debug.Log("[REWIRED ACTION] UISubmit が押されました！");
            }
            PressCurrentItem();
        }

        // UICancel - キャンセル（オプション画面から戻る）
        if (player.GetButtonDown("UICancel"))
        {
            if (enableDebugMode)
            {
                Debug.Log("[REWIRED ACTION] UICancel が押されました！");
            }
            if (currentMenuState == MenuState.Options)
            {
                OnBackPressed();
            }
        }
    }

    void CheckKeyboardInputs()
    {
        // キーボード入力
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (enableDebugMode)
            {
                Debug.Log("[KEYBOARD] 上矢印キー/Wキー");
            }
            MoveFocusUp();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (enableDebugMode)
            {
                Debug.Log("[KEYBOARD] 下矢印キー/Sキー");
            }
            MoveFocusDown();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (enableDebugMode)
            {
                Debug.Log("[KEYBOARD] 左矢印キー/Aキー");
            }
            AdjustSliderValue(-1);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (enableDebugMode)
            {
                Debug.Log("[KEYBOARD] 右矢印キー/Dキー");
            }
            AdjustSliderValue(1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (enableDebugMode)
            {
                Debug.Log("[KEYBOARD] 決定キー (Enter/Space)");
            }
            PressCurrentItem();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (enableDebugMode)
            {
                Debug.Log("[KEYBOARD] Escapeキー");
            }
            if (currentMenuState == MenuState.Options)
            {
                OnBackPressed();
            }
        }
    }

    void MoveFocusUp()
    {
        if (currentMenuItems == null || currentMenuItems.Length <= 1) return;

        if (enableDebugMode)
        {
            Debug.Log("[MoveFocusUp] 上方向の移動 - 現在のフォーカス: " + currentFocus);
        }

        // 上方向に移動（最上位で下端にループ）
        currentFocus--;
        if (currentFocus < 0)
        {
            currentFocus = currentMenuItems.Length - 1;
        }

        SetFocusToCurrentItem();
    }

    void MoveFocusDown()
    {
        if (currentMenuItems == null || currentMenuItems.Length <= 1) return;

        if (enableDebugMode)
        {
            Debug.Log("[MoveFocusDown] 下方向の移動 - 現在のフォーカス: " + currentFocus);
        }

        // 下方向に移動（最下位で上端にループ）
        currentFocus++;
        if (currentFocus >= currentMenuItems.Length)
        {
            currentFocus = 0;
        }

        SetFocusToCurrentItem();
    }

    void AdjustSliderValue(int direction)
    {
        if (currentMenuItems == null || currentFocus < 0 || currentFocus >= currentMenuItems.Length)
            return;

        Selectable currentItem = currentMenuItems[currentFocus];
        Slider slider = currentItem as Slider;

        if (slider != null && slider.interactable)
        {
            float newValue = slider.value + direction;
            slider.value = Mathf.Clamp(newValue, slider.minValue, slider.maxValue);

            if (enableDebugMode)
            {
                Debug.Log("[Slider Adjust] " + slider.name + " の値を " + slider.value + " に変更");
            }

            // スライダーの値が変更されたときの処理
            OnSliderValueChanged(slider);
        }
    }

    void OnSliderValueChanged(Slider slider)
    {
        // 各スライダーの値変更時の処理
        if (slider == bgmVolumeSlider)
        {
            if (enableDebugMode)
            {
                Debug.Log("[BGM Volume] " + slider.value);
            }
            // BGMボリューム変更処理をここに実装
        }
        else if (slider == seVolumeSlider)
        {
            if (enableDebugMode)
            {
                Debug.Log("[SE Volume] " + slider.value);
            }
            // SEボリューム変更処理をここに実装
        }
        else if (slider == vibrationSlider)
        {
            if (enableDebugMode)
            {
                Debug.Log("[Vibration] " + (slider.value == 1 ? "ON" : "OFF"));
            }
            // 振動設定変更処理をここに実装

            // 振動表示を更新
            UpdateVibrationDisplay();
        }
        else if (slider == microphoneSlider)
        {
            if (enableDebugMode)
            {
                Debug.Log("[Microphone] " + (slider.value == 1 ? "ON" : "OFF"));
            }
            // マイク設定変更処理をここに実装

            // マイク表示を更新
            UpdateMicrophoneDisplay();

            // マイク入力システムに設定変更を通知
            NotifyMicrophoneSettingChanged();
        }

        // 設定の自動保存
        SaveOptions();
    }

    void PressCurrentItem()
    {
        if (currentMenuItems == null || currentFocus < 0 || currentFocus >= currentMenuItems.Length)
            return;

        Selectable currentItem = currentMenuItems[currentFocus];
        if (currentItem == null) return;

        if (enableDebugMode)
        {
            Debug.Log("[Item Press] " + currentItem.name + " が選択されました！");
        }

        // ボタンの場合
        Button button = currentItem as Button;
        if (button != null)
        {
            if (button == gameStartButton)
            {
                OnGameStartPressed();
            }
            else if (button == beginGameButton)
            {
                OnBeginGamePressed();
            }
            else if (button == optionsButton)
            {
                OnOptionsPressed();
            }
            else if (button == exitGameButton)
            {
                OnExitGamePressed();
            }
            else if (button == backButton)
            {
                OnBackPressed();
            }

            // ボタンのOnClickイベントも呼び出し
            button.onClick.Invoke();
        }
    }

    void SetFocusToCurrentItem()
    {
        if (currentMenuItems == null || currentFocus < 0 || currentFocus >= currentMenuItems.Length)
            return;

        // 全ての要素のハイライトを解除
        for (int i = 0; i < currentMenuItems.Length; i++)
        {
            SetItemHighlight(currentMenuItems[i], false);
        }

        // 現在の要素を選択してハイライト
        Selectable focusedItem = currentMenuItems[currentFocus];
        if (focusedItem != null)
        {
            focusedItem.Select();
            SetItemHighlight(focusedItem, true);
            if (enableDebugMode)
            {
                Debug.Log("[Focus Change] " + focusedItem.name + " を選択しました！");
            }
        }

        // フォーカス表示Obiの更新
        UpdateFocusObiDisplay();
    }

    void SetItemHighlight(Selectable item, bool highlight)
    {
        if (item == null) return;

        ColorBlock colors = item.colors;

        if (highlight)
        {
            if (item.image != null)
                item.image.color = colors.selectedColor;
        }
        else
        {
            if (item.image != null)
                item.image.color = colors.normalColor;
        }
    }

    // ボタンが押された時の処理
    void OnGameStartPressed()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Game Start] メインメニューを表示します");
        }
        ShowMainMenu();
    }

    void OnBeginGamePressed()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Begin Game] ゲームを開始します！フェードアウト後にシーン遷移");
        }

        // フェードアウト後にシーン遷移
        if (!isFading)
        {
            StartCoroutine(FadeOutAndLoadScene(gameSceneName));
        }
    }

    void OnOptionsPressed()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Options] オプション画面を開きます");
        }
        ShowOptionsMenu();
    }

    void OnExitGamePressed()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Exit Game] ゲームを終了します！フェードアウト後に終了");
        }

        // フェードアウト後にゲーム終了
        if (!isFading)
        {
            StartCoroutine(FadeOutAndQuitGame());
        }
    }

    void OnBackPressed()
    {
        if (enableDebugMode)
        {
            Debug.Log("[Back] メインメニューに戻ります - 設定を保存");
        }

        // オプション画面から戻る時に設定を保存
        SaveOptions();
        ShowMainMenu();
    }

    void LogControllerInfo()
    {
        Debug.Log("接続中のジョイスティック数: " + player.controllers.joystickCount);
        Debug.Log("キーボードが利用可能: " + (player.controllers.Keyboard != null));
        Debug.Log("マウスが利用可能: " + (player.controllers.Mouse != null));

        for (int i = 0; i < player.controllers.joystickCount; i++)
        {
            var joystick = player.controllers.Joysticks[i];
            Debug.Log("ジョイスティック " + i + ": " + joystick.name + " (ボタン数: " + joystick.buttonCount + ", 軸数: " + joystick.axisCount + ")");
        }
    }

    void CheckAllActionsOnStart()
    {
        Debug.Log("=== 起動時アクション存在確認 ===");

        string[] actionNames = {
          "UIUp", "UIDown", "UILeft", "UIRight", "UISubmit", "UICancel",
          "UIVertical", "UIHorizontal"
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

    [ContextMenu("強制メインメニュー表示")]
    void ForceShowMainMenu()
    {
        ShowMainMenu();
    }

    [ContextMenu("強制オプション表示")]
    void ForceShowOptions()
    {
        ShowOptionsMenu();
    }

    [ContextMenu("初期状態にリセット")]
    void ResetToInitialState()
    {
        SetupInitialState();
    }

    [ContextMenu("設定を強制保存")]
    void ForceSaveOptions()
    {
        SaveOptions();
    }

    [ContextMenu("設定を再読み込み")]
    void ForceLoadOptions()
    {
        LoadOptions();
    }

    [ContextMenu("振動表示を更新")]
    void ForceUpdateVibrationDisplay()
    {
        UpdateVibrationDisplay();
    }

    [ContextMenu("マイク表示を更新")]
    void ForceUpdateMicrophoneDisplay()
    {
        UpdateMicrophoneDisplay();
    }

    [ContextMenu("全ハイライトをリセット")]
    void ResetAllHighlights()
    {
        // すべてのオプションボタンのハイライトをリセット
        SetButtonHighlight(VibOn1, false);
        SetButtonHighlight(VibOn2, false);
        SetButtonHighlight(MicOn1, false);
        SetButtonHighlight(MicOn2, false);

        if (enableDebugMode)
        {
            Debug.Log("[Reset] すべてのハイライトをリセットしました");
        }
    }

    [ContextMenu("フォーカスObi表示を更新")]
    void ForceUpdateFocusObiDisplay()
    {
        UpdateFocusObiDisplay();
    }

    [ContextMenu("すべてのObiを非表示")]
    void ForceHideAllObiObjects()
    {
        HideAllObiObjects();
    }

    [ContextMenu("フェードインテスト")]
    void TestFadeIn()
    {
        StartCoroutine(FadeInOnStart());
    }

    [ContextMenu("フェードアウトテスト（シーン遷移なし）")]
    void TestFadeOut()
    {
        if (!isFading && White != null)
        {
            StartCoroutine(TestFadeOutCoroutine());
        }
    }

    private IEnumerator TestFadeOutCoroutine()
    {
        isFading = true;
        White.gameObject.SetActive(true);

        Color startColor = White.color;
        startColor.a = 0.0f;
        White.color = startColor;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / fadeDuration);
            Color currentColor = White.color;
            currentColor.a = alpha;
            White.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Color finalColor = White.color;
        finalColor.a = 1.0f;
        White.color = finalColor;

        Debug.Log("[Test] フェードアウトテスト完了");
        isFading = false;
    }
}