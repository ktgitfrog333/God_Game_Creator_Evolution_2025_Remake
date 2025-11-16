using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Rewired;

[DisallowMultipleComponent]
public class IntroMovieScene : MonoBehaviour
{
    // ======================================================================
    // ★ 基本設定
    // ======================================================================

    private Player player;                       // Rewired Player
    public Image White;                          // フェード用白Image（全画面）
    public float fadeDuration = 1.0f;            // フェード時間
    public string gameSceneName = "GameScene";   // 遷移先シーン名

    [Header("Rewired")]
    public int rewiredPlayerId = 0;
    public string submitActionName = "UISubmit";
    public bool enableDebugMode = false;

    [Header("Skip (Hold)")]
    public float holdToSkipSeconds = 1.0f;       // 長押し秒
    public bool allowSkipAnytime = true;         // タイムライン中でもスキップ許可
    private float submitHeldTime = 0f;
    private bool isTransitioning = false;

    public enum TimelineTimeUnit { Seconds, Beats }

    [Header("Timeline Clock")]
    public TimelineTimeUnit timelineTimeUnit = TimelineTimeUnit.Beats;
    public float bpm = 95f;                      // Beats時必須
    public float startOffsetBeats = 0f;          // 開始オフセット（拍）
    public float startOffsetSeconds = 0f;        // 開始オフセット（秒）

    public enum Ease { Linear, EaseIn, EaseOut, EaseInOut }

    // ===================== 画像タイムライン（「何拍間」表示） =====================
    [Header("Sprite Timeline")]
    public Image spriteTarget;                   // 表示先（UI Image）
    public List<SpriteCue> spriteCues = new();   // 画像タイムライン（※「何拍間」）

    [Serializable]
    public class SpriteCue
    {
        public Sprite sprite;
        public bool clearInstead = false;

        [Header("Display Length")]
        [Tooltip("この画像を表示する長さ（拍）。timelineTimeUnit=Beats のとき使用。")]
        public float durationBeats = 4f;

        [Tooltip("timelineTimeUnit=Secondsの場合に使用。通常はBeatsを推奨。")]
        public float durationSeconds = 1f;

        [Header("Pan & Zoom（開始→終了）")]
        public Vector2 startAnchoredPos = Vector2.zero;
        public float startScale = 1f;
        public Vector2 endAnchoredPos = Vector2.zero;
        public float endScale = 1f;
        public Ease ease = Ease.Linear;
    }

    // ===================== テキストタイムライン（「何拍間」表示） =====================
    [Header("Text Timeline")]
    public TextMeshProUGUI textTarget;           // テキスト表示先
    public TextDisplayMode textDisplayMode = TextDisplayMode.Replace;
    public enum TextDisplayMode { Replace, Append }

    public List<TextCue> textCues = new();       // テキストも「何拍間」表示に統一（オフセット中は空白）

    [Serializable]
    public class TextCue
    {
        public bool clearInstead = false;        // クリア表示（空行）用
        [TextArea(1, 6)]
        public string text = "";

        [Header("Display Length")]
        [Tooltip("このテキストを表示する長さ（拍）。timelineTimeUnit=Beats のとき使用。")]
        public float durationBeats = 4f;

        [Tooltip("timelineTimeUnit=Seconds の場合に使用。通常はBeatsを推奨。")]
        public float durationSeconds = 1f;
    }

    // 内部状態
    private bool timelineCompleted = false;
    private float timelineStartTime;

    // ======================================================================
    // ★ Awake / Start
    // ======================================================================

    void Awake()
    {
        // Rewired
        player = ReInput.players.GetPlayer(rewiredPlayerId);
        if (player == null) Debug.LogError("[Intro] Rewiredプレイヤーが取得できませんでした！");
        if (enableDebugMode && player != null) Debug.Log("[Intro] Rewiredプレイヤー取得成功");

        // 初期化：テキストは空、画像はいったん非表示（※画像はオフセット中に最初の一枚を出す仕様）
        if (textTarget != null) textTarget.text = "";
        if (spriteTarget != null)
        {
            spriteTarget.sprite = null;
            spriteTarget.enabled = false;
        }
    }

    void Start()
    {
        StartCoroutine(FadeIn());
        StartCoroutine(RunTimeline());
    }

    // ======================================================================
    // ★ Update（長押しスキップ）
    // ======================================================================

