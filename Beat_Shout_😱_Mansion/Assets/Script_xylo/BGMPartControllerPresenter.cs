// [2026/11/16] Amagata issue #88 start
using CriWare;
using R3;
using UnityEngine;

/// <summary>
/// CRIWARE_conductor -> BGMクロスフェーダー のプレゼンタ
/// </summary>
/// <remarks><see cref="CRIWARE_conductor"/> 側にチュートリアル用のクロスフェード機能は作れないため<br>
/// <see cref="BGMPartController"/> へチュートリアル用クロスフェード機能を作る<br><br>
/// チュートリアル用のBGM再生中は、<see cref="CRIWARE_conductor"/> 側の内部データ（ビート同期等）は更新されないため<br>
/// 本クラスを通して更新させる</remarks>
[CreateAssetMenu(fileName = "BGMPartControllerPresenter", menuName = "Scriptable Objects/BGMPartControllerPresenter")]
public class BGMPartControllerPresenter : ScriptableObject, System.IDisposable
{
    /// <summary>初期処理完了フラグ</summary>
    private ReactiveProperty<bool> _isCompletedInitialize;
    /// <summary>BGMコンダクタ</summary>
    private ReactiveProperty<CRIWARE_conductor> _cRIWARE_conductor;
    /// <summary>BGMコンダクタ</summary>
    public ReactiveCommand<CRIWARE_conductor> CRIWARE_conductorCommand { private set; get; }
    /// <summary>現在のビート同期コールバック情報</summary>
    private CriAtomExBeatSync.Info _currentInfo;
    /// <summary>R3のリソース管理</summary>
    private DisposableBag _disposableBag;

    public void Initialize()
    {
        if (_isCompletedInitialize != null && _isCompletedInitialize.CurrentValue) return;

        _isCompletedInitialize = new ReactiveProperty<bool>();
        _cRIWARE_conductor = new ReactiveProperty<CRIWARE_conductor>();
        CRIWARE_conductorCommand = new ReactiveCommand<CRIWARE_conductor>();
        _disposableBag = new DisposableBag();

        CRIWARE_conductorCommand.Subscribe(x => _cRIWARE_conductor.Value = x).AddTo(ref _disposableBag);

        _isCompletedInitialize.Value = true;
    }

    /// <summary>
    /// <see cref="CRIWARE_conductor.DoForceSetTempo(float)"/>へ渡す
    /// </summary>
    /// <param name="bpm">BPM</param>
    public void DoForceSetTempo(float bpm)
    {
        var cRIWARE_conductor = _cRIWARE_conductor;

        if (cRIWARE_conductor.CurrentValue != null)
        {
            cRIWARE_conductor.CurrentValue.DoForceSetTempo(bpm);
        }
        else
        {
            ReactiveProperty<float> tmpBpm = new ReactiveProperty<float>(bpm);

            cRIWARE_conductor.Where(x => x != null)
                .Take(1)
                .Subscribe(cRIWARE_conductor =>
                {
                    var bpm = tmpBpm.CurrentValue;

                    cRIWARE_conductor.DoForceSetTempo(bpm);
                })
                .AddTo(ref _disposableBag);
        }
    }

    /// <summary>
    /// <see cref="CRIWARE_conductor.OnBeatSync(ref CriAtomExBeatSync.Info)"/>との同期用
    /// </summary>
    /// <param name="info">ビート同期コールバック情報</param>
    public void OnBeatSync(ref CriAtomExBeatSync.Info info)
    {
        var cRIWARE_conductor = _cRIWARE_conductor;

        if (cRIWARE_conductor.CurrentValue != null)
        {
            cRIWARE_conductor.CurrentValue.DoOnBeatSync(ref info);
        }
        else
        {
            _currentInfo = info;

            cRIWARE_conductor.Where(x => x != null)
                .Take(1)
                .Subscribe(cRIWARE_conductor =>
                {
                    cRIWARE_conductor.DoOnBeatSync(ref _currentInfo);
                })
                .AddTo(ref _disposableBag);
        }
    }

    public void Dispose()
    {
        _disposableBag.Dispose();

        _cRIWARE_conductor = null;
        CRIWARE_conductorCommand = null;

        _isCompletedInitialize = null;
    }
}
// [2026/11/16] Amagata issue #88 end
