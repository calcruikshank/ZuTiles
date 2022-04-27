using UnityEngine;

namespace Gameboard
{
    public interface IAndroidContext
    {
        AndroidJavaObject GetNativeContext();
    }
}
