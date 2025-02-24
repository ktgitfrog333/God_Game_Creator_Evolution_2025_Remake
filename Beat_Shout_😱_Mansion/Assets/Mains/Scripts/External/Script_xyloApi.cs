using System.Reflection;
using UnityEngine;
using Mains.Script_xylo;

namespace Mains.External
{
    /// <summary>
    /// シロさんのコンポーネントへアクセスするAPI
    /// </summary>
    public class Script_xyloApi
    {
        private Test_FootStep _test_FootStep;

        public Script_xyloApi()
        {
            Test_FootStep test_FootStep = Component.FindAnyObjectByType<Test_FootStep>();
            if (test_FootStep != null)
            {
                _test_FootStep = test_FootStep;
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
    }
}
