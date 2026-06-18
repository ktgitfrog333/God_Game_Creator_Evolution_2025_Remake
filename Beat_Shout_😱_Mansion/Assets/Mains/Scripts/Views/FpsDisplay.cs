using UnityEngine;
using TMPro;
using Mains.External;

/// <summary>
/// FPSをUIに表示するコンポーネント
/// TextMeshProUGUI にアタッチして使用
/// </summary>
public class FpsDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI dBLevelText;

    [Header("設定")]
    [Tooltip("何秒ごとに更新するか（0 = 毎フレーム）")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("色閾値")]
    [SerializeField] private int goodFps = 60;
    [SerializeField] private int warnFps = 30;
    [SerializeField] private Color colorGood = Color.green;
    [SerializeField] private Color colorWarn = Color.yellow;
    [SerializeField] private Color colorBad = Color.red;

    private Script_xyloApi _api;

    private float _timer;
    private int _fps;

    private void Awake()
    {
        // SerializeField が未設定なら自身の TMP を探す
        if (fpsText == null)
            fpsText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _api = new Script_xyloApi();
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;

        if (_timer >= updateInterval)
        {
            _fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            _timer = 0f;
            Refresh();
        }
    }

    private void OnDestroy()
    {
        _api?.Dispose();
    }

    private void Refresh()
    {
        if (fpsText == null) return;

        fpsText.text = $"FPS: {_fps}";
        fpsText.color = _fps >= goodFps ? colorGood
                      : _fps >= warnFps ? colorWarn
                      : colorBad;

        dBLevelText.text = $"DBLevel: [{_api.GetDBLevel()}]";
    }
}
