namespace Gameboard.EventArgs
{
    public class CompanionDisplayMessageErrorResponse : CompanionErrorResponse
    {
        public enum DisplayMessageErrorTypes
        {
            NoError = 0,
            DisplayLengthInvalid = 1, // The requested time to display was invalid.
            DisplayTextInvalid = 2, // The requested text to display was invalid.
        }
    }
}