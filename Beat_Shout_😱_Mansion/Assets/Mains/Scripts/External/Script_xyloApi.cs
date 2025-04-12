using System.Reflection;
using UnityEngine;
using System.Text.RegularExpressions;
using R3;

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
    }
}
