using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mains.Views
{
    /// <summary>
    /// オブジェクトプールビュー
    /// </summary>
    public class ObjectsPoolView : MonoBehaviour
    {
        /// <summary>トランスフォーム</summary>
        private Transform _transform;
        /// <summary>トランスフォーム</summary>
        public Transform Transform => _transform != null ? _transform : _transform = transform;
        // TODO:トランスフォームでなくカスタムのスクリプトコンポーネントが存在するならそのデータ型に変更する
        [SerializeField] private Transform _３D空間で発音するprefab;
        // TODO:トランスフォームでなくカスタムのスクリプトコンポーネントが存在するならそのデータ型に変更する
        /// <summary>３D空間で発音するスクリプトコンポーネント配列</summary>
        private List<Transform> _3DSoundPlayers = new();

        #region 実装例 ①プレハブの宣言 ②カスタムコンポーネントのリストの宣言
        //[Tooltip("魔力弾のプレハブ")]
        //[SerializeField] private Transform onmyoBulletPrefab;
        ///// <summary>魔力弾配列</summary>
        //private List<OnmyoBulletModel> _onmyoBulletModels = new List<OnmyoBulletModel>();
        #endregion

        /// <summary>
        /// ３D空間で発音するクローンを取得
        /// </summary>
        /// <returns>３D空間で発音するクローン</returns>
        /// <exception cref="System.NotImplementedException">TODO:トランスフォームでなくカスタムのスクリプトコンポーネントが存在するならそのデータ型に変更する</exception>
        public Transform Get3DSoundPlayer()
        {
            throw new System.NotImplementedException("TODO:トランスフォームでなくカスタムのスクリプトコンポーネントが存在するならそのデータ型に変更する");
            //return GetInactiveComponent(_3DSoundPlayers, _３D空間で発音するprefab, Transform);
        }

        #region 実装例 ③非アクティブなコンポーネントを取得
        //public OnmyoBulletModel GetOnmyoBulletModel()
        //{
        //    return GetInactiveComponent(_onmyoBulletModels, onmyoBulletPrefab, _transform);
        //}
        #endregion

        /// <summary>
        /// 非アクティブなコンポーネントを取得
        /// </summary>
        /// <typeparam name="T">カスタムコンポーネントのType</typeparam>
        /// <param name="components">カスタムコンポーネントのリスト</param>
        /// <param name="prefab">プレハブ</param>
        /// <param name="parent">親対象のトランスフォーム</param>
        /// <returns>非アクティブなコンポーネント</returns>
        /// <remarks>プール対象のクローンは役割を終えたら非アクティブになっている前提<br/>
        /// カスタムコンポーネントのリストから未使用のものを取得<br/>
        /// もし取得出来なかったらプレハブからクローンしてプールしておく</remarks>
        private T GetInactiveComponent<T>(List<T> components, Transform prefab, Transform parent) where T : MonoBehaviour
        {
            var inactiveComponents = components.Where(q => !q.isActiveAndEnabled).ToArray();
            if (inactiveComponents.Length < 1)
            {
                var newComponent = GetClone(prefab, parent).GetComponent<T>();
                components.Add(newComponent);

                return newComponent;
            }
            else
            {
                return inactiveComponents[0];
            }
        }

        /// <summary>
        /// プール内のクローンオブジェクトを取得
        /// </summary>
        /// <param name="cloneObject">プレハブ</param>
        /// <param name="parent">親</param>
        /// <returns>クローンオブジェクト</returns>
        private Transform GetClone(Transform cloneObject, Transform parent)
        {
            return Instantiate(cloneObject, parent);
        }
    }
}
