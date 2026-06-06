using UnityEngine;

[CreateAssetMenu(fileName = "MushroomData", menuName = "Data/Mushroom", order = 0)]
public class MushroomData : ScriptableObject
{
    public enum MushroomType { None = -1, Edible, Poisonous }
    
    [field: SerializeField] public GameObject GrowingPrefab { get; private set; }
    [field: SerializeField] public GameObject CutTopPrefab { get; private set; }
    [field: SerializeField] public GameObject CutBottomPrefab { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public int Value { get; private set; }
    [field: SerializeField] public MushroomType Type { get; private set; }
    [field: SerializeField] public string Name { get; private set; }

    public int ID => Name.GetHashCode();
    public float WormChance => Value / 120f;
}