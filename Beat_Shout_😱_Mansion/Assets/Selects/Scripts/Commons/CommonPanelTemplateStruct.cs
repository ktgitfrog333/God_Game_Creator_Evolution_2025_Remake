using UnityEngine;

namespace Selects.Commons
{
    [System.Serializable]
    /// <summary>
    /// 共通UIのテンプレート構造体
    /// </summary>
    public struct CommonPanelTemplateStruct
    {
        [Tooltip("CommonPanel > CenterPanel > ReturnTitleScenePanel > EnterRoomText のテンプレートをセット")]
        public string enterRoomText;
    }
}
