using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CompanionChangeCardHandTest : MonoBehaviour
{
    public List<Texture2D> cardHandAImageList = new List<Texture2D>();
    public List<Texture2D> cardHandBImageList = new List<Texture2D>();

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
        // NOTE: Currently not awaiting the LoadAsset as the companion simulator doesn't respond for Asset loads.
        //CompanionCreateObjectEventArgs downImgEventArgs = await Gameboard.Gameboard.Instance.companionController.LoadAsset(inPlayer.gameboardId, buttonDownImage);

        await Gameboard.Gameboard.Instance.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "1", "Swap!", "SwapCardHands");
        await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "1", DataTypes.ObjectDisplayStates.Displayed);
        AddToLog("--- Added Button " + inPlayer.gameboardId);

        inPlayer.handAId = await CreateCardHand(inPlayer.gameboardId, cardHandAImageList);
        inPlayer.handBId = await CreateCardHand(inPlayer.gameboardId, cardHandBImageList);

        inPlayer.handAActive = false;
        SwapHandDisplays(inPlayer.gameboardId);
    }

    async Task<string> CreateCardHand(string inGameboardId, List<Texture2D> cardImageList)
    {
        string cardHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inGameboardId);
        AddToLog("--- Card Hand created with ID " + cardHandId + " on " + cardHandId);

        List<CardDefinition> cardHandIdList = new List<CardDefinition>();
        for (int i = 0; i < cardImageList.Count; i++)
        {
            byte[] textureArray = cardImageList[i].EncodeToPNG();

            CardDefinition newCardDef = new CardDefinition(cardImageList[i].name, textureArray, "", null, cardImageList[i].width, cardImageList[i].height);
            cardHandIdList.Add(newCardDef);

            await CardsTool.singleton.GiveCardToPlayer(inGameboardId, newCardDef);
            await CardsTool.singleton.PlaceCardInPlayerHand_Async(inGameboardId, cardHandId, newCardDef);
        }

        return cardHandId;
    }

    void CompanionButtonPressed(string inGameboardUserId, string inCallbackMethod)
    {
        AddToLog("--- Companion Button Pressed on user " + inGameboardUserId + " with callback: " + inCallbackMethod);
        DigestButtonPress(inGameboardUserId, inCallbackMethod);
    }

    void CardsButtonPressed(string inGameboardUserId, string inCallbackMethod, string inCardsId)
    {
        AddToLog("--- Cards Button Pressed on user " + inGameboardUserId + " with callback: " + inCallbackMethod + " and card ID " + inCardsId);
        DigestButtonPress(inGameboardUserId, inCallbackMethod);
    }

    void DigestButtonPress(string inPlayerId, string inCallbackMethod)
    {
        if(inCallbackMethod == "SwapCardHands")
        {
            SwapHandDisplays(inPlayerId);
        }
    }

    async void SwapHandDisplays(string inPlayerId)
    {
        CompanionCardTestPlayer testPlayer = playerList.Find(s => s.gameboardId == inPlayerId);

        if (testPlayer.handsAreSwapping)
        {
            Debug.Log("-- Hand Display Swap still occuring!");
            return;
        }

        Debug.Log("--- Beginning swap card hands!");

        testPlayer.handsAreSwapping = true;

        testPlayer.handAActive = !testPlayer.handAActive;
        string showHand = testPlayer.handAActive ? testPlayer.handAId : testPlayer.handBId;
        await CardsTool.singleton.ShowHandDisplay(inPlayerId, showHand);

        testPlayer.handsAreSwapping = false;
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
        public string handAId;
        public string handBId;
        public bool handAActive;
        public bool handsAreSwapping;
    }
}
