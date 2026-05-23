using UnityEngine;
using Mains.Commons;

namespace Selects.Commons
{
    /// <summary>
    /// メッセージデータ
    /// </summary>
    [System.Serializable]
    public class MessageData
    {
        /// <summary>メッセージID</summary>
        public string messageId;
        /// <summary>メインメッセージ</summary>
        [TextArea]
        public string mainMessage;
        /// <summary>サブメッセージ</summary>
        [TextArea]
        public string subMessage;
        /// <summary>進捗テキスト</summary>
        [TextArea]
        public string progressText;
        /// <summary>インタラクト有り</summary>
        public bool hasInteract;
        /// <summary>電池落下タイプ</summary>
        public BatteryDropType batteryDropType;
    }

    /// <summary>
    /// メッセージの設定
    /// </summary>
    [CreateAssetMenu(fileName = "MessageTable", menuName = "Scriptable Objects/MessageTable")]
    public class MessageTable : ScriptableObject
    {
        /// <summary>メッセージデータ配列</summary>
        public MessageData[] list;

        /// <summary>
        /// メッセージIDからメッセージデータを取得
        /// </summary>
        /// <param name="messageId">メッセージID</param>
        /// <returns>メッセージデータ</returns>
        public MessageData Get(string messageId)
        {
            if (list == null) return null;
            foreach (var data in list)
            {
                if (data.messageId == messageId) return data;
            }
            return null;
        }
    }
}
