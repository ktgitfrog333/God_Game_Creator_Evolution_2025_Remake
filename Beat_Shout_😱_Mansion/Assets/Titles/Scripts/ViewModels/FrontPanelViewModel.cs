using Titles.Models;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// 手前に表示するパネルのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "FrontPanelViewModel", menuName = "Scriptable Objects/FrontPanelViewModel")]
    public class FrontPanelViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] private LauncherModel model;
        /// <summary>自動ロードまでの制限時間</summary>
        public float autoLoadTimeLimit = .5f;
        /// <summary>ロード対象シーン</summary>
        public string loadSceneName = "TitleScene";

        public void Initialize()
        {
            model.Initialize();
        }

        public void SetIsActiveAdminMode(bool isActiveAdminMode)
        {
            model.SetIsActiveAdminMode(isActiveAdminMode);
        }

        public void Dispose()
        {
            model.Dispose();
        }
    }
}
