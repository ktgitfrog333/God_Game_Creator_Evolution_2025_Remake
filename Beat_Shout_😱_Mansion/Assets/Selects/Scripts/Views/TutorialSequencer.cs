using Cysharp.Threading.Tasks;
using Mains.Commons;
using R3;
using Selects.Commons;
using System.Threading;
using Universal.Commons;
using Universal.Utilities;
using UnityEngine;

namespace Selects.Views
{
    /// <summary>
    /// チュートリアルのフロー制御のみを担う純粋C#クラス。
    /// View（MonoBehaviour）・Rewired・xyloApi には直接依存しない。
    /// すべての外部依存は TutorialSequencerContext 経由でインターフェース越しに受け取る。
    /// </summary>
    public class TutorialSequencer
    {
        /// <summary>コンテキスト</summary>
        private readonly TutorialSequencerContext _ctx;
        /// <summary>ユーザー情報を保持するクラス</summary>
        private UserBean _userBean;

        public TutorialSequencer(TutorialSequencerContext context)
        {
            _ctx = context;
        }

        // =========================================================
        // エントリポイント
        // =========================================================

        /// <summary>
        /// チュートリアル全体を順に実行する。
        /// TutorialPanelView.Start() から呼ばれる。
        /// </summary>
        /// <param name="userBean">ユーザー情報を保持するクラス</param>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        public async UniTask RunAsync(UserBean userBean, CancellationToken token)
        {
            _userBean = userBean;

            if (_userBean == null || TutorialConditionEvaluator.ShouldSkip(_userBean))
                return;

            if (TutorialConditionEvaluator.ShouldRunMove(_userBean))
                await RunMoveTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunAimMove(_userBean))
                await RunAimMoveTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunShout(_userBean))
                await RunShoutTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunRhythm(_userBean))
                await RunRhythmTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunStage1Guide(_userBean))
                await RunStage1GuideTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunShoutNoteGuide(_userBean))
                await RunShoutNoteGuideTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunShoutNote(_userBean))
                await RunShoutNoteTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunStage3Guide(_userBean))
                await RunStage3GuideTutorialAsync(token);
        }

        // =========================================================
        // 各ステップ共通の初期化処理
        // =========================================================

        /// <summary>
        /// 各チュートリアルステップの冒頭で呼び出す共通処理。
        /// Rewired操作の無効化、マイクの無効化、敵戦パートをチュートリアルに設定する。
        /// </summary>
        private void InitializeStep()
        {
            var vm = _ctx.ViewModel;
            var input = _ctx.Input;
            var side = _ctx.SideEffect;

            vm.SetEnemyBattlePart(EnemyBattlePart.Tutorial);
            side.SetMicrophoneActive(false);
            input.EnableOnlyControllerMapCategory(null);
        }

        // =========================================================
        // 各チュートリアルステップ
        // =========================================================

        /// <summary>
        /// チュートリアル_移動の実行
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunMoveTutorialAsync(CancellationToken token)
        {
            var ui = _ctx.UI;
            var input = _ctx.Input;
            var side = _ctx.SideEffect;
            var vm = _ctx.ViewModel;
            var lvl = _ctx.LevelObjects;

            InitializeStep();

            var playerTransform = vm.PlayerTransform;
            var flashLight = vm.PlayerFlashLight;
            if (flashLight != null) flashLight.gameObject.SetActive(false);

            var headerPanel = vm.CommonHeaderPanelRectTrans;
            if (headerPanel != null) headerPanel.gameObject.SetActive(false);

            // --- 前進 ---
            ui.ApplyMessage("MSG0000");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialForwardOnly");

            Vector3 startPos = playerTransform.position;
            await Observable.EveryUpdate()
                .Where(_ => input.MoveVertical > 0.1f)
                .Where(_ => Vector3.Distance(startPos, playerTransform.position) > 1.5f)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- 後退 ---
            ui.ApplyMessage("MSG0001");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialBackOnly");

            startPos = playerTransform.position;
            await Observable.EveryUpdate()
                .Where(_ => input.MoveVertical < -0.1f)
                .Where(_ => Vector3.Distance(startPos, playerTransform.position) > 1.5f)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- 左移動 ---
            ui.ApplyMessage("MSG0002");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialLeftOnly");

            startPos = playerTransform.position;
            await Observable.EveryUpdate()
                .Where(_ => input.MoveHorizontal < -0.1f)
                .Where(_ => Vector3.Distance(startPos, playerTransform.position) > 1.5f)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- 右移動 ---
            ui.ApplyMessage("MSG0003");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialRightOnly");

            startPos = playerTransform.position;
            await Observable.EveryUpdate()
                .Where(_ => input.MoveHorizontal > 0.1f)
                .Where(_ => Vector3.Distance(startPos, playerTransform.position) > 1.5f)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- 懐中電灯の取得 ---
            ui.ApplyMessage("MSG0004");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearch");

            if (lvl.lightRingParticleSys != null) lvl.lightRingParticleSys.SetActive(true);
            if (lvl.flashLightItem != null) lvl.flashLightItem.SetActive(true);

            // OnTriggerEnterベースの接触判定（SearchRangeView準拠）
            await vm.FlashLightTriggerStay
                .Where(x => x)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- 懐中電灯 取得ボタン押下 ---
            ui.ApplyMessage("MSG0005");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearch");
            
            // トリガー接触中 かつ Searchボタン押下を待つ
            await Observable.EveryUpdate()
                .Where(_ => vm.FlashLightTriggerStay.CurrentValue)
                .Where(_ => input.SearchButtonDown)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            if (lvl.flashLightItem != null) lvl.flashLightItem.SetActive(false);
            if (lvl.lightRingParticleSys != null) lvl.lightRingParticleSys.SetActive(false);
            if (flashLight != null) flashLight.gameObject.SetActive(true);

            SaveEventProgress((int)TutorialEventId.ETB0000);
        }

        /// <summary>
        /// チュートリアル_視点移動
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunAimMoveTutorialAsync(CancellationToken token)
        {
            var ui = _ctx.UI;
            var input = _ctx.Input;
            var vm = _ctx.ViewModel;
            var lvl = _ctx.LevelObjects;
            var playerTransform = vm.PlayerTransform;

            InitializeStep();

            var sp = lvl.moveCompletePoint;
            if (sp != null)
            {
                await _ctx.SideEffect.TeleportPlayerAsync(sp.position, sp.eulerAngles, token);
            }

            // --- 視点移動 ---
            ui.ApplyMessage("MSG0006");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");

            float lookTimer = 0f;
            await Observable.EveryUpdate()
                .Where(_ =>
                {
                    if (IsLookingAt(lvl.batteryItem))
                        lookTimer += Time.deltaTime;
                    else
                        lookTimer = 0f;
                    return lookTimer >= 0.25f;
                })
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- 電池の取得 ---
            ui.ApplyMessage("MSG0007");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMove");

            if (lvl.lightRingParticleSys1 != null) lvl.lightRingParticleSys1.SetActive(true);
            if (lvl.batteryItem != null) lvl.batteryItem.SetActive(true);

            await vm.BatteryTriggerStay
                .Where(x => x)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- 電池 取得ボタン押下 ---
            ui.ApplyMessage("MSG0008");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMove");

            await Observable.EveryUpdate()
                .Where(_ => vm.BatteryTriggerStay.CurrentValue)
                .Where(_ => input.SearchButtonDown)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            if (lvl.batteryItem != null) lvl.batteryItem.SetActive(false);
            if (lvl.lightRingParticleSys1 != null) lvl.lightRingParticleSys1.SetActive(false);
            
            if (lvl.aimMoveCompletePoint != null)
            {
                await _ctx.SideEffect.TeleportPlayerAsync(
                    lvl.aimMoveCompletePoint.position,
                    lvl.aimMoveCompletePoint.eulerAngles,
                    token
                );
            }

            SaveEventProgress((int)TutorialEventId.ETB0001);
        }

        /// <summary>
        /// チュートリアル_シャウト
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunShoutTutorialAsync(CancellationToken token)
        {
            var ui = _ctx.UI;
            var input = _ctx.Input;
            var side = _ctx.SideEffect;
            var vm = _ctx.ViewModel;
            var lvl = _ctx.LevelObjects;
            var playerTransform = vm.PlayerTransform;

            InitializeStep();

            if (lvl.aimMoveCompletePoint != null)
            {
                await _ctx.SideEffect.TeleportPlayerAsync(
                    lvl.aimMoveCompletePoint.position,
                    lvl.aimMoveCompletePoint.eulerAngles,
                    token
                );
            }

            // --- ステップ_0（視点操作にてオバケを探す） ---
            ui.ApplyMessage("MSG0006");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");
            if (lvl.missGhostEscapeNormal != null) lvl.missGhostEscapeNormal.SetActive(true);

            float lookTimer = 0f;
            await Observable.EveryUpdate()
                .Where(_ =>
                {
                    if (lvl.missGhostEscapeNormal == null) return true;
                    if (IsLookingAt(lvl.missGhostEscapeNormal))
                        lookTimer += Time.deltaTime;
                    else
                        lookTimer = 0f;
                    return lookTimer >= 0.25f;
                })
                .FirstAsync(token);

            await UniTask.Delay(1000, cancellationToken: token); // オバケ移動アニメの完了待機（簡易代用）

            await FadeOutAndResetAsync(token);
            if (lvl.missGhostEscapeNormal != null) lvl.missGhostEscapeNormal.SetActive(false);

            // --- ステップ_1（オバケを追いかける） ---
            ui.ApplyMessage("MSG0009");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMove");

            if (lvl.lightRingParticleSys2 != null) lvl.lightRingParticleSys2.SetActive(true);

            await vm.LightRing2TriggerStay
                .Where(x => x)
                .FirstAsync(token);

            if (lvl.lightRingParticleSys2 != null) lvl.lightRingParticleSys2.SetActive(false);
            await FadeOutAndResetAsync(token);

            // --- ステップ_2（視点操作にてオバケが隠れた家具を探す） ---
            ui.ApplyMessage("MSG0006");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");

            if (lvl.vaseAndDeskGroup != null) lvl.vaseAndDeskGroup.SetActive(true);

            lookTimer = 0f;
            await Observable.EveryUpdate()
                .Where(_ =>
                {
                    if (lvl.vaseAndDeskGroup == null) return true;
                    if (IsLookingAt(lvl.vaseAndDeskGroup))
                        lookTimer += Time.deltaTime;
                    else
                        lookTimer = 0f;
                    return lookTimer >= 0.25f;
                })
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_3（シャウトチャンスパート切り替え） ---
            ui.ApplyMessage("MSG0010");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialSwitchPartOnly");

            await Observable.EveryUpdate()
                .Where(_ => input.SwitchPartButtonDown)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_4（シャウト練習） ---
            ui.ApplyMessage("MSG0011");
            await ui.FadeInAsync(0.5f, token);
            side.SetMicrophoneActive(true);

            await vm.DbLevelReactive
                .Where(db => db > 0.5f)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_5（シャウト本番） ---
            ui.ApplyMessage("MSG0012");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialForwardOnly");
            side.SetMicrophoneActive(true);

            await vm.DbLevelReactive
                .Where(db => db > 0.5f)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            SaveEventProgress((int)TutorialEventId.ETB0002);
        }

        /// <summary>
        /// チュートリアル_リズムパート
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunRhythmTutorialAsync(CancellationToken token)
        {
            var ui = _ctx.UI;
            var input = _ctx.Input;
            var vm = _ctx.ViewModel;
            var side = _ctx.SideEffect;
            var tables = _ctx.Tables;
            var lvl = _ctx.LevelObjects;

            InitializeStep();

            // --- ステップ_0（オバケ出現テロップ） ---
            ui.ApplyMessage("MSG0013");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");
            if (lvl.vaseAndDeskGroup != null) lvl.vaseAndDeskGroup.SetActive(true);
            side.SetAllNotesClickDetection(false);
            
            side.WatchFirstHomingObjectSpawn();
            await side.OnFirstHomingObjectSpawned.FirstAsync(token);

            Time.timeScale = 0f;
            side.SetBgmPause(true);

            await vm.EventStateReactive
                .Where(x => x == EnumEventCommand.Submited)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_1（ターゲットクロス操作について） ---
            ui.ApplyMessage("MSG0014");
            await ui.FadeInAsync(0.5f, token);
            Time.timeScale = 1f;
            side.SetBgmPause(false);

            await side.OnGhostHomingStarted().FirstAsync(token);
            Time.timeScale = 0f;
            side.SetBgmPause(true);
            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");

            await Observable.EveryUpdate()
                .Where(_ => side.GetNoteToCrosshairScreenDistance() <= 1.0f)
                .FirstAsync(token);
            await FadeOutAndResetAsync(token);

            // --- ステップ_2（ノーツクリックについて） ---
            ui.ApplyMessage("MSG0015");
            await ui.FadeInAsync(0.5f, token);
            Time.timeScale = 1f;
            side.SetBgmPause(false);

            await Observable.EveryUpdate()
                .Where(_ => side.IsAnyShortNoteClickable())
                .FirstAsync(token);

            Time.timeScale = 0f;
            side.SetBgmPause(true);
            input.EnableOnlyControllerMapCategory("CategoryTutorialTapLightOnly");

            await Observable.EveryUpdate()
                .Where(_ => input.TapLightButtonDown)
                .FirstAsync(token);
            side.ForceClickAnyClickableNote();
            
            Time.timeScale = 1f;
            side.SetBgmPause(false);
            await FadeOutAndResetAsync(token);

            // --- ステップ_3（ノーツクリック本番） ---
            var patternData = tables.missilePatternTable.Get("SMP0000");
            if (patternData != null) side.SetMissilePattern(patternData.pattern);
            string total = patternData != null ? patternData.successCount.ToString() : "3";

            ui.ApplyMessageWithProgress("MSG0016", "0", total);
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            side.SetAllNotesClickDetection(true);

            int successCount = 0;
            int targetCount = patternData != null ? patternData.successCount : 3;
            await side.OnNoteSuccessful
                .Where(success => success)
                .Do(_ => successCount++)
                .Where(_ => successCount >= targetCount)
                .FirstAsync(token);
            await FadeOutAndResetAsync(token);

            // --- ステップ_4（ロングノーツクリックについて） ---
            ui.ApplyMessage("MSG0017");
            await ui.FadeInAsync(0.5f, token);
            var patternData1 = tables.missilePatternTable.Get("SMP0001");
            if (patternData1 != null) side.SetMissilePattern(patternData1.pattern);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            side.SetAllNotesClickDetection(true);

            await Observable.EveryUpdate()
                .Where(_ => side.IsAnyLongNoteClickable())
                .FirstAsync(token);

            Time.timeScale = 0f;
            side.SetBgmPause(true);

            await Observable.EveryUpdate()
                .Where(_ => input.TapLightButtonDown)
                .FirstAsync(token);
            side.ForceClickAnyClickableNote();

            await FadeOutAndResetAsync(token);

            // --- ステップ_5（ロングノーツリリースについて） ---
            ui.ApplyMessage("MSG0018");
            await ui.FadeInAsync(0.5f, token);
            Time.timeScale = 1f;
            side.SetBgmPause(false);

            await UniTask.WhenAny(
                side.OnNoteSuccessful.Where(x => x).FirstAsync(token).AsUniTask(),
                side.OnNoteFailed.Where(x => x).FirstAsync(token).AsUniTask()
            );
            await FadeOutAndResetAsync(token);

            // --- ステップ_6（ロングノーツクリック＆リリース本番） ---
            var patternData2 = tables.missilePatternTable.Get("SMP0002");
            if (patternData2 != null) side.SetMissilePattern(patternData2.pattern);
            string total2 = patternData2 != null ? patternData2.successCount.ToString() : "3";

            ui.ApplyMessageWithProgress("MSG0019", "0", total2);
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            side.SetAllNotesClickDetection(true);

            int successCount2 = 0;
            int targetCount2 = patternData2 != null ? patternData2.successCount : 3;
            await side.OnNoteSuccessful
                .Where(success => success)
                .Do(_ => successCount2++)
                .Where(_ => successCount2 >= targetCount2)
                .FirstAsync(token);
            await FadeOutAndResetAsync(token);

            // --- ステップ_7（ミスとリカバリについて） ---
            ui.ApplyMessage("MSG0020");
            await ui.FadeInAsync(0.5f, token);
            if (patternData1 != null) side.SetMissilePattern(patternData1.pattern);
            
            await side.OnNoteFailed.Where(x => x).FirstAsync(token);
            Time.timeScale = 0f;
            side.SetBgmPause(true);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");

            await side.OnBatteryPicked.FirstAsync(token);
            side.ClearAllAttackingGhosts();
            await FadeOutAndResetAsync(token);

            // --- ステップ_8（ミスとペナルティについて） ---
            ui.ApplyMessage("MSG0021");
            await ui.FadeInAsync(0.5f, token);
            Time.timeScale = 1f;
            side.SetBgmPause(false);

            await side.OnHpDecreased.FirstAsync(token);
            await FadeOutAndResetAsync(token);

            // --- ステップ_9（ゲームオーバーについて） ---
            ui.ApplyMessage("MSG0022");
            await ui.FadeInAsync(0.5f, token);
            await UniTask.WhenAny(
                UniTask.Delay(1000, cancellationToken: token),
                vm.EventStateReactive.Where(x => x == EnumEventCommand.Submited).FirstAsync(token).AsUniTask()
            );
            await FadeOutAndResetAsync(token);

            // --- ステップ_10（ステージクリアについて） ---
            ui.ApplyMessage("MSG0023");
            await ui.FadeInAsync(0.5f, token);
            await UniTask.WhenAny(
                UniTask.Delay(1000, cancellationToken: token),
                vm.EventStateReactive.Where(x => x == EnumEventCommand.Submited).FirstAsync(token).AsUniTask()
            );
            await FadeOutAndResetAsync(token);

            SaveEventProgress((int)TutorialEventId.ETB0003);
        }

        /// <summary>
        /// チュートリアル_ステージ1の案内
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunStage1GuideTutorialAsync(CancellationToken token)
        {
            var ui = _ctx.UI;
            var input = _ctx.Input;
            var vm = _ctx.ViewModel;
            var lvl = _ctx.LevelObjects;

            InitializeStep();

            // --- ステップ_0（逃げるオバケのカット） ---
            var footerPanel = vm.CommonFooterPanelRectTrans;
            if (footerPanel != null) footerPanel.gameObject.SetActive(false);

            if (lvl.aimMoveCompletePoint != null)
            {
                await _ctx.SideEffect.TeleportPlayerAsync(
                    lvl.aimMoveCompletePoint.position,
                    lvl.aimMoveCompletePoint.eulerAngles,
                    token
                );
            }

            await UniTask.Delay(2000, cancellationToken: token); // ステージ1案内演出の代用
            await FadeOutAndResetAsync(token);

            // --- ステップ_1（逃げたオバケの追跡） ---
            ui.ApplyMessage("MSG0024");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("Default");

            if (lvl.vaseAndDeskGroup != null) lvl.vaseAndDeskGroup.SetActive(false);

            await vm.SelectedStageIndex.Where(x => x == 1).FirstAsync(token);
            await vm.EventStateReactive.Where(x => x == EnumEventCommand.Submited).FirstAsync(token);

            await FadeOutAndResetAsync(token);
            SaveEventProgress((int)TutorialEventId.ETB0004);
        }

        /// <summary>
        /// チュートリアル_シャウトノーツの案内
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunShoutNoteGuideTutorialAsync(CancellationToken token)
        {
            var ui = _ctx.UI;
            var input = _ctx.Input;
            var vm = _ctx.ViewModel;
            var lvl = _ctx.LevelObjects;

            InitializeStep();

            // --- ステップ_0（待ち構えているオバケのカット） ---
            //if (lvl.stage2DoorPoint != null)
            //{
            //    vm.PlayerTransform.SetPositionAndRotation(
            //        lvl.stage2DoorPoint.position,
            //        lvl.stage2DoorPoint.rotation
            //    );
            //}

            await UniTask.Delay(2000, cancellationToken: token); // ステージ3の案内演出_ステージ2扉前 代用
            await FadeOutAndResetAsync(token);

            // --- ステップ_1（寝室へ向かうオバケのカット） ---
            input.EnableOnlyControllerMapCategory("Default");
            if (lvl.rightStairsTrigger1F != null) lvl.rightStairsTrigger1F.gameObject.SetActive(true);

            await Observable.EveryUpdate()
                .Where(_ => lvl.rightStairsTrigger1F != null && lvl.rightStairsTrigger1F.bounds.Contains(vm.PlayerTransform.position))
                .FirstAsync(token);

            input.EnableOnlyControllerMapCategory(null);
            await UniTask.Delay(2000, cancellationToken: token); // ステージ3の案内演出_ステージ3扉前 代用
            await FadeOutAndResetAsync(token);

            // --- ステップ_2（階段にて目の前にオバケ） ---
            input.EnableOnlyControllerMapCategory("Default");
            if (lvl.leftStairsTrigger2F != null) lvl.leftStairsTrigger2F.gameObject.SetActive(true);

            await Observable.EveryUpdate()
                .Where(_ => lvl.leftStairsTrigger2F != null && lvl.leftStairsTrigger2F.bounds.Contains(vm.PlayerTransform.position))
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            SaveEventProgress((int)TutorialEventId.ETS0000);
        }

        /// <summary>
        /// チュートリアル_チュートリアル_シャウトノーツ
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunShoutNoteTutorialAsync(CancellationToken token)
        {
            var ui = _ctx.UI;
            var input = _ctx.Input;
            var vm = _ctx.ViewModel;
            var side = _ctx.SideEffect;
            var tables = _ctx.Tables;
            var lvl = _ctx.LevelObjects;

            InitializeStep();

            // --- ステップ_0（オバケ出現テロップシャウトノーツ版） ---
            ui.ApplyMessage("MSG0013");
            await ui.FadeInAsync(0.5f, token);
            if (lvl.vaseAndDeskGroup != null) lvl.vaseAndDeskGroup.SetActive(true);
            side.SetAllNotesClickDetection(false);
            side.SetBgmPause(true);

            await UniTask.WhenAny(
                UniTask.Delay(1000, cancellationToken: token),
                vm.EventStateReactive.Where(x => x == EnumEventCommand.Submited).FirstAsync(token).AsUniTask()
            );
            await FadeOutAndResetAsync(token);

            // --- ステップ_1（シャウトノーツシャウトについて） ---
            ui.ApplyMessage("MSG0025");
            await ui.FadeInAsync(0.5f, token);
            var patternData = tables.missilePatternTable.Get("SMP0003");
            if (patternData != null) side.SetMissilePattern(patternData.pattern);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            side.SetMicrophoneActive(true);
            side.SetAllNotesClickDetection(true);

            await UniTask.Delay(1000, cancellationToken: token); // ロングノーツ重なり待ち代用
            side.SetBgmPause(true);

            await vm.DbLevelReactive.Where(db => db > 0.5f).FirstAsync(token);
            await FadeOutAndResetAsync(token);

            // --- ステップ_2（シャウトノーツロングトーンについて） ---
            ui.ApplyMessage("MSG0026");
            await ui.FadeInAsync(0.5f, token);
            side.SetBgmPause(false);

            await UniTask.Delay(1000, cancellationToken: token); // 成功監視代用
            await FadeOutAndResetAsync(token);

            // --- ステップ_3（シャウトノーツシャウト＆ロングトーン本番） ---
            var patternData2 = tables.missilePatternTable.Get("SMP0004");
            if (patternData2 != null) side.SetMissilePattern(patternData2.pattern);
            string total = patternData2 != null ? patternData2.successCount.ToString() : "3";

            ui.ApplyMessageWithProgress("MSG0027", "0", total);
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            side.SetMicrophoneActive(true);
            side.SetAllNotesClickDetection(true);

            await vm.IsPostRhythmFaceOff.Where(x => x).FirstAsync(token); // 成功数監視代用
            await FadeOutAndResetAsync(token);

            SaveEventProgress((int)TutorialEventId.ETS0001);
        }

        /// <summary>
        /// チュートリアル_ステージ3の案内
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunStage3GuideTutorialAsync(CancellationToken token)
        {
            var ui = _ctx.UI;
            var input = _ctx.Input;
            var vm = _ctx.ViewModel;
            var lvl = _ctx.LevelObjects;

            InitializeStep();

            // --- ステップ_0（逃げるオバケのカットステージ3版） ---
            await UniTask.Delay(2000, cancellationToken: token); // ステージ3案内演出代用
            await FadeOutAndResetAsync(token);

            // --- ステップ_1（逃げたオバケの追跡ステージ3版） ---
            ui.ApplyMessage("MSG0024");
            await ui.FadeInAsync(0.5f, token);
            input.EnableOnlyControllerMapCategory("Default");

            if (lvl.vaseAndDeskGroup != null) lvl.vaseAndDeskGroup.SetActive(false);

            await vm.SelectedStageIndex.Where(x => x == 3).FirstAsync(token);
            await vm.EventStateReactive.Where(x => x == EnumEventCommand.Submited).FirstAsync(token);

            await FadeOutAndResetAsync(token);
            SaveEventProgress((int)TutorialEventId.ETS0002);
        }

        // =========================================================
        // 内部ユーティリティ
        // =========================================================

        /// <summary>
        /// フェードアウト → メッセージリセット → コントローラマップ全解除 をまとめて実行。
        /// 各ステップで繰り返されていたボイラープレートを一本化する。
        /// </summary>
        /// <param name="token">UniTaskのキャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask FadeOutAndResetAsync(CancellationToken token)
        {
            Time.timeScale = 1f; // フェードアウト時にタイムスケールを確実に元へ戻す
            await _ctx.UI.FadeOutAsync(0.5f, token);
            _ctx.UI.ResetMessages();
            _ctx.Input.EnableOnlyControllerMapCategory(null);
            _ctx.SideEffect.SetMicrophoneActive(false);
        }

        /// <summary>
        /// 指定オブジェクトがカメラの中央（Raycast）にあるか判定する
        /// </summary>
        /// <param name="target">判定対象のオブジェクト</param>
        /// <returns>視線の先に対象があればtrue</returns>
        private bool IsLookingAt(GameObject target)
        {
            if (target == null) return true; // 対象が破棄されていたらブロック回避のためtrue
            
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.collider.gameObject == target || hit.collider.transform.IsChildOf(target.transform);
            }
            return false;
        }

        /// <summary>
        /// イベント進捗を完了にしてセーブする
        /// </summary>
        /// <param name="eventId">イベントID</param>
        private void SaveEventProgress(int eventId)
        {
            if (_userBean.eventProgressList == null)
                _userBean.eventProgressList = new System.Collections.Generic.List<EventProgress>();

            var progress = _userBean.eventProgressList
                .Find(p => p.eventId == eventId);

            if (progress == null)
                _userBean.eventProgressList.Add(new EventProgress(eventId, 1));
            else
                progress.status = 1;

            var utility = new ResourcesUtility();
            utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, _userBean);
        }
    }
}
