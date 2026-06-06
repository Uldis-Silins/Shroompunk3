using System;
using System.Collections.Generic;
using MenuStates;
using UnityEngine;

public class BasketController : MonoBehaviour
{
    /// <summary>
    /// T1: item index in inventory array, T2: item data
    /// </summary>
    public Action<int, MushroomData> onItemAdded;
    
    /// <summary>
    /// T1: item index in inventory array, T2: item data
    /// </summary>
    public Action<int, MushroomData> onItemRemoved;
    
    private MushroomData[] m_inventory;
    private float[] m_infectionLevels;
    private Stack<int> m_previousAddedIDs;
    private HashSet<int> m_foundTypeIDs;

    private const int InventorySize = 15;
    
    public int ComboCount => m_previousAddedIDs.Count;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        m_inventory = new MushroomData[InventorySize];
        m_infectionLevels = new float[InventorySize];
        m_previousAddedIDs = new Stack<int>();
        m_foundTypeIDs = new HashSet<int>();
    }

    /// <summary>
    /// Try to <paramref name="data"/> to the inventory.
    /// Fires <see cref="onItemAdded"/> on success.
    /// </summary>
    /// <returns>False if inventory is full.</returns>
    public bool AddItem(MushroomData data)
    {
        int index = -1;

        for (int i = 0; i < m_inventory.Length; i++)
        {
            if (m_inventory[i] == null)
            {
                index = i;
                break;
            }
        }
        
        if (index == -1) return false;
        
        m_inventory[index] = data;

        if (m_previousAddedIDs.Count == 0 || m_previousAddedIDs.Peek() != data.ID)
        {
            m_previousAddedIDs.Push(data.ID);
        }
        else
        {
            m_previousAddedIDs.Clear();
        }
        
        onItemAdded?.Invoke(index, data);
        
        return true;
    }

    public void RemoveItem(int index)
    {
        var item = m_inventory[index];
        m_inventory[index] = null;
        onItemRemoved?.Invoke(index, item);
    }

    public bool IsFirstTimePickup(MushroomData data)
    {
        foreach (var m in m_inventory)
        {
            if (string.IsNullOrEmpty(data.Name)) throw new NullReferenceException($"{data.name}: Mushroom name is null or empty");
            
            if(data.ID == m.ID) return false;
        }
        
        return true;
    }

    public int GetPoisonousCount()
    {
        return Array.FindAll(m_inventory, m =>m != null && m.Type == MushroomData.MushroomType.Poisonous).Length;
    }

    public int GetTypeCount(MushroomData data)
    {
        int count = 0;
        if (string.IsNullOrEmpty(data.Name)) throw new NullReferenceException($"{data.name}: Mushroom name is null or empty");

        foreach (var m in m_inventory)
        {
            if(m.ID == data.ID) count++;
        }
        
        return count;
    }

    public bool IsFirstFind(MushroomData data)
    {
        return !m_foundTypeIDs.Contains(data.ID);
    }

    public void AddToFoundIDs(MushroomData data)
    {
        m_foundTypeIDs.Add(data.ID);
    }
}