using UnityEngine;

namespace Universal.Commons
{
    /// <summary>
    /// リソースフォルダ内のファイル名
    /// </summary>
    public class ConstResorcesNames
    {
        /// <summary>ホームディレクトリ（UnityEditor用）</summary>
        public static readonly string HOMEPATH_UNITYEDITOR = @".\Assets\SaveDatas\";
        /// <summary>ホームディレクトリ（ビルド用）</summary>
        public static readonly string HOMEPATH_BUILD = @".\SaveDatas\";
        /// <summary>ユーザーデータ</summary>
        public static readonly string USER_DATA = "UserData";
    }
}
