using Gameboard.EventArgs;
using Gameboard.Helpers;
using Gameboard.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace Gameboard.Examples
{

    public class PlayerPresenceDrawer : UserPresenceSceneObject
    {
        UserPresenceController userPresenceController;
        public string CurrentActiveHandID;
        PlayerContainer playerContainer;
        CardController cardController;
        AssetController assetController;
        private void Awake()
        {
            GameboardLogging.Verbose("UserPresenceExample Awake");
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            GameboardLogging.Verbose("UserPresenceExample Awake Success");
            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            cardController = gameboardObject.GetComponent<CardController>();
            assetController = gameboardObject.GetComponent<AssetController>();
            Gameboard.singleton.companionController.CardPlayed += CardsButtonPressedAsync;
            Gameboard.singleton.companionController.CompanionButtonPressed += ButtonPressed;
            this.playerContainer = this.GetComponent<PlayerContainer>();
        }

        private void ButtonPressed(object sender, GameboardCompanionButtonPressedEventArgs e)
        {
            Debug.Log("CompanionButtonPressed");
        }

        private void CardsButtonPressedAsync(object sender, CompanionCardPlayedEventArgs e)
        {
            if (e.userId != userId)
            {
                return;
            }

            //CardDefinition selectedCard = CardsTool.singleton.GetCardDefinitionByGUID(e.cardId);
            string selectedCard = e.cardId;

            cardController.RemoveCardFromHandDisplay(userId, CurrentActiveHandID, e.cardId);
            this.transform.GetComponentInChildren<PlayerContainer>().FindCardToRemove(e.cardId);
            //CardsTool.singleton.RemoveCardFromPlayerHand(userId, CardsTool.singleton.GetCardHandDisplayedForPlayer(userId), selectedCard);
        }
        public void UpdatePlayerPositionOnStart(Vector2 vectorSent)
        {
            var  playerSceneDrawerPosition = GameboardHelperMethods.GameboardScreenPointToScenePoint(Camera.main, vectorSent);
            Debug.LogError(playerSceneDrawerPosition + " Player presence scene drawer update");
            ScenePositionUpdated(playerSceneDrawerPosition);
            ScenePositionUpdated(new Vector3(playerSceneDrawerPosition.x, 0.2f, playerSceneDrawerPosition.z));

           /* Vector3 eulerRotation = Vector3.zero;
            if (playerScreenPosition.y == 0f)
            {
                // At top of screen
                eulerRotation.y = 180f;
            }
            else if (playerScreenPosition.y == 1f)
            {
                // At bottom of screen
                eulerRotation.y = 0f;
            }
            else if (playerScreenPosition.x == 0f)
            {
                // At left of screen
                eulerRotation.y = 90f;
            }
            else if (playerScreenPosition.x == 1f)
            {
                // At right of screen
                eulerRotation.y = -90f;
            }*/
            //LocalEulerAnglesUpdated(eulerRotation);
        }
        protected override void ScenePositionUpdated(Vector3 inNewPosition)
        {
            this.transform.position = inNewPosition;
            Debug.Log("UpdatedPosition = " + inNewPosition);
        }
        protected override void LocalEulerAnglesUpdated(Vector3 inNewEulers)
        {
            this.transform.localEulerAngles = inNewEulers;
        }
        protected override void PlayerAdded()
        {
            Debug.Log("Adding Player");
        }
        protected override void PlayerRemoved()
        {
            Debug.Log("Player Removed");
            Destroy(this.gameObject);
        }
        protected override void PlayerNameChanged()
        {
            Debug.Log("Player name changed");
        }
        protected override void PlayerColorChanged()
        {
            Debug.Log("Player Color Changed ");
        }

        public Vector3 GetRotation()
        {
            return this.transform.eulerAngles;
        }

        

    }


}
