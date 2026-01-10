using UnityEngine;

namespace Selects.Commons
{
    /// <summary>
    /// Startイベント呼び出しを順番通りにするためのレイヤーデータ構造体
    /// </summary>
    /// <remarks>通常は使わない想定<br/>
    /// 一部のObserverを使用したコンポーネントのみ<br/>
    /// 個々のコンポーネントに対してStartを待ってから順番に生成したい場合に利用する</remarks>
    [System.Serializable]
    public struct DidStartLayerStruct
    {
        /// <summary>スクリプト名</summary>
        public string scriptName;
    }
}
