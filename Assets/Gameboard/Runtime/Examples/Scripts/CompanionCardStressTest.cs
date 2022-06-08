using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CompanionCardStressTest : MonoBehaviour
{
    public List<Texture2D> cardImageList = new List<Texture2D>();

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
        string cardHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inPlayer.gameboardId);
        AddToLog("--- === Card Hand created with ID " + cardHandId + " for user " + inPlayer.gameboardId);

        // Create a card hand on the player
        List<CardDefinition> testCardList = new List<CardDefinition>();
        for (int i = 0; i < cardImageList.Count; i++)
        {
            byte[] textureBytes = cardImageList[i].EncodeToPNG();
            Debug.Log("----- CREATING CARD WITH IMAGE " + cardImageList[i].name);
            CardDefinition newCardDef = new CardDefinition(cardImageList[i].name, textureBytes, "", null, cardImageList[i].width, cardImageList[i].height);
            testCardList.Add(newCardDef);

            await CardsTool.singleton.GiveCardToPlayer(inPlayer.gameboardId, newCardDef);
        }

        for(int i = 0; i < 20; i++)
        {
            AddToLog($"--- === Entering card test pass {i + 1} for user {inPlayer.gameboardId}");

            foreach(CardDefinition thisCardDef in testCardList)
            {
                AddToLog($"--- === Begin add card {thisCardDef.cardGuid} to player hand {cardHandId} for user {inPlayer.gameboardId}");
                await CardsTool.singleton.PlaceCardInPlayerHand_Async(inPlayer.gameboardId, cardHandId, thisCardDef);
                AddToLog($"--- === Finished adding card {thisCardDef.cardGuid} to player hand {cardHandId} for user {inPlayer.gameboardId}");
            }

            Debug.Log("--- === All cards added to hand. Waiting 4 seconds then removing... for user " + inPlayer.gameboardId);
            await Task.Delay(4000);

            foreach (CardDefinition thisCardDef in testCardList)
            {
                AddToLog($"--- === Begin remove card {thisCardDef.cardGuid} to player hand {cardHandId} for user {inPlayer.gameboardId}");
                await CardsTool.singleton.RemoveCardFromPlayerHand_Async(inPlayer.gameboardId, cardHandId, thisCardDef);
                AddToLog($"--- === Finished removing card {thisCardDef.cardGuid} to player hand {cardHandId} for user {inPlayer.gameboardId}");
            }

            AddToLog($"--- Completed card test pass {i + 1} for user " + inPlayer.gameboardId);
            AddToLog("--------------------------------------------------------------------------------------");
            if(i < 19)
            {
                AddToLog("--- Waiting 4 seconds then performing another pass... for user " + inPlayer.gameboardId);
                await Task.Delay(4000);
            }
            else
            {
                AddToLog("--- All passes completed for user " + inPlayer.gameboardId);
            }
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

    class CompanionCardTestPlayer
    {
        public string gameboardId;
    }
}
