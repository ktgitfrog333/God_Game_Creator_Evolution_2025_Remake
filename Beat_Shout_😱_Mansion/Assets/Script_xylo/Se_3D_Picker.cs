using CriWare;
// [2026/09/06] Amagata issue #87 start
using R3;
using System.Collections.Generic;

// [2026/09/06] Amagata issue #87 end
using UnityEngine;

/// <summary>
/// 3D空間でSEを再生するためのコンポーネント
/// CriAtomSourceを使用して位置に応じた音声を再生する
/// </summary>
public class Se_3D_Picker : MonoBehaviour
{
    private bool ActiveNow = false;

    [Header("CRIWARE設定")]
    [Tooltip("CriAtomSourceコンポーネントへの参照")]
    public CriAtomSource source; // CriAtomSourceコンポーネントへの参照

    // マスター音量（SEと同じ値を使用）
    private float masterSEVolume = 1.0f;
    // [2026/09/06] Amagata issue #87 start
    private ReactiveProperty<bool> _isCompletedStart = new ReactiveProperty<bool>();
    [SerializeField] private string CueSheetName = "SE_System";
    [SerializeField] private string AcbFilePath = "SE_System.acb";
    private CriAtomExAcb _acbBomb;
    private Dictionary<int, Se_3D_PickerPoltergeistAudioData> _audioDataDictionary = new Dictionary<int, Se_3D_PickerPoltergeistAudioData>();
    private Transform _transform;
    private DisposableBag _disposableBag = new DisposableBag();
    // [2026/09/06] Amagata issue #87 end

    private void Start()
    {
        // 保存されているSE音量を読み込む
        LoadMasterSEVolume();
        // [2026/09/06] Amagata issue #87 start
        var cueSheetDmg = CriAtom.AddCueSheet(CueSheetName, AcbFilePath, "");
        if (cueSheetDmg == null || cueSheetDmg.acb == null)
        {
            Debug.LogError("Failed to load cue sheet or ACB file.");
            return;
        }

        if (source == null)
        {
            Debug.LogError("CriAtomSource is not assigned.");
            return;
        }

        _acbBomb = cueSheetDmg.acb;
        _transform = transform;

        _isCompletedStart.Value = true;
        // [2026/09/06] Amagata issue #87 end
    }

    private void OnEnable()
    {
        // コンポーネントがアクティブになった時に音量を更新
        LoadMasterSEVolume();
    }

    private void OnDisable()
    {
        // 非アクティブになる前に必要な処理
        CleanUp();
    }
    // [2026/09/06] Amagata issue #87 start
    private void OnDestroy()
    {
        _disposableBag.Dispose();
    }
    // [2026/09/06] Amagata issue #87 end

    /// <summary>
    /// マスターSE音量を読み込むメソッド
    /// AudioSettingsManagerから設定を取得
    /// </summary>
    private void LoadMasterSEVolume()
    {
        // AudioSettingsManagerから設定を読み込む
        AudioSettingsData settings = AudioSettingsManager.LoadSettings();
        masterSEVolume = settings.seVolume;
        Debug.Log($"Se_3D_Picker: マスターSE音量を読み込みました: {masterSEVolume:F1}");
    }

    /// <summary>
    /// マスターSE音量を設定するメソッド（AudioSettingsControllerから呼び出される）
    /// </summary>
    /// <param name="volume">設定する音量 (0.0 - 1.0)</param>
    public void SetMasterSEVolume(float volume)
    {
        float previousVolume = masterSEVolume;
        masterSEVolume = Mathf.Clamp(volume, 0f, 1f);

        // 既に再生中の音の音量も更新
        if (source != null && source.status == CriAtomSource.Status.Playing)
        {
            // 以前の音量で割って、新しい音量を掛ける（比率を保持）
            float volumeRatio = previousVolume > 0 ? masterSEVolume / previousVolume : masterSEVolume;
            source.volume = source.volume * volumeRatio;
        }

        Debug.Log($"Se_3D_Picker: マスターSE音量を設定しました: {masterSEVolume:F1}");
    }

    /// <summary>
    /// リソースを解放するメソッド
    /// </summary>
    public void CleanUp()
    {
        // 再生中の場合は停止
        if (ActiveNow && source != null && source.status == CriAtomSource.Status.Playing)
        {
            source.Stop();
        }
        ActiveNow = false;
    }

    /// <summary>
    /// 指定されたキュー名が再生可能かどうかを確認するメソッド
    /// </summary>
    /// <param name="cueName">チェックするキュー名</param>
    /// <returns>再生可能かどうか</returns>
    private bool IsPlayable(string cueName)
    {
        if (string.IsNullOrEmpty(cueName))
        {
            Debug.LogError("Cue name is null or empty.");
            return false;
        }

        if (!ActiveNow)
        {
            // 自動的にアクティブにする
            ActiveNow = true;
        }

        if (source == null)
        {
            Debug.LogError("CriAtomSource is not assigned.");
            return false;
        }

        return true;
    }

