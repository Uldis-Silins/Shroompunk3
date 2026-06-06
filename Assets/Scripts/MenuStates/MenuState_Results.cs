using System;
using System.Collections.Generic;
using MenuStates;
using UnityEngine;

public class MenuState_Results : MenuState_Base
{
    [SerializeField] private RectTransform m_listContent;
    [SerializeField] private ResultsElement m_resultsElementPrefab;

    private List<ResultsElement> m_spawnedResults;
    
    private Dictionary<string, int> m_results;

    private void Awake()
    {
        m_results = new Dictionary<string, int>();
        m_spawnedResults = new List<ResultsElement>();
    }

    public void AddHighScore(string username, int score)
    {
        m_results.Add(username, score);

        foreach (ResultsElement result in m_spawnedResults)
        {
            Destroy(result.gameObject);
        }

        var instance = Instantiate(m_resultsElementPrefab, m_listContent);
        m_spawnedResults.Add(instance);
        instance.Initialize(username, score);
    }
}