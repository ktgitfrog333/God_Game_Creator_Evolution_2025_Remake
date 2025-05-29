using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Rewired;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    // プレイヤー参照
    private Player rewiredPlayer;

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

    // 現在のメニュー状態 - publicに変更
    public enum MenuState { StartScreen, MainMenu, Options, ExitConfirm }
    private MenuState currentState;

    [Header("Fade Settings")]
    public Image FadeImage;
    public float fadeDuration = 1.5f; // フェード時間

    void Start()
    {
        // フェードイメージが割り当てられていることを確認
        if (FadeImage == null)
        {
            Debug.LogError("FadeImage is not assigned!");
            return;
        }

        // ゲーム開始時に不透明から透明へフェードイン
        StartCoroutine(FadeIn());

        // Rewiredプレイヤーの初期化
        rewiredPlayer = ReInput.players.GetSystemPlayer();
        if (rewiredPlayer == null && ReInput.players.playerCount > 0)
        {
            rewiredPlayer = ReInput.players.GetPlayer(0);
        }

        // 初期状態の設定
        SetMenuState(MenuState.StartScreen);

        // 選択オブジェクトの設定
        EventSystem.current.SetSelectedGameObject(startButton);
    }

    void Update()
    {
        // 決定ボタン入力の検出（Rewired）
        bool confirmPressed = false;
        if (rewiredPlayer != null)
        {
            confirmPressed = rewiredPlayer.GetButtonDown("UISubmit");
        }
        else
        {
            // フォールバック：標準入力
            confirmPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        }

        // 現在の状態に応じた入力処理
        switch (currentState)
        {
            case MenuState.StartScreen:
                if (confirmPressed || Input.GetMouseButtonDown(0))
                {
                    SetMenuState(MenuState.MainMenu);
                }
                break;

                // 他の状態の更新処理はSetMenuState内で行います
        }
    }

    // メニュー状態の変更
    public void SetMenuState(MenuState newState)
    {
        currentState = newState;
        Debug.Log("新しいパネル" + newState);
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
                EventSystem.current.SetSelectedGameObject(startGameButton.gameObject);
                break;

            case MenuState.Options:
                optionsPanel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(optionSliders[0].gameObject);
                break;

            case MenuState.ExitConfirm:
                exitConfirmPanel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(noButton.gameObject);
                break;
        }
    }

    // UI要素から呼び出すボタン関数
    public void OnStartButtonClicked()
    {
        SE_Picker.Instance.PlayFootStep(1);
        SetMenuState(MenuState.MainMenu);
    }

    public void OnStartGameClicked()
    {
        SE_Picker.Instance.PlayFootStep(1);
        StartCoroutine(LoadSceneWithFade(gameSceneName));
    }

    public void OnOptionsClicked()
    {
        SE_Picker.Instance.PlayFootStep(1);
        SetMenuState(MenuState.Options);
    }

    public void OnExitClicked()
    {
        SE_Picker.Instance.PlayFootStep(1);
        SetMenuState(MenuState.ExitConfirm);
    }

    public void OnBackFromOptionsClicked()
    {
        SE_Picker.Instance.PlayFootStep(1);
        SetMenuState(MenuState.MainMenu);
        SaveOptions();
    }

    public void OnYesExitClicked()
    {
        SE_Picker.Instance.PlayFootStep(1);
        StartCoroutine(QuitWithFade());
    }

    public void OnNoExitClicked()
    {
        SE_Picker.Instance.PlayFootStep(1);
        SetMenuState(MenuState.MainMenu);
    }

    // オプション設定の保存
    private void SaveOptions()
    {
        // PlayerPrefsにスライダー値を保存
        if (optionSliders.Length >= 3)
        {
            PlayerPrefs.SetFloat("VolumeMain", optionSliders[0].value);
            PlayerPrefs.SetFloat("VolumeSE", optionSliders[1].value);
            PlayerPrefs.SetFloat("VolumeMic", optionSliders[2].value);
            PlayerPrefs.Save();
        }
    }

    // オプション設定の読み込み（Start時に呼び出すことも可能）
    private void LoadOptions()
    {
        if (optionSliders.Length >= 3)
        {
            optionSliders[0].value = PlayerPrefs.GetFloat("VolumeMain", 0.75f);
            optionSliders[1].value = PlayerPrefs.GetFloat("VolumeSE", 0.75f);
            optionSliders[2].value = PlayerPrefs.GetFloat("VolumeMic", 0.75f);
        }
    }

    // フェードイン処理（不透明から透明へ）
    private IEnumerator FadeIn()
    {
        // フェードイメージが不透明になるように設定
        FadeImage.gameObject.SetActive(true);
        FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 1f);

        // 徐々に透明にする
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 完全に透明になったらゲームオブジェクトを非アクティブにする
        FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 0f);
        FadeImage.gameObject.SetActive(false);
    }

    // フェードアウト処理（透明から不透明へ）
    private IEnumerator FadeOut()
    {
        // フェードイメージを表示して透明に設定
        FadeImage.gameObject.SetActive(true);
        FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 0f);

        // 徐々に不透明にする
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 完全に不透明になるように設定
        FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 1f);
    }

    // シーン読み込みとフェード処理
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        // まず画面をフェードアウト（透明→不透明）
        yield return StartCoroutine(FadeOut());

        // シーンの読み込み
        SceneManager.LoadScene(sceneName);
    }

    // ゲーム終了処理とフェード
    private IEnumerator QuitWithFade()
    {
        // まず画面をフェードアウト（透明→不透明）
        yield return StartCoroutine(FadeOut());

        // ゲームの終了
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}