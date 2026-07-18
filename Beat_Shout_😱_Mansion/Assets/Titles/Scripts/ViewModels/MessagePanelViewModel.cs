using R3;
using UnityEngine;
using System.Collections.Generic;
using Titles.Models;

namespace Titles.ViewModels
{
    /// <summary>
    /// メッセージパネルのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "MessagePanelViewModel", menuName = "Scriptable Objects/MessagePanelViewModel")]
    public class MessagePanelViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] private LauncherModel model;
        /// <summary>メッセージリスト</summary>
        private ReactiveCommand<List<string>> _messages;
        /// <summary>メッセージリスト</summary>
        public ReactiveCommand<List<string>> Messages => _messages;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag;

        public void Initialize()
        {
            _messages = new ReactiveCommand<List<string>>();
            _disposableBag = new DisposableBag();

            model.Initialize();

            model.Messages.Where(x => 0 < x.Count)
                .Subscribe(messages =>
                {
                    _messages.Execute(messages);
                })
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            _messages = null;

            model.Dispose();
            _disposableBag.Dispose();
        }
    }
}
