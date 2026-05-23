using System;

namespace Universal.Commons
{
    [Serializable]
    /// <summary>
    /// イベントの進捗状態を保持するクラス
    /// </summary>
    public class EventProgress
    {
        /// <summary>イベントID</summary>
        public int eventId;
        /// <summary>ステータス（0:未完了, 1:完了）</summary>
        public int status;

        public EventProgress() { }

        public EventProgress(int eventId, int status)
        {
            this.eventId = eventId;
            this.status = status;
        }

        public EventProgress(EventProgress other)
        {
            this.eventId = other.eventId;
            this.status = other.status;
        }
    }
}
