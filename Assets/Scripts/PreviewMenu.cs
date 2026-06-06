using UnityEngine;
using UnityEngine.UI;
using System;
using PrimeTween;
using TMPro;

public class PreviewMenu : MonoBehaviour
{
    public Action<int> onDeleteItem;
    
    [SerializeField] private GameObject m_previewContainer;
    [SerializeField] private Transform m_previewSpawnPoint;
    [SerializeField] private Button m_deleteButton;
    [SerializeField] private Button m_cutButton;
    [SerializeField] private TextMeshProUGUI m_score;

    private MushroomData m_data;
    private int m_inventoryIndex;
    
    // Offset for pivot at the mesh base
    // TODO: offset by half of renderer bounds size in Y axis
    private readonly Vector3 m_uncutPreviewOffset = new Vector3(0f, -0.12f, 0f);

    private bool m_isCut;
    
    private GameObject m_spawnedObject;

    private void OnEnable()
    {
        m_deleteButton.onClick.AddListener(OnDeleteButtonClick);
        m_cutButton.onClick.AddListener(OnCutButtonClick);
    }

    private void OnDisable()
    {
        m_deleteButton.onClick.RemoveListener(OnDeleteButtonClick);
        m_cutButton.onClick.RemoveListener(OnCutButtonClick);
    }

    public void Show(MushroomData data, int index, bool isCut)
    {
        if (m_spawnedObject != null)
        {
            Destroy(m_spawnedObject);
            m_spawnedObject = null;
        }
        
        m_data = data;
        m_inventoryIndex = index;
        
        m_isCut = isCut;
        m_cutButton.gameObject.SetActive(!isCut);

        m_spawnedObject = Instantiate(data.GrowingPrefab, m_previewSpawnPoint);
        m_spawnedObject.layer = LayerMask.NameToLayer("Preview");
        m_spawnedObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Preview");
        m_spawnedObject.transform.localScale = Vector3.one * 2;
        if(!isCut) m_spawnedObject.transform.localPosition += m_uncutPreviewOffset;
        m_previewContainer.SetActive(true);

        m_score.text = data.Value.ToString();
        
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        Destroy(m_spawnedObject);
        m_spawnedObject = null;
        
        m_previewContainer.SetActive(false);
        
        gameObject.SetActive(false);
    }

    private void OnDeleteButtonClick()
    {
        onDeleteItem?.Invoke(m_inventoryIndex);
        Hide();
    }

    public void OnCutButtonClick()
    {
        if (m_data != null && !m_isCut)
        {
            Destroy(m_spawnedObject);
            m_spawnedObject = Instantiate(m_data.CutTopPrefab, m_previewSpawnPoint);
            m_spawnedObject.layer = LayerMask.NameToLayer("Preview");
            m_spawnedObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Preview");
            m_spawnedObject.transform.localScale = Vector3.one * 2;
            m_spawnedObject.transform.localPosition += m_uncutPreviewOffset;
            Tween.LocalRotation(m_spawnedObject.transform, Quaternion.Euler(45f, 0f, 0f), duration: 0.33f);
            Vector3 tweenOffset = m_spawnedObject.transform.localPosition + Vector3.forward * 0.25f;
            Sequence.Create().Chain(
                Tween.LocalPosition(m_spawnedObject.transform, tweenOffset, duration: 0.25f, Ease.OutElastic)).Chain(
                Tween.LocalPosition(m_spawnedObject.transform, m_uncutPreviewOffset, duration: 0.25f)
                );
            m_cutButton.gameObject.SetActive(false);
        }
    }
}