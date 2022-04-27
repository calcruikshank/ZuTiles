namespace Gameboard.EventArgs
{
    public class CompanionAssetErrorResponse : CompanionErrorResponse
    {
        public enum AssetErrorTypes
        {
            NoError = 0,
            NoCompanionAssetFoundAtUID = 1, // Message: No CompanionAsset could be found at the supplied UID of {UID}
            RequestedCompanionAssetIsNotInExpectedFormat = 2, // Message: Requested CompanionAsset is in format {FORMATNAME}, however the expected format was {FORMATNAME}
            UnableToApplyStyleToPlayerStyledObject = 3, // Message: Requested object with UID {UID} is a PlayerStyled object, and cannot have its style changed manually.
        }
    }
}