using R3;
using Titles.ViewModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Titles.Views
{
    /// <summary>
    /// メッセージパネルのビュー
    /// </summary>
    public class MessagePanelView : MonoBehaviour
    {
        /// <summary>メッセージパネルのビューモデル</summary>
        [SerializeField] private MessagePanelViewModel viewModel;
        /// <summary>メッセージ内容表示用テキスト</summary>
        [SerializeField] private Text messageContent;
        /// <summary>初期化済みフラグ</summary>
        private readonly ReactiveProperty<bool> _isCompleted = new ReactiveProperty<bool>();
        /// <summary>初期化済みフラグ</summary>
        public ReadOnlyReactiveProperty<bool> IsCompleted => _isCompleted;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag = new DisposableBag();

        private void Reset()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals("MessageContent"))
                {
                    if (messageContent == null)
                        messageContent = child.GetComponent<Text>();
                }
            }
        }

        private void Start()
        {
            viewModel.Initialize();

            viewModel.Messages.Subscribe(messages =>
            {
                RenderMessage(messages);
            })
                .AddTo(ref _disposableBag);

            _isCompleted.Value = true;
        }

        private void OnDestroy()
        {
            _disposableBag.Dispose();
            viewModel.Dispose();
        }

        /// <summary>
        /// メッセージ内容を表示
        /// </summary>
        /// <param name="messages">メッセージリスト</param>
        private void RenderMessage(List<string> messages)
        {
            var messageTextArea = string.Join("\n", messages);
            messageContent.text = messageTextArea;
        }
    }
}
