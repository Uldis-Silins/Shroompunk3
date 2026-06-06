using UnityEditor;
using UnityEngine;
using MenuStates;

[CustomEditor(typeof(MenuState_Basket))]
public class Editor_MenuState_Basket : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MenuState_Basket menuState = (MenuState_Basket)target;
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