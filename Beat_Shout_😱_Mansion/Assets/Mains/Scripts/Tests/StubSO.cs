using Mains.Commons;
using System.Collections.Generic;
using UnityEngine;

namespace Mains.Tests
{
    /// <summary>
    /// スタブ
    /// </summary>
    [CreateAssetMenu(fileName = "StubSO", menuName = "Scriptable Objects/StubSO")]
    public class StubSO : ScriptableObject
    {
        public Commons commons;
    }

    [System.Serializable]
    public class Commons
    {
        public MidBossGhostFactory midBossGhostFactory;
        public NormalGhostFactory normalGhostFactory;
        public PoltergeistTable poltergeistTable;
        public HomingObjectCustomizeViewModel homingObjectCustomizeViewModel;
        public MissGhostAttackCustomizeViewModel missGhostAttackCustomizeViewModel;

        [System.Serializable]
        public class MidBossGhostFactory
        {
            public CreateEmpty呼び出し時に初期値になること CreateEmpty呼び出し時に初期値になること;
            public CreateFrom呼び出し時にbaseの値になること CreateFrom呼び出し時にbaseの値になること;
        }

        [System.Serializable]
        public class NormalGhostFactory
        {
            public CreateEmpty呼び出し時に初期値になること CreateEmpty呼び出し時に初期値になること;
            public CreateFrom呼び出し時にbaseの値になること CreateFrom呼び出し時にbaseの値になること;
        }

        [System.Serializable]
        public class CreateEmpty呼び出し時に初期値になること
        {
            public Input[] inputs;
            public Output[] outputs;

            [System.Serializable]
            public class Input
            {
                public GhostInStaticObjectStruct ghostInStaticObjectStruct;
                public string ghostTeamID;
            }

            [System.Serializable]
            public class Output
            {
                public GhostInStaticObjectStruct ghostInStaticObjectStruct;
                public string ghostTeamID;
            }
        }

        [System.Serializable]
        public class CreateFrom呼び出し時にbaseの値になること
        {
            public Input[] inputs;
            public Output[] outputs;

            [System.Serializable]
            public class Input
            {
                public GhostInStaticObjectStruct ghostInStaticObjectStruct;
                public string ghostTeamID;
            }

            [System.Serializable]
            public class Output
            {
                public GhostInStaticObjectStruct ghostInStaticObjectStruct;
                public string ghostTeamID;
            }
        }

        [System.Serializable]
        public class PoltergeistTable
        {
            public マッピングリストとオバケモデルタイプを渡して対応するプレハブが取得できること _マッピングリストとオバケモデルタイプを渡して対応するプレハブが取得できること;

            [System.Serializable]
            public class マッピングリストとオバケモデルタイプを渡して対応するプレハブが取得できること
            {
                public Input[] inputs;
                public Output[] outputs;

                [System.Serializable]
                public class Input
                {
                    public List<GhostModelTypePrefabMapping> mappings;
                    public GhostModelType modelType;
                }

                [System.Serializable]
                public class Output
                {
                    public Transform transform;
                }
            }
        }

        [System.Serializable]
        public class HomingObjectCustomizeViewModel
        {
            public 現在のトランザクション中のオバケボイスタイプが取得できること _現在のトランザクション中のオバケボイスタイプが取得できること;
            public 現在のトランザクション中のオバケモデルタイプが取得できること _現在のトランザクション中のオバケモデルタイプが取得できること;

            [System.Serializable]
            public class 現在のトランザクション中のオバケボイスタイプが取得できること
            {
                public Input[] inputs;
                public Output[] outputs;

                [System.Serializable]
                public class Input
                {
                    public PlayerModel playerModel;
                    public bool isNullPlayerModel;

                    [System.Serializable]
                    public class PlayerModel
                    {
                        public GhostInStaticObjectStruct TransactionGhostInStaticObjectStruct;
                        public string ghostTeamID;
                    }
                }

                [System.Serializable]
                public class Output
                {
                    public GhostVoiceType ghostVoiceType;
                }
            }

            [System.Serializable]
            public class 現在のトランザクション中のオバケモデルタイプが取得できること
            {
                public Input[] inputs;
                public Output[] outputs;

                [System.Serializable]
                public class Input
                {
                    public PlayerModel playerModel;
                    public bool isNullPlayerModel;

                    [System.Serializable]
                    public class PlayerModel
                    {
                        public GhostInStaticObjectStruct TransactionGhostInStaticObjectStruct;
                        public string ghostTeamID;
                    }
                }

                [System.Serializable]
                public class Output
                {
                    public GhostModelType ghostModelType;
                }
            }
        }

        [System.Serializable]
        public class MissGhostAttackCustomizeViewModel
        {
            public 現在のトランザクション中のオバケモデルタイプが取得できること _現在のトランザクション中のオバケモデルタイプが取得できること;

            [System.Serializable]
            public class 現在のトランザクション中のオバケモデルタイプが取得できること
            {
                public Input[] inputs;
                public Output[] outputs;

                [System.Serializable]
                public class Input
                {
                    public PlayerModel playerModel;
                    public bool isNullPlayerModel;

                    [System.Serializable]
                    public class PlayerModel
                    {
                        public GhostInStaticObjectStruct TransactionGhostInStaticObjectStruct;
                        public string ghostTeamID;
                    }
                }

                [System.Serializable]
                public class Output
                {
                    public GhostModelType ghostModelType;
                }
            }
        }
    }
}
