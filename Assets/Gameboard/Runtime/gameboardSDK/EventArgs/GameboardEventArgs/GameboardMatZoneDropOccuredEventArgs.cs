namespace Gameboard.EventArgs
{
    public class GameboardMatZoneDropOccuredEventArgs : GameboardIncomingEventArg
    {
        public string userId;
        public string matZoneId;
        public string droppedObjectId;
    }
}