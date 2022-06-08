namespace Gameboard.EventArgs
{
    public class CompanionPropertyErrorResponse : CompanionErrorResponse
    {
        public enum CompanionPropertyErrorTypes
        {
            NoError = 0,
            InvalidPropertyName = 1, // Property name is invalid.
            InvalidValue = 2, // Value sent is invalid for the given property.
        }
    }
}