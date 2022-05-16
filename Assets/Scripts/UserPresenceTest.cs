using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Examples;
using Gameboard.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{

    public class UserPresenceTest : MonoBehaviour
    {
        public List<PlayerPresenceDrawer> playerList = new List<PlayerPresenceDrawer>();
        private List<string> onScreenLog = new List<string>();
        [SerializeField] GameObject playerPresenceSceneObject;

        public static UserPresenceTest singleton;
        SetStencilReference setStencilReference;

        public List<CardDefinition> cardIdList;
        List<Texture2D> cardImageList = new List<Texture2D>();
        private float cachedTime; private string resolveOnUpdate;
        bool setupComplete = false;
        void Start()
        {
            if (singleton != null)
            {
                Destroy(this);
            }
            singleton = this;
            setStencilReference = FindObjectOfType<SetStencilReference>();
            GameObject presenceObserverObj = GameObject.FindWithTag("UserPresenceObserver");
            UserPresenceObserver userPresenceObserver = presenceObserverObj.GetComponent<UserPresenceObserver>();
            userPresenceObserver.OnUserPresence += OnUserPresence;
            //CompanionTemplateTool.singleton.ButtonPressed += CompanionButtonPressed;

        }


        private void Update()
        {
            cachedTime = Time.time;
            if (Gameboard.singleton == null)
            {
                return;
            }

            // Do the setup here in Update so we can just do a Singleton lookup on Gameboard, and not worry about race-conditions in using Start.
            if (!setupComplete)
            {

                setupComplete = true;

                //Gameboard.singleton.companionController.CompanionCardsButtonPressed += CardsButtonPressed;

            }
            if (!string.IsNullOrEmpty(resolveOnUpdate))
            {
                ResolveButtonPress(resolveOnUpdate);
                resolveOnUpdate = "";
            }
        }

        void OnUserPresence(GameboardUserPresenceEventArgs userPresence)
        {
            PlayerPresenceDrawer myObject = playerList.Find(s => s.userId == userPresence.userId);
            if (myObject == null)
            {
                // Add it here, and when adding also populate myObject
                // If the user doesn't exist in our player list, add them now.
                if (playerList.Find(s => s.userId == userPresence.userId) == null)
                {
                    /*UserPresencePlayer testPlayer = new UserPresencePlayer()
                    {
                        gameboardId = userPresence.userId
                    };*/

                    GameObject scenePrefab = Instantiate(playerPresenceSceneObject, userPresence.boardUserPosition.screenPosition, Quaternion.identity);

                    myObject = scenePrefab.GetComponent<PlayerPresenceDrawer>();
                    myObject.InjectDependencies(userPresence);

                    //setStencilReference.hideObjectsWalls.Add(myObject.GetComponentInChildren<TransparentShader>().gameObject);


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

        internal void RemoveCardFromUser(string userId, CardDefinition selectedCard)
        {
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
            await Gameboard.singleton.companionController.SetCompanionButtonValues(inPlayer.userId, "1", "Play Card", "ButtonAPressed");

            await Gameboard.singleton.companionController.ChangeObjectDisplayState(inPlayer.userId, "1", DataTypes.ObjectDisplayStates.Displayed);
           
            string cardHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inPlayer.userId);
            await CardsTool.singleton.ShowHandDisplay(inPlayer.userId, cardHandId);
            AddToLog("--- Card Hand created with ID " + cardHandId + " on " + inPlayer.userId);

            cardImageList.Clear();
            for (int i = 0; i < deckToGivePlayer.GetComponent<Deck>().cardsInDeck.Count; i++)
            {
                cardImageList.Add((Texture2D)deckToGivePlayer.GetComponent<Deck>().cardsInDeck[i].GetComponentInChildren<Renderer>().material.mainTexture);
            }

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

       

        
        private void ResolveButtonPress(string inCallbackMethod)
        {
            Debug.Log("ResolveButtonPress for " + inCallbackMethod);
            if (inCallbackMethod == "ButtonAPressed")
            {
                Debug.Log("Button A Pressed");
            }
        }
       
    }

}
