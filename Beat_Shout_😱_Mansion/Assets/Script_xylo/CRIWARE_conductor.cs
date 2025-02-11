using System;
using System.Collections;
using System.Collections.Generic;
using CriWare;
using UnityEngine;
using static CriWare.CriAtomExBeatSync;

public class CRIWARE_conductor : MonoBehaviour
{
    public int ThisStageNumber = 0;

    [HideInInspector] public static CRIWARE_conductor Instance { get; private set; }//シングルトンを設定

    public CriAtomSource atomSourceA;//BGMの音源A
    public CriAtomSource atomSourceB;//BGMの音源B
    public CriAtomSource atomSourceC;//BGMの音源C 同様の形で増やすことが可能

    [HideInInspector] public CriAtomSource currentSource;
    private bool isInitializedAfterChange = false; // 楽曲変更後に初期化が必要かどうかのフラグ

    [HideInInspector] public float lastBeatSyncTime = -1f; // 最後にビート同期イベントが発生した時刻

    private float lastBeatSyncTime01 = -1f; // 最後に一拍目同期イベントが発生した時刻
    private float lastBeatSyncTime02 = -1f; // 最後に一拍目同期イベントが発生した時刻
    private float lastBeatSyncTime03 = -1f; // 最後に一拍目同期イベントが発生した時刻
    private float lastBeatSyncTime04 = -1f; // 最後に一拍目同期イベントが発生した時刻


    public float BeatFuzzy = 20f; // ビートのズレ許容範囲
    public float BeatOffSet = 0.0f; // ビートのズレのオフセット。マイナス値前提。秒数の実数
    private float beatFuzzySet; // ビートのズレ許容範囲の決定値
    [HideInInspector] public float BasicBeat; // ４分音符の秒数
    [HideInInspector] public float frameRate;  // フレームレートを保持するプロパティ

    public static event Action TempoSet;

    //拍頭
    public static event Action TempoMethodEvent1;
    public static event Action TempoMethodEvent2;
    public static event Action TempoMethodEvent3;
    public static event Action TempoMethodEvent4;
    public static event Action TempoMethodEvent5;
    public static event Action TempoMethodEvent6;
    public static event Action TempoMethodEvent7;
    public static event Action TempoMethodEvent8;

    //拍頭の１フレーム後。分散処理用
    public static event Action TempoMethodDelay1_1;
    public static event Action TempoMethodDelay1_2;
    public static event Action TempoMethodDelay1_3;
    public static event Action TempoMethodDelay1_4;
    public static event Action TempoMethodDelay1_5;
    public static event Action TempoMethodDelay1_6;
    public static event Action TempoMethodDelay1_7;
    public static event Action TempoMethodDelay1_8;

    //拍頭の２フレーム後。分散処理用
    public static event Action TempoMethodDelay2_1;
    public static event Action TempoMethodDelay2_2;
    public static event Action TempoMethodDelay2_3;
    public static event Action TempoMethodDelay2_4;
    public static event Action TempoMethodDelay2_5;
    public static event Action TempoMethodDelay2_6;
    public static event Action TempoMethodDelay2_7;
    public static event Action TempoMethodDelay2_8;

    //16ビートの処理用
    public static event Action TempoMethod16Beat2;
    public static event Action TempoMethod16Beat3;
    public static event Action TempoMethod16Beat4;

    private Coroutine invoke16BeatCoroutine; // コルーチンの参照を保持

    //BGM切り替え用のboolスイッチ
    [HideInInspector] public bool BGM_A_Sw = false;
    [HideInInspector] public bool BGM_B_Sw = false;
    [HideInInspector] public bool BGM_C_Sw = false;

    public enum BeatResult
    {
        Tick01,//1拍目に合致した
        Tick02,//2拍目に合致した
        Tick03,//3拍目に合致した
        Tick04,//4拍目に合致した
        Miss // 条件を満たさなかった場合
    }

    void OnEnable() //初期化待ちで少し遅らせる。初期化完了を取得する方法があれば変更する
    {
        Invoke("Delay", 0.3f);
    }

