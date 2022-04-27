#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Gameboard.Tools
{
    [CustomEditor(typeof(ScreenTouchButton))]
    public class ScreenTouchButtonInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            ScreenTouchButton touchButton = target as ScreenTouchButton;

            EditorGUILayout.LabelField($"Button State: {touchButton.activeTouchState}");
            EditorGUILayout.LabelField($"Has Sprite Renderer: {touchButton.buttonSprite != null}");

            GUILayout.BeginVertical("box");
                GUILayout.Label("Button Settings", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                    GUILayout.Label("Down Axis", GUILayout.Width(120f));
                    touchButton.downAxis = (ScreenTouchButton.AxisAngles)EditorGUILayout.EnumPopup(touchButton.downAxis);
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
                GUILayout.Label("Button Sprites", EditorStyles.boldLabel);                
                DrawButtonEditor("Up", ref touchButton.idleSprite);
                DrawButtonEditor("Down", ref touchButton.downSprite);
                DrawButtonEditor("Disabled", ref touchButton.disabledSprite);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
                GUILayout.Label("Object References", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                    GUILayout.Label("Collider", GUILayout.Width(120f));
                        touchButton.inputCollider = (Collider)EditorGUILayout.ObjectField(touchButton.inputCollider, typeof(Collider), true);
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
                GUILayout.Label("Input Action", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                    GUILayout.Label("Input Target", GUILayout.Width(120f));
                    touchButton.buttonTarget = (GameObject)EditorGUILayout.ObjectField(touchButton.buttonTarget, typeof(GameObject), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                    GUILayout.Label("Input Method", GUILayout.Width(120f));
                    touchButton.buttonMethod = EditorGUILayout.TextField(touchButton.buttonMethod);
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            Repaint();

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(touchButton);
            }
        }

        private void DrawButtonEditor(string buttonName, ref Sprite buttonSlot)
        {
            GUILayout.BeginHorizontal();
                GUILayout.Label(buttonName, GUILayout.Width(60f));
                buttonSlot = (Sprite)EditorGUILayout.ObjectField(buttonSlot, typeof(Sprite), false);
            GUILayout.EndHorizontal();
        }
    }
}
#endif