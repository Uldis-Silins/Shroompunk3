using TMPro;
using UnityEngine;

public class InfoPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_title;
    [SerializeField] private TextMeshProUGUI m_description;

    public void Show(string title, string description)
    {
        m_title.text = title;
        m_description.text = description;
    }
}