using Mains.ViewModels;
using Mains.Views;
using Selects.ViewModels;
using System.Reflection;
using UnityEngine;

namespace Selects.Tests
{
    /// <summary>
    /// CommonPanelCustomizeOfMainViewのテスト
    /// </summary>
    /// <remarks>
    /// OnGUIボタンによる目視確認テスト<br/>
    /// パラメータは<see cref="StubSO"/>から取得
    /// </remarks>
    public class CommonPanelCustomizeOfMainViewTest : MonoBehaviour
    {
        [SerializeField] private StubSO stubSO;
        [SerializeField] private CommonPanelCustomizeOfMainViewModel viewModel;
        [SerializeField] private HPDownDirectionView hpDownDirectionView;
        // リフレクション用のフィールド情報（キャッシュ）
        private FieldInfo _hpDownDirectionViewModelField;

        private void OnGUI()
        {
            // テスト1: 最大HPがセットされることを確認
            Rect buttonRect1 = new Rect(10, 10, 400, 50);
            if (GUI.Button(buttonRect1, "最大HPがセットされることを確認"))
            {
                var stub = stubSO.commons.commonPanelCustomizeOfMainViewTest._最大HPがセットされることを確認;
                viewModel.PlayerHealthPointMax.Value = stub.playerHealthPointMax;
                Debug.Log($"[テスト1] PlayerHealthPointMax を {stub.playerHealthPointMax} にセットしました。RenderIconHeartImageViews の結果を目視で確認してください。");
            }

            // テスト2: 現在HPがセットされることを確認
            Rect buttonRect2 = new Rect(10, 70, 400, 50);
            if (GUI.Button(buttonRect2, "現在HPがセットされることを確認"))
            {
                var stub = stubSO.commons.commonPanelCustomizeOfMainViewTest._現在HPがセットされることを確認;
                viewModel.PlayerHealthPoint.Value = stub.playerHealthPoint;
                Debug.Log($"[テスト2] PlayerHealthPoint を {stub.playerHealthPoint} にセットしました。RenderIconHeartImageViewsCurrent の結果を目視で確認してください。");
            }

            // テスト3: ハートが減少する演出が実行されることを確認
            Rect buttonRect3 = new Rect(10, 130, 400, 50);
            if (GUI.Button(buttonRect3, "ハートが減少する演出が実行されることを確認"))
            {
                var stub = stubSO.commons.commonPanelCustomizeOfMainViewTest._ハートが減少する演出が実行されることを確認;
                // HPDownDirectionViewの_viewModelフィールドにリフレクションでアクセス
                if (_hpDownDirectionViewModelField == null)
                {
                    var viewType = typeof(HPDownDirectionView);
                    _hpDownDirectionViewModelField = viewType.GetField("_viewModel", BindingFlags.NonPublic | BindingFlags.Instance);
                }

                if (_hpDownDirectionViewModelField == null)
                {
                    Debug.LogError("[テスト3] _viewModel フィールドが見つかりませんでした");

                    return;
                }

                var hpDownDirectionViewModel = _hpDownDirectionViewModelField.GetValue(hpDownDirectionView) as HPDownDirectionViewModel;
                if (hpDownDirectionViewModel == null)
                {
                    Debug.LogError("[テスト3] HPDownDirectionViewModel が null です");

                    return;
                }

                hpDownDirectionViewModel.IsHitGhostAttack.Execute(stub.isHitGhostAttack);
                Debug.Log($"[テスト3] IsHitGhostAttack を {stub.isHitGhostAttack} で Execute しました。PlayHPDownDirection の結果を目視で確認してください。");
            }
        }
    }
}
