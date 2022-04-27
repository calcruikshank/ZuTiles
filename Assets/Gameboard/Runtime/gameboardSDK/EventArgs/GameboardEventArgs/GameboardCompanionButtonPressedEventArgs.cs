namespace Gameboard.EventArgs
{
    public class GameboardCompanionButtonPressedEventArgs : GameboardIncomingEventArg
    {
        public string buttonId;
        public string callbackMethod;
    }
}