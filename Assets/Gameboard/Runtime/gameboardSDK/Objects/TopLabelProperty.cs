namespace Gameboard.Objects
{
    public struct TopLabelProperty
    {
        /// <summary>
        /// limited to 18 characters, sending an empty string will remove the top label
        /// </summary>
        public string label;

        //labelIconAssetId: string //phase 2 (not supported yet)
        //backgroundAssetId: string //phase 2 (not supported yet)
    }
}