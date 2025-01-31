using System.IO;
using System.Text;
using UnityEngine;
using Universal.Commons;

namespace Universal.Utilities
{
    /// <summary>
    /// リソースアクセスユーティリティ
    /// </summary>
    public class ResourcesUtility
    {
        /// <summary>
        /// JSONファイルの拡張子
        /// </summary>
        private readonly string EXTENSION_JSON = ".json";
        /// <summary>
        /// エンコーディング
        /// </summary>
        private readonly string ENCODING = "UTF-8";

        public ResourcesUtility()
        {
            // リソース管理ディレクトリが存在しない場合は作成
            if (!Directory.Exists(GetHomePath()))
            {
                Directory.CreateDirectory(GetHomePath());
            }
            if (!File.Exists($"{GetHomePath()}{ConstResorcesNames.USER_DATA}{EXTENSION_JSON}"))
            {
                using (File.Create($"{GetHomePath()}{ConstResorcesNames.USER_DATA}{EXTENSION_JSON}")) { }
                if (!SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, new UserBean(EnumLoadMode.Default)))
                    Debug.LogError("ユーザデータをJSONファイルへ保存の失敗");
            }
        }

        /// <summary>
        /// ホームディレクトリを取得
        /// </summary>
        /// <returns>ホームディレクトリ</returns>
        private string GetHomePath()
        {
            var path = "";
#if UNITY_EDITOR
            path = ConstResorcesNames.HOMEPATH_UNITYEDITOR;
#elif UNITY_STANDALONE
                path = ConstResorcesNames.HOMEPATH_BUILD;
#endif
            return path;
        }

        /// <summary>
        /// JSONデータからユーザー情報を取得します
        /// </summary>
        /// <param name="resourcesLoadName">リソースJSONファイル名</param>
        /// <returns>ユーザー情報</returns>
        public UserBean LoadSaveDatasJsonOfUserBean(string resourcesLoadName, EnumLoadMode enumLoadMode = EnumLoadMode.Continue)
        {
            try
            {
                var path = GetHomePath();
                switch (enumLoadMode)
                {
                    case EnumLoadMode.Continue:
                        // 設定内容を保存
                        using (var sr = new StreamReader($"{path}{resourcesLoadName}{EXTENSION_JSON}", Encoding.GetEncoding(ENCODING)))
                            return new UserBean(JsonUtility.FromJson<UserBean>(sr.ReadToEnd()));
                    case EnumLoadMode.Default:
                        return new UserBean(enumLoadMode);
                    case EnumLoadMode.All:
                        return new UserBean(enumLoadMode);
                    default:
                        throw new System.Exception("例外エラー");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        /// <summary>
        /// ユーザデータをJSONファイルへ保存
        /// </summary>
        /// <param name="resourcesLoadName">リソースCSVファイル名</param>
        /// <param name="UserBean">ユーザー情報を保持するクラス</param>
        /// <returns>成功／失敗</returns>
        public bool SaveDatasJsonOfUserBean(string resourcesLoadName, UserBean userBean)
        {
            try
            {
                var path = GetHomePath();
                // 設定内容を保存
                using (var sw = new StreamWriter($"{path}{resourcesLoadName}{EXTENSION_JSON}", false, Encoding.GetEncoding(ENCODING)))
                {
                    var json = JsonUtility.ToJson(userBean);
                    sw.WriteLine(json);
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
    }
}
