namespace Gameboard.EventArgs
{
    public class GameboardTickBoxStateChangedEventArgs : GameboardIncomingEventArg
    {
        public string userId;
        public string tickboxId;
        public DataTypes.TickboxStates newState;
    }
}
