using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCompanionHandler : MonoBehaviour
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
            if (Gameboard.Gameboard.singleton != null &&
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
        //CompanionCreateObjectEventArgs downImgEventArgs = await Gameboard.Gameboard.singleton.companionController.LoadAsset(inPlayer.gameboardId, buttonDownImage);

        await Gameboard.Gameboard.singleton.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "1", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.singleton.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "1", "Button A", "ButtonAPressed");
        AddToLog("--- Added Button A to " + inPlayer.gameboardId);

        await Gameboard.Gameboard.singleton.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "2", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.singleton.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "2", "Button B", "ButtonBPressed");
        AddToLog("--- Added Button B " + inPlayer.gameboardId);

        await Gameboard.Gameboard.singleton.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "3", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.singleton.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "3", "Button C", "ButtonCPressed");
        AddToLog("--- Added Button C " + inPlayer.gameboardId);

        string cardHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inPlayer.gameboardId);
        AddToLog("--- Card Hand created with ID " + cardHandId + " on " + inPlayer.gameboardId);

        List<CardDefinition> cardIdList = new List<CardDefinition>();
        for (int i = 0; i < cardImageList.Count; i++)
        {
            byte[] textureArray = DeCompress(cardImageList[i]).EncodeToPNG();

            CardDefinition newCardDef = new CardDefinition(cardImageList[i].name, textureArray, "", null, cardImageList[i].width / 2, cardImageList[i].height / 2);
            cardIdList.Add(newCardDef);

            await CardsTool.singleton.GiveCardToPlayer(inPlayer.gameboardId, newCardDef);
            await CardsTool.singleton.PlaceCardInPlayerHand_Async(inPlayer.gameboardId, cardHandId, newCardDef);
        }
    }
    public  Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
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
        foreach (string thisString in onScreenLog)
        {
            GUILayout.Label(thisString);
        }
    }

    class CompanionCardTestPlayer
    {
        public string gameboardId;
    }
}
