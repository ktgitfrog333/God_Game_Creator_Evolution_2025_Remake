using UnityEngine;

/// <summary>
/// ホーミングミサイルマネージャークラスのユーティリティ
/// </summary>
/// <remarks>元々の機能をなるべく汚さないように別部品として扱う</remarks>
public class MissileDirectAnimManagerBUtility
{
    private MissileNoteType noteType;
    private float oneBeat;

    private bool _forceAutoMode = false;
    public bool ForceAutoMode => _forceAutoMode;
    private bool _autoSuccessTriggered = false;
    public bool AutoSuccessTriggered => _autoSuccessTriggered;

    public MissileDirectAnimManagerBUtility(MissileNoteType noteType, float oneBeat)
    {
        this.noteType = noteType;
        this.oneBeat = oneBeat;
    }

    public void SetOneBeat(float oneBeat)
    {
        this.oneBeat = oneBeat;
    }

    public void SetAutoSuccessTriggered(bool autoSuccessTriggered)
    {
        _autoSuccessTriggered = autoSuccessTriggered;
    }

    public void SetForceAutoMode(bool enable)
    {
        _forceAutoMode = enable;
        if (!enable) _autoSuccessTriggered = false;
    }

    public float GetReleaseTargetTime()
    {
        switch (noteType)
        {
            case MissileNoteType.Long1Beat: return oneBeat * 5f;
            case MissileNoteType.Long2Beat: return oneBeat * 6f;
            case MissileNoteType.Long3Beat: return oneBeat * 7f;
            case MissileNoteType.Long2Beat_Mic: return oneBeat * 6f; // Long2Beatと同じ
            default: return oneBeat * 5f;
        }
    }

    public void ResetAnimation()
    {
        _forceAutoMode = false;
        _autoSuccessTriggered = false;
    }
}
