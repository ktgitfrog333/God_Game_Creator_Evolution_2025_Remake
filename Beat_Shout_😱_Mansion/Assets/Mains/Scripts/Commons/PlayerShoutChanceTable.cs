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
    }
}
