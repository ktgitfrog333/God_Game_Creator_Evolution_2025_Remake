using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// オバケの家具入居管理のファクトリ用インターフェース
    /// </summary>
    public interface IGhostStructFactory
    {
        /// <summary>
        /// IDを引き継いだ空オブジェクトを作成
        /// </summary>
        /// <param name="baseData">オバケの家具入居管理のデータクラス</param>
        /// <returns>IDを引き継いだ空オブジェクト</returns>
        GhostInStaticObjectStruct CreateEmpty(GhostInStaticObjectStruct baseData);
        /// <summary>
        /// 元オブジェクト情報を引き継いだオブジェクトを作成
        /// </summary>
        /// <param name="baseData">オバケの家具入居管理のデータクラス</param>
        /// <returns>元オブジェクト情報を引き継いだオブジェクト</returns>
        GhostInStaticObjectStruct CreateFrom(GhostInStaticObjectStruct baseData);
    }
}
