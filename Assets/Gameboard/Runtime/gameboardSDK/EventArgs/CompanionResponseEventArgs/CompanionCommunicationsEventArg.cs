namespace Gameboard.EventArgs
{
    public class CompanionCommunicationsEventArg
    {
        public string message;
        public StatusCodes statusCode = StatusCodes.NoError;

        public enum StatusCodes
        {
            NoError = 0,
            Error = 1,
        }
    }
}