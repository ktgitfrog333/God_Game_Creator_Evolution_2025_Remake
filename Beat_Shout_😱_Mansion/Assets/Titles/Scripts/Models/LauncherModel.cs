using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Titles.Models
{
    /// <summary>
    /// ランチャーモデル
    /// </summary>
    [CreateAssetMenu(fileName = "LauncherModel", menuName = "Scriptable Objects/LauncherModel")]
    public class LauncherModel : ScriptableObject, System.IDisposable
    {
        /// <summary>初期化済みフラグ</summary>
        private bool _isInitialized;
        /// <summary>メッセージリスト</summary>
        private ReactiveProperty<List<string>> _messages;
        /// <summary>メッセージリスト</summary>
        public ReadOnlyReactiveProperty<List<string>> Messages => _messages;
        /// <summary>管理者モード有効フラグ</summary>
        private ReactiveProperty<bool> _isActiveAdminMode;
        /// <summary>管理者モード有効フラグ</summary>
        public ReadOnlyReactiveProperty<bool> IsActiveAdminMode => _isActiveAdminMode;

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _messages = new ReactiveProperty<List<string>>(new List<string>());
            _isActiveAdminMode = new ReactiveProperty<bool>();
        }

        /// <summary>
        /// メッセージリストへメッセージを追加
        /// </summary>
        /// <param name="message">メッセージ</param>
        public void AddMessages(string message)
        {
            List<string> tmpMessages = new List<string>(_messages.Value);
            tmpMessages.Insert(0, message);

            _messages.Value = tmpMessages;
        }

        public void SetIsActiveAdminMode(bool isActiveAdminMode)
        {
            _isActiveAdminMode.Value = isActiveAdminMode;
        }

        public void Dispose()
        {
            _messages = null;
            _isActiveAdminMode = null;
            _isInitialized = false;
        }
    }
}
