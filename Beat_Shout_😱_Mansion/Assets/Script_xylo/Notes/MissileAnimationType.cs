using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アニメーションの種類を定義する列挙型
/// </summary>
public enum MissileAnimationType
{
    None,
    Long1st,
    Long2nd,
    Long3rd,
    Long4th,
    Short1st,
    Short2nd,
    Short3rd,
    Long02_01,
    Long02_02,
    Long03_01,
    Long03_02,
    Long03_03,
    Hit
}

/// <summary>
/// ノーツの種類を定義する列挙型
/// </summary>
public enum MissileNoteType
{
    None,
    Short,
    Long1Beat,
    Long2Beat,
    Long3Beat,
    Long2Beat_Mic   // マイク音量判定型2拍長押し
}

/// <summary>
/// アニメーションシーケンスを定義するstaticクラス
/// </summary>
public static class MissileAnimationSequences
{
    public static readonly MissileAnimationType[] ShortSequence = new MissileAnimationType[]
    {
        MissileAnimationType.Short1st,
        MissileAnimationType.Short2nd,
        MissileAnimationType.Short3rd,
        MissileAnimationType.Hit
    };

    public static readonly MissileAnimationType[] Long1BeatSequence = new MissileAnimationType[]
    {
        MissileAnimationType.Long1st,
        MissileAnimationType.Long2nd,
        MissileAnimationType.Long3rd,
        MissileAnimationType.Long4th,
        MissileAnimationType.Hit
    };

    public static readonly MissileAnimationType[] Long2BeatSequence = new MissileAnimationType[]
    {
        MissileAnimationType.Long1st,
        MissileAnimationType.Long2nd,
        MissileAnimationType.Long3rd,
        MissileAnimationType.Long02_01,
        MissileAnimationType.Long02_02,
        MissileAnimationType.Hit
    };

    public static readonly MissileAnimationType[] Long3BeatSequence = new MissileAnimationType[]
    {
        MissileAnimationType.Long1st,
        MissileAnimationType.Long2nd,
        MissileAnimationType.Long3rd,
        MissileAnimationType.Long03_01,
        MissileAnimationType.Long03_02,
        MissileAnimationType.Long03_03,
        MissileAnimationType.Hit
    };

    public static MissileAnimationType[] GetSequenceForPattern(int pattern)
    {
        switch (pattern)
        {
            case 0: return new MissileAnimationType[0];
            case 1: return ShortSequence;
            case 2: return Long1BeatSequence;
            case 3: return Long2BeatSequence;
            case 4: return Long3BeatSequence;
            default: return ShortSequence;
        }
    }

    public static MissileAnimationType[] GetSequenceForNoteType(MissileNoteType noteType)
    {
        switch (noteType)
        {
            case MissileNoteType.None: return new MissileAnimationType[0];
            case MissileNoteType.Short: return ShortSequence;
            case MissileNoteType.Long1Beat: return Long1BeatSequence;
            case MissileNoteType.Long2Beat: return Long2BeatSequence;
            case MissileNoteType.Long3Beat: return Long3BeatSequence;
            case MissileNoteType.Long2Beat_Mic: return Long2BeatSequence; // Long2Beatと同じシーケンス
            default: return ShortSequence;
        }
    }

    public static string GetTypeName(MissileAnimationType type)
    {
        switch (type)
        {
            case MissileAnimationType.None: return "なし";
            case MissileAnimationType.Long1st: return "長押し1段階目";
            case MissileAnimationType.Long2nd: return "長押し2段階目";
            case MissileAnimationType.Long3rd: return "長押し3段階目";
            case MissileAnimationType.Long4th: return "1拍長押し4段階目";
            case MissileAnimationType.Long02_01: return "2拍長押し1段階目";
            case MissileAnimationType.Long02_02: return "2拍長押し2段階目";
            case MissileAnimationType.Long03_01: return "3拍長押し1段階目";
            case MissileAnimationType.Long03_02: return "3拍長押し2段階目";
            case MissileAnimationType.Long03_03: return "3拍長押し3段階目";
            case MissileAnimationType.Short1st: return "短押し1段階目";
            case MissileAnimationType.Short2nd: return "短押し2段階目";
            case MissileAnimationType.Short3rd: return "短押し3段階目";
            case MissileAnimationType.Hit: return "ヒット";
            default: return "不明";
        }
    }
}