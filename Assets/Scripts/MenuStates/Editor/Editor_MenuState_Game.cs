using UnityEditor;
using UnityEngine;
using MenuStates;

[CustomEditor(typeof(MenuState_Game))]
public class Editor_MenuState_Game : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MenuState_Game menuState = (MenuState_Game)target;
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