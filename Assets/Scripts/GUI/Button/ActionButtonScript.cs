using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionButtonScript : EnergyButtonScript {

    public ActionScript m_action;
    private ActionViewerPanel m_actionViewer;

    // Use this for initialization
    new void Start () {
        base.Start();
        if (m_energyPanel.Length == 0)
            EnergyInit();

        m_main = m_parent.transform.parent.GetComponent<SlidingPanelScript>();
        m_actionViewer = GameObject.Find("ActionViewer Panel").GetComponent<ActionViewerPanel>();

        if (GameObject.Find("Scene Manager"))
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
    }

    // Update is called once per frame
    void Update () {

    }

    override public void HoverTrue(BaseEventData eventData)
    {
        if (m_boardScript)
        {
            if (m_boardScript.m_highlightedTile && m_boardScript.m_highlightedTile.m_targetRadius.Count > 0)
                m_boardScript.m_highlightedTile.ClearRadius();

            m_boardScript.m_hoverButton = GetComponent<Button>();
        }

        if (m_boardScript.m_currButton)
            return;

        if (m_boardScript)
        {
            if (m_boardScript.m_highlightedTile)
            {
                if (m_boardScript.m_highlightedTile.m_radius.Count > 1)
                    m_boardScript.m_highlightedTile.ClearRadius();
                else
                    m_boardScript.m_highlightedTile.ClearTile();
                m_boardScript.m_oldTile = null;
            }
        }

        if (m_boardScript && m_boardScript.m_camIsFrozen ||
            m_boardScript && m_boardScript.m_currCharScript.m_isAI ||
            m_boardScript.m_currButton)
            return;


        Text text = GetComponent<Button>().GetComponentInChildren<Text>();

        // REFACTOR: That's so wack though
        //if (text.text == "EMPTY" || !m_main || !m_parent || GetComponent<Image>().color == new Color(1, .5f, .5f, 1) ||
        //    m_parent && m_parent.m_main && m_parent.m_main.GetComponent<PanelScript>() && !m_parent.m_main.GetComponent<SlidingPanelScript>().m_inView)
        //    return;

        if (text.text == "EMPTY")
            return;

        m_main.m_cScript = m_parent.m_cScript.GetComponent<CharacterScript>();
        m_object = m_main.m_cScript.gameObject;

        if (m_main.m_inView) // Need this check to avoid selecting another action while menu is moving
            m_main.m_cScript.m_currAction = m_action;
        if (m_main.m_inView && m_boardScript) // Need this check to avoid selecting another action while menu is moving
        {
            if (GetComponent<Button>().GetComponent<Image>().color == PanelScript.b_isFree)
            {
                m_boardScript.m_currCharScript.m_currAction = m_action;
                m_action.ActionTargeting(m_boardScript.m_currCharScript.m_tile);
            }
            else
            {
                CharacterScript charScript = m_main.m_cScript;
                charScript.m_currAction = m_action;
                m_action.ActionTargeting(charScript.m_tile);
            }
        }

        m_actionViewer.m_cScript = m_main.m_cScript;
        m_actionViewer.PopulatePanel();
    }

    override public void HoverFalse()
    {
        if (m_boardScript)
            m_boardScript.m_hoverButton = null;

        if (GetComponent<Button>().GetComponentInChildren<Text>().text == "EMPTY" || m_boardScript && m_boardScript.m_currCharScript.m_isAI)
            return;

        if (m_boardScript && !m_boardScript.m_currButton || !m_boardScript)
        {
            m_actionViewer.ClosePanel();
            if (m_boardScript)
                m_boardScript.m_currCharScript.m_currAction = null;
        }

        if (m_boardScript && !m_boardScript.m_currButton && GetComponent<Image>().color != Color.cyan)
        {
            TileScript selectedTileScript = null;
            if (m_panMan.GetPanel("HUD Panel LEFT").transform == transform.parent.parent)
                selectedTileScript = m_panMan.GetPanel("HUD Panel LEFT").m_cScript.m_tile;
            else if (m_panMan.GetPanel("HUD Panel RIGHT").transform == transform.parent.parent)
                selectedTileScript = m_panMan.GetPanel("HUD Panel RIGHT").m_cScript.m_tile;

            //if (selectedTileScript.m_radius.Count > 0)
            selectedTileScript.ClearRadius();

            //if (m_boardScript.m_highlightedTile)
            //{
            //    selectedTileScript = m_boardScript.m_highlightedTile;
            //    if (selectedTileScript.m_targetRadius.Count > 0)
            //        selectedTileScript.ClearRadius();
            //}
        }
    }

    override public void Select()
    {
        //if (//GetComponent<Button>().GetComponent<Image>().color == PanelScript.b_isDisallowed || //refactor
        //    //m_boardScript && m_object != m_boardScript.m_currCharScript.gameObject && GetComponent<Button>().GetComponent<Image>().color == Color.white ||
        //    //m_boardScript && m_boardScript.m_isForcedMove ||
        //    SlidingPanelManagerScript.m_confirmPanel.m_inView ||
        //    m_boardScript && m_boardScript.m_camIsFrozen ||
        //    m_boardScript && m_boardScript.m_currCharScript.m_isAI)
        //    return;

        //TileScript selectedTileScript = m_boardScript.m_currCharScript.m_tile.GetComponent<TileScript>();
        //if (selectedTileScript.m_radius.Count > 0)
        //    selectedTileScript.ClearRadius();
        //
        //if (m_boardScript.m_highlightedTile)
        //{
        //    selectedTileScript = m_boardScript.m_highlightedTile.GetComponent<TileScript>();
        //    selectedTileScript.ClearRadius();
        //}
        //

        if (transform.parent.parent.name == "HUD Panel RIGHT")
            return;

        m_boardScript.m_currCharScript.m_currAction = m_action;
        m_main.m_cScript = m_boardScript.m_currCharScript;

        if (m_boardScript.m_currButton)
        {
            m_boardScript.m_currButton.GetComponent<Image>().color = m_boardScript.m_currButton.GetComponent<ButtonScript>().m_oldColor;
            if (m_boardScript.m_currButton.GetComponent<SlidingPanelScript>())
                m_boardScript.m_currButton.GetComponent<SlidingPanelScript>().m_inView = false;

            if (gameObject == m_boardScript.m_currButton.gameObject)
            {
                m_boardScript.m_currButton = null;
                m_boardScript.m_currCharScript.m_currAction = null;

                return;
            }
            else
            {
                TileScript selectedTileScript = m_boardScript.m_currCharScript.m_tile.GetComponent<TileScript>();
                selectedTileScript.ClearRadius();

                m_actionViewer.m_cScript = m_main.m_cScript;
                m_actionViewer.PopulatePanel();
            }
        }

        m_oldColor = GetComponent<Image>().color;
        GetComponent<Image>().color = Color.cyan;
        m_boardScript.m_currButton = GetComponent<Button>();

        if (GetComponent<SlidingPanelScript>())
            GetComponent<SlidingPanelScript>().m_inView = true;

        if (m_panMan.GetPanel("Choose Panel").m_inView)
            m_boardScript.m_isForcedMove = null;

        m_panMan.GetPanel("ActionViewer Panel").transform.Find("ActionView Slide").GetComponent<SlidingPanelScript>().ClosePanel();

        if (m_oldColor == PanelScript.b_isSpecial)
        {
            m_boardScript.m_selected.ClearRadius();
            m_panMan.GetPanel("HUD Panel RIGHT").ClosePanel();
            return;
        }

        m_action.ActionTargeting(m_boardScript.m_currCharScript.m_tile);
        
        if (m_panMan.GetPanel("HUD Panel RIGHT").m_inView)
            m_panMan.GetPanel("HUD Panel RIGHT").PopulatePanel();
    }
}
