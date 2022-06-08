using UnityEngine;

namespace Gameboard.EventArgs
{
    public class CompanionCreateObjectEventArgs : CompanionMessageResponseArgs
    {
        public string newObjectUid;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}