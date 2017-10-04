using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BoardScript : MonoBehaviour {

    public enum pnls { MAIN_PANEL, ACTION_PANEL, AUXILIARY_PANEL, STATUS_PANEL, HUD_LEFT_PANEL, HUD_RIGHT_PANEL, TURN_PANEL,
        ROUND_PANEL, ENERGY_PANEL, STATUS_VIEWER_PANEL, STATUS_SELECTOR, ENERGY_SELECTOR, ACTION_PREVIEW,
        ACTION_VIEWER_PANEL, CHARACTER_VIEWER_PANEL, TOTAL_PANEL }

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
    public Button m_currButton;

    // Use this for initialization
    void Start ()
    {
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
            m_width = (int) (r.bounds.size.x / m_tile.GetComponent<Renderer>().bounds.size.x);
            m_height = (int)(r.bounds.size.z / m_tile.GetComponent<Renderer>().bounds.size.z);
        }

        InitBoardTiles();
        AssignNeighbors();
        EnvironmentInit();
        CharacterInit();
        NewTurn();
    }
	
	// Update is called once per frame
	void Update ()
    {
        Inputs();
        Hover();
    }

    void Inputs()
    {
        float w = Input.GetAxis("Mouse ScrollWheel");
        PanelScript actPan = PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)CharacterScript.HUDPan.ACT_PAN].GetComponent<PanelScript>();

        if (Input.GetKeyDown(KeyCode.Q)) //Formally getmousebuttondown(1);
            OnRightClick();
        else if (Input.GetMouseButtonDown(0) && m_camera.GetComponent<CameraScript>().m_rotate)
            SceneManager.LoadScene("Menu");
        else if (Input.GetKeyDown(KeyCode.Escape))
            PanelScript.GetPanel("Options Panel").PopulatePanel();
        else if (Input.GetKeyDown(KeyCode.LeftShift) && m_currCharScript.m_hasActed[(int)CharacterScript.trn.MOV] == 0)
            PanelScript.GetPanel("HUD Panel LEFT").m_panels[(int)CharacterScript.HUDPan.MOV_PASS].GetComponent<PanelScript>().m_buttons[0].GetComponent<ButtonScript>().Select();
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_currButton)
            {
                m_currButton.GetComponent<Image>().color = Color.white;
                m_currButton.GetComponent<ButtonScript>().m_main.m_inView = false;
                m_currButton = null;
                TileScript selectedTileScript = m_currCharScript.m_tile.GetComponent<TileScript>();
                if (selectedTileScript.m_radius.Count > 0)
                    selectedTileScript.ClearRadius(selectedTileScript);
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
                        m_currCharScript.m_player.GetComponent<PlayerScript>().CheckEnergy(DatabaseScript.GetActionData(actPan.m_buttons[i + 1].GetComponent<ButtonScript>().m_action, DatabaseScript.actions.ENERGY)))
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
                         m_currCharScript.m_player.GetComponent<PlayerScript>().CheckEnergy(DatabaseScript.GetActionData(actPan.m_buttons[i - 1].GetComponent<ButtonScript>().m_action, DatabaseScript.actions.ENERGY)))
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

    private void EnvironmentInit()
    {
        int numOfObstacles = (m_height * m_width) / m_environmentalDensity;

        while (numOfObstacles > 0)
        {
            int randomOBJ = Random.Range(0, m_environmentOBJs.Length);

            GameObject newOBJ = Instantiate(m_environmentOBJs[randomOBJ]);
            RandomRotation(newOBJ);
            PlaceOnBoard(newOBJ);
            m_obstacles.Add(newOBJ);
            if (newOBJ.GetComponent<ObjectScript>().m_width == 0)
                numOfObstacles--;
            else
                numOfObstacles -= newOBJ.GetComponent<ObjectScript>().m_width;
        }
    }

    public void RandomRotation(GameObject _obj)
    {
        int randomRot = Random.Range(0, 5);
        _obj.transform.Rotate(0, randomRot * 90, 0);

        if (randomRot == 0)
            _obj.GetComponent<ObjectScript>().m_facing = TileScript.nbors.bottom;
        else if (randomRot == 1)
            _obj.GetComponent<ObjectScript>().m_facing = TileScript.nbors.left;
        else if (randomRot == 2)
            _obj.GetComponent<ObjectScript>().m_facing = TileScript.nbors.top;
        else if (randomRot == 3)
            _obj.GetComponent<ObjectScript>().m_facing = TileScript.nbors.right;
    }

    public void OnRightClick()
    {
        m_selected = null;
        if (m_currButton)
        {
            m_currButton.GetComponent<Image>().color = Color.white;
            m_currButton = null;
            TileScript selectedTileScript = m_currCharScript.m_tile.GetComponent<TileScript>();
            if (selectedTileScript.m_radius.Count > 0)
                selectedTileScript.ClearRadius(selectedTileScript);

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

        Renderer sRend = currTScript.m_radius[0].GetComponent<Renderer>();

        if (m_highlightedTile)
        {
            TileScript highScript = m_highlightedTile.GetComponent<TileScript>();
            currTScript.ClearRadius(highScript);
        }

        currTScript.ClearRadius(currTScript);
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
                    cScript.m_player = m_players[i];
                    PlayerScript playScript = m_players[i].GetComponent<PlayerScript>();
                    m_players[i].name = "Team " + (i + 1).ToString();
                    playScript.m_characters.Add(newChar);

                    PlaceOnBoard(newChar);
                }
            }
    }

    private void PlaceOnBoard(GameObject _obj)
    {
        // Set up position
        TileScript script;
        ObjectScript objScript = _obj.GetComponent<ObjectScript>();
        bool isPlacable = false;
        int randX;
        int randZ;

        do
        {
            randX = Random.Range(0, m_width - 1);
            randZ = Random.Range(0, m_height - 1);

            script = m_tiles[randX + randZ * m_width].GetComponent<TileScript>();

            if (!script.m_holding)
            {
                if (objScript.m_width <= 1)
                    isPlacable = true;
                else if (objScript.m_width == 2 && script.m_neighbors[(int)objScript.m_facing] && !script.m_neighbors[(int)objScript.m_facing].GetComponent<TileScript>().m_holding)
                {
                    isPlacable = true;
                    script.m_neighbors[(int)objScript.m_facing].GetComponent<TileScript>().m_holding = _obj;
                }
            }

        } while (!isPlacable);

        script.m_holding = _obj;
        _obj.transform.SetPositionAndRotation(m_tiles[randX + randZ * m_width].transform.position, _obj.transform.rotation);
        objScript.m_tile = m_tiles[randX + randZ * m_width];
        objScript.m_boardScript = GetComponent<BoardScript>();
    }

    public void Hover()
    {
        if (PanelScript.m_confirmPanel.m_inView || m_camIsFrozen || !m_currButton && m_currCharScript.m_tile.GetComponent<TileScript>().m_radius.Count > 0 || 
            m_selected && m_selected.GetComponent<TileScript>().m_holding && m_selected.GetComponent<TileScript>().m_holding.tag == "Player")
            return;
        else if (!m_currCharScript || PanelScript.CheckIfPanelOpen())
        {
            if (m_oldTile)
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
                TileScript m_highlightedTileScript = m_highlightedTile.GetComponent<TileScript>();
                if (m_highlightedTileScript.m_holding)
                    DeselectHighlightedTile();
            }
            return;
        }

        TileScript tScript = null;
        CharacterScript charScript = null;

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

        if (charScript && PanelScript.GetPanel("HUD Panel RIGHT").m_inView == false)
            HighlightCharacter(charScript);

        if (tScript)
            HandleTile(tScript);

        if (charScript && tScript.gameObject.GetComponent<Renderer>().material.color == new Color(1, 0, 0, 1) ||
            charScript && tScript.gameObject.GetComponent<Renderer>().material.color == Color.yellow)
        {
            PanelScript.GetPanel("ActionPreview").m_cScript = charScript;
            PanelScript.GetPanel("ActionPreview").PopulatePanel();
        }

        if (m_highlightedTile && tScript != m_highlightedTile)
        {
            TileScript m_highlightedTileScript = m_highlightedTile.GetComponent<TileScript>();
            if (m_highlightedTileScript.m_holding && m_highlightedTileScript.m_holding.tag == "Player" && hit.collider.gameObject != m_highlightedTileScript.m_holding)
                DeselectHighlightedTile();
        }

        if (tScript)
            m_highlightedTile = tScript;
    }

    private void HandleTile(TileScript _target)
    {
        if (m_oldTile)
        {
            Renderer oTR = m_oldTile.GetComponent<Renderer>();
            if (oTR.material.color == Color.yellow)
            {
                TileScript oldTileScript = m_oldTile.GetComponent<TileScript>();
                oldTileScript.ClearRadius(oldTileScript);
            }
            else if (oTR.material.color.a != 0)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a - 0.5f);

            // Hacky fix for when you spawn colored m_tiles for range and your cursor starts on one of the m_tiles
            if (oTR.material.color.r == 0 && oTR.material.color.b == 1 && oTR.material.color.a == 0f || oTR.material.color.r == 1 && oTR.material.color.b == 0 && oTR.material.color.a == 0f)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a + 0.5f);
        }

        Renderer tarRend = _target.gameObject.GetComponent<Renderer>();

        // TARGET RED TILE

        bool targetSelf = true;
        int rad = m_currCharScript.m_currRadius + m_currCharScript.m_tempStats[(int)CharacterScript.sts.RAD];

        if (m_currCharScript.m_currAction.Length > 0 && tarRend.material.color == CharacterScript.c_attack) // REFACTOR: All this code just for piercing attack...
        {
            string actName = DatabaseScript.GetActionData(m_currCharScript.m_currAction, DatabaseScript.actions.NAME);
            string actRng = DatabaseScript.GetActionData(m_currCharScript.m_currAction, DatabaseScript.actions.RNG);

            if (actName == "Piercing ATK" || actName == "Thrust ATK")
            {
                _target.FetchTilesWithinRange(int.Parse(actRng) + m_currCharScript.m_tempStats[(int)CharacterScript.sts.RNG], Color.yellow, true, TileScript.targetRestriction.HORVERT, false);
                m_oldTile = _target;
                return;
            }
            else if (actName == "Cross ATK" || actName == "Dual ATK")
            {
                _target.FetchTilesWithinRange(int.Parse(actRng) + m_currCharScript.m_tempStats[(int)CharacterScript.sts.RNG], Color.yellow, false, TileScript.targetRestriction.DIAGONAL, false);
                m_oldTile = _target;
                return;
            }
            else if (actName == "Slash ATK")
            {
                _target.FetchTilesWithinRange(int.Parse(actRng) + m_currCharScript.m_tempStats[(int)CharacterScript.sts.RNG], Color.yellow, false, TileScript.targetRestriction.HORVERT, true);
                m_oldTile = _target;
                return;
            }

            if (actName == "Fortifying ATK" || actName == "Blinding ATK")
                targetSelf = false;

            if (actName == "Magnet ATK")
                rad += m_currCharScript.m_tempStats[(int)CharacterScript.sts.TEC];
        }

        if (m_currCharScript.m_currRadius + m_currCharScript.m_tempStats[(int)CharacterScript.sts.RAD] > 0 && tarRend.material.color == CharacterScript.c_attack)
            _target.FetchTilesWithinRange(rad, Color.yellow, targetSelf, TileScript.targetRestriction.NONE, true);
        else
            tarRend.material.color = new Color(tarRend.material.color.r, tarRend.material.color.g, tarRend.material.color.b, tarRend.material.color.a + 0.5f);

        m_oldTile = _target;
    }

    public void HighlightCharacter(CharacterScript _character)
    {
        if (_character && _character != m_currCharScript && _character.m_isAlive)
        {
            // Change color of turn panel to indicate where the character is in the turn order
            for (int i = 0; i < _character.m_turnPanels.Count; i++)
            {
                Image turnPanImage = _character.m_turnPanels[i].GetComponent<Image>();
                turnPanImage.color = Color.cyan;
            }

            // Reveal right HUD with highlighted character's data
            PanelScript hudPanScript = PanelScript.GetPanel("HUD Panel RIGHT");
            hudPanScript.m_cScript = _character;
            hudPanScript.PopulatePanel();

            Image[] hudEnergy = hudPanScript.m_panels[(int)CharacterScript.HUDPan.ENG_PAN].GetComponentsInChildren<Image>();
            int[] charEnergy = _character.m_player.GetComponent<PlayerScript>().m_energy;
            for (int i = 0; i < charEnergy.Length; i++)
            {
                Text t = hudEnergy[i + 1].GetComponentInChildren<Text>();
                t.text = charEnergy[i].ToString();
            }
        }
    }

    private void DeselectHighlightedTile()
    {
        PanelScript hudPanScript = PanelScript.GetPanel("HUD Panel RIGHT");
        PanelScript actPreviewScript = PanelScript.GetPanel("ActionPreview");

        TileScript m_highlightedTileScript = m_highlightedTile.GetComponent<TileScript>();

        if (m_highlightedTileScript.m_holding.GetComponent<CharacterScript>())
        {
            CharacterScript colScript = m_highlightedTileScript.m_holding.GetComponent<CharacterScript>();

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
        actPreviewScript.m_inView = false;
    }

    public void NewTurn()
    {
        PlayerScript playScript = null;
        PlayerScript winningTeam = null;
        int numTeamsActive = 0;
        for (int i = 0; i < m_players.Length; i++)
        {
            playScript = m_players[i].GetComponent<PlayerScript>();
            for (int j = 0; j < playScript.m_characters.Count; j++)
            {
                if (playScript.m_characters[j].GetComponent<CharacterScript>().m_isAlive)
                {
                    numTeamsActive++;
                    winningTeam = playScript;
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
            //Renderer oldRenderer = m_currPlayer.GetComponent<Renderer>();
            //oldRenderer.material.color = m_currPlayer.GetComponent<CharacterScript>().m_teamColor;
        }

        if (m_currRound.Count == 0)
            NewRound();
        else
            m_newTurn = true;

        m_currCharScript = m_currRound[0].GetComponent<CharacterScript>();
        m_currRound.RemoveAt(0);

        if (m_currCharScript.m_effects[(int)StatusScript.effects.STUN] || !m_currCharScript.m_isAlive)
        {
            m_currCharScript.m_turnPanels.Clear();
            StatusScript.UpdateStatus(m_currCharScript.gameObject, StatusScript.mode.TURN_END);

            if (m_currRound.Count == 0)
                NewRound();

            m_currCharScript = m_currRound[0].GetComponent<CharacterScript>();
            m_currRound.RemoveAt(0);
        }

        m_currTile = m_currCharScript.m_tile;

        if (m_currCharScript.m_turnPanels.Count > 0)
            m_currCharScript.m_turnPanels.RemoveAt(0);

        PanelScript HUDLeftScript = PanelScript.GetPanel("HUD Panel LEFT");
        HUDLeftScript.m_cScript = m_currCharScript;
        HUDLeftScript.PopulatePanel();

        playScript = m_currCharScript.m_player.GetComponent<PlayerScript>();
        if (playScript)
            playScript.SetEnergyPanel();

        CameraScript camScript = m_camera.GetComponent<CameraScript>();
        camScript.m_target = m_currCharScript.gameObject;

        Renderer rend = m_currCharScript.gameObject.transform.GetComponentInChildren<Renderer>();
        rend.materials[1].shader = Resources.Load<Shader>("Outlined-Silhouette Only (NOT MINE)");

        if (m_currCharScript.m_isAI)
            m_currCharScript.AITurn();
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

        //PanelScript.GetPanel("Turn Panel").NewTurnOrder();
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
