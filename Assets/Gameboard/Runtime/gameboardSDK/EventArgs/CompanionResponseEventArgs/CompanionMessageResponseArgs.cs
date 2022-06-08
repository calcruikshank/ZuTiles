using UnityEngine;

namespace Gameboard.EventArgs
{
    public class CompanionMessageResponseArgs
    {
        // These come in from the Companion Server
        public int versionTag = 1;
        public int errorId = 0;

        // These are locally defined
        public bool wasSuccessful { get { return errorId == 0 && errorResponse == null; } }
        public CompanionErrorResponse errorResponse;
        public string ownerId;

        // NOTE: userId is a placeholder item to cover some edge cases in the companion. These are all being turned into ownerId, and this will get removed.
        public string userId;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}