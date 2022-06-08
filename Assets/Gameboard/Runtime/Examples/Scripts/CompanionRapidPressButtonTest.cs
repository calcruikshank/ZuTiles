using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CompanionRapidPressButtonTest : MonoBehaviour
{
    public List<Texture2D> cardHandAImageList = new List<Texture2D>();

    private bool setupCompleted;
    private List<CompanionRapidButtonPressTestPlayer> playerList = new List<CompanionRapidButtonPressTestPlayer>();
    private float buttonAllowedTimeCountdown = 0f;
    private string resolveOnUpdate;

    private const string buttonMethod = "ButtonAPressed";
    private float cachedTime;

    void Update()
    {
        cachedTime = Time.time;

        if (!string.IsNullOrEmpty(resolveOnUpdate))
        {
            ResolveButtonPress(resolveOnUpdate);
            resolveOnUpdate = "";
        }

        if (!setupCompleted)
        {
            if (Gameboard.Gameboard.Instance != null &&
               UserPresenceTool.singleton != null &&
               CardsTool.singleton != null &&
               CompanionAssetsTool.singleton != null &&
               CompanionTemplateTool.singleton != null)
            {
                Debug.Log("--- CompanionRapidPressButtonTest is ready!");

                setupCompleted = true;

                CompanionTemplateTool.singleton.ButtonPressed += CompanionButtonPressed;
                CompanionTemplateTool.singleton.CardsButtonPressed += CardsButtonPressed;
                UserPresenceTool.singleton.RequestUserPresenceUpdate();
            }
        }
        else
        {
            buttonAllowedTimeCountdown = Mathf.MoveTowards(buttonAllowedTimeCountdown, 0f, Time.deltaTime);
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
                Debug.Log("--- Begin adding player " + eventArgs.userId);

                CompanionRapidButtonPressTestPlayer testPlayer = new CompanionRapidButtonPressTestPlayer()
                {
                    gameboardId = eventArgs.userId
                };

                playerList.Add(testPlayer);

                AddCardsToPlayer(testPlayer);
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

    private async void AddCardsToPlayer(CompanionRapidButtonPressTestPlayer inPlayer)
    {
        inPlayer.handAId = await CreateCardHand(inPlayer.gameboardId, cardHandAImageList);
    }

    async Task<string> CreateCardHand(string inGameboardId, List<Texture2D> cardImageList)
    {
        string cardHandId = await CardsTool.singleton.CreateCardHandOnPlayer(inGameboardId);

        List<CardDefinition> cardHandIdList = new List<CardDefinition>();
        for (int i = 0; i < cardImageList.Count; i++)
        {
            byte[] textureArray = cardImageList[i].EncodeToPNG();

            CardDefinition newCardDef = new CardDefinition(cardImageList[i].name, textureArray, "", null, cardImageList[i].width, cardImageList[i].height);
            cardHandIdList.Add(newCardDef);

            await CardsTool.singleton.GiveCardToPlayer(inGameboardId, newCardDef);
            await CardsTool.singleton.PlaceCardInPlayerHand_Async(inGameboardId, cardHandId, newCardDef);
        }

        return cardHandId;
    }

    private async void AddButtonsToPlayer(CompanionRapidButtonPressTestPlayer inPlayer)
    {
        Debug.Log("--- Adding buttons to player " + inPlayer.gameboardId);

        await Gameboard.Gameboard.Instance.companionController.SetCompanionButtonValues(inPlayer.gameboardId, "1", "Go Fast!", buttonMethod);
        Debug.Log("--- Button 1 now has control contents");

        await Gameboard.Gameboard.Instance.companionController.ChangeObjectDisplayState(inPlayer.gameboardId, "1", DataTypes.ObjectDisplayStates.Displayed);
        Debug.Log("--- Button 1 is now displayed.");
    }

    void CompanionButtonPressed(string inGameboardUserId, string inCallbackMethod)
    {
        if(!string.IsNullOrEmpty(resolveOnUpdate))
        {
            Debug.Log("--- Still resolving previous button press!");
            return;
        }

        resolveOnUpdate = inCallbackMethod;
    }

    void CardsButtonPressed(string inGameboardUserId, string inCallbackMethod, string inCardsId)
    {
        if (!string.IsNullOrEmpty(resolveOnUpdate))
        {
            Debug.Log("--- Still resolving previous button press!");
            return;
        }

        resolveOnUpdate = inCallbackMethod; 
    }

    private void ResolveButtonPress(string inCallbackMethod)
    {
        Debug.Log("--- ResolveButtonPress for " + inCallbackMethod);
        if (inCallbackMethod == buttonMethod)
        {
            if(buttonAllowedTimeCountdown > 0f)
            {
                Debug.Log("--- Button action still resolving. " + buttonAllowedTimeCountdown.ToString() + " remaining.");
            }
            else
            {
                buttonAllowedTimeCountdown = 1f;
                Debug.Log("--- Button presssed!");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Gameboard.Gameboard.Instance.companionController.DisplaySystemPopup(playerList[0].gameboardId, "Pressed at " + cachedTime, 1f);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
    }

    class CompanionRapidButtonPressTestPlayer
    {
        public string gameboardId;
        public string handAId;
    }
}
