using CriWare;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SE音声を再生するための管理クラス
/// シングルトンパターンで実装されており、どこからでもアクセス可能
/// </summary>
public class SE_Picker : MonoBehaviour
{
    public static SE_Picker Instance { get; private set; }

    private CriAtomExAcb acbBomb;
    private List<CriAtomExPlayer> atomExPlayers;
    private bool ActiveNow = false;
    private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();
    private Queue<CriAtomExPlayer> availablePlayers = new Queue<CriAtomExPlayer>();
    private float Beat; // ビート関連の処理の為に一応作っている。現時点では使用していない
    private int playerIndex;

    // マスター音量の設定（AudioSettingsControllerから設定される）
    private float masterSEVolume = 1.0f;

    // インスペクター上での設定項目
    [Header("CRIWARE設定")]
    public string CueSheetName; // SEライブラリの情報
    public string AcbFilePath; // 同上
    public int maxPlayers = 10; // 再生機の数。限度を超えると最初のものが止まる。CRIWARE側で最大数を設定していればある程度は回避される

    [Header("SEキュー名")]
    public string FootStep; // 足音SE

    private void OnEnable()
    {
        Invoke("Delay", 1.2f);
    }

    /// <summary>
    /// 初期化を遅延実行するメソッド
    /// </summary>
    private void Delay()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        var cueSheetDmg = CriAtom.AddCueSheet(CueSheetName, AcbFilePath, "");
        if (cueSheetDmg == null || cueSheetDmg.acb == null)
        {
            Debug.LogError("Failed to load cue sheet or ACB file.");
            return;
        }

        acbBomb = cueSheetDmg.acb;
        atomExPlayers = new List<CriAtomExPlayer>();
        for (int i = 0; i < maxPlayers; i++)
        {
            var player = new CriAtomExPlayer();
            atomExPlayers.Add(player);
            availablePlayers.Enqueue(player);
        }

        ActiveNow = true;

        // 保存されているSE音量を読み込む
        LoadMasterSEVolume();
    }

    /// <summary>
    /// マスターSE音量を読み込むメソッド
    /// AudioSettingsManagerから設定を取得
    /// </summary>
    private void LoadMasterSEVolume()
    {
        // AudioSettingsManagerから設定を読み込む
        AudioSettingsData settings = AudioSettingsManager.LoadSettings();
        masterSEVolume = settings.seVolume;
        Debug.Log($"SE_Picker: マスターSE音量を読み込みました: {masterSEVolume:F1}");
    }

    /// <summary>
    /// マスターSE音量を設定するメソッド（AudioSettingsControllerから呼び出される）
    /// </summary>
    /// <param name="volume">設定する音量 (0.0 - 1.0)</param>
    public void SetMasterSEVolume(float volume)
    {
        masterSEVolume = Mathf.Clamp(volume, 0f, 1f);
        Debug.Log($"SE_Picker: マスターSE音量を設定しました: {masterSEVolume:F1}");
    }

    /// <summary>
    /// 次に使用するプレイヤーを取得するメソッド
    /// </summary>
    private CriAtomExPlayer GetNextPlayer()
    {
        if (availablePlayers.Count > 0)
        {
            var player = availablePlayers.Dequeue();
            availablePlayers.Enqueue(player);
            return player;
        }
        Debug.LogWarning("No available players; returning the first player.");
        return atomExPlayers[0];
    }

    /// <summary>
    /// 指定されたキュー名が再生可能かどうかを確認するメソッド
    /// </summary>
    private bool IsPlayable(string cueName)
    {
        if (string.IsNullOrEmpty(cueName))
        {
            Debug.LogError("Cue name is null or empty.");
            return false;
        }

        if (!ActiveNow || acbBomb == null)
        {
            Debug.LogWarning("Sound system is not active or ACB is null.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 音声を再生するメソッド
    /// </summary>
    /// <param name="cueName">再生するキュー名</param>
    /// <param name="volume">個別の音量 (0.0 - 1.0)</param>
    private void PlaySound(string cueName, float volume)
    {
        if (!IsPlayable(cueName)) return;

        // 個別の音量にマスター音量を乗算して最終的な音量を決定
        float finalVolume = volume * masterSEVolume;
        finalVolume = Mathf.Clamp(finalVolume, 0f, 1f); // 音量を制限

        CriAtomExPlayer player = GetNextPlayer();
        if (player == null)
        {
            Debug.LogWarning("Failed to retrieve a valid CriAtomExPlayer.");
            return;
        }

        player.SetVolume(finalVolume);
        player.SetCue(acbBomb, cueName);
        player.Start();
    }

    // ここから先にSEを登録していく。音量は明示的に指定する

    /// <summary>
    /// 足音SEを再生するメソッド
    /// </summary>
    /// <param name="volume">音量 (0.0 - 1.0)</param>
    public void PlayFootStep(float volume)
    {
        PlaySound(FootStep, volume);
    }
}