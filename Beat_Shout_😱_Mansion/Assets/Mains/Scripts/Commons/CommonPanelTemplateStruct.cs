using UnityEngine;

namespace Mains.Commons
{
    [System.Serializable]
    /// <summary>
    /// 共通UIのテンプレート構造体
    /// </summary>
    public struct CommonPanelTemplateStruct
    {
        [Tooltip("CommonPanel > HeaderPanel > IconAndGuidePanel > GuideText のテンプレートをセット")]
        /// <summary>ミッションガイド概要のテキスト</summary>
        public string guideText;
        [Tooltip("CommonPanel > HeaderPanel > MissionText のテンプレートをセット")]
        /// <summary>ミッションガイド詳細のテキスト</summary>
        public string missionText;
        [Tooltip("CommonPanel > HeaderPanel > dBLevelText のテンプレートをセット")]
        /// <summary>dB(A)を表示する用のテキスト</summary>
        public string dBLevelText;
    }
}
