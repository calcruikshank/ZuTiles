namespace Gameboard.EventArgs
{
    public enum CardProperty
    {
        Highlight = 0,
        FrontAssetId = 1,
        BackAssetId = 2,
    }

    public class EventArgsSetCardProperty
    {
        /// <summary>
        /// CardProperty you want to update
        /// </summary>
        public int property;

        /// <summary>
        /// The value to be set for the specified property, this would either be a CardHighlight or a CardAssetId
        /// </summary>
        public object value;

        /// <summary>
        /// The Gameboard ID where this event originated
        /// </summary>
        public string ownerId;
    }

    public struct CardHighlights
    {
        /// <summary>
        /// List of cardIds to highlight
        /// </summary>
        public string[] cardIds;

        /// <summary>
        /// If true, will apply to all cards
        /// </summary>
        public bool all;

        /// <summary>
        /// The color value assigned to this highlight.
        /// </summary>
        /// <remarks>
        /// The following formats are accepted:
        /// HEX - "#ff0000"
        /// RGB - "rgb(255, 0, 0)" or "rgb(100%, 0%, 0%)"
        /// HSL - "hsl(0, 100%, 50%)"
        /// X11 color name in lowercase - "red"
        /// You can easily specify the hex format with Unity's ColorUtility:
        ///     $"#{UnityEngine.ColorUtility.ToHtmlStringRGB(UnityEngine.Color.green)}"
        /// </remarks>
        public string color;

        /// <summary>
        /// Asset to place ontop of card as highlight asset
        /// </summary>
        public string assetId;

        /// <summary>
        /// True to add highlight, false to remove
        /// </summary>
        public bool enabled;
    }

    public struct CardAssetId
    {
        /// <summary>
        /// CardId to change assetId
        /// </summary>
        public string cardId;

        /// <summary>
        /// Asset to set
        /// </summary>
        public string assetId;
    }

}
