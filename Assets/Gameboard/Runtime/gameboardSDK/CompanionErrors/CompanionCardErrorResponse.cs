namespace Gameboard.EventArgs
{
    public class CompanionCardErrorResponse : CompanionErrorResponse
    {
        public enum CardErrorTypes
        {
            NoError = 0,
            NoCardFoundWithUID = 1, // Message: No object could be found with requested UID {UID}
            FrontTextureIDNotFound = 2, // Message: No front texture found with UID {UID}
            BackTextureIDNotFound = 3, // Message: Not back texture found with UID {UID}
            FrontIDWrongFormat = 4, // Message: Front texture UID {UID} is not a texture format.
            BackIDWrongFormat = 5, // Message: Back texture UID {UID} is not a texture format.
        }
    }
}