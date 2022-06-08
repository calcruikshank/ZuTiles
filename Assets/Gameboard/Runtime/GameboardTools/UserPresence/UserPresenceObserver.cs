using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using UnityEngine;

public class UserPresenceObserver : MonoBehaviour
{
    public delegate void OnUserPresenceHandler(GameboardUserPresenceEventArgs userPresence);
    public event OnUserPresenceHandler OnUserPresence;

    private bool noRequestMade;

    void Start()
    {
        noRequestMade = true;
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(Gameboard.Gameboard.Instance is null)
        {
            Debug.LogWarning("Gameboard controller is uninitialized. Cannot handle player presence updates.");
            return;
        }

        if (UserPresenceTool.singleton is null)
        {
            Debug.LogWarning("User presence tool is uninitialized. Cannot handle player presence updates.");
            return;
        }

        // Acquire any presence updates that the UserPresenceTool has acquired since our last update. This will also
        // clear the list on the UserPresenceTool, so any new presence updates it receives are entirely new.
        Queue<GameboardUserPresenceEventArgs> drainedQueue = UserPresenceTool.singleton.DrainQueue();
        while (drainedQueue.Count > 0)
        {
            GameboardUserPresenceEventArgs eventArg = drainedQueue.Dequeue();
            OnUserPresence?.Invoke(eventArg);
        }

        // Make sure we always do one initial request to get the starting setup.
        if(noRequestMade)
        {
            UserPresenceTool.singleton.playerPresenceRequestActive = false;
            UserPresenceTool.singleton.RequestUserPresenceUpdate();
            noRequestMade = false;
        }
    }
}