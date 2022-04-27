namespace Gameboard.EventArgs
{
    public class CompanionHandDisplayErrorResponse : CompanionErrorResponse
    {
        public enum CompanionHandDisplayErrorTypes
        {
            NoError = 0,
            CardAlreadyExistsInHandDisplay = 1, // Card with UID already exists in Hand Display with UID {UID} 
            CardDoesNotExistInHandDisplay = 2, // Card with UID does not exist in Hand Display with UID {UID} 
            CardUIDNotFound = 3, // No card found with UID
            HandDisplayUIDNotFound = 4, // No hand display found with UID
        }
    }
}