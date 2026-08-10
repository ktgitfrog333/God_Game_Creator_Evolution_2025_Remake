using Cysharp.Threading.Tasks;
using Mains.Commons;
using Mains.Views;
using R3;
using Rewired;
using Selects.Commons;
using System.Threading;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

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
            var vm = _ctx.ViewModel;
            vm.SetIsLockUserBean(true);

            if (_userBean == null || TutorialConditionEvaluator.ShouldSkip(_userBean))
            {
                vm.SetIsLockUserBean(false);
                vm.SetEnemyBattlePart(EnemyBattlePart.Normal);

                return;
            }

            if (TutorialConditionEvaluator.ShouldRunMove(_userBean))
                await RunMoveTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunAimMove(_userBean))
                await RunAimMoveTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunShout(_userBean))
            {
                await RunShoutTutorialAsync(token);
                await RunRhythmTutorialAsync(token);
            }

            if (TutorialConditionEvaluator.ShouldRunStage1Guide(_userBean))
                await RunStage1GuideTutorialAsync(token);

            if (TutorialConditionEvaluator.ShouldRunShoutNoteGuide(_userBean))
            {
                await RunShoutNoteGuideTutorialAsync(token);
                await RunShoutNoteTutorialAsync(token);
            }

            if (TutorialConditionEvaluator.ShouldRunStage3Guide(_userBean))
                await RunStage3GuideTutorialAsync(token);

            vm.SetIsLockUserBean(false);
            vm.SetEnemyBattlePart(EnemyBattlePart.Normal);
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

            await Observable.EveryUpdate()
                .Select(_ => vm.PlayerTransform)
                .Where(player => player != null)
                .FirstAsync(token);

            Transform playerTransform = vm.PlayerTransform;
            var playerCharacterController = vm.PlayerCharacterController;

            var flashLight = vm.PlayerFlashLight;
            if (flashLight != null) flashLight.gameObject.SetActive(false);

            await Observable.EveryUpdate()
                .Select(_ => vm.CommonHeaderPanelRectTrans)
                .Where(trans => trans != null)
                .FirstAsync(token);

            var headerPanel = vm.CommonHeaderPanelRectTrans;
            headerPanel.gameObject.SetActive(false);

            // TODO: IsCompletedStartDirectionAndCharacterControllerEnabled は使わない IsCompletedStartDirectionReactive を監視する形でいい
            await vm.IsCompletedStartDirection.Where(x => x)
                .FirstAsync(token);

            // --- 前進 ---
            ui.ApplyMessage("MSG0000");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialForwardOnly");
            // ここでキャラクターコントローラーを有効にする
            playerCharacterController.enabled = true;
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

            input.EnableOnlyControllerMapCategory("CategoryTutorialSearchOnly");
            
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
            var lightRingParticleSys1 = lvl.lightRingParticleSys1;
            var batteryItem = lvl.batteryItem;
            var player = ReInput.players.GetPlayer(0);

            await Observable.EveryUpdate()
                .Select(_ => vm.PlayerTransform)
                .Where(player => player != null)
                .FirstAsync(token);

            var playerTransform = vm.PlayerTransform;
            var playerHead = vm.PlayerHead;
            var playerView = playerTransform.GetComponent<PlayerView>();
            var playerCharacterController = vm.PlayerCharacterController;

            InitializeStep();

            await Observable.EveryUpdate()
                .Select(_ => vm.CommonHeaderPanelRectTrans)
                .Where(trans => trans != null)
                .FirstAsync(token);

            var headerPanel = vm.CommonHeaderPanelRectTrans;
            headerPanel.gameObject.SetActive(false);

            var isCompletedStartDirection = vm.IsCompletedStartDirection.CurrentValue;
            //await Observable.EveryUpdate()
            //    .Where(_ => vm.IsCompletedStartDirection.CurrentValue)
            //    .FirstAsync(token);

            // TODO: 移動演出が悩ましい
            // 1. プレイヤー移動（Tween）
            // 2. フェードイン->プレイヤー移動（瞬間移動）->フェードアウト
            // 3. プレイヤー移動（瞬間移動）
            var fadeImageView = _ctx.UIObjects.fadeImageView;
            var sp = lvl.moveCompletePoint;
            if (sp != null)
            {
                await _ctx.SideEffect.TeleportPlayerAsync(sp.position, sp.eulerAngles, isCompletedStartDirection, playerCharacterController, player, playerTransform, playerHead, playerView, fadeImageView, token);
            }

            // --- 視点移動 ---
            ui.ApplyMessage("MSG0006");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");
            lightRingParticleSys1.SetActive(true);
            batteryItem.SetActive(true);

            float lookTimer = 0f;

            await Observable.EveryUpdate()
                .Where(_ =>
                {
                    if (vm.BatteryHitPlayerAim.CurrentValue)
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

            await vm.BatteryTriggerStay
                .Where(x => x)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- 電池 取得ボタン押下 ---
            ui.ApplyMessage("MSG0008");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialSearchOnly");

            await Observable.EveryUpdate()
                .Where(_ => vm.BatteryTriggerStay.CurrentValue)
                .Where(_ => input.SearchButtonDown)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            batteryItem.SetActive(false);
            lightRingParticleSys1.SetActive(false);
            
            if (lvl.aimMoveCompletePoint != null)
            {
                await _ctx.SideEffect.TeleportPlayerAsync(
                    lvl.aimMoveCompletePoint.position,
                    lvl.aimMoveCompletePoint.eulerAngles,
                    isCompletedStartDirection,
                    playerCharacterController,
                    player,
                    playerTransform,
                    playerHead,
                    playerView,
                    fadeImageView,
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
            var tables = _ctx.Tables;
            var player = ReInput.players.GetPlayer(0);

            await Observable.EveryUpdate()
                .Select(_ => vm.PlayerTransform)
                .Where(player => player != null)
                .FirstAsync(token);

            var playerTransform = vm.PlayerTransform;
            var playerHead = vm.PlayerHead;
            var playerCharacterController = vm.PlayerCharacterController;
            var playerView = playerTransform.GetComponent<PlayerView>();

            InitializeStep();

            await Observable.EveryUpdate()
                .Select(_ => vm.CommonHeaderPanelRectTrans)
                .Where(trans => trans != null)
                .FirstAsync(token);

            var headerPanel = vm.CommonHeaderPanelRectTrans;
            headerPanel.gameObject.SetActive(false);

            var isCompletedStartDirection = vm.IsCompletedStartDirection.CurrentValue;
            //await vm.IsCompletedStartDirection.Where(x => x)
            //    .FirstAsync(token);

            var fadeImageView = _ctx.UIObjects.fadeImageView;
            if (lvl.aimMoveCompletePoint != null)
            {
                await side.TeleportPlayerAsync(
                    lvl.aimMoveCompletePoint.position,
                    lvl.aimMoveCompletePoint.eulerAngles,
                    isCompletedStartDirection,
                    playerCharacterController,
                    player,
                    playerTransform,
                    playerHead,
                    playerView,
                    fadeImageView,
                    token
                );
            }

            // --- ステップ_0（視点操作にてオバケを探す） ---
            ui.ApplyMessage("MSG0006");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");
            side.PlayGhostLaughV2Normal();
            var missGhostEscapeNormal = lvl.missGhostEscapeNormal;
            missGhostEscapeNormal.SetActive(true);

            float lookTimer = 0f;
            await Observable.EveryUpdate()
                .Where(_ =>
                {
                    if (vm.MissGhostEscapeNormalHitPlayerAim.CurrentValue)
                        lookTimer += Time.deltaTime;
                    else
                        lookTimer = 0f;
                    return lookTimer >= 0.25f;
                })
                .FirstAsync(token);

            input.EnableOnlyControllerMapCategory(null);

            var missGhostEscapeNormalAnimator = lvl.missGhostEscapeNormalAnimator;
            var missGhostEscapeView = lvl.missGhostEscapeView;

            await side.EnabledAnimator(missGhostEscapeNormalAnimator, missGhostEscapeView)
                .FirstAsync(token);

            side.PlayGhostLaughV2Normal();
            //await UniTask.Delay(1000, cancellationToken: token); // オバケ移動アニメの完了待機（簡易代用）

            await FadeOutAndResetAsync(token);

            missGhostEscapeNormal.SetActive(false);

            // --- ステップ_1（オバケを追いかける） ---
            ui.ApplyMessage("MSG0009");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMove");
            var lightRingParticleSys2 = lvl.lightRingParticleSys2;
            lightRingParticleSys2.SetActive(true);

            await vm.LightRing2TriggerStay
                .Where(x => x)
                .FirstAsync(token);

            lightRingParticleSys2.SetActive(false);

            await FadeOutAndResetAsync(token);

            // --- ステップ_2（視点操作にてオバケが隠れた家具を探す） ---
            ui.ApplyMessage("MSG0006");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");
            var vaseAndDeskGroup = lvl.vaseAndDeskGroup;
            vaseAndDeskGroup.gameObject.SetActive(true);

            lookTimer = 0f;

            await Observable.EveryUpdate()
                .Where(_ =>
                {
                    if (vm.VaseAndDeskGroupHitPlayerAim.CurrentValue)
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

            await Observable.EveryUpdate()
                .Where(_ => vm.InteractionPart.CurrentValue.Equals(InteractionPart.ShoutChance))
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_4（シャウト練習） ---
            ui.ApplyMessage("MSG0011");
            playerView.SwitchRayLengthType(1);

            await ui.FadeInAsync(0.5f, token);

            side.SetMicrophoneActive(true);
            input.EnableOnlyControllerMapCategory("CategoryTutorialInhaleOnly");
            var dbLevelMax = tables.playerShoutChanceTable.シャウト達成デシベル;

            await vm.DbLevelReactive
                .Where(db => dbLevelMax <= db)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_5（シャウト本番） ---
            ui.ApplyMessage("MSG0012");
            playerView.SwitchRayLengthType(2);

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndInhale");
            side.SetMicrophoneActive(true);

            await Observable.EveryUpdate()
                .Select(_ => vm)
                .Where(vm => vm.InteractionPart.CurrentValue.Equals(InteractionPart.Rhythm))
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // リズムパートから再開が難しいのでシャウト版はここで保存しない
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
            var missilePatternTable = tables.missilePatternTable;

            InitializeStep();

            side.SetMicrophoneActive(false);
            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");
            var vaseAndDeskGroup = lvl.vaseAndDeskGroup;
            if (vaseAndDeskGroup != null) vaseAndDeskGroup.gameObject.SetActive(true);

            Transform missileTempoSpawnerTrans = null;

            await Observable.EveryUpdate()
                .Where(_ => vm.MissileTempoSpawnerTrans.CurrentValue != null)
                .FirstAsync(token);

            missileTempoSpawnerTrans = vm.MissileTempoSpawnerTrans.CurrentValue;
            side.SetMissileTempoSpawner(missileTempoSpawnerTrans);
            int firstHomingObjectTargetIndex = missilePatternTable.firstHomingObjectTargetIndex;
            side.WatchFirstHomingObjectSpawn(firstHomingObjectTargetIndex);

            await side.OnFirstHomingObjectSpawned.FirstAsync(token);

            // 「ノーツクリック判定」を更新する場合は、モデル側の「強制的に背面扱いで返すかのフラグ」も更新する
            vm.SetIsBackReturnForce(true);
            side.SetAllNotesClickDetection(false);
            // --- ステップ_0（オバケ出現テロップ） ---
            ui.ApplyMessage("MSG0013");

            await ui.FadeInAsync(0.5f, token);

            Time.timeScale = 0f;
            side.SetBgmPause(true);

            await UniTask.WhenAny(vm.EventStateReactive
                    .Where(x => x == EnumEventCommand.Submited)
                    .FirstAsync(token)
                    .AsUniTask(),
                UniTask.Delay(3000, DelayType.UnscaledDeltaTime, cancellationToken: token));

            await FadeOutAndResetAsync(token);

            // --- ステップ_1（ターゲットクロス操作について） ---
            ui.ApplyMessage("MSG0014");

            await ui.FadeInAsync(0.5f, token);

            int ghostHomingStartedTargetIndex = missilePatternTable.ghostHomingStartedTargetIndex;
            input.EnableOnlyControllerMapCategory("CategoryTutorialAimMoveOnly");
            float fromMouseToNotesDistance = missilePatternTable.fromMouseToNotesDistance;

            await Observable.EveryUpdate()
                .Where(_ => side.GetNoteToCrosshairScreenDistance(ghostHomingStartedTargetIndex) <= fromMouseToNotesDistance)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_2（ノーツクリックについて） ---
            ui.ApplyMessage("MSG0015");
            await ui.FadeInAsync(0.5f, token);

            Time.timeScale = 1f;
            side.SetBgmPause(false);

            await Observable.EveryUpdate()
                .Where(_ => side.IsAnyShortNoteClickable(ghostHomingStartedTargetIndex))
                .FirstAsync(token);

            Time.timeScale = 0f;
            side.SetBgmPause(true);
            input.EnableOnlyControllerMapCategory("CategoryTutorialTapLightOnly");

            await Observable.EveryUpdate()
                .Where(_ => input.TapLightButtonDown)
                .FirstAsync(token);

            side.ForceClickAnyClickableNote(ghostHomingStartedTargetIndex);
            Time.timeScale = 1f;
            side.SetBgmPause(false);

            await FadeOutAndResetAsync(token);

            // --- ステップ_3（ノーツクリック本番） ---
            var patternData = tables.missilePatternTable.Get("SMP0000");
            if (patternData != null) side.SetMissilePattern(patternData.pattern);
            int total = patternData != null ? patternData.successCount : 3;

            ui.ApplyMessageWithProgress("MSG0016", "0", $"{total}");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            vm.SetIsBackReturnForce(false);
            side.SetAllNotesClickDetection(true);
            int successCount = 0;

            await vm.OnNoteSuccessful
                .Where(x => x)
                .Do(_ =>
                {
                    successCount++;
                    ui.ApplyMessageWithProgress("MSG0016", $"{successCount}", $"{total}");
                })
                .Where(_ => successCount >= total)
                .FirstAsync(token);

            // ミサイルオバケとミスアタックオバケを消す
            var patternData3 = tables.missilePatternTable.Get("SMP0005");
            var objectPoolerXyloOtherCustomizeView = GameObject.FindAnyObjectByType<ObjectPoolerXyloOtherCustomizeView>();
            side.SetObjectPoolerXyloOther(objectPoolerXyloOtherCustomizeView.transform);
            var homingObjectPoolerCustomizeView = GameObject.FindAnyObjectByType<HomingObjectPoolerCustomizeView>();
            if (patternData3 != null) side.SetMissilePattern(patternData3.pattern);
            homingObjectPoolerCustomizeView.DoReturnAllMissilesToPool();

            await objectPoolerXyloOtherCustomizeView.DoAllDisabled()
                .Where(x => x)
                .FirstAsync(token);

            vm.SetIsBackReturnForce(true);
            side.SetAllNotesClickDetection(false);

            await FadeOutAndResetAsync(token);

            // --- ステップ_4（ロングノーツクリックについて） ---
            ui.ApplyMessage("MSG0017");

            await ui.FadeInAsync(0.5f, token);

            var patternData1 = tables.missilePatternTable.Get("SMP0001");
            if (patternData1 != null) side.SetMissilePattern(patternData1.pattern);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");

            await Observable.EveryUpdate()
                .Where(_ => side.IsAnyLongNoteClickable(ghostHomingStartedTargetIndex))
                .FirstAsync(token);

            Time.timeScale = 0f;
            side.SetBgmPause(true);

            await Observable.EveryUpdate()
                .Where(_ => side.GetNoteToCrosshairScreenDistance(ghostHomingStartedTargetIndex) <= fromMouseToNotesDistance &&
                    input.TapLightButtonDown)
                .FirstAsync(token);

            side.ForceClickAnyClickableNote(ghostHomingStartedTargetIndex);
            // ロングノーツが失敗しない状態にする（強制的に押しっぱなしの状態にする）
            side.ForceSetAutoMode(ghostHomingStartedTargetIndex, true);

            await FadeOutAndResetAsync(token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            // --- ステップ_5（ロングノーツリリースについて） ---
            ui.ApplyMessage("MSG0018");

            await ui.FadeInAsync(0.5f, token);

            Time.timeScale = 1f;
            side.SetBgmPause(false);

            // ロングノーツの完了によるGOODを監視
            await vm.OnNoteSuccessful
                .Where(x => x)
                .FirstAsync(token);

            vm.SetIsBackReturnForce(false);
            side.SetAllNotesClickDetection(true);
            side.ForceSetAutoMode(ghostHomingStartedTargetIndex, false);

            await FadeOutAndResetAsync(token);

            // --- ステップ_6（ロングノーツクリック＆リリース本番） ---
            var patternData2 = tables.missilePatternTable.Get("SMP0002");
            if (patternData2 != null) side.SetMissilePattern(patternData2.pattern);
            int total2 = patternData2 != null ? patternData2.successCount : 3;

            ui.ApplyMessageWithProgress("MSG0019", "0", $"{total2}");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            int successCount2 = 0;

            await vm.OnNoteSuccessful
                .Where(x => x)
                .Do(_ =>
                {
                    successCount2++;
                    ui.ApplyMessageWithProgress("MSG0019", $"{successCount2}", $"{total2}");
                })
                .Where(_ => successCount2 >= total2)
                .FirstAsync(token);

            // ミサイルオバケとミスアタックオバケを消す
            if (patternData3 != null) side.SetMissilePattern(patternData3.pattern);
            homingObjectPoolerCustomizeView.DoReturnAllMissilesToPool();

            await objectPoolerXyloOtherCustomizeView.DoAllDisabled()
                .Where(x => x)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_7（ミスとリカバリについて） ---
            ui.ApplyMessage("MSG0020");

            await ui.FadeInAsync(0.5f, token);

            if (patternData1 != null) side.SetMissilePattern(patternData1.pattern);
            
            await vm.OnNoteFailed
                .Where(x => x)
                .FirstAsync(token);

            // 電池出現後の一定時間後、時間を一時停止する
            await vm.OnFalledBattery.FirstAsync(token);

            await UniTask.Delay(1000, DelayType.UnscaledDeltaTime, cancellationToken: token);

            Time.timeScale = 0f;
            side.SetBgmPause(true);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");

            await vm.OnBatteryPicked.FirstAsync(token);

            Time.timeScale = 1f;
            side.SetBgmPause(false);

            // ミサイルオバケとミスアタックオバケを消す
            if (patternData3 != null) side.SetMissilePattern(patternData3.pattern);
            homingObjectPoolerCustomizeView.DoReturnAllMissilesToPool();

            await objectPoolerXyloOtherCustomizeView.DoAllDisabled()
                .Where(x => x)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_8（ミスとペナルティについて） ---
            ui.ApplyMessage("MSG0021");

            // HPゲージを表示する
            var commonPanelCustomizeOfMainView = ui.CommonPanelCustomizeOfMainView;

            ui.SetCommonPanelCustomizeOfMainViewEnabled(true);

            await commonPanelCustomizeOfMainView.IsCompletedStart.FirstAsync(token);

            // 体力の初期値をセット
            vm.SetHealthPointMax();

            await ui.FadeInAsync(0.5f, token);

            if (patternData1 != null) side.SetMissilePattern(patternData1.pattern);

            // ここでミスを発生させるのは難しいので、強制的にHPを減らす
            await vm.OnHpDecreasedTutorial.FirstAsync(token);

            if (patternData3 != null) side.SetMissilePattern(patternData3.pattern);

            homingObjectPoolerCustomizeView.DoReturnAllMissilesToPool();
            // 電池を自動で拾う
            var batteryTransform = vm.BatteryTransform;
            if (batteryTransform != null)
            {
                BatteryView batteryView = batteryTransform.GetComponent<BatteryView>();
                batteryView.GetBattery();
            }

            await objectPoolerXyloOtherCustomizeView.DoAllDisabled()
                .Where(x => x)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_9（ゲームオーバーについて） ---
            ui.ApplyMessage("MSG0022");

            await ui.FadeInAsync(0.5f, token);

            await UniTask.WhenAny(vm.EventStateReactive
                    .Where(x => x == EnumEventCommand.Submited)
                    .FirstAsync(token)
                    .AsUniTask(),
                UniTask.Delay(3000, DelayType.UnscaledDeltaTime, cancellationToken: token));

            await FadeOutAndResetAsync(token);

            // --- ステップ_10（ステージクリアについて） ---
            ui.ApplyMessage("MSG0023");

            await ui.FadeInAsync(0.5f, token);

            await UniTask.WhenAny(vm.EventStateReactive
                    .Where(x => x == EnumEventCommand.Submited)
                    .FirstAsync(token)
                    .AsUniTask(),
                UniTask.Delay(3000, DelayType.UnscaledDeltaTime, cancellationToken: token));

            await FadeOutAndResetAsync(token);

            // HPのUIを消す
            ui.SetCommonPanelCustomizeOfMainViewEnabled(false);
            // リズムパートを終了させる
            vm.SetIsCompletedDirection(true);

            await Observable.EveryUpdate()
                .Where(_ => vm.InteractionPart.CurrentValue.Equals(InteractionPart.Search))
                .FirstAsync(token);

            if (vaseAndDeskGroup != null) vaseAndDeskGroup.DisableStaticCollders();

            // リズムパートから再開が難しいのでここで保存する
            SaveEventProgress((int)TutorialEventId.ETB0002);
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
            var player = ReInput.players.GetPlayer(0);

            await Observable.EveryUpdate()
                .Select(_ => vm.PlayerTransform)
                .Where(player => player != null)
                .FirstAsync(token);

            var playerTransform = vm.PlayerTransform;
            var playerHead = vm.PlayerHead;
            var playerCharacterController = vm.PlayerCharacterController;
            var playerView = playerTransform.GetComponent<PlayerView>();

            InitializeStep();

            await Observable.EveryUpdate()
                .Select(_ => vm.CommonHeaderPanelRectTrans)
                .Where(trans => trans != null)
                .FirstAsync(token);

            var headerPanel = vm.CommonHeaderPanelRectTrans;
            headerPanel.gameObject.SetActive(false);

            var isCompletedStartDirection = vm.IsCompletedStartDirection.CurrentValue;
            // --- ステップ_0（逃げるオバケのカット） ---
            var fadeImageView = _ctx.UIObjects.fadeImageView;
            if (lvl.shoutCompletePoint != null)
            {
                await _ctx.SideEffect.TeleportPlayerAsync(
                    lvl.shoutCompletePoint.position,
                    lvl.shoutCompletePoint.eulerAngles,
                    isCompletedStartDirection,
                    playerCharacterController,
                    player,
                    playerTransform,
                    playerHead,
                    playerView,
                    fadeImageView,
                    token
                );
            }

            // ここだけメッセージ表示はなく、メッセージ表示によるオブジェクトを有効化が行われないため、明示的にオブジェクトを有効にする
            ui.SetEnabledTutorialPanel(true);

            await Observable.EveryUpdate()
                .Where(_ => ui.IsCompletedStart.CurrentValue &&
                    ui.IsEnabled.CurrentValue)
                .FirstAsync(token);

            ui.PlayStage1GuideDirection();

            await Observable.EveryUpdate()
                .Where(_ => ui.IsCompletedStage1GuideDirection.CurrentValue)
                .FirstAsync(token);

            // --- ステップ_1（逃げたオバケの追跡） ---
            ui.ApplyMessage("MSG0024");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("Default");

            await vm.SelectedStageIndex.Where(x => x == 0).FirstAsync(token);

            await vm.EventStateReactive.Where(x => x == EnumEventCommand.Submited).FirstAsync(token);

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
            var side = _ctx.SideEffect;
            var player = ReInput.players.GetPlayer(0);

            await Observable.EveryUpdate()
                .Select(_ => vm.PlayerTransform)
                .Where(player => player != null)
                .FirstAsync(token);

            var playerTransform = vm.PlayerTransform;
            var playerHead = vm.PlayerHead;
            var playerCharacterController = vm.PlayerCharacterController;
            var playerView = playerTransform.GetComponent<PlayerView>();

            InitializeStep();

            var isCompletedStartDirection = vm.IsCompletedStartDirection.CurrentValue;
            // --- ステップ_0（待ち構えているオバケのカット） ---
            var fadeImageView = _ctx.UIObjects.fadeImageView;
            var playerRespawnPosition_1 = lvl.playerRespawnPosition_1;
            if (playerRespawnPosition_1 != null)
            {
                await side.TeleportPlayerAsync(
                    playerRespawnPosition_1.position,
                    playerRespawnPosition_1.eulerAngles,
                    isCompletedStartDirection,
                    playerCharacterController,
                    player,
                    playerTransform,
                    playerHead,
                    playerView,
                    fadeImageView,
                    token
                );
            }

            // ここだけメッセージ表示はなく、メッセージ表示によるオブジェクトを有効化が行われないため、明示的にオブジェクトを有効にする
            ui.SetEnabledTutorialPanel(true);
            
            var missGhostEscapeView_1 = lvl.missGhostEscapeView_1;
            missGhostEscapeView_1.gameObject.SetActive(true);

            await missGhostEscapeView_1.IsEscapeCompleted.Where(x => x).FirstAsync(token);

            side.PlayGhostLaughV2Normal();

            // --- ステップ_1（寝室へ向かうオバケのカット） ---
            input.EnableOnlyControllerMapCategory("Default");
            if (lvl.rightStairsTrigger1F != null) lvl.rightStairsTrigger1F.gameObject.SetActive(true);

            await Observable.EveryUpdate()
                .Where(_ => vm.RightStairsTrigger1FStay.CurrentValue)
                .FirstAsync(token);

            input.EnableOnlyControllerMapCategory(null);
            var missGhostEscapeView_2 = lvl.missGhostEscapeView_2;
            missGhostEscapeView_2.gameObject.SetActive(true);
            // 注視するオバケ情報をセット
            vm.SetTargetGhost(missGhostEscapeView_2.transform);

            await missGhostEscapeView_2.IsEscapeCompleted.Where(x => x).FirstAsync(token);

            side.PlayGhostLaughV2Normal();
            // 注視するオバケ情報を解除
            vm.SetTargetGhost(null);
            var vaseAndDeskGroup1 = lvl.vaseAndDeskGroup1;
            vaseAndDeskGroup1.gameObject.SetActive(true);

            // --- ステップ_2（階段にて目の前にオバケ） ---
            input.EnableOnlyControllerMapCategory("Default");
            if (lvl.leftStairsTrigger2F != null) lvl.leftStairsTrigger2F.gameObject.SetActive(true);
            side.SetMicrophoneActive(true);

            await Observable.EveryUpdate()
                .Where(_ => vm.LeftStairsTrigger2FStay.CurrentValue)
                .FirstAsync(token);

            await Observable.EveryUpdate()
                .Select(_ => vm)
                .Where(vm => vm.InteractionPart.CurrentValue.Equals(InteractionPart.Rhythm))
                .FirstAsync(token);

            // リズムパートから再開が難しいのでチュートリアル_シャウトノーツの案内版はここで保存しない
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
            var missilePatternTable = tables.missilePatternTable;
            var playerShoutChanceTable = tables.playerShoutChanceTable;

            InitializeStep();

            await Observable.EveryUpdate()
                .Select(_ => vm.CommonHeaderPanelRectTrans)
                .Where(trans => trans != null)
                .FirstAsync(token);

            var headerPanel = vm.CommonHeaderPanelRectTrans;
            headerPanel.gameObject.SetActive(false);

            input.EnableOnlyControllerMapCategory("Default");
            Transform missileTempoSpawnerTrans = null;

            await Observable.EveryUpdate()
                .Where(_ => vm.MissileTempoSpawnerTrans.CurrentValue != null)
                .FirstAsync(token);

            missileTempoSpawnerTrans = vm.MissileTempoSpawnerTrans.CurrentValue;
            side.SetMissileTempoSpawner(missileTempoSpawnerTrans);
            int firstHomingObjectTargetIndex = missilePatternTable.firstHomingObjectTargetIndex;
            side.WatchFirstHomingObjectSpawn(firstHomingObjectTargetIndex);

            await side.OnFirstHomingObjectSpawned.FirstAsync(token);

            // 「ノーツクリック判定」を更新する場合は、モデル側の「強制的に背面扱いで返すかのフラグ」も更新する
            vm.SetIsBackReturnForce(true);
            side.SetAllNotesClickDetection(false);
            // --- ステップ_0（オバケ出現テロップシャウトノーツ版） ---
            ui.ApplyMessage("MSG0013");

            await ui.FadeInAsync(0.5f, token);

            Time.timeScale = 0f;
            side.SetBgmPause(true);

            await UniTask.WhenAny(vm.EventStateReactive
                    .Where(x => x == EnumEventCommand.Submited)
                    .FirstAsync(token)
                    .AsUniTask(),
                UniTask.Delay(3000, DelayType.UnscaledDeltaTime, cancellationToken: token));

            await FadeOutAndResetAsync(token);

            // --- ステップ_1（シャウトノーツシャウトについて） ---
            ui.ApplyMessage("MSG0025");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");

            Time.timeScale = 1f;
            side.SetBgmPause(false);
            int ghostHomingStartedTargetIndex = missilePatternTable.ghostHomingStartedTargetIndex;

            // シャウトノーツ重なり待ち
            await Observable.EveryUpdate()
                .Where(_ => side.IsAnyLongNoteClickable(ghostHomingStartedTargetIndex))
                .FirstAsync(token);

            side.SetMicrophoneActive(true);
            side.SetAllNotesClickDetection(true);
            Time.timeScale = 0f;
            side.SetBgmPause(true);
            var dbLevelMax = playerShoutChanceTable.シャウト達成デシベル;

            await vm.DbLevelReactive
                .Where(db => dbLevelMax <= db)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // --- ステップ_2（シャウトノーツロングトーンについて） ---
            ui.ApplyMessage("MSG0026");

            await ui.FadeInAsync(0.5f, token);

            side.SetMicrophoneActive(true);
            Time.timeScale = 1f;
            side.SetBgmPause(false);
            // シャウトノーツが失敗しない状態にする（強制的に押しっぱなしの状態にする）
            var dbLevelShoutNote = playerShoutChanceTable.マイク手動入力値;
            var dbInputRate = playerShoutChanceTable.マイク自動入力間隔;
            side.ForceSeriaSetMicButtonInput(dbLevelShoutNote, dbInputRate);

            // シャウトノーツの完了によるGOODを監視
            await vm.OnNoteSuccessful
                .Where(x => x)
                .FirstAsync(token);

            // 強制停止
            side.ForceStopSetMicButtonInput();

            await FadeOutAndResetAsync(token);

            // --- ステップ_3（シャウトノーツシャウト＆ロングトーン本番） ---
            var patternData2 = tables.missilePatternTable.Get("SMP0004");
            if (patternData2 != null) side.SetMissilePattern(patternData2.pattern);
            int total = patternData2 != null ? patternData2.successCount : 3;

            ui.ApplyMessageWithProgress("MSG0027", "0", $"{total}");

            await ui.FadeInAsync(0.5f, token);

            side.SetMicrophoneActive(true);
            input.EnableOnlyControllerMapCategory("CategoryTutorialMoveAllAndSearchAndAimMoveAndSwitchPartInhaleAndTapLight");
            int successCount = 0;

            await vm.OnNoteSuccessful
                .Where(x => x)
                .Do(_ =>
                {
                    successCount++;
                    ui.ApplyMessageWithProgress("MSG0027", $"{successCount}", $"{total}");
                })
                .Where(_ => successCount >= total)
                .FirstAsync(token);

            await FadeOutAndResetAsync(token);

            // リズムパートを終了させる
            vm.SetIsCompletedDirection(true);

            await Observable.EveryUpdate()
                .Where(_ => vm.InteractionPart.CurrentValue.Equals(InteractionPart.Search))
                .FirstAsync(token);

            var vaseAndDeskGroup1 = lvl.vaseAndDeskGroup1;
            vaseAndDeskGroup1.DisableStaticCollders();

            // リズムパートから再開が難しいのでここで保存する
            SaveEventProgress((int)TutorialEventId.ETS0000);
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
            var player = ReInput.players.GetPlayer(0);

            await Observable.EveryUpdate()
                .Select(_ => vm.PlayerTransform)
                .Where(player => player != null)
                .FirstAsync(token);

            var playerTransform = vm.PlayerTransform;
            var playerHead = vm.PlayerHead;
            var playerCharacterController = vm.PlayerCharacterController;
            var playerView = playerTransform.GetComponent<PlayerView>();

            InitializeStep();

            await Observable.EveryUpdate()
                .Select(_ => vm.CommonHeaderPanelRectTrans)
                .Where(trans => trans != null)
                .FirstAsync(token);

            var headerPanel = vm.CommonHeaderPanelRectTrans;
            headerPanel.gameObject.SetActive(false);

            var isCompletedStartDirection = vm.IsCompletedStartDirection.CurrentValue;
            // --- ステップ_0（逃げるオバケのカットステージ3版） ---
            var fadeImageView = _ctx.UIObjects.fadeImageView;
            if (lvl.shoutCompletePoint != null)
            {
                await _ctx.SideEffect.TeleportPlayerAsync(
                    lvl.shoutCompletePoint.position,
                    lvl.shoutCompletePoint.eulerAngles,
                    isCompletedStartDirection,
                    playerCharacterController,
                    player,
                    playerTransform,
                    playerHead,
                    playerView,
                    fadeImageView,
                    token
                );
            }

            // ここだけメッセージ表示はなく、メッセージ表示によるオブジェクトを有効化が行われないため、明示的にオブジェクトを有効にする
            ui.SetEnabledTutorialPanel(true);

            await Observable.EveryUpdate()
                .Where(_ => ui.IsCompletedStart.CurrentValue &&
                    ui.IsEnabled.CurrentValue)
                .FirstAsync(token);

            ui.PlayStage3GuideDirection();

            await Observable.EveryUpdate()
                .Where(_ => ui.IsCompletedStage3GuideDirection.CurrentValue)
                .FirstAsync(token);

            // --- ステップ_1（逃げたオバケの追跡ステージ3版） ---
            ui.ApplyMessage("MSG0024");

            await ui.FadeInAsync(0.5f, token);

            input.EnableOnlyControllerMapCategory("Default");

            await vm.SelectedStageIndex.Where(x => x == 2).FirstAsync(token);

            await vm.EventStateReactive.Where(x => x == EnumEventCommand.Submited).FirstAsync(token);

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
