using Movies.Commons;
using Selects.Views;
using UnityEngine;
using UnityEngine.SceneManagement;
using Universal.Commons;
using Universal.Utilities;

namespace Movies.Views
{
    /// <summary>
    /// イントロムービーシーンのカスタマイズビュー
    /// </summary>
    public class IntroMovieSceneCustomizeView : MonoBehaviour
    {
        /// <summary>イントロムービーシーンのカスタマイズ設定</summary>
        [SerializeField] private IntroMovieSceneCustomizeSettings settings;

        private void Start()
        {
            var set = settings;
            var table = set.introMovieSceneCustomizeTable;
            CheckContinueAndLoadScene(table.gameSceneName);
        }

        /// <summary>
        /// セーブデータが続きからの場合かどうかのチェック及びシーンロード
        /// </summary>
        /// <param name="gameSceneName">遷移先シーン名</param>
        /// <remarks>セーブデータが続きからの場合：セレクトシーンへ遷移<br/>
        /// 初期データの場合：ムービーシーンのデモ再生を続ける</remarks>
        private void CheckContinueAndLoadScene(string gameSceneName)
        {
            ResourcesUtility utility = new ResourcesUtility();
            var userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            // チュートリアルの基本操作が完了していない＝初期データとみなす
            if (!TutorialConditionEvaluator.ShouldRunMove(userBean))
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }

    /// <summary>
    /// イントロムービーシーンのカスタマイズ設定
    /// </summary>
    [System.Serializable]
    public class IntroMovieSceneCustomizeSettings
    {
        /// <summary>イントロムービーシーンのカスタマイズテーブル</summary>
        public IntroMovieSceneCustomizeTable introMovieSceneCustomizeTable;
    }
}
