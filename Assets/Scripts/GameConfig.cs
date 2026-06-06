using UnityEngine;

[CreateAssetMenu(fileName = "DefaultGameConfig", menuName = "Data/Game Config", order = 0)]
public class GameConfig : ScriptableObject
{
    [field: SerializeField, Tooltip("Total game time in minutes.")] 
    public float Time { get; private set; } = 5f;
}