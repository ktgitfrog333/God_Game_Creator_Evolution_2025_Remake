using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class UiInputSamp250531 : MonoBehaviour
{
    public Button button1; // 1番目のボタン
    public Button button2; // 2番目のボタン
    private Player player; // プレイヤーの入力
    private int currentFocus = 0; // どのボタンにフォーカスがあるか（0か1）

    void Start()
    {
        Debug.Log("=== デバッグモード開始 ===");

        player = ReInput.players.GetPlayer(0);

        if (player != null)
        {
            Debug.Log("[OK] Rewiredプレイヤー取得成功！");

            // 詳細な接続情報を表示
            Debug.Log("接続中のジョイスティック数: " + player.controllers.joystickCount);
            Debug.Log("キーボードが利用可能: " + (player.controllers.Keyboard != null));
            Debug.Log("マウスが利用可能: " + (player.controllers.Mouse != null));

            // 各ジョイスティックの詳細情報
            for (int i = 0; i < player.controllers.joystickCount; i++)
            {
                var joystick = player.controllers.Joysticks[i];
                Debug.Log("ジョイスティック " + i + ": " + joystick.name + " (ボタン数: " + joystick.buttonCount + ", 軸数: " + joystick.axisCount + ")");
            }
        }
        else
        {
            Debug.LogError("[NG] Rewiredプレイヤーが取得できませんでした！");
            return;
        }

        if (button1 != null && button2 != null)
        {
            Debug.Log("[OK] ボタン設定完了");
            SetFocusToButton1();
        }

        // 起動時にアクションの存在確認
        CheckAllActionsOnStart();
    }

    void Update()
    {
        if (player == null) return;

        // === まず、生のボタン入力をチェック ===
        CheckRawInputs();

        // === 次に、Rewiredアクションをチェック ===
        CheckRewiredActions();

        // === キーボード入力も確認 ===
        CheckKeyboardInputs();
    }

    void CheckRawInputs()
    {
        // 接続されている全ジョイスティックの生入力をチェック
        for (int joyIndex = 0; joyIndex < player.controllers.joystickCount; joyIndex++)
        {
            var joystick = player.controllers.Joysticks[joyIndex];

            // 全ボタンをチェック
            for (int buttonIndex = 0; buttonIndex < joystick.buttonCount; buttonIndex++)
            {
                if (joystick.GetButtonDown(buttonIndex))
                {
                    Debug.Log("[RAW INPUT] ジョイスティック" + joyIndex + " ボタン" + buttonIndex + " が押されました！");
                }
            }

            // 全軸をチェック
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
            "UIUp", "UIDown", "UILeft", "UIRight", "UISubmit",
            "UIVertical", "UIHorizontal"
        };

        foreach (string actionName in actionNames)
        {
            try
            {
                if (player.GetButtonDown(actionName))
                {
                    Debug.Log("[REWIRED ACTION] " + actionName + " が押されました！");

                    // アクションに応じた処理
                    if (actionName == "UIUp")
                    {
                        MoveFocusUp();
                    }
                    else if (actionName == "UIDown")
                    {
                        MoveFocusDown();
                    }
                    else if (actionName == "UISubmit")
                    {
                        PressCurrentButton();
                    }
                }

                // 軸の値もチェック
                float axisValue = player.GetAxis(actionName);
                if (Mathf.Abs(axisValue) > 0.5f)
                {
                    Debug.Log("[REWIRED AXIS] " + actionName + " 軸の値: " + axisValue.ToString("F2"));
                }
            }
            catch
            {
                // このアクションは存在しない（エラーログは出さない）
            }
        }
    }

    void CheckKeyboardInputs()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("[KEYBOARD] 上矢印キー");
            MoveFocusUp();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("[KEYBOARD] 下矢印キー");
            MoveFocusDown();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[KEYBOARD] 決定キー");
            PressCurrentButton();
        }

        // 何でもいいからキーが押されたら
        if (Input.anyKeyDown)
        {
            Debug.Log("[KEYBOARD] 何かのキーが押されました");
        }
    }

    void MoveFocusUp()
    {
        Debug.Log("[MoveFocusUp] 上方向の移動処理開始！現在のフォーカス: " + currentFocus);

        if (currentFocus == 1)
        {
            SetFocusToButton1();
        }
        else
        {
            Debug.Log("[Focus] すでにボタン1が選択中");
        }
    }

    void MoveFocusDown()
    {
        Debug.Log("[MoveFocusDown] 下方向の移動処理開始！現在のフォーカス: " + currentFocus);

        if (currentFocus == 0)
        {
            SetFocusToButton2();
        }
        else
        {
            Debug.Log("[Focus] すでにボタン2が選択中");
        }
    }

    void PressCurrentButton()
    {
        if (currentFocus == 0 && button1 != null)
        {
            Debug.Log("[Button Press] ボタン1がクリックされました！");
            button1.onClick.Invoke();
        }
        else if (currentFocus == 1 && button2 != null)
        {
            Debug.Log("[Button Press] ボタン2がクリックされました！");
            button2.onClick.Invoke();
        }
    }

    void SetFocusToButton1()
    {
        currentFocus = 0;
        button1.Select();
        SetButtonHighlight(button1, true);
        SetButtonHighlight(button2, false);
        Debug.Log("[Focus Change] ボタン1 を選択しました！");
    }

    void SetFocusToButton2()
    {
        currentFocus = 1;
        button2.Select();
        SetButtonHighlight(button1, false);
        SetButtonHighlight(button2, true);
        Debug.Log("[Focus Change] ボタン2 を選択しました！");
    }

    void SetButtonHighlight(Button button, bool highlight)
    {
        if (button == null) return;

        ColorBlock colors = button.colors;

        if (highlight)
        {
            button.image.color = colors.selectedColor;
        }
        else
        {
            button.image.color = colors.normalColor;
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

    [ContextMenu("今すぐ全入力チェック")]
    void ForceCheckAllInputs()
    {
        Debug.Log("=== 強制全入力チェック ===");
        CheckAllActionsOnStart();
        Debug.Log("コントローラーのボタンを押してみて、[RAW INPUT]が出るかチェック！");
    }
}