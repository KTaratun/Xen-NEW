using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class TileScript : NetworkBehaviour {

    public int m_id;
    public GameObject m_holding;
    public TileScript[] m_neighbors;
    public List<TileScript> m_radius;
    public List<TileScript> m_targetRadius;
    public int m_x;
    public int m_z;
    public Color m_oldColor;
    public GameObject m_traversed; // Used for path planning and to determine future occupancy of a tile
    public TileScript m_parent; // Used for determining path for AI

    // REFERENCES
    private SlidingPanelManagerScript m_panMan;
    private GameManagerScript m_gamMan;
    private CustomDirect m_cD;
    private BoardScript m_boardScript;
    //private LineRenderer m_line;

    // Use this for initialization
    void Start()
    {
        if (GameObject.Find("Scene Manager"))
        {
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
            m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();
        }

        if (GameObject.Find("Network"))
            m_cD = GameObject.Find("Network").GetComponent<CustomDirect>();

        if (GameObject.Find("Board"))
            m_boardScript = GameObject.Find("Board").GetComponent<BoardScript>();

        m_traversed = null;
        m_radius = new List<TileScript>();
        m_oldColor = Color.black;

        //m_line = gameObject.AddComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update() {
    }

    public void OnMouseDown()
    {
        if (m_panMan.CheckIfPanelOpen() || m_boardScript.m_camIsFrozen || m_boardScript.m_hoverButton || !m_gamMan.m_currCharScript || EventSystem.current.IsPointerOverGameObject())
            return;

        TileScript oldTile = m_boardScript.m_selected;
        m_boardScript.m_selected = this;

        Color color = GetComponent<Renderer>().material.color;

        if (color == TileLinkScript.c_neutral) // If selecting a tile while moving
            NeutralTileClick(oldTile);
        if (color == Color.blue) // If tile is blue when clicked, perform movement code
            m_panMan.GetPanel("Confirmation Panel").GetComponent<ConfirmationPanelScript>().ConfirmationButton("Move");
        // If selecting a tile that is holding a character while using an action
        else if (color == Color.red && m_holding || color == Color.green && m_holding ||
            color == TileLinkScript.c_radius) // Otherwise if color is red, perform action code
            m_panMan.GetPanel("Confirmation Panel").GetComponent<ConfirmationPanelScript>().ConfirmationButton("Action");
    }

    private void NeutralTileClick(TileScript _oldTile)
    {
        // If we select a different tile than the current selected tile
        if (this != _oldTile && m_holding != m_gamMan.m_currCharScript.gameObject) //&& m_gamMan.m_currCharScript.m_tile.m_radius.Count == 0)
        {
            // If there was a character being selected on the old selected tile
            if (_oldTile && _oldTile.m_holding && _oldTile.m_holding.tag == "Player" && _oldTile.m_holding != m_gamMan.m_currCharScript.gameObject)
                _oldTile.m_holding.GetComponent<CharacterScript>().DeselectCharacter();
            // If new selected tile has a player
            if (m_holding && m_holding.tag == "Player" && _oldTile != this)
            {
                CharacterScript charScript = m_holding.GetComponent<CharacterScript>();
                charScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].gameObject.SetActive(true);
                charScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor = Color.magenta;
            }
        }
        // If the selected tile is the same as the old selected tile
        else if (this == _oldTile && m_holding != m_gamMan.m_currCharScript.gameObject)
        {
            // If there was a character being selected on the old selected tile
            if (this && this.m_holding && this.m_holding.tag == "Player")
                m_boardScript.m_selected.m_holding.GetComponent<CharacterScript>().DeselectCharacter();
            m_boardScript.m_selected = null;
            return;
        }

        if (m_holding && m_holding.tag == "Player" && !m_boardScript.m_isForcedMove && !m_boardScript.m_moveLocked)
        {
            GameObject.Find("BoardCam/Main Camera").GetComponent<BoardCamScript>().m_target = m_holding;

            // If we select the current character while owning them, start movement selection
            if (m_holding == m_gamMan.m_currCharScript.gameObject && m_gamMan.m_hasActed[(int)GameManagerScript.trn.MOV] == false)
                if (m_cD.CheckIfMine(m_holding))
                {
                    Button movB = m_panMan.GetPanel("HUD Panel LEFT").transform.Find("Move Pass Panel/Move").GetComponent<Button>();
                    movB.GetComponent<ButtonScript>().Select();
                }
        }
    }

    public void OnHover()
    {
        if (!m_boardScript)
            return;

        CharacterScript currChar = m_gamMan.m_currCharScript;

        if (m_boardScript.m_oldTile)
            UnHover();

        Renderer tarRend = gameObject.GetComponent<Renderer>();

        // TARGET RED TILE

        // For special case, multi targeting attacks
        if (currChar.m_currAction)
            if (tarRend.material.color == TileLinkScript.c_attack || tarRend.material.color == TileLinkScript.c_action)
                if (currChar.m_currAction.UniqueActionProperties(ActionScript.uniAct.NON_RAD) >= 0 ||
                    currChar.m_currAction.m_radius > 0 ||
                    m_gamMan.m_currCharScript.m_tempStats[(int)CharacterScript.sts.RAD] > 0 &&
                    currChar.m_currAction.UniqueActionProperties(ActionScript.uniAct.RAD_NOT_MODDABLE) != 1)
                {
                    TileLinkScript.HandleRadius(m_boardScript, gameObject);
                    return;
                }

        tarRend.material.color = new Color(tarRend.material.color.r, tarRend.material.color.g, tarRend.material.color.b, tarRend.material.color.a + 0.5f);

        m_boardScript.m_oldTile = this;
    }

    public void UnHover()
    {
        TileScript oldTile = m_boardScript.m_oldTile;

        Renderer oTR = oldTile.GetComponent<Renderer>();
        if (oTR.material.color == TileLinkScript.c_radius)
            TileLinkScript.ClearRadius(oldTile);
        else if (oTR.material.color.a != 0)
            oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);

        // Hacky fix for when you spawn colored m_tiles for range and your cursor starts on one of the m_tiles. If it has no alpha and isn't white
        if (oTR.material.color.a == 0f && oTR.material.color.r + oTR.material.color.g + oTR.material.color.b != 3)
            oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a + 0.5f);
    }

    public void ClearTile()
    {
        Renderer tRend = GetComponent<Renderer>();

        if (m_oldColor == Color.black)
            tRend.material.color = new Color(1, 1, 1, 0f);
        else
        {
            tRend.material.color = m_oldColor;
            m_oldColor = Color.black;
        }
    }
}