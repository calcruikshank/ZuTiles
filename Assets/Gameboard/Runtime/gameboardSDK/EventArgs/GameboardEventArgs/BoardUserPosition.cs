using Gameboard.Helpers;
using System;
using UnityEngine;

namespace Gameboard.EventArgs
{
    public class BoardUserPosition
    {
        /// <summary>
        /// X position on the Gameboard screen.
        /// </summary>
        public float x;

        /// <summary>
        /// Y position on the Gameboard screen.
        /// </summary>
        public float y;

        /// <summary>
        /// What coordinate system (where the 0,0 point is) that this position is using (TOP_LEFT, BOTTOM_LEFT).
        /// </summary>
        public string coordinateSystem;

        /// <summary>
        /// Converted enum value for the coordinateSystem string.
        /// </summary>
        public DataTypes.GameboardScreenCoordinateSystem coordinateSystemEnum { get { return GameboardHelperMethods.GetEnumValueFromString<DataTypes.GameboardScreenCoordinateSystem>(coordinateSystem); } }

        /// <summary>
        /// The Vector2 version of the Gameboard screen x/y position.
        /// </summary>
        public Vector2 screenPosition { get { return new Vector2(x, y); } }
    }
}