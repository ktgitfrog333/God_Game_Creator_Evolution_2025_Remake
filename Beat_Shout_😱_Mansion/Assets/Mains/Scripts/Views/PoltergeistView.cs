using CriWare;
using DG.Tweening;
using UnityEngine;
using Mains.Commons;
using Mains.ViewModels;

namespace Mains.Views
{
    /// <summary>
    /// ポルターガイストのビュー
    /// </summary>
    public class PoltergeistView : MonoBehaviour
    {
        [Tooltip("Assets/Mains/Scripts/Commons/PoltergeistTable.assetをセットしておく。")]
        [SerializeField] private PoltergeistTable poltergeistTable;
        [Tooltip("Assets/Mains/Prefabs/Level/Poltergeist.prefabをセットしておく。")]
        [SerializeField] private GameObject poltergeistPrefab;
        [Tooltip("Assets/Mains/Prefabs/Level/ShoutChanceRange.prefabをセットしておく。")]
        [SerializeField] private GameObject shoutChanceRangePrefab;
        // TODO:シャウトチャンスパートでポルターガイストが発生した時のエフェクト
        [SerializeField] private GameObject dustParticlePrefab;
        // TODO:SE/MEの素材をUnityに導入して、調整する
        private CriAtomSource _sePoltergeist;
        /// <summary>生成されたPoltergeist.prefabのキャッシュ</summary>
        private GameObject _poltergeistInstance;
        // TODO:シャウトチャンスパートでポルターガイストが発生した時のエフェクト
        private GameObject _dustParticleInstance;
        private Transform _transform;
        /// <summary>ポルターガイストのビューモデル</summary>
        private PoltergeistViewModel _poltergeistViewModel;

        void Start()
        {
            _transform = transform;
            // TODO:SE/MEの素材をUnityに導入して、調整する
            // _sePoltergeist = 
            _poltergeistViewModel = new PoltergeistViewModel(poltergeistTable);
            InstantiatePoltergeist();
        }

        /// <summary>
        /// ポルターガイストのプレハブを生成
        /// </summary>
        /// <remarks>
        /// ●Poltergeist：ポルターガイスト<br/>
        /// ●ShoutChanceRange：シャウトチャンスレンジ
        /// </remarks>
        private void InstantiatePoltergeist()
        {
            // Poltergeistの生成
            var originParent = _transform.parent;
            _poltergeistInstance = Instantiate(poltergeistPrefab, _transform.position, Quaternion.identity);
            _poltergeistInstance.transform.SetParent(originParent);
            _transform.SetParent(_poltergeistInstance.transform);
            RuntimeAnimatorController currentAnimCtrl = poltergeistTable.poltergeistAnimatorControllers[Random.Range(0, poltergeistTable.poltergeistAnimatorControllers.Length)];
            _poltergeistInstance.GetComponent<Animator>().runtimeAnimatorController = currentAnimCtrl;

            // ShoutChanceRangeの生成
            var originParent_1 = _poltergeistInstance.transform.parent;
            Transform shoutChanceInstance = Instantiate(shoutChanceRangePrefab, transform.position, Quaternion.identity).transform;
            shoutChanceInstance.SetParent(originParent_1);
            _poltergeistInstance.transform.SetParent(shoutChanceInstance);
            _transform.SetParent(_poltergeistInstance.transform);
        }

        /// <summary>
        /// AnimarionClipからトリガー（アクション）の受信
        /// TODO:アニメーションクリップを作成
        /// </summary>
        /// <see cref=""/>
        public void OnAction()
        {
            if (_poltergeistInstance != null)
            {
                ApplyPoltergeistEffect(_poltergeistInstance);
                PlayDustParticle(_dustParticleInstance);
                PlayPoltergeistSE(_sePoltergeist);
                _poltergeistViewModel.SetIsOnActionPoltergeist(true);
            }
        }

        /// <summary>
        /// ポルターガイストの挙動
        /// </summary>
        /// <param name="target">ポルターガイスト対象のオブジェクト</param>
        private void ApplyPoltergeistEffect(GameObject target)
        {
            float randomAngle = Random.Range(-15f, 15f);
            float randomImpact = Random.Range(0.1f, 0.3f);
            target.transform.DOLocalMoveY(randomImpact, 0.2f).SetLoops(2, LoopType.Yoyo);
            target.transform.DOLocalRotate(new Vector3(randomAngle, 0, randomAngle), 0.2f).SetLoops(2, LoopType.Yoyo);
        }

        /// <summary>
        /// 砂埃パーティクルの再生
        /// </summary>
        /// <param name="dustParticleInstance">対象のパーティクルオブジェクト</param>
        private void PlayDustParticle(GameObject dustParticleInstance)
        {
            if (dustParticleInstance == null)
            {
                dustParticleInstance = Instantiate(dustParticlePrefab, transform.position, Quaternion.identity);
                dustParticleInstance.transform.SetParent(_poltergeistInstance.transform);
            }
            else
            {
                dustParticleInstance.SetActive(false);
                dustParticleInstance.SetActive(true);
            }
        }

        /// <summary>
        /// ポルターガイストのSE再生
        /// </summary>
        /// <param name="sePoltergeist">対象のオーディオソース</param>
        private void PlayPoltergeistSE(CriAtomSource sePoltergeist)
        {
            if (sePoltergeist != null)
            {
                sePoltergeist.Play();
            }
        }
    }
}
