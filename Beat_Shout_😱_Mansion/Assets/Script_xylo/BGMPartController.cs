// [2026/11/16] Amagata issue #88 start
using CriWare;
using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGMクロスフェーダー
/// </summary>
public class BGMPartController : MonoBehaviour
{
    /// <summary>BGMクロスフェーダー設定テーブル</summary>
    [SerializeField] private BGMPartConfingTable bGMPartConfingTable;
    /// <summary>CRIWARE_conductor -> BGMクロスフェーダー のプレゼンタ</summary>
    [SerializeField] private BGMPartControllerPresenter bGMPartControllerPresenter;
    /// <summary>BGMクロスフェーダー設定</summary>
    [SerializeField] private BGMPartSettings settings;
    /// <summary>フェード用コルーチン</summary>
    private Coroutine _fadeCoroutine;
    /// <summary>現在のAisac値</summary>
    private float _currentAisacValue;
    /// <summary>初期処理の完了フラグ</summary>
    private readonly ReactiveProperty<bool> _isCompletedStart = new ReactiveProperty<bool>();
    /// <summary>R3のリソース管理</summary>
    private DisposableBag _disposableBag = new DisposableBag();

    private void Reset()
    {
        var set = settings;

        if (set.atomSourceBase == null)
            set.atomSourceBase = GetComponent<CriAtomSource>();
    }

    private void Start()
    {
        var table = bGMPartConfingTable;
        var presenter = bGMPartControllerPresenter;

        table.Initialize();
        presenter.Initialize();

        _isCompletedStart.Value = true;
    }

    private void OnDisable()
    {
        var presenter = bGMPartControllerPresenter;
        var set = settings;
        var atomSourceBase = set.atomSourceBase;

        if (atomSourceBase != null && atomSourceBase.player != null)
        {
            atomSourceBase.player.OnBeatSyncCallback -= presenter.OnBeatSync;
        }
    }

    private void OnDestroy()
    {
        var table = bGMPartConfingTable;
        var presenter = bGMPartControllerPresenter;

        table.Dispose();
        presenter.Dispose();
    }

    /// <summary>
    /// BGMを再生
    /// </summary>
    /// <param name="toId">再生先ID</param>
    public void PlayBgm(int toId)
    {
        var isCompletedStart = _isCompletedStart;

        void DoPlay()
        {
            var set = settings;
            var atomSourceBase = set.atomSourceBase;
            var atomSourceShortNotes = set.atomSourceShortNotes;
            var atomSourceLongNotes = set.atomSourceLongNotes;
            var table = bGMPartConfingTable;
            var bGMPartConfingTablePartMapDic = table.BGMPartConfingTablePartMapDic;
            var atomSourceCOfBPM = table.atomSourceCOfBPM;
            var targetValue = bGMPartConfingTablePartMapDic[toId].value;
            var presenter = bGMPartControllerPresenter;

            _currentAisacValue = targetValue;

            var volumeBase = table.GetVolumeBase(targetValue);

            atomSourceBase.volume = volumeBase;
            atomSourceBase.player.OnBeatSyncCallback += presenter.OnBeatSync;
            presenter.DoForceSetTempo(atomSourceCOfBPM);
            atomSourceBase.Play();

            var volumeShortNotes = table.GetVolumeShortNotes(targetValue);

            atomSourceShortNotes.volume = volumeShortNotes;
            atomSourceShortNotes.Play();

            var volumeLongNotes = table.GetVolumeLongNotes(targetValue);

            atomSourceLongNotes.volume = volumeLongNotes;
            atomSourceLongNotes.Play();
        }

        if (isCompletedStart.CurrentValue)
        {
            DoPlay();
        }
        else
        {
            isCompletedStart.Where(x => x)
                .Take(1)
                .Subscribe(_ =>
                {
                    DoPlay();
                })
                .AddTo(ref _disposableBag);
        }
    }

