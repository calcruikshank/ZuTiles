using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Gameboard.Menu
{
#if UNITY_EDITOR
    public class GameboardMenu : MonoBehaviour
    {
        [MenuItem("Gameboard/Add SDK")]
        static void InitializeProject()
        {
            string gameboardPrefabPath = "Packages/com.lastgameboard.unity.plugin/Runtime/gameboardSDK/Gameboard.prefab";
            Object gameboardPrefab = AssetDatabase.LoadAssetAtPath(gameboardPrefabPath, typeof(GameObject));

            // Instantiate on active scene.
            PrefabUtility.InstantiatePrefab(gameboardPrefab, SceneManager.GetActiveScene());

            // Ensure the execution order for the gameboard script.
            var gameboardMainBehaviorPath = "Packages/com.lastgameboard.unity.plugin/Runtime/gameboardSDK/Gameboard.cs";
            var monoImporter = AssetImporter.GetAtPath(gameboardMainBehaviorPath) as MonoImporter;
            MonoImporter.SetExecutionOrder(monoImporter.GetScript(), -105);
        }

        [MenuItem("Gameboard/Documentation")]
        static void ShowDocumentation()
        {
            Help.BrowseURL("https://lastgameboard.atlassian.net/wiki/spaces/DC/pages/757530631/Gameboard+Developers+Guide");
        }

    }
#endif
}
