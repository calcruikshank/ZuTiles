using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AssetLoadPerformanceTest : MonoBehaviour
{
    public Texture2D imageLoadTest;

    private bool setupCompleted;
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
                CompanionCardTestPlayer testPlayer = new CompanionCardTestPlayer()
                {
                    gameboardId = eventArgs.userId
                };

                playerList.Add(testPlayer);

                AddToLog("--- === New player added: " + testPlayer.gameboardId);
                BeginCardTest(testPlayer);
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

    private async void BeginCardTest(CompanionCardTestPlayer inPlayer)
    {
        float totalProcessTimeStart = Time.time;

        float createCardStartTime = Time.time;
        string cardHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inPlayer.gameboardId);
        AddToLog("--- === Create Card Hand Time: " + (Time.time - createCardStartTime));

        AddToLog("--- Adding texture name " + imageLoadTest.name);

        float loadStartTime = Time.time;
        CardDefinition newCardDef = new CardDefinition(imageLoadTest.name, imageLoadTest.EncodeToPNG(), "", null, imageLoadTest.width, imageLoadTest.height);
        await CardsTool.singleton.GiveCardToPlayer(inPlayer.gameboardId, newCardDef);
        AddToLog("--- === Give Card to Player Time: " + (Time.time - loadStartTime));

        float placeCardTime = Time.time;
        await CardsTool.singleton.PlaceCardInPlayerHand_Async(inPlayer.gameboardId, cardHandId, newCardDef);
        AddToLog("--- Place card in hand time: " + (Time.time - placeCardTime));

        AddToLog("--- Total Processing Time: " + (Time.time - totalProcessTimeStart));
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
