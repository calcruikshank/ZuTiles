namespace Gameboard.EventArgs
{
    public class CompanionDialogErrorResponse : CompanionErrorResponse
    {
        public enum DialogErrorTypes
        {
            NoError = 0,
            NoDialogFoundWithUID = 1, // No dialog found with UID
            NoObjectFoundWithUID = 2, // No object could be found with requested UID
        }
    }
}