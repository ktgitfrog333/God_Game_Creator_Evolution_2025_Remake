using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UIアニメーションレイヤーの基本クラスのカスタマイズテーブル
/// </summary>
[CreateAssetMenu(fileName = "MissileUIAnimationLayerCustomizeTable", menuName = "Scriptable Objects/MissileUIAnimationLayerCustomizeTable")]
public class MissileUIAnimationLayerCustomizeTable : ScriptableObject, System.IDisposable
{
    /// <summary>ショートノーツのアニメーション設定</summary>
    [SerializeField] private MissileUIAnimationLayerCustomizeShortSettings[] missileUIAnimationLayerCustomizeShortSettings;
    /// <summary>ショートノーツのアニメーション設定</summary>
    private Dictionary<MissileAnimationType, RingRadiusShortRange> _missileUIAnimationLayerCustomizeShortSettingsDic;
    /// <summary>ロングノーツのアニメーション設定</summary>
    [SerializeField] private MissileUIAnimationLayerCustomizeLongSettings[] missileUIAnimationLayerCustomizeLongSettings;
    /// <summary>ロングノーツのアニメーション設定</summary>
    private Dictionary<MissileAnimationType, RingRadiusLongRange> _missileUIAnimationLayerCustomizeLongSettingsDic;
    /// <summary>ショートノーツのマテリアル</summary>
    public Material ringMaterialShort;
    /// <summary>ロングノーツのマテリアル</summary>
    public Material ringMaterialLong;

    public void Initialize()
    {
        if (missileUIAnimationLayerCustomizeShortSettings != null &&
            0 < missileUIAnimationLayerCustomizeShortSettings.Length)
        {
            _missileUIAnimationLayerCustomizeShortSettingsDic = new Dictionary<MissileAnimationType, RingRadiusShortRange>();
            for (int i = 0; i < missileUIAnimationLayerCustomizeShortSettings.Length; i++)
            {
                var settings = missileUIAnimationLayerCustomizeShortSettings[i];
                _missileUIAnimationLayerCustomizeShortSettingsDic[settings.missileAnimationType] = settings.ringRadiusShortRange;
            }
        }
        if (missileUIAnimationLayerCustomizeLongSettings != null &&
            0 < missileUIAnimationLayerCustomizeLongSettings.Length)
        {
            _missileUIAnimationLayerCustomizeLongSettingsDic = new Dictionary<MissileAnimationType, RingRadiusLongRange>();
            for (int i = 0; i < missileUIAnimationLayerCustomizeLongSettings.Length; i++)
            {
                var settings = missileUIAnimationLayerCustomizeLongSettings[i];
                _missileUIAnimationLayerCustomizeLongSettingsDic[settings.missileAnimationType] = settings.ringRadiusLongRange;
            }
        }
    }

    /// <summary>
    /// リング半径の情報を取得する
    /// </summary>
    /// <param name="type">ミサイルアニメーションの種類</param>
    /// <returns>[ショートノーツ]リング半径の情報</returns>
    public RingRadiusShortRange GetRingRadiusShortRange(MissileAnimationType type)
    {
        RingRadiusShortRange ringRadiusShortRange = null;
        var shortSettingsDic = _missileUIAnimationLayerCustomizeShortSettingsDic;

        switch (type)
        {
            case MissileAnimationType.Short1st:
            case MissileAnimationType.Short2nd:
            case MissileAnimationType.Short3rd:
                ringRadiusShortRange = new RingRadiusShortRange();
                ringRadiusShortRange = shortSettingsDic[type];

                break;
            default:

                break;
        }

        return ringRadiusShortRange;
    }

    /// <summary>
    /// リング半径の情報を取得する
    /// </summary>
    /// <param name="type">ミサイルアニメーションの種類</param>
    /// <returns>[ロングノーツ]リング半径の情報</returns>
    public RingRadiusLongRange GetRingRadiusLongRange(MissileAnimationType type)
    {
        RingRadiusLongRange ringRadiusLongRange = null;
        var longSettingsDic = _missileUIAnimationLayerCustomizeLongSettingsDic;

        switch (type)
        {
            case MissileAnimationType.Long1st:
            case MissileAnimationType.Long2nd:
            case MissileAnimationType.Long3rd:
            case MissileAnimationType.Long4th:
            case MissileAnimationType.Long02_01:
            case MissileAnimationType.Long02_02:
            case MissileAnimationType.Long03_01:
            case MissileAnimationType.Long03_02:
            case MissileAnimationType.Long03_03:
                ringRadiusLongRange = new RingRadiusLongRange();
                ringRadiusLongRange = longSettingsDic[type];

                break;
            default:

                break;
        }

        return ringRadiusLongRange;
    }

