namespace Gameboard.EventArgs
{
    public class EventArgRollDice
    {
        public int[] diceSizesToRoll;
        public int addedModifier;
        public string diceTintHexColor;
        public string diceNotation;
        public string ownerId;
        //public string[] orderedDiceTextureUIDs; // Phase 2
        //public string[] customDiceUIDs; // Phase 4
    }
}