using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MenuStates
{
    public class MenuState_Map : MenuState_Base
    {
        [SerializeField] private Button m_gameButton;
        [SerializeField] private Button m_basketButton;
        
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private GameController m_gameController;
        
        [SerializeField] private TextMeshProUGUI m_timeText;
        [SerializeField] private TextMeshProUGUI m_scoreText;

        private void OnEnable()
        {
            m_gameButton.onClick.AddListener(m_gameManager.ShowGame);
            m_basketButton.onClick.AddListener(m_gameManager.ShowBasket);
            
            m_gameController.onScoreChanged += OnScoreChanged;
            m_gameController.onTimeChanged += OnTimeChanged;
        }

        private void OnDisable()
        {
            m_gameButton.onClick.RemoveListener(m_gameManager.ShowGame);
            m_basketButton.onClick.RemoveListener(m_gameManager.ShowBasket);
            
            m_gameController.onScoreChanged -= OnScoreChanged;
            m_gameController.onTimeChanged -= OnTimeChanged;
        }
        
        private void OnScoreChanged(int prevScore, int currentScore)
        {
            m_scoreText.text = currentScore.ToString();
        }
        
        private void OnTimeChanged(float time)
        {
            m_timeText.text = string.Format("{0:00}:{1:00}", (int)(time / 60), (int)(time % 60));
        }
    }
}