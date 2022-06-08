using Gameboard.EventArgs;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gameboard.Objects;

namespace Gameboard.Examples
{
    [RequireComponent(typeof(Gameboard))]
    [RequireComponent(typeof(UserPresenceController))]
    [RequireComponent(typeof(AssetController))]
    /// <summary>
    /// The asset controller handles data management for assets on the companion app.
    /// Assets are required to be loaded prior to being used. More information can be found here:
    /// https://lastgameboard.atlassian.net/wiki/spaces/DC/pages/757530631/Gameboard+Developers+Guide
    /// </summary>
    public class AssetControllerExample : MonoBehaviour
    {
        private UserPresenceController userPresenceController;
        private AssetController assetController;

        public Text Results;

        /// <summary>
        /// Texture2D assets defined in unity will be used to load into the companion app.
        /// </summary>
        public List<Texture2D> TextureAssets;

        private void Awake()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            Gameboard gameboard = gameboardObject.GetComponent<Gameboard>();
            gameboard.GameboardInitializationCompleted += OnGameboardInit;
            gameboard.GameboardShutdownBegun += OnGameboardShutdown;

            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            assetController = gameboardObject.GetComponent<AssetController>();

            var textureDelegate = new AssetController.AddAsset<CompanionTextureAsset, Texture2D>(assetController.CreateTextureAsset);
            var textureAssetsFromPath = assetController.CreateCompanionAssetsFromPath("Cards", textureDelegate);
            GameboardLogging.Verbose($"textureAssetsFromPath count {textureAssetsFromPath.Count}");

            var fbxDelegate = new AssetController.AddAsset<CompanionFBXAsset, Mesh>(assetController.CreateFBXAsset);
            var fbxAssetsFromPath = assetController.CreateCompanionAssetsFromPath("Models", fbxDelegate);
            GameboardLogging.Verbose($"fbxAssetsFromPath count: {fbxAssetsFromPath.Count}");
            
            GameboardLogging.Verbose($"assetController.CompanionAssets count: {assetController.CompanionAssets.Count}");
            GameboardLogging.Verbose($"assetController.LoadedAssets count: {assetController.LoadedAssets.Count}");
        }

        public void TestCreateAssets()
        {
            CreateCompanionAssets();
        }

        public async void TestLoadAssets()
        {
            await LoadAllAssets();
        }

        public async void TestDeleteAssets()
        {
            await DeleteAllAssetsFromCompanion();
        }

        private async void OnGameboardInit()
        {
            await LoadAllAssets();
        }

        private async void OnGameboardShutdown()
        {
            await DeleteAllAssetsFromCompanion();
        }

        /// <summary>
        /// Create ICompanionAsset objects to use when loading onto the companion app.
        /// </summary>
        private void CreateCompanionAssets()
        {
            List<ICompanionAsset> newAssets = new List<ICompanionAsset>();
            TextureAssets.ForEach(t =>
            {
                byte[] imageBytes = t.EncodeToPNG();
                CompanionTextureAsset asset = new CompanionTextureAsset(imageBytes, assetController);
                newAssets.Add(asset);
            });

            var logMessage = $"Created {newAssets.Count} new assets, {assetController.CompanionAssets.Count} total.";
            Results.text = logMessage;
            GameboardLogging.Verbose(logMessage);
        }

        /// <summary>
        /// Load all assets objects that were previously instantiated with CreateCompanionAssets 
        /// onto the companion app cor each connected user presence via the AssetController.
        /// </summary>
        /// <returns></returns>
        private async Task LoadAllAssets()
        {
            Results.text = "";
            var results = await assetController.LoadAllAssetsOntoAllCompanions();
            results.ForEach(r => Results.text += $"Loaded assetId, result={r}");
        }

        /// <summary>
        /// Delete all assets objects that were previously loaded via the LoadAssetsOntoCompanion.
        /// </summary>
        /// <returns></returns>
        private async Task DeleteAllAssetsFromCompanion()
        {
            Results.text = "";
            var loadedAssetCount = 0;
            foreach (var asset in assetController.LoadedAssets)
                loadedAssetCount += asset.Value.Count;
            
            GameboardLogging.Verbose($"Loaded Assets Count = {loadedAssetCount}");
            List<string> keys = new List<string>(assetController.LoadedAssets.Keys);
            
            foreach (var key in keys)
            {
                var assets = new List<ICompanionAsset>(assetController.LoadedAssets[key]);
                foreach (var asset in assets)
                {
                    CompanionMessageResponseArgs result = await asset.DeleteAssetFromCompanion(userPresenceController, key);
                    var resultMessage = $"Deleted assetId={asset.AssetGuid} for userId={key}, result={result}";
                    Results.text += resultMessage + "\n";
                    GameboardLogging.Verbose(resultMessage);
                }
            }

            loadedAssetCount = 0;
            foreach (var asset in assetController.LoadedAssets)
                loadedAssetCount += asset.Value.Count;
            
            GameboardLogging.Verbose($"Loaded Assets after Delete Count = {loadedAssetCount}");
        }

    }

}
