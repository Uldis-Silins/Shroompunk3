using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryElement : MonoBehaviour
{
    public Action<MushroomData, InventoryElement> onPreviewClick;
    
    [SerializeField] private Image m_icon;
    [SerializeField] private Button m_previewButton;

    private MushroomData m_data;

    public void SetItem(MushroomData data)
    {
        m_data = data;
        
        if (data == null)
        {
            m_icon.sprite = null;
            m_icon.enabled = false;
            m_previewButton.onClick.RemoveListener(OnPreviewClick);
            m_previewButton.interactable = false;
        }
        else
        {
            m_icon.sprite = data.Icon;
            m_icon.enabled = true;
            m_previewButton.onClick.AddListener(OnPreviewClick);
            m_previewButton.interactable = true;
        }
    }

    private void OnPreviewClick()
    {
        onPreviewClick?.Invoke(m_data, this);
    }
}