using System;
using System.Collections;
using MenuStates;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Base menu state types. Substates should be handled by the containing state.
    /// </summary>
    public enum MenuStateType { None = -1, Start, Game, Map, Basket, Results }

    [System.Serializable]
    public class MenuState
    {
        public MenuStateType type;
        public GameObject menu;
    }
    
    /// <summary>
    /// Singleton should always stay private to avoid state leaks.
    /// </summary>
    private static GameManager s_instance;
    
    [SerializeField, Tooltip("State GameObjects by state type")]
    private MenuState[] m_states;

    [SerializeField] private GameController m_gameController;
    [SerializeField] private LoadingScreen m_loadingScreen;
    [SerializeField] private InfoPopup m_infoPopup;
    [SerializeField] private MenuState_Results m_results;
    
    private MenuStateType m_previousState = MenuStateType.None;

    public MenuStateType CurrentState { get; private set; } = MenuStateType.None;
    public string Username { get; private set; }

    private void Awake()
    {
        if (s_instance != null)
        {
            Destroy(this);
            throw new Exception("There is a GameManager in the scene already. Destroying this. Check scene for duplicates.");
        }

        s_instance = this;
        
        Screen.sleepTimeout =  SleepTimeout.NeverSleep;
    }

    private void OnEnable()
    {
        m_gameController.onGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        m_gameController.onGameOver -= OnGameOver;
    }

    private void Start()
    {
        SetMenuState(MenuStateType.Start);
    }

    public bool SetMenuState(MenuStateType state, bool ignoreIfActive = true)
    {
        if (ignoreIfActive && state == CurrentState)
        {
            Debug.LogWarning($"{state} is already active");
            return false;
        }
        
        m_previousState = CurrentState;
        if(m_previousState != MenuStateType.None) Array.Find(m_states, x => x.type == m_previousState).menu.GetComponent<MenuState_Base>().Deactivate();
        
        MenuState targetState = Array.Find(m_states, x => x.type == state);

        if (targetState == null)
        {
            Debug.LogError($"{state} is not found in states array");
            return false;
        }
        
        CurrentState = state;
        targetState.menu.GetComponent<MenuState_Base>().Activate();
        
        return true;
    }

    public void StartNewGame(string username)
    {
        m_loadingScreen.gameObject.SetActive(true);
        m_loadingScreen.Show();
        
        m_gameController.StartNewGame(
            () => { m_loadingScreen.onFadeIn += OnStartGameFadeIn; m_loadingScreen.Hide(); },
            () =>
            {
                m_loadingScreen.Hide(); 
                SetMenuState(MenuStateType.Start, false);
                m_infoPopup.gameObject.SetActive(true);
                m_infoPopup.Show("AR not supported", "Your device does not support this game");
            });
        
        Username = username;
    }

    private void OnStartGameFadeIn()
    {
        SetMenuState(MenuStateType.Game);
        m_loadingScreen.onFadeIn -= OnStartGameFadeIn;
    }

    public void ShowGame()
    {
        SetMenuState(MenuStateType.Game);
        m_gameController.ToggleMapContainer(false);
    }

    public void ShowStartMenu()
    {
        SetMenuState(MenuStateType.Start);
    }

    public void ShowMap()
    {
        SetMenuState(MenuStateType.Map);
        m_gameController.ToggleMapContainer(true);
    }

    public void ShowBasket()
    {
        SetMenuState(MenuStateType.Basket);
        m_gameController.ToggleMapContainer(false);
    }

    private void OnGameOver(int score)
    {
        m_results.AddHighScore(Username, score);
        StartCoroutine(ShowResultsDelayed(3f));
    }

    private IEnumerator ShowResultsDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetMenuState(MenuStateType.Results);
    }
}
