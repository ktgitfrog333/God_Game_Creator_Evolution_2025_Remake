using Cysharp.Threading.Tasks;
using Mains.Views;
using R3;
using Rewired;
using Selects.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Universal.Commons;
using Universal.Utilities;

namespace Selects.Views
{
    /// <summary>
    /// ステージ開始演出のシーケンサ
    /// </summary>
    [CreateAssetMenu(fileName = "StartDirectionSequencer", menuName = "Scriptable Objects/StartDirectionSequencer")]
    public class StartDirectionSequencer : ScriptableObject, System.IDisposable
    {
        /// <summary>ステージ開始演出の設定</summary>
        [SerializeField] private StartDirectionSettings settings;
        /// <summary>ユーザー情報を保持するクラス</summary>
        private UserBean _userBean;
        /// <summary>ユーザー情報を保持するクラス</summary>
        public UserBean UserBean => _userBean;
        /// <summary>ステージ開始演出のモード</summary>
        private StartDirectionMode _startDirectionMode;
        /// <summary>ステージ開始演出のモード</summary>
        public StartDirectionMode StartDirectionMode => _startDirectionMode;
        /// <summary>ステージ開始演出のステップディクショナリ</summary>
        private Dictionary<StartDirectionStep, int> _stepDictionary;
        /// <summary>ステージ開始演出のステップディクショナリ</summary>
        public Dictionary<StartDirectionStep, int> StepDictionary => _stepDictionary != null && 0 < _stepDictionary.Count ?
            _stepDictionary : _stepDictionary = InitializeStepDictionary();
        /// <summary>実行中</summary>
        private bool _isRunning;

        /// <summary>
        /// 事前処理（入力無効化、BGM一時停止など）デリゲート
        /// </summary>
        /// <param name="characterControllerEnabled">キャラクターコントローラーの有効／無効</param>
        /// <param name="characterController">キャラクターコントローラー</param>
        /// <param name="playerEnabled">Rewiredプレイヤーの未設定／有効／無効</param>
        /// <param name="player">Rewiredプレイヤー</param>
        public delegate void DoPreProcessDelegate(bool characterControllerEnabled, CharacterController characterController, int playerEnabled, Player player);
        /// <summary>事前処理（入力無効化、BGM一時停止など）デリゲート</summary>
        private DoPreProcessDelegate _doPreProcessDelegate;
        /// <summary>キャラクターコントローラーの有効／無効</summary>
        private bool _preProcessCharacterControllerEnabled;
        /// <summary>キャラクターコントローラー</summary>
        private CharacterController _characterController;
        /// <summary>Rewiredプレイヤーの未設定／有効／無効</summary>
        private int _preProcessPlayerEnabled;
        /// <summary>Rewiredプレイヤー</summary>
        private Player _player;

        /// <summary>
        /// 事前処理（入力無効化、BGM一時停止など）デリゲートをセット
        /// </summary>
        /// <param name="del">事前処理（入力無効化、BGM一時停止など）デリゲート</param>
        /// <param name="characterControllerEnabled">キャラクターコントローラーの有効／無効</param>
        /// <param name="characterController">キャラクターコントローラー</param>
        /// <param name="playerEnabled">Rewiredプレイヤーの未設定／有効／無効</param>
        /// <param name="player">Rewiredプレイヤー</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        public async UniTask SetDoPreProcessDelegate(DoPreProcessDelegate del, bool characterControllerEnabled, CharacterController characterController, int playerEnabled, Player player, CancellationToken token)
        {
            var dic = StepDictionary;
            if (dic[StartDirectionStep.PreProcess] == 1)
                return;

            _doPreProcessDelegate = del;
            _preProcessCharacterControllerEnabled = characterControllerEnabled;
            _characterController = characterController;
            _preProcessPlayerEnabled = playerEnabled;
            _player = player;
            dic[StartDirectionStep.PreProcess] = 1;
            DoPreProcessDelegate doPostProcessDel = (characterControllerEnabled, characterController, playerEnabled, player) => del(characterControllerEnabled, characterController, playerEnabled, player);

            if (!_isRunning)
                await RunAsync(token);
        }

