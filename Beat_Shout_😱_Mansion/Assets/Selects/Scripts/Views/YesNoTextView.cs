using R3;
using Selects.Commons;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Selects.Views
{
    /// <summary>
    /// YesかNoのボタン制御ビュー
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(EventTrigger))]
    public class YesNoTextView : MonoBehaviour
    {
        /// <summary>ボタン</summary>
        [SerializeField] private Button button;
        /// <summary>イベントトリガー</summary>
        [SerializeField] private EventTrigger eventTrigger;
        /// <summary>実行イベントの監視</summary>
        /// <remarks>-1 : Default<br/>
        /// 0 : Selected<br/>
        /// 1 : DeSelected<br/>
        /// 2 : Submited<br/>
        /// 3 : Canceled<br/>
        /// 4 : AnyKeysPushed</remarks>
        private ReactiveCommand<EnumEventCommand> _eventState = new ReactiveCommand<EnumEventCommand>();
        /// <summary>実行イベントの監視</summary>
        /// <remarks>-1 : Default<br/>
        /// 0 : Selected<br/>
        /// 1 : DeSelected<br/>
        /// 2 : Submited<br/>
        /// 3 : Canceled<br/>
        /// 4 : AnyKeysPushed</remarks>
        public ReactiveCommand<EnumEventCommand> EventState => _eventState;
        /// <summary>親コンテンツから見たボタンインデックス</summary>
        private int _index;
        /// <summary>イベントシステム</summary>
        private EventSystem _eventSystem;

        private void Reset()
        {
            button = GetComponent<Button>();
            eventTrigger = GetComponent<EventTrigger>();
        }

        private void Awake()
        {
            _index = transform.GetSiblingIndex();
            _eventSystem = FindAnyObjectByType<EventSystem>();
        }

        private async void OnEnable()
        {
            _eventSystem.SetSelectedGameObject(null);
            if (_index == 0)
            {
                await Task.Delay(100);
                _eventSystem.SetSelectedGameObject(gameObject);
                _eventState.Execute(EnumEventCommand.Selected);
            }
            else
            {
                _eventState.Execute(EnumEventCommand.Default);
            }
        }

        /// <summary>
        /// 選択された時に発火するイベント
        /// </summary>
        public void Selected()
        {
            _eventState.Execute(EnumEventCommand.Selected);
        }

        /// <summary>
        /// 選択されなかった時に発火するイベント
        /// </summary>
        public void DeSelected()
        {
            _eventState.Execute(EnumEventCommand.DeSelected);
        }

        /// <summary>
        /// 確定時に発火するイベント
        /// </summary>
        public virtual void Submited()
        {
            _eventState.Execute(EnumEventCommand.Submited);
        }

        /// <summary>
        /// キャンセル時に発火するイベント
        /// </summary>
        public void Canceled()
        {
            _eventState.Execute(EnumEventCommand.Canceled);
        }

        /// <summary>
        /// 有効かどうかをセット
        /// </summary>
        /// <param name="enabled">有効か</param>
        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                if (!button.interactable)
                    button.interactable = true;
                if (!eventTrigger.enabled)
                    eventTrigger.enabled = true;
            }
            else
            {
                if (button.interactable)
                    button.interactable = false;
                if (eventTrigger.enabled)
                    eventTrigger.enabled = false;
            }
        }
    }
}