    /// <summary>
    /// BGMを切り替え
    /// </summary>
    /// <param name="toId">再生先ID</param>
    public void ChangeBgmFromCurrentTo(int toId)
    {
        var isCompletedStart = _isCompletedStart;

        void DoChange()
        {
            var table = bGMPartConfingTable;
            var bGMPartConfingTablePartMapDic = table.BGMPartConfingTablePartMapDic;
            var targetValue = bGMPartConfingTablePartMapDic[toId].value;

            StartFade(targetValue);
        }

        if (isCompletedStart.CurrentValue)
        {
            DoChange();
        }
        else
        {
            isCompletedStart.Where(x => x)
                .Take(1)
                .Subscribe(_ =>
                {
                    DoChange();
                })
                .AddTo(ref _disposableBag);
        }
    }

    /// <summary>
    /// BGMを停止
    /// </summary>
    public void StopBgmAny()
    {
        var isCompletedStart = _isCompletedStart;

        void DoStop()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = null;

            var set = settings;
            var atomSourceBase = set.atomSourceBase;
            var atomSourceShortNotes = set.atomSourceShortNotes;
            var atomSourceLongNotes = set.atomSourceLongNotes;
            var table = bGMPartConfingTable;
            var bGMPartConfingTablePartMapDic = table.BGMPartConfingTablePartMapDic;
            var atomSourceCOfBPM = table.atomSourceCOfBPM;
            var presenter = bGMPartControllerPresenter;

            _currentAisacValue = 0f;

            atomSourceBase.Stop();

            atomSourceShortNotes.Stop();

            atomSourceLongNotes.Stop();
        }

        if (isCompletedStart.CurrentValue)
        {
            DoStop();
        }
        else
        {
            isCompletedStart.Where(x => x)
                .Take(1)
                .Subscribe(_ =>
                {
                    DoStop();
                })
                .AddTo(ref _disposableBag);
        }
    }

    public void PauseBGM(bool isPause)
    {
        var set = settings;
        var atomSourceBase = set.atomSourceBase;
        var atomSourceShortNotes = set.atomSourceShortNotes;
        var atomSourceLongNotes = set.atomSourceLongNotes;

        atomSourceBase.Pause(isPause);
        atomSourceShortNotes.Pause(isPause);
        atomSourceLongNotes.Pause(isPause);
    }

    private void StartFade(float targetValue)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeAisacCoroutine(targetValue));
    }

    private IEnumerator FadeAisacCoroutine(float targetValue)
    {
        var set = settings;
        var atomSourceBase = set.atomSourceBase;
        var atomSourceShortNotes = set.atomSourceShortNotes;
        var atomSourceLongNotes = set.atomSourceLongNotes;
        var table = bGMPartConfingTable;
        float crossFadeDuration = table.crossFadeDuration;
        float startValue = _currentAisacValue;
        float elapsed = 0f;

        while (elapsed < crossFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossFadeDuration);
            // 必要ならイージングを掛ける（例：Mathf.SmoothStep）
            _currentAisacValue = Mathf.Lerp(startValue, targetValue, t);

            var volumeBase = table.GetVolumeBase(_currentAisacValue);

            atomSourceBase.volume = volumeBase;

            var volumeShortNotes = table.GetVolumeShortNotes(_currentAisacValue);

            atomSourceShortNotes.volume = volumeShortNotes;

            var volumeLongNotes = table.GetVolumeLongNotes(_currentAisacValue);

            atomSourceLongNotes.volume = volumeLongNotes;

            yield return null;
        }

        _currentAisacValue = targetValue;

        var volumeBase1 = table.GetVolumeBase(_currentAisacValue);

        atomSourceBase.volume = volumeBase1;

        var volumeShortNotes1 = table.GetVolumeShortNotes(_currentAisacValue);

        atomSourceShortNotes.volume = volumeShortNotes1;

        var volumeLongNotes1 = table.GetVolumeLongNotes(_currentAisacValue);

        atomSourceLongNotes.volume = volumeLongNotes1;

        _fadeCoroutine = null;
    }
}

/// <summary>
/// BGMクロスフェーダー設定
/// </summary>
[System.Serializable]
public class BGMPartSettings
{
    /// <summary>CRIAtomソース（ベース音源）</summary>
    public CriAtomSource atomSourceBase;
    /// <summary>CRIAtomソース（ショートノーツ）</summary>
    public CriAtomSource atomSourceShortNotes;
    /// <summary>CRIAtomソース（ロングノーツ）</summary>
    public CriAtomSource atomSourceLongNotes;
}
// [2026/11/16] Amagata issue #88 end
