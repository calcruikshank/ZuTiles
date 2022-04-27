namespace Gameboard
{
    public static class DataTypes
    {
        /// <summary>
        /// The different types of Operating Modes that the SDK can be run in.
        /// </summary>
        public enum OperatingMode { Debug, Production }

        /// <summary>
        /// How to visually display objects inside of a Container on the Companion.
        /// </summary>
        public enum CompanionContainerSortingTypes
        {
            Stack = 0,
            Fan = 1,
        }

        /// <summary>
        /// Directions that a card can face on the Companion.
        /// </summary>
        public enum CardFacingDirections
        {
            FaceUp = 0,
            FaceDown = 1,
        }

        /// <summary>
        /// Display state of an object on the Companion.
        /// </summary>
        public enum ObjectDisplayStates
        {
            Hidden = 0,
            Displayed = 1,
        }

        /// <summary>
        /// Tickbox toggle state on the Companion.
        /// </summary>
        public enum TickboxStates
        {
            Off = 0,
            On = 1,
        }

        /// <summary>
        /// Types of objects that can be tracked on the Gameboard screen. Not the same as on-screen touches.
        /// </summary>
        public enum TrackedBoardObjectTypes
        {
            None = 0,
            Pointer = 1,
            Token = 2,
        }

        /// <summary>
        /// Coordinate systems used by the Gameboard screen
        /// </summary>
        public enum GameboardScreenCoordinateSystem
        {
            UNKNOWN = 0,
            TOP_LEFT = 1,
            BUTTOM_LEFT = 2,
        }

        /// <summary>
        /// Types of changes that can occure in a User Presence Update.
        /// </summary>
        public enum UserPresenceChangeTypes
        {
            UNKNOWN = -1,
            ADD = 0,
            REMOVE = 1,
            CHANGE = 2,
            CHANGE_POSITION = 3
        }

        /// <summary>
        /// The places a Presence Change can originate.
        /// </summary>
        public enum PresenceType
        {
            DRAWER = 0,
            COMPANION = 1,
            NONE = 2,
        }

        /// <summary>
        /// The types of users that can exist.
        /// </summary>
        public enum UserType
        {
            NONE = -1, // Value is empty
            USER = 0, // Logged in with a permanent UserID.
            GUEST = 1, // Guest user with a generated userID not associated with a Gameboard account.
            OPEN = 2, // Open seat with no user.
        }

        /// <summary>
        /// When input is dected on the Gameboard screen as a Pointer, these are the kinds of Pointers it can be.
        /// </summary>
        public enum PointerTypes
        {
            NONE = -1,
            Finger = 0, // Single finger on the board
            Blade = 64, // Blade of the hand on the board
        }

        /// <summary>
        /// The sides of the Gameboard screen.
        /// </summary>
        public enum ScreenSides
        {
            UNKNOWN = 0,
            Forward = 1,
            Back = 2,
            Left = 3,
            Right = 4,
        }

        /// <summary>
        /// The states that a queued event can be in. These are the events that go to Companions and the Gameboard.
        /// </summary>
        public enum EventQueueStates
        {
            WaitingToProcess = 0,
            Processing = 1,
            Cancelled = 2,
            TimedOut = 3,
            Completed = 4,
        }

        /// <summary>
        /// The types of devices used with the Gameboard SDK.
        /// </summary>
        public enum DeviceTypes
        {
            Gameboard = 0,
            Companion = 1,
        }
    }
}