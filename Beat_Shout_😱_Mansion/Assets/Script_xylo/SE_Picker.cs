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
    private string FootStep; // 足音SE
    private string SubmitL;
    private string SubmitS;
    private string BatteryGet1;
    private string BatteryGet2;
    private string BatteryGet3;
    private string BatteryLost1;
    private string BatteryLost2;
    private string BatteryLost3;
    private string DoorOpen1;
    private string DoorOpen2;
    private string DoorOpen3;
    private string GhostLaugh1;
    private string GhostLaugh2;
    private string GhostLaugh3;
    private string GhostLaugh4;
    private string Cancel1;
    private string Cancel2;
    private string Cancel3;
    private string Cancel4;
    private string Cancel5;
    private string Submit1;
    private string Submit2;
    private string Submit3;
    private string Submit4;
    private string Submit5;
    private string Move1;
    private string Move2;
    private string Move3;
    private string Move4;
    private string Move5;
    private string Damage1;
    private string Damage2;
    private string Damage3;
    private string HitSuccess1;
    private string HitSuccess2;
    private string HitSuccess3;
    private string HitMiss1;
    private string HitMiss2;
    private string HitMiss3;
    private string HitMiss4;
    private string HitMiss5;
    private string HeartbeatFast;
    private string HeartbeatSlow;
    private string Shouchitsu;
    private string garakuta;
    // [2026/05/08] Amagata Support for Obake FBX variations and Obake voice variations start
    private string GhostLaughV2Normal;
    private string GhostLaughV2Fat;
    private string GhostLaughV2Chatter;
    // [2026/05/08] Amagata Support for Obake FBX variations and Obake voice variations end


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

   FootStep ="footstep"; // 足音SE
   SubmitL= "SubmitL_KoukaonLab";
   SubmitS= "SubmitS_KoukaonLab";
        BatteryGet1 = "BatteryGet1";
        BatteryGet2 = "BatteryGet2";
        BatteryGet3 = "BatteryGet3";
 BatteryLost1= "BatteryLost1";
BatteryLost2= "BatteryLost2";
 BatteryLost3 = "BatteryLost3";
 DoorOpen1 = "DoorOpen1";
 DoorOpen2 = "DoorOpen2";
 DoorOpen3 = "DoorOpen3";
 GhostLaugh1 = "GhostLaugh1";
 GhostLaugh2 = "GhostLaugh2";
 GhostLaugh3 = "GhostLaugh3";
GhostLaugh4 = "GhostLaugh4";
 Cancel1 = "Cancel1";
 Cancel2 = "Cancel2";
 Cancel3 = "Cancel3";
 Cancel4 = "Cancel4";
 Cancel5 = "Cancel5";
 Submit1 = "Submit1";
 Submit2 = "Submit2";
 Submit3 = "Submit3";
 Submit4 = "Submit4";
 Submit5 = "Submit5";
 Move1 = "Move1";
 Move2 = "Move2";
 Move3 = "Move3";
