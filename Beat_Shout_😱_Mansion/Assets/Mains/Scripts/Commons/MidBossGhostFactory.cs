using R3;
using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// オバケの家具入居管理のファクトリ
    /// </summary>
    /// <remarks>中ボスオバケ</remarks>
    public class MidBossGhostFactory : IGhostStructFactory
    {
        public GhostInStaticObjectStruct CreateEmpty(GhostInStaticObjectStruct baseData)
        {
            return new GhostInStaticObjectStruct
            {
                poltergeistViewID = baseData.poltergeistViewID,
                ghostTeamID = new ReactiveProperty<string>(""),
                useStatus = UseStatus.Empty,
                membersCount = 0,
                attackType = GhostAttackType.None,
                moveType = MoveType.None,
                customShoutRadius = 0f,
                soundOutputType = SoundOutputType.TableDefault,
                role = GhostRole.MidBoss,
                ghostModelType = GhostModelType.ghost_model_normal_type,
                ghostVoiceType = GhostVoiceType.ghost_voice_normal_type,
            };
        }

        public GhostInStaticObjectStruct CreateFrom(GhostInStaticObjectStruct baseData)
        {
            return new GhostInStaticObjectStruct
            {
                poltergeistViewID = baseData.poltergeistViewID,
                ghostTeamID = new ReactiveProperty<string>(baseData.ghostTeamID.Value),
                useStatus = baseData.useStatus,
                membersCount = baseData.membersCount,
                attackType = baseData.attackType,
                moveType = baseData.moveType,
                customShoutRadius = baseData.customShoutRadius,
                soundOutputType = baseData.soundOutputType,
                role = GhostRole.MidBoss,
                ghostModelType = baseData.ghostModelType,
                ghostVoiceType = baseData.ghostVoiceType,
            };
        }
    }
}
