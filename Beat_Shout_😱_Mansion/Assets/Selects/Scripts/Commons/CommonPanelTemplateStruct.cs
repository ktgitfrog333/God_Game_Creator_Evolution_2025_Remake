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
        [TextArea] public string enterRoomText;
        [Tooltip("CommonPanel > CenterPanel > ReturnTitleScenePanel > EnterRoomText のテンプレートをセット\nチュートリアル用")]
        [TextArea] public string enterRoomTextPlayingTutorial;
    }
}
