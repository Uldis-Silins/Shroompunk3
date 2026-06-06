using System;
using UnityEngine;

namespace MenuStates
{
    public abstract class MenuState_Base : MonoBehaviour
    {
        [System.Serializable]
        public class MenuElement
        {
            public GameObject gameObject;
            // TODO: Tween onEnable
            // TODO: Tween onDisable
        }

        [SerializeField] private MenuElement[] m_menuElements;

        private void OnValidate()
        {
            if (m_menuElements == null || m_menuElements.Length == 0)
            {
                m_menuElements = new MenuElement[transform.childCount];
                
                for (int i = 0; i < transform.childCount; i++)
                {
                    m_menuElements[i] = new MenuElement() { gameObject = transform.GetChild(i).gameObject };
                }
            }
        }

        [ExecuteAlways] public virtual void Activate() { Array.ForEach(m_menuElements, menuElement => menuElement.gameObject.SetActive(true)); }
        [ExecuteAlways] public virtual void Deactivate() { Array.ForEach(m_menuElements, menuElement => menuElement.gameObject.SetActive(false)); }
    }
}