namespace Gameboard.EventArgs
{
    public class CompanionDiceRollErrorResponse : CompanionErrorResponse
    {
        public enum DiceRollErrorTypes
        {
            NoError = 0,
            InvalidDiceSize = 1, // Message: Requested dice size of {DICE_SIZE} is invalid.
            TintColorInvalid = 2, // Message: Requested color tint is invalid.
            AddedModifierInvalid = 3, // Message: The pre-added Modifier is an invalid value.
        }
    }
}