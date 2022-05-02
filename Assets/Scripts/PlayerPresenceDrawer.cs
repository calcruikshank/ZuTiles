using Gameboard.EventArgs;
using Gameboard.Tools;
using Gameboard.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gameboard.Examples{

    public class PlayerPresenceDrawer : UserPresenceSceneObject
    {
        private void Awake()
        {
            Gameboard.singleton.companionController.CompanionCardsButtonPressed += CardsButtonPressed;
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


        public void DrawCard(GameObject cardToDraw)
        {
            this.gameObject.GetComponent<PlayerContainer>().AddCardToHand(cardToDraw);
        }

        public Vector3 GetRotation()
        {
            return this.transform.eulerAngles;
        }

        void CardsButtonPressed(object sender, EventArgs.GameboardCompanionCardsButtonPressedEventArgs e)
        {
            if (e.userId != userId)
            {
                return;
            }
            Debug.Log(this.gameObject + "Player to play card + " + e.ownerId + " Owner id " + e.callbackMethod + " callback method  " + e.selectedCardId + " Selected card id");
        }
        /*void CardsButtonEvent(string gameboardUserId, string callbackMethod, string cardsId)
        {
            if (gameboardUserId != Gameboard_UserID || isResolvingCardAction || isAiPlayer)
            {
                Debug.Log("--- CardsButtonEvent refused because: " + (gameboardUserId != Gameboard_UserID) + " / " + isResolvingCardAction + " / " + isAiPlayer);
                return;
            }

            if (isResolvingCardAction || isAiPlayer)
            {
                Debug.Log("--- Still resolving card action! Received event " + callbackMethod + " for usre " + gameboardUserId + " and cardID " + cardsId);
                return;
            }

            Debug.Log("--- Successfully Received Cards Button event " + callbackMethod + " for user " + gameboardUserId + " and cardID " + cardsId);

            if (callbackMethod == listenForCompanionEventGameplayCardSelect)
            {
                PrepareProjectCardToPlay(cardsId);
            }
            else if (callbackMethod == listenForCompanionEventCardSelectFinished)
            {
                finalizeSelectionNextUpdate = true;
            }
            else if (callbackMethod == listenForCompanionEventSelectCardToAddToHand)
            {
                SelectCardToAddToHand(cardsId);
            }
            else if (callbackMethod == listenForCompanionEventSelectForPatent)
            {
                FinalizeSellPatent(cardsId);
            }
            else if (callbackMethod == listenForCompanionEventCorporationSelected)
            {
                CorporationCardSelected(cardsId);
            }
        }*/

    }


}
