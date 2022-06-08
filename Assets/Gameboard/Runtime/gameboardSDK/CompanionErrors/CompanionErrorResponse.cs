using UnityEngine;

namespace Gameboard.EventArgs
{
    public class CompanionErrorResponse
    {
        public System.Enum ErrorValue;
        public string Message;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}