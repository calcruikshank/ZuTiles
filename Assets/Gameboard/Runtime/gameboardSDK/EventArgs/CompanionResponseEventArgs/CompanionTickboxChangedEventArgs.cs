using static Gameboard.DataTypes;

namespace Gameboard.EventArgs
{
    public class CompanionTickboxChangedEventArgs
    {
        public string userIdOfChangedTickbox;
        public string changedTickboxId;
        public TickboxStates newTickboxState;
    }
}