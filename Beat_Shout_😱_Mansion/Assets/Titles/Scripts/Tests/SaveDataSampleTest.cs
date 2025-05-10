using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Titles.Tests
{
    /// <summary>
    /// セーブデータ管理（ビートシャウトマンション）
    /// </summary>
    /// <see cref="https://www.notion.so/1e2c82111c0580a18b55c861d4bb0b44?pvs=4"/>
    public class SaveDataSampleTest : MonoBehaviour
    {
        [SerializeField] private UserBean 入出力コンテンツ;

        private void OnGUI()
        {
            var temp = new ResourcesUtility();

            const float height = 50;
            const float y = 10;

            // ロードボタンを追加
            Rect buttonRect = new Rect(10, y, 250, height);
            if (GUI.Button(buttonRect, "ロード"))
            {
                入出力コンテンツ = temp.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            }
            // セーブボタンを追加
            Rect buttonRect1 = new Rect(10, y + height * 1, 250, height);
            if (GUI.Button(buttonRect1, "セーブ"))
            {
                temp.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, 入出力コンテンツ);
            }
            // レベル初期化ボタンを追加
            Rect buttonRect2 = new Rect(10, y + height * 2, 250, height);
            if (GUI.Button(buttonRect2, "レベル初期化"))
            {
                入出力コンテンツ = temp.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, EnumLoadMode.Default);
                temp.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, 入出力コンテンツ);
            }
            // オプション初期化ボタンを追加
            Rect buttonRect3 = new Rect(10, y + height * 3, 250, height);
            if (GUI.Button(buttonRect3, "オプション初期化"))
            {
                入出力コンテンツ = temp.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, EnumLoadMode.Default1);
                temp.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, 入出力コンテンツ);
            }
            // 全開放ボタンを追加
            Rect buttonRect4 = new Rect(10, y + height * 4, 250, height);
            if (GUI.Button(buttonRect4, "全開放"))
            {
                入出力コンテンツ = temp.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, EnumLoadMode.All);
                temp.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, 入出力コンテンツ);
            }
        }
    }
}
