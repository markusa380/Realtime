using System;
using KSP.UI.Screens;
using UnityEngine;
using static KSP.UI.Screens.ApplicationLauncher;

namespace Realtime
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class RealtimeInterface : MonoBehaviour
    {
        public static RealtimeInterface Instance { get; private set; }

        // false when entire UI is hidden by pressing F2
        private bool visible = true;

        // true when the window is opened from the toolbar
        private bool open = false;

        private ApplicationLauncherButton toolbarButton;
        private readonly int mainGuid = Guid.NewGuid().GetHashCode();
        private Rect rect = new Rect(100, 100, 200, 100);

        public void Start()
        {
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onHideUI.Add(OnHideUI);
            Texture icon = GameDatabase.Instance.GetTexture("Realtime/Textures/icon", false);
            toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                OnOpen,
                OnClose,
                null,
                null,
                null,
                null,
                AppScenes.SPACECENTER,
                icon
            );
            Instance = this;
        }

        private bool IsShown()
        {
            return visible && open;
        }

        public void OnGUI()
        {
            if (IsShown())
            {
                rect = GUILayout.Window(mainGuid, rect, WindowFunction, "Realtime");
            }
        }

        private string configuredTimeStr = "";

        private void WindowFunction(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Base time:");
            var baseTimeStr = "Not set";
            if (RealtimeConfig.Instance.baseTime.HasValue)
            {
                var baseTime = RealtimeConfig.Instance.baseTime.Value;
                var baseTimeLocalized = DateTimeUtil.Localize(
                    baseTime,
                    RealtimeConfig.Instance.useLocalTime
                );
                baseTimeStr = DateTimeUtil.ToHumanReadable(baseTimeLocalized);
            }
            GUILayout.Label(baseTimeStr);

            configuredTimeStr = GUILayout.TextField(configuredTimeStr);
            DateTimeOffset? time = null;
            string error = "";
            try
            {
                time = DateTimeUtil.FromHumanReadable(configuredTimeStr);
            }
            catch (FormatException)
            {
                error = "Invalid date format!";
            }

            var color = GUI.color;
            GUI.color = Color.red;
            GUILayout.Label(error);
            GUI.color = color;

            GUILayout.BeginHorizontal();
            if (time == null)
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Set"))
            {
                if (time.HasValue)
                {
                    RealtimeConfig.Instance.baseTime = time;
                    RealtimePlugin.Instance.Reset();
                }
            }
            GUI.enabled = true;
            if (GUILayout.Button("Now"))
            {
                var configuredTimeLocalized = DateTimeUtil.Localize(
                    DateTimeOffset.Now,
                    RealtimeConfig.Instance.useLocalTime
                );

                configuredTimeStr = DateTimeUtil.ToHumanReadable(configuredTimeLocalized);
            }
            if (GUILayout.Button("Unset"))
            {
                RealtimeConfig.Instance.baseTime = null;
                RealtimePlugin.Instance.Reset();
            }
            GUILayout.EndHorizontal();

            var useLocalTimeLabel = RealtimeConfig.Instance.useLocalTime
                ? "Use Local time"
                : "Use UTC";
            if (GUILayout.Button(useLocalTimeLabel))
            {
                RealtimeConfig.Instance.useLocalTime = !RealtimeConfig.Instance.useLocalTime;
                RefreshConfiguredTimeStr();
            }
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void OnOpen()
        {
            open = true;

            RefreshConfiguredTimeStr();
        }

        private void RefreshConfiguredTimeStr()
        {
            DateTimeOffset configuredTime;
            if (RealtimeConfig.Instance.baseTime.HasValue)
            {
                configuredTime = RealtimeConfig.Instance.baseTime.Value;
            }
            else
            {
                configuredTime = DateTime.UtcNow;
            }

            var configuredTimeLocalized = DateTimeUtil.Localize(
                configuredTime,
                RealtimeConfig.Instance.useLocalTime
            );

            configuredTimeStr = DateTimeUtil.ToHumanReadable(configuredTimeLocalized);
        }

        private void OnClose()
        {
            open = false;
        }

        private void OnShowUI()
        {
            visible = true;
        }

        private void OnHideUI()
        {
            visible = false;
        }

        public void OnDestroy()
        {
            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onHideUI.Remove(OnHideUI);
            ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
        }
    }
}
