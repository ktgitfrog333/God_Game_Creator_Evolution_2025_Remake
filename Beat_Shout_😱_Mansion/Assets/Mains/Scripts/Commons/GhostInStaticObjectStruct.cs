using UnityEngine;
using R3;

namespace Mains.Commons
{
    /// <summary>
    /// オバケの家具入居管理のデータクラス
    /// </summary>
    /// <remarks>旧：オバケの家具入居管理の構造体</remarks>
    [System.Serializable]
    public class GhostInStaticObjectStruct
    {
        /// <summary>ポルターガイストのビューID</summary>
        public int poltergeistViewID;
        /// <summary>オバケ団体ID</summary>
        public ReactiveProperty<string> ghostTeamID = new ReactiveProperty<string>();
        /// <summary>利用ステータス（空室／使用中）</summary>
        public UseStatus useStatus;
        /// <summary>利用人数</summary>
        public int membersCount;
        /// <summary>オバケの攻撃タイプ</summary>
        public GhostAttackType attackType;
        /// <summary>オバケの移動タイプ</summary>
        public MoveType moveType;
        /// <summary>シャウト範囲（0等で未設定の場合はテーブルのデフォルト値を使用）</summary>
        public float customShoutRadius;
        /// <summary>音の出力タイプ</summary>
        public SoundOutputType soundOutputType;
        /// <summary>オバケの役割</summary>
        public GhostRole role;
        /// <summary>オバケモデルタイプ（FBX差分）</summary>
        public GhostModelType ghostModelType;
        /// <summary>オバケボイスタイプ（SE差分）</summary>
        public GhostVoiceType ghostVoiceType;

        public GhostInStaticObjectStruct()
        {

        }

        public GhostInStaticObjectStruct(GhostInStaticObjectStruct ghostInStaticObjectStruct)
        {
            poltergeistViewID = ghostInStaticObjectStruct.poltergeistViewID;
            string ghostTeamIDValue = ghostInStaticObjectStruct.ghostTeamID?.Value ?? null;
            ghostTeamID = new ReactiveProperty<string>();
            ghostTeamID.Value = ghostTeamIDValue != null ? ghostTeamIDValue : string.Empty;
            useStatus = ghostInStaticObjectStruct.useStatus;
            membersCount = ghostInStaticObjectStruct.membersCount;
            attackType = ghostInStaticObjectStruct.attackType;
            moveType = ghostInStaticObjectStruct.moveType;
            customShoutRadius = ghostInStaticObjectStruct.customShoutRadius;
            soundOutputType = ghostInStaticObjectStruct.soundOutputType;
            role = ghostInStaticObjectStruct.role;
            ghostModelType = ghostInStaticObjectStruct.ghostModelType;
            ghostVoiceType = ghostInStaticObjectStruct.ghostVoiceType;
        }
    }
}
