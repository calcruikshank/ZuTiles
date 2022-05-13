using Gameboard.EventArgs;
using Gameboard.Tools;
using Gameboard.Utilities;
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
            Gameboard.singleton.companionController.CompanionCardsButtonPressed += CardsButtonPressedAsync;
            this.playerContainer = this.GetComponent<PlayerContainer>();
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

        void CardsButtonPressedAsync(object sender, EventArgs.GameboardCompanionCardsButtonPressedEventArgs e)
        {
            if (e.userId != userId)
            {
                return;
            }
            Debug.Log(this.gameObject + "Player to play card + " + e.ownerId + " Owner id " + e.callbackMethod + " callback method  " + e.selectedCardId + " Selected card id");

            CardDefinition selectedCard = CardsTool.singleton.GetCardDefinitionByGUID(e.selectedCardId);

            UserPresenceTest.singleton.RemoveCardFromUser(userId, selectedCard);
            
            this.transform.GetComponentInChildren<PlayerContainer>().FindCardToRemove(selectedCard);
            CardsTool.singleton.RemoveCardFromPlayerHand(userId, CardsTool.singleton.GetCardHandDisplayedForPlayer(userId), selectedCard);
        }

    }


}
