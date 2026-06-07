using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public enum GameStateType
    {
        None = -1,
        Running,
        GameOver
    }
    
    public Action<int, int> onScoreChanged; // T1: previous score, T2: current score
    public Action<float> onTimeChanged;
    public Action<MushroomData, int> onMushroomPickup;  // T1: mushroom data, T2: bonus growth score
    public Action<int> onGameOver;  // T1: score
    
    [SerializeField] private ARSession m_session;
    [SerializeField] private Camera m_arCamera;
    [SerializeField] private GameObject m_mapContainer;
    
    [SerializeField] private GameConfig m_gameConfig;
    
    [SerializeField] private BasketController m_basketController;
    [SerializeField] private MushroomSpawner m_mushroomSpawner;
    [SerializeField] private VegetationSpawner m_vegetationSpawner;
    
    [SerializeField] private Camera m_camera;
    
    private Coroutine m_waitingARInitialization;
    
    private readonly float m_waitARInitializationTimeout = 5f;

    private float m_time;
    private int m_score;
    
    private float m_scoreTimer;
    private readonly float m_subtrackScoreTime = 1f;
    
    public GameStateType CurrentGameState { get; private set; } = GameStateType.None;
    public bool IsInPreview { get; set; }

    private void Start()
    {
        m_session.enabled = false;
        m_arCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (CurrentGameState == GameStateType.Running)
        {
            if(GameManager.CurrentState != GameManager.MenuStateType.Game) return;
            
            m_time -= Time.deltaTime;
            onTimeChanged?.Invoke(m_time);

            if (m_time <= 0f)
            {
                m_time = 0f;
                m_mushroomSpawner.SpawningEnabled = false;
                m_vegetationSpawner.SpawningEnabled = false;
                m_mushroomSpawner.ResetSpawn();
                onGameOver?.Invoke(m_score);
                m_mushroomSpawner.DespawnMushrooms();
                m_vegetationSpawner.DespawnVegetation();
                m_session.Reset();
                m_session.enabled = false;
                m_arCamera.gameObject.SetActive(false);
                
                CurrentGameState = GameStateType.GameOver;
            }
            else
            {
                m_mushroomSpawner.SetSpawnChance(m_time / m_gameConfig.Time);
            }

            if (m_scoreTimer <= 0f)
            {
                m_scoreTimer = m_subtrackScoreTime;

                int subScore = m_basketController.GetPoisonousCount() * 2;
                AddScore(-subScore);
            }

            m_scoreTimer -= Time.deltaTime;

            if (!IsInPreview && Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(m_camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 0.5f,
                        1 << LayerMask.NameToLayer("Mushroom")))
                {
                    Mushroom hitMushroom = hit.collider.GetComponent<Mushroom>();

                    if (hitMushroom != null)
                    {
                        onMushroomPickup?.Invoke(hitMushroom.Data, hitMushroom.Growth);
                        Destroy(hitMushroom.gameObject);
                        m_mushroomSpawner.PickupMushroom(hitMushroom.Data);
                    }
                }
            }
        }
    }

    public void StartNewGame(Action onARSessionStarted, Action onARNotSupported)
    {
        if (m_waitingARInitialization == null)
        {
            m_waitingARInitialization = StartCoroutine(WaitARInitialization(onARSessionStarted, onARNotSupported));
        }
        
        ToggleMapContainer(false);
        CurrentGameState = GameStateType.Running;
        m_time = m_gameConfig.Time * 60f;
        m_score = 0;
        
        onScoreChanged?.Invoke(m_score, m_score);
        onTimeChanged?.Invoke(m_time);

        m_mushroomSpawner.SpawningEnabled = true;
        m_vegetationSpawner.SpawningEnabled = true;
    }
    
    private IEnumerator WaitARInitialization(Action onARSessionStarted, Action onARNotSupported)
    {
        if ((ARSession.state == ARSessionState.None) ||
            (ARSession.state == ARSessionState.CheckingAvailability))
        {
            Debug.Log("Checking for AR availability");
            yield return ARSession.CheckAvailability();
            
            float timeout = Time.time + m_waitARInitializationTimeout;

            while (Time.time < timeout)
            {
                yield return null;
            }

            if ((ARSession.state == ARSessionState.None) ||
                (ARSession.state == ARSessionState.CheckingAvailability))
            {
                onARNotSupported?.Invoke();
            }
        }
        
        if (ARSession.state == ARSessionState.Unsupported)
        {
            // TODO: fallback and show unsupported popup
            onARNotSupported?.Invoke();
            Debug.LogError("AR Session unsupported");
        }
        
        if (ARSession.state == ARSessionState.Ready)   // This is set before SessionInitializing and SessionTracking
        {
            onARSessionStarted?.Invoke();
            m_session.enabled = true;
            m_arCamera.gameObject.SetActive(true);
            Debug.Log("AR Session Ready");
        }
        
        m_waitingARInitialization = null;
    }

    public void ToggleMapContainer(bool isEnabled)
    {
        m_mapContainer.gameObject.SetActive(isEnabled);
        m_arCamera.enabled = !isEnabled;
    }

    public void AddScore(int amount)
    {
        int prevScore = m_score;
        m_score = Mathf.Clamp(m_score + amount, 0, int.MaxValue);
        onScoreChanged?.Invoke(prevScore, m_score);
    }

    public void AddMushroom(MushroomData data, int growth)
    {
        if (m_basketController.AddItem(data, growth))
        {
            float scoreMultiplier = 1f;

            if (m_basketController.IsFirstFind(data))
            {
                scoreMultiplier += 0.5f;
                m_basketController.AddToFoundIDs(data);
                Debug.Log("Is first find");
            }

            int score = Mathf.FloorToInt((data.Value + growth) * scoreMultiplier);
            int comboBonus = m_basketController.ComboCount > 0 ? (m_basketController.ComboCount - 1) * 10 : 0;
            score += comboBonus;
            Debug.Log("Combo: " + comboBonus);
            AddScore(score);
                
            m_scoreTimer = m_subtrackScoreTime;
        }
    }
}