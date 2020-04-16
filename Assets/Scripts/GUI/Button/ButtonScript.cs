using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour {

    public Color m_oldColor;

    public GameObject m_object;
    public SlidingPanelScript m_main;
    public BoardScript m_boardScript;
    protected SlidingPanelManagerScript m_panMan;
    public PanelScript m_parent;

    // References
    public GameObject m_camera;
    public AudioSource m_audio;

    // Use this for initialization
    protected void Start ()
    {
        if (GameObject.Find("Scene Manager"))
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();

        m_audio = gameObject.AddComponent<AudioSource>();

        if (GameObject.Find("Board") && GameObject.Find("Board").GetComponent<BoardScript>())
            m_boardScript = GameObject.Find("Board").GetComponent<BoardScript>();

        m_parent = transform.parent.GetComponent<PanelScript>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    virtual public void HoverTrue(BaseEventData eventData)
    {
        if (m_boardScript && m_boardScript.m_isForcedMove && m_parent.tag != "Selector" ||
            m_panMan.m_confirmPanel.m_inView && m_parent != m_panMan.m_confirmPanel ||
            m_boardScript && m_boardScript.m_currCharScript.m_isAI)
            return;

        if (GetComponent<Button>() && GetComponent<Button>().interactable)
            m_audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Menu Sound 3"));

        if (m_boardScript && GetComponent<Button>())
            m_boardScript.m_hoverButton = GetComponent<Button>();

        // REFACTOR: Make this more like how status images are handled
        if (GetComponent<Button>() && GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            CharacterScript charScript = m_object.GetComponent<CharacterScript>();

            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;

            Renderer charRenderer = charScript.GetComponentInChildren<Renderer>();
            charRenderer.material.color = Color.cyan;
            PanelScript hudPanScript = m_panMan.GetPanel("HUD Panel RIGHT");
            hudPanScript.m_cScript = charScript;
            hudPanScript.PopulatePanel();

            return;
        }

        if (m_boardScript && m_boardScript.m_currButton)
            return;
    }

    virtual public void HoverFalse()
    {
        if (m_boardScript)
            m_boardScript.m_hoverButton = null;

        if (m_boardScript && m_boardScript.m_currCharScript.m_isAI || name == "Move" && m_boardScript.m_isForcedMove)
            return;

        if (GetComponent<Button>() && GetComponent<Button>().name == "Turn Panel Energy Button")
        {
            //Image turnPanImage = charScript.turnPanel.GetComponent<Image>();
            //turnPanImage.color = Color.cyan;
            Renderer charRenderer = m_object.GetComponentInChildren<Renderer>();
            charRenderer.material.color = m_object.GetComponent<CharacterScript>().m_teamColor;
            SlidingPanelScript hudPanScript = m_panMan.GetPanel("HUD Panel RIGHT");
            hudPanScript.ClosePanel();

            return;
        }
        else if (gameObject.tag == "Status Image")
        {
            m_main.ClosePanel();
            return;
        }

        if (!m_main)
            return;

        if (m_boardScript && !m_boardScript.m_currButton || !m_boardScript)
        {
            m_main.ClosePanel();
            if (m_boardScript)
                m_boardScript.m_currCharScript.m_currAction = null;
        }

        if (m_boardScript && !m_boardScript.m_currButton && GetComponent<Image>().color != Color.cyan)
        {
            if (GetComponent<SlidingPanelScript>())
                GetComponent<SlidingPanelScript>().m_inView = false;
            TileScript selectedTileScript = null;
            if (GetComponent<Button>().GetComponent<Image>().color == PanelScript.b_isFree)
                selectedTileScript = m_boardScript.m_currCharScript.m_tile;
            else
                selectedTileScript = m_object.GetComponent<CharacterScript>().m_tile;

            if (selectedTileScript.m_radius.Count > 0)
                selectedTileScript.ClearRadius();

            if (m_boardScript.m_highlightedTile)
            {
                selectedTileScript = m_boardScript.m_highlightedTile;
                if (selectedTileScript.m_targetRadius.Count > 0)
                    selectedTileScript.ClearRadius();
            }
        }
    }

    virtual public void Select()
    {
        //if (GetComponent<Button>().GetComponent<Image>().color == PanelScript.b_isDisallowed ||
        //    m_boardScript && m_object != m_boardScript.m_currCharScript.gameObject && GetComponent<Button>().GetComponent<Image>().color == Color.white ||
        //    m_boardScript && m_boardScript.m_isForcedMove && m_parent && m_parent.tag != "Selector" ||
        //    SlidingPanelManagerScript.m_confirmPanel.m_inView && m_parent != SlidingPanelManagerScript.m_confirmPanel ||
        //    m_boardScript && m_boardScript.m_camIsFrozen ||
        //    m_boardScript && m_boardScript.m_currCharScript.m_isAI)
        //    return;

        TileScript selectedTileScript = m_boardScript.m_currCharScript.m_tile.GetComponent<TileScript>();
        if (selectedTileScript.m_radius.Count > 0)
            selectedTileScript.ClearRadius();

        if (m_boardScript.m_highlightedTile)
        {
            selectedTileScript = m_boardScript.m_highlightedTile.GetComponent<TileScript>();
            selectedTileScript.ClearRadius();
        }

        if (m_boardScript.m_currButton)
        {
            //m_boardScript.m_currButton.GetComponent<Image>().color = m_boardScript.m_currButton.GetComponent<ButtonScript>().m_oldColor;
            if (m_boardScript.m_currButton.GetComponent<SlidingPanelScript>())
                m_boardScript.m_currButton.GetComponent<SlidingPanelScript>().m_inView = false;

            if (gameObject == m_boardScript.m_currButton.gameObject)
            {
                m_boardScript.m_currButton = null;
                m_boardScript.m_currCharScript.m_currAction = null;
                //if (!m_hovered)
                //    GetComponent<ButtonScript>().m_main.ClosePanel();
                return;
            }
        }

        //m_oldColor = GetComponent<Image>().color;
        //GetComponent<Image>().color = Color.cyan;
        if (GetComponent<SlidingPanelScript>())
            GetComponent<SlidingPanelScript>().m_inView = true;
        m_boardScript.m_currButton = GetComponent<Button>();
        //m_main.m_cScript = m_boardScript.m_currCharScript;
        //m_main.PopulatePanel();

        if (gameObject.name == "Move" && !m_panMan.GetPanel("Choose Panel").m_inView)
        {
            m_panMan.GetPanel("ActionViewer Panel").ClosePanel();
            m_boardScript.m_currCharScript.MovementSelection(0);
        }
    }

    public void SetCameraTarget()
    {
        CameraScript camScript = m_camera.GetComponent<CameraScript>();
        camScript.m_target = m_object;
    }
}
