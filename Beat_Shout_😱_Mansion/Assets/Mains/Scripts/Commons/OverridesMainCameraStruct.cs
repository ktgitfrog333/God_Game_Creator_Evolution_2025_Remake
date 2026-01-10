using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// Main Cameraをオーバーライドする構造体
    /// </summary>
    [System.Serializable]
    public struct OverridesMainCameraStruct
    {
        public bool 独自ポストプロセス設定を有効にする;
        public LayerMask PostProcessing用のLayer;
    }
}
