using UnityEngine;

namespace Gameboard.EventArgs
{
    public class GameboardCompanionCardsButtonPressedEventArgs : GameboardCompanionButtonPressedEventArgs
    {
        public string selectedCardId;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}