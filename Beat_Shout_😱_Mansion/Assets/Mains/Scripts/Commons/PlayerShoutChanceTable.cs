using Rewired;
using System.Linq;
using UnityEngine;

namespace Mains.Commons
{
    /// <summary>
    /// シャウトチャンスパートの共通パラメータ管理用テーブル
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerShoutChanceTable", menuName = "Scriptable Objects/PlayerShoutChanceTable")]
    public class PlayerShoutChanceTable : ScriptableObject
    {
        public float シャウト達成デシベル;
        public float シャウトゲージスライダー最大値;
        public float 恐怖値のカウント停止時間;
        public float マイク手動入力値;
        public float マイク手動入力解放時間;
        public float マイク自動入力間隔;
        /// <summary>シャウトチャンスパートで有効なRewairedコントローラーマップカテゴリ</summary>
        [SerializeField] private string[] enabledMapsInCategories;

        /// <summary>
        /// マイク入力用のボタンが有効か
        /// </summary>
        /// <param name="player">Rewired</param>
        /// <returns>マイク入力用のボタンが有効か</returns>
        public bool IsMicButtonActive(Player player)
        {
            bool isDefaultMapEnabled = false;
            foreach (var enabledMapsInCategory in enabledMapsInCategories)
            {
                isDefaultMapEnabled = player.controllers.maps.GetAllMapsInCategory(enabledMapsInCategory)
                        .Any(m => m.enabled);

                if (isDefaultMapEnabled)
                    break;
            }

            return isDefaultMapEnabled;
        }
    }
}