        /// <summary>
        /// 移動前隠蔽用の演出（フェードイン、画面暗転など）デリゲート
        /// </summary>
        /// <param name="duration">終了時間</param>
        /// <returns>オブザーバブル</returns>
        /// <see cref="Mains.Views.FadeImageView.PlayFadeInDirection(float)"/>
        /// <see cref="Selects.Views.FadeImageView.PlayFadeInDirection(float)"/>
        public delegate Observable<bool> DoPreHideEffectDelegate(float duration = .5f);
        /// <summary>移動前隠蔽用の演出（フェードイン、画面暗転など）デリゲート</summary>
        private DoPreHideEffectDelegate _doPreHideEffectDelegate;
        /// <summary>終了時間</summary>
        private float _preHideEffectDuration;

        /// <summary>
        /// 移動前隠蔽用の演出（フェードイン、画面暗転など）デリゲートのセット
        /// </summary>
        /// <param name="del">移動前隠蔽用の演出（フェードイン、画面暗転など）デリゲート</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <param name="duration">終了時間</param>
        /// <returns>UniTask</returns>
        public async UniTask SetDoPreHideEffectDelegate(DoPreHideEffectDelegate del, CancellationToken token, float duration = .5f)
        {
            var dic = StepDictionary;
            if (dic[StartDirectionStep.PreHideEffect] == 1)
                return;

            _doPreHideEffectDelegate = del;
            _preHideEffectDuration = duration;
            dic[StartDirectionStep.PreHideEffect] = 1;

            if (!_isRunning)
                await RunAsync(token);
        }

        /// <summary>
        /// 瞬間移動処理（座標・回転の即時設定）デリゲート
        /// </summary>
        /// <param name="position">移動先の位置</param>
        /// <param name="angles">移動先の角度</param>
        /// <param name="playerTransform">プレイヤーのTransform</param>
        /// <param name="playerHeadTransform">プレイヤー頭のTransform</param>
        /// <param name="playerView">プレイヤーのビュー</param>
        public delegate void DoTeleportDelegate(Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform, PlayerView playerView);
        /// <summary>瞬間移動処理（座標・回転の即時設定）デリゲート</summary>
        private DoTeleportDelegate _doTeleportDelegate;
        /// <summary>移動先の位置</summary>
        private Vector3 _teleportPosition;
        /// <summary>移動先の角度</summary>
        private Vector3 _teleportAngles;
        /// <summary>プレイヤーのTransform</summary>
        private Transform _teleportPlayerTransform;
        /// <summary>プレイヤー頭のTransform</summary>
        private Transform _teleportPlayerHeadTransform;
        /// <summary>プレイヤーのビュー</summary>
        private PlayerView _teleportPlayerView;

        /// <summary>
        /// 瞬間移動処理（座標・回転の即時設定）デリゲートをセット
        /// </summary>
        /// <param name="del">瞬間移動処理（座標・回転の即時設定）デリゲート</param>
        /// <param name="position">移動先の位置</param>
        /// <param name="angles">移動先の角度</param>
        /// <param name="playerTransform">プレイヤーのTransform</param>
        /// <param name="playerHeadTransform">プレイヤー頭のTransform</param>
        /// <param name="playerView">プレイヤーのビュー</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        public async UniTask SetDoTeleportDelegate(DoTeleportDelegate del, Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform,
            PlayerView playerView, CancellationToken token)
        {
            var dic = StepDictionary;
            if (dic[StartDirectionStep.Teleport] == 1)
                return;

            _doTeleportDelegate = del;
            _teleportPosition = position;
            _teleportAngles = angles;
            _teleportPlayerTransform = playerTransform;
            _teleportPlayerHeadTransform = playerHeadTransform;
            _teleportPlayerView = playerView;
            dic[StartDirectionStep.Teleport] = 1;

            if (!_isRunning)
                await RunAsync(token);
        }

        /// <summary>
        /// 移動後隠蔽用の演出（フェードアウト、画面復帰など）デリゲート
        /// </summary>
        /// <param name="duration">終了時間</param>
        /// <param name="andFromTweenMode">0から1遷移演出を有効</param>
        /// <returns>オブザーバブル</returns>
        /// <see cref="Mains.Views.FadeImageView.PlayFadeOutDirection(float, bool)"/>
        /// <see cref="Selects.Views.FadeImageView.PlayFadeOutDirection(float, bool)"/>
        public delegate Observable<bool> DoPostHideEffectDelegate(float duration = .5f, bool andFromTweenMode = true);
        /// <summary>移動後隠蔽用の演出（フェードアウト、画面復帰など）デリゲート</summary>
        private DoPostHideEffectDelegate _doPostHideEffectDelegate;
        /// <summary>終了時間</summary>
        private float _postHideEffectDuration;
        /// <summary>0から1遷移演出を有効</summary>
        private bool _andFromTweenMode;