    void Update()
    {
        if (isTransitioning) return;

        bool pressed = player != null && player.GetButton(submitActionName);
        bool released = player != null && player.GetButtonUp(submitActionName);

        if (!allowSkipAnytime && !timelineCompleted)
        {
            submitHeldTime = 0f;
            return;
        }

        if (pressed)
        {
            submitHeldTime += Time.unscaledDeltaTime;
            if (submitHeldTime >= holdToSkipSeconds)
            {
                if (enableDebugMode) Debug.Log("[Intro] 長押し成立 → シーン遷移");
                StartCoroutine(GoToScene());
            }
        }
        if (released) submitHeldTime = 0f;
    }

    // ======================================================================
    // ★ ユーティリティ
    // ======================================================================

    float BeatsToSeconds(float beats)
    {
        if (bpm <= 0f) return 0f;
        return beats * (60f / bpm);
    }

    // ======================================================================
    // ★ タイムライン（画像＝「何拍間」／テキスト＝「何拍間」）
    // ======================================================================

    IEnumerator RunTimeline()
    {
        // ---- オフセット秒を先に計算 ----
        float offsetSec = 0f;
        if (timelineTimeUnit == TimelineTimeUnit.Beats && bpm > 0f)
            offsetSec += BeatsToSeconds(startOffsetBeats);
        offsetSec += startOffsetSeconds;

        // ---- オフセット待機前の見せ方 ----
        // 画像：最初の可視スプライトを表示して待機
        if (spriteTarget != null && spriteCues.Count > 0)
        {
            SpriteCue firstVisible = null;
            foreach (var c in spriteCues)
            {
                if (!c.clearInstead) { firstVisible = c; break; }
            }
            if (firstVisible != null)
            {
                spriteTarget.sprite = firstVisible.sprite;
                spriteTarget.enabled = (firstVisible.sprite != null);
                RectTransform rt0 = (RectTransform)spriteTarget.transform;
                rt0.anchoredPosition = firstVisible.startAnchoredPos;
                rt0.localScale = Vector3.one * Mathf.Max(0.0001f, firstVisible.startScale);
            }
        }

        // テキスト：オフセット中は空白（表示しない）
        if (textTarget != null) textTarget.text = "";

        // ---- オフセット待機 ----
        if (offsetSec > 0f) yield return new WaitForSecondsRealtime(offsetSec);

        // ---- 画像タイムライン（各キューの「表示拍数」から開始/終了秒を積み上げ計算）----
        List<(float startSec, float durSec, SpriteCue cue)> spriteTimeline = new();
        float spriteAccum = 0f;

        for (int i = 0; i < spriteCues.Count; i++)
        {
            SpriteCue cue = spriteCues[i];
            float durSec = (timelineTimeUnit == TimelineTimeUnit.Beats)
                ? Mathf.Max(0f, BeatsToSeconds(cue.durationBeats))
                : Mathf.Max(0f, cue.durationSeconds);

            float startSec = spriteAccum;
            float endSec = startSec + durSec;

            spriteTimeline.Add((startSec, durSec, cue));
            spriteAccum = endSec;
        }

        // ---- テキストタイムライン（画像と同様に「何拍間」）----
        List<(float startSec, float durSec, TextCue cue)> textTimeline = new();
        float textAccum = 0f;

        for (int i = 0; i < textCues.Count; i++)
        {
            TextCue cue = textCues[i];
            float durSec = (timelineTimeUnit == TimelineTimeUnit.Beats)
                ? Mathf.Max(0f, BeatsToSeconds(cue.durationBeats))
                : Mathf.Max(0f, cue.durationSeconds);

            float startSec = textAccum;
            float endSec = startSec + durSec;

            textTimeline.Add((startSec, durSec, cue));
            textAccum = endSec;
        }

        timelineStartTime = Time.unscaledTime;

        int sIndex = 0;
        int tIndex = 0;
        Coroutine panCo = null;

        // ---- 実行ループ ----
        while (sIndex < spriteTimeline.Count || tIndex < textTimeline.Count)
        {
            float elapsed = Time.unscaledTime - timelineStartTime;
            bool did = false;

            // スプライト切替＆パン・ズーム開始
            if (sIndex < spriteTimeline.Count && elapsed >= spriteTimeline[sIndex].startSec)
            {
                var item = spriteTimeline[sIndex];

                if (panCo != null) StopCoroutine(panCo);
                panCo = StartCoroutine(PanZoom(item.cue, item.durSec));

                sIndex++;
                did = true;
            }

            // テキスト切替（「何拍間」：次のテキスト開始で上書き／clearInsteadなら空行）
            if (tIndex < textTimeline.Count && elapsed >= textTimeline[tIndex].startSec)
            {
                var item = textTimeline[tIndex];
                if (textTarget != null)
                {
                    if (item.cue.clearInstead)
                    {
                        // 空表示
                        if (textDisplayMode == TextDisplayMode.Replace) textTarget.text = "";
                        else
                        {
                            // Appendモードでも「クリア」を表現したい場合は改行追加なしで空行を入れる
                            textTarget.text += (string.IsNullOrEmpty(textTarget.text) ? "" : "\n");
                        }
                    }
                    else
                    {
                        if (textDisplayMode == TextDisplayMode.Replace)
                            textTarget.text = item.cue.text ?? "";
                        else
                        {
                            if (!string.IsNullOrEmpty(textTarget.text)) textTarget.text += "\n";
                            textTarget.text += item.cue.text ?? "";
                        }
                    }
                }

                tIndex++;
                did = true;
            }

            if (!did) yield return null;
        }

        // ---- 全部終わったら遷移（自動）----
        timelineCompleted = true;
        yield return new WaitForSecondsRealtime(0.5f);
        if (!isTransitioning) StartCoroutine(GoToScene());
    }

