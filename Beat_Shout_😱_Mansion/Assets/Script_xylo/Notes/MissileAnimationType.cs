using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アニメーションの種類を定義する列挙型
/// </summary>
public enum MissileAnimationType
{
    None,       // アニメーションなし（非表示）
    Long1st,    // 長押し系1段階目
    Long2nd,    // 長押し系2段階目
    Long3rd,    // 長押し系3段階目
    Long4th,    // 長押し系4段階目
    Short1st,   // 短押し系1段階目
    Short2nd,   // 短押し系2段階目
    Short3rd,   // 短押し系3段階目

    // 長押し2拍用
    Long02_01,  // 2拍長押し1段階目
    Long02_02,  // 2拍長押し2段階目

    // 長押し3拍用
    Long03_01,  // 3拍長押し1段階目
    Long03_02,  // 3拍長押し2段階目
    Long03_03,  // 3拍長押し3段階目

    Hit         // ヒットエフェクト
}

/// <summary>
/// ノーツの種類を定義する列挙型
/// </summary>
public enum MissileNoteType
{
    None,       // なし
    Short,      // 短押し
    Long1Beat,  // 1拍長押し
    Long2Beat,  // 2拍長押し
    Long3Beat   // 3拍長押し
}

/// <summary>
/// アニメーションシーケンスを定義するstaticクラス
/// </summary>
public static class MissileAnimationSequences
{
    // Short系ノーツのアニメーションシーケンス
    public static readonly MissileAnimationType[] ShortSequence = new MissileAnimationType[]
    {
        MissileAnimationType.Short1st,
        MissileAnimationType.Short2nd,
        MissileAnimationType.Short3rd,
        MissileAnimationType.Hit
    };

    // 1拍長押し系ノーツのアニメーションシーケンス
    public static readonly MissileAnimationType[] Long1BeatSequence = new MissileAnimationType[]
    {
        MissileAnimationType.Long1st,
        MissileAnimationType.Long2nd,
        MissileAnimationType.Long3rd,
        MissileAnimationType.Long4th,
        MissileAnimationType.Hit
    };

    // 2拍長押し系ノーツのアニメーションシーケンス
    public static readonly MissileAnimationType[] Long2BeatSequence = new MissileAnimationType[]
    {
        MissileAnimationType.Long1st,
        MissileAnimationType.Long2nd,
        MissileAnimationType.Long3rd,
        MissileAnimationType.Long02_01,
        MissileAnimationType.Long02_02,
        MissileAnimationType.Hit
    };

    // 3拍長押し系ノーツのアニメーションシーケンス
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

    /// <summary>
    /// アニメーションパターンに応じたシーケンスを取得
    /// </summary>
    public static MissileAnimationType[] GetSequenceForPattern(int pattern)
    {
        switch (pattern)
        {
            case 0: return new MissileAnimationType[0]; // アニメーションなし
            case 1: return ShortSequence;               // 短押し
            case 2: return Long1BeatSequence;           // 1拍長押し
            case 3: return Long2BeatSequence;           // 2拍長押し
            case 4: return Long3BeatSequence;           // 3拍長押し
            default: return ShortSequence;
        }
    }

    /// <summary>
    /// ノーツタイプに応じたシーケンスを取得
    /// </summary>
    public static MissileAnimationType[] GetSequenceForNoteType(MissileNoteType noteType)
    {
        switch (noteType)
        {
            case MissileNoteType.None: return new MissileAnimationType[0];
            case MissileNoteType.Short: return ShortSequence;
            case MissileNoteType.Long1Beat: return Long1BeatSequence;
            case MissileNoteType.Long2Beat: return Long2BeatSequence;
            case MissileNoteType.Long3Beat: return Long3BeatSequence;
            default: return ShortSequence;
        }
    }

    /// <summary>
    /// アニメーションタイプの表示名を取得
    /// </summary>
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