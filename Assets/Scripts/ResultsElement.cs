using TMPro;
using UnityEngine;

public class ResultsElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_username;
    [SerializeField] private TextMeshProUGUI m_score;

    public void Initialize(string username, int score)
    {
        int position = transform.GetSiblingIndex();
        m_username.text = position + ". " + username;
        m_score.text = score.ToString();
    }
}