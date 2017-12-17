using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BoardScript : MonoBehaviour {

    public enum prjcts { BEAM, GRENADE, LASER }
    // Tiles
    public int m_height; // Height of the board
    public int m_width; // Width of the board
    public TileScript[] m_tiles; // All m_tiles on the board
    public TileScript m_highlightedTile; // Currently hovered over tile
    public TileScript m_selected; // Last pressed tile
    public TileScript m_oldTile; // A pointer to the previously hovered over m_tile

    // OBJs
    public List<GameObject> m_characters; // A list of all characters within the game
    public List<GameObject> m_currRound; // A list of the players who have not taken a turn in the current round
    public CharacterScript m_currCharScript; // A pointer to the character that's turn is currently up
    public List<GameObject> m_obstacles; // The actual array of objects on the board
    public GameObject[] m_netOBJs; // List of all objects for the network to index through

    // GUI
    public Button m_hoverButton; // The button which is currently hovered over
    public Button m_currButton; // The button which is currently selected

    // State info
    public int m_roundCount;
    public bool m_battle; // This checks to see if a battle has started
    public bool m_newTurn; // This tells the turn panels to slide
    public bool m_camIsFrozen; // This is used to disable camera movement input from the player
    public int m_environmentalDensity; // This dictates how many environmental objs per tile
    public int m_livingPlayersInRound; // This is mainly for the turn panels. This tells them how far to move when I player finishes their turn.
    public float m_actionEndTimer; // This is used to delay camera movement when focusing on a target
    public GameObject m_isForcedMove; // This is to keep track of whichever player is turning an action out of order

    // References
    public GameObject m_players; // A list of all player (Teams) within the game
    public FieldScript m_field;
    public Camera m_camera;
    public AudioSource m_audio;
    public GameObject[] m_projectiles;



    // Use this for initialization
    void Start()
    {
        m_actionEndTimer = 0;
        m_roundCount = 0;
        m_newTurn = false;
        m_camIsFrozen = false;
        m_netOBJs = new GameObject[60];

        GameObject[] projs = Resources.LoadAll<GameObject>("OBJs/Projectiles");
        m_projectiles = new GameObject[projs.Length];

        for (int i = 0; i < projs.Length; i++)
        {
            m_projectiles[i] = Instantiate(projs[i]);
            if (m_projectiles[i].GetComponent<ProjectileScript>())
                m_projectiles[i].GetComponent<ProjectileScript>().m_boardScript = this;
            m_projectiles[i].SetActive(false);
        }

        m_projectiles[0].GetComponent<BeamScript>().m_boardScript = this;

        m_audio = gameObject.AddComponent<AudioSource>();
        m_isForcedMove = null;

        PanelScript.MenuPanelInit("Board Canvas", gameObject);

        InitBoardTiles();
        AssignNeighbors();

        if (m_field)
            m_battle = false;
        else
            BattleOverview();
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
                GameObject newtile = Instantiate(Resources.Load<GameObject>("OBJs/Tile"));

                Renderer ntR = newtile.GetComponent<Renderer>();
                ntR.material.color = new Color(1, 1, 1, 0f);

                newtile.transform.SetPositionAndRotation(new Vector3(x * ntR.bounds.size.x + (r.bounds.min.x + ntR.bounds.size.x / 2), transform.position.y + 0.05f, z * ntR.bounds.size.z + (r.bounds.min.z + ntR.bounds.size.z / 2)), new Quaternion());

                TileScript tScript = newtile.GetComponent<TileScript>();
                tScript.m_x = x;
                tScript.m_z = z;
                tScript.m_boardScript = GetComponent<BoardScript>();
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

    private void RandomEnvironmentInit()
    {
        if(m_environmentalDensity <= 2)
            m_environmentalDensity = 10;

        int numOfObstacles = (m_height * m_width) / m_environmentalDensity;

        while (numOfObstacles > 0)
        {
            GameObject[] environmentalOBJs = Resources.LoadAll<GameObject>("OBJs/Environmental");
            int randomOBJ = Random.Range(0, environmentalOBJs.Length);

            GameObject newOBJ = Instantiate(environmentalOBJs[randomOBJ]);
            newOBJ.GetComponent<ObjectScript>().m_name = randomOBJ.ToString();
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
        {
            PlayerScript playScript = m_players.AddComponent<PlayerScript>();
            playScript.m_characters = new List<CharacterScript>();
            playScript.m_bScript = this;
            playScript.m_energy = new int[4];

            for (int j = 0; j < 6; j++)
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
                    
                    if (i == 0)
                        cScript.m_teamColor = new Color(1, .5f, 0, 1);
                    else if (i == 1)
                        cScript.m_teamColor = new Color(.5f, 0, 1, 1);
                    else if (i == 2)
                        cScript.m_teamColor = new Color(.8f, 1, 0, 1);
                    else if (i == 3)
                        cScript.m_teamColor = Color.magenta;
                    
                    Renderer rend = newChar.transform.GetComponentInChildren<Renderer>();
                    rend.materials[0].color = cScript.m_teamColor;

                    cScript.SetPopupSpheres("");
                    m_characters.Add(newChar);
                    
                    if (cScript.m_hasActed.Length == 0)
                        cScript.m_hasActed = new int[2];
                    
                    cScript.m_hasActed[0] = 0;
                    cScript.m_hasActed[1] = 0;
                    
                    // Link to player
                    cScript.m_player = playScript;
                    playScript.name = "Team " + (i + 1).ToString();

                    playScript.m_characters.Add(cScript);
                    newChar.GetComponent<ObjectScript>().PlaceRandomly(this);
                }
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!m_battle)
            return;

        Inputs();
        Hover();
        EndOfActionTimer();
        EndTurn(false);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (m_battle)
            return;

        else if (collision.collider.tag == "Player")
        {
            JoinBattle(collision.gameObject.GetComponent<CharacterScript>());

            if (collision.collider.gameObject == m_field.m_mainChar.gameObject)
                BattleOverview();
        }
    }

    void Inputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PanelScript rEndPan = PanelScript.GetPanel("Round End Panel");
            if (rEndPan.m_inView && rEndPan.m_text[0].text == "BATTLE START" && !GameObject.Find("Network") ||
                rEndPan.m_inView && rEndPan.m_text[0].text == "BATTLE START" && GameObject.Find("Network") && GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted)
                StartCoroutine(StartBattle());
            else if (rEndPan.m_inView && rEndPan.m_text[0].text[rEndPan.m_text[0].text.Length - 1] == 'S')
            {
                m_battle = false;
                SceneManager.LoadScene("Menu");
            }
        }

        if (m_camIsFrozen || m_camera.GetComponent<CameraScript>().m_rotate || m_isForcedMove)
            return;

        //if (m_selected && m_selected.m_holding && m_selected.m_holding.tag == "Player")
        //{
        //    // If character is not being viewed
        //    Renderer oldRend = m_selected.m_holding.transform.GetComponentInChildren<Renderer>();
        //    if (oldRend.materials[2].shader != oldRend.materials[0].shader)
        //        return;
        //}

        float w = Input.GetAxis("Mouse ScrollWheel");
        PanelScript actPan = PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)PanelScript.HUDPan.ACT_PAN].GetComponent<PanelScript>();

        if (Input.GetKeyDown(KeyCode.Q) && !PanelScript.GetPanel("Choose Panel").m_inView) //Formally getmousebuttondown(1);
            OnRightClick();
        else if (Input.GetKeyDown(KeyCode.Escape))
            PanelScript.GetPanel("Options Panel").PopulatePanel();
        else if (Input.GetKeyDown(KeyCode.LeftShift) && m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] == 0)
            PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)PanelScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[0].GetComponent<ButtonScript>().Select();
        else if (Input.GetKeyDown(KeyCode.Space) && !PanelScript.GetPanel("Choose Panel").m_inView)
        {
            if (m_currButton)
                m_currButton.GetComponent<ButtonScript>().CloseButton();

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
            if (m_highlightedTile && m_highlightedTile.m_holding && m_highlightedTile.m_holding.tag == "Player")
                if (m_highlightedTile.gameObject.GetComponent<Renderer>().material.color != TileScript.c_attack &&
                    m_highlightedTile.gameObject.GetComponent<Renderer>().material.color != Color.yellow)
                    PanelScript.GetPanel("ActionViewer Panel").m_panels[2].m_inView = false;
        }
    }

    public void OnRightClick()
    {
        m_selected = null;
        if (m_currButton && !m_camIsFrozen)
        {
            m_currButton.GetComponent<Image>().color = m_currButton.GetComponent<ButtonScript>().m_oldColor;
            if (m_currButton.GetComponent<PanelScript>())
                m_currButton.GetComponent<PanelScript>().m_inView = false;
            m_currButton = null;
            TileScript selectedTileScript = m_currCharScript.m_tile.GetComponent<TileScript>();
            if (selectedTileScript.m_radius.Count > 0)
                selectedTileScript.ClearRadius();

            PanelScript.GetPanel("ActionViewer Panel").ClosePanel();
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
        actPreviewScript.ClosePanel();

        if (m_highlightedTile)
            m_highlightedTile.ClearRadius();

        currTScript.ClearRadius();
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
                    PanelScript.GetPanel("HUD Panel RIGHT").ClosePanel();

                if (m_currCharScript && m_currRound.Count == m_livingPlayersInRound - 1 && m_currCharScript.m_hasActed[(int)CharacterScript.trn.ACT] == 0)
                    m_camera.GetComponent<CameraScript>().m_target = m_currCharScript.gameObject;
            }
        }
    }


    // Hover
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
                PanelScript.GetPanel("StatusViewer Panel").ClosePanel();

            TurnMoveSelectOff();
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

        if (tScript && PanelScript.GetPanel("HUD Panel LEFT").m_panels[2].m_buttons[0].GetComponent<Image>().color == Color.cyan)
            CheckRange(tScript);

        // If you hit a character/character tile, show their info
        if (charScript)
            charScript.HighlightCharacter();

        if (tScript)
            tScript.HandleTile();

        // If you hit a character/character tile and while you are aiming with an attack, show the new health after a hit
        if (charScript && tScript.gameObject.GetComponent<Renderer>().material.color == new Color(1, 0, 0, 1) ||
            charScript && tScript.gameObject.GetComponent<Renderer>().material.color == Color.yellow)
        {
            PanelScript.GetPanel("ActionViewer Panel").m_panels[2].m_cScript = charScript;
            PanelScript.GetPanel("ActionViewer Panel").m_panels[2].PopulatePanel();
        }

        if (m_highlightedTile && tScript != m_highlightedTile)
        {
            if (!PanelScript.GetPanel("StatusViewer Panel").m_cScript)
                PanelScript.GetPanel("StatusViewer Panel").ClosePanel();
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

    private void DeselectHighlightedTile()
    {
        PanelScript.GetPanel("ActionViewer Panel").m_panels[2].ClosePanel();

        if (m_selected && m_selected.m_holding && m_selected.m_holding.tag == "Player" && 
            m_selected.m_holding.GetComponent<CharacterScript>().m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor == Color.magenta || 
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

        hudPanScript.ClosePanel();
        m_highlightedTile = null;
    }


    // Turn
    public void BattleOverview()
    {
        CameraScript cam = m_camera.GetComponent<CameraScript>();
        cam.m_rotate = true;
        cam.m_zoomIn = false;
        m_camIsFrozen = true;
        cam.m_target = gameObject;
        PanelScript rEndPan = PanelScript.GetPanel("Round End Panel");
        rEndPan.m_inView = true;
        rEndPan.GetComponentInChildren<Text>().text = "BATTLE START";
        m_battle = true;
    }

    public IEnumerator StartBattle()
    {
        RandomEnvironmentInit();

        if (!m_field)
        {
            if (GameObject.Find("Network"))
                ServerInit();
            else
                CharacterInit();
        }

        m_camera.GetComponent<CameraScript>().m_zoomIn = true;
        m_camera.GetComponent<CameraScript>().m_rotate = false;
        PanelScript.GetPanel("HUD Panel LEFT").m_inView = true;
        PanelScript.GetPanel("Round Panel").m_inView = true;

        yield return new WaitForSeconds(.1f);

        NewTurn(false);
    }

    public void NewTurn(bool _isForced)
    {
        if (m_currRound.Count == 0 && GameObject.Find("Network") && !GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted)
            return;

        // Turn previous character back to original color
        if (m_currCharScript)
        {
            m_currCharScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].SetActive(false);
            m_currCharScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor = Color.white;
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

        if (!newRound && !_isForced)
        {
            m_newTurn = true;
            CameraScript camScript = m_camera.GetComponent<CameraScript>();
            camScript.m_target = m_currCharScript.gameObject;
        }

        if (m_currCharScript.m_turnPanels.Count > 0)
            m_currCharScript.m_turnPanels.RemoveAt(0);

        PanelScript HUDLeftScript = PanelScript.GetPanel("HUD Panel LEFT");
        HUDLeftScript.m_cScript = m_currCharScript;
        HUDLeftScript.PopulatePanel();

        m_currCharScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].gameObject.SetActive(true);
        m_currCharScript.m_particles[(int)CharacterScript.prtcles.CHAR_MARK].GetComponent<ParticleSystem>().startColor = Color.green;

        if (m_currCharScript.m_isAI)
            m_currCharScript.AITurn();

        m_camIsFrozen = true;
    }

    public void EndTurn(bool _isForced)
    {
        if (!m_currCharScript || !m_currCharScript.m_anim ||
            GameObject.Find("Network") && !m_currCharScript.CheckIfMine() && !_isForced)
            return;

        PlayerScript winningTeam = null;
        int numTeamsActive = 0;
        for (int i = 0; i < m_players.GetComponents<PlayerScript>().Length; i++)
        {
            PlayerScript p = m_players.GetComponents<PlayerScript>()[i];
            for (int j = 0; j < p.m_characters.Count; j++)
            {
                CharacterScript c = p.m_characters[j];
                if (c.m_isAlive)
                {
                    numTeamsActive++;
                    winningTeam = p;
                    break;
                }
            }
        }

        if (numTeamsActive < 2)
        {
            GameOver(winningTeam);
            return;
        }

        if (m_currCharScript.m_anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base.Idle Melee") ||
            m_currCharScript.m_anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base.Death") || _isForced)
            if (m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] + m_currCharScript.m_hasActed[(int)CharacterScript.trn.ACT] > 3 &&
                !m_isForcedMove && !m_camIsFrozen && !PanelScript.GetPanel("Choose Panel").m_inView && m_actionEndTimer == 0 || _isForced)
            {
                if (GameObject.Find("Network") && !GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted &&
                    !_isForced)
                {
                    ClientScript c = GameObject.Find("Network").GetComponent<ClientScript>();
                    c.Send("TURNEND~", c.m_reliableChannel);
                    m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] = 0;
                    m_currCharScript.m_hasActed[(int)CharacterScript.trn.ACT] = 0;
                    return;
                }
                else if (GameObject.Find("Network") && GameObject.Find("Network").GetComponent<ServerScript>().m_isStarted)
                {
                    ServerScript s = GameObject.Find("Network").GetComponent<ServerScript>();
                    s.Send("TURNEND~", s.m_reliableChannel, s.m_clients);
                }

                print(m_currCharScript.m_name + " has ended their turn.\n");
                PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)PanelScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[(int)CharacterScript.trn.MOV].interactable = true;
                StatusScript.UpdateStatus(m_currCharScript.gameObject, StatusScript.mode.TURN_END);
                m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] = 0;
                m_currCharScript.m_hasActed[(int)CharacterScript.trn.ACT] = 0;
                m_currCharScript.m_currAction = "";
                NewTurn(false);
            }
    }

    public void NewRound()
    {
        List<int> roundOrder = new List<int>();

        int numPool = PreRoundSetUp();
        int randNum;
        int currNum;

        do
        {
            randNum = Random.Range(0, numPool);
            currNum = 0;

            for (int i = 0; i < m_characters.Count; i++)
            {
                CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();
                if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] <= 0 || charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0 ||
                    !charScript.m_isAlive)
                    continue;

                currNum += charScript.m_tempStats[(int)CharacterScript.sts.SPD];

                if (randNum < currNum)
                {
                    m_currRound.Add(m_characters[i]);
                    roundOrder.Add(charScript.m_id);

                    if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] >= 10)
                        numPool -= 10;
                    else
                        numPool -= charScript.m_tempStats[(int)CharacterScript.sts.SPD];

                    charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 10;
                    break;
                }
            }
        } while (m_currRound.Count < m_livingPlayersInRound && numPool > 0);

        PostRoundSetUp(roundOrder);
    }

    private int PreRoundSetUp()
    {
        m_currCharScript = null;
        m_livingPlayersInRound = 0;
        int numPool = 0;

        for (int i = 0; i < m_characters.Count; i++)
        {
            StatusScript.UpdateStatus(m_characters[i], StatusScript.mode.ROUND_END);
            CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();

            if (charScript.m_isAlive)
                m_livingPlayersInRound++;

            if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] < 0 || charScript.m_effects[(int)StatusScript.effects.STUN] && i == 0 ||
                !charScript.m_isAlive)
                continue;
            else
                numPool += charScript.m_tempStats[(int)CharacterScript.sts.SPD];
        }

        return numPool;
    }

    private void PostRoundSetUp(List<int> _roundOrder)
    {
        for (int i = 0; i < m_characters.Count; i++)
        {
            CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();
            if (!charScript.m_effects[(int)StatusScript.effects.STUN] && charScript.m_isAlive)
                charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 10;
        }

        m_livingPlayersInRound = m_currRound.Count;
        m_roundCount++;
        PanelScript roundPanScript = PanelScript.GetPanel("Round Panel");
        roundPanScript.PopulatePanel();

        PanelScript.GetPanel("Turn Panel").NewTurnOrder();

        ObjectScript obj = SpawnItem(-1, -1, -1);

        PanelScript roundPan = PanelScript.GetPanel("Round End Panel");
        roundPan.m_inView = true;
        roundPan.GetComponentInChildren<Text>().text = "NEW ROUND";

        if (GameObject.Find("Network"))
        {
            ServerScript network = GameObject.Find("Network").GetComponent<ServerScript>();
            string roundData = PackUpRoundData(_roundOrder, obj);
            network.Send(roundData, network.m_reliableChannel, network.m_clients);
        }
    }

    public void GameOver(PlayerScript _winningTeam)
    {
        PanelScript roundPan = PanelScript.GetPanel("Round End Panel");
        roundPan.m_inView = true;
        roundPan.m_text[0].text = _winningTeam.name + " WINS";
        m_camera.GetComponent<CameraScript>().m_rotate = true;
        m_currCharScript = null;
    }

    public void Pass()
    {
        m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] = 2;
        m_currCharScript.m_hasActed[(int)CharacterScript.trn.ACT] = 2;
        PanelScript.CloseHistory();
        PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)PanelScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[1].GetComponent<Image>().color = Color.white;
        m_currButton = null;
    }


    // Network
    public void ServerInit()
    {
        ServerScript network = GameObject.Find("Network").GetComponent<ServerScript>();

        for (int i = 0; i < network.m_clients.Count; i++)
        {
            PlayerScript playScript = m_players.AddComponent<PlayerScript>();
            playScript.m_characters = new List<CharacterScript>();
            playScript.m_bScript = this;
            playScript.m_energy = new int[4];
            playScript.m_id = i;

            //playScript.name = "Team " + (i + 1).ToString();

            string[] chars = network.m_clients[i].m_team.Split(';');

            for (int j = 0; j < chars.Length; j++)
            {
                string[] data = chars[j].Split('|');

                // Set up character
                GameObject[] characterTypes = Resources.LoadAll<GameObject>("OBJs/Characters");
                GameObject newChar = Instantiate(characterTypes[int.Parse(data[(int)PlayerPrefScript.netwrkPak.GENDER])]);
                newChar.name = data[(int)PlayerPrefScript.netwrkPak.NAME];

                CharacterScript cScript = newChar.GetComponent<CharacterScript>();
                cScript = ReadCharNetData(data, cScript);

                if (i == 0)
                    cScript.m_teamColor = new Color(1, .5f, 0, 1);
                else if (i == 1)
                    cScript.m_teamColor = new Color(.5f, 0, 1, 1);
                else if (i == 2)
                    cScript.m_teamColor = new Color(.8f, 1, 0, 1);
                else if (i == 3)
                    cScript.m_teamColor = Color.magenta;

                cScript.CharInit();
                m_characters.Add(newChar);

                // Link to player
                cScript.m_player = playScript;
                playScript.name = "Team " + (0).ToString();

                playScript.m_characters.Add(cScript);
                newChar.GetComponent<ObjectScript>().PlaceRandomly(this);
            }
        }

        PackUpInitData();
    }

    public void ClientEnvironmentInit(string _data)
    {
        GameObject[] environmentalOBJs = Resources.LoadAll<GameObject>("OBJs/Environmental");

        string[] objs = _data.Split('|');

        for (int i = 0; i < objs.Length; i++)
        {
            string[] obj = objs[i].Split(',');
            GameObject newOBJ = Instantiate(environmentalOBJs[int.Parse(obj[0])]);
            newOBJ.GetComponent<ObjectScript>().SetRotation((TileScript.nbors)int.Parse(obj[1]));
            newOBJ.GetComponent<ObjectScript>().PlaceOBJ(this, int.Parse(obj[2]), int.Parse(obj[3]));
            m_obstacles.Add(newOBJ);
        }
    }

    public void ClientCharInit(string _data)
    {
        GameObject[] characterTypes = Resources.LoadAll<GameObject>("OBJs/Characters");
        string[] characters = _data.Split(';');

        for (int i = 0; i < characters.Length; i++)
        {
            string[] character = characters[i].Split('|');

            GameObject newChar = Instantiate(characterTypes[int.Parse(character[(int)PlayerPrefScript.netwrkPak.GENDER])]);
            newChar.name = character[(int)PlayerPrefScript.netwrkPak.NAME];

            CharacterScript cScript = newChar.GetComponent<CharacterScript>();
            cScript = ReadCharNetData(character, cScript);

            int team = int.Parse(character[(int)PlayerPrefScript.netwrkPak.TEAM]);

            // Color character
            if (team == 0)
                cScript.m_teamColor = new Color(1, .5f, 0, 1);
            else if (team == 1)
                cScript.m_teamColor = new Color(.5f, 0, 1, 1);
            else if (team == 2)
                cScript.m_teamColor = new Color(.8f, 1, 0, 1);
            else if (team == 3)
                cScript.m_teamColor = Color.magenta;

            cScript.CharInit();
            m_characters.Add(newChar);

            PlayerScript[] p = m_players.GetComponents<PlayerScript>();

            if (team >= p.Length)
            {
                m_players.AddComponent<PlayerScript>();
                p = m_players.GetComponents<PlayerScript>();

                p[p.Length - 1].m_characters = new List<CharacterScript>();
                p[p.Length - 1].m_bScript = this;
                p[p.Length - 1].m_energy = new int[4];
                p[p.Length - 1].m_id = team;
            }

            // Link to player
            p[team].m_characters.Add(cScript);
            cScript.m_player = m_players.GetComponents<PlayerScript>()[team];

            // Place on Board
            string[] pos = character[(int)PlayerPrefScript.netwrkPak.POS].Split(',');
            cScript.PlaceOBJ(this, int.Parse(pos[0]), int.Parse(pos[1]));
        }
        
        // Doing this for each team unnecessarily
        m_camera.GetComponent<CameraScript>().m_zoomIn = true;
        m_camera.GetComponent<CameraScript>().m_rotate = false;
        PanelScript.GetPanel("HUD Panel LEFT").m_inView = true;
        PanelScript.GetPanel("Round Panel").m_inView = true;
    }

    public void ClientRoundInit(string _data)
    {
        string[] roundOrder = _data.Split(';')[0].Split(',');
        string[] charSpeed = _data.Split(';')[1].Split(',');
        string[] powerUp = _data.Split(';')[2].Split(',');

        for (int i = 0; i < roundOrder.Length; i++)
            m_currRound.Add(m_netOBJs[int.Parse(roundOrder[i])]);

        for (int i = 0; i < charSpeed.Length; i++)
            m_characters[i].GetComponent<CharacterScript>().m_tempStats[(int)CharacterScript.sts.SPD] = int.Parse(charSpeed[i]);

        m_livingPlayersInRound = m_currRound.Count;
        m_roundCount++;
        PanelScript roundPanScript = PanelScript.GetPanel("Round Panel");
        roundPanScript.PopulatePanel();

        PanelScript.GetPanel("Turn Panel").NewTurnOrder();

        SpawnItem(int.Parse(powerUp[0]), int.Parse(powerUp[1]), int.Parse(powerUp[2]));

        PanelScript roundPan = PanelScript.GetPanel("Round End Panel");
        roundPan.m_inView = true;
        roundPan.GetComponentInChildren<Text>().text = "NEW ROUND";

        NewTurn(true);
    }

    private CharacterScript ReadCharNetData(string[] _data, CharacterScript _char)
    {
        _char.m_name = _data[(int)PlayerPrefScript.netwrkPak.NAME];
        _char.m_color = _data[(int)PlayerPrefScript.netwrkPak.COLOR];

        string[] acts = _data[(int)PlayerPrefScript.netwrkPak.ACTIONS].Split(',');
        string[] actionsForChar = new string[acts.Length];

        for (int k = 0; k < acts.Length; k++)
        {
            int ind = int.Parse(acts[k]) - 1;
            DatabaseScript db = GameObject.Find("Database").GetComponent<DatabaseScript>();
            actionsForChar[k] = db.m_actions[ind];
        }

        _char.m_actions = actionsForChar;
        _char.m_exp = int.Parse(_data[(int)PlayerPrefScript.netwrkPak.EXP]);
        _char.m_level = int.Parse(_data[(int)PlayerPrefScript.netwrkPak.LVL]);
        _char.m_gender = int.Parse(_data[(int)PlayerPrefScript.netwrkPak.GENDER]);
        _char.m_isAI = bool.Parse(_data[(int)PlayerPrefScript.netwrkPak.AI]);

        // Load in stats
        string[] stats = _data[(int)PlayerPrefScript.netwrkPak.STATS].Split(',');

        if (_char.m_stats.Length == 0)
        {
            _char.m_stats = new int[(int)CharacterScript.sts.TOT];
            _char.m_tempStats = new int[(int)CharacterScript.sts.TOT];
            _char.InitializeStats();
        }

        for (int k = 0; k < _char.m_stats.Length; k++)
        {
            _char.m_stats[k] = int.Parse(stats[k]);
            _char.m_tempStats[k] = int.Parse(stats[k]);
        }

        return _char;
    }

    private void PackUpInitData()
    {
        ServerScript network = GameObject.Find("Network").GetComponent<ServerScript>();

        string enviroData = "READYENVIRONMENT~";
        for (int i = 0; i < m_obstacles.Count; i++)
        {
            ObjectScript obj = m_obstacles[i].GetComponent<ObjectScript>();

            enviroData += obj.m_name + "," + (int)obj.m_facing + "," + obj.m_tile.m_x.ToString() + "," + obj.m_tile.m_z.ToString() + "|";
        }

        enviroData = enviroData.Trim('|');
        network.Send(enviroData, network.m_reliableChannel, network.m_clients);


        PlayerScript[] players = m_players.GetComponents<PlayerScript>();

        for (int i = 0; i < players.Length; i++)
        {
            string charString = "READYCHARS~";
            for (int j = 0; j < players[i].m_characters.Count; j++)
            {
                CharacterScript charScript = players[i].m_characters[j];
                char symbol = '|';

                charString += charScript.m_name + symbol;
                charString += charScript.m_color + symbol;

                for (int k = 0; k < charScript.m_actions.Length; k++)
                    charString += DatabaseScript.GetActionData(charScript.m_actions[k], DatabaseScript.actions.ID) + ',';
                charString = charString.Trim(','); charString += symbol;

                charString += charScript.m_exp.ToString() + symbol;
                charString += charScript.m_level.ToString() + symbol;
                charString += charScript.m_gender.ToString() + symbol;
                charString += charScript.m_isAI.ToString() + symbol;

                string stats = null;
                for (int k = 0; k < charScript.m_stats.Length; k++)
                {
                    if (k != 0)
                        stats += ",";
                    stats += charScript.m_stats[k];
                }

                charString += stats + symbol;

                charString += i.ToString() + symbol;

                charString += charScript.m_tile.m_x.ToString() + ',' + charScript.m_tile.m_z.ToString() + ';';

            }
            charString = charString.Trim(';');
            network.Send(charString, network.m_reliableChannel, network.m_clients);
        }
    }

    public string PackUpRoundData(List<int> _roundOrder, ObjectScript _obj)
    {
        string data = "NEWROUND~";

        for (int i = 0; i < _roundOrder.Count; i++)
            data += _roundOrder[i].ToString() + ",";

        data = data.Trim(',') + ';';

        // Update speed
        for (int i = 0; i < m_characters.Count; i++)
            data += m_characters[i].GetComponent<CharacterScript>().m_tempStats[(int)CharacterScript.sts.SPD].ToString() + ",";

        data = data.Trim(',') + ';';

        PowerupScript pUp = _obj.GetComponent<PowerupScript>();


        data += pUp.name + ',';
        data += _obj.m_tile.m_x.ToString() + ',' + _obj.m_tile.m_z.ToString();

        return data;
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
        m_camera.GetComponent<CameraScript>().m_target = pUP;

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
        m_characters.Add(_char.gameObject);
    }

    public void AddToOBJList(ObjectScript _obj)
    {
        for (int i = 0; i < m_netOBJs.Length; i++)
        {
            if (m_netOBJs[i] == null)
            {
                _obj.m_id = i;
                m_netOBJs[i] = _obj.gameObject;
                return;
            }
        }
    }

    private void CheckRange(TileScript _hTile)
    {
        for (int i = 0; i < m_characters.Count; i++)
        {
            CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();

            if (charScript == m_currCharScript)
                continue;

            charScript.m_RangeCheck[0].transform.parent.gameObject.SetActive(true);

            if (charScript.m_isAlive)
            {
                for (int j = 0; j < m_currCharScript.m_actions.Length; j++)
                {
                    int finalRng = m_currCharScript.ActFinalDistAfterMods(m_currCharScript.m_actions[j]);

                    if (finalRng < TileScript.CaclulateDistance(_hTile, charScript.m_tile) || _hTile.CheckIfBlocked(charScript.m_tile) &&
                            CharacterScript.UniqueActionProperties(m_currCharScript.m_actions[j], CharacterScript.uniAct.IS_NOT_BLOCK) < 1)
                        charScript.m_RangeCheck[j].SetActive(false);
                    else
                    {
                        charScript.m_RangeCheck[j].SetActive(true);
                        charScript.m_RangeCheck[j].GetComponent<TextMesh>().text = DatabaseScript.GetActionData(m_currCharScript.m_actions[j], DatabaseScript.actions.NAME);
                        if (m_currCharScript.m_player.CheckEnergy(DatabaseScript.GetActionData(m_currCharScript.m_actions[j], DatabaseScript.actions.ENERGY)))
                            charScript.m_RangeCheck[j].GetComponent<TextMesh>().offsetZ = -20;
                        else
                            charScript.m_RangeCheck[j].GetComponent<TextMesh>().offsetZ = 10;
                    }
                }
            }
        }
    }

    public void TurnMoveSelectOff()
    {
        for (int i = 0; i < m_characters.Count; i++)
            m_characters[i].GetComponent<CharacterScript>().m_RangeCheck[0].transform.parent.gameObject.SetActive(false);
    }
}
