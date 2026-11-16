using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// オブジェクトをホーミングする処理のカスタマイズテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "HomingObjectCustomizeTable", menuName = "Scriptable Objects/HomingObjectCustomizeTable")]
    public class HomingObjectCustomizeTable : ScriptableObject
    {
        [Tooltip("Assets/Mains/Prefabs/UIs/DurabilityRatePanel.prefabをセットしておく。")]
        public Transform durabilityRatePanelPrefab;
        /// <summary>ロングノーツのガイドメッセージ設定配列</summary>
        public GuideMessageLongNote[] guideMessageLongNotes;
        /// <summary>ショートノーツのガイドメッセージ設定</summary>
        public GuideMessageShortNote[] guideMessageShortNotes;

        /// <summary>
        /// ロングノーツのガイドメッセージを取得
        /// </summary>
        /// <param name="currentScneName">ターゲットシーン名</param>
        /// <returns>ロングノーツのガイドメッセージ（クリック開始メッセージ, 離すメッセージ）</returns>
        public (string, string) GetGuideMessageLongNote(string currentScneName)
        {
            var tmpGuideMessageLongNotes = guideMessageLongNotes;
            // クリック開始メッセージ
            string clickStartMessage = string.Empty;
            // 離すメッセージ
            string releaseMessage = string.Empty;

            foreach (var guideMessageLongNote in guideMessageLongNotes)
            {
                if (guideMessageLongNote.targetSceneName.Equals(currentScneName))
                {
                    switch (guideMessageLongNote.actionType)
                    {
                        case 1:
                            releaseMessage = guideMessageLongNote.message;

                            break;
                        default:
                            clickStartMessage = guideMessageLongNote.message;

                            break;
                    }
                }
            }

            return (clickStartMessage, releaseMessage);
        }

        /// <summary>
        /// ショートノーツのガイドメッセージを取得
        /// </summary>
        /// <param name="currentScneName">ターゲットシーン名</param>
        /// <returns>ショートノーツのガイドメッセージ</returns>
        public string GetGuideMessageShortNote(string currentScneName)
        {
            foreach (var guideMessageShortNote in guideMessageShortNotes)
            {
                if (guideMessageShortNote.targetSceneName.Equals(currentScneName))
                {
                    return guideMessageShortNote.message;
                }
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// ロングノーツのガイドメッセージ設定
    /// </summary>
    [System.Serializable]
    public class GuideMessageLongNote
    {
        /// <summary>ID</summary>
        public int id;
        /// <summary>ターゲットシーン名</summary>
        public string targetSceneName;
        /// <summary>アクションタイプ</summary>
        /// <remarks>0: 押下開始<br/>
        /// 1: 離す</remarks>
        public int actionType;
        /// <summary>メッセージ</summary>
        public string message;
    }

    /// <summary>
    /// ショートノーツのガイドメッセージ設定
    /// </summary>
    [System.Serializable]
    public class GuideMessageShortNote
    {
        /// <summary>ID</summary>
        public int id;
        /// <summary>ターゲットシーン名</summary>
        public string targetSceneName;
        /// <summary>メッセージ</summary>
        public string message;
    }
}
