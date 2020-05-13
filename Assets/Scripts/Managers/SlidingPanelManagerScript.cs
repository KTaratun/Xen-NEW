using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingPanelManagerScript : MonoBehaviour {

    public List<SlidingPanelScript> m_history;
    public List<SlidingPanelScript> m_allPanels;

    public bool m_locked;
    public SlidingPanelScript m_confirmPanel;

    private GameManagerScript m_gamMan;

    // Use this for initialization
    void Start ()
    {
        m_locked = false;

        if (m_history == null)
            m_history = new List<SlidingPanelScript>();

        if (GameObject.Find("Confirmation Panel"))
            m_confirmPanel = GameObject.Find("Confirmation Panel").GetComponent<SlidingPanelScript>();

        if (GameObject.Find("Scene Manager"))
            m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();
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
        SlidingPanelScript[] pans = can.GetComponentsInChildren<SlidingPanelScript>();
        m_allPanels = new List<SlidingPanelScript>();

        for (int i = 0; i < pans.Length; i++)
        {
            if (pans[i].transform.parent.name == _canvasName)
            {
                SlidingPanelScript[] children = pans[i].GetComponentsInChildren<SlidingPanelScript>();
                for (int j = 0; j < children.Length; j++)
                    children[j].m_panMan = this;

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

    public void ClosePanelLast()
    {
        if (m_history.Count > 0)
            if (GetCurrentPanel().name == "Confirmation Panel" || GetCurrentPanel().name != "Confirmation Panel" && !m_locked)

        if (m_history.Count > 0)
        {
            RemoveFromHistory("");
            if (m_history.Count > 0)
                m_history[m_history.Count - 1].m_inView = true;
        }
    }

    public void CloseAll()
    {
        BoardScript board = GameObject.Find("Board").GetComponent<BoardScript>();

        board.m_highlightedTile = null;
        board.m_selected = null;
        board.m_isForcedMove = null;
        m_gamMan.m_currCharScript.m_currAction = null;
        TileLinkScript.ClearRadius(m_gamMan.m_currCharScript.m_tile);

        GetPanel("HUD Panel RIGHT").ClosePanel();
        CloseHistory();
        GetPanel("HUD Panel LEFT").PopulatePanel();
    }

    public void CloseHistory()
    {
        while (m_history.Count > 0)
        {
            m_history[0].ClosePanel();
            m_history.RemoveAt(0);
        }
    }

    public bool CheckIfPanelOpen()
    {
        for (int i = 0; i < m_allPanels.Count; i++)
            if (m_allPanels[i].m_inView && m_allPanels[i].m_direction == SlidingPanelScript.dir.UP)
                return true;

        return false;
    }

    public SlidingPanelScript GetCurrentPanel()
    {
        if (m_history.Count > 0)
            return m_history[m_history.Count - 1];

        return null;
    }

    public void RemoveFromHistory(string _name)
    {
        if (_name == "")
        {
            m_history[m_history.Count - 1].ClosePanel();
            m_history.RemoveAt(m_history.Count - 1);
            return;
        }

        for (int i = 0; i < m_history.Count; i++)
        {
            if (m_history[i].name == _name)
            {
                m_history[i].ClosePanel();
                m_history.RemoveAt(i);
            }
        }
    }

    public SlidingPanelScript GetPanel(string _name)
    {
        for (int i = 0; i < m_allPanels.Count; i++)
            if (m_allPanels[i].name == _name)
                return m_allPanels[i].GetComponent<SlidingPanelScript>();

        return null;
    }
}
