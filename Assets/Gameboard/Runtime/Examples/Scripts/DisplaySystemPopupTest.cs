using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DisplaySystemPopupTest : MonoBehaviour
{
    private bool setupCompleted;
    private List<PopupTestUser> playerList = new List<PopupTestUser>();

    private List<string> onScreenLog = new List<string>();

    // Update is called once per frame
    void Update()
    {
        if (!setupCompleted)
        {
            if (Gameboard.Gameboard.Instance != null && UserPresenceTool.singleton != null)
            {
                setupCompleted = true;

                AddToLog("Setup Completed");

                UserPresenceTool.singleton.RequestUserPresenceUpdate();
            }
        }
        else
        {
            UpdateWithReceivedUserPresences();
        }
    }

    private void UpdateWithReceivedUserPresences()
    {
        Queue<GameboardUserPresenceEventArgs> presenceUpdates = UserPresenceTool.singleton.DrainQueue();

        while (presenceUpdates.Count > 0)
        {
            GameboardUserPresenceEventArgs eventArgs = presenceUpdates.Dequeue();

            if (playerList.Find(s => s.gameboardId == eventArgs.userId) == null)
            {
                PopupTestUser testPlayer = new PopupTestUser()
                {
                    gameboardId = eventArgs.userId
                };

                playerList.Add(testPlayer);

                AddToLog("--- New player added: " + testPlayer.gameboardId);
                PerformPopupTests(testPlayer);
            }

            switch (eventArgs.changeValue)
            {
                case Gameboard.DataTypes.UserPresenceChangeTypes.UNKNOWN:
                    break;
                case Gameboard.DataTypes.UserPresenceChangeTypes.ADD:
                    break;
                case Gameboard.DataTypes.UserPresenceChangeTypes.REMOVE:
                    break;
                case Gameboard.DataTypes.UserPresenceChangeTypes.CHANGE:
                    break;
                case Gameboard.DataTypes.UserPresenceChangeTypes.CHANGE_POSITION:
                    break;
            }
        }
    }

    private async void PerformPopupTests(PopupTestUser inPlayer)
    {
        AddToLog("Performing 1 second popup.");
        CompanionMessageResponseArgs responseArgs = await Gameboard.Gameboard.Instance.companionController.DisplaySystemPopup(inPlayer.gameboardId, "This is a 1 second test.", 1f);
        AddResultToLog(responseArgs);
        await Task.Delay(2000);

        AddToLog("Performing 3 second popup.");
        responseArgs = await Gameboard.Gameboard.Instance.companionController.DisplaySystemPopup(inPlayer.gameboardId, "This is a 3 second test.", 3f);
        AddResultToLog(responseArgs);
        await Task.Delay(4000);

        AddToLog("Performing 5 second popup.");
        responseArgs = await Gameboard.Gameboard.Instance.companionController.DisplaySystemPopup(inPlayer.gameboardId, "This is a 5 second test.", 5f);
        AddResultToLog(responseArgs);
        await Task.Delay(6000);

        AddToLog("Performing 10 second popup.");
        responseArgs = await Gameboard.Gameboard.Instance.companionController.DisplaySystemPopup(inPlayer.gameboardId, "This is a 10 second test.", 10f);
        AddResultToLog(responseArgs);
        await Task.Delay(11000);

        AddToLog("All tests have completed.");
    }

    private void AddResultToLog(CompanionMessageResponseArgs inArgs)
    {
        if(inArgs.wasSuccessful)
        {
            AddToLog("--- Test was successful.");
        }
        else
        {
            AddToLog("Test failed. ErrorID: " + inArgs.errorId + " / Error Message: " + inArgs.errorResponse.Message);
        }
    }

    
    void AddToLog(string logMessage)
    {
        onScreenLog.Add(logMessage);
        Debug.Log(logMessage);
    }

    private void OnGUI()
    {
        foreach(string thisString in onScreenLog)
        {
            GUILayout.Label(thisString);
        }
    }

    class PopupTestUser
    {
        public string gameboardId;
    }
}
