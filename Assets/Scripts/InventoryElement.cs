using UnityEngine;
using UnityEngine.UI;
using System;
using MenuStates;

public class InventoryElement : MonoBehaviour
{
    public Action<MushroomData, InventoryElement> onPreviewClick;
    
    [SerializeField] private Image m_icon;
    [SerializeField] private Button m_previewButton;

    private MenuState_Basket.Item m_item;

    public void SetItem(MenuState_Basket.Item item)
    {
        m_item = item;
        
        if (item == null)
        {
            m_icon.sprite = null;
            m_icon.enabled = false;
            m_previewButton.onClick.RemoveListener(OnPreviewClick);
            m_previewButton.interactable = false;
        }
        else
        {
            m_icon.sprite = item.data.Icon;
            m_icon.enabled = true;
            m_previewButton.onClick.AddListener(OnPreviewClick);
            m_previewButton.interactable = true;
        }
    }

    private void OnPreviewClick()
    {
        onPreviewClick?.Invoke(m_item.data, this);
    }
}