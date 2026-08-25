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
        /// <summary>シーンロード演出の完了タイプ</summary>
        /// <remarks>0: シーン開始演出中<br/>
        /// 1: シーン開始演出の完了<br/>
        /// 2: シーン終了演出中<br/>
        /// 3: シーン終了演出の完了</remarks>
        private ReactiveProperty<int> _completedSceneLoadDirectionType;
        /// <summary>シーンロード演出の完了タイプ</summary>
        public ReadOnlyReactiveProperty<int> CompletedSceneLoadDirectionType => _completedSceneLoadDirectionType;

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _messages = new ReactiveProperty<List<string>>(new List<string>());
            _isActiveAdminMode = new ReactiveProperty<bool>();
            _completedSceneLoadDirectionType = new ReactiveProperty<int>();
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

        public void SetCompletedSceneLoadDirectionType(int completedSceneLoadDirectionType)
        {
            _completedSceneLoadDirectionType.Value = completedSceneLoadDirectionType;
        }

        public void Dispose()
        {
            _messages = null;
            _isActiveAdminMode = null;
            _completedSceneLoadDirectionType = null;
            _isInitialized = false;
        }
    }
}