Move4 = "Move4";
 Move5 = "Move5";
 Damage1 = "Damage1";
 Damage2 = "Damage2";
 Damage3 = "Damage3";
 HitSuccess1 = "HitSuccess1";
 HitSuccess2 = "HitSuccess2";
 HitSuccess3 = "HitSuccess3";
 HitMiss1 = "HitMiss1";
        HitMiss2 = "HitMiss2";
        HitMiss3 = "HitMiss3";
        HitMiss4 = "HitMiss4";
        HitMiss5 = "HitMiss5";
        HeartbeatFast = "HeartbeatFast";
        HeartbeatSlow = "HeartbeatSlow";
        Shouchitsu = "Shouchitsu";
        garakuta = "garakuta";
        // [2026/05/08] Amagata Support for Obake FBX variations and Obake voice variations start
        GhostLaughV2Normal = "GhostLaughV2Normal";
        GhostLaughV2Fat = "GhostLaughV2Fat";
        GhostLaughV2Chatter = "GhostLaughV2Chatter";
        // [2026/05/08] Amagata Support for Obake FBX variations and Obake voice variations end


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

    /// <summary>
    /// 決定音（大）を再生するメソッド
    /// </summary>
    /// <param name="volume">音量 (0.0 - 1.0)</param>
    public void PlaySubmitL(float volume)
    {
        PlaySound(SubmitL, volume);
    }

    /// <summary>
    /// 決定音（小）を再生するメソッド
    /// </summary>
    /// <param name="volume">音量 (0.0 - 1.0)</param>
    public void PlaySubmitS(float volume)
    {
        PlaySound(SubmitS, volume);
    }
    public void PlayBatteryGet1(float volume)
    {
        PlaySound(BatteryGet1, volume);
    }
    public void PlayBatteryGet2(float volume)
    {
        PlaySound(BatteryGet2, volume);
    }
    public void PlayBatteryGet3(float volume)
    {
        PlaySound(BatteryGet3, volume);
    }
    public void PlayBatteryLost1(float volume)
    {
        PlaySound(BatteryLost1, volume);
    }
    public void PlayBatteryLost2(float volume)
    {
        PlaySound(BatteryLost2, volume);
    }
    public void PlayBatteryLost3(float volume)
    {
        PlaySound(BatteryLost3, volume);
    }
    public void PlayDoorOpen1(float volume)
    {
        PlaySound(DoorOpen1, volume);
    }
    public void PlayDoorOpen2(float volume)
    {
        PlaySound(DoorOpen2, volume);
    }
    public void PlayDoorOpen3(float volume)
    {
        PlaySound(DoorOpen3, volume);
    }
    public void PlayGhostLaugh1(float volume)
    {
        PlaySound(GhostLaugh1, volume);
    }
    public void PlayGhostLaugh2(float volume)
    {
        PlaySound(GhostLaugh2, volume);
    }
    public void PlayGhostLaugh3(float volume)
    {
        PlaySound(GhostLaugh3, volume);
    }
    public void PlayGhostLaugh4(float volume)
    {
        PlaySound(GhostLaugh4, volume);
    }

    public void PlayCancel1(float volume)
    {
        PlaySound(Cancel1, volume);
    }
    public void PlayCancel2(float volume)
    {
        PlaySound(Cancel2, volume);
    }
    public void PlayCancel3(float volume)
    {
        PlaySound(Cancel3, volume);
    }
    public void PlayCancel4(float volume)
    {
        PlaySound(Cancel4, volume);
    }
    public void PlayCancel5(float volume)
    {
        PlaySound(Cancel5, volume);
    }
    public void PlaySubmit1(float volume)
    {
        PlaySound(Submit1, volume);
    }
    public void PlaySubmit2(float volume)
    {
        PlaySound(Submit2, volume);
    }
    public void PlaySubmit3(float volume)
    {
        PlaySound(Submit3, volume);
    }
    public void PlaySubmit4(float volume)
    {
        PlaySound(Submit4, volume);
    }
    public void PlaySubmit5(float volume)
    {
        PlaySound(Submit5, volume);
    }
    public void PlayMove1(float volume)
    {
        PlaySound(Move1, volume);
    }
    public void PlayMove2(float volume)
    {
        PlaySound(Move2, volume);
    }
    public void PlayMove3(float volume)
    {
        PlaySound(Move3, volume);
    }
    public void PlayMove4(float volume)
    {
        PlaySound(Move4, volume);
    }
    public void PlayMove5(float volume)
    {
        PlaySound(Move5, volume);
    }
    public void PlayDamage1(float volume)
    {
        PlaySound(Damage1, volume);
    }
    public void PlayDamage2(float volume)
    {
        PlaySound(Damage2, volume);
    }
    public void PlayDamage3(float volume)
    {
        PlaySound(Damage3, volume);
    }
    public void PlayHitSuccess1(float volume)
    {
        PlaySound(HitSuccess1, volume);
    }
    public void PlayHitSuccess2(float volume)
    {
        PlaySound(HitSuccess1, volume);
    }
    public void PlayHitSuccess3(float volume)
    {
        PlaySound(HitSuccess1, volume);
    }
    public void PlayHitMiss1(float volume)
    {
        PlaySound(HitMiss1, volume);
    }
    public void PlayHitMiss2(float volume)
    {
        PlaySound(HitMiss2, volume);
    }
    public void PlayHitMiss3(float volume)
    {
        PlaySound(HitMiss3, volume);
    }
    public void PlayHitMiss4(float volume)
    {
        PlaySound(HitMiss4, volume);
    }
    public void PlayHitMiss5(float volume)
    {
        PlaySound(HitMiss5, volume);
    }
    public void PlayHeartbeatFast(float volume) 
    {
        PlaySound(HeartbeatFast, volume);
    }
    public void PlayHeartbeatSlow(float volume)
    {
        PlaySound(HeartbeatSlow, volume);
    }

    public void PlayShouchitsu(float volume)
    {
        PlaySound(Shouchitsu, volume);
    }

    public void Playgarakuta(float volume)
    {
        PlaySound(garakuta, volume);
    }

    // [2026/05/08] Amagata Support for Obake FBX variations and Obake voice variations start
    public void PlayGhostLaughV2Normal(float volume)
    {
        PlaySound(GhostLaughV2Normal, volume);
    }

    public void PlayGhostLaughV2Fat(float volume)
    {
        PlaySound(GhostLaughV2Fat, volume);
    }

    public void PlayGhostLaughV2Chatter(float volume)
    {
        PlaySound(GhostLaughV2Chatter, volume);
    }
    // [2026/05/08] Amagata Support for Obake FBX variations and Obake voice variations end
}