    /// <summary>
    /// アニメーションの種類を定義する列挙型からシェーダー用のノーツタイプを返却
    /// </summary>
    /// <param name="type">ミサイルアニメーションの種類</param>
    /// <returns>シェーダー用のノーツタイプ</returns>
    public int GetNotesType(MissileAnimationType type)
    {
        int notesType = 0;

        switch (type)
        {
            case MissileAnimationType.Long1st:
            case MissileAnimationType.Long2nd:
            case MissileAnimationType.Long3rd:
            case MissileAnimationType.Long4th:
            case MissileAnimationType.Long02_01:
            case MissileAnimationType.Long02_02:
            case MissileAnimationType.Long03_01:
            case MissileAnimationType.Long03_02:
            case MissileAnimationType.Long03_03:
                notesType = 1;
                break;
            case MissileAnimationType.Short1st:
            case MissileAnimationType.Short2nd:
            case MissileAnimationType.Short3rd:
            case MissileAnimationType.Hit:
                break;
            default:
                break;
        }

        return notesType;
    }

    public void Dispose()
    {
        _missileUIAnimationLayerCustomizeShortSettingsDic = null;
        _missileUIAnimationLayerCustomizeLongSettingsDic = null;
    }
}

/// <summary>
/// ショートノーツのアニメーション設定
/// </summary>
[System.Serializable]
public class MissileUIAnimationLayerCustomizeShortSettings
{
    /// <summary>アニメーションの種類を定義する列挙型</summary>
    public MissileAnimationType missileAnimationType;
    /// <summary>[ショートノーツ]リング半径の情報</summary>
    public RingRadiusShortRange ringRadiusShortRange;
}

/// <summary>
/// ロングノーツのアニメーション設定
/// </summary>
[System.Serializable]
public class MissileUIAnimationLayerCustomizeLongSettings
{
    /// <summary>アニメーションの種類を定義する列挙型</summary>
    public MissileAnimationType missileAnimationType;
    /// <summary>[ロングノーツ]リング半径の情報</summary>
    public RingRadiusLongRange ringRadiusLongRange;
}

/// <summary>
/// [ショートノーツ]リング半径の情報
/// </summary>
[System.Serializable]
public class RingRadiusShortRange
{
    /// <summary>リング半径の最大値</summary>
    public float ringRadiusMax;
    /// <summary>リング半径の最小値</summary>
    public float ringRadiusMin;
    /// <summary>リング半径のピース値</summary>
    public float RingRadiusPiece => (ringRadiusMax - ringRadiusMin) / 7f;
}

/// <summary>
/// [ロングノーツ]リング半径の情報
/// </summary>
[System.Serializable]
public class RingRadiusLongRange
{
    /// <summary>リング半径（内側）の最大値</summary>
    public float ringRadiusMax;
    /// <summary>リング半径（内側）の最小値</summary>
    public float ringRadiusMin;
    /// <summary>リング半径（内側）のピース値</summary>
    public float RingRadiusPiece => (ringRadiusMax - ringRadiusMin) / 7f;
    /// <summary>フィル半径の最大値</summary>
    public float fillRadiusMax;
    /// <summary>フィル半径の最小値</summary>
    public float fillRadiusMin;
    /// <summary>フィル半径のピース値</summary>
    public float FillRadiusPiece => (fillRadiusMax - fillRadiusMin) / 7f;
    /// <summary>フィルマスク半径の最大値</summary>
    public float fillMaskRadiusMax;
    /// <summary>フィルマスク半径の最小値</summary>
    public float fillMaskRadiusMin;
    /// <summary>フィルマスク半径のピース値</summary>
    public float FillMaskRadiusPiece => (fillMaskRadiusMax - fillMaskRadiusMin) / 7f;
    /// <summary>リング半径（外側）の最大値</summary>
    public float ring_1RadiusMax;
    /// <summary>リング半径（外側）の最小値</summary>
    public float ring_1RadiusMin;
    /// <summary>リング半径（外側）のピース値</summary>
    public float Ring_1RadiusPiece => (ring_1RadiusMax - ring_1RadiusMin) / 7f;
    /// <summary>フィル量の最大値</summary>
    public float fillAmountMax;
    /// <summary>フィル量の最小値</summary>
    public float fillAmountMin;
    /// <summary>フィル量のピース値</summary>
    public float FillAmountPiece => (fillAmountMax - fillAmountMin) / 7f;
}
