using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Examples;
using Gameboard.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UserPresenceTest : MonoBehaviour
{
    public List<PlayerPresenceDrawer> playerList = new List<PlayerPresenceDrawer>();
    private List<string> onScreenLog = new List<string>();
    [SerializeField]GameObject playerPresenceSceneObject;

    public static UserPresenceTest singleton;


    List<Texture2D> cardImageList = new List<Texture2D>();

    void Start()
    {
        if (singleton != null)
        {
            Destroy(this);
        }
        singleton = this;
        GameObject presenceObserverObj = GameObject.FindWithTag("UserPresenceObserver");
        UserPresenceObserver userPresenceObserver = presenceObserverObj.GetComponent<UserPresenceObserver>();
        userPresenceObserver.OnUserPresence += OnUserPresence;
    }

    void OnUserPresence(GameboardUserPresenceEventArgs userPresence)
    {
        PlayerPresenceDrawer myObject = playerList.Find(s => s.userId == userPresence.userId);
        if (myObject == null)
        {
            Debug.Log("my object is not null");
            // Add it here, and when adding also populate myObject
            // If the user doesn't exist in our player list, add them now.
            if (playerList.Find(s => s.userId == userPresence.userId) == null)
            {
                Debug.Log(userPresence.userId + " user presence id");
                /*UserPresencePlayer testPlayer = new UserPresencePlayer()
                {
                    gameboardId = userPresence.userId
                };*/

                GameObject scenePrefab = Instantiate(playerPresenceSceneObject, userPresence.boardUserPosition.screenPosition, Quaternion.identity);
               

                myObject = scenePrefab.GetComponent<PlayerPresenceDrawer>();
                myObject.InjectDependencies(userPresence);


                //this checks if the game is zu tiles and if it is then adds a zu tile player script to myobject has to be a better way to do this
                if (ZuTilesSetup.singleton != null)
                {
                    ZuTilesSetup.singleton.AddZuTilePlayer(myObject);
                }

                if (!string.IsNullOrEmpty(userPresence.userId))
                {
                    myObject.InjectUserId(userPresence.userId);
                }


                playerList.Add(myObject);
                AddToLog("--- === New player added: " + userPresence.userId);
            }

        }
        if (myObject != null)
        {
            myObject.UpdateUserPresence(userPresence);
        }
        
    }


    void AddToLog(string logMessage)
    {
        onScreenLog.Add(logMessage);
        Debug.Log(logMessage);
    }

    void OnGUI()
    {
        foreach (string thisString in onScreenLog)
        {
            GUILayout.Label(thisString);
        }
    }
    public async void AddButtonsToPlayer(PlayerPresenceDrawer inPlayer, GameObject deckToGivePlayer)
    {
        // NOTE: Currently not awaiting the LoadAsset as the companion simulator doesn't respond for Asset loads.
        //CompanionCreateObjectEventArgs downImgEventArgs = await Gameboard.Gameboard.singleton.companionController.LoadAsset(inPlayer.gameboardId, buttonDownImage);

        await Gameboard.Gameboard.singleton.companionController.ChangeObjectDisplayState(inPlayer.userId, "1", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.singleton.companionController.SetCompanionButtonValues(inPlayer.userId, "1", "Button A", "ButtonAPressed");
        AddToLog("--- Added Button A to " + inPlayer.userId);

        await Gameboard.Gameboard.singleton.companionController.ChangeObjectDisplayState(inPlayer.userId, "2", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.singleton.companionController.SetCompanionButtonValues(inPlayer.userId, "2", "Button B", "ButtonBPressed");
        AddToLog("--- Added Button B " + inPlayer.userId);

        await Gameboard.Gameboard.singleton.companionController.ChangeObjectDisplayState(inPlayer.userId, "3", DataTypes.ObjectDisplayStates.Displayed);
        await Gameboard.Gameboard.singleton.companionController.SetCompanionButtonValues(inPlayer.userId, "3", "Button C", "ButtonCPressed");
        AddToLog("--- Added Button C " + inPlayer.userId);

        string cardHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inPlayer.userId);
        await CardsTool.singleton.ShowHandDisplay(inPlayer.userId, cardHandId);
        AddToLog("--- Card Hand created with ID " + cardHandId + " on " + inPlayer.userId);

        cardImageList.Clear();
        for (int i = 0; i < deckToGivePlayer.GetComponent<Deck>().cardsInDeck.Count; i++)
        {
            cardImageList.Add((Texture2D)deckToGivePlayer.GetComponent<Deck>().cardsInDeck[i].GetComponentInChildren<Renderer>().material.mainTexture);
        }

        List<CardDefinition> cardIdList = new List<CardDefinition>();
        for (int i = 0; i < cardImageList.Count; i++)
        {
            Debug.Log(cardImageList[i]);
            byte[] textureArray = DeCompress(cardImageList[i]).EncodeToPNG();

            CardDefinition newCardDef = new CardDefinition(cardImageList[i].name, textureArray, "", null, cardImageList[i].width / 2, cardImageList[i].height / 2);
            Debug.Log(cardImageList[i].width / 2);
            cardIdList.Add(newCardDef);

            await CardsTool.singleton.GiveCardToPlayer(inPlayer.userId, newCardDef);
        }
    }
    public Texture2D DeCompress(Texture2D source)
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


}
