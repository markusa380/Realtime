using UnityEngine;

namespace Realtime
{
    internal class Logging
    {
        public static void Info(string message)
        {
            Debug.Log("[Realtime] " + message);
        }

        public static void Warn(string message)
        {
            Debug.LogWarning("[Realtime] " + message);
        }
    }
}