    private void Delay()
    {
        // シングルトンの処理
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 二つ目のインスタンスが作成された場合は破棄
        }

        atomSourceA = GetComponent<CriAtomSource>();
        if (atomSourceA.player != null)
        {
            currentSource = atomSourceA; // 初期状態でAを使用
            CRIWARE_AisacChange.Instance.SetSource(currentSource); // AISACの適用先を設定
            atomSourceA.player.OnBeatSyncCallback += OnBeatSync; // ビート同期イベントのコールバックを登録
            CRIWARE_AisacChange.Instance.PlayStart(); //スタートのAisacを再生
        }
        else
        {
            Invoke("InitDelay", 0.05f); //ここも雰囲気で処理を待っている。他データ読み込みと連携を整える必要あり
        }
    }

    private void InitDelay()
    {
        if (atomSourceA.player != null)
        {
            currentSource = atomSourceA; // 初期状態でAを使用
            CRIWARE_AisacChange.Instance.SetSource(currentSource); // AISACの適用先を設定
            atomSourceA.player.OnBeatSyncCallback += OnBeatSync; // ビート同期イベントのコールバックを登録
            Debug.Log("初期化完了");
            CRIWARE_AisacChange.Instance.PlayStart(); //スタートのAisacを再生

        }
        else
        {
            Invoke("InitDelay", 0.05f);
            Debug.Log("初期化待ち");
        }
    }

    // このオブジェクトの破壊時にビート同期イベントのコールバックを削除
    private void OnDisable()
    {
        currentSource.player.OnBeatSyncCallback -= OnBeatSync;

        if (invoke16BeatCoroutine != null)
        {
            StopCoroutine(invoke16BeatCoroutine);
            invoke16BeatCoroutine = null; // 参照をクリア
        }

    }

    public void ChangeBgmA(int TickNumber) //通常BGM切り替え専用メソッド
    {
        if (currentSource == atomSourceA)
        {
            return; // 同じBGMの場合は何もしない
        }
        BGM_A_Sw = true;
        BGM_B_Sw = false;
        BGM_C_Sw = false;

        currentSource.Stop(); // 現在のBGMを停止

        // 新しいBGMを設定
        currentSource = atomSourceA; // 新しい音源を設定

        // 再生開始位置を設定
        SetStartTimeByTick(TickNumber);

        // 新しいBGMを再生
        currentSource.Play();
        isInitializedAfterChange = true; // 楽曲変更後に初期化するためのフラグを設定
        CRIWARE_AisacChange.Instance.ApplyAisac(currentSource);  // 現在のBGMソースにAISACを適用
        CRIWARE_AisacChange.Instance.BGM0(); // AISACをBGM０にする

        // テンポ情報の更新
        if (currentSource.player != null)
        {
            currentSource.player.OnBeatSyncCallback -= OnBeatSync; // 既存のコールバックを解除
            currentSource.player.OnBeatSyncCallback += OnBeatSync; // 新しいコールバックを登録
        }
    }

    public void ChangeBgmB(int TickNumber) //スキルゲット用BGM切り替え専用メソッド
    {
        if (currentSource == atomSourceB)
        {
            return; // 同じBGMの場合は何もしない
        }
        BGM_A_Sw = false;
        BGM_B_Sw = true;
        BGM_C_Sw = false;

        currentSource.Stop(); // 現在のBGMを停止

        // 新しいBGMを設定
        currentSource = atomSourceB; // 新しい音源を設定

        // 再生開始位置を設定
        SetStartTimeByTick(TickNumber);

        // 新しいBGMを再生
        currentSource.Play();
        isInitializedAfterChange = true; // 楽曲変更後に初期化するためのフラグを設定
        CRIWARE_AisacChange.Instance.ApplyAisac(currentSource);  // 現在のBGMソースにAISACを適用
        CRIWARE_AisacChange.Instance.BGM0(); // AISACをBGM０にする

        // テンポ情報の更新
        if (currentSource.player != null)
        {
            currentSource.player.OnBeatSyncCallback -= OnBeatSync; // 既存のコールバックを解除
            currentSource.player.OnBeatSyncCallback += OnBeatSync; // 新しいコールバックを登録
        }

    }

    public void ChangeBgmC(int TickNumber) //スキルゲット用BGM切り替え専用メソッド
    {
        if (currentSource == atomSourceC)
        {
            return; // 同じBGMの場合は何もしない
        }
        BGM_A_Sw = false;
        BGM_B_Sw = false;
        BGM_C_Sw = true;

        currentSource.Stop(); // 現在のBGMを停止

        // 新しいBGMを設定
        currentSource = atomSourceC; // 新しい音源を設定

        // 再生開始位置を設定
        SetStartTimeByTick(TickNumber);

        // 新しいBGMを再生
        currentSource.Play();
        isInitializedAfterChange = true; // 楽曲変更後に初期化するためのフラグを設定
        CRIWARE_AisacChange.Instance.ApplyAisac(currentSource);  // 現在のBGMソースにAISACを適用
        CRIWARE_AisacChange.Instance.BGM0(); // AISACをBGM０にする

        // テンポ情報の更新
        if (currentSource.player != null)
        {
            currentSource.player.OnBeatSyncCallback -= OnBeatSync; // 既存のコールバックを解除
            currentSource.player.OnBeatSyncCallback += OnBeatSync; // 新しいコールバックを登録
        }
    }





    private void SetStartTimeByTick(int TickNumber)
    {
        if (currentSource == null) return;

        // BPMから1拍の長さを計算
        float beatLengthInSeconds = 60f / frameRate;

        // TickNumberから再生開始位置を計算
        float startTimeInSeconds = beatLengthInSeconds * TickNumber;

        // 再生開始位置を設定
        currentSource.player.SetStartTime((long)(startTimeInSeconds * 1000)); // ミリ秒単位で設定
    }


    private void OnBeatSync(ref CriAtomExBeatSync.Info info)
    {
        lastBeatSyncTime = Time.time; // ビート同期イベントの時刻を更新

        if ((info.barCount == 1 && info.beatCount == 0) || isInitializedAfterChange)
        {
            // 初期化処理
            beatFuzzySet = BeatFuzzy / info.bpm; // ビートのズレ許容範囲の決定値を計算
            BasicBeat = 60 / info.bpm;   // ４分音符の秒数を計算
            frameRate = info.bpm; //再生速度調整の為BPMの情報をそのまま渡す。BPMxx/1BPM120*xx%で速度調整
            TempoSet?.Invoke(); //他スクリプトにテンポ情報を送る

            // 初期化が終わったのでフラグをリセット
            isInitializedAfterChange = false;
        }


        //ここから拍頭を送信する為のイベント

        //info.barCountが偶数で、なおかつinfo.beatCountが1の時に実行 イントロの一小節で奇数偶数が逆になる
        if (info.barCount % 2 == 1 && info.beatCount == 0)
        {
            TempoMethodEvent1?.Invoke();

            lastBeatSyncTime01 = Time.time; // ビート同期イベントの時刻を更新
            StartCoroutine(InvokeTempoMethodDelay1NextFrame()); // 1フレーム後に発火
        }
        else if (info.barCount % 2 == 1 && info.beatCount == 1)
        {
            TempoMethodEvent2?.Invoke();

            lastBeatSyncTime02 = Time.time; // ビート同期イベントの時刻を更新
            StartCoroutine(InvokeTempoMethodDelay2NextFrame()); // 1フレーム後に発火
        }
        else if (info.barCount % 2 == 1 && info.beatCount == 2)//スローモードの時は実行しない
        {
            TempoMethodEvent3?.Invoke();

            lastBeatSyncTime03 = Time.time; // ビート同期イベントの時刻を更新
            StartCoroutine(InvokeTempoMethodDelay3NextFrame()); // 1フレーム後に発火
        }
        else if (info.barCount % 2 == 1 && info.beatCount == 3)
        {
            TempoMethodEvent4?.Invoke();

            lastBeatSyncTime04 = Time.time; // ビート同期イベントの時刻を更新
            StartCoroutine(InvokeTempoMethodDelay4NextFrame()); // 1フレーム後に発火
        }
        //info.barCountが奇数で、なおかつinfo.beatCountが1の時に実行
        else if (info.barCount % 2 == 0 && info.beatCount == 0)
        {
            TempoMethodEvent5?.Invoke();

            lastBeatSyncTime01 = Time.time; // ビート同期イベントの時刻を更新
            StartCoroutine(InvokeTempoMethodDelay5NextFrame()); // 1フレーム後に発火
        }

        else if (info.barCount % 2 == 0 && info.beatCount == 1)//スローモードの時は実行しない
        {
            TempoMethodEvent6?.Invoke();

            lastBeatSyncTime02 = Time.time; // ビート同期イベントの時刻を更新
            StartCoroutine(InvokeTempoMethodDelay6NextFrame()); // 1フレーム後に発火
        }
        else if (info.barCount % 2 == 0 && info.beatCount == 2)//スローモードの時は実行しない)
        {
            TempoMethodEvent7?.Invoke();

            lastBeatSyncTime03 = Time.time; // ビート同期イベントの時刻を更新
            StartCoroutine(InvokeTempoMethodDelay7NextFrame()); // 1フレーム後に発火
        }
        else if (info.barCount % 2 == 0 && info.beatCount == 3)
        {
            TempoMethodEvent8?.Invoke();

            lastBeatSyncTime04 = Time.time; // ビート同期イベントの時刻を更新
            StartCoroutine(InvokeTempoMethodDelay8NextFrame()); // 1フレーム後に発火
        }


        // コルーチンを開始する前に、以前のコルーチンが実行中であれば停止
        if (invoke16BeatCoroutine != null)
        {
            StopCoroutine(invoke16BeatCoroutine);
            invoke16BeatCoroutine = null;
        }
        // コルーチンを開始して 1/4, 2/4, 3/4 拍後に処理を実行
        StartCoroutine(Invoke16BeatCoroutine());

    }

    private IEnumerator Invoke16BeatCoroutine()
    {
        // 1/4拍後に実行
        yield return new WaitForSeconds(BasicBeat * 1 / 4);
        TempoMethod16Beat2?.Invoke();

        // 2/4拍後に実行
        yield return new WaitForSeconds(BasicBeat * 1 / 4); // 前回の1/4拍分はすでに待機済み
        TempoMethod16Beat3?.Invoke();

        // 3/4拍後に実行
        yield return new WaitForSeconds(BasicBeat * 1 / 4); // またさらに1/4拍待機
        TempoMethod16Beat4?.Invoke();

        // コルーチン終了時に参照をクリア
        invoke16BeatCoroutine = null;
    }

    private IEnumerator InvokeTempoMethodDelay1NextFrame()
    {
        // 1フレーム待機
        yield return null;
        StartCoroutine(InvokeTempoMethod2nd1NextFrame());

        TempoMethodDelay1_1?.Invoke();
    }



    private IEnumerator InvokeTempoMethodDelay2NextFrame()
    {
        // 1フレーム待機
        yield return null;
        StartCoroutine(InvokeTempoMethod2nd2NextFrame());

        TempoMethodDelay1_2?.Invoke();
    }


    private IEnumerator InvokeTempoMethodDelay3NextFrame()
    {
        // 1フレーム待機
        yield return null;

        StartCoroutine(InvokeTempoMethod2nd3NextFrame());

        TempoMethodDelay1_3?.Invoke();
    }
    private IEnumerator InvokeTempoMethodDelay4NextFrame()
    {
        // 1フレーム待機
        yield return null;
        StartCoroutine(InvokeTempoMethod2nd4NextFrame());

        TempoMethodDelay1_4?.Invoke();
    }
    private IEnumerator InvokeTempoMethodDelay5NextFrame()
    {
        // 1フレーム待機
        yield return null;
        StartCoroutine(InvokeTempoMethod2nd5NextFrame());

        TempoMethodDelay1_5?.Invoke();
    }
    private IEnumerator InvokeTempoMethodDelay6NextFrame()
    {
        // 1フレーム待機
        yield return null;
        StartCoroutine(InvokeTempoMethod2nd6NextFrame());

        TempoMethodDelay1_6?.Invoke();
    }
    private IEnumerator InvokeTempoMethodDelay7NextFrame()
    {
        // 1フレーム待機
        yield return null;
        StartCoroutine(InvokeTempoMethod2nd7NextFrame());

        TempoMethodDelay1_7?.Invoke();

    }
    private IEnumerator InvokeTempoMethodDelay8NextFrame()
    {
        // 1フレーム待機
        yield return null;
        StartCoroutine(InvokeTempoMethod2nd8NextFrame());

        TempoMethodDelay1_8?.Invoke();
    }


    private IEnumerator InvokeTempoMethod2nd1NextFrame()
    {
        // 1フレーム待機
        yield return null;

        TempoMethodDelay2_1?.Invoke();
    }

    private IEnumerator InvokeTempoMethod2nd2NextFrame()
    {
        // 1フレーム待機
        yield return null;

        TempoMethodDelay2_2?.Invoke();
    }
    private IEnumerator InvokeTempoMethod2nd3NextFrame()
    {
        // 1フレーム待機
        yield return null;

        TempoMethodDelay2_3?.Invoke();
    }
    private IEnumerator InvokeTempoMethod2nd4NextFrame()
    {
        // 1フレーム待機
        yield return null;

        TempoMethodDelay2_4?.Invoke();
    }
    private IEnumerator InvokeTempoMethod2nd5NextFrame()
    {
        // 1フレーム待機
        yield return null;

        TempoMethodDelay2_5?.Invoke();
    }
    private IEnumerator InvokeTempoMethod2nd6NextFrame()
    {
        // 1フレーム待機
        yield return null;

        TempoMethodDelay2_6?.Invoke();
    }
    private IEnumerator InvokeTempoMethod2nd7NextFrame()
    {
        // 1フレーム待機
        yield return null;

        TempoMethodDelay2_7?.Invoke();
    }
    private IEnumerator InvokeTempoMethod2nd8NextFrame()
    {
        // 1フレーム待機
        yield return null;

        TempoMethodDelay2_8?.Invoke();
    }


    public BeatResult JustBeatTick()
    {
        // OnBeatSyncが一度も実行されていない場合、NotExecutedを返す
        if (lastBeatSyncTime < 0) return BeatResult.Miss;
        float elapsedTime1 = Time.time - lastBeatSyncTime01;
        float elapsedTime2 = Time.time - lastBeatSyncTime02;
        float elapsedTime3 = Time.time - lastBeatSyncTime03;
        float elapsedTime4 = Time.time - lastBeatSyncTime04;

        // 経過時間がビート4拍の秒数からbeatFuzzySetを減算したもの以上、もしくはbeatFuzzySet以下の場合は拍目に合致したと判定
        if (elapsedTime1 >= ((BasicBeat * 4) - beatFuzzySet) - BeatOffSet || elapsedTime1 <= beatFuzzySet - BeatOffSet)
        {
            // 条件を満たした場合の処理をここに記述する
            return BeatResult.Tick01;
        }
        else if (elapsedTime2 >= ((BasicBeat * 4) - beatFuzzySet) - BeatOffSet || elapsedTime2 <= beatFuzzySet - BeatOffSet)
        { return BeatResult.Tick02; }

        else if (elapsedTime3 >= ((BasicBeat * 4) - beatFuzzySet) - BeatOffSet || elapsedTime3 <= beatFuzzySet - BeatOffSet)
        { return BeatResult.Tick03; }

        else if (elapsedTime4 >= ((BasicBeat * 4) - beatFuzzySet) - BeatOffSet || elapsedTime4 <= beatFuzzySet - BeatOffSet)
        { return BeatResult.Tick04; }

        return BeatResult.Miss;
    }

    public float AllowedTimeAroundBeat　// ビートのズレ許容範囲の秒数
    {
        get
        {
            return BeatFuzzy / frameRate;
        }
    }

}