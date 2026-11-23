// [2026/11/16] Amagata issue #88 start
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGMクロスフェーダー設定テーブル
/// </summary>
[CreateAssetMenu(fileName = "BGMPartConfingTable", menuName = "Scriptable Objects/BGMPartConfingTable")]
public class BGMPartConfingTable : ScriptableObject, System.IDisposable
{
    /// <summary>AISAC設定のクロスフェード終了時間（Sec）</summary>
    public float crossFadeDuration = 0.5f;
    /// <summary>BGMクロスフェーダー設定テーブル用パートマップ配列</summary>
    [SerializeField] private BGMPartConfingTablePartMap[] bGMPartConfingTablePartMaps;
    /// <summary>BGMクロスフェーダー設定テーブル用パートマップ</summary>
    private Dictionary<int, BGMPartConfingTablePartMap> _bGMPartConfingTablePartMapDic;
    /// <summary>BGMクロスフェーダー設定テーブル用パートマップ</summary>
    public Dictionary<int, BGMPartConfingTablePartMap> BGMPartConfingTablePartMapDic => _bGMPartConfingTablePartMapDic;
    /// <summary>フェード用カーブ（ベース音源）</summary>
    public AnimationCurve fadeCurveBase;
    /// <summary>フェード用カーブ（ショートノーツ）</summary>
    public AnimationCurve fadeCurveShortNotes;
    /// <summary>フェード用カーブ（ロングノーツ）</summary>
    public AnimationCurve fadeCurveLongNotes;
    /// <summary>BPM（ベース音源）</summary>
    public float atomSourceCOfBPM = 100f;

    public void Initialize()
    {
        _bGMPartConfingTablePartMapDic = new Dictionary<int, BGMPartConfingTablePartMap>();

        for (int i = 0; i < bGMPartConfingTablePartMaps.Length; i++)
        {
            var bGMPartConfingTablePartMap = bGMPartConfingTablePartMaps[i];

            _bGMPartConfingTablePartMapDic[i] = bGMPartConfingTablePartMap;
        }
    }

    /// <summary>
    /// フェード用カーブ（ベース音源）からボリューム値を取得
    /// </summary>
    /// <param name="targetValue">進捗値（0f~1f）</param>
    /// <returns>ボリューム値</returns>
    public float GetVolumeBase(float targetValue)
    {
        var progress = Mathf.Clamp01(targetValue);
        var volume = fadeCurveBase.Evaluate(progress);

        return volume;
    }

    /// <summary>
    /// フェード用カーブ（ショートノーツ）からボリューム値を取得
    /// </summary>
    /// <param name="targetValue">進捗値（0f~1f）</param>
    /// <returns>ボリューム値</returns>
    public float GetVolumeShortNotes(float targetValue)
    {
        var progress = Mathf.Clamp01(targetValue);
        var volume = fadeCurveShortNotes.Evaluate(progress);

        return volume;
    }

    /// <summary>
    /// フェード用カーブ（ロングノーツ）からボリューム値を取得
    /// </summary>
    /// <param name="targetValue">進捗値（0f~1f）</param>
    /// <returns>ボリューム値</returns>
    public float GetVolumeLongNotes(float targetValue)
    {
        var progress = Mathf.Clamp01(targetValue);
        var volume = fadeCurveLongNotes.Evaluate(progress);

        return volume;
    }

    public void Dispose()
    {
        _bGMPartConfingTablePartMapDic = null;
    }
}

/// <summary>
/// BGMクロスフェーダー設定テーブル用パートマップ
/// </summary>
[System.Serializable]
public class BGMPartConfingTablePartMap
{
    /// <summary>ID</summary>
    public int id;
    /// <summary>値</summary>
    public float value;
}
// [2026/11/16] Amagata issue #88 end
