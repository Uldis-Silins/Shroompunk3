using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuStates
{
    public class MenuState_Start : MenuState_Base
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private Button m_startButton;
        [SerializeField] private Button m_resultsButton;

        [SerializeField] private RectTransform m_resultsPanel;

        [SerializeField] private TMP_InputField m_usernameField;

        [SerializeField] private Sprite m_startButtonEnabled;
        [SerializeField] private Sprite m_startButtonDisabled;

        private void OnEnable()
        {
            m_startButton.onClick.AddListener(OnStartButtonClick);
            m_resultsButton.onClick.AddListener(OnResultButtonClick);
            
            m_usernameField.onValueChanged.AddListener(OnUsernameValueChanged);
        }

        private void OnDisable()
        {
            m_startButton.onClick.RemoveListener(OnStartButtonClick);
            m_resultsButton.onClick.RemoveListener(OnResultButtonClick);
            
            m_usernameField.onValueChanged.RemoveListener(OnUsernameValueChanged);
        }

        public override void Activate()
        {
            base.Activate();
            m_startButton.interactable = true;
            m_resultsButton.interactable = true;
        }

        private void OnStartButtonClick()
        {
            if (m_usernameField.text.Length <= 2)
            {
                Tween.Scale(m_usernameField.transform,
                    Vector3.one, Vector3.one * 1.25f, duration: 0.1f, Ease.InOutBounce, cycles: 4, CycleMode.Yoyo);
                return;
            }

            m_startButton.interactable = false;
            m_resultsButton.interactable = false;
            m_usernameField.text = "";
            m_gameManager.StartNewGame(m_usernameField.text);
        }

        private void OnResultButtonClick()
        {
            m_resultsPanel.gameObject.SetActive(true);
        }

        private void OnUsernameValueChanged(string value)
        {
            if (m_usernameField.text.Length > 2)
            {
                m_startButton.image.sprite = m_startButtonEnabled;
            }
            else
            {
                m_startButton.image.sprite = m_startButtonDisabled;
            }
        }
    }
}