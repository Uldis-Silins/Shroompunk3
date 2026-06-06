using UnityEditor;
using UnityEngine;
using MenuStates;

[CustomEditor(typeof(MenuState_Start))]
public class Editor_MenuState_Start : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MenuState_Start menuState = (MenuState_Start)target;
        if (GUILayout.Button("Activate"))
        {
            menuState.Activate();
        }

        if (GUILayout.Button("Deactivate"))
        {
            menuState.Deactivate();
        }
    }
}