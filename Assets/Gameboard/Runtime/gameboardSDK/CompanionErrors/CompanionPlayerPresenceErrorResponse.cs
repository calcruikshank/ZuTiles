namespace Gameboard.EventArgs
{
    public class CompanionPlayerPresenceErrorResponse : CompanionErrorResponse
    {
        public enum PlayerPresenceErrorTypes
        {
            NoError = 0,
            UnableToDeserializePlayerList = 1,
            UnableToDeserializeCompanionServerResponse = 2,
            UnableToSerializeEventArgsToServer = 3,
            ExceptionOccured = 4,
        }
    }
}