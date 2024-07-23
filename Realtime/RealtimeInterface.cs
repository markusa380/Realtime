using KSP.UI.Screens;
using System;
using UnityEngine;
using static KSP.UI.Screens.ApplicationLauncher;

namespace Realtime
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class RealtimeInterface: MonoBehaviour
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
            toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                OnOpen,
                OnClose,
                null,
                null,
                null,
                null,
                AppScenes.SPACECENTER,
                new Texture2D(36, 36)
            );
            Instance = this;
        }

        public bool IsShown()
        {
            return visible && open;
        }

        public void OnGUI()
        {
            if (IsShown())
            {
                rect = GUILayout.Window(
                    mainGuid,
                    rect,
                    WindowFunction,
                    "Realtime"
                );
            }
        }

        private string configuredTimeStr = "";

        private void WindowFunction(int windowID)
        {
            GUILayout.BeginVertical();
            if (RealtimeConfig.Instance.baseTime.HasValue)
            {
                GUILayout.Label("Base time: " + DateTimeUtil.ToISO8601(RealtimeConfig.Instance.baseTime.Value));
            }
            else
            {
                GUILayout.Label("Base time not set");
            }

            configuredTimeStr = GUILayout.TextField(configuredTimeStr);
            DateTime? time = null;
            try
            {
                time = DateTimeUtil.FromISO8601(configuredTimeStr);
            }
            catch (FormatException)
            {
                var color = GUI.color;
                GUI.color = Color.red;
                GUILayout.Label("Invalid date format!");
                GUI.color = color;
            }

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
                RealtimeConfig.Instance.baseTime = DateTime.UtcNow;
            }
            if (GUILayout.Button("Unset"))
            {
                RealtimeConfig.Instance.baseTime = null;
                RealtimePlugin.Instance.Reset();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void OnOpen()
        {
            open = true;

            if (RealtimeConfig.Instance.baseTime.HasValue)
            {
                configuredTimeStr = DateTimeUtil.ToISO8601(RealtimeConfig.Instance.baseTime.Value);
            }
            else
            {
                configuredTimeStr = DateTimeUtil.ToISO8601(DateTime.UtcNow);
            }
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
