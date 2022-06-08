namespace Gameboard.EventArgs
{
    public class EventArgSetConfiguration
    {
        /// <summary>
        /// CardProperty you want to update
        /// </summary>
        public int property;

        /// <summary>
        /// The value to be set for the specified ConfigurationProperty
        /// </summary>
        public object value;

        /// <summary>
        /// The Gameboard ID where this event originated
        /// </summary>
        public string ownerId;
    }

    public enum ConfigurationProperty
    {
        CardStartControlAssetId = 0, // value is a string
        CardEndControlAssetId = 1, // value is a string
        CardBackgroundAssetId = 2, // value is a string
        CardSelectedAssetId = 3, // value is a string
        CardTemplate = 4, // value is an int
        DiceBackgroundAssetId = 5, // value is a string
        DiceSelectorEnabled = 6, // value is a bool
        TopLabel = 7, // value is a string
    }
}