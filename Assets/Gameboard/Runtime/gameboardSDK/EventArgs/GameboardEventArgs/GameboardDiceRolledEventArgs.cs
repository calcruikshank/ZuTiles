namespace Gameboard.EventArgs
{
    public class GameboardDiceRolledEventArgs : GameboardIncomingEventArg
    {
        public string diceNotation;
        public int[] diceSizesRolledList;
        public int addedModifier;
    }
}