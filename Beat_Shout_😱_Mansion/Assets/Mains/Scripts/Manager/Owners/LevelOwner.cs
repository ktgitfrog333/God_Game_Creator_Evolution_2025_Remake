using Mains.Commons;
using System.Linq;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Mains.Manager.Owners
{
    /// <summary>
    /// レベルオーナー
    /// </summary>
    public class LevelOwner : MonoBehaviour
    {
        /// <summary>レベルの親オブジェクト</summary>
        private Transform _level;
        /// <summary>インスタンス済みレベル</summary>
        private Transform _instancedLevel;
        /// <summary>インスタンス済みレベル</summary>
        public Transform InstancedLevel => _instancedLevel;
        [SerializeField] private LevelStruct[] レベル構造体リスト;
        [SerializeField] private bool ステージを動的に生成する;

        void Start()
        {
            if (!ステージを動的に生成する)
                return;

            _level = GameObject.Find("Level").transform;
            var temp = new ResourcesUtility();
            var userBean = temp.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA);
            var stage = レベル構造体リスト.FirstOrDefault(q => q.階層 == userBean.sceneIdx).Stage_xと書かれたプレハブ;
            if (stage == null)
                throw new System.ArgumentNullException($"条件に一致するステージインデックス [{userBean.sceneIdx}] が見つかりませんでした。");

            _instancedLevel = Instantiate(stage.transform, Vector3.zero, Quaternion.identity, _level).transform;
        }
    }
}
