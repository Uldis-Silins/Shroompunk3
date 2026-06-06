using UnityEditor;
using UnityEngine;
using MenuStates;

[CustomEditor(typeof(MenuState_Map))]
public class Editor_MenuState_Map : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MenuState_Map menuState = (MenuState_Map)target;
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