using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuStates
{
    public class MenuState_Game : MenuState_Base
    {
        [SerializeField] private Button m_mapButton;
        [SerializeField] private Button m_basketButton;
        [SerializeField] private GameObject m_bottomPanel;
        
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private GameController m_gameController;

        [SerializeField] private RectTransform m_timePanel;
        [SerializeField] private TextMeshProUGUI m_timeText;
        [SerializeField] private RectTransform m_scorePanel;
        [SerializeField] private TextMeshProUGUI m_scoreText;
        [SerializeField] private RectTransform m_gameOverContainer;

        [Header("Preview")]
        [SerializeField] private GameObject m_blurBackground;
        [SerializeField] private RawImage m_previewImage;
        [SerializeField] private GameObject m_previewContainer;
        [SerializeField] private Transform m_previewPoint;

        [SerializeField] private Image m_addMushroomPrefab;
        [SerializeField] private Button m_nayButton;
        [SerializeField] private Button m_yayButton;
        
        private MushroomData m_currentPreviewMushroom;
        private int m_currentGrowthScore;
        
        private Vector2 m_startDragPosition;
        
        public bool InPreview { get; private set; }

        private void OnEnable()
        {
            m_mapButton.onClick.AddListener(m_gameManager.ShowMap);
            m_basketButton.onClick.AddListener(m_gameManager.ShowBasket);
            
            m_gameController.onScoreChanged += OnScoreChanged;
            m_gameController.onTimeChanged += OnTimeChanged;
            m_gameController.onMushroomPickup += OnMushroomPickup;
            m_gameController.onGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            m_mapButton.onClick.RemoveAllListeners();
            m_basketButton.onClick.RemoveAllListeners();
            
            m_gameController.onScoreChanged -= OnScoreChanged;
            m_gameController.onTimeChanged -= OnTimeChanged;
            m_gameController.onMushroomPickup -= OnMushroomPickup;
            m_gameController.onGameOver -= OnGameOver;
        }

        private void Update()
        {
            if (InPreview)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    m_startDragPosition = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    float swipeDirection = Input.mousePosition.x - m_startDragPosition.x;

                    const float swipeDistancePx = 200;
                    
                    if (swipeDirection > swipeDistancePx)
                    {
                        m_yayButton.gameObject.SetActive(false);
                        m_nayButton.gameObject.SetActive(false);
                        Tween.LocalPosition(m_previewImage.transform, m_previewImage.transform.right * 200f,
                            duration: 0.15f).OnComplete(() =>
                            {
                                m_gameController.AddMushroom(m_currentPreviewMushroom, m_currentGrowthScore);
                                AnimateMushroomAdd(m_currentPreviewMushroom.Icon);
                                m_previewImage.transform.localPosition = Vector3.zero;
                                DisablePreview();
                            }
                        );
                    }
                    else if(swipeDirection < -swipeDistancePx)
                    {
                        m_yayButton.gameObject.SetActive(false);
                        m_nayButton.gameObject.SetActive(false);
                        Tween.LocalPosition(m_previewImage.transform, -m_previewImage.transform.right * 200f,
                            duration: 0.15f).OnComplete(() =>
                            {
                                m_previewImage.transform.localPosition = Vector3.zero;
                                DisablePreview();
                            }
                        );
                    }
                }
            }
        }

        public override void Deactivate()
        {
            m_gameOverContainer.gameObject.SetActive(false);
            base.Deactivate();
        }

        private void OnScoreChanged(int prevScore, int currentScore)
        {
            m_scoreText.text = currentScore.ToString();
        }
        
        private void OnTimeChanged(float time)
        {
            m_timeText.text = string.Format("{0:00}:{1:00}", (int)(time / 60), (int)(time % 60));
        }

        private void OnMushroomPickup(MushroomData data, int growth)
        {
            m_blurBackground.SetActive(true);
            
            if(m_previewPoint.childCount > 0) 
            {
                for (int i = m_previewPoint.childCount; --i > 0;)
                {
                    Destroy(m_previewPoint.GetChild(i).gameObject);
                }
            }
            
            var instance = Instantiate(data.GrowingPrefab, m_previewPoint);
            instance.layer = LayerMask.NameToLayer("Preview");
            instance.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Preview");
            m_previewContainer.SetActive(true);
            
            m_previewImage.gameObject.SetActive(true);
            
            m_yayButton.gameObject.SetActive(true);
            m_nayButton.gameObject.SetActive(true);
            
            m_currentPreviewMushroom = data;
            m_currentGrowthScore = growth;
            m_gameController.IsInPreview = true;
            InPreview = true;
        }

        private void DisablePreview()
        {
            m_currentPreviewMushroom = null;
            Destroy(m_previewPoint.GetChild(0).gameObject);
            m_previewContainer.SetActive(false);
            m_previewImage.gameObject.SetActive(false);
            m_blurBackground.SetActive(false);
            m_currentGrowthScore = 0;
            m_yayButton.gameObject.SetActive(false);
            m_nayButton.gameObject.SetActive(false);
            m_gameController.IsInPreview = false;
        }

        public void OnYayButtonClick()
        {
            Tween.Scale(m_yayButton.transform, Vector3.one * 1.25f, duration: 0.15f, Ease.InOutBounce, cycles: 4,
                CycleMode.Yoyo).OnComplete(() => { m_yayButton.gameObject.SetActive(false); }
            );
            
            m_nayButton.gameObject.SetActive(false);
            Tween.LocalPosition(m_previewImage.transform, m_previewImage.transform.right * 200f,
                duration: 0.15f).OnComplete(() =>
                {
                    m_gameController.AddMushroom(m_currentPreviewMushroom, m_currentGrowthScore);
                    AnimateMushroomAdd(m_currentPreviewMushroom.Icon);
                    m_previewImage.transform.localPosition = Vector3.zero;
                    DisablePreview();
                }
            );
        }

        public void OnNayButtonClick()
        {
            m_yayButton.gameObject.SetActive(false);
            Tween.Scale(m_nayButton.transform, Vector3.one * 1.25f, duration: 0.15f, Ease.InOutBounce, cycles: 4,
                CycleMode.Yoyo).OnComplete(() => { m_nayButton.gameObject.SetActive(false); }
            );
            Tween.LocalPosition(m_previewImage.transform, -m_previewImage.transform.right * 200f,
                duration: 0.15f).OnComplete(() =>
                {
                    m_previewImage.transform.localPosition = Vector3.zero;
                    DisablePreview();
                }
            );
        }

        private void AnimateMushroomAdd(Sprite mushroomSprite)
        {
            Image instance = Instantiate(m_addMushroomPrefab, transform);
            instance.transform.position = m_previewImage.transform.position;
            instance.sprite = mushroomSprite;
            Tween.Scale(instance.transform, Vector3.zero, duration: 0.2f, Ease.InExpo);
            Tween.LocalPosition(instance.transform, -Vector3.up * Screen.height, duration: 0.25f, Ease.InOutCubic).OnComplete(
                () =>
                {
                    Tween.Scale(m_basketButton.transform, Vector3.one * 1.5f, duration: 0.15f, Ease.InOutBounce,
                        cycles: 2, CycleMode.Yoyo);
                    Destroy(instance.gameObject);
                });
        }

        private void OnGameOver(int score)
        {
            m_timePanel.gameObject.SetActive(false);
            m_scorePanel.gameObject.SetActive(false);
            m_gameOverContainer.gameObject.SetActive(true);
            m_bottomPanel.gameObject.SetActive(false);

            if (m_gameController.IsInPreview)
            {
                DisablePreview();
            }
        }
    }
}