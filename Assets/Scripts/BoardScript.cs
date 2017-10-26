using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BoardScript : MonoBehaviour {

    public Camera m_camera; // Why it do that squiggle?
    public int m_roundCount;
    public int m_environmentalDensity; // This dictates how many environmental objs per tile
    public GameObject[] m_environmentOBJs;
    public List<GameObject> m_obstacles; // The actual array of objects on the board
    public TileScript[] m_tiles; // All m_tiles on the board
    public GameObject m_tile; // A reference to the m_tile prefab
    public GameObject[] m_character; // A reference to the character prefab
    public int m_width; // Width of the board
    public int m_height; // Height of the board
    public TileScript m_selected; // Last pressed tile
    public TileScript m_highlightedTile; // Currently hovered over tile
    public TileScript m_oldTile; // A pointer to the previously hovered over m_tile
    public TileScript m_currTile; // The m_tile of the player who's turn is currently up
    public CharacterScript m_currCharScript; // A pointer to the character that's turn is currently up
    public List<GameObject> m_characters; // A list of all characters within the game
    public GameObject[] m_players;
    public List<GameObject> m_currRound; // A list of the players who have taken a turn in the current round
    public GameObject m_isForcedMove; // This is to keep track of whichever player is turning an action out of order
    public int m_livingPlayersInRound; // This is mainly for the turn panels. This tells them how far to move when I player finishes their turn.
    public bool m_camIsFrozen;
    public bool m_newTurn;
    public AudioSource m_audio;
    public Button m_hoverButton;
    public Button m_currButton;
    public GameObject m_powerup;
    public float m_actionEndTimer;

    // Use this for initialization
    void Start()
    {
        m_actionEndTimer = 0;
        m_audio = gameObject.AddComponent<AudioSource>();

        m_newTurn = false;
        PanelScript.MenuPanelInit("Canvas");
        m_roundCount = 0;
        m_isForcedMove = null;
        m_camIsFrozen = false;
        Renderer r = GetComponent<Renderer>();

        if (m_environmentalDensity <= 2)
            m_environmentalDensity = 10;
        // If the user didn't assign a m_tile size in the editor, auto adjust the m_tiles to fit the board
        if (m_height == 0 && m_width == 0)
        {
            m_width = (int)(r.bounds.size.x / m_tile.GetComponent<Renderer>().bounds.size.x);
            m_height = (int)(r.bounds.size.z / m_tile.GetComponent<Renderer>().bounds.size.z);
        }

        InitBoardTiles();
        AssignNeighbors();
        EnvironmentInit();
        CharacterInit();
        NewTurn();
    }

    public void InitBoardTiles()
    {
        Renderer r = GetComponent<Renderer>();
        m_tiles = new TileScript[m_width * m_height];

        for (int z = 0; z < m_height; z++)
        {
            for (int x = 0; x < m_width; x++)
            {
                GameObject newtile = Instantiate(m_tile);

                Renderer ntR = newtile.GetComponent<Renderer>();
                ntR.material.color = new Color(1, 1, 1, 0f);

                newtile.transform.SetPositionAndRotation(new Vector3(x * ntR.bounds.size.x + (r.bounds.min.x + ntR.bounds.size.x / 2), transform.position.y + 0.05f, z * ntR.bounds.size.z + (r.bounds.min.z + ntR.bounds.size.z / 2)), new Quaternion());

                TileScript tScript = newtile.GetComponent<TileScript>();
                tScript.m_x = x;
                tScript.m_z = z;
                tScript.m_boardScript = GetComponent<BoardScript>();

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
                    tScript.m_neighbors[(int)TileScript.nbors.left] = m_tiles[(x + z * m_width) - 1];
                if (x < m_width - 1)
                    tScript.m_neighbors[(int)TileScript.nbors.right] = m_tiles[(x + z * m_width) + 1];
                if (z != 0)
                    tScript.m_neighbors[(int)TileScript.nbors.bottom] = m_tiles[(x + z * m_width) - m_width];
                if (z < m_height - 1)
                    tScript.m_neighbors[(int)TileScript.nbors.top] = m_tiles[(x + z * m_width) + m_width];
            }
        }
    }

    private void EnvironmentInit()
    {
        int numOfObstacles = (m_height * m_width) / m_environmentalDensity;

        while (numOfObstacles > 0)
        {
            int randomOBJ = Random.Range(0, m_environmentOBJs.Length);

            GameObject newOBJ = Instantiate(m_environmentOBJs[randomOBJ]);
            newOBJ.GetComponent<ObjectScript>().RandomRotation();
            newOBJ.GetComponent<ObjectScript>().PlaceRandomly(this);
            m_obstacles.Add(newOBJ);
            if (newOBJ.GetComponent<ObjectScript>().m_width == 0)
                numOfObstacles--;
            else
                numOfObstacles -= newOBJ.GetComponent<ObjectScript>().m_width;
        }
    }

    public void CharacterInit()
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 6; j++)
            {
                string key = i.ToString() + ',' + j.ToString();
                string name = PlayerPrefs.GetString(key + ",name");
                if (name.Length > 0)
                {
                    // Set up character
                    GameObject newChar = Instantiate(m_character[int.Parse(PlayerPrefs.GetString(key + ",gender"))]);
                    newChar.name = name;
                    CharacterScript cScript = newChar.GetComponent<CharacterScript>();

                    cScript = PlayerPrefScript.LoadChar(key, cScript);

                    if (i == 0)
                        cScript.m_teamColor = new Color(1, .5f, 0, 1);
                    else if (i == 1)
                        cScript.m_teamColor = new Color(.5f, 0, 1, 1);
                    else if (i == 2)
                        cScript.m_teamColor = new Color(.8f, 1, 0, 1);
                    else if (i == 3)
                        cScript.m_teamColor = Color.magenta;

                    newChar.GetComponentInChildren<Renderer>().materials[0].color = cScript.m_teamColor;
                    newChar.GetComponentInChildren<Renderer>().materials[1].color = cScript.m_teamColor;

                    cScript.SetPopupSpheres("");
                    m_characters.Add(newChar);

                    if (cScript.m_hasActed.Length == 0)
                        cScript.m_hasActed = new int[2];

                    cScript.m_hasActed[0] = 0;
                    cScript.m_hasActed[1] = 0;

                    // Link to player
                    PlayerScript playScript = m_players[i].GetComponent<PlayerScript>();
                    cScript.m_player = playScript;
                    playScript.name = "Team " + (i + 1).ToString();
                    playScript.m_characters.Add(cScript);
                    newChar.GetComponent<ObjectScript>().PlaceRandomly(this);
                }
            }
    }
    
    // Update is called once per frame
    void Update()
    {
        Inputs();
        Hover();
        EndOfActionTimer();
        EndTurn();
    }

    void EndOfActionTimer()
    {
        if (m_actionEndTimer > 0)
        {
            m_actionEndTimer += Time.deltaTime;
            if (m_actionEndTimer > 2)
            {
                m_actionEndTimer = 0;
                m_camIsFrozen = false;

                if (!m_selected)
                    PanelScript.GetPanel("HUD Panel RIGHT").m_inView = false;
            }
        }
    }

    void Inputs()
    {
        if (m_selected && m_selected.m_holding && m_selected.m_holding.tag == "Player")
        {
            Renderer oldRend = m_selected.m_holding.transform.GetComponentInChildren<Renderer>();
            if (oldRend.materials[2].shader != oldRend.materials[0].shader)
                return;
        }

        float w = Input.GetAxis("Mouse ScrollWheel");
        PanelScript actPan = PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)CharacterScript.HUDPan.ACT_PAN].GetComponent<PanelScript>();

        if (Input.GetKeyDown(KeyCode.Q) && !PanelScript.GetPanel("Choose Panel").m_inView) //Formally getmousebuttondown(1);
            OnRightClick();
        else if (Input.GetMouseButtonDown(0) && m_camera.GetComponent<CameraScript>().m_rotate)
            SceneManager.LoadScene("Menu");
        else if (Input.GetKeyDown(KeyCode.Escape))
            PanelScript.GetPanel("Options Panel").PopulatePanel();
        else if (Input.GetKeyDown(KeyCode.LeftShift) && m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] == 0)
            PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[0].GetComponent<ButtonScript>().Select();
        else if (Input.GetKeyDown(KeyCode.Space) && !PanelScript.GetPanel("Choose Panel").m_inView)
        {
            if (m_currButton)
            {
                m_currButton.GetComponent<Image>().color = Color.white;
                m_currButton.GetComponent<ButtonScript>().m_main.m_inView = false;
                m_currButton = null;
                TileScript selectedTileScript = m_currCharScript.m_tile.GetComponent<TileScript>();
                if (selectedTileScript.m_radius.Count > 0)
                    selectedTileScript.ClearRadius();
            }
            PanelScript.GetPanel("Confirmation Panel").m_buttons[1].GetComponent<ButtonScript>().ConfirmationButton("Pass");
        }
        else if (Input.GetKey(KeyCode.E) && w != 0)
        {
            if (!m_currButton)
            {
                actPan.m_buttons[0].GetComponent<ButtonScript>().Select();
                return;
            }

            int i = int.Parse(m_currButton.name);
            if (w < 0)
            {
                while (i + 1 < 8)
                {
                    if (actPan.m_buttons[i + 1].GetComponentInChildren<Text>().text != "EMPTY" &&
                        m_currCharScript.m_player.CheckEnergy(DatabaseScript.GetActionData(actPan.m_buttons[i + 1].GetComponent<ButtonScript>().m_action, DatabaseScript.actions.ENERGY)))
                    {
                        actPan.m_buttons[i + 1].GetComponent<ButtonScript>().Select();
                        break;
                    }
                    i++;
                }
            }
            else if (w > 0)
            {
                while (i - 1 >= 0)
                {
                    if (i - 1 >= 0 && actPan.m_buttons[i - 1].GetComponentInChildren<Text>().text != "EMPTY" &&
                         m_currCharScript.m_player.CheckEnergy(DatabaseScript.GetActionData(actPan.m_buttons[i - 1].GetComponent<ButtonScript>().m_action, DatabaseScript.actions.ENERGY)))
                    {
                        actPan.m_buttons[i - 1].GetComponent<ButtonScript>().Select();
                        break;
                    }
                    i--;
                }
            }
        }

        int num = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1))
            num = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            num = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            num = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            num = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            num = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            num = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            num = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            num = 7;

        Text t = null;
        Button butt = null;
        if (num > -1)
        {
            butt = actPan.GetComponent<PanelScript>().m_buttons[num];
            t = butt.GetComponentInChildren<Text>();
        }

        if (num >= 0 && t.text != "EMPTY" && butt.interactable == true)
        {
            butt.GetComponent<ButtonScript>().Select();
        }
    }

    public void OnRightClick()
    {
        m_selected = null;
        if (m_currButton)
        {
            m_currButton.GetComponent<Image>().color = m_currButton.GetComponent<ButtonScript>().m_oldColor;
            m_currButton = null;
            TileScript selectedTileScript = m_currCharScript.m_tile.GetComponent<TileScript>();
            if (selectedTileScript.m_radius.Count > 0)
                selectedTileScript.ClearRadius();

            PanelScript.GetPanel("ActionViewer Panel").m_inView = false;
        }

        // If right click while the confirmation panel is up, do nothing but close that specific panel
        if (PanelScript.m_confirmPanel.m_inView)
        {
            PanelScript.RemoveFromHistory("");
            return;
        }

        TileScript currTScript = m_currCharScript.m_tile.GetComponent<TileScript>();

        if (currTScript.m_radius.Count == 0 || m_isForcedMove)
            return;

        PanelScript actPreviewScript = PanelScript.GetPanel("ActionPreview");
        actPreviewScript.m_inView = false;

        if (m_highlightedTile)
            m_highlightedTile.ClearRadius();

        currTScript.ClearRadius();
    }

    public void Hover()
    {
        // Don't hover under these conditions
        if (m_camIsFrozen && !m_currButton || !m_currCharScript || m_currCharScript && 
                m_currCharScript.m_targets.Count > 0 || 
                PanelScript.CheckIfPanelOpen())
        {
            if (m_oldTile && !m_selected)
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
                    m_highlightedTile.ClearRadius();
                if (m_highlightedTile.m_holding)
                    DeselectHighlightedTile();

                m_highlightedTile = null;
            }

            if (!PanelScript.GetPanel("StatusViewer Panel").m_cScript)
                PanelScript.GetPanel("StatusViewer Panel").m_inView = false;
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
            HandleTile(tScript);

        // If you hit a character/character tile and while you are aiming with an attack, show the new health after a hit
        if (charScript && tScript.gameObject.GetComponent<Renderer>().material.color == new Color(1, 0, 0, 1) ||
            charScript && tScript.gameObject.GetComponent<Renderer>().material.color == Color.yellow)
        {
            PanelScript.GetPanel("ActionViewer Panel").m_panels[1].m_cScript = charScript;
            PanelScript.GetPanel("ActionViewer Panel").m_panels[1].PopulatePanel();
        }

        if (m_highlightedTile && tScript != m_highlightedTile)
        {
            if (!PanelScript.GetPanel("StatusViewer Panel").m_cScript)
                PanelScript.GetPanel("StatusViewer Panel").m_inView = false;
            if (m_highlightedTile.m_holding && m_highlightedTile.m_holding.tag == "Player")
                DeselectHighlightedTile();
        }

        if (tScript)
            m_highlightedTile = tScript;

        if (tScript && tScript.m_holding && tScript.m_holding.tag == "PowerUp")
        {
            PanelScript.GetPanel("StatusViewer Panel").m_cScript = null;
            PanelScript.GetPanel("StatusViewer Panel").PopulatePanel();
        }
    }
    
    private void HandleTile(TileScript _target)
    {
        if (m_oldTile)
        {
            Renderer oTR = m_oldTile.GetComponent<Renderer>();
            if (oTR.material.color == Color.yellow)
                m_oldTile.ClearRadius();
            else if (oTR.material.color.a != 0)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);

            // Hacky fix for when you spawn colored m_tiles for range and your cursor starts on one of the m_tiles. If it has no alpha and isn't white
            if (oTR.material.color.a == 0f && oTR.material.color.r + oTR.material.color.g + oTR.material.color.b != 3)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a + 0.5f);
        }

        Renderer tarRend = _target.gameObject.GetComponent<Renderer>();

        // TARGET RED TILE

        // For special case, multi targeting attacks
        if (m_currCharScript.m_currAction.Length > 0 && tarRend.material.color == TileScript.c_attack &&
            CharacterScript.UniqueActionProperties(m_currCharScript.m_currAction, CharacterScript.uniAct.NON_RAD) >= 0)
        {
            string actRng = DatabaseScript.GetActionData(m_currCharScript.m_currAction, DatabaseScript.actions.RNG);

            bool targetSelf = true;
            if (CharacterScript.UniqueActionProperties(m_currCharScript.m_currAction, CharacterScript.uniAct.TAR_SELF) >= 0)
                targetSelf = false;

            TileScript.targetRestriction tR = TileScript.targetRestriction.NONE;
            if (CharacterScript.UniqueActionProperties(m_currCharScript.m_currAction, CharacterScript.uniAct.TAR_RES) >= 0)
                tR = (TileScript.targetRestriction)CharacterScript.UniqueActionProperties(m_currCharScript.m_currAction, CharacterScript.uniAct.TAR_RES);

            bool isBlockable = true;
            if (CharacterScript.UniqueActionProperties(m_currCharScript.m_currAction, CharacterScript.uniAct.IS_NOT_BLOCK) >= 0)
                isBlockable = false;

            _target.FetchTilesWithinRange(int.Parse(actRng) + m_currCharScript.m_tempStats[(int)CharacterScript.sts.RNG], Color.yellow, targetSelf, tR, isBlockable);
            m_oldTile = _target;
            return;
        }

        // For standard radius attacks
        int rad = m_currCharScript.m_currRadius + m_currCharScript.m_tempStats[(int)CharacterScript.sts.RAD];
        bool tarSelf = true;

        if (m_currCharScript.m_currAction.Length > 0 && CharacterScript.UniqueActionProperties(m_currCharScript.m_currAction, CharacterScript.uniAct.RAD_MOD) >= 0)
            rad += m_currCharScript.m_tempStats[(int)CharacterScript.sts.TEC];

        if (m_currCharScript.m_currAction.Length > 0 && CharacterScript.UniqueActionProperties(m_currCharScript.m_currAction, CharacterScript.uniAct.TAR_SELF) >= 0)
            tarSelf = false;

        if (rad > 0 && tarRend.material.color == TileScript.c_attack)
            _target.FetchTilesWithinRange(rad, Color.yellow, tarSelf, TileScript.targetRestriction.NONE, true);
        else
            tarRend.material.color = new Color(tarRend.material.color.r, tarRend.material.color.g, tarRend.material.color.b, tarRend.material.color.a + 0.5f);

        m_oldTile = _target;
    }

    private void DeselectHighlightedTile()
    {
        PanelScript.GetPanel("ActionViewer Panel").m_panels[1].m_inView = false;

        if (m_selected && m_selected.m_holding && m_selected.m_holding.tag == "Player" && 
            m_selected.m_holding.transform.GetComponentInChildren<Renderer>().materials[2].shader != m_selected.m_holding.transform.GetComponentInChildren<Renderer>().materials[0].shader || 
            PanelScript.GetPanel("Choose Panel").m_inView)
            return;

        PanelScript hudPanScript = PanelScript.GetPanel("HUD Panel RIGHT");

        if (m_highlightedTile.m_holding.GetComponent<CharacterScript>())
        {
            CharacterScript colScript = m_highlightedTile.m_holding.GetComponent<CharacterScript>();

            for (int i = 0; i < colScript.m_turnPanels.Count; i++)
            {
                Image turnPanImage = colScript.m_turnPanels[i].GetComponent<Image>();
                if (colScript.m_effects[(int)StatusScript.effects.STUN] || !colScript.m_isAlive)
                    turnPanImage.color = new Color(1, .5f, .5f, 1);
                else
                    turnPanImage.color = new Color(colScript.m_teamColor.r + 0.3f, colScript.m_teamColor.g + 0.3f, colScript.m_teamColor.b + 0.3f, 1);
            }
        }
        hudPanScript.m_inView = false;
        m_highlightedTile = null;
    }

    public void NewTurn()
    {
        PlayerScript winningTeam = null;
        int numTeamsActive = 0;
        for (int i = 0; i < m_players.Length; i++)
        {
            if (!m_players[i])
                continue;

            for (int j = 0; j < m_players[i].GetComponent<PlayerScript>().m_characters.Count; j++)
            {
                if (m_players[i].GetComponent<PlayerScript>().m_characters[j].m_isAlive)
                {
                    numTeamsActive++;
                    winningTeam = m_players[i].GetComponent<PlayerScript>();
                    break;
                }
            }
        }

        if (numTeamsActive < 2)
        {
            GameOver(winningTeam);
            return;
        }

        // Turn previous character back to original color
        if (m_currCharScript)
        {
            Renderer oldRend = m_currCharScript.gameObject.transform.GetComponentInChildren<Renderer>();
            oldRend.materials[1].shader = oldRend.materials[0].shader;
            oldRend.materials[2].shader = oldRend.materials[0].shader;
            //Renderer oldRenderer = m_currPlayer.GetComponent<Renderer>();
            //oldRenderer.material.color = m_currPlayer.GetComponent<CharacterScript>().m_teamColor;
        }

        bool newRound = false;
        if (m_currRound.Count == 0)
        {
            NewRound();
            newRound = true;
        }

        m_currCharScript = m_currRound[0].GetComponent<CharacterScript>();
        m_currRound.RemoveAt(0);

        while (m_currCharScript.m_effects[(int)StatusScript.effects.STUN] || !m_currCharScript.m_isAlive)
        {
            StatusScript.UpdateStatus(m_currCharScript.gameObject, StatusScript.mode.TURN_END);
            m_currCharScript.m_turnPanels[0].SetActive(false);
            m_currCharScript.m_turnPanels.RemoveAt(0);

            if (m_currRound.Count == 0)
            {
                NewRound();
                newRound = true;
            }

            m_currCharScript = m_currRound[0].GetComponent<CharacterScript>();
            m_currRound.RemoveAt(0);
        }

        if (!newRound)
            m_newTurn = true;

        m_currTile = m_currCharScript.m_tile;

        if (m_currCharScript.m_turnPanels.Count > 0)
            m_currCharScript.m_turnPanels.RemoveAt(0);

        PanelScript HUDLeftScript = PanelScript.GetPanel("HUD Panel LEFT");
        HUDLeftScript.m_cScript = m_currCharScript;
        HUDLeftScript.PopulatePanel();

        CameraScript camScript = m_camera.GetComponent<CameraScript>();
        camScript.m_target = m_currCharScript.gameObject;

        Renderer rend = m_currCharScript.gameObject.transform.GetComponentInChildren<Renderer>();
        rend.materials[2].shader = rend.materials[0].shader;
        rend.materials[1].shader = Resources.Load<Shader>("Outlined-Silhouette Only (NOT MINE)");

        if (m_currCharScript.m_isAI)
            m_currCharScript.AITurn();
    }

    public void EndTurn()
    {
        if (m_currCharScript.m_anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base.Idle Melee") ||
            m_currCharScript.m_anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base.Death"))
            if (m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] + m_currCharScript.m_hasActed[(int)CharacterScript.trn.ACT] > 3 &&
                !m_isForcedMove && !m_camIsFrozen && !PanelScript.GetPanel("Choose Panel").m_inView)
            {
                print(m_currCharScript.m_name + " has ended their turn.\n");

                PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[(int)CharacterScript.trn.MOV].interactable = true;
                StatusScript.UpdateStatus(m_currCharScript.gameObject, StatusScript.mode.TURN_END);
                m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] = 0;
                m_currCharScript.m_hasActed[(int)CharacterScript.trn.ACT] = 0;
                m_currCharScript.m_currAction = "";
                NewTurn();
            }
    }

    public void NewRound()
    {
        List<GameObject> tempChars = new List<GameObject>();
        m_currCharScript = null;

        m_livingPlayersInRound = 0;
        for (int i = 0; i < m_characters.Count; i++)
            if (m_characters[i].GetComponent<CharacterScript>().m_isAlive)
            {
                tempChars.Add(m_characters[i]);
                m_livingPlayersInRound++;
            }

        int numPool;
        do
        {
            numPool = 0;
            // Gather all characters speed to pull from
            for (int i = 0; i < tempChars.Count; i++)
            {
                CharacterScript charScript = tempChars[i].GetComponent<CharacterScript>();
                if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] < 0 || charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0)
                    continue;
                else
                    numPool += charScript.m_tempStats[(int)CharacterScript.sts.SPD];
            }

            int randNum = Random.Range(0, numPool);
            int currNum = 0;

            for (int i = 0; i < tempChars.Count; i++)
            {
                CharacterScript charScript = tempChars[i].GetComponent<CharacterScript>();
                if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] <= 0 || charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0)
                    continue;

                currNum += charScript.m_tempStats[(int)CharacterScript.sts.SPD];

                if (randNum < currNum)
                {
                    m_currRound.Add(tempChars[i]);

                    if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] >= 10)
                        numPool -= 10;
                    else
                    {
                        numPool -= charScript.m_tempStats[(int)CharacterScript.sts.SPD];
                        tempChars.RemoveAt(i);
                    }

                    charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 10;
                    break;
                }
            }
        } while (m_currRound.Count < m_livingPlayersInRound && numPool > 0);

        for (int i = 0; i < m_characters.Count; i++)
        {
            CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();
            if (!charScript.m_effects[(int)StatusScript.effects.STUN] && charScript.m_isAlive)
                charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 10;

            StatusScript.UpdateStatus(m_characters[i], StatusScript.mode.ROUND_END);
        }

        m_livingPlayersInRound = m_currRound.Count;
        m_roundCount++;
        PanelScript roundPanScript = PanelScript.GetPanel("Round Panel");
        roundPanScript.PopulatePanel();

        PanelScript.GetPanel("Turn Panel").NewTurnOrder();

        SpawnItem();
    }

    public void SpawnItem()
    {
        GameObject pUP = Instantiate(m_powerup);
        pUP.GetComponent<ObjectScript>().PlaceRandomly(this);
    }

    public void GameOver(PlayerScript _winningTeam)
    {
        PanelScript roundPan = PanelScript.GetPanel("Round End Panel");
        roundPan.m_inView = true;
        roundPan.m_text[0].text = _winningTeam.name + " WINS";
        m_camera.GetComponent<CameraScript>().m_rotate = true;
    }

    public void Pass()
    {
        m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] = 2;
        m_currCharScript.m_hasActed[(int)CharacterScript.trn.ACT] = 2;
        PanelScript.CloseHistory();
        PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[1].GetComponent<Image>().color = Color.white;
        m_currButton = null;
    }
}
