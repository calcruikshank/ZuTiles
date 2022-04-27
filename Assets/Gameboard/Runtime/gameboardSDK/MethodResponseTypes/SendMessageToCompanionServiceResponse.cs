using Gameboard.EventArgs;

namespace Gameboard.Utilities
{
    public class SendMessageToCompanionServiceResponse
    {
        public bool success;
        public EventArgsToGameboard eventArgs;
        public string errorMessage;
        public string responseForEventId;
    }
}