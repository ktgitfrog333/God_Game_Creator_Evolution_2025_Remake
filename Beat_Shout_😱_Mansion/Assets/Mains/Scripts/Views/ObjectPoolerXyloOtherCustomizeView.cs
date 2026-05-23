using Mains.External;
using System.Collections.Generic;
using UnityEngine;
using R3;
using System.Collections;

namespace Mains.Views
{
    /// <summary>
    /// ObjectPoolerXyloOtherのカスタマイズビュー
    /// </summary>
    public class ObjectPoolerXyloOtherCustomizeView : MonoBehaviour
    {
        private Script_xyloApi _script_XyloApi;
        private System.IDisposable _allDisabledDisposable;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Start()
        {
            _script_XyloApi = new Script_xyloApi();
            Transform trans = transform;
            _script_XyloApi.SetObjectPoolerXyloOther(trans);
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
        }

        /// <summary>
        /// MissGhostAttackコンポーネントのオブジェクトを無効にする処理
        /// </summary>
        /// <param name="observer">オブザーバー</param>
        /// <returns>コルーチン</returns>
        public IEnumerator AllDisabled(Observer<bool> observer)
        {
            float elapsed = 0f;

            while (elapsed < 3f)
            {
                var missGhostAttack = FindAnyObjectByType<MissGhostAttackCustomizeView>();

                if (missGhostAttack != null)
                {
                    if (missGhostAttack.gameObject.activeSelf)
                    {
                        missGhostAttack.gameObject.SetActive(false);
                    }

                    elapsed = 0f;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            observer.OnNext(true);
            observer.OnCompleted();
        }

        /// <summary>
        /// MissGhostAttackコンポーネントのオブジェクトを無効にする処理を呼び出す
        /// </summary>
        /// <returns>オブザーバー</returns>
        public Observable<bool> DoAllDisabled()
        {
            return Observable.Create<bool>(observer =>
            {
                StartCoroutine(AllDisabled(observer));

                return Disposable.Empty;
            });
        }
    }
}
