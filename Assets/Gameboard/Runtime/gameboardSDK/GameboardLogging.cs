using UnityEngine;

namespace Gameboard
{
    public static class GameboardLogging
    {
        /// <summary>
        /// Types of messages that can be logged.
        /// </summary>
        public enum MessageTypes { Log, Warning, Error, Verbose }

        
        public static void Log(string inString)
        {
            LogMessage(inString, MessageTypes.Log);
        }

        public static void Error(string inString)
        {
            LogMessage(inString, MessageTypes.Error);
        }

        public static void Warning(string inString)
        {
            LogMessage(inString, MessageTypes.Warning);
        }

        public static void Verbose(string inString)
        {
            LogMessage(inString, MessageTypes.Verbose);
        }

        /// <summary>
        /// Records a message with the Gameboard logging service. This is intended to get all SDK logging in one place to eventually extend this
        /// to allow recording logs in an external service.
        /// </summary>
        /// <param name="inString"></param>
        /// <param name="inType"></param>
        public static void LogMessage(string inString, MessageTypes inType)
        {
            switch (inType)
            {
                case MessageTypes.Log: Debug.Log($"--- Gameboard Log: {inString}"); break;
                case MessageTypes.Error: Debug.LogError($"--- Gameboard Error: {inString}"); break;
                case MessageTypes.Warning: Debug.LogWarning($"--- Gameboard Warning: {inString}"); break;
                case MessageTypes.Verbose:

#if GAMEBOARD_VERBOSE_LOGGING
                Debug.Log("--- Gameboard Verbose: " + inString);        
#endif
                break;
            }
        }
    }
}