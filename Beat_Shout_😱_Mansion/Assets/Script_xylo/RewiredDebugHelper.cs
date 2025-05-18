using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerManager : MonoBehaviour
{
    // Rewiredプレイヤー番号（通常は0）
    [SerializeField] private int playerId = 0;

    // 最初に選択するUIオブジェクト
    [SerializeField] private GameObject firstSelected;

    private Player player;
    private bool initialized = false;

    void Awake()
    {
        Invoke("DelaySet", 2);
    }

    void DelaySet()
    {
        // ReInputの初期化を待つ
        ReInput.InitializedEvent += OnRewiredInitialized;
    }

    void OnDestroy()
    {
        // イベントの登録解除
        ReInput.InitializedEvent -= OnRewiredInitialized;
    }

    void OnRewiredInitialized()
    {
        // Rewiredプレイヤーを取得
        player = ReInput.players.GetPlayer(playerId);
        initialized = true;

        Debug.Log("Rewiredが初期化されました。プレイヤー: " + player.name);

        // 最初のUIオブジェクトを選択
        SelectFirstUI();
    }

    void Start()
    {
        // すでに初期化されている場合はStart時に設定
        if (initialized)
        {
            SelectFirstUI();
        }

        // ControllerのUIInputが正しく設定されているか確認
        if (player != null)
        {
            foreach (var joystick in player.controllers.Joysticks)
            {
                Debug.Log($"ジョイスティック: {joystick.name} (ID: {joystick.id})");
                Debug.Log($"ボタン数: {joystick.buttonCount}");
            }
        }
    }

    void Update()
    {
        if (!initialized || player == null) return;

        // UIアクションのデバッグ
        DebugUIInputs();

        // 選択オブジェクトがない場合は強制的に再選択
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null &&
            firstSelected != null)
        {
            SelectFirstUI();
        }
    }

    void SelectFirstUI()
    {
        if (firstSelected == null || EventSystem.current == null) return;

        // 現在の選択を解除してから再選択（これが重要）
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);

        Debug.Log("UIの初期選択を設定: " + firstSelected.name);
    }

    void DebugUIInputs()
    {
        // UIアクションのチェック
        if (player.GetButtonDown("UISubmit"))
        {
            Debug.Log("UISubmitボタンが押されました");
        }

        if (player.GetButtonDown("UICancel"))
        {
            Debug.Log("UICancelボタンが押されました");
        }

        // 水平・垂直入力
        float h = player.GetAxis("UIHorizontal");
        float v = player.GetAxis("UIVertical");

        if (Mathf.Abs(h) > 0.5f)
        {
            Debug.Log("UIHorizontal: " + h);
        }

        if (Mathf.Abs(v) > 0.5f)
        {
            Debug.Log("UIVertical: " + v);
        }

        // すべてのボタン入力をチェック
        for (int i = 0; i < player.controllers.joystickCount; i++)
        {
            Joystick joystick = player.controllers.Joysticks[i];

            for (int j = 0; j < joystick.buttonCount; j++)
            {
                if (joystick.GetButtonDown(j))
                {
                    Debug.Log($"ジョイスティック {joystick.name} ボタン {j} が押されました");
                }
            }
        }
    }
}