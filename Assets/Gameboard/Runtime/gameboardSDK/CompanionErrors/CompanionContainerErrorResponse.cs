namespace Gameboard.EventArgs
{
    public class CompanionContainerErrorResponse : CompanionErrorResponse
    {
        public enum ContainerErrorTypes
        {
            NoError = 0,
            NoObjectFoundWithUID = 1, // Message: No object could be found with requested UID {UID}
            NoContainerFoundWithUID = 2, // Message: No container could be found with requested UID {UID}
            InvalidSortingType = 3, // Message: Requested sorting type was invalid
            ObjectUIDAlreadyInContainer = 4, // Message: Object with UID is already in container UID.
            ObjectNotInContainer = 5, // Message: Object with UID does not exist in container UID.
        }
    }
}