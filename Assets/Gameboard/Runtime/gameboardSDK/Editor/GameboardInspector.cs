#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Gameboard
{
    [CustomEditor(typeof(Gameboard))]
    public class GameboardInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            Gameboard gameboard = target as Gameboard;

            if (gameboard.gameCamera != null)
            {
                if (GUILayout.Button("Align Camera for Gameboard"))
                {
                    gameboard.AlignCameraForGameboard();
                }
            }

            base.OnInspectorGUI();
        }
    }
}
#endif