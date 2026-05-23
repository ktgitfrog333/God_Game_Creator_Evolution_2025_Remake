using CriWare;
using Mains.Views;
using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Mains.External
{
    /// <summary>
    /// シロさんのコンポーネントへアクセスするAPI
    /// </summary>
    public class Script_xyloApi : System.IDisposable
    {
        private Test_FootStep _test_FootStep;
        private MicInput_Criware _micInput_Criware;
        private readonly ReactiveCommand<float> _frameRateReactive = new ReactiveCommand<float>();
        public ReactiveCommand<float> FrameRateReactive => _frameRateReactive;
        private float _basicBeat;
        public float BasicBeat => _basicBeat;
        /// <see cref="CRIWARE_conductor.TempoSet"/>
        private ReactiveCommand<bool> _isOnTempoMethodEventAny = new ReactiveCommand<bool>();
        public ReactiveCommand<bool> IsOnTempoMethodEventAny => _isOnTempoMethodEventAny;
        private void OnTempoMethodEventAny()
        {
            _isOnTempoMethodEventAny.Execute(true);
        }
        private MissileDirectAnimManagerB _missileDirectAnimManagerB;
        private readonly ReactiveCommand<bool> _isSuccessful = new ReactiveCommand<bool>();
        public ReactiveCommand<bool> IsSuccessfulReactive => _isSuccessful;
        private readonly ReactiveCommand<bool> _isFailed = new ReactiveCommand<bool>();
        public ReactiveCommand<bool> IsFailedReactive => _isFailed;

        private readonly Subject<Unit> _onHpDecreased = new Subject<Unit>();
        public Observable<Unit> OnHpDecreased => _onHpDecreased;

        private readonly Subject<Unit> _onBatteryPicked = new Subject<Unit>();
        public Observable<Unit> OnBatteryPicked => _onBatteryPicked;

        public Transform NoteTransform
        {
            get
            {
                if (_missileDirectAnimManagerB == null)
                {
                    return null;
                }

                // プライベートフィールド `_uiManager` をリフレクションで取得
                var containerObject = GetContainerObjectInMissileDirectAnimManagerB(_missileDirectAnimManagerB);

                return containerObject.transform as RectTransform;
            }
        }
        private readonly ReactiveCommand<float> _lifeBeat = new ReactiveCommand<float>();
        public ReactiveCommand<float> LifeBeat => _lifeBeat;
        public float OneBeat => throw new System.NotImplementedException();
        private FieldInfo _homingObjectLifeBeatField;
        private HomingObject _homingObject;
        private MissileObjectPooler _missileObjectPooler;
        public Queue<GameObject>[] MissileGameObjects
        {
            get
            {
                if (_missileObjectPooler == null)
                {
                    Debug.LogWarning("MissileObjectPoolerがセットされていません。");
                    return null;
                }

                var fieldInfo = typeof(MissileObjectPooler).GetField("missilePoolDict", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogWarning("missilePoolDictフィールドが見つかりませんでした。");
                    return null;
                }

                var dict = fieldInfo.GetValue(_missileObjectPooler) as Dictionary<int, Queue<GameObject>>;
                if (dict == null)
                {
                    Debug.LogWarning("missilePoolDictの取得に失敗しました。");
                    return null;
                }

                var missileTypesField = typeof(MissileObjectPooler).GetField("missileTypes", BindingFlags.NonPublic | BindingFlags.Instance);
                if (missileTypesField == null)
                {
                    Debug.LogWarning("missileTypesフィールドが見つかりませんでした。");
                    return null;
                }

                var missileTypes = missileTypesField.GetValue(_missileObjectPooler) as List<MissileObjectPooler.MissileType>;
                if (missileTypes == null)
                {
                    Debug.LogWarning("missileTypesの取得に失敗しました。");
                    return null;
                }

                var missileIds = missileTypes.Select(q => q.missileId).ToArray();

                var matchingQueues = dict
                    .Where(pair => missileIds.Any(id => id == pair.Key))
                    .Select(pair => pair.Value)
                    .ToArray();

                return matchingQueues;
            }
        }
        private readonly ReactiveCommand<Color> _missileRendererColor = new ReactiveCommand<Color>();
        public ReactiveCommand<Color> MissileRendererColor => _missileRendererColor;

        public Transform[] GetNoteTransforms(Queue<GameObject>[] missileGameObjects)
        {
            if (missileGameObjects == null)
            {
                Debug.LogWarning("MissileGameObjectsがnullです。");
                return null;
            }

            List<Transform> transforms = new List<Transform>();

            foreach (var queue in missileGameObjects)
            {
                foreach (var gameObject in queue)
                {
                    if (gameObject != null)
                    {
                        MissileDirectAnimManagerB missileDirectAnimManagerB = gameObject.GetComponent<MissileDirectAnimManagerB>();
                        var managerType = missileDirectAnimManagerB.GetType();
                        var containerObject = GetContainerObjectInMissileDirectAnimManagerB(missileDirectAnimManagerB);
                        transforms.Add(containerObject.transform);
                    }
                }
            }

            return transforms.ToArray();
        }
        /// <summary>BGMの再生状態</summary>
        /// <see cref="CriWare.CriAtomSourceBase.Status"/>
        private readonly ReactiveCommand<int> _bgmCStatus = new ReactiveCommand<int>();
        /// <summary>BGMの再生状態</summary>
        public ReactiveCommand<int> BgmCStatus => _bgmCStatus;
        /// <summary>BGMの更新</summary>
        System.IDisposable _currentSourceStatusDisposable;
        private ObjectPoolerXyloOther _objectPoolerXyloOther;
        public Dictionary<string, Queue<GameObject>> PoolDictionary => _objectPoolerXyloOther.poolDictionary;
        public bool IsReturningToPool
        {
            get
            {
                if (_missileDirectAnimManagerB == null) return false;

                var managerType = _missileDirectAnimManagerB.GetType();
                var fieldInfo = managerType.GetField("isReturningToPool", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogWarning("isReturningToPool フィールドが見つかりませんでした。");
                    return false;
                }

                var value = fieldInfo.GetValue(_missileDirectAnimManagerB);
                return value != null && (bool)value;
            }
        }
        public bool IsForceReturning
        {
            get
            {
                if (_missileDirectAnimManagerB == null) return false;

                var managerType = _missileDirectAnimManagerB.GetType();
                var fieldInfo = managerType.GetField("isForceReturning", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogWarning("isForceReturning フィールドが見つかりませんでした。");
                    return false;
                }

                var value = fieldInfo.GetValue(_missileDirectAnimManagerB);
                return value != null && (bool)value;
            }
        }
        public bool IsPointerOverUI
        {
            get
            {
                var isUIActive = IsUIActiveInMissileDirectAnimManagerB(_missileDirectAnimManagerB);

                return isUIActive;
            }
        }
        public bool EnableClickDetection
        {
            get
            {
                if (_missileDirectAnimManagerB == null)
                {
                    Debug.LogWarning("MissileDirectAnimManagerBがセットされていません。");
                    return false;
                }
                var managerType = _missileDirectAnimManagerB.GetType();
                var enableClickDetectionField = managerType.GetField("enableClickDetection", BindingFlags.NonPublic | BindingFlags.Instance);
                if (enableClickDetectionField == null)
                {
                    Debug.LogWarning("enableClickDetection フィールドが見つかりませんでした。");
                    return false;
                }
                var enableClickDetection = enableClickDetectionField.GetValue(_missileDirectAnimManagerB);
                if (enableClickDetection == null)
                {
                    Debug.LogWarning("enableClickDetection が null です。");
                    return false;
                }
                return (bool)enableClickDetection;
            }
        }
        public bool IsSuccessful
        {
            get
            {
                if (_missileDirectAnimManagerB == null) return false;

                var managerType = _missileDirectAnimManagerB.GetType();
                var fieldInfo = managerType.GetField("isSuccessful", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogWarning("isSuccessful フィールドが見つかりませんでした。");
                    return false;
                }

                var value = fieldInfo.GetValue(_missileDirectAnimManagerB);
                return value != null && (bool)value;
            }
        }

        public bool IsFailed
        {
            get
            {
                if (_missileDirectAnimManagerB == null) return false;

                var managerType = _missileDirectAnimManagerB.GetType();
                var fieldInfo = managerType.GetField("isFailed", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogWarning("isFailed フィールドが見つかりませんでした。");
                    return false;
                }

                var value = fieldInfo.GetValue(_missileDirectAnimManagerB);
                return value != null && (bool)value;
            }
        }

        public float ObjectCreationTime
        {
            get
            {
                if (_missileDirectAnimManagerB == null) return -1f;

                var type = _missileDirectAnimManagerB.GetType();
                var fieldInfo = type.GetField("objectCreationTime", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogWarning("objectCreationTime フィールドが見つかりませんでした。");
                    return -1f;
                }

                var value = fieldInfo.GetValue(_missileDirectAnimManagerB);
                return value is float floatVal ? floatVal : -1f;
            }
        }

        public int NoteType
        {
            get
            {
                if (_missileDirectAnimManagerB == null) return (int)MissileNoteType.None;

                var type = _missileDirectAnimManagerB.GetType();
                var fieldInfo = type.GetField("noteType", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogWarning("noteType フィールドが見つかりませんでした。");
                    return (int)MissileNoteType.None;
                }

                var value = fieldInfo.GetValue(_missileDirectAnimManagerB);
                return value is MissileNoteType enumVal ? (int)enumVal : (int)MissileNoteType.None;
            }
        }

        public bool IsLongPressStarted
        {
            get
            {
                if (_missileDirectAnimManagerB == null) return false;

                var type = _missileDirectAnimManagerB.GetType();
                var fieldInfo = type.GetField("isLongPressStarted", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogWarning("isLongPressStarted フィールドが見つかりませんでした。");
                    return false;
                }

                var value = fieldInfo.GetValue(_missileDirectAnimManagerB);
                return value is bool boolVal && boolVal;
            }
        }
        /// <summary>ボリュームスライダーのMAX値が2fへセットされたか</summary>
        /// <remarks>タイトル画面のスライダー設定では最終的にMAXは10になっているのでそれに合わせる必要がある<br/>
        /// しかし、MicInputのディテクターではStartで2が設定されるためディテクターの後に呼ばないと2のままになってしまうため<br/>
        /// MAX値を2へ更新されるのを待つためのリアクティブなフラグを追加</remarks>
        /// <see cref="TitleScreenController.SetupOptionsSliders"/>
        /// <see cref="MicInput_Criware.Start"/>
        private ReactiveCommand<bool> _isVolumeSliderMaxValueToTwo = new();
        /// <summary>ボリュームスライダーのMAX値が2fへセットされたか</summary>
        public ReactiveCommand<bool> IsVolumeSliderMaxValueToTwo => _isVolumeSliderMaxValueToTwo;
        /// <summary>ボリュームスライダーのMAX値が2fへセットされたかDisposable</summary>
        private readonly SerialDisposable _volumeSliderMaxValueToTwoDisposable = new SerialDisposable();

        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        public Script_xyloApi()
        {
            Test_FootStep test_FootStep = Component.FindAnyObjectByType<Test_FootStep>();
            if (test_FootStep != null)
            {
                _test_FootStep = test_FootStep;
            }
            MicInput_Criware micInput_Criware = Component.FindAnyObjectByType<MicInput_Criware>();
            if (micInput_Criware != null)
                _micInput_Criware = micInput_Criware;
            Observable.EveryUpdate()
                .Select(_ => CRIWARE_conductor.Instance)
                .Where(x => x != null)
                .Select(x => x.frameRate)
                .Pairwise()
                .Where(frameRate => frameRate.Previous != frameRate.Current)
                .Subscribe(frameRate =>
                {
                    _frameRateReactive.Execute(frameRate.Current);
                })
                .AddTo(ref _disposableBag);
            CRIWARE_conductor.TempoMethodEvent1 += OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent2 += OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent3 += OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent4 += OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent5 += OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent6 += OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent7 += OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent8 += OnTempoMethodEventAny;
            Observable.EveryUpdate()
                .Select(_ => CRIWARE_conductor.Instance)
                .Where(x => x != null)
                .Subscribe(conductor =>
                {
                    _basicBeat = conductor.BasicBeat;
                })
                .AddTo(ref _disposableBag);
        }

        public void SetMissileDirectAnimManagerB(Transform transform)
        {
            var missileDirectAnimManagerB = transform.GetComponent<MissileDirectAnimManagerB>();
            if (missileDirectAnimManagerB != null)
            {
                _missileDirectAnimManagerB = missileDirectAnimManagerB;
                Observable.EveryUpdate()
                    .Select(_ => missileDirectAnimManagerB.IsSuccessful())
                    .Pairwise()
                    .Where(x => x.Previous != x.Current)
                    .Select(x => x.Current)
                    .Subscribe(isSuccessful =>
                    {
                        _isSuccessful.Execute(isSuccessful);
                    })
                    .AddTo(ref _disposableBag);
                Observable.EveryUpdate()
                    .Select(_ => missileDirectAnimManagerB.IsFailed())
                    .Pairwise()
                    .Where(x => x.Previous != x.Current)
                    .Select(x => x.Current)
                    .Subscribe(isFailed =>
                    {
                        _isFailed.Execute(isFailed);
                    })
                    .AddTo(ref _disposableBag);
                Observable.EveryUpdate()
                    .Select(_ =>
                    {
                        var type = missileDirectAnimManagerB.GetType();
                        FieldInfo fieldInfo = type.GetField("micInputManager", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fieldInfo == null)
                        {
                            Debug.LogWarning("micInputManagerフィールドが見つかりませんでした。");

                            return 0;
                        }
                        var value = fieldInfo.GetValue(missileDirectAnimManagerB);
                        if (value == null)
                        {
                            Debug.LogWarning("micInputManagerフィールドから値の取得に失敗しました。");

                            return 0;
                        }
                        MissileMicInputManager manager = (MissileMicInputManager)value;
                        int score = manager.GetCurrentScore();

                        return score;
                    })
                    .DistinctUntilChanged()
                    .Subscribe(score =>
                    {
                        _score.Execute(score);
                    })
                    .AddTo(ref _disposableBag);
                Observable.EveryUpdate()
                    .Where(_ => missileDirectAnimManagerB.gameObject.activeSelf)
                    .Select(_ => missileDirectAnimManagerB.GetComponentInChildren<SpectrumGauge>())
                    .Where(x => x != null)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        _durabilityRateTarget.OnNext(Unit.Default);
                        _durabilityRateTarget.OnCompleted();
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        /// <summary>耐久率UI表示フラグ</summary>
        private Subject<Unit> _durabilityRateTarget = new Subject<Unit>();
        /// <summary>耐久率UI表示フラグ</summary>
        public Subject<Unit> DurabilityRateTarget => _durabilityRateTarget;

        private ReactiveCommand<int> _score = new ReactiveCommand<int>();
        public ReactiveCommand<int> Score => _score;

        /// <summary>
        /// マイクボタン入力セット処理
        /// </summary>
        /// <param name="dbLevel">デシベルレベル</param>
        public void SetMicButtonInput(float dbLevel)
        {
            if (_micInput_Criware == null)
                return;

            var type = typeof(MicInput_Criware);

            // privateフィールドをリフレクションで取得
            var volumeHistoryField = type.GetField("volumeHistory", BindingFlags.NonPublic | BindingFlags.Instance);
            var timeHistoryField = type.GetField("timeHistory", BindingFlags.NonPublic | BindingFlags.Instance);
            var totalVolumeField = type.GetField("totalVolume", BindingFlags.NonPublic | BindingFlags.Instance);

            if (volumeHistoryField == null || timeHistoryField == null || totalVolumeField == null)
            {
                Debug.LogWarning("MicInput_Criware の volumeHistory / timeHistory / totalVolume フィールドが見つかりませんでした。");
                return;
            }

            var volumeHistory = volumeHistoryField.GetValue(_micInput_Criware) as Queue<float>;
            var timeHistory = timeHistoryField.GetValue(_micInput_Criware) as Queue<float>;
            var totalVolumeObj = totalVolumeField.GetValue(_micInput_Criware);

            if (volumeHistory == null || timeHistory == null || totalVolumeObj == null)
            {
                Debug.LogWarning("MicInput_Criware の volumeHistory / timeHistory / totalVolume の値取得に失敗しました。");
                return;
            }

            float totalVolume = (float)totalVolumeObj;
            float currentTime = Time.time;
            // publicフィールドを直接参照
            float averagingDuration = _micInput_Criware.averagingDuration;

            // 新しいデータを履歴へ追加
            volumeHistory.Enqueue(dbLevel);
            timeHistory.Enqueue(currentTime);
            totalVolume += dbLevel;

            // 古いデータを除去（averagingDuration秒より古いデータ）
            while (timeHistory.Count > 0 && currentTime - timeHistory.Peek() > averagingDuration)
            {
                timeHistory.Dequeue();
                totalVolume -= volumeHistory.Dequeue();
            }

            // totalVolumeをリフレクションで書き戻す
            totalVolumeField.SetValue(_micInput_Criware, totalVolume);
        }

        public void SetHomingObject(Transform transform)
        {
            _homingObject = transform.GetComponent<HomingObject>();
            if (_homingObject != null)
            {
                // LifeBeatフィールドをリフレクションで取得
                _homingObjectLifeBeatField = typeof(HomingObject).GetField("LifeBeat", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_homingObjectLifeBeatField == null)
                {
                    Debug.LogWarning("LifeBeatフィールドが見つかりませんでした。");
                    return;
                }

                Observable.EveryUpdate()
                    .Select(_ =>
                    {
                        if (_homingObject == null || _homingObjectLifeBeatField == null)
                            return 0f;
                        return (float)_homingObjectLifeBeatField.GetValue(_homingObject);
                    })
                    .DistinctUntilChanged()
                    .Subscribe(lifeBeat =>
                    {
                        _lifeBeat.Execute(lifeBeat);
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        public void SetMissileObjectPooler(Transform transform)
        {
            MissileObjectPooler missileObjectPooler = transform.GetComponent<MissileObjectPooler>();
            if (missileObjectPooler != null)
            {
                _missileObjectPooler = missileObjectPooler;
            }
        }

        /// <summary>
        /// 足音を鳴らすSEを再生する
        /// </summary>
        public void StartFootsteps()
        {
            if (_test_FootStep == null)
            {
                Debug.LogWarning("Test_FootStep インスタンスが見つかりません。");
                return;
            }
            if (SE_Picker.Instance == null)
            {
                Debug.LogWarning("SE_Picker インスタンスが見つかりません。");
                return;
            }

            // Test_FootStep クラスの StartFootsteps メソッドを取得
            MethodInfo methodInfo = _test_FootStep.GetType().GetMethod("StartFootsteps", BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo != null)
            {
                // メソッドを実行
                methodInfo.Invoke(_test_FootStep, null);
            }
            else
            {
                Debug.LogWarning("StartFootsteps メソッドが見つかりません。");
            }
        }

        /// <summary>
        /// 足音を鳴らすSEを停止する
        /// </summary>
        public void StopFootsteps()
        {
            if (_test_FootStep == null)
            {
                Debug.LogWarning("Test_FootStep インスタンスが見つかりません。");
                return;
            }
            if (SE_Picker.Instance == null)
            {
                Debug.LogWarning("SE_Picker インスタンスが見つかりません。");
                return;
            }

            // Test_FootStep クラスの StopFootsteps メソッドを取得
            MethodInfo methodInfo = _test_FootStep.GetType().GetMethod("StopFootsteps", BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo != null)
            {
                // メソッドを実行
                methodInfo.Invoke(_test_FootStep, null);
            }
            else
            {
                Debug.LogWarning("StopFootsteps メソッドが見つかりません。");
            }
        }

        /// <summary>
        /// 足音を再生
        /// </summary>
        /// <remarks>TODO: Test_FootStep クラスにある PlayFootStep メソッドをパプリックにする
        /// </remarks>
        /// <see cref="Test_FootStep.PlayFootStep"/>
        public void PlayFootStep()
        {
            if (_test_FootStep == null)
            {
                Debug.LogWarning("Test_FootStep インスタンスが見つかりません。");
                return;
            }
            if (SE_Picker.Instance == null)
            {
                return;
            }

            // Test_FootStep クラスの PlayFootStep メソッドを取得
            MethodInfo methodInfo = _test_FootStep.GetType().GetMethod("PlayFootStep", BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo != null)
            {
                // メソッドを実行
                methodInfo.Invoke(_test_FootStep, null);
            }
            else
            {
                Debug.LogWarning("PlayFootStep メソッドが見つかりません。");
            }
        }

        public void PlayMove5()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Selects.Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayMove5(seVolumeIndex);
        }

        public void PlaySubmit2()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Selects.Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlaySubmit2(seVolumeIndex);
        }

        public void PlayCancel4()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Selects.Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayCancel4(seVolumeIndex);
        }

        public void PlayHeartbeatFast()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayHeartbeatFast(seVolumeIndex);
        }

        public void PlayHeartbeatSlow()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayHeartbeatSlow(seVolumeIndex);
        }

        public void PlayHitSuccess3()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayHitSuccess3(seVolumeIndex);
        }

        public void PlayHitMiss3()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayHitMiss3(seVolumeIndex);
        }

        public void PlayBatteryLost1()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayBatteryLost1(seVolumeIndex);
        }

        public void PlayBatteryGet3()
        {
            _onBatteryPicked.OnNext(Unit.Default);
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayBatteryGet3(seVolumeIndex);
        }

        public void PlayDamage1()
        {
            _onHpDecreased.OnNext(Unit.Default);
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var mainsManager = Manager.GameManager.Instance;
            var selectsManager = Selects.Manager.GameManager.Instance;
            if (mainsManager != null ||
                selectsManager != null)
            {
                if (mainsManager != null &&
                    selectsManager == null)
                {
                    var seVolumeIndex = mainsManager.AudioOwner.GetSeVolumeIndex();
                    sePicker.PlayDamage1(seVolumeIndex);
                }
                else
                {
                    var seVolumeIndex = selectsManager.AudioOwner.GetSeVolumeIndex();
                    sePicker.PlayDamage1(seVolumeIndex);
                }
            }
        }

        public void PlayGhostLaugh3()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayGhostLaugh3(seVolumeIndex);
        }

        /// <summary>
        /// ボイスタイプに応じたオバケ笑い声SEを再生する
        /// </summary>
        /// <param name="voiceType">オバケボイスタイプ</param>
        public void PlayGhostLaughByVoiceType(Commons.GhostVoiceType voiceType)
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            switch (voiceType)
            {
                case Commons.GhostVoiceType.ghost_voice_chatter_type:
                    sePicker.PlayGhostLaughV2Chatter(seVolumeIndex);

                    break;
                case Commons.GhostVoiceType.ghost_voice_fat_type:
                    sePicker.PlayGhostLaughV2Fat(seVolumeIndex);

                    break;
                case Commons.GhostVoiceType.ghost_voice_normal_type:
                default:
                    sePicker.PlayGhostLaughV2Normal(seVolumeIndex);

                    break;
            }
        }

        /// <summary>
        /// ボイスタイプに応じた3Dサウンド用SE名を取得する
        /// </summary>
        /// <param name="voiceType">オバケボイスタイプ</param>
        /// <returns>SE名</returns>
        public static string GetGhostLaughSEName(Commons.GhostVoiceType voiceType)
        {
            switch (voiceType)
            {
                case Commons.GhostVoiceType.ghost_voice_chatter_type:
                    return "GhostLaughV2Chatter";
                case Commons.GhostVoiceType.ghost_voice_fat_type:
                    return "GhostLaughV2Fat";
                case Commons.GhostVoiceType.ghost_voice_normal_type:
                default:
                    return "GhostLaughV2Normal";
            }
        }


        public void PlayDoorOpen3()
        {
            var sePicker = SE_Picker.Instance;
            if (sePicker == null)
            {
                return;
            }
            var manager = Manager.GameManager.Instance;
            if (manager == null)
            {
                return;
            }
            var seVolumeIndex = manager.AudioOwner.GetSeVolumeIndex();
            sePicker.PlayDoorOpen3(seVolumeIndex);
        }

        /// <summary>
        /// ヘルパー関数StartManagedCoroutineにReturnToPoolWithDelayを渡して実行する
        /// </summary>
        public void StartManagedCoroutineInReturnToPoolWithDelay()
        {
            var missileDirectAnimManagerB = _missileDirectAnimManagerB;
            if (missileDirectAnimManagerB == null)
            {
                Debug.LogWarning("MissileDirectAnimManagerB インスタンスが見つかりません。");
                return;
            }
            MethodInfo methodInfo = missileDirectAnimManagerB.GetType().GetMethod("StartManagedCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo != null)
            {
                object[] parameters = { missileDirectAnimManagerB.ReturnToPoolWithDelay(0f) };
                // メソッドを実行
                methodInfo.Invoke(missileDirectAnimManagerB, parameters);
            }
            else
            {
                Debug.LogWarning("StartManagedCoroutine メソッドが見つかりません。");
            }
        }

        private MissileTempoSpawner _missileTempoSpawner;

        /// <summary>最初のHomingObjectスポーン通知</summary>
        private readonly ReactiveCommand<Unit> _onFirstHomingObjectSpawned = new ReactiveCommand<Unit>();
        public ReactiveCommand<Unit> OnFirstHomingObjectSpawned => _onFirstHomingObjectSpawned;

        /// <summary>currentBeatIndex監視のDisposable</summary>
        private System.IDisposable _firstSpawnWatcherDisposable;

        /// <summary>
        /// リズムパート開始時に呼ぶ。
        /// MissileTempoSpawnerのcurrentBeatIndexをリフレクションで監視し、
        /// 最初のHomingObjectが飛び出したタイミングを検知する。
        /// </summary>
        public void WatchFirstHomingObjectSpawn()
        {
            _firstSpawnWatcherDisposable?.Dispose();

            if (_missileTempoSpawner == null)
            {
                Debug.LogWarning("MissileTempoSpawnerがセットされていません。");
                return;
            }

            var spawnerType = _missileTempoSpawner.GetType();

            var beatIndexField = spawnerType.GetField(
                "currentBeatIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            var activeField = spawnerType.GetField(
                "Active", BindingFlags.Public | BindingFlags.Instance);
            if (activeField == null)
            {
                activeField = spawnerType.GetField("Active", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            var patternListField = spawnerType.GetField(
                "patternList", BindingFlags.NonPublic | BindingFlags.Instance);

            if (beatIndexField == null || activeField == null || patternListField == null)
            {
                Debug.LogWarning("MissileTempoSpawner のフィールドが見つかりませんでした。");
                return;
            }

            _firstSpawnWatcherDisposable = Observable.EveryUpdate()
                .Select(_ =>
                {
                    var active = activeField.GetValue(_missileTempoSpawner);
                    var beatIndex = beatIndexField.GetValue(_missileTempoSpawner);
                    return active is true && beatIndex is int idx && idx > 0;
                })
                .DistinctUntilChanged()
                .Where(x => x)   // false→true の瞬間だけ
                .Take(1)
                .Subscribe(_ =>
                {
                    _onFirstHomingObjectSpawned.Execute(Unit.Default);
                })
                .AddTo(ref _disposableBag);
        }

        /// <summary>
        /// 1体目のオバケ(HomingObject)のFlightPhaseがHoming状態になったことを監視するストリームを提供します。
        /// </summary>
        public Observable<Unit> OnGhostHomingStarted()
        {
            if (_homingObject == null) return Observable.Empty<Unit>();

            FieldInfo currentPhaseField = typeof(HomingObject).GetField("currentPhase", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (currentPhaseField == null)
            {
                Debug.LogError("[Script_xyloApi] HomingObject 内に 'currentPhase' フィールドが見つかりませんでした。");
                return Observable.Empty<Unit>();
            }

            return Observable.EveryUpdate()
                .TakeWhile(_ => _homingObject != null && _homingObject.gameObject != null)
                .Select(_ =>
                {
                    var phaseValue = currentPhaseField.GetValue(_homingObject);
                    return phaseValue?.ToString();
                })
                .Where(phaseName => phaseName == "Homing")
                .Take(1)
                .AsUnitObservable();
        }

        public void SetMissileTempoSpawner(Transform transform)
        {
            var missileTempoSpawner = transform.GetComponent<MissileTempoSpawner>();
            if (missileTempoSpawner != null)
            {
                _missileTempoSpawner = missileTempoSpawner;
            }
        }

        public void SetActiveMissileTempoSpawner(bool isEnabled)
        {
            if (_missileTempoSpawner != null)
            {
                _missileTempoSpawner.gameObject.SetActive(isEnabled);
                Debug.LogWarning($"_missileTempoSpawner: [{_missileTempoSpawner.gameObject.activeSelf}]");
            }
        }

        public void SetMissilePattern(string newPattern)
        {
            if (_missileTempoSpawner == null)
            {
                Debug.LogWarning("MissileTempoSpawnerがセットされていません。");
                return;
            }

            _missileTempoSpawner.SetMissilePattern(newPattern);
        }

        /// <summary>
        /// マイク入力中か
        /// </summary>
        /// <returns>マイク入力中か</returns>
        /// <remarks>TODO: MicInput_Criware クラス - CheckVolume メソッドにある if (volume > startThreshold) 判定をTrue／Falseで返す、<br/>
        /// パプリックなゲッターのプロパティを MicInput_Criware クラスへ追加する
        /// </remarks>
        /// <see cref="MicInput_Criware.CheckVolume"/>
        public bool IsMicInput()
        {
            return 0f < GetDBLevel();
        }

        public void SetVolumeSliderMaxValueToTwo()
        {
            _volumeSliderMaxValueToTwoDisposable.Disposable = Observable.EveryUpdate()
                .Select(_ => _micInput_Criware)
                .Where(x => x != null && x.volumeSlider != null)
                .Select(x => x.volumeSlider.maxValue == 2f)
                .DistinctUntilChanged()
                .Subscribe(isMaxValueToTwo =>
                {
                    _isVolumeSliderMaxValueToTwo.Execute(isMaxValueToTwo);
                })
                .AddTo(ref _disposableBag);
        }

        /// <see cref="_isVolumeSliderMaxValueToTwo"/>
        public void SetVolumeSliderMaxValue(float minValue, float maxValue, bool wholeNumbers, bool interactable)
        {
            if (_micInput_Criware == null)
                return;

            var volumeSlider = _micInput_Criware.volumeSlider;
            volumeSlider.minValue = minValue;
            volumeSlider.maxValue = maxValue;
            volumeSlider.wholeNumbers = wholeNumbers;
            volumeSlider.interactable = interactable;
        }

        /// <summary>
        /// dBレベルを取得
        /// </summary>
        /// <returns>dBレベル</returns>
        /// <remarks>TODO: MicInput_Criware クラスにある text フィールドにセットしている<br/>
        /// Volの情報を取得可能なゲッタープロパティを MicInput_Criware クラスへ追加する
        /// </remarks>
        /// <see cref="MicInput_Criware.text"/>
        public float GetDBLevel()
        {
            if (_micInput_Criware == null)
                return 0f;

            var volumeSlider = _micInput_Criware.volumeSlider;
            return volumeSlider.value;
        }

        private ReactiveCommand<float> _volumeLevelReactive = new ReactiveCommand<float>();
        public ReactiveCommand<float> VolumeLevelReactive => _volumeLevelReactive;

        /// <see cref="MicInput_Criware.Update"/>
        /// <see cref="MicInput_Criware.CheckVolume"/>
        public void InitVolumeLevelReactive()
        {
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    // Update
                    var fieldInfoIsMicActive = typeof(MicInput_Criware).GetField("isMicActive", BindingFlags.NonPublic | BindingFlags.Instance);
                    var valueIsMicActive = fieldInfoIsMicActive.GetValue(_micInput_Criware);
                    if (valueIsMicActive == null ||
                        !(bool)valueIsMicActive)
                        return;

                    var fieldInfoMic = typeof(MicInput_Criware).GetField("mic", BindingFlags.NonPublic | BindingFlags.Instance);
                    var valueMic = fieldInfoMic.GetValue(_micInput_Criware) as CriAtomExMic;
                    if (valueMic == null)
                        return;

                    // CheckVolume
                    var valueMic1 = fieldInfoMic.GetValue(_micInput_Criware) as CriAtomExMic;
                    if (valueMic1 == null)
                        return;
                    
                    float[] micBuffer = new float[_micInput_Criware.sampleSize];
                    uint samplesRead = valueMic1.ReadData(micBuffer, (uint)_micInput_Criware.sampleSize);
                    if (samplesRead > 0)
                    {
                        MethodInfo methodInfoCalculateRMS = _micInput_Criware.GetType().GetMethod("CalculateRMS", BindingFlags.NonPublic | BindingFlags.Instance);
                        float instantVolume = 0f;
                        if (methodInfoCalculateRMS != null)
                        {
                            object[] parameters = new object[] { micBuffer, (int)samplesRead };
                            instantVolume = (float)methodInfoCalculateRMS.Invoke(_micInput_Criware, parameters);
                        }
                        // マイク感度を適用
                        instantVolume *= _micInput_Criware.microphoneSensitivity;
                        // BGM除去処理
                        if (_micInput_Criware.enableBgmCancellation)
                        {
                            MethodInfo methodInfoApplyBgmCancellation = _micInput_Criware.GetType().GetMethod("ApplyBgmCancellation", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (methodInfoApplyBgmCancellation != null)
                            {
                                object[] parameters = new object[] { instantVolume };
                                instantVolume = (float)methodInfoApplyBgmCancellation.Invoke(_micInput_Criware, parameters);
                            }
                        }
                        // 音量履歴を更新（スライダー表示用）
                        MethodInfo methodInfoUpdateVolumeHistory = _micInput_Criware.GetType().GetMethod("UpdateVolumeHistory", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (methodInfoUpdateVolumeHistory != null)
                        {
                            object[] parameters = new object[] { instantVolume };
                            methodInfoUpdateVolumeHistory.Invoke(_micInput_Criware, parameters);
                        }
                        // 変化検知用履歴を更新
                        MethodInfo methodInfoUpdateChangeDetectionHistory = _micInput_Criware.GetType().GetMethod("UpdateChangeDetectionHistory", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (methodInfoUpdateChangeDetectionHistory != null)
                        {
                            object[] parameters = new object[] { instantVolume };
                            methodInfoUpdateChangeDetectionHistory.Invoke(_micInput_Criware, parameters);
                        }
                        // 平均音量を計算
                        float averagedVolume = 0f;
                        averagedVolume = _micInput_Criware.GetAveragedVolume();

                        float level = 0f;
                        MethodInfo methodInfoGetVolumeDisplayLevel = _micInput_Criware.GetType().GetMethod("GetVolumeDisplayLevel", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (methodInfoGetVolumeDisplayLevel != null)
                        {
                        	object[] parameters = new object[] { averagedVolume };
                        	int tmpLevel = (int)methodInfoGetVolumeDisplayLevel.Invoke(_micInput_Criware, parameters);
                        	if (4 == tmpLevel)
                        	{
                        		// 0スタートの4段階なら最大は3でいい
                        		tmpLevel = 3;
                        	}
                        	level = (float)tmpLevel / 3f;
                        }
                        _volumeLevelReactive.Execute(level);
                    }
                })
                .AddTo(ref _disposableBag);
        }

        public void SetMicrophoneActive(bool active)
        {
            _micInput_Criware.SetMicrophoneActive(active);
        }

        public void ChangeBgmA()
        {
            var conductor = CRIWARE_conductor.Instance;
            if (conductor != null)
            {
                conductor.ChangeBgmA(6);
            }
        }

        /// <see cref="CRIWARE_conductor.InitializeWhenReady"/>
        public void ChangeBgmB()
        {
            var conductor = CRIWARE_conductor.Instance;
            var aisac = CRIWARE_AisacChange.Instance;
            if (conductor != null && aisac != null)
            {
                bool isCompletedIntro = aisac.IsCompletedPlayStart;
                if (!isCompletedIntro)
                {
                    /*
                     * TODO: イントロを即時終了したいために呼び出している処理
                     * この処理が呼ばれるタイミングでは、OnEnable->InitializeWhenReady->yield return new WaitForSecondsRealtime(introDelayTime); まで呼ばれている前提
                     * 上記の後の、DelayBGMLoopStartを呼ばせたくはないので暫定的にコルーチンを止めている
                     * 他の処理との不整合が生じた場合、方針を変更
                     */
                    conductor.StopAllCoroutines();
                    // TODO: デバッグを元にBGMのAのフレームの設定しているため、BPMが変わった場合は修正する
                    conductor.frameRate = 85f;
                    // イントロを停止
                    StopIntro();
                }
                conductor.ChangeBgmB(3);
            }
        }

        /// <see cref="CRIWARE_conductor.InitializeWhenReady"/>
        public void ChangeBgmC()
        {
            var conductor = CRIWARE_conductor.Instance;
            var aisac = CRIWARE_AisacChange.Instance;
            if (conductor != null && aisac != null)
            {
                bool isCompletedIntro = aisac.IsCompletedPlayStart;
                if (!isCompletedIntro)
                {
                    /*
                     * TODO: イントロを即時終了したいために呼び出している処理
                     * この処理が呼ばれるタイミングでは、OnEnable->InitializeWhenReady->yield return new WaitForSecondsRealtime(introDelayTime); まで呼ばれている前提
                     * 上記の後の、DelayBGMLoopStartを呼ばせたくはないので暫定的にコルーチンを止めている
                     * 他の処理との不整合が生じた場合、方針を変更
                     */
                    conductor.StopAllCoroutines();
                    // TODO: デバッグを元にBGMのAのフレームの設定しているため、BPMが変わった場合は修正する
                    conductor.frameRate = 85f;
                    // イントロを停止
                    StopIntro();
                }
                conductor.ChangeBgmC(3);
                _currentSourceStatusDisposable?.Dispose();
                _currentSourceStatusDisposable = Observable.EveryUpdate()
                    .Select(_ => conductor.currentSource.status)
                    .Subscribe(status =>
                    {
                        _bgmCStatus.Execute((int)status);
                    })
                    .AddTo(ref _disposableBag);
            }
        }

        /// <summary>
        /// イントロを停止する
        /// </summary>
        public void StopIntro()
        {
            var longSePicker = LongSePicker.Instance;
            if (longSePicker == null) return;

            var longSePickerType = typeof(LongSePicker);
            var atomExPlayersField = longSePickerType.GetField("atomExPlayers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (atomExPlayersField == null)
            {
                Debug.LogWarning("atomExPlayers フィールドが見つかりませんでした。");
                return;
            }

            var atomExPlayers = atomExPlayersField.GetValue(longSePicker) as List<CriAtomExPlayer>;
            if (atomExPlayers == null) return;

            // すべてのプレイヤーを停止
            foreach (var player in atomExPlayers)
            {
                if (player != null)
                {
                    player.Stop();
                }
            }
        }

        public int JustBeatTick()
        {
            var conductor = CRIWARE_conductor.Instance;
            if (conductor != null)
            {
                return (int)conductor.JustBeatTick();
            }
            Debug.LogWarning($"{nameof(CRIWARE_conductor)}がインスタンスされていません！");

            return -1;
        }

        public void SetLifeBeat(float newLifeBeat)
        {
            if (_homingObject == null || _homingObjectLifeBeatField == null)
            {
                Debug.LogWarning("HomingObjectまたはLifeBeatフィールドがセットされていません。");
                return;
            }

            _homingObjectLifeBeatField.SetValue(_homingObject, newLifeBeat);
        }

        public void SetMissileRendererColor()
        {
            if (_missileDirectAnimManagerB == null)
            {
                Debug.LogWarning("MissileDirectAnimManagerBがセットされていません。");
                return;
            }
            var managerType = _missileDirectAnimManagerB.GetType();
            var missileRendererField = managerType.GetField("missileRenderer", BindingFlags.NonPublic | BindingFlags.Instance);
            if (missileRendererField == null)
            {
                Debug.LogWarning("missileRenderer フィールドが見つかりませんでした。");
                return;
            }
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    try
                    {
                        var missileRenderer = missileRendererField.GetValue(_missileDirectAnimManagerB);
                        if (missileRenderer == null)
                        {
                            Debug.LogWarning("missileRenderer が null です。");
                            return;
                        }
                        Material material = ((Renderer)missileRenderer).material;
                        if (material != null)
                        {
                            Color color = material.color;
                            _missileRendererColor.Execute(color);
                        }
                    }
                    catch (MissingReferenceException e)
                    {
                        Debug.LogWarning($"監視先のオブジェクトがDestroy済みです。[{e.Message}]");
                        return;
                    }
                    catch (System.NullReferenceException e)
                    {
                        Debug.LogWarning($"Renderer情報取得の失敗[{e.Message}]");
                        return;
                    }
                })
                .AddTo(ref _disposableBag);
        }

        public void ProcessClick(float elapsedTime)
        {
            if (_missileDirectAnimManagerB == null)
            {
                return;
            }
            MethodInfo methodInfo = _missileDirectAnimManagerB.GetType().GetMethod("ProcessClick", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo != null)
            {
                // メソッドを実行
                object[] parameters = new object[]
                {
                    elapsedTime,
                };
                methodInfo.Invoke(_missileDirectAnimManagerB, parameters);
            }
        }

        public void HandleLongPressRelease(float elapsedTime)
        {
            if (_missileDirectAnimManagerB == null)
            {
                return;
            }
            // inputManagerを取得してHandleLongPressReleaseを呼び出し
            var inputManagerField = _missileDirectAnimManagerB.GetType().GetField("inputManager", BindingFlags.NonPublic | BindingFlags.Instance);
            if (inputManagerField == null)
            {
                Debug.LogWarning("inputManager フィールドが見つかりませんでした。");
                return;
            }
            var inputManager = inputManagerField.GetValue(_missileDirectAnimManagerB);
            if (inputManager == null)
            {
                Debug.LogWarning("missileRenderer が null です。");
                return;
            }
            MissileInputManager tmpInputManager = (MissileInputManager)inputManager;
            MethodInfo methodInfo = tmpInputManager.GetType().GetMethod("HandleLongPressRelease", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo != null)
            {
                // メソッドを実行
                object[] parameters = new object[]
                {
                    elapsedTime,
                };
                methodInfo.Invoke(_missileDirectAnimManagerB, parameters);
            }
        }

        /// <summary>
        /// UpdateUIPositionにてreturnとなり得る処理のみ抽出
        /// </summary>
        /// <see cref="MissileDirectAnimManagerB.UpdateUIPosition"/>
        public void CheckUpdateUIPosition()
        {
            if (_missileDirectAnimManagerB == null) return;

            var managerType = _missileDirectAnimManagerB.GetType();
            var fieldInfoCamera = managerType.GetField("mainCamera", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfoCamera == null)
            {
                Debug.LogWarning("mainCamera フィールドが見つかりませんでした。");
                return;
            }

            var valueCamera = fieldInfoCamera.GetValue(_missileDirectAnimManagerB);
            Camera mainCamera = (Camera)valueCamera;
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }
            var fieldInfoUI = managerType.GetField("uiManager", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfoUI == null)
            {
                Debug.LogWarning("uiManager フィールドが見つかりませんでした。");
                return;
            }

            var valueUI = fieldInfoUI.GetValue(_missileDirectAnimManagerB);
            MissileUIManager uiManager = (MissileUIManager)valueUI;
            if (uiManager == null) return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(_missileDirectAnimManagerB.transform.position);
            // 画面背後にある場合は非表示
            if (screenPos.z < 0)
            {
                return;
            }
        }

        public void SetActiveObjectCount(int activeObjectCount)
        {
            if (_missileObjectPooler == null)
            {
                Debug.LogWarning("MissileObjectPoolerがセットされていません。");
                return;
            }

            // MissileObjectPoolerクラスの"activeObjectCount"フィールドをリフレクションで取得
            var fieldInfo = typeof(MissileObjectPooler).GetField("activeObjectCount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                Debug.LogWarning("activeObjectCountフィールドが見つかりませんでした。");
                return;
            }

            // フィールドに新しい値を設定
            fieldInfo.SetValue(_missileObjectPooler, activeObjectCount);
        }

        public void ReturnAllMissilesToPool()
        {
            var missileObjectPooler = _missileObjectPooler;
            if (missileObjectPooler == null)
            {
                Debug.LogWarning("MissileObjectPoolerがセットされていません。");
                return;
            }
            // 下記バグありのため代替処理
            //missileObjectPooler.ReturnAllMissilesToPool();
            var missileDirectAnimManagerBs = GameObject.FindObjectsByType<MissileDirectAnimManagerB>(FindObjectsSortMode.None);
            foreach (var missileDirectAnimManagerB in missileDirectAnimManagerBs)
            {
                missileDirectAnimManagerB.gameObject.SetActive(false);
            }
            var missileDirectAnimManagers = GameObject.FindObjectsByType<MissileDirectAnimManager>(FindObjectsSortMode.None);
            foreach (var missileDirectAnimManager in missileDirectAnimManagers)
            {
                missileDirectAnimManager.gameObject.SetActive(false);
            }
        }

        public void SetObjectPoolerXyloOther(Transform transform)
        {
            var objectPoolerXyloOther = transform.GetComponent<ObjectPoolerXyloOther>();
            if (objectPoolerXyloOther != null)
            {
                _objectPoolerXyloOther = objectPoolerXyloOther;
            }
        }

        private Se_3D_Picker _se_3D_Picker;
        
        public void InitializeSe_3D_Picker(Transform transform)
        {
            var se_3D_Picker = transform.GetComponent<Se_3D_Picker>();
            if (se_3D_Picker != null)
            {
                _se_3D_Picker = se_3D_Picker;
            }
        }

        public void PlaySound(string SeName, float volume)
        {
            if (_se_3D_Picker != null)
            {
                _se_3D_Picker.PlaySound(SeName, volume);
            }
        }

        public void SetEnableClickDetection(bool isEnableClickDetection)
        {
            if (_missileDirectAnimManagerB == null)
            {
                return;
            }

            var managerType = _missileDirectAnimManagerB.GetType();
            var enableClickDetectionField = managerType.GetField("enableClickDetection", BindingFlags.NonPublic | BindingFlags.Instance);
            if (enableClickDetectionField == null)
            {
                Debug.LogWarning("enableClickDetection フィールドが見つかりませんでした。");
                return;
            }

            // 値をセットする
            enableClickDetectionField.SetValue(_missileDirectAnimManagerB, isEnableClickDetection);
        }

        /// <summary>
        /// MissileDirectAnimManagerBのContainerObjectを取得
        /// </summary>
        /// <param name="managerType">MissileDirectAnimManagerBのタイプ</param>
        /// <param name="missileDirectAnimManagerB">MissileDirectAnimManagerB</param>
        /// <returns>ContainerObject</returns>
        private GameObject GetContainerObjectInMissileDirectAnimManagerB(MissileDirectAnimManagerB missileDirectAnimManagerB)
        {
            var managerType = missileDirectAnimManagerB.GetType();
            var uiManagerField = managerType.GetField("uiManager", BindingFlags.NonPublic | BindingFlags.Instance);
            if (uiManagerField == null)
            {
                Debug.LogWarning("_uiManager フィールドが見つかりませんでした。");
                return null;
            }
            var uiManager = uiManagerField.GetValue(missileDirectAnimManagerB);
            if (uiManager == null)
            {
                Debug.LogWarning("_uiManager が null です。");
                return null;
            }

            var containerObject = ((MissileUIManager)uiManager).GetUIContainer();
            if (containerObject == null)
            {
                Debug.LogWarning("GetUIContainer の結果が null です。");
                return null;
            }

            return containerObject;
        }

        /// <summary>
        /// MissileDirectAnimManagerBのIsUIActiveを取得
        /// </summary>
        /// <param name="missileDirectAnimManagerB">MissileDirectAnimManagerB</param>
        /// <returns>MissileDirectAnimManagerBのIsUIActive</returns>
        private bool IsUIActiveInMissileDirectAnimManagerB(MissileDirectAnimManagerB missileDirectAnimManagerB)
        {
            var managerType = missileDirectAnimManagerB.GetType();
            var uiManagerField = managerType.GetField("uiManager", BindingFlags.NonPublic | BindingFlags.Instance);
            if (uiManagerField == null)
            {
                Debug.LogWarning("_uiManager フィールドが見つかりませんでした。");
                return false;
            }
            var uiManager = uiManagerField.GetValue(missileDirectAnimManagerB);
            if (uiManager == null)
            {
                Debug.LogWarning("_uiManager が null です。");
                return false;
            }
            var isUIActive = ((MissileUIManager)uiManager).IsUIActive();

            return isUIActive;
        }

        /// <summary>
        /// BGMの一時停止・再開を切り替える
        /// </summary>
        /// <param name="isPause">trueで一時停止、falseで再開</param>
        public void SetBgmPause(bool isPause)
        {
            var conductor = CRIWARE_conductor.Instance;
            if (conductor == null)
            {
                Debug.LogWarning("CRIWARE_conductorのインスタンスが見つかりませんでした。");
                return;
            }

            // 既存コードで currentSource を参照しているのに倣い同じ経路でアクセス
            var source = conductor.currentSource;
            if (source == null)
            {
                Debug.LogWarning("conductor.currentSource が null です。");
                return;
            }

            source.Pause(isPause);
        }

        /// <summary>
        /// シーン内の全ノーツのクリック判定のみを有効・無効にする
        /// </summary>
        /// <remarks>
        /// enabled = false と違いアニメーション・リングの表示は維持される
        /// enableClickDetection フィールドをリフレクションで操作
        /// </remarks>
        public void SetAllNotesClickDetection(bool isEnable)
        {
            var managers = GameObject.FindObjectsByType<MissileDirectAnimManagerB>(FindObjectsSortMode.None);
            if (managers == null || managers.Length == 0)
            {
                Debug.LogWarning("MissileDirectAnimManagerB がシーン内に見つかりませんでした。");
                return;
            }

            var managerType = typeof(MissileDirectAnimManagerB);
            var enableClickDetectionField = managerType.GetField("enableClickDetection", BindingFlags.NonPublic | BindingFlags.Instance);
            if (enableClickDetectionField == null)
            {
                Debug.LogWarning("enableClickDetection フィールドが見つかりませんでした。");
                return;
            }

            foreach (var manager in managers)
            {
                enableClickDetectionField.SetValue(manager, isEnable);
            }
        }

        /// <summary>
        /// アクティブなショートノーツを監視して、クリック可能なタイミングになったらtrueを返す
        /// </summary>
        /// <returns>クリック可能なタイミングのショートノーツが存在するか</returns>
        public bool IsAnyShortNoteClickable()
        {
            var managers = GameObject.FindObjectsByType<MissileDirectAnimManagerB>(FindObjectsSortMode.None);
            if (managers == null || managers.Length == 0) return false;

            var managerType = typeof(MissileDirectAnimManagerB);
            var clickGracePeriodField = managerType.GetField("clickGracePeriod", BindingFlags.NonPublic | BindingFlags.Instance);
            var oneBeatField = managerType.GetField("oneBeat", BindingFlags.NonPublic | BindingFlags.Instance);
            var objectCreationTimeField = managerType.GetField("objectCreationTime", BindingFlags.NonPublic | BindingFlags.Instance);
            var enableClickDetectionField = managerType.GetField("enableClickDetection", BindingFlags.NonPublic | BindingFlags.Instance);
            var isFailedField = managerType.GetField("isFailed", BindingFlags.NonPublic | BindingFlags.Instance);
            var isSuccessfulField = managerType.GetField("isSuccessful", BindingFlags.NonPublic | BindingFlags.Instance);
            var isReturningToPoolField = managerType.GetField("isReturningToPool", BindingFlags.NonPublic | BindingFlags.Instance);
            var isForceReturningField = managerType.GetField("isForceReturning", BindingFlags.NonPublic | BindingFlags.Instance);

            if (clickGracePeriodField == null || oneBeatField == null || objectCreationTimeField == null)
            {
                return false;
            }

            foreach (var manager in managers)
            {
                if (manager == null || !manager.gameObject.activeInHierarchy) continue;

                if (manager.noteType != MissileNoteType.Short) continue;

                if (isFailedField != null && (bool)isFailedField.GetValue(manager)) continue;
                if (isSuccessfulField != null && (bool)isSuccessfulField.GetValue(manager)) continue;
                if (enableClickDetectionField != null && !(bool)enableClickDetectionField.GetValue(manager)) continue;
                if (isReturningToPoolField != null && (bool)isReturningToPoolField.GetValue(manager)) continue;
                if (isForceReturningField != null && (bool)isForceReturningField.GetValue(manager)) continue;

                float clickGracePeriod = (float)clickGracePeriodField.GetValue(manager);
                float oneBeat = (float)oneBeatField.GetValue(manager);
                float objectCreationTime = (float)objectCreationTimeField.GetValue(manager);

                float elapsedTime = Time.time - objectCreationTime;
                float absoluteClickTargetTime = oneBeat * 4;
                float timingDifference = elapsedTime - absoluteClickTargetTime;

                bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

                if (inClickWindow)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// アクティブなロングノーツを監視して、クリック可能なタイミング、または長押し中であればtrueを返す
        /// </summary>
        /// <returns>重なっている（判定有効な）ロングノーツが存在するか</returns>
        public bool IsAnyLongNoteClickable()
        {
            var managers = GameObject.FindObjectsByType<MissileDirectAnimManagerB>(FindObjectsSortMode.None);
            if (managers == null || managers.Length == 0) return false;

            var managerType = typeof(MissileDirectAnimManagerB);
            var clickGracePeriodField = managerType.GetField("clickGracePeriod", BindingFlags.NonPublic | BindingFlags.Instance);
            var oneBeatField = managerType.GetField("oneBeat", BindingFlags.NonPublic | BindingFlags.Instance);
            var objectCreationTimeField = managerType.GetField("objectCreationTime", BindingFlags.NonPublic | BindingFlags.Instance);
            var enableClickDetectionField = managerType.GetField("enableClickDetection", BindingFlags.NonPublic | BindingFlags.Instance);
            var isFailedField = managerType.GetField("isFailed", BindingFlags.NonPublic | BindingFlags.Instance);
            var isSuccessfulField = managerType.GetField("isSuccessful", BindingFlags.NonPublic | BindingFlags.Instance);
            var isReturningToPoolField = managerType.GetField("isReturningToPool", BindingFlags.NonPublic | BindingFlags.Instance);
            var isForceReturningField = managerType.GetField("isForceReturning", BindingFlags.NonPublic | BindingFlags.Instance);
            var inputManagerField = managerType.GetField("inputManager", BindingFlags.NonPublic | BindingFlags.Instance);
            var micInputManagerField = managerType.GetField("micInputManager", BindingFlags.NonPublic | BindingFlags.Instance);

            if (clickGracePeriodField == null || oneBeatField == null || objectCreationTimeField == null)
            {
                return false;
            }

            foreach (var manager in managers)
            {
                if (manager == null || !manager.gameObject.activeInHierarchy) continue;

                if (manager.noteType != MissileNoteType.Long1Beat &&
                    manager.noteType != MissileNoteType.Long2Beat &&
                    manager.noteType != MissileNoteType.Long3Beat &&
                    manager.noteType != MissileNoteType.Long2Beat_Mic) continue;

                if (isFailedField != null && (bool)isFailedField.GetValue(manager)) continue;
                if (isSuccessfulField != null && (bool)isSuccessfulField.GetValue(manager)) continue;
                if (enableClickDetectionField != null && !(bool)enableClickDetectionField.GetValue(manager)) continue;
                if (isReturningToPoolField != null && (bool)isReturningToPoolField.GetValue(manager)) continue;
                if (isForceReturningField != null && (bool)isForceReturningField.GetValue(manager)) continue;

                float clickGracePeriod = (float)clickGracePeriodField.GetValue(manager);
                float oneBeat = (float)oneBeatField.GetValue(manager);
                float objectCreationTime = (float)objectCreationTimeField.GetValue(manager);

                float elapsedTime = Time.time - objectCreationTime;
                float absoluteClickTargetTime = oneBeat * 4;
                float timingDifference = elapsedTime - absoluteClickTargetTime;

                bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

                if (inClickWindow)
                {
                    return true;
                }

                if (manager.noteType == MissileNoteType.Long2Beat_Mic)
                {
                    if (micInputManagerField != null)
                    {
                        var micInputManager = micInputManagerField.GetValue(manager);
                        if (micInputManager != null)
                        {
                            var methodInfo = micInputManager.GetType().GetMethod("IsLongPressStarted", BindingFlags.Public | BindingFlags.Instance);
                            if (methodInfo != null)
                            {
                                bool isLongPressStarted = (bool)methodInfo.Invoke(micInputManager, null);
                                if (isLongPressStarted)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (inputManagerField != null)
                    {
                        var inputManager = inputManagerField.GetValue(manager);
                        if (inputManager != null)
                        {
                            var methodInfo = inputManager.GetType().GetMethod("IsLongPressStarted", BindingFlags.Public | BindingFlags.Instance);
                            if (methodInfo != null)
                            {
                                bool isLongPressStarted = (bool)methodInfo.Invoke(inputManager, null);
                                if (isLongPressStarted)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 判定可能状態のノーツに対して強制的にGOOD判定（クリック）を行う
        /// </summary>
        public void ForceClickAnyClickableNote()
        {
            var managers = GameObject.FindObjectsByType<MissileDirectAnimManagerB>(FindObjectsSortMode.None);
            if (managers == null || managers.Length == 0) return;

            var managerType = typeof(MissileDirectAnimManagerB);
            var clickGracePeriodField = managerType.GetField("clickGracePeriod", BindingFlags.NonPublic | BindingFlags.Instance);
            var oneBeatField = managerType.GetField("oneBeat", BindingFlags.NonPublic | BindingFlags.Instance);
            var objectCreationTimeField = managerType.GetField("objectCreationTime", BindingFlags.NonPublic | BindingFlags.Instance);
            var enableClickDetectionField = managerType.GetField("enableClickDetection", BindingFlags.NonPublic | BindingFlags.Instance);
            var isFailedField = managerType.GetField("isFailed", BindingFlags.NonPublic | BindingFlags.Instance);
            var isSuccessfulField = managerType.GetField("isSuccessful", BindingFlags.NonPublic | BindingFlags.Instance);
            var isReturningToPoolField = managerType.GetField("isReturningToPool", BindingFlags.NonPublic | BindingFlags.Instance);
            var isForceReturningField = managerType.GetField("isForceReturning", BindingFlags.NonPublic | BindingFlags.Instance);

            if (clickGracePeriodField == null || oneBeatField == null || objectCreationTimeField == null) return;

            foreach (var manager in managers)
            {
                if (manager == null || !manager.gameObject.activeInHierarchy) continue;

                if (isFailedField != null && (bool)isFailedField.GetValue(manager)) continue;
                if (isSuccessfulField != null && (bool)isSuccessfulField.GetValue(manager)) continue;
                if (enableClickDetectionField != null && !(bool)enableClickDetectionField.GetValue(manager)) continue;
                if (isReturningToPoolField != null && (bool)isReturningToPoolField.GetValue(manager)) continue;
                if (isForceReturningField != null && (bool)isForceReturningField.GetValue(manager)) continue;

                float clickGracePeriod = (float)clickGracePeriodField.GetValue(manager);
                float oneBeat = (float)oneBeatField.GetValue(manager);
                float objectCreationTime = (float)objectCreationTimeField.GetValue(manager);

                float elapsedTime = Time.time - objectCreationTime;
                float absoluteClickTargetTime = oneBeat * 4;
                float timingDifference = elapsedTime - absoluteClickTargetTime;

                bool inClickWindow = Mathf.Abs(timingDifference) <= clickGracePeriod;

                if (inClickWindow)
                {
                    MethodInfo methodInfo = managerType.GetMethod("ProcessClick", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(manager, new object[] { elapsedTime });
                        return; // 1つ処理したら終了
                    }
                }
            }
        }

        /// <summary>
        /// 攻撃に向かってきているオバケをすべて消去する
        /// </summary>
        public void ClearAllAttackingGhosts()
        {
            if (_objectPoolerXyloOther != null)
            {
                var view = _objectPoolerXyloOther.GetComponent<Mains.Views.ObjectPoolerXyloOtherCustomizeView>();
                if (view != null)
                {
                    view.DoAllDisabled();
                }
            }
        }

        /// <summary>
        /// ターゲットクロスと直近ノーツのスクリーン距離を取得する
        /// </summary>
        public float GetNoteToCrosshairScreenDistance()
        {
            if (NoteTransform == null) return float.MaxValue;
            
            Camera cam = Camera.main;
            if (cam == null) return float.MaxValue;

            // ターゲットクロスは画面中央と仮定
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            // NoteTransformがRectTransformである前提でスクリーン座標を取得
            Vector2 noteScreenPos = RectTransformUtility.WorldToScreenPoint(null, NoteTransform.position); // Overlay Canvasならcamはnull
            
            // Camera.mainを用いてWorldToScreenPointを試行するフォールバック
            if (noteScreenPos == Vector2.zero && NoteTransform.position != Vector3.zero)
            {
                noteScreenPos = RectTransformUtility.WorldToScreenPoint(cam, NoteTransform.position);
            }

            return Vector2.Distance(screenCenter, noteScreenPos);
        }

        public void Dispose()
        {
            _disposableBag.Dispose();
            CRIWARE_conductor.TempoMethodEvent1 -= OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent2 -= OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent3 -= OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent4 -= OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent5 -= OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent6 -= OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent7 -= OnTempoMethodEventAny;
            CRIWARE_conductor.TempoMethodEvent8 -= OnTempoMethodEventAny;
            _volumeSliderMaxValueToTwoDisposable.Disposable = null;
        }
    }
}
