using Mains.Commons;
using Mains.ViewModels;
using ObservableCollections;
using System.Linq;
using UnityEngine;

namespace Mains.Models
{
    /// <summary>
    /// オバケ管理
    /// </summary>
    public class GhostContainer
    {
        /// <summary>オバケの家具入居管理のデータクラスリスト</summary>
        private ObservableList<GhostInStaticObjectStruct> _ghosts;
        /// <summary>オバケの家具入居管理のデータクラスリスト</summary>
        public ObservableList<GhostInStaticObjectStruct> Ghosts => _ghosts != null ? _ghosts : _ghosts = new ObservableList<GhostInStaticObjectStruct>();
        /// <summary>オバケの家具入居管理のファクトリ用インターフェース</summary>
        private IGhostStructFactory _factory;

        public GhostContainer(IGhostStructFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// オバケの家具入居管理のデータクラスリストへ追加
        /// </summary>
        /// <param name="ghost">オバケの家具入居管理のデータクラス</param>
        public void Add(GhostInStaticObjectStruct ghost)
        {
            Ghosts.Add(ghost);
        }

        /// <summary>
        /// オバケの家具入居管理のデータクラスリストのレコードを空にする
        /// </summary>
        /// <param name="ghost">オバケの家具入居管理のデータクラス</param>
        public void Reset(GhostInStaticObjectStruct target)
        {
            // 移動元の家具のポルターガイスト情報は初期化
            var prevIndex = Ghosts
                .Select((p, i) => new { Content = p, Index = i })
                .FirstOrDefault(x => x.Content.poltergeistViewID == target.poltergeistViewID)
                .Index;
            switch (target.role)
            {
                case GhostRole.MidBoss:
                    _factory = new MidBossGhostFactory();

                    break;
            }
            var newStruct = _factory.CreateEmpty(target);
            Ghosts[prevIndex] = newStruct;
        }

        /// <summary>
        /// オバケの家具入居管理のデータクラスリストにてレコードをシャッフルする
        /// </summary>
        /// <param name="target">オバケの家具入居管理のデータクラス</param>
        public void Shuffle(GhostInStaticObjectStruct target)
        {
            var ghostStructs = Ghosts;
            // 空いているポルターガイストのインデックスを取得
            var emptyGhostStructIndices = ghostStructs
                .Select((p, i) => new { Content = p, Index = i })
                .Where(x => x.Content.useStatus.Equals(UseStatus.Empty))
                .Select(x => x.Index)
                .ToList();

            // 空きがある場合のみ処理を続行
            if (0 < emptyGhostStructIndices.Count)
            {
                // インデックスをランダムで選択
                int randomIndex = emptyGhostStructIndices[Random.Range(0, emptyGhostStructIndices.Count)];

                // 移動先の家具へポルターガイスト情報を更新
                var nextGhostInStaticObjectStruct = new GhostInStaticObjectStruct();
                nextGhostInStaticObjectStruct.poltergeistViewID = Ghosts[randomIndex].poltergeistViewID;
                nextGhostInStaticObjectStruct.ghostTeamID = Ghosts[randomIndex].ghostTeamID;
                nextGhostInStaticObjectStruct.ghostTeamID.Value = target.ghostTeamID.Value;
                nextGhostInStaticObjectStruct.useStatus = target.useStatus;
                nextGhostInStaticObjectStruct.membersCount = target.membersCount;
                nextGhostInStaticObjectStruct.attackType = target.attackType;
                nextGhostInStaticObjectStruct.moveType = target.moveType;
                nextGhostInStaticObjectStruct.customShoutRadius = target.customShoutRadius;
                nextGhostInStaticObjectStruct.soundOutputType = target.soundOutputType;
                nextGhostInStaticObjectStruct.role = target.role;
                Ghosts[randomIndex] = nextGhostInStaticObjectStruct;

                Reset(target);
            }
            else
            {
                Debug.Log("移動できる空きがありません。");
            }
        }

        /// <summary>
        /// オバケの家具入居管理のデータクラスリストにて対象レコードを更新する
        /// </summary>
        /// <param name="target">オバケの家具入居管理のデータクラス</param>
        public void Replace(GhostInStaticObjectStruct target)
        {
            var ghostStructs = Ghosts;
            // IDと紐づくポルターガイストのインデックスを取得
            var targetGhostStructIndices = ghostStructs
                .Select((p, i) => new { Content = p, Index = i })
                .Where(x => x.Content.poltergeistViewID == target.poltergeistViewID)
                .Select(x => x.Index)
                .ToList();
            if (1 == targetGhostStructIndices.Count)
            {
                var targetGhostStructIndex = targetGhostStructIndices[0];
                // IDと紐づく家具へポルターガイスト情報を更新
                var nextGhostInStaticObjectStruct = new GhostInStaticObjectStruct();
                nextGhostInStaticObjectStruct.poltergeistViewID = Ghosts[targetGhostStructIndex].poltergeistViewID;
                nextGhostInStaticObjectStruct.ghostTeamID = Ghosts[targetGhostStructIndex].ghostTeamID;
                nextGhostInStaticObjectStruct.ghostTeamID.Value = target.ghostTeamID.Value;
                nextGhostInStaticObjectStruct.useStatus = target.useStatus;
                nextGhostInStaticObjectStruct.membersCount = target.membersCount;
                nextGhostInStaticObjectStruct.attackType = target.attackType;
                nextGhostInStaticObjectStruct.moveType = target.moveType;
                nextGhostInStaticObjectStruct.customShoutRadius = target.customShoutRadius;
                nextGhostInStaticObjectStruct.soundOutputType = target.soundOutputType;
                nextGhostInStaticObjectStruct.role = target.role;
                Ghosts[targetGhostStructIndex] = nextGhostInStaticObjectStruct;
            }
            else
            {
                Debug.LogWarning("データが空もしくは重複しています");
            }
        }
    }
}
