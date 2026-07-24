using Mains.External;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Mains.Tests
{
    /// <summary>
    /// プールのデバッガ
    /// </summary>
    public class PoolDictionaryDbugger : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dicText;

        private Script_xyloApi _api;

        private void Start()
        {
            _api = new Script_xyloApi();
        }

        private void Update()
        {
            RenderDebugReport();
        }

        private void OnDestroy()
        {
            _api?.Dispose();
        }

        private void RenderDebugReport()
        {
            var poolDic = _api.PoolDictionary;
            List<string> msgs = new List<string>();
            foreach (var item in poolDic)
            {
                var key = item.Key;
                var cnt = item.Value.Count;
                var msg = $"[{key}]_[{cnt}]";
                msgs.Add(msg);
            }
            dicText.text = string.Join("\\n", msgs);
        }
    }
}
