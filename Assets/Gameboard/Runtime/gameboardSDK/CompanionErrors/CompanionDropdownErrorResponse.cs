namespace Gameboard.EventArgs
{
    public class CompanionDropdownErrorResponse : CompanionErrorResponse
    {
        public enum DropdownErrorTypes
        {
            NoError = 0,
            RequestedIndexOutsideDropdownSize = 1, // The index requested is outside the size of the dropdown index count
        }
    }
}