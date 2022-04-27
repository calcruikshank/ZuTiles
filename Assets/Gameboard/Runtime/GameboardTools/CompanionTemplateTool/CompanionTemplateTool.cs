using UnityEngine;

namespace Gameboard.Tools
{
    public class CompanionTemplateTool : GameboardToolFoundation
    {
        public delegate void CompanionButtonPressed(string inGameboardUserId, string inCallbackMethod);
            public CompanionButtonPressed ButtonPressed;

        public delegate void CompanionCardButtonPressed(string inGameboardUserId, string inCallbackMethod, string inCardsId = "");
            public CompanionCardButtonPressed CardsButtonPressed;

        public static CompanionTemplateTool singleton;

        private bool setupCompleted;

        protected override void PerformCleanup()
        {
            if (Gameboard.singleton != null && setupCompleted)
            {
                Gameboard.singleton.companionController.CompanionButtonPressed -= CompanionController_CompanionButtonPressed;
                Gameboard.singleton.companionController.CompanionCardsButtonPressed -= CompanionController_CompanionCardsButtonPressed;
            }
        }

        void Update()
        {
            if (Gameboard.singleton == null)
            {
                return;
            }

            // Do the setup here in Update so we can just do a Singleton lookup on Gameboard, and not worry about race-conditions in using Start.
            if (!setupCompleted)
            {
                if (Gameboard.singleton.companionController.isConnected)
                {
                    Gameboard.singleton.companionController.CompanionButtonPressed += CompanionController_CompanionButtonPressed;
                    Gameboard.singleton.companionController.CompanionCardsButtonPressed += CompanionController_CompanionCardsButtonPressed;

                    singleton = this;
                    setupCompleted = true;

                    Debug.Log("--- Gameboard Companion Template Tool is ready!");
                }
            }
        }

        private void CompanionController_CompanionButtonPressed(object sender, EventArgs.GameboardCompanionButtonPressedEventArgs e)
        {
            ButtonPressed?.Invoke(e.ownerId, e.callbackMethod);
        }

        private void CompanionController_CompanionCardsButtonPressed(object sender, EventArgs.GameboardCompanionCardsButtonPressedEventArgs e)
        {
            Debug.Log("--- CompanionController_CompanionCardsButtonPressed reached");
            CardsButtonPressed?.Invoke(e.ownerId, e.callbackMethod, e.selectedCardId);
        }
    }
}