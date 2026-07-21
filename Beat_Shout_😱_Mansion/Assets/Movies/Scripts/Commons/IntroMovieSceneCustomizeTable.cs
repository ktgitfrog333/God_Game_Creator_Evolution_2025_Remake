using UnityEngine;

namespace Movies.Commons
{
    /// <summary>
    /// イントロムービーシーンのカスタマイズテーブル
    /// </summary>
    [CreateAssetMenu(fileName = "IntroMovieSceneCustomizeTable", menuName = "Scriptable Objects/IntroMovieSceneCustomizeTable")]
    public class IntroMovieSceneCustomizeTable : ScriptableObject
    {
        /// <summary>遷移先シーン名</summary>
        public string gameSceneName = "SelectScene";
    }
}