        /// <summary>
        /// 移動後隠蔽用の演出（フェードアウト、画面復帰など）デリゲートをセット
        /// </summary>
        /// <param name="del">移動後隠蔽用の演出（フェードアウト、画面復帰など）デリゲート</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <param name="duration">終了時間</param>
        /// <param name="andFromTweenMode">0から1遷移演出を有効</param>
        /// <returns>UniTask</returns>
        public async UniTask SetDoPostHideEffectDelegate(DoPostHideEffectDelegate del, CancellationToken token, float duration = .5f, bool andFromTweenMode = true)
        {
            var dic = StepDictionary;
            if (dic[StartDirectionStep.PostHideEffect] == 1)
                return;

            _doPostHideEffectDelegate = del;
            _postHideEffectDuration = duration;
            _andFromTweenMode = andFromTweenMode;
            dic[StartDirectionStep.PostHideEffect] = 1;

            if (!_isRunning)
                await RunAsync(token);
        }

        /// <summary>
        /// Tween移動処理（スムーズな移動アニメーション）デリゲート
        /// </summary>
        /// <param name="position">移動先の位置</param>
        /// <param name="angles">移動先の角度</param>
        /// <param name="playerTransform">プレイヤーのTransform</param>
        /// <param name="playerHeadTransform">プレイヤー頭のTransform</param>
        /// <param name="durations">移動アニメーションの各ステップの終了時間配列</param>
        /// <param name="playerView">プレイヤーのビュー</param>
        /// <returns>オブザーバブル</returns>
        public delegate Observable<Unit> DoTweenMoveDelegate(Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform, float[] durations, PlayerView playerView);
        /// <summary>Tween移動処理（スムーズな移動アニメーション）デリゲート</summary>
        private DoTweenMoveDelegate _doTweenMoveDelegate;
        /// <summary>移動先の位置</summary>
        private Vector3 _tweenMovePosition;
        /// <summary>移動先の角度</summary>
        private Vector3 _tweenMoveAngles;
        /// <summary>プレイヤーのTransform</summary>
        private Transform _tweenPlayerTransform;
        /// <summary>プレイヤー頭のTransform</summary>
        private Transform _tweenPlayerHeadTransform;
        /// <summary>移動アニメーションの各ステップの終了時間配列</summary>
        private float[] _tweenMoveDurations;
        /// <summary>プレイヤーのビュー</summary>
        private PlayerView _tweenPlayerView;

