using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EventTimeoutTest : MonoBehaviour
{
    private bool setupCompleted;
    private bool waitingForResponse;
    private bool lastDidShow;
    private List<CompanionCardTestPlayer> playerList = new List<CompanionCardTestPlayer>();

    private List<string> onScreenLog = new List<string>();

    // Update is called once per frame
    void Update()
    {
        if (!setupCompleted)
        {
            if (Gameboard.Gameboard.Instance != null &&
               CardsTool.singleton != null &&
               CompanionAssetsTool.singleton != null &&
               UserPresenceTool.singleton != null &&
               CompanionTemplateTool.singleton != null)
            {
                setupCompleted = true;

                AddToLog("Setup Completed");

                CompanionTemplateTool.singleton.ButtonPressed += CompanionButtonPressed;
                CompanionTemplateTool.singleton.CardsButtonPressed += CardsButtonPressed;
                UserPresenceTool.singleton.RequestUserPresenceUpdate();
            }
        }
        else
        {
            if(!waitingForResponse && playerList.Count > 0)
            {
                lastDidShow = !lastDidShow;
                PerformTest(playerList[0], lastDidShow);
            }

            UpdateWithReceivedUserPresences();
        }
    }

    private void UpdateWithReceivedUserPresences()
    {
        Queue<GameboardUserPresenceEventArgs> presenceUpdates = UserPresenceTool.singleton.DrainQueue();

        while (presenceUpdates.Count > 0)
        {
            GameboardUserPresenceEventArgs eventArgs = presenceUpdates.Dequeue();

            CompanionCardTestPlayer testPlayer = playerList.Find(s => s.gameboardId == eventArgs.userId);
            if (testPlayer == null)
            {
                testPlayer = new CompanionCardTestPlayer()
                {
                    gameboardId = eventArgs.userId,
                };

                playerList.Add(testPlayer);

                AddToLog("--- New player added: " + testPlayer.gameboardId);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                PerformTest(testPlayer, true);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

    private async void PerformTest(CompanionCardTestPlayer inPlayer, bool doShow)
    {
        waitingForResponse = true;

        Debug.Log("--- Change button display show to " + doShow);
        CompanionMessageResponseArgs responseArgs = await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "1", doShow ? DataTypes.ObjectDisplayStates.Displayed : DataTypes.ObjectDisplayStates.Hidden);

        if (!responseArgs.wasSuccessful)
        {
            AddToLog("--- Response failed: " + responseArgs.errorResponse);
        }
        else
        {
            Debug.Log("--- Finished change button display show to " + doShow);

            await Task.Delay(2000);

            Debug.Log("--- Ready to queue next event...");
            waitingForResponse = false;
        }
    }

    void CompanionButtonPressed(string inGameboardUserId, string inCallbackMethod)
    {
        AddToLog("--- Companion Button Pressed with callback: " + inCallbackMethod);
    }

    void CardsButtonPressed(string inGameboardUserId, string inCallbackMethod, string inCardsId)
    {
        AddToLog("--- Cards Button Pressed with callback: " + inCallbackMethod + " and card ID " + inCardsId);
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

    class CompanionCardTestPlayer
    {
        public string gameboardId;
    }
}