    // [2026/09/06] Amagata issue #87 start
    // [2026/09/08] Amagata issue #87 start
    /// <summary>
    /// 指定した音声を再生するメソッド
    /// </summary>
    /// <param name="SeName">再生するキュー名</param>
    /// <param name="volume">個別の音量 (0.0 - 1.0)</param>
    /// <param name="instanceId">インスタンスID</param>
    /// <param name="position">再生位置</param>
    /// <param name="front">前方方向</param>
    /// <param name="regionHandle">3Dリージョンハンドル</param>
    public void PlaySound(string SeName, float volume, int instanceId, Vector3 position, Vector3 front, CriAtomEx3dRegion regionHandle = null)
    {
        //public void PlaySound(string SeName, float volume)
        //public void PlaySound(string SeName, float volume, int instanceId, Vector3 position, Vector3 front)
        // [2026/09/06] Amagata issue #87 end
        // [2026/09/08] Amagata issue #87 end
        // CriAtomSourceがアタッチされているか確認
        if (source == null)
        {
            Debug.LogError("CriAtomSource is not assigned.");
            return;
        }

        if (!IsPlayable(SeName)) return;

        // 個別の音量にマスター音量を乗算して最終的な音量を決定
        // [2026/09/06] Amagata issue #87 start
        //float finalVolume = volume * masterSEVolume;
        float finalVolume = 1f * masterSEVolume;
        // [2026/09/06] Amagata issue #87 end
        finalVolume = Mathf.Clamp(finalVolume, 0f, 1f); // 音量を制限

        // [2026/09/06] Amagata issue #87 start
        // source→playerへ設定を引き継ぐ
        //// CriAtomSourceを使用して音を再生
        //source.cueName = SeName;
        //source.volume = finalVolume;
        //source.Play();
        Observable.EveryUpdate()
            .Where(_ => _isCompletedStart.CurrentValue)
            .Take(1)
            .Subscribe(_ =>
            {
                var trans = _transform;
                var audioDataDictionary = _audioDataDictionary;
                var tmpInstanceId = instanceId;
                var tmpPosition = position;
                var tmpFront = front;
                // [2026/09/08] Amagata issue #87 start
                var tmpRegionHandle = regionHandle;
                // [2026/09/08] Amagata issue #87 end
                CriAtomExPlayer player = null;
                CriAtomEx3dSource criAtomEx3DSource = null;
                var tmpSeName = SeName;
                var tmpVolume = finalVolume;

                if (audioDataDictionary.ContainsKey(tmpInstanceId))
                {
                    var audioData = audioDataDictionary[tmpInstanceId];
                    player = audioData.player;
                    criAtomEx3DSource = audioData.criAtomEx3DSource;

                }
                else
                {
                    player = new CriAtomExPlayer();
                    criAtomEx3DSource = new CriAtomEx3dSource();
                    audioDataDictionary[tmpInstanceId] = new Se_3D_PickerPoltergeistAudioData
                    {
                        player = player,
                        criAtomEx3DSource = criAtomEx3DSource
                    };
                }

                if (_acbBomb == null)
                {
                    Debug.LogWarning("Sound system is not active or ACB is null.");
                    return;
                }

                player.Set3dSource(criAtomEx3DSource);
                player.SetVolume(tmpVolume);
                player.SetCue(_acbBomb, tmpSeName);

                criAtomEx3DSource.SetPosition(tmpPosition.x, tmpPosition.y, tmpPosition.z);
                criAtomEx3DSource.SetOrientation(tmpFront, Vector3.up);
                // [2026/09/08] Amagata issue #87 start
                if (tmpRegionHandle != null)
                {
                    criAtomEx3DSource.Set3dRegion(tmpRegionHandle);
                }
                // [2026/09/08] Amagata issue #87 end
                criAtomEx3DSource.Update();

                player.Start();

                _audioDataDictionary = audioDataDictionary;
            })
            .AddTo(ref _disposableBag);
        // [2026/09/06] Amagata issue #87 end
    }

    /// <summary>
    /// 現在再生中の音声を停止するメソッド
    /// </summary>
    public void StopSound()
    {
        if (source != null && source.status == CriAtomSource.Status.Playing)
        {
            source.Stop();
        }
    }
}
// [2026/09/06] Amagata issue #87 start
public class Se_3D_PickerPoltergeistAudioData
{
    public CriAtomExPlayer player;
    public CriAtomEx3dSource criAtomEx3DSource;
}
// [2026/09/06] Amagata issue #87 end
