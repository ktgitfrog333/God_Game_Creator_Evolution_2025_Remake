using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rewired;
using System.Collections.Generic;

public class OptionsMenuController : MonoBehaviour
{
    // スライダーと戻るボタン
    public Slider[] sliders;
    public Button backButton;

    // ハイライト色
    public Color highlightColor = new Color(1f, 0.8f, 0.2f);

    // 値の変化量
    public float stepSize = 0.1f;

    // 選択インデックス（0-2: スライダー、3: 戻るボタン）
    private int currentIndex = 0;
    private int itemCount;

    // Rewiredプレイヤー
    private Player rewiredPlayer;

    // コントローラー情報
    private Joystick joystick;

    void OnEnable()
    {
        // Rewiredイベントリスナーを追加
        ReInput.ControllerConnectedEvent += OnControllerConnected;
        ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;

        // 初期化
        Initialize();

        // UI要素にイベントハンドラーを設定
        SetupUIEvents();

        Debug.Log("OptionsMenuController有効化");
    }

    void OnDisable()
    {
        // Rewiredイベントリスナーを解除
        ReInput.ControllerConnectedEvent -= OnControllerConnected;
        ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
    }

    void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        Debug.Log("コントローラーが接続されました: " + args.controllerId);
        // プレイヤーとジョイスティックの再取得
        UpdatePlayerAndJoystick();
    }

    void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
    {
        Debug.Log("コントローラーが切断されました: " + args.controllerId);
        // プレイヤーとジョイスティックの再取得
        UpdatePlayerAndJoystick();
    }

    void UpdatePlayerAndJoystick()
    {
        // システムプレイヤーを取得
        rewiredPlayer = ReInput.players.GetSystemPlayer();
        if (rewiredPlayer == null && ReInput.players.playerCount > 0)
        {
            rewiredPlayer = ReInput.players.GetPlayer(0);
        }

        // 接続されたジョイスティックを取得
        if (rewiredPlayer != null && rewiredPlayer.controllers.joystickCount > 0)
        {
            joystick = rewiredPlayer.controllers.Joysticks[0];
            Debug.Log("ジョイスティック取得: " + joystick.name);
        }
        else
        {
            joystick = null;
            Debug.Log("接続されたジョイスティックはありません");
        }
    }

    void Initialize()
    {
        // プレイヤーとジョイスティックを初期化
        UpdatePlayerAndJoystick();

        // アイテム数の計算
        itemCount = sliders.Length + 1; // スライダー + 戻るボタン
        Debug.Log("アイテム数: " + itemCount);

        // 最初のスライダーを選択
        currentIndex = 0;
        SelectItem(0);
        Debug.Log("初期化完了");
    }

    void SetupUIEvents()
    {
        // 各スライダーにクリックイベントを追加
        for (int i = 0; i < sliders.Length; i++)
        {
            int index = i; // ローカル変数にコピー

            // スライダーの値が変更されたときに選択状態も更新
            sliders[i].onValueChanged.AddListener((value) => {
                if (currentIndex != index)
                {
                    Debug.Log("スライダー" + index + "が操作されたため選択");
                    SelectItem(index);
                }
            });

            // クリック検出用のイベントトリガーを追加
            EventTrigger trigger = sliders[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = sliders[i].gameObject.AddComponent<EventTrigger>();
            }

            // トリガーリストが初期化されていることを確認
            if (trigger.triggers == null)
            {
                trigger.triggers = new List<EventTrigger.Entry>();
            }

            // ポインターダウンイベントを追加
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;

            entry.callback.AddListener((data) => {
                Debug.Log("スライダー" + index + "がクリックされました");
                SelectItem(index);
            });

            trigger.triggers.Add(entry);
        }

        // バックボタンにもイベントを追加
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => {
                SelectItem(sliders.Length);
                TitleScreenManager manager = FindAnyObjectByType<TitleScreenManager>();
                if (manager != null)
                {
                    manager.OnBackFromOptionsClicked();
                }
            });
        }
    }

    void Update()
    {
        // 上下キーでアイテム間の移動
        bool upPressed = false;
        bool downPressed = false;
        bool leftPressed = false;
        bool rightPressed = false;
        bool confirmPressed = false;

        // Rewiredからの入力取得
        if (rewiredPlayer != null)
        {
            // 大文字小文字を修正: UiVertical → UIVertical
            float verticalValue = rewiredPlayer.GetAxisRaw("UIVertical");
            float horizontalValue = rewiredPlayer.GetAxisRaw("UIHorizontal");

            upPressed = verticalValue > 0 && rewiredPlayer.GetButtonDown("UIVertical");
            downPressed = verticalValue < 0 && rewiredPlayer.GetButtonDown("UIVertical");
            leftPressed = horizontalValue < 0 && rewiredPlayer.GetButtonDown("UIHorizontal");
            rightPressed = horizontalValue > 0 && rewiredPlayer.GetButtonDown("UIHorizontal");
            confirmPressed = rewiredPlayer.GetButtonDown("Submit");

            // 入力デバッグ
            if (verticalValue != 0) Debug.Log("垂直軸の値: " + verticalValue);
            if (horizontalValue != 0) Debug.Log("水平軸の値: " + horizontalValue);
            if (confirmPressed) Debug.Log("決定ボタン押下");
        }
        else
        {
            // フォールバック: 標準入力
            upPressed = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            downPressed = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
            leftPressed = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            rightPressed = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
            confirmPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        }

        // デバッグログで入力を確認
        if (upPressed) Debug.Log("上キー押下");
        if (downPressed) Debug.Log("下キー押下");
        if (leftPressed) Debug.Log("左キー押下");
        if (rightPressed) Debug.Log("右キー押下");

        // 移動処理
        if (upPressed)
        {
            int newIndex = (currentIndex - 1 + itemCount) % itemCount;
            Debug.Log("上キー: " + currentIndex + " -> " + newIndex);
            SelectItem(newIndex);
        }
        else if (downPressed)
        {
            int newIndex = (currentIndex + 1) % itemCount;
            Debug.Log("下キー: " + currentIndex + " -> " + newIndex);
            SelectItem(newIndex);
        }

        // スライダー値の変更
        if (currentIndex < sliders.Length)
        {
            float changeAmount = 0f;

            if (leftPressed)
            {
                changeAmount = -stepSize;
            }
            else if (rightPressed)
            {
                changeAmount = stepSize;
            }
            // 数字キーでの直接入力（1キーで10%、2キーで20%...）
            else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                sliders[currentIndex].value = 0.1f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                sliders[currentIndex].value = 0.2f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                sliders[currentIndex].value = 0.3f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                sliders[currentIndex].value = 0.4f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                sliders[currentIndex].value = 0.5f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                sliders[currentIndex].value = 0.6f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            {
                sliders[currentIndex].value = 0.7f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
            {
                sliders[currentIndex].value = 0.8f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
            {
                sliders[currentIndex].value = 0.9f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                sliders[currentIndex].value = 1.0f;
            }

            if (changeAmount != 0f)
            {
                sliders[currentIndex].value = Mathf.Clamp(
                    sliders[currentIndex].value + changeAmount,
                    sliders[currentIndex].minValue,
                    sliders[currentIndex].maxValue
                );
            }
        }

        // 戻るボタンの決定
        if (currentIndex == sliders.Length && confirmPressed)
        {
            Debug.Log("戻るボタンで決定キー押下");

            // TitleScreenManagerを取得して直接メソッドを呼び出す
            TitleScreenManager manager = FindAnyObjectByType<TitleScreenManager>();
            if (manager != null)
            {
                Debug.Log("TitleScreenManagerのOnBackFromOptionsClickedを呼び出し");
                manager.OnBackFromOptionsClicked();
            }
            else
            {
                Debug.Log("TitleScreenManagerが見つからないため、直接backButton.onClickを呼び出し");
                backButton.onClick.Invoke();
            }
        }
    }

    void SelectItem(int index)
    {
        Debug.Log("SelectItem: " + currentIndex + " -> " + index);

        // 前の選択をリセット
        if (currentIndex < sliders.Length)
        {
            ResetHighlight(sliders[currentIndex].gameObject);
        }
        else if (currentIndex == sliders.Length && backButton != null)
        {
            ResetHighlight(backButton.gameObject);
        }

        // 新しい選択
        currentIndex = index;

        // 選択したアイテムをハイライト
        if (currentIndex < sliders.Length)
        {
            SetHighlight(sliders[currentIndex].gameObject);
            EventSystem.current.SetSelectedGameObject(sliders[currentIndex].gameObject);
            Debug.Log("スライダー" + currentIndex + "を選択");
        }
        else if (currentIndex == sliders.Length && backButton != null)
        {
            SetHighlight(backButton.gameObject);
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);
            Debug.Log("戻るボタンを選択");
        }
    }

    void SetHighlight(GameObject obj)
    {
        Debug.Log("ハイライト設定: " + obj.name);
        // UI要素をハイライト
        Image[] images = obj.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img.name.Contains("Background") || img.name.Contains("Handle") ||
                img.name.Contains("Fill") || img == obj.GetComponent<Image>())
            {
                img.color = highlightColor;
                Debug.Log("色変更: " + img.name);
            }
        }
    }

    void ResetHighlight(GameObject obj)
    {
        Debug.Log("ハイライトリセット: " + obj.name);
        // ハイライトをリセット
        Image[] images = obj.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            img.color = Color.white;
        }
    }
}