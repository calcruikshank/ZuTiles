using Gameboard.EventArgs;
using UnityEngine;

public class SampleUserPresenceConsumer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject presenceObserverObj = GameObject.FindWithTag("UserPresenceObserver");
        UserPresenceObserver userPresenceObserver = presenceObserverObj.GetComponent<UserPresenceObserver>();
        userPresenceObserver.OnUserPresence += OnUserPresence;
    }

    void OnUserPresence(GameboardUserPresenceEventArgs userPresence)
    {
        // Handle User Presence here.
    }

}