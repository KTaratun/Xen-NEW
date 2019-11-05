using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManagerScript : MonoBehaviour {

    static public List<PanelScript> m_history;
    static public List<PanelScript> m_allPanels;

    static public bool m_locked;
    static public PanelScript m_confirmPanel;

    // Use this for initialization
    void Start ()
    {
        m_locked = false;

        if (m_history == null)
            m_history = new List<PanelScript>();

        if (GameObject.Find("Confirmation Panel"))
            m_confirmPanel = GameObject.Find("Confirmation Panel").GetComponent<PanelScript>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        
    }

    public void MenuPanelInit(string _canvasName, GameObject _main)
    {
        if (m_history != null)
            m_history.Clear();

        Canvas can = GameObject.Find(_canvasName).GetComponent<Canvas>();
        PanelScript[] pans = can.GetComponentsInChildren<PanelScript>();
        m_allPanels = new List<PanelScript>();

        for (int i = 0; i < pans.Length; i++)
        {
            if (pans[i].transform.parent.name == _canvasName)
            {
                pans[i].m_main = _main;
                PanelScript[] children = pans[i].GetComponentsInChildren<PanelScript>();
                for (int j = 0; j < children.Length; j++)
                {
                    children[j].m_main = _main;
                    children[j].m_pMan = this;
                }

                if (_main.tag == "Board")
                {
                    ButtonScript[] buttons = pans[i].GetComponentsInChildren<ButtonScript>();
                    for (int j = 0; j < buttons.Length; j++)
                        buttons[j].m_boardScript = _main.GetComponent<BoardScript>();
                }

                m_allPanels.Add(pans[i]);
            }
        }
    }

    static public void CloseHistory()
    {
        while (m_history.Count > 0)
        {
            m_history[0].m_slideScript.ClosePanel();
            m_history.RemoveAt(0);
        }
    }

    static public bool CheckIfPanelOpen()
    {
        for (int i = 0; i < m_allPanels.Count; i++)
            if (m_allPanels[i].m_slideScript && m_allPanels[i].m_slideScript.m_inView && m_allPanels[i].m_slideScript.m_direction == PanelSlideScript.dir.UP)
                return true;

        return false;
    }

    static public PanelScript GetCurrentPanel()
    {
        if (m_history.Count > 0)
            return m_history[m_history.Count - 1];

        return null;
    }

    static public void RemoveFromHistory(string _name)
    {
        if (_name == "")
        {
            m_history[m_history.Count - 1].m_slideScript.ClosePanel();
            m_history.RemoveAt(m_history.Count - 1);
            return;
        }

        for (int i = 0; i < m_history.Count; i++)
        {
            if (m_history[i].name == _name)
            {
                m_history[i].m_slideScript.ClosePanel();
                m_history.RemoveAt(i);
            }
        }
    }

    static public PanelScript GetPanel(string _name)
    {
        for (int i = 0; i < m_allPanels.Count; i++)
            if (m_allPanels[i].name == _name)
                return m_allPanels[i].GetComponent<PanelScript>();

        return null;
    }
}
