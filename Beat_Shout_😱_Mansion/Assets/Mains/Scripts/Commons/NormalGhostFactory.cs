using R3;
using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// オバケの家具入居管理のファクトリ
    /// </summary>
    /// <remarks>通常オバケ</remarks>
    public class NormalGhostFactory : IGhostStructFactory
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
                role = GhostRole.Normal,
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
                role = GhostRole.Normal,
            };
        }
    }
}
