using Mains.Commons;
using UnityEngine;

namespace Mains.Models
{
    /// <summary>
    /// オバケトランザクション管理
    /// </summary>
    public class GhostTransaction : IPoltergeistModel, IHomingObjectCustomizeModel
    {
        /// <summary>オバケの家具入居管理の構造体トランザクション</summary>
        private GhostInStaticObjectStruct _transactionGhostInStaticObjectStruct;
        /// <summary>オバケの家具入居管理の構造体トランザクション</summary>
        public GhostInStaticObjectStruct TransactionGhostInStaticObjectStruct => _transactionGhostInStaticObjectStruct;
        /// <summary>オバケの家具入居管理のファクトリ用インターフェース</summary>
        private IGhostStructFactory _factory;

        public GhostTransaction(IGhostStructFactory factory)
        {
            _factory = factory;
        }

        public void AddGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            throw new System.NotImplementedException();
        }

        public void SetDefaultTransactionGhostInStaticObjectStruct()
        {
            _transactionGhostInStaticObjectStruct = new GhostInStaticObjectStruct();
        }

        public void SetInteractionPartToSearch()
        {
            throw new System.NotImplementedException();
        }

        public void SetIsCompletedBurstGhosts(bool isCompletedBurstGhosts)
        {
            throw new System.NotImplementedException();
        }

        public void SetIsCompletedMoveGhostDirection(bool isCompletedMoveGhostDirection)
        {
            throw new System.NotImplementedException();
        }

        public void SetIsCompletedRhythmPart(int isCompletedRhythmPart)
        {
            throw new System.NotImplementedException();
        }

        public void SetIsMissionClear(bool isMissionClear)
        {
            throw new System.NotImplementedException();
        }

        public void SetOnActionPoltergeistPosition(Vector3 onActionPoltergeistPosition)
        {
            throw new System.NotImplementedException();
        }

        public void SetTargetGhost(Transform targetGhost)
        {
            throw new System.NotImplementedException();
        }

        public void SetTransactionGhostInStaticObjectStruct(GhostInStaticObjectStruct target)
        {
            switch (target.role)
            {
                case GhostRole.MidBoss:
                    _factory = new MidBossGhostFactory();

                    break;
            }
            var newStruct = _factory.CreateFrom(target);
            _transactionGhostInStaticObjectStruct = newStruct;
        }

        public void SubtractionTransactionGhostInStaticObjectStruct()
        {
            var ghostStuct = _transactionGhostInStaticObjectStruct;
            if (ghostStuct.ghostTeamID != null &&
                !string.IsNullOrEmpty(ghostStuct.ghostTeamID.Value) &&
                ghostStuct.useStatus.Equals(UseStatus.Using) &&
                0 < ghostStuct.membersCount)
            {
                ghostStuct.membersCount--;
            }
        }

        public void SetIsFailed(bool isFailed)
        {
            throw new System.NotImplementedException();
        }

        public void SetShoutNoteActive(bool shoutNoteActive)
        {
            throw new System.NotImplementedException();
        }

        public void SetEnemyBattlePart(EnemyBattlePart enemyBattlePart)
        {
            throw new System.NotImplementedException();
        }

        public void ReplaceGhostInStaticObjectStructs(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            throw new System.NotImplementedException();
        }
    }
}
