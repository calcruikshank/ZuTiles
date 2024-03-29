using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Examples;
using Gameboard.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameboard
{

    public class UserPresenceGameObjectController : MonoBehaviour
    {
        public List<PlayerPresenceDrawer> playerList = new List<PlayerPresenceDrawer>();
        private List<string> onScreenLog = new List<string>();
        [SerializeField] GameObject playerPresenceSceneObject;

        public static UserPresenceGameObjectController singleton;
        SetStencilReference setStencilReference;

        public List<CompanionTextureAsset> cardIdList;
        public List<string> cardImageList = new List<string>();
        private float cachedTime; private string resolveOnUpdate;

        UserPresenceController userPresenceController;
        CardController cardController;
        AssetController assetController;


        bool setupComplete = false;
        private void Awake()
        {
            if (singleton != null)
            {
                Destroy(this);
            }
            singleton = this;
            setStencilReference = FindObjectOfType<SetStencilReference>();
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            cardController = gameboardObject.GetComponent<CardController>();
            assetController = gameboardObject.GetComponent<AssetController>();
        }
        void Start()
        {
            cardController.CardPlayed += OnCardPlayed;
            userPresenceController.OnUserPresence += OnUserPresence;
            GetUP();
        }


        async void GetUP()
        {
            if (userPresenceController != null)
            {
                CompanionUserPresenceEventArgs compasa = await userPresenceController.GetCompanionUserPresence();
                foreach (GameboardUserPresenceEventArgs user in compasa.playerPresenceList)
                {
                    OnUserPresence(user);
                }
            }
            
        }

        internal async Task SetCardBackgroundAsync(PlayerPresenceDrawer playerPresenceDrawer, ControlAssetType cat, Guid thisBackgoundGUID)
        {
            //await cardController.SetCardControlAsset(playerPresenceDrawer.userId, ControlAssetType.CardBackgroundAssetId, thisBackgoundGUID.ToString());
            //first make a companion texture asset
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
            Debug.LogError("On  user presence " + userPresence.userId);
            PlayerPresenceDrawer myObject = playerList.Find(s => s.userId == userPresence.userId);
            lock (playerList)
            {
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

                        GameObject scenePrefab = Instantiate(playerPresenceSceneObject);

                        myObject = scenePrefab.GetComponent<PlayerPresenceDrawer>();
                        //myObject.transform.position = new Vector3( userPresence.boardUserPosition.x);
                        myObject.InjectDependencies(userPresence);

                        //myObject.UpdatePlayerPositionOnStart(userPresence.boardUserPosition.screenPosition);

                        CreateCardHandOnPlayersAsync(myObject);
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
                        Debug.Log("--- === New player added: " + userPresence.userId);
                    }

                }
                if (myObject != null)
                {
                    myObject.UpdateUserPresence(userPresence);
                }

            }
            

        }


        

        public async void CreateCardHandOnPlayersAsync(PlayerPresenceDrawer inPlayer)
        {
            string cardHandId = (await cardController.CreateCompanionHandDisplay(inPlayer.userId)).newObjectUid;
            inPlayer.CurrentActiveHandID = cardHandId;
            //await CardsTool.singleton.ShowHandDisplay(inPlayer.userId, cardHandId);
            
            await cardController.ShowCompanionHandDisplay(inPlayer.userId, cardHandId);

        }
       

        void AddToLog(string logMessage)
        {
           // onScreenLog.Add(logMessage);
            Debug.Log(logMessage);
        }

        void OnGUI()
        {
            foreach (string thisString in onScreenLog)
            {
                GUILayout.Label(thisString);
            }
        }

        public async void AddButtonsToPlayer(PlayerPresenceDrawer inPlayer)
        {
            await  Gameboard.singleton.companionController.SetCompanionButtonValues(inPlayer.userId, "1", "Play Card", "ButtonAPressed");

            await Gameboard.singleton.companionController.ChangeObjectDisplayState(inPlayer.userId, "1", DataTypes.ObjectDisplayStates.Displayed);
        }

        public async void AddCardsToPlayer(PlayerPresenceDrawer inPlayer, GameObject deckToGivePlayer)
        {
            /*for (int i = 0; i < deckToGivePlayer.GetComponent<Card>().cardsInDeck.Count; i++)
            {
                cardImageList.Add((Texture2D)deckToGivePlayer.GetComponent<Card>().cardsInDeck[i].GetComponentInChildren<Renderer>().material.mainTexture);
            }
            cardImageList.Clear();


            for (int i = 0; i < cardImageList.Count; i++)
            {
                Debug.Log(cardImageList[i]);
                byte[] textureArray = DeCompress(cardImageList[i]).EncodeToPNG();

                //CardDefinition newCardDef = new CardDefinition(cardImageList[i].name, textureArray, "", null, cardImageList[i].width / 2, cardImageList[i].height / 2);
                Debug.Log(cardImageList[i].width / 2);
                //CardHandler.singleton.LoadAssets(inPlayer.userId, textureArray, newCardDef.cardGuid);
                CompanionTextureAsset cta = new CompanionTextureAsset(textureArray, assetController, cardImageList[i].name);
                await assetController.LoadAsset(inPlayer.userId, textureArray, cta.AssetGuid.ToString());
                cardIdList.Add(cta);
                //await CardsTool.singleton.GiveCardToPlayer(inPlayer.userId, newCardDef);
            }*/
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



        private async void OnCardPlayed(CompanionCardPlayedEventArgs cardPlayedEvent)
        {
            string currentHand;
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].CurrentActiveHandID == cardPlayedEvent.userId)
                {
                    currentHand = playerList[i].CurrentActiveHandID;
                    await cardController.RemoveCardFromHandDisplay(cardPlayedEvent.userId, currentHand, cardPlayedEvent.cardId);
                }
            }
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
