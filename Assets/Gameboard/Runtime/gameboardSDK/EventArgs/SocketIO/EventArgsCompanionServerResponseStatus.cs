namespace Gameboard.EventArgs
{
    public class EventArgsCompanionServerResponseStatus
    {
        public enum ResponseStatusCode
        {
            OK = 200,
            EndpointNotFound = 404,
            ServerInternalError = 500
        }
    }
}