using Titles.Models;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// マイク入力パネルのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "InputMicPanelViewModel", menuName = "Scriptable Objects/InputMicPanelViewModel")]
    public class InputMicPanelViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] private LauncherModel model;

        public void Initialize()
        {
            model.Initialize();
        }

        public void AddMessages(string message)
        {
            model.AddMessages(message);
        }

        public void Dispose()
        {
            model.Dispose();
        }
    }
}
