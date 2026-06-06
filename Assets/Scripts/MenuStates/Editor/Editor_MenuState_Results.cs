using UnityEditor;
using UnityEngine;
using MenuStates;

[CustomEditor(typeof(MenuState_Results))]
public class Editor_MenuState_Results : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MenuState_Results menuState = (MenuState_Results)target;
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