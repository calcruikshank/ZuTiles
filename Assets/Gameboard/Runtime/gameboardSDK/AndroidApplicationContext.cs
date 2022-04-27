using UnityEngine;

namespace Gameboard
{

    public class AndroidApplicationContext : IAndroidContext
    {

        AndroidJavaObject mContext = null;

        public AndroidApplicationContext()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                mContext = activity.Call<AndroidJavaObject>("getApplicationContext");
            }
        }

        public AndroidJavaObject GetNativeContext()
        {
            return mContext;
        }
    }
}