        /// <summary>
        /// Tween移動処理（スムーズな移動アニメーション）デリゲートをセット
        /// </summary>
        /// <param name="del">Tween移動処理（スムーズな移動アニメーション）デリゲート</param>
        /// <param name="position">移動先の位置</param>
        /// <param name="angles">移動先の角度</param>
        /// <param name="playerTransform">プレイヤーのTransform</param>
        /// <param name="playerHeadTransform">プレイヤー頭のTransform</param>
        /// <param name="durations">移動アニメーションの各ステップの終了時間配列</param>
        /// <param name="playerView">プレイヤーのビュー</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        public async UniTask SetDoTweenMoveDelegate(DoTweenMoveDelegate del, Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform, float[] durations,
            PlayerView playerView, CancellationToken token)
        {
            var dic = StepDictionary;
            if (dic[StartDirectionStep.TweenMove] == 1)
                return;

            _doTweenMoveDelegate = del;
            _tweenMovePosition = position;
            _tweenMoveAngles = angles;
            _tweenPlayerTransform = playerTransform;
            _tweenPlayerHeadTransform = playerHeadTransform;
            _tweenMoveDurations = durations;
            _tweenPlayerView = playerView;
            dic[StartDirectionStep.TweenMove] = 1;

            if (!_isRunning)
                await RunAsync(token);
        }

        /// <summary>事後処理（入力有効化、BGM再開など）デリゲート</summary>
        /// <remarks>事前処理に対して入力値を反転させて再呼び出しさせる用</remarks>
        private DoPreProcessDelegate _doPostProcessDelegate;
        /// <summary>キャラクターコントローラーの有効／無効</summary>
        private bool _postProcessCharacterControllerEnabled;
        /// <summary>Rewiredプレイヤーの未設定／有効／無効</summary>
        private int _postProcessPlayerEnabled;

        /// <summary>
        /// 事後処理（入力有効化、BGM再開など）デリゲートをセット
        /// </summary>
        /// <param name="del">事後処理デリゲート</param>
        /// <param name="characterControllerEnabled">キャラクターコントローラーの有効／無効</param>
        /// <param name="playerEnabled">Rewiredプレイヤーの未設定／有効／無効</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        public async UniTask SetDoPostProcessDelegate(DoPreProcessDelegate del, bool characterControllerEnabled, int playerEnabled, CancellationToken token)
        {
            var dic = StepDictionary;
            if (dic[StartDirectionStep.PostProcess] == 1)
                return;

            _doPostProcessDelegate = del;
            _postProcessCharacterControllerEnabled = characterControllerEnabled;
            _postProcessPlayerEnabled = playerEnabled;
            dic[StartDirectionStep.PostProcess] = 1;

            if (!_isRunning)
                await RunAsync(token);
        }

        /// <summary>
        /// ステージ開始演出のモードを初期化
        /// </summary>
        /// <returns>ステージ開始演出のモード</returns>
        private StartDirectionMode InitializeMode()
        {
            var set = settings;
            var mode = _startDirectionMode;
            // 条件①
            var currentSceneName = SceneManager.GetActiveScene().name;
            if (!currentSceneName.Equals(set.targetSceneName))
            {

                return StartDirectionMode.MAIN_SCENE;
            }
            else
            {
                // 条件②
                UserBean userBean = UserBean;
                bool isNormal = TutorialConditionEvaluator.ShouldSkip(userBean);
                if (isNormal)
                {
                    // 条件③
                    var sceneIdx = userBean.sceneIdx;
                    bool isMove = sceneIdx < 5;
                    if (isMove)
                    {
                        
                        return StartDirectionMode.SELECT_SCENE_AND_NORMAL_AND_FROM_SCENE_ROOMS;
                    }
                    else
                    {
                        
                        return StartDirectionMode.SELECT_SCENE_AND_NORMAL_AND_FROM_SCENE_TITLE;
                    }
                }
                else
                {
                    bool isMoveTutorial = TutorialConditionEvaluator.ShouldRunMove(userBean);
                    if (isMoveTutorial)
                    {
                        return StartDirectionMode.SELECT_SCENE_AND_TUTORIALS_3;
                    }
                    else
                    {
                        var strategyType = set.playerTeleporterStrategySOsLink?.PlayerTeleporterStrategy.PlayerTeleporterStrategyType ?? PlayerTeleporterStrategyType.None;
                        switch (strategyType)
                        {
                            case PlayerTeleporterStrategyType.None:
                                Debug.LogError("プレイヤー移動演出ストラテジータイプが未設定です。");

                                break;
                            case PlayerTeleporterStrategyType.Tween:

                                return StartDirectionMode.SELECT_SCENE_AND_TUTORIALS;
                            case PlayerTeleporterStrategyType.FadeAndTeleport:

                                return StartDirectionMode.SELECT_SCENE_AND_TUTORIALS_1;
                            case PlayerTeleporterStrategyType.Teleport:

                                return StartDirectionMode.SELECT_SCENE_AND_TUTORIALS_2;
                        }
                    }
                }
            }

            return mode;
        }

        /// <summary>
        /// ステージ開始演出のステップディクショナリを初期化
        /// </summary>
        /// <returns>ステージ開始演出のステップディクショナリ</returns>
        private Dictionary<StartDirectionStep, int> InitializeStepDictionary()
        {
            var dic = _stepDictionary;
            dic = new Dictionary<StartDirectionStep, int>();
            foreach (StartDirectionStep startDirectionStep in System.Enum.GetValues(typeof(StartDirectionStep)))
            {
                dic[startDirectionStep] = 0;
            }

            return dic;
        }

        /// <summary>
        /// 実行開始
        /// </summary>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask RunAsync(CancellationToken token)
        {
            var set = settings;
            var viewModel = set.viewModel;
            viewModel.Initialize();
            var isCompletedDirection = viewModel.IsCompletedStartDirection.CurrentValue;
            _isRunning = true;
            Initialize();
            _startDirectionMode = InitializeMode();
            InitializeAllDelegates(StartDirectionMode, isCompletedDirection, token);

            await Observable.EveryUpdate()
                .Select(_ => StepDictionary)
                .Where(dic => IsAllEntered(dic))
                .FirstAsync(token);

            await ExecuteAsync(token);

            viewModel.SetIsCompletedStartDirection(true);
            _stepDictionary.Clear();
            _isRunning = false;
        }

        private void Initialize()
        {
            ResourcesUtility utility = new ResourcesUtility();
            UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            _userBean = userBean;
        }

        /// <summary>
        /// 全てのデリゲートを初期化
        /// </summary>
        /// <param name="startDirectionMode">ステージ開始演出のモード</param>
        /// <param name="isCompletedDirection">ステージ開始演出が完了したか</param>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <remarks>ステージ開始演出のモードに応じて処理のセットされないデリゲートはあらかじめ初期化する</remarks>
        private void InitializeAllDelegates(StartDirectionMode startDirectionMode, bool isCompletedDirection, CancellationToken token)
        {
            Observable<bool> DummyPreHideEffect(float duration = .5f)
            {
                return Observable.Create<bool>(observer =>
                {
                    observer.OnNext(true);
                    observer.OnCompleted();

                    return Disposable.Empty;
                });
            }
            void DummyTeleport(Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform, PlayerView playerView) { }
            Observable<Unit> DummyTweenMove(Vector3 position, Vector3 angles, Transform playerTransform, Transform playerHeadTransform, float[] durations, PlayerView playerView)
            {
                return Observable.Create<Unit>(observer =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();

                    return Disposable.Empty;
                });
            }
            Observable<bool> DummyPostHideEffect(float duration = .5f, bool andFromTweenMode = true)
            {
                return Observable.Create<bool>(observer =>
                {
                    observer.OnNext(true);
                    observer.OnCompleted();

                    return Disposable.Empty;
                });
            }

            DoPreHideEffectDelegate doPreHideEffectDelegate = (duration) => DummyPreHideEffect(duration);
            DoTeleportDelegate doTeleportDelegate = (position, angles, playerTransform, playerHeadTransform, playerView) => DummyTeleport(position, angles, playerTransform, playerHeadTransform, playerView);
            DoTweenMoveDelegate doTweenMoveDelegate = (position, angles, playerTransform, playerHeadTransform, durations, playerView) => DummyTweenMove(position, angles, playerTransform, playerHeadTransform, durations, playerView);
            DoPostHideEffectDelegate doPostHideEffectDelegate = (duration, andFromTweenMode) => DummyPostHideEffect(duration, andFromTweenMode);

            switch (startDirectionMode)
            {
                case StartDirectionMode.MAIN_SCENE:
                case StartDirectionMode.SELECT_SCENE_AND_NORMAL_AND_FROM_SCENE_TITLE:
                case StartDirectionMode.SELECT_SCENE_AND_TUTORIALS_3:
                    SetDoPreHideEffectDelegate(doPreHideEffectDelegate, token, default).Forget();
                    SetDoTeleportDelegate(doTeleportDelegate, default, default, default, default, default, token).Forget();
                    SetDoTweenMoveDelegate(doTweenMoveDelegate, default, default, default, default, default, default, token).Forget();

                    break;
                case StartDirectionMode.SELECT_SCENE_AND_NORMAL_AND_FROM_SCENE_ROOMS:
                    SetDoPreHideEffectDelegate(doPreHideEffectDelegate, token, default).Forget();
                    SetDoTweenMoveDelegate(doTweenMoveDelegate, default, default, default, default, default, default, token).Forget();

                    break;
                case StartDirectionMode.SELECT_SCENE_AND_TUTORIALS:
                    SetDoPreHideEffectDelegate(doPreHideEffectDelegate, token, default).Forget();
                    SetDoTeleportDelegate(doTeleportDelegate, default, default, default, default, default, token).Forget();
                    if (isCompletedDirection)
                        SetDoPostHideEffectDelegate(doPostHideEffectDelegate, token, default, default).Forget();

                    break;
                case StartDirectionMode.SELECT_SCENE_AND_TUTORIALS_1:
                    if (!isCompletedDirection)
                    {
                        SetDoPreHideEffectDelegate(doPreHideEffectDelegate, token, default).Forget();
                    }
                    SetDoTweenMoveDelegate(doTweenMoveDelegate, default, default, default, default, default, default, token).Forget();

                    break;
                case StartDirectionMode.SELECT_SCENE_AND_TUTORIALS_2:
                    SetDoPreHideEffectDelegate(doPreHideEffectDelegate, token, default).Forget();
                    if (isCompletedDirection)
                    {
                        SetDoPostHideEffectDelegate(doPostHideEffectDelegate, token, default, default).Forget();
                    }
                    SetDoTweenMoveDelegate(doTweenMoveDelegate, default, default, default, default, default, default, token).Forget();

                    break;
            }
        }

        /// <summary>
        /// 全て登録したかを判定する
        /// </summary>
        /// <param name="stepDictionary">ステップディクショナリ</param>
        /// <returns>全て登録したか</returns>
        private bool IsAllEntered(Dictionary<StartDirectionStep, int> stepDictionary)
        {
            if (stepDictionary == null || stepDictionary.Count < 1)
                return false;

            foreach (var status in stepDictionary.Values)
            {
                if (status == 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="token">キャンセラレーショントークン</param>
        /// <returns>UniTask</returns>
        private async UniTask ExecuteAsync(CancellationToken token)
        {
            _doPreProcessDelegate.Invoke(_preProcessCharacterControllerEnabled, _characterController, _preProcessPlayerEnabled, _player);

            await _doPreHideEffectDelegate.Invoke(_preHideEffectDuration)
                .Where(x => x)
                .FirstAsync(token);

            _doTeleportDelegate.Invoke(_teleportPosition, _teleportAngles, _teleportPlayerTransform, _teleportPlayerHeadTransform, _teleportPlayerView);

            await _doPostHideEffectDelegate.Invoke(_postHideEffectDuration, _andFromTweenMode)
                .Where(x => x)
                .FirstAsync(token);

            await _doTweenMoveDelegate.Invoke(_tweenMovePosition, _tweenMoveAngles, _tweenPlayerTransform, _tweenPlayerHeadTransform, _tweenMoveDurations, _tweenPlayerView)
                .FirstAsync(token);

            _doPostProcessDelegate.Invoke(_postProcessCharacterControllerEnabled, _characterController, _postProcessPlayerEnabled, _player);
        }

        public void Dispose()
        {
            var set = settings;
            set.viewModel.Dispose();
            _startDirectionMode = StartDirectionMode.EMPTY;
            _stepDictionary?.Clear();
            _isRunning = false;
            _userBean = null;
        }
    }

    /// <summary>
    /// ステージ開始演出の設定
    /// </summary>
    [System.Serializable]
    public class StartDirectionSettings
    {
        /// <summary>ステージ開始演出のビューモデル</summary>
        public StartDirectionViewModel viewModel;
        /// <summary>プレイヤー移動演出ストラテジーの設定を同期管理させる</summary>
        public PlayerTeleporterStrategySOsLink playerTeleporterStrategySOsLink;
        /// <summary>対象シーン名</summary>
        /// <remarks>セレクトシーンを指定する</remarks>
        public string targetSceneName;
    }

    /// <summary>
    /// ステージ開始演出のモード
    /// </summary>
    public enum StartDirectionMode
    {
        /// <summary>未登録</summary>
        EMPTY,
        /// <summary>メイン</summary>
        MAIN_SCENE,
        /// <summary>セレクトかつ通常かつステージ番号0～4</summary>
        SELECT_SCENE_AND_NORMAL_AND_FROM_SCENE_ROOMS,
        /// <summary>セレクトかつ通常かつステージ番号5</summary>
        SELECT_SCENE_AND_NORMAL_AND_FROM_SCENE_TITLE,
        /// <summary>セレクトかつチュートリアル①</summary>
        SELECT_SCENE_AND_TUTORIALS,
        /// <summary>セレクトかつチュートリアル②</summary>
        SELECT_SCENE_AND_TUTORIALS_1,
        /// <summary>セレクトかつチュートリアル③</summary>
        SELECT_SCENE_AND_TUTORIALS_2,
        /// <summary>セレクトかつチュートリアル_基本操作</summary>
        SELECT_SCENE_AND_TUTORIALS_3,
    }

    /// <summary>
    /// ステージ開始演出のステップ
    /// </summary>
    public enum StartDirectionStep
    {
        /// <summary>事前処理（入力無効化、BGM一時停止など）</summary>
        PreProcess,

        /// <summary>移動前隠蔽用の演出（フェードイン、画面暗転など）</summary>
        PreHideEffect,

        /// <summary>瞬間移動処理（座標・回転の即時設定）</summary>
        Teleport,

        /// <summary>移動後隠蔽用の演出（フェードアウト、画面復帰など）</summary>
        PostHideEffect,

        /// <summary>Tween移動処理（スムーズな移動アニメーション）</summary>
        TweenMove,

        /// <summary>事後処理（入力有効化、BGM再開など）</summary>
        PostProcess,
    }
}
