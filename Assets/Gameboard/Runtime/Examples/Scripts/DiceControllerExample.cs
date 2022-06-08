using Gameboard.EventArgs;
using Gameboard.Objects;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace Gameboard.Examples
{
    public class DiceControllerExample : MonoBehaviour
    {
        private UserPresenceController userPresenceController;
        private DiceRollController diceController;
        private AssetController assetController;
        private bool diceSelectorVisibility = true;
        public Texture2D CompanionBackground;
        public Text Results;

        private void Awake()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            Gameboard gameboard = gameboardObject.GetComponent<Gameboard>();
            gameboard.GameboardInitializationCompleted += OnGameboardInit;
            gameboard.GameboardShutdownBegun += OnGameboardShutdown;

            diceController = gameboardObject.GetComponent<DiceRollController>();
            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            assetController = gameboardObject.GetComponent<AssetController>();
        }

        private void OnGameboardInit()
        {
            diceController.OnDiceRolled += OnDiceRolled;
        }

        private void OnGameboardShutdown()
        {
            diceController.OnDiceRolled -= OnDiceRolled;
        }

        public async void SetDiceBackground()
        {
            // Get the user id of the first companion user from the UserPresenceController
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                GameboardLogging.Error("There are no companion users connected.");
                return;
            }

            Results.text = $"Loading Background Asset...";

            // Create texture assets to load into the companion app
            var textureDelegate = new AssetController.AddAsset<CompanionTextureAsset, Texture2D>(assetController.CreateTextureAsset);
            var backgroundAsset = textureDelegate(CompanionBackground);

            if (backgroundAsset == null)
            {
                GameboardLogging.Error("Failed to create background asset.");
                Results.text = "Failed to create background asset.";
                return;
            }

            var loadAssetResponse = await backgroundAsset.LoadAssetToCompanion(userPresenceController, userId);
            Results.text += $"\nLoad background asset: {loadAssetResponse}";

            // Set the background of the dice area on the companion for the first user
            var setBackgroundResponse = await diceController.SetDiceBackgroundAsset(userId, backgroundAsset.AssetGuid.ToString());
            Results.text += $"\nSet Dice Background: {setBackgroundResponse}";
            return;
        }

        public async void ToggleDiceSelectorVisibility()
        {
            // Get the user id of the first companion user from the UserPresenceController
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                GameboardLogging.Error("There are no companion users connected.");
                return;
            }

            // toggle the dice selector visibility on the companion for the first user
            diceSelectorVisibility = !diceSelectorVisibility;
            var response = await diceController.SetDiceSelectorVisibility(userId, diceSelectorVisibility);
            Results.text = $"Dice Selector Visibility set: {response}";
        }

        /// <summary>
        /// Ask the first companion user to roll dice with the specified inputs.
        /// </summary>
        /// <param name="dice"></param>
        /// <param name="color"></param>
        /// <param name="notation"></param>
        /// <param name="overallModifier"></param>
        /// <returns></returns>
        public async Task RollDice(int[] dice, Color color, string notation, int overallModifier = 0)
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            // Ask the first joined user to roll dice on their companion app.
            CompanionMessageResponseArgs responseArgs = await diceController.RollDice(userId, dice, overallModifier, color, notation);
            Results.text = responseArgs.wasSuccessful ? $"Asked companion user {responseArgs.ownerId} to roll a dice." : responseArgs.errorResponse.Message;
            GameboardLogging.Log(Results.text);
        }

        /// <summary>
        /// Ask the first joined user to roll a d20
        /// </summary>
        public async void RollD20()
        {
            await RollDice(new int[] { 20 }, Color.green, "1d20");
        }

        /// <summary>
        /// Ask the first joined user to roll a clear d20
        /// </summary>
        public async void RollClearD20()
        {
            await RollDice(new int[] { 20 }, Color.clear, "1d20");
        }

        /// <summary>
        /// Ask the first joined user to roll a lot of dice.
        /// </summary>
        public async void RollALotOfDice()
        {
            await RollDice(new int[] { 20, 6, 6, 4, 10, 12, 100 }, Color.gray, "1d20+2d6+1d4+1d10+1d12+1d100");
        }

        /// <summary>
        /// Ask the first joined user to roll a modified d6
        /// </summary>
        public async void RollModifiedD6()
        {
            await RollDice(new int[] { 6 }, Color.blue, "1d6", 1000);
        }

        /// <summary>
        /// Handle the dice roll event generated by the companion user
        /// </summary>
        /// <param name="diceRolledEvent"></param>
        void OnDiceRolled(CompanionDiceRollEventArgs diceRolledEvent)
        {
            // Handle Dice Rolls
            GameboardLogging.Log($"{diceRolledEvent}");

            Results.text = $"ownerId: {diceRolledEvent.ownerId} \n" +
                $"addedModifier: {diceRolledEvent.addedModifier} \n" +
                $"diceNotation: {diceRolledEvent.diceNotation} \n" +
                $"diceSizesRolledList: {string.Join(", ", diceRolledEvent.diceSizesRolledList)} \n" +
                $"errorId: {diceRolledEvent.errorId} \n" +
                $"errorResponse: {diceRolledEvent.errorResponse} \n";
            GameboardLogging.Log(Results.text);
        }
    }
}
