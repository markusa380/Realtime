using System;
using UnityEngine;

namespace Realtime
{
    enum PluginState
    {
        INIT,
        OFF,
        GRACE_PERIOD,
        BEHIND,
        ON_TIME,
    }

    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class RealtimePlugin : MonoBehaviour
    {
        public static RealtimePlugin Instance { get; private set; }

        private static readonly double MAX_OFFSET_ERROR_SECONDS = 10.0;
        private static readonly double LOAD_GRACE_PERIOD_SECONDS = 3.0;
        private static readonly double CATCHUP_DURATION_SECONDS = 0.2;

        private PluginState state = PluginState.INIT;
        private float startTime;

        public void Start()
        {
            startTime = Time.time;
            Instance = this;
            KSPUtil.dateTimeFormatter = new RealtimeFormatter();
        }

        public void Update()
        {
            if (FlightDriver.Pause)
            {
                return;
            }

            PluginState oldState = state;

            double offsetToRealtime;
            switch (state)
            {
                case PluginState.INIT:
                    if (CheckScene())
                    {
                        state = PluginState.GRACE_PERIOD;
                    }
                    else
                    {
                        state = PluginState.OFF;
                    }
                    break;
                case PluginState.GRACE_PERIOD:
                    if (Time.time - startTime > LOAD_GRACE_PERIOD_SECONDS)
                    {
                        if (RealtimeConfig.Instance.baseTime.HasValue)
                        {
                            state = PluginState.ON_TIME;
                        }
                        else
                        {
                            InfoNoop();
                            state = PluginState.OFF;
                        }
                    }
                    break;
                case PluginState.ON_TIME:
                    // Disable warping
                    if (TimeWarp.CurrentRateIndex != 0)
                    {
                        StopWarp();
                    }

                    offsetToRealtime = GetOffsetToRealtimeSeconds();

                    if (offsetToRealtime > MAX_OFFSET_ERROR_SECONDS)
                    {
                        WarnAhead(offsetToRealtime);
                        state = PluginState.OFF;
                    }
                    else if (offsetToRealtime < -MAX_OFFSET_ERROR_SECONDS)
                    {
                        InfoBehind(offsetToRealtime);
                        state = PluginState.BEHIND;
                    }
                    break;
                case PluginState.BEHIND:
                    offsetToRealtime = GetOffsetToRealtimeSeconds();
                    if (GetOffsetToRealtimeSeconds() > 0)
                    {
                        InfoReachedRealtime();
                        StopWarp();
                        state = PluginState.ON_TIME;
                    }
                    else
                    {
                        WarpAhead(offsetToRealtime);
                    }
                    break;
                case PluginState.OFF:
                    break;
            }

            if (oldState != state)
            {
                Logging.Info($"State transition: {oldState} -> {state}");
            }
        }

        public void Reset()
        {
            Logging.Info("Resetting plugin");
            if (state != PluginState.INIT)
            {
                StopWarp();
                state = PluginState.INIT;
            }
        }

        private double GetOffsetToRealtimeSeconds()
        {
            var baseTime = RealtimeConfig.Instance.baseTime.Value;
            var inGameTime = baseTime.AddSeconds(Planetarium.GetUniversalTime());
            return inGameTime.Subtract(DateTimeOffset.UtcNow).TotalSeconds;
        }

        private static readonly GameScenes[] ALLOWED_SCENES = new GameScenes[]
        {
            GameScenes.FLIGHT,
            GameScenes.TRACKSTATION,
            GameScenes.SPACECENTER
        };

        private bool CheckScene()
        {
            return Array.IndexOf(ALLOWED_SCENES, HighLogic.LoadedScene) != -1;
        }

        private void WarnAhead(double diff)
        {
            var deltaStr = KSPUtil.dateTimeFormatter.PrintDateDelta(diff, true, true, true);
            var message = $"Your in game time is ahead of real time by {deltaStr}!";
            PostScreenMessage(message);
        }

        private void InfoBehind(double diff)
        {
            var deltaStr = KSPUtil.dateTimeFormatter.PrintDateDelta(diff, true, true, true);
            var message = $"Your in game time is behind real time by {deltaStr}, warping ahead!";
            PostScreenMessage(message);
        }

        private void InfoNoop()
        {
            var message = "Realtime: No start time configured.";
            PostScreenMessage(message);
        }

        private void InfoReachedRealtime()
        {
            var message = "Caught up with real time!";
            PostScreenMessage(message);
        }

        private void StopWarp()
        {
            TimeWarp.fetch.CancelAutoWarp();
            TimeWarp.SetRate(0, true, false);
        }

        private void PostScreenMessage(string message)
        {
            ScreenMessages.PostScreenMessage(
                new ScreenMessage(message, 3f, ScreenMessageStyle.UPPER_CENTER)
            );
        }

        private void WarpAhead(double offsetToRealtime)
        {
            if (TimeWarp.WarpMode == TimeWarp.Modes.HIGH)
            {
                // Choose a warp rate so that catching up to real time will take more than 1 second
                // Default is one above 1x
                var chosenRateIndex = 1;

                for (int i = TimeWarp.fetch.warpRates.Length - 1; i > 0; i--)
                {
                    var rate = TimeWarp.fetch.warpRates[i];
                    var timeToCatchUp = Math.Abs(offsetToRealtime) / rate;

                    if (timeToCatchUp > CATCHUP_DURATION_SECONDS)
                    {
                        chosenRateIndex = i;
                        break;
                    }
                }

                if (chosenRateIndex != TimeWarp.CurrentRateIndex)
                {
                    TimeWarp.SetRate(chosenRateIndex, true, false);
                }
            }
        }
    }
}
