using Gameboard.EventArgs;
using Gameboard.Tools;
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
        PlayerContainer playerContainer;
        private void Awake()
        {
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

            CardDefinition selectedCard = CardsTool.singleton.GetCardDefinitionByGUID(e.cardId);

            UserPresenceTest.singleton.RemoveCardFromUser(userId, selectedCard);

            this.transform.GetComponentInChildren<PlayerContainer>().FindCardToRemove(selectedCard);
            CardsTool.singleton.RemoveCardFromPlayerHand(userId, CardsTool.singleton.GetCardHandDisplayedForPlayer(userId), selectedCard);
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
