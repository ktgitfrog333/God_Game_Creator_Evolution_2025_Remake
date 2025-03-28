using CriWare;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LongSePicker : MonoBehaviour
{
    public static LongSePicker Instance { get; private set; }
    private CriAtomExAcb acbBomb;
    private List<CriAtomExPlayer> atomExPlayers;
    private bool ActiveNow = false;
    private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();
    private float cooldownTime = 0.06f; // 同じ音声の再生間隔
    private Queue<CriAtomExPlayer> availablePlayers = new Queue<CriAtomExPlayer>();
    private float Beat; //ビート関連の処理の為に一応作っている。現時点では使用していない
    private int playerIndex;
    //インスペクター上での設定項目
    public string CueSheetName;　//SEライブラリの情報
    public string AcbFilePath;　//同上
    public int maxPlayers = 10; // 再生機の数。限度を超えると最初のものが止まる。CRIWARE側で最大数を設定していればある程度は回避される
    //ここから先は個別のSEの登録
    public string IntroStage01;
    private void OnEnable()
    {
        Invoke("Delay", 1.2f);
    }
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


        PlayIntroStage01(1,2f);
    }
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
    private void PlaySound(string cueName, float volume, float startTimeSeconds = 0f)
    {
        if (!IsPlayable(cueName)) return;
        volume = Mathf.Clamp(volume, 0f, 1f); // 音量を制限
        CriAtomExPlayer player = GetNextPlayer();
        if (player == null)
        {
            Debug.LogWarning("Failed to retrieve a valid CriAtomExPlayer.");
            return;
        }
        player.SetVolume(volume);
        player.SetCue(acbBomb, cueName);

        // 開始位置を秒数で指定
        if (startTimeSeconds > 0f)
        {
            long startTimeMilliseconds = (long)(startTimeSeconds * 1000f); // float -> long に明示的に変換
            player.SetStartTime(startTimeMilliseconds); // ミリ秒単位で指定
        }

        player.Start();
    }
    //ここから先にSEを登録していく。音量は明示的に指定する
    public void PlayIntroStage01(float Vol, float startTimeSeconds = 0f)
    {
        PlaySound(IntroStage01, Vol, startTimeSeconds);
    }

    // 汎用的に任意のキューを再生開始位置指定付きで再生するメソッド
    public void PlayCueWithStartTime(string cueName, float Vol, float startTimeSeconds = 0f)
    {
        PlaySound(cueName, Vol, startTimeSeconds);
    }
}