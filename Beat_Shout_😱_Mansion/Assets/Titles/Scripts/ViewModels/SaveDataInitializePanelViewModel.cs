using R3;
using System.Collections.Generic;
using Titles.Models;
using UnityEngine;
using Universal.Commons;
using Universal.Utilities;

namespace Titles.ViewModels
{
    /// <summary>
    /// セーブデータ初期化パネルのビューモデル
    /// </summary>
    [CreateAssetMenu(fileName = "SaveDataInitializePanelViewModel", menuName = "Scriptable Objects/SaveDataInitializePanelViewModel")]
    public class SaveDataInitializePanelViewModel : ScriptableObject, System.IDisposable
    {
        /// <summary>ランチャーモデル</summary>
        [SerializeField] private LauncherModel model;
        /// <summary>セーブデータ初期化メッセージのデータ</summary>
        [SerializeField] private SaveDataInitializeMessageData[] saveDataInitializeMessageDatas;
        /// <summary>セーブデータ初期化メッセージのデータ</summary>
        private Dictionary<int, SaveDataInitializeMessageData> _saveDataInitializeMessageDatasDic;
        /// <summary>セーブデータ初期化の例外メッセージ</summary>
        [SerializeField, TextArea] private string errorMessage = "メッセージデータが存在しないか、未設定です。";
        /// <summary>初期化済みフラグ</summary>
        private bool _isInitialized;
        /// <summary>管理者モード有効フラグ</summary>
        private ReactiveCommand<bool> _isActiveAdminMode;
        /// <summary>管理者モード有効フラグ</summary>
        public ReactiveCommand<bool> IsActiveAdminMode => _isActiveAdminMode;
        /// <summary>R3のリソース管理</summary>
        private DisposableBag _disposableBag;

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _isActiveAdminMode = new ReactiveCommand<bool>();
            _disposableBag = new DisposableBag();

            model.Initialize();

            model.IsActiveAdminMode.Where(x => x)
                .Subscribe(isActiveAdminMode =>
                {
                    _isActiveAdminMode.Execute(isActiveAdminMode);
                })
                .AddTo(ref _disposableBag);

            _saveDataInitializeMessageDatasDic = new Dictionary<int, SaveDataInitializeMessageData>();
            foreach (var data in saveDataInitializeMessageDatas)
            {
                if (data != null)
                {
                    _saveDataInitializeMessageDatasDic[data.commandId] = data;
                }
            }
        }

        public void Execute(int targetCommandId)
        {
            var dic = _saveDataInitializeMessageDatasDic;
            string message = string.Empty;
            if (dic.ContainsKey(targetCommandId) && !string.IsNullOrEmpty(dic[targetCommandId].executeMessage))
            {
                message = dic[targetCommandId].executeMessage;
                var enumLoadMode = dic[targetCommandId].enumLoadMode;
                ResourcesUtility utility = new ResourcesUtility();
                UserBean userBean = utility.LoadSaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, enumLoadMode);
                utility.SaveDatasJsonOfUserBean(ConstResorcesNames.USER_DATA, userBean);
            }
            else
            {
                message = errorMessage;
            }
            model.AddMessages(message);
        }

        public void Dispose()
        {
            _saveDataInitializeMessageDatasDic = null;
            model.Dispose();
            _isActiveAdminMode = null;
            _disposableBag.Dispose();
            _isInitialized = false;
        }
    }

    /// <summary>
    /// セーブデータ初期化メッセージのデータ
    /// </summary>
    [System.Serializable]
    public class SaveDataInitializeMessageData
    {
        /// <summary>コマンドID</summary>
        public int commandId;
        /// <summary>実行メッセージ</summary>
        [TextArea]
        public string executeMessage;
        /// <summary>ロードモードの列挙型</summary>
        public EnumLoadMode enumLoadMode;
    }
}
