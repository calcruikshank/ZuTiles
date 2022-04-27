namespace Gameboard.EventArgs
{
    public class CompanionTickboxErrorResponse : CompanionErrorResponse
    {
        public enum TickboxErrorTypes
        {
            NoError = 0,
            TextureMissing = 1, // Message: Texture not found at UID {UID}
            AssetInWrongFormat = 2, // Message: Asset at UID {UID} was in format {FORMAT} however was expected to be a Texture.
        }
    }
}