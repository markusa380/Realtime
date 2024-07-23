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
        private static readonly string UNSET_VALUE = "UNSET";

        public DateTime? baseTime = null;

        public RealtimeConfig() : base()
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
                    try { 
                        baseTime = DateTimeUtil.FromISO8601(value);
                    }
                    catch (Exception e)
                    {
                        Logging.Warn("Failed to parse baseTime " + value + ": " + e.Message);
                    }
                }
            }
            else
            {
                Debug.LogWarning("BaseTimeSeconds not found in config");
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
        }
    }
}
