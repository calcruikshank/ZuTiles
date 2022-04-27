using Gameboard.Utilities;

namespace Gameboard
{
    public class GameboardServiceManager : IGameboardServiceManager
    {
        public TouchUtility touchUtility { get; }
        public CompanionHandlerUtility companionUtility { get; }
        public GameboardCommunicationUtility gameBoardCommunicationsUtility { get; }
        public GameboardTouchHandlerUtility gameBoardHandler { get; }
        public CompanionCommunicationsUtility companionCommunications { get; }
        public CompanionHandlerUtility companionHandler { get; }
        public JsonUtility jsonUtility { get; }

        public GameboardServiceManager(GameboardConfig gameboardConfig)
        {
            // Utilities
            jsonUtility = new JsonUtility();
            touchUtility = new TouchUtility();
            companionUtility = new CompanionHandlerUtility();
            gameBoardCommunicationsUtility = new GameboardCommunicationUtility(gameboardConfig, jsonUtility);
            gameBoardHandler = new GameboardTouchHandlerUtility(gameBoardCommunicationsUtility, gameboardConfig);
            companionCommunications = new CompanionCommunicationsUtility(gameboardConfig);
            companionHandler = new CompanionHandlerUtility();
        }

        public void PerformUtilityUpdate()
        {
            jsonUtility?.ProcessUpdate();
            touchUtility?.ProcessUpdate();
            companionUtility?.ProcessUpdate();
            gameBoardCommunicationsUtility?.ProcessUpdate();
            gameBoardHandler?.ProcessUpdate();
            companionCommunications?.ProcessUpdate();
            companionHandler?.ProcessUpdate();
        }

        public void PerformUtilityLateUpdate()
        {
            jsonUtility?.ProcessLateUpdate();
            touchUtility?.ProcessLateUpdate();
            companionUtility?.ProcessLateUpdate();
            gameBoardCommunicationsUtility?.ProcessLateUpdate();
            gameBoardHandler?.ProcessLateUpdate();
            companionCommunications?.ProcessLateUpdate();
            companionHandler?.ProcessLateUpdate();
        }
    }
}