using System;
using UnityEngine;

namespace Realtime
{
    [KSPScenario(
        ScenarioCreationOptions.AddToAllGames,
        GameScenes.FLIGHT,
        GameScenes.TRACKSTATION,
        GameScenes.SPACECENTER
    )]
    internal class RealtimeConfig : ScenarioModule
    {
        public static RealtimeConfig Instance { get; private set; }

        private static readonly string BASE_TIME_NODE = "baseTime";
        private static readonly string USE_LOCAL_TIME_NODE = "useLocalTime";
        private static readonly string UNSET_VALUE = "UNSET";

        public DateTimeOffset? baseTime = null;
        public bool useLocalTime = false;

        public RealtimeConfig()
            : base()
        {
            Instance = this;
        }

        public override void OnLoad(ConfigNode node)
        {
            Logging.Info("Config OnLoad");

            if (node.HasValue(BASE_TIME_NODE))
            {
                var value = node.GetValue(BASE_TIME_NODE);

                if (value == UNSET_VALUE)
                {
                    baseTime = null;
                }
                else
                {
                    try
                    {
                        baseTime = DateTimeUtil.FromISO8601(value);
                    }
                    catch (Exception e)
                    {
                        Logging.Warn($"Failed to parse {BASE_TIME_NODE} {value}: {e.Message}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"{BASE_TIME_NODE} not found in config");
            }

            if (node.HasValue(USE_LOCAL_TIME_NODE))
            {
                var value = node.GetValue(USE_LOCAL_TIME_NODE);
                try
                {
                    useLocalTime = bool.Parse(value);
                }
                catch (Exception e)
                {
                    Logging.Warn($"Failed to parse {USE_LOCAL_TIME_NODE} {value}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"{USE_LOCAL_TIME_NODE} not found in config");
            }
        }

        public override void OnSave(ConfigNode node)
        {
            Logging.Info("Config OnSave");
            if (baseTime.HasValue)
            {
                node.AddValue(BASE_TIME_NODE, DateTimeUtil.ToISO8601(baseTime.Value));
            }
            else
            {
                node.AddValue(BASE_TIME_NODE, UNSET_VALUE);
            }

            node.AddValue(USE_LOCAL_TIME_NODE, useLocalTime);
        }
    }
}
