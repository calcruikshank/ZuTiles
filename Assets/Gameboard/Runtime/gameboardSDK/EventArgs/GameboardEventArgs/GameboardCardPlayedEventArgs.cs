namespace Gameboard.EventArgs
{
    public class GameboardCardPlayedEventArgs : GameboardIncomingEventArg
    {
        public string cardId;
        public string userId;
    }
}