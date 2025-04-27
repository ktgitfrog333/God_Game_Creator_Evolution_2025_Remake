using System.Reflection;
using UnityEngine;
using System.Text.RegularExpressions;
using R3;
using System.Collections.Generic;
using System.Linq;

namespace Mains.External
{
    /// <summary>
    /// シロさんのコンポーネントへアクセスするAPI
    /// </summary>
    public class Script_xyloApi
    {
        private Test_FootStep _test_FootStep;
        private MicInput_Criware _micInput_Criware;
        private readonly ReactiveCommand<float> _frameRate = new ReactiveCommand<float>();
        public ReactiveCommand<float> FrameRate => _frameRate;
        private MissileDirectAnimManagerB _missileDirectAnimManagerB;
        private readonly ReactiveCommand<bool> _isSuccessful = new ReactiveCommand<bool>();
        public ReactiveCommand<bool> IsSuccessful => _isSuccessful;
        private readonly ReactiveCommand<bool> _isFailed = new ReactiveCommand<bool>();
        public ReactiveCommand<bool> IsFailed => _isFailed;
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
                    _frameRate.Execute(frameRate.Current);
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
            }
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
            return .2f < GetDBLevel();
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

            // text フィールドの取得（MicInput_Criwareの public TextMeshProUGUI text）
            var textField = _micInput_Criware.GetType().GetField("text", BindingFlags.Public | BindingFlags.Instance);
            if (textField == null)
                return 0f;

            var textMesh = textField.GetValue(_micInput_Criware) as TMPro.TextMeshProUGUI;
            if (textMesh == null || string.IsNullOrEmpty(textMesh.text))
                return 0f;

            // 正規表現で "Vol: 数値" を抽出
            var match = Regex.Match(textMesh.text, @"Vol:\s*([0-9.]+).*");
            if (match.Success && float.TryParse(match.Groups[1].Value, out float volume))
            {
                return volume;
            }

            return 0f;
        }

        public void ChangeBgmB()
        {
            var conductor = CRIWARE_conductor.Instance;
            if (conductor != null)
            {
                conductor.ChangeBgmB(3);
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
                })
                .AddTo(ref _disposableBag);
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
    }
}
