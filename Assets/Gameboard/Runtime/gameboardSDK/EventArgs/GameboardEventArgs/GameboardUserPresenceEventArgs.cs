using Gameboard.Helpers;
using UnityEngine;

namespace Gameboard.EventArgs
{
    public class GameboardUserPresenceEventArgs : GameboardIncomingEventArg
    {
        /// <summary>
        /// Identifies this presence as a unique NUID value.
        /// </summary>
        public string id;

        /// <summary>
        /// Gameboard UserID
        /// </summary>
        public string userId;

        /// <summary>
        /// Timestamp when this user arrived
        /// </summary>
        public string startStamp;

        /// <summary>
        /// Timestamp when this user left.
        /// </summary>
        public string endStamp;

        /// <summary>
        /// The type of presence that this refers to. Use presenceTypeValue to get the enum version. (Drawer, Companion, None)
        /// </summary>
        public string presence;

        /// <summary>
        /// This is the ID of the Drawer on the Gameboard that this player is connected through.
        /// </summary>
        public string seatKey;

        /// <summary>
        /// What this change type is, as a string. Use changeValue to get the enum version. (Add, Remove, Change, Change_Position)
        /// </summary>
        public string change;

        /// <summary>
        /// The event arguments of the previous change.
        /// </summary>
        public GameboardUserPresenceEventArgs changePrevious;
        
        /// <summary>
        /// The Username of this player. Can also be acquired with the property userName.
        /// </summary>
        public string display;

        /// <summary>
        /// What type of user this is. User userTypeValue to get the enum version. (User, Guest, Open)
        /// </summary>
        public string type;

        /// <summary>
        /// The ID of the Companion that this user is attached to, if they are using a Companion.
        /// </summary>
        public string companionId;

        /// <summary>
        /// The raw HTML Color value assigned to this player. Use unityColor to acquire the Unity Color converted version of this.
        /// </summary>
        public string tokenColor;

        /// <summary>
        /// The position that the drawer assocaited with this player is currently located on the Gameboard screen.
        /// </summary>
        public BoardUserPosition boardUserPosition;

        /// <summary>
        /// Helper property to get the userName, as it's called 'display' which isn't extremely end-user clear.
        /// </summary>
        public string userName => display;

        /// <summary>
        /// The tokenColor, converted to the Unity Color format.
        /// </summary>
        public Color unityColor
        {
            get
            {
                Color returnCol = Color.white;
                ColorUtility.TryParseHtmlString(tokenColor, out returnCol);
                return returnCol;
            }
        }

        /// <summary>
        /// Returns the UserPresenceChangeTypes value for the change string received from the Companion.
        /// </summary>
        public DataTypes.UserPresenceChangeTypes changeValue
        {
            get 
            {
                if (string.IsNullOrEmpty(change))
                {
                    return DataTypes.UserPresenceChangeTypes.UNKNOWN;
                }
                else
                {
                    return GameboardHelperMethods.GetEnumValueFromString<DataTypes.UserPresenceChangeTypes>(change);
                }
            }
        }

        /// <summary>
        /// Returns the PresenceType value for the presence string received from the Companion. (Drawer, Companion, None).
        /// </summary>
        public DataTypes.PresenceType presenceTypeValue
        {
            get
            {
                if (string.IsNullOrEmpty(presence))
                {
                    return DataTypes.PresenceType.NONE;
                }
                else
                {
                    return GameboardHelperMethods.GetEnumValueFromString<DataTypes.PresenceType>(presence);
                }
            }
        }

        /// <summary>
        /// Returns the UserType value for the type string received from the Companion. (None, User, Guest, Open).
        /// </summary>
        public DataTypes.UserType userTypeValue
        {
            get
            {
                if (string.IsNullOrEmpty(type))
                {
                    return DataTypes.UserType.NONE;
                }
                else
                {
                    return GameboardHelperMethods.GetEnumValueFromString<DataTypes.UserType>(type);
                }
            }
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}