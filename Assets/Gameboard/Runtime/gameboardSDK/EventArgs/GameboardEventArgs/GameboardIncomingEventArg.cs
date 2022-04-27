namespace Gameboard.EventArgs
{
    public class GameboardIncomingEventArg
    {
        /// <summary>
        /// The ownerId is the ID that this event was regarding. If the event is about a Gameboard, it will be the ID of that Gameboard. If the event is for a specific Companion, it will be the PlayerID for that Companion's player.
        /// </summary>
        public string ownerId;

        // NOTE: userId is a placeholder item to cover some edge cases in the companion. These are all being turned into ownerId, and this will get removed.
        public string userId;
    }
}