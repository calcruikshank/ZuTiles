using UnityEngine;

namespace Gameboard.EventArgs
{
    public class CompanionCardPlayedEventArgs : CompanionCommunicationsEventArg
    {
        public string cardId;
        public string userId;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}