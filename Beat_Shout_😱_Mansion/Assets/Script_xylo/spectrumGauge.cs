using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectrumGauge : MonoBehaviour
{
    [Header("Auto Generate Settings")]
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private int barCount = 64;
    [SerializeField] private float radius = 100f;
    [SerializeField] private float startAngle = 0f;
    [SerializeField] private bool autoRotateBars = true;

    [Header("Manual Setup (if not using prefab)")]
    [SerializeField] private RectTransform[] bars;

    [Header("Beat Animation Settings")]
    [SerializeField] private bool enableBeatAnimation = true;
    [SerializeField] private int numberOfPeaks = 2;
    [SerializeField] private float beatScaleFactor = 1.2f;
    [SerializeField] private float offBeatStrength = 0.55f;
    [SerializeField] private float scaleRandomRange = 0.2f;
    [SerializeField] private int maxDistanceRange = 6;
    [SerializeField] private float beatAnimationDuration = 0.3f;
    [SerializeField] private int minDistanceFromPrevious = 4;

    [Header("Peak Shape Settings")]
    [SerializeField] private float peakSharpness = 4f;
    // 1 = なだらか / 大きいほど尖る（2〜8あたりが実用）

    [Header("Rotation Settings")]
    [SerializeField] private bool enableRotation = false;
    [SerializeField] private float rotationSpeed = 10f;

    private Vector3[] originalScales;
    private Coroutine[] scaleCoroutines;
    private List<int> previousBarIndices = new List<int>();
    private bool isDestroying = false;
    private List<GameObject> generatedBars = new List<GameObject>();

    private void Awake()
    {
        if (barPrefab != null)
        {
            GenerateCircularBars();
        }

        if (bars == null || bars.Length == 0)
        {
            Debug.LogError("bars配列にRectTransformを設定してください");
            return;
        }

        originalScales = new Vector3[bars.Length];
        scaleCoroutines = new Coroutine[bars.Length];

        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] != null)
                originalScales[i] = bars[i].localScale;
        }
    }

    private void GenerateCircularBars()
    {
        foreach (var bar in generatedBars)
        {
            if (bar != null)
                DestroyImmediate(bar);
        }
        generatedBars.Clear();

        bars = new RectTransform[barCount];

        for (int i = 0; i < barCount; i++)
        {
            GameObject newBar = Instantiate(barPrefab, transform);
            newBar.name = $"Bar_{i:D2}";

            RectTransform rectTransform = newBar.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                DestroyImmediate(newBar);
                continue;
            }

            float angle = startAngle + (360f / barCount) * i;
            float rad = angle * Mathf.Deg2Rad;

            rectTransform.anchoredPosition =
                new Vector2(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius);

            if (autoRotateBars)
                rectTransform.localRotation = Quaternion.Euler(0, 0, -angle);

            bars[i] = rectTransform;
            generatedBars.Add(newBar);
        }
    }

    private void OnEnable()
    {
        if (enableBeatAnimation)
            RegisterBeatDelegates();
    }

    private void Update()
    {
        if (enableRotation)
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnDisable()
    {
        UnregisterBeatDelegates();
        CancelInvoke();
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        isDestroying = true;
        CancelInvoke();
        StopAllCoroutines();

        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] != null && i < originalScales.Length)
                bars[i].localScale = originalScales[i];
        }
    }

    private int GetCircularDistance(int a, int b, int total)
    {
        int d = Mathf.Abs(a - b);
        return Mathf.Min(d, total - d);
    }

    private void RegisterBeatDelegates()
    {
        CRIWARE_conductor.TempoMethodEvent1 += OnBeat;
        CRIWARE_conductor.TempoMethodEvent2 += OnBeat;
        CRIWARE_conductor.TempoMethodEvent3 += OnBeat;
        CRIWARE_conductor.TempoMethodEvent4 += OnBeat;
        CRIWARE_conductor.TempoMethodEvent5 += OnBeat;
        CRIWARE_conductor.TempoMethodEvent6 += OnBeat;
        CRIWARE_conductor.TempoMethodEvent7 += OnBeat;
        CRIWARE_conductor.TempoMethodEvent8 += OnBeat;
    }

    private void UnregisterBeatDelegates()
    {
        CRIWARE_conductor.TempoMethodEvent1 -= OnBeat;
        CRIWARE_conductor.TempoMethodEvent2 -= OnBeat;
        CRIWARE_conductor.TempoMethodEvent3 -= OnBeat;
        CRIWARE_conductor.TempoMethodEvent4 -= OnBeat;
        CRIWARE_conductor.TempoMethodEvent5 -= OnBeat;
        CRIWARE_conductor.TempoMethodEvent6 -= OnBeat;
        CRIWARE_conductor.TempoMethodEvent7 -= OnBeat;
        CRIWARE_conductor.TempoMethodEvent8 -= OnBeat;
    }

    private void OnBeat()
    {
        if (!enableBeatAnimation || isDestroying) return;

        BeatPulse(1.0f);

        float halfBeat = CRIWARE_conductor.Instance.BasicBeat * 0.5f;
        CancelInvoke(nameof(OnOffBeat));
        Invoke(nameof(OnOffBeat), halfBeat);
    }

    private void OnOffBeat()
    {
        BeatPulse(offBeatStrength);
    }

    /// <summary>
    /// ★ peakSharpness方式：尖った山
    /// </summary>
    private float CalculateInfluence(int distance, float centerScaleFactor)
    {
        if (distance > maxDistanceRange)
            return 1.0f;

        float t = (float)distance / maxDistanceRange; // 0..1
        float decay = Mathf.Pow(1.0f - t, peakSharpness); // ★尖りやすい

        float scaleDelta = centerScaleFactor - 1.0f;
        return 1.0f + (scaleDelta * decay);
    }


    public void BeatPulse(float strengthMul)
    {
        if (!enableBeatAnimation || isDestroying) return;

        int count = bars.Length;
        int peaks = Mathf.Min(numberOfPeaks, count);

        int[] centers = SelectMultipleCenters(peaks);

        float[] scales = new float[peaks];
        for (int i = 0; i < peaks; i++)
        {
            float s = beatScaleFactor + Random.Range(-scaleRandomRange, scaleRandomRange);
            scales[i] = Mathf.Lerp(1.0f, s, Mathf.Clamp01(strengthMul));
        }

        for (int i = 0; i < count; i++)
        {
            if (bars[i] == null) continue;

            float maxInfluence = 1f;
            int minDist = int.MaxValue;

            for (int j = 0; j < peaks; j++)
            {
                int d = GetCircularDistance(i, centers[j], count);
                float inf = CalculateInfluence(d, scales[j]);
                maxInfluence = Mathf.Max(maxInfluence, inf);
                minDist = Mathf.Min(minDist, d);
            }

            if (maxInfluence < 1.01f) continue;

            float durMul = Mathf.Clamp(1f - minDist * 0.05f, 0.6f, 1f);

            if (scaleCoroutines[i] != null)
                StopCoroutine(scaleCoroutines[i]);

            scaleCoroutines[i] = StartCoroutine(
                ScaleBarAnimation(bars[i], originalScales[i],
                maxInfluence, beatAnimationDuration * durMul, i));
        }

        previousBarIndices.Clear();
        previousBarIndices.AddRange(centers);
    }

    private int[] SelectMultipleCenters(int peakCount)
    {
        int count = bars.Length;
        int[] centers = new int[peakCount];
        List<int> candidates = new List<int>();

        for (int i = 0; i < count; i++)
            candidates.Add(i);

        int minBetween = Mathf.Max(2, count / (peakCount * 2));

        for (int p = 0; p < peakCount; p++)
        {
            List<int> valid = new List<int>();

            foreach (int c in candidates)
            {
                bool ok = true;

                for (int j = 0; j < p; j++)
                {
                    if (GetCircularDistance(c, centers[j], count) < minBetween)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok && p == 0 && previousBarIndices.Count > 0)
                {
                    foreach (int prev in previousBarIndices)
                    {
                        if (GetCircularDistance(c, prev, count) < minDistanceFromPrevious)
                        {
                            ok = false;
                            break;
                        }
                    }
                }

                if (ok) valid.Add(c);
            }

            centers[p] = valid.Count > 0
                ? valid[Random.Range(0, valid.Count)]
                : candidates[Random.Range(0, candidates.Count)];

            candidates.Remove(centers[p]);
        }

        return centers;
    }

    private IEnumerator ScaleBarAnimation(
        Transform bar, Vector3 original, float scale, float duration, int index)
    {
        if (bar == null || isDestroying) yield break;

        Vector3 target = new Vector3(
            original.x,
            original.y * scale,
            original.z);

        bar.localScale = target;

        float t = 0f;
        while (t < duration)
        {
            if (bar == null || isDestroying) yield break;
            bar.localScale = Vector3.Lerp(target, original, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        bar.localScale = original;
        scaleCoroutines[index] = null;
    }
}
