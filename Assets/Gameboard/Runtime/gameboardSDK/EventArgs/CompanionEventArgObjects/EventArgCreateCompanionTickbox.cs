namespace Gameboard.EventArgs
{
    public class EventArgCreateCompanionTickbox
    {
        public string id;
        public string onTextureId;
        public string offTextureId;
        public DataTypes.TickboxStates startingState;
        public bool isInputLocked;
    }
}