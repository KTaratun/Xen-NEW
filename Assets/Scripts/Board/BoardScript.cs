using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour {

    // Tiles
    public int m_height; // Height of the board
    public int m_width; // Width of the board
    public TileScript[] m_tiles; // All m_tiles on the board
    public TileScript m_highlightedTile; // Currently hovered over tile
    public TileScript m_selected; // Last pressed tile
    public TileScript m_oldTile; // A pointer to the previously hovered over m_tile

    // OBJs
    public List<CharacterScript> m_characters; // A list of all characters within the game
    public List<GameObject> m_obstacles; // The actual array of objects on the board

    // GUI
    public Button m_hoverButton; // The button which is currently hovered over
    public Button m_currButton; // The button which is currently selected

    // State info
    public int m_environmentalDensity; // This dictates how many environmental objs per tile
    public bool m_camIsFrozen; // This is used to disable camera movement input from the player
    public GameObject m_isForcedMove; // This is to keep track of whichever player is turning an action out of order
    public bool m_moveLocked;

    // References
    public PlayerScript[] m_players; // A list of all player (Teams) within the game
    protected Camera m_camera;
    public AudioSource m_audio;
    public GameObject m_grenade;
    public GameObject m_laser;
    public ObjectScript m_powerUp; // Last created powerup

    protected SlidingPanelManagerScript m_panMan;
    protected GameManagerScript m_gamMan;
    protected ActionLoaderScript m_actLoad;

    // Use this for initialization
    private void Start()
    {
        Application.targetFrameRate = 60;

        if (GameObject.Find("Scene Manager"))
        {
            m_panMan = GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>();
            m_actLoad = GameObject.Find("Scene Manager").GetComponent<ActionLoaderScript>();
            m_gamMan = GameObject.Find("Scene Manager").GetComponent<GameManagerScript>();
        }

        m_camera = GameObject.Find("BoardCam/Main Camera").GetComponent<Camera>();


        m_grenade = Instantiate(Resources.Load<GameObject>("OBJs/Grenade"));
        m_laser = Instantiate(Resources.Load<GameObject>("OBJs/Laser"));

        m_audio = gameObject.AddComponent<AudioSource>();

        m_camIsFrozen = false;
        m_isForcedMove = null;

        GameObject.Find("Scene Manager").GetComponent<SlidingPanelManagerScript>().MenuPanelInit("Board Canvas", gameObject);

        GameObject.Find("Network").GetComponent<CustomDirect>().m_board = this;

        m_players = GameObject.Find("Board/Players").GetComponentsInChildren<PlayerScript>();

        InitBoardTiles();
        AssignNeighbors();
    }

    public void InitBoardTiles()
    {
        Renderer r = GetComponent<Renderer>();
        // If the user didn't assign a m_tile size in the editor, auto adjust the m_tiles to fit the board
        if (m_height == 0 && m_width == 0)
        {
            m_width = (int)(r.bounds.size.x / Resources.Load<GameObject>("OBJs/Tile").GetComponent<Renderer>().bounds.size.x);
            m_height = (int)(r.bounds.size.z / Resources.Load<GameObject>("OBJs/Tile").GetComponent<Renderer>().bounds.size.z);
        }

        m_tiles = new TileScript[m_width * m_height];


        for (int z = 0; z < m_height; z++)
        {
            for (int x = 0; x < m_width; x++)
            {
                GameObject newTile = Instantiate(Resources.Load<GameObject>("OBJs/Tile"));
                newTile.transform.parent = transform.Find("Tiles");

                Renderer ntR = newTile.GetComponent<Renderer>();
                ntR.material.color = new Color(1, 1, 1, 0f);

                newTile.transform.SetPositionAndRotation(new Vector3(x * ntR.bounds.size.x + (r.bounds.min.x + ntR.bounds.size.x / 2), transform.position.y + 0.05f, z * ntR.bounds.size.z + (r.bounds.min.z + ntR.bounds.size.z / 2)), new Quaternion());

                TileScript tScript = newTile.GetComponent<TileScript>();
                tScript.m_x = x;
                tScript.m_z = z;
                tScript.m_id = x + z * m_width;
                m_tiles[x + z * m_width] = tScript;
            }
        }
    }

    public void AssignNeighbors()
    {
        for (int z = 0; z < m_height; z++)
        {
            for (int x = 0; x < m_width; x++)
            {
                TileScript tScript = m_tiles[x + z * m_width];

                tScript.m_neighbors = new TileScript[4];

                if (x != 0)
                    tScript.m_neighbors[(int)TileLinkScript.nbors.LEFT] = m_tiles[(x + z * m_width) - 1];
                if (x < m_width - 1)
                    tScript.m_neighbors[(int)TileLinkScript.nbors.RIGHT] = m_tiles[(x + z * m_width) + 1];
                if (z != 0)
                    tScript.m_neighbors[(int)TileLinkScript.nbors.BOTTOM] = m_tiles[(x + z * m_width) - m_width];
                if (z < m_height - 1)
                    tScript.m_neighbors[(int)TileLinkScript.nbors.TOP] = m_tiles[(x + z * m_width) + m_width];
            }
        }
    }

    public void RandomEnvironmentInit()
    {
        if(m_environmentalDensity <= 2)
            m_environmentalDensity = 10;

        int numOfObstacles = (m_height * m_width) / m_environmentalDensity;

        while (numOfObstacles > 0)
        {

            GameObject newOBJ = SpawnEnvironmentOBJ("", -1, -1, -1);

            if (newOBJ.GetComponent<ObjectScript>().m_width == 0)
                numOfObstacles--;
            else
                numOfObstacles -= newOBJ.GetComponent<ObjectScript>().m_width;
        }
    }

    public GameObject SpawnEnvironmentOBJ(string objName, int _x, int _z, int _rot)
    {
        GameObject newOBJ = null;

        if (objName == "")
        {
            GameObject[] environmentalOBJs = Resources.LoadAll<GameObject>("OBJs/Environmental");
            int randInd = Random.Range(0, environmentalOBJs.Length);
            newOBJ = Instantiate(environmentalOBJs[randInd]);

            string[] stringSeparators = new string[] { "(Clone)" };
            string[] s = newOBJ.name.Split(stringSeparators, System.StringSplitOptions.None);
            objName = s[0];
        }
        else
            newOBJ = Instantiate(Resources.Load<GameObject>("OBJs/Environmental/" + objName));

        ObjectScript objScript = newOBJ.GetComponent<ObjectScript>();
        newOBJ.transform.parent = transform.Find("Obstacles");
        objScript.m_name = objName;

        if (_rot == -1)
            objScript.RandomRotation();
        else
            objScript.SetRotation((TileLinkScript.nbors)_rot);

        if (_x == -1)
            newOBJ.GetComponent<ObjectScript>().PlaceRandomly(this);
        else
            objScript.PlaceOBJ(this, _x, _z);

        m_obstacles.Add(newOBJ);

        return newOBJ;
    }

    public void TeamInit()
    {
        for (int i = 0; i < 4; i++)
        {
            PlayerScript playScript = m_players[i];
            playScript.m_characters = new List<CharacterScript>();
            playScript.m_bScript = this;
            playScript.m_energy = new int[4];

            for (int j = 0; j < 4; j++)
            {
                string key = i.ToString() + ',' + j.ToString();
                string name = PlayerPrefs.GetString(key + ",name");
                if (name.Length > 0)
                {
                    // Set up character
                    GameObject[] characterTypes = Resources.LoadAll<GameObject>("OBJs/Characters");
                    GameObject newChar = Instantiate(characterTypes[int.Parse(PlayerPrefs.GetString(key + ",gender"))]);
                    CharacterScript cScript = newChar.GetComponent<CharacterScript>();
                    newChar.name = name;
                    
                    cScript = PlayerPrefScript.LoadChar(key, cScript);
                    
                    m_actLoad.AddActions(cScript);
                    cScript.CharInit();
                    
                    // Link to player
                    cScript.m_player = playScript;
                    playScript.name = "Team " + (i + 1).ToString();

                    playScript.m_characters.Add(cScript);

                    //newChar.GetComponent<ObjectScript>().PlaceRandomly(this);
                    if (i == 0)
                        newChar.GetComponent<ObjectScript>().PlaceWithinRange(this, 0, m_width - 1, 0, m_height / 3);
                    else
                        newChar.GetComponent<ObjectScript>().PlaceWithinRange(this, 0, m_width - 1, m_height / 3, m_height - 1);

                    m_characters.Add(cScript);
                }
            }
        }
    }

    public CharacterScript SpawnCharacter(string _name, int _playerInd, Color _teamColor, int _x, int _z, int _rot, string _color, string[] _actions, int _hp)
    {
        // Set up character
        GameObject newChar = Instantiate(Resources.Load<GameObject>("OBJs/Characters/Robo Scout Character"));
        CharacterScript cScript = newChar.GetComponent<CharacterScript>();
        newChar.name = _name;

        cScript.m_name = newChar.name;
        cScript.m_color = _color;
        cScript.m_actNames = _actions;
        cScript.m_isAlive = true;
        cScript.m_effects = new bool[(int)StatusScript.effects.TOT];

        cScript.InitializeStats();
        cScript.m_totalHealth = _hp;

        m_actLoad.AddActions(cScript);

        cScript.m_teamColor = _teamColor;
        cScript.m_player = m_players[_playerInd];

        cScript.CharInit();

        // Link to player
        cScript.m_player.m_characters.Add(cScript);

        cScript.PlaceOBJ(this, _x, _z);

        if (_rot == -1)
            cScript.RandomRotation();
        else
            cScript.SetRotation((TileLinkScript.nbors)_rot);

        return cScript;
    }


    // Update is called once per frame
    protected void Update()
    {
        if (!m_gamMan.m_battle)
            return;

        Hover();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (m_gamMan.m_battle)
            return;

        else if (collision.collider.tag == "Player")
        {
            JoinBattle(collision.gameObject.GetComponent<CharacterScript>());

            //if (collision.collider.gameObject == m_field.m_mainChar.gameObject)
            //    BattleOverview();
        }
    }

    public void OnRightClick()
    {
        m_selected = null;
        if (m_currButton && !m_camIsFrozen)
        {
            m_currButton.GetComponent<Image>().color = m_currButton.GetComponent<ButtonScript>().m_oldColor;
            if (m_currButton.GetComponent<SlidingPanelScript>())
                m_currButton.GetComponent<SlidingPanelScript>().m_inView = false;
            m_currButton = null;
            TileScript selectedTileScript = m_gamMan.m_currCharScript.m_tile.GetComponent<TileScript>();
            if (selectedTileScript.m_radius.Count > 0)
                TileLinkScript.ClearRadius(selectedTileScript);

            m_panMan.GetPanel("ActionViewer Panel").ClosePanel();
        }

        // If right click while the confirmation panel is up, do nothing but close that specific panel
        if (m_panMan.m_confirmPanel.m_inView)
        {
            m_panMan.RemoveFromHistory("");
            return;
        }

        TileScript currTScript = m_gamMan.m_currCharScript.m_tile.GetComponent<TileScript>();

        if (currTScript.m_radius.Count == 0 || m_isForcedMove)
            return;

        SlidingPanelScript actPreviewScript = m_panMan.GetPanel("ActionPreview");
        actPreviewScript.ClosePanel();

        if (m_highlightedTile)
            TileLinkScript.ClearRadius(m_highlightedTile);

        TileLinkScript.ClearRadius(currTScript);
    }

    // Hover
    public void Hover()
    {
        // Don't hover under these conditions
        if (m_camIsFrozen && !m_currButton || !m_gamMan.m_currCharScript || m_gamMan.m_currCharScript &&
                m_gamMan.m_currCharScript.m_targets.Count > 0 ||
                m_panMan.CheckIfPanelOpen() ||
                m_hoverButton)
        {
            if (m_oldTile && !m_selected && !m_hoverButton)
            {
                Renderer oTR = m_oldTile.GetComponent<Renderer>();
                if (oTR.material.color.a != 0)
                    oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);

                m_oldTile = null;
            }
            return;
        }

        Ray ray;
        RaycastHit hit;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out hit))
        {
            if (m_highlightedTile)
            {
                Renderer oTR = m_highlightedTile.GetComponent<Renderer>();
                if (oTR.material.color.a != 0)
                    oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);
                if (m_highlightedTile.m_targetRadius.Count > 0)
                    TileLinkScript.ClearRadius(m_highlightedTile);

                // If we were highlighting and not selecting the tile, deselect
                if (m_highlightedTile.m_holding && m_highlightedTile.m_holding.tag == "Player" && m_highlightedTile.m_holding != m_gamMan.m_currCharScript.gameObject)
                    if (!m_selected || m_selected.m_holding && m_selected.m_holding.tag != "Player" ||
                        m_selected.m_holding && m_selected.m_holding == m_gamMan.m_currCharScript.gameObject)// m_selected.m_holding != m_highlightedTile.m_holding)
                        m_highlightedTile.m_holding.GetComponent<CharacterScript>().DeselectCharacter();

                m_highlightedTile = null;
            }

            if (!m_panMan.GetPanel("StatusViewer Panel").m_cScript && m_panMan.GetPanel("StatusViewer Panel").transform.Find("Status Image").GetComponent<SlidingPanelScript>().m_inView)
                m_panMan.GetPanel("StatusViewer Panel").GetComponent<SlidingPanelScript>().ClosePanel();

            return;
        }

        TileScript tScript = null;
        CharacterScript charScript = null;

        // Check to see if you hit a tile or object, then grab the one you didn't hit
        if (hit.collider.gameObject.tag == "Tile")
        {
            tScript = hit.collider.gameObject.GetComponent<TileScript>();
            if (tScript.m_holding && tScript.m_holding.tag == "Player")
                charScript = tScript.m_holding.GetComponent<CharacterScript>();
        }
        else if (hit.collider.GetComponent<ObjectScript>())
        {
            tScript = hit.collider.GetComponent<ObjectScript>().m_tile;
            if (hit.collider.gameObject.tag == "Player")
                charScript = hit.collider.gameObject.GetComponent<CharacterScript>();
        }

        // If you hit a character/character tile, show their info
        if (charScript)
            charScript.HighlightCharacter();

        if (tScript)
            tScript.OnHover();

        // If you hit a character/character tile and while you are aiming with an attack, show the new health after a hit
        if (charScript && tScript.gameObject.GetComponent<Renderer>().material.color == new Color(1, 0, 0, 1) ||
            charScript && tScript.gameObject.GetComponent<Renderer>().material.color == TileLinkScript.c_radius)
        {
            m_panMan.GetPanel("ActionViewer Panel").transform.Find("ActionView Slide/DamagePreview").GetComponent<DamagePreviewPanelScript>().m_cScript = charScript;
            m_panMan.GetPanel("ActionViewer Panel").transform.Find("ActionView Slide/DamagePreview").GetComponent<DamagePreviewPanelScript>().PopulatePanel();
        }
        
        if (m_highlightedTile && tScript != m_highlightedTile)
        {
            if (m_highlightedTile.m_holding && m_highlightedTile.m_holding.tag == "PowerUp" && 
                m_panMan.GetPanel("StatusViewer Panel").GetComponent<SlidingPanelScript>().m_inView)
                m_panMan.GetPanel("StatusViewer Panel").GetComponent<SlidingPanelScript>().ClosePanel();

            if (m_highlightedTile.m_holding && m_highlightedTile.m_holding.tag == "Player" && m_highlightedTile.m_holding != m_gamMan.m_currCharScript.gameObject) 
                if (!m_selected || !m_selected.m_holding || m_selected.m_holding && m_selected.m_holding.tag != "Player" ||
                    m_selected.m_holding && m_selected.m_holding == m_gamMan.m_currCharScript.gameObject)
                {
                    m_highlightedTile.m_holding.GetComponent<CharacterScript>().DeselectCharacter();
                    m_highlightedTile = null;
                }
        }

        if (tScript)
            m_highlightedTile = tScript;

        if (tScript && tScript.m_holding && tScript.m_holding.tag == "PowerUp")
            m_panMan.GetPanel("StatusViewer Panel").GetComponent<StatusViewerPanel>().PopulatePanel();
    }

    // Utilities
    public ObjectScript SpawnItem(int _ind, int _x, int _z)
    {
        GameObject pUP = Instantiate(Resources.Load<GameObject>("OBJs/PowerUp"));
        PowerupScript pScript = pUP.GetComponent<PowerupScript>();
        pScript.Init(_ind);

        if (_x == -1 || _z == -1)
            pScript.PlaceRandomly(this);
        else
            pScript.PlaceOBJ(this, _x, _z);

        m_camIsFrozen = true;
        m_camera.GetComponent<BoardCamScript>().m_target = pUP;

        return pScript;
    }

    public void JoinBattle(CharacterScript _char)
    {
        TileScript closest = null;
        for (int i = 0; i < m_tiles.Length; i++)
        {
            if (!closest || Vector3.Distance(m_tiles[i].gameObject.transform.position, _char.transform.position) <
                Vector3.Distance(closest.transform.position, _char.transform.position))
                closest = m_tiles[i];
        }

        _char.m_tile = closest;
        _char.m_boardScript = this;
        m_characters.Add(_char);
    }
}