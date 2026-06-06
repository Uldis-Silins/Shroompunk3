using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class LoadingScreen : MonoBehaviour
{
    public enum FadeStateType { None = -1, FadeIn, FadeOut }

    public Action onFadeIn;
    public Action onFadeOut;
    
    [SerializeField] private Image m_backgroundImage;
    [SerializeField] private Image m_logo;
    [SerializeField] private TextMeshProUGUI m_loadingText;
    
    private FadeStateType m_fadeState = FadeStateType.None;
    private float m_fadeInDuration = 0.25f;
    private float m_fadeOutDuration = 0.25f;

    private float m_fadeTimer;
    
    private bool m_queueFadeOut = false;

    private void Awake()
    {
        m_backgroundImage.material.SetFloat("_DissolveAmount", 0f);
    }

    public void Show()
    {
        if(m_fadeState == FadeStateType.FadeIn) return;
        
        m_backgroundImage.gameObject.SetActive(true);
        m_loadingText.gameObject.SetActive(false);
        m_logo.gameObject.SetActive(false);
        m_fadeTimer = m_fadeInDuration;
        m_fadeState = FadeStateType.FadeIn;
    }

    public void Hide()
    {
        if (m_fadeState == FadeStateType.FadeIn)
        {
            m_queueFadeOut = true;
        }
        else
        {
            Tween.Scale(m_logo.rectTransform, Vector3.one, Vector3.one * 0.5f, duration: 0.25f, startDelay: 0.15f, ease: Ease.InBack).OnComplete(
                () => { m_logo.gameObject.SetActive(false); });
            Tween.Scale(m_loadingText.rectTransform, Vector3.one, Vector3.one * 0.5f, duration: 0.25f, ease: Ease.InBack).OnComplete(
                () => { m_loadingText.gameObject.SetActive(false); });
            
            m_fadeTimer = m_fadeOutDuration;
            m_fadeState = FadeStateType.FadeOut;
        }
    }

    private void Update()
    {
        if (m_fadeState == FadeStateType.FadeIn)
        {
            m_fadeTimer -= Time.deltaTime;
            m_backgroundImage.material.SetFloat("_DissolveAmount", 1f - (m_fadeTimer /  m_fadeInDuration));

            if (m_fadeTimer <= 0f)
            {
                if (!m_queueFadeOut)
                {
                    m_logo.gameObject.SetActive(true);
                    Tween.Scale(m_logo.rectTransform, Vector3.one * 0.5f, Vector3.one, duration: 0.25f, ease: Ease.OutBack);
                    m_loadingText.gameObject.SetActive(true);
                    Tween.Scale(m_loadingText.rectTransform, Vector3.one * 0.5f, Vector3.one, startDelay: 0.15f, duration: 0.25f);
                }
                else
                {
                    m_fadeTimer = m_fadeOutDuration;
                    // Tween.Scale(m_logo.rectTransform, Vector3.one, Vector3.one * 0.5f, duration: 0.25f, startDelay: 0.15f, ease: Ease.InBack).OnComplete(
                    //     () => { m_logo.gameObject.SetActive(false); });
                    // Tween.Scale(m_loadingText.rectTransform, Vector3.one, Vector3.one * 0.5f, duration: 0.25f, ease: Ease.InBack).OnComplete(
                    //     () => { m_loadingText.gameObject.SetActive(false); });
                }
                
                m_fadeState = m_queueFadeOut ? FadeStateType.FadeOut : FadeStateType.None;
                onFadeIn?.Invoke();
            }
        }

        if (m_fadeState == FadeStateType.FadeOut)
        {
            m_fadeTimer -= Time.deltaTime;
            m_backgroundImage.material.SetFloat("_DissolveAmount", m_fadeTimer /  m_fadeInDuration);

            if (m_fadeTimer <= 0f)
            {
                m_backgroundImage.gameObject.SetActive(false);
                m_fadeState = FadeStateType.None;
                onFadeOut?.Invoke();
                gameObject.SetActive(false);
            }
        }
    }
}