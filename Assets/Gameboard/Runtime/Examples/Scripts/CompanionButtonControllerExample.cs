using Gameboard.EventArgs;
using UnityEngine;
using UnityEngine.UI;
using Gameboard.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Gameboard.EventArgs.CompanionAssetErrorResponse;
using static Gameboard.EventArgs.CompanionPlayerPresenceErrorResponse;

namespace Gameboard.Examples
{
    public class CompanionButtonControllerExample : MonoBehaviour
    {
        private CompanionButtonController companionButtonController;
        private UserPresenceController userPresenceController;
        private AssetController assetController;

        private string userId;
        private CompanionTextureAsset buttonIdle;
        private CompanionTextureAsset buttonDown;

        public Text Results;

        private void Awake()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            Gameboard gameboard = gameboardObject.GetComponent<Gameboard>();
            gameboard.GameboardInitializationCompleted += OnGameboardInit;
            gameboard.GameboardShutdownBegun += OnGameboardShutdown;

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            companionButtonController = gameboardObject.GetComponent<CompanionButtonController>();
            assetController = gameboardObject.GetComponent<AssetController>();
        }

        private void OnDestroy()
        {
            companionButtonController.CompanionButtonPressed -= OnCompanionButtonPressed;
        }

        private void OnGameboardInit()
        {
            companionButtonController.CompanionButtonPressed += OnCompanionButtonPressed;
        }

        private void OnGameboardShutdown()
        {
            companionButtonController.CompanionButtonPressed -= OnCompanionButtonPressed;
        }

        public async void SetButton()
        {
            await SetCompanionButton();
        }

        public void LoadAssets()
        {
            LoadCompanionAssets();
        }

        /// <summary>
        ///  Set a companion button with the text Hello World on the first companion user's device.
        /// </summary>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetCompanionButton()
        {
            userId = Utils.GetFirstCompanionUserId(userPresenceController);
            CompanionMessageResponseArgs response = await companionButtonController.SetCompanionButtonValues(userId, "1", "Hello World.", "HelloWorldCallback");
            Results.text = $"response for companionButtonController.SetCompanionButtonValues: {response}";
            GameboardLogging.Verbose($"response for companionButtonController.SetCompanionButtonValues: {response}");

            return response;
        }

        /// <summary>
        /// Load button textures from the Buttons resource folder and then create the companion assets
        /// </summary>
        /// <returns></returns>
        public CompanionCreateObjectEventArgs LoadCompanionAssets()
        {
            if (userPresenceController.Users.Count == 0)
            {
                Results.text = "No users were found in userPresenceController.Users.";
                GameboardLogging.Error("No users were found in userPresenceController.Users.");
                return new CompanionCreateObjectEventArgs() { errorResponse = new CompanionPlayerPresenceErrorResponse() { ErrorValue = PlayerPresenceErrorTypes.NoError, } };
            }

            // Get the user id of the first companion user from the UserPresenceController
            userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                GameboardLogging.Error("There are no companion users connected.");
                return new CompanionCreateObjectEventArgs() { errorResponse = new CompanionAssetErrorResponse() { ErrorValue = PlayerPresenceErrorTypes.NoError, } };
            }

            // Create texture assets to load into the companion app
            var textureDelegate = new AssetController.AddAsset<CompanionTextureAsset, Texture2D>(assetController.CreateTextureAsset);
            List<CompanionTextureAsset> textureAssetsFromPath = assetController.CreateCompanionAssetsFromPath("Buttons", textureDelegate);

            // Get the idle and down button assets from the list of created assets
            buttonIdle = textureAssetsFromPath.Find(a => a.Name == "Button_Idle");
            buttonDown = textureAssetsFromPath.Find(a => a.Name == "Button_Down");

            if (buttonIdle == null || buttonDown == null)
            {
                GameboardLogging.Error("Failed to load button assets.");
                Results.text = "Failed to load button assets.";
                return new CompanionCreateObjectEventArgs() { errorResponse = new CompanionAssetErrorResponse() { ErrorValue = AssetErrorTypes.NoCompanionAssetFoundAtUID, } };
            }

            Results.text = $"Loaded Button Assets {buttonIdle.Name} and {buttonDown.Name}.";
            return new CompanionCreateObjectEventArgs() { errorResponse = new CompanionAssetErrorResponse() { ErrorValue = AssetErrorTypes.NoError, } };
        }

        private void OnCompanionButtonPressed(GameboardCompanionButtonPressedEventArgs companionButtonEvent)
        {
            Results.text = $"OnCompanionButtonPressed {companionButtonEvent}";
        }
    }
}
