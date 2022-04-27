using System;
using UnityEngine;

namespace Gameboard
{
    public interface IGameboardConfig
    {
        Version SDKVersion { get; }
        int gameboardConnectPort { get; }
        Vector2 deviceResolution { get; }
        string companionServerUri { get; }
        string companionServerUriNamespace { get; }
        string gameboardId { get; }
        int eventTimeoutLength { get; }
        int eventRetryCount { get; }
        AndroidJavaClass mGameboardSettings { get; }
        AndroidApplicationContext androidApplicationContext { get; }
        DataTypes.OperatingMode operatingMode { get; }
    }
}