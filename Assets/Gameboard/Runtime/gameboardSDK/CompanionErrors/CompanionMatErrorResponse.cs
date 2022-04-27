namespace Gameboard.EventArgs
{
    public class CompanionMatErrorResponse : CompanionErrorResponse
    {
        public enum MatErrorTypes
        {
            NoError = 0,
            MatUIDDoesNotExist = 1, // No mat found at UID
            ObjectUIDAlreadyParented = 2, // Object at UID is already parented to mat with UID
            ObjectUIDNotFound = 3, // No object found at UID
            ObjectUIDDoesNotExistOnMat = 4, // Object at UID has not been placed on mat UID
            BackgroundTextureUIDDoesNotExist = 5, // No object found with UID for background texture
            BackgroundTextureUIDWrongFormat = 6, // The object at UID is not an image format object
        }
    }
}