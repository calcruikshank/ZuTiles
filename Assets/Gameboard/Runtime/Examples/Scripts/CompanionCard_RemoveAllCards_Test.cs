using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using UnityEngine;

public class CompanionCard_RemoveAllCards_Test : MonoBehaviour
{
    public Texture2D buttonUpImage;
    public Texture2D buttonDownImage;
    public List<Texture2D> cardImageList = new List<Texture2D>();

    private bool setupCompleted;
    private List<CompanionCardTestPlayer> playerList = new List<CompanionCardTestPlayer>();
    private List<CardDefinition> testCardDefinition = new List<CardDefinition>();

    private List<string> onScreenLog = new List<string>();

    private string testPlayerId;
    private string testPlayerHandId;

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

                AddToLog("--- New player added: " + testPlayer.gameboardId);
                AddButtonsToPlayer(testPlayer);
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

    private async void AddButtonsToPlayer(CompanionCardTestPlayer inPlayer)
    {
        testPlayerId = inPlayer.gameboardId;

        byte[] buttonUpImageBytes = buttonUpImage.EncodeToPNG();

        // NOTE: Currently not awaiting the LoadAsset as the companion simulator doesn't respond for Asset loads.
        CompanionCreateObjectEventArgs upImgEventArgs = await Gameboard.Gameboard.Instance.companionController.LoadAsset(inPlayer.gameboardId, buttonUpImageBytes, null);
        //CompanionCreateObjectEventArgs downImgEventArgs = await Gameboard.Gameboard.Instance.companionController.LoadAsset(inPlayer.gameboardId, buttonDownImage);

        await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "1", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.Instance.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "1", "Select", "SelectCard");
        AddToLog("--- Added Button A");

        await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "2", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.Instance.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "2", "Remove Selected", "RemoveSelected");
        AddToLog("--- Added Button B");

        await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "3", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.Instance.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "3", "Remove All", "RemoveAllCards");
        AddToLog("--- Added Button C");

        testPlayerHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inPlayer.gameboardId);
        AddToLog("--- Card Hand created with ID " + testPlayerHandId);

        for(int i = 0; i < cardImageList.Count; i++)
        {
            byte[] imageByteArray = cardImageList[i].EncodeToPNG();

            CardDefinition newCardDef = new CardDefinition("", imageByteArray, "", null, cardImageList[i].width, cardImageList[i].height);
            testCardDefinition.Add(newCardDef);

            await CardsTool.singleton.GiveCardToPlayer(inPlayer.gameboardId, newCardDef);
            await CardsTool.singleton.PlaceCardInPlayerHand_Async(inPlayer.gameboardId, testPlayerHandId, newCardDef);
        }
    }

    void CompanionButtonPressed(string inGameboardUserId, string inCallbackMethod)
    {
        if(inCallbackMethod == "RemoveAllCards")
        {
            RemoveEverything();
        }
    }

    void CardsButtonPressed(string inGameboardUserId, string inCallbackMethod, string inCardsId)
    {
        if (inCallbackMethod == "SelectCard")
        {
            AddToLog("--- Select Card pressed for card " + inCardsId);
        }
        else if(inCallbackMethod == "RemoveSelected")
        {
            CardDefinition cardDef = testCardDefinition.Find(s => s.cardGuid == inCardsId);
            CardsTool.singleton.RemoveCardFromPlayerHand(testPlayerId, testPlayerHandId, cardDef);
        }
    }

    void AddToLog(string logMessage)
    {
        onScreenLog.Add(logMessage);
        Debug.Log(logMessage);
    }

    async void RemoveEverything()
    {
        for (int i = 1; i <= 3; i++)
        {
            await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(testPlayerId, i.ToString(), DataTypes.ObjectDisplayStates.Hidden);
        }

        await CardsTool.singleton.RemoveAllCardsFromPlayerHand_Async(testPlayerId, testPlayerHandId);
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
