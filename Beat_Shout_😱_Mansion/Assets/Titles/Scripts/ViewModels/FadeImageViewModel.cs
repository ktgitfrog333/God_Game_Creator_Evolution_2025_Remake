using R3;
using Titles.Models;
using UnityEngine;

namespace Titles.ViewModels
{
    /// <summary>
    /// フェードイメージのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "FadeImageViewModel", menuName = "Scriptable Objects/FadeImageViewModel")]
    public class FadeImageViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] private LauncherModel model;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag;

        public void Initialize()
        {
            _disposableBag = new DisposableBag();

            model.Initialize();
        }

        public void SetCompletedSceneLoadDirectionType(int completedSceneLoadDirectionType)
        {
            model.SetCompletedSceneLoadDirectionType(completedSceneLoadDirectionType);
        }

        public void Dispose()
        {
            model.Dispose();

            _disposableBag.Dispose();
        }
    }
}
