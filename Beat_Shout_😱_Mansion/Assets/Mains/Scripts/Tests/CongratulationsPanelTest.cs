using Cysharp.Threading.Tasks;
using Mains.Views;
using R3;
using UnityEngine;

namespace Mains.Tests
{
    /// <summary>
    /// コングラチュレーションのテスト
    /// </summary>
    public class CongratulationsPanelTest : MonoBehaviour
    {
        [SerializeField] private CongratulationsPanelView congratulationsPanelView;
        private DisposableBag _disposableBag = new DisposableBag();

        private void OnGUI()
        {
            int y = 10;
            const int buttonWidth = 600;
            const int buttonHeight = 35;
            //const int spacing = 45;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "EnableAndPlayFadeInCongratulations"))
            {
                congratulationsPanelView.EnableAndPlayFadeInCongratulations().Subscribe(_ =>
                {
                    Debug.Log($"パネルを有効にして、コングラチュレーションのアニメーションを再生する処理の完了を通知");
                })
                    .AddTo(ref _disposableBag);
            }
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }
    }
}
