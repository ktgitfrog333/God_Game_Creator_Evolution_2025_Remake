using UnityEngine;

namespace Titles.Tests
{
    /// <summary>
    /// オーディオ設定のテスト
    /// </summary>
    public class AudioSettingsManagerTest : MonoBehaviour
    {
        private void OnGUI()
        {
            int y = 10;
            const int buttonWidth = 500;
            const int buttonHeight = 50;

            if (GUI.Button(new Rect(10, y, buttonWidth, buttonHeight), "currentDeviceIdをブランク"))
            {
                var audioSettings = AudioSettingsManager.LoadSettings();
                audioSettings.currentDeviceId = string.Empty;
                bool success = AudioSettingsManager.SaveSettings(audioSettings);
                if (success)
                {
                    Debug.Log($"音声設定を保存しました: BGM={audioSettings.bgmVolume}, SE={audioSettings.seVolume}, MicInput={audioSettings.micInputEnabled}, DeviceId={audioSettings.currentDeviceId}");
                }
                else
                {
                    Debug.LogWarning("音声設定の保存に失敗しました。");
                }
            }
        }
    }
}
