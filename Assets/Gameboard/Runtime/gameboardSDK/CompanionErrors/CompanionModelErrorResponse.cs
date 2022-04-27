namespace Gameboard.EventArgs
{
    public class CompanionModelErrorResponse : CompanionErrorResponse
    {
        public enum ModelErrorTypes
        {
            NoError = 0,
            NoModelFoundAtUID = 1, // No asset be found at UID
            NoTextureFoundAtUID = 2, // No asset found at UID
            ModelAtUIDInWrongFormat = 3, // Asset at UID is not a model format
            TextureAtUIDInWrongFormat = 4, // Asset at UID is not a texture format
            ColorNotInValidHexFormat = 5, // The entered color is not in a valid Hex Color format
        }
    }
}