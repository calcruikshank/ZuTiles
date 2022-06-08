using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ResetPlayPanelTest : MonoBehaviour
{
    public Texture2D buttonUpImage;
    public Texture2D buttonDownImage;
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

                AddToLog("--- ResetPlayPanelTest Setup Completed");

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

        if (presenceUpdates.Count > 0)
            Debug.Log("--- ResetPlayPanelTest found " + presenceUpdates.Count + " updates!");

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
                PerformTest(testPlayer);
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

    private async Task PerformTest(CompanionCardTestPlayer inPlayer)
    {
        // NOTE: Currently not awaiting the LoadAsset as the companion simulator doesn't respond for Asset loads.
        //CompanionCreateObjectEventArgs downImgEventArgs = await Gameboard.Gameboard.Instance.companionController.LoadAsset(inPlayer.gameboardId, buttonDownImage);

        AddToLog("--- Beginning setup for player " + inPlayer);

        await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "1", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.Instance.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "1", "Button A", "ButtonAPressed");
        AddToLog("--- Added Button A to " + inPlayer.gameboardId);

        await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "2", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.Instance.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "2", "Button B", "ButtonBPressed");
        AddToLog("--- Added Button B " + inPlayer.gameboardId);

        await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "3", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.Instance.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "3", "Button C", "ButtonCPressed");
        AddToLog("--- Added Button C " + inPlayer.gameboardId);

        string cardHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inPlayer.gameboardId);
        AddToLog("--- Card Hand created with ID " + cardHandId + " on " + inPlayer.gameboardId);

        List<CardDefinition> cardIdList = new List<CardDefinition>();
        for(int i = 0; i < cardImageList.Count; i++)
        {
            byte[] textureArray = cardImageList[i].EncodeToPNG();

            CardDefinition newCardDef = new CardDefinition("", textureArray, "", null, cardImageList[i].width, cardImageList[i].height);
            cardIdList.Add(newCardDef);

            await CardsTool.singleton.GiveCardToPlayer(inPlayer.gameboardId, newCardDef);
            await CardsTool.singleton.PlaceCardInPlayerHand_Async(inPlayer.gameboardId, cardHandId, newCardDef);
        }

        Debug.Log("--- Begin wait before wiping...");
        await Task.Delay(10);
        Debug.Log("--- Begin wipe!");

        await Gameboard.Gameboard.Instance.companionController.ResetPlayPanel(inPlayer.gameboardId);

        Debug.Log("--- Test finished! PlayPanel should now be wiped.");

        return;
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
