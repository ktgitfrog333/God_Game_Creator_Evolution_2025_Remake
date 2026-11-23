using UnityEngine;

namespace Selects.Tests
{
    public class BGMPartControllerTest : MonoBehaviour
    {
        [SerializeField] private StubSO stubSO;
        [SerializeField] private BGMPartController bGMPartController;

        private void OnGUI()
        {
            var bGMPartControllerTest = stubSO.other.bGMPartControllerTest;
            Rect buttonRect1 = new Rect(10, 10, 400, 50);

            if (GUI.Button(buttonRect1, "フェード: BGM_A to BGM_B"))
            {
                bGMPartController.ChangeBgmFromCurrentTo(bGMPartControllerTest.bgmB);
            }

            Rect buttonRect2 = new Rect(10, 70, 400, 50);

            if (GUI.Button(buttonRect2, "フェード: BGM_B to BGM_A"))
            {
                bGMPartController.ChangeBgmFromCurrentTo(bGMPartControllerTest.bgmA);
            }

            Rect buttonRect3 = new Rect(10, 130, 400, 50);

            if (GUI.Button(buttonRect3, "フェード: BGM_A to BGM_C"))
            {
                bGMPartController.ChangeBgmFromCurrentTo(bGMPartControllerTest.bgmC);
            }

            Rect buttonRect4 = new Rect(10, 190, 400, 50);

            if (GUI.Button(buttonRect4, "フェード: BGM_C to BGM_A"))
            {
                bGMPartController.ChangeBgmFromCurrentTo(bGMPartControllerTest.bgmA);
            }

            Rect buttonRect5 = new Rect(10, 250, 400, 50);

            if (GUI.Button(buttonRect5, "プレイ: BGM_A"))
            {
                bGMPartController.PlayBgm(bGMPartControllerTest.bgmA);
            }
        }
    }
}
