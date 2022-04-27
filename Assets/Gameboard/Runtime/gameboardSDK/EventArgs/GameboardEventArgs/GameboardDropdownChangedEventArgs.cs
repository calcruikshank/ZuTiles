namespace Gameboard.EventArgs
{
    public class GameboardDropdownChangedEventArgs : GameboardIncomingEventArg
    {
        public string userId;
        public string dropDownId;
        public int newIndex;
    }
}