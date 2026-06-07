using System;
using UnityEngine;
using UnityEngine.UI;

namespace MenuStates
{
    public class MenuState_Basket : MenuState_Base
    {
        public class Item
        {
            public int score;
            public MushroomData data;
        }
        
        [SerializeField] private Button m_mapButton;
        [SerializeField] private Button m_gameButton;
        
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private BasketController m_basketController;
        [SerializeField] private PreviewMenu m_previewMenu;

        [SerializeField] private InventoryElement[] m_inventory;

        private void OnEnable()
        {
            m_mapButton.onClick.AddListener(m_gameManager.ShowMap);
            m_gameButton.onClick.AddListener(m_gameManager.ShowGame);
            
            m_basketController.onItemAdded += OnItemAdded;
            m_basketController.onItemRemoved += OnItemRemoved;

            foreach (var item in m_inventory)
            {
                item.onPreviewClick += OnItemSelected;
            }
            
            m_previewMenu.onDeleteItem += OnDeletePreviewItem;
        }
        
        private void OnDisable()
        {
            m_mapButton.onClick.RemoveListener(m_gameManager.ShowMap);
            m_gameButton.onClick.RemoveListener(m_gameManager.ShowGame);
            
            m_basketController.onItemAdded -= OnItemAdded;
            m_basketController.onItemRemoved -= OnItemRemoved;
            
            foreach (var item in m_inventory)
            {
                item.onPreviewClick -= OnItemSelected;
            }
            
            m_previewMenu.onDeleteItem -= OnDeletePreviewItem;
        }

        private void OnItemAdded(int index, Item item)
        {
            m_inventory[index].SetItem(item);
        }
        
        private void OnItemRemoved(int index, Item item)
        {
            m_inventory[index].SetItem(null);
        }
        
        private void OnItemSelected(MushroomData data, InventoryElement element)
        {
            int index = Array.IndexOf(m_inventory, element);
            m_previewMenu.Show(data, index, false);
        }
        
        private void OnDeletePreviewItem(int index)
        {
            m_basketController.RemoveItem(index);
        }
    }
}