    // ======================================================================
    // ★ パン＆ズーム（画像）
    // ======================================================================

    IEnumerator PanZoom(SpriteCue cue, float durationSec)
    {
        if (spriteTarget == null) yield break;

        if (cue.clearInstead)
        {
            spriteTarget.enabled = false;
            yield break;
        }

        // 画像セット
        spriteTarget.sprite = cue.sprite;
        spriteTarget.enabled = (cue.sprite != null);

        // 初期値
        RectTransform rt = (RectTransform)spriteTarget.transform;
        rt.anchoredPosition = cue.startAnchoredPos;
        rt.localScale = new Vector3(Mathf.Max(0.0001f, cue.startScale),
                                    Mathf.Max(0.0001f, cue.startScale), 1f);

        // 時間0なら終端へ
        if (durationSec <= 0f)
        {
            rt.anchoredPosition = cue.endAnchoredPos;
            float es = Mathf.Max(0.0001f, cue.endScale);
            rt.localScale = new Vector3(es, es, 1f);
            yield break;
        }

        // 補間
        float t = 0f;
        while (t < durationSec)
        {
            float u = t / durationSec;
            switch (cue.ease)
            {
                case Ease.EaseIn: u = u * u; break;
                case Ease.EaseOut: u = 1f - (1f - u) * (1f - u); break;
                case Ease.EaseInOut: u = (u < 0.5f) ? 2f * u * u : 1f - Mathf.Pow(-2f * u + 2f, 2f) / 2f; break;
                default: break; // Linear
            }

            rt.anchoredPosition = Vector2.LerpUnclamped(cue.startAnchoredPos, cue.endAnchoredPos, u);
            float s = Mathf.LerpUnclamped(cue.startScale, cue.endScale, u);
            s = Mathf.Max(0.0001f, s);
            rt.localScale = new Vector3(s, s, 1f);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 最終値担保
        rt.anchoredPosition = cue.endAnchoredPos;
        float sEnd = Mathf.Max(0.0001f, cue.endScale);
        rt.localScale = new Vector3(sEnd, sEnd, 1f);
    }

    // ======================================================================
    // ★ フェードイン / フェードアウト
    // ======================================================================

    IEnumerator FadeIn()
    {
        if (White == null) yield break;

        White.gameObject.SetActive(true);
        Color c = White.color; c.a = 1f; White.color = c;

        float t = 0f;
        while (t < fadeDuration)
        {
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            c.a = a; White.color = c;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        c.a = 0f; White.color = c;
        White.gameObject.SetActive(false);
    }

    IEnumerator FadeOut()
    {
        if (White == null) yield break;

        White.gameObject.SetActive(true);
        Color c = White.color;

        float t = 0f;
        while (t < fadeDuration)
        {
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            c.a = a; White.color = c;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        c.a = 1f; White.color = c;
    }

    // ======================================================================
    // ★ シーン遷移
    // ======================================================================

    IEnumerator GoToScene()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(gameSceneName);
    }
}
