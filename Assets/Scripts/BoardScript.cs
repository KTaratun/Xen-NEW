using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour {

    public enum pnls { MAIN_PANEL, ACTION_PANEL, STATUS_PANEL, HUD_LEFT_PANEL, HUD_RIGHT_PANEL, ROUND_PANEL, AUXILIARY_PANEL, TOTAL_PANEL }

    public Camera m_camera; // Why it do that squiggle?
    public int m_roundCount;
    public GameObject[] m_panels; // All movable GUI on screen
    public GameObject[] m_tiles; // All m_tiles on the board
    public GameObject m_tile; // A reference to the m_tile prefab
    public GameObject m_character; // A reference to the character prefab
    public int m_width; // Width of the board
    public int m_height; // Height of the board
    public GameObject m_selected; // Currently selected m_tile
    private GameObject m_highlightedTile;
    public GameObject m_oldTile; // A pointer to the previously hovered over m_tile
    public GameObject m_currTile; // The m_tile of the player who's turn is currently up
    public GameObject m_currPlayer; // A pointer to the player that's turn is currently up
    public string m_currAction;
    public List<GameObject> m_characters; // A list of all players within the game
    public GameObject[] m_players;
    public List<GameObject> m_currRound; // A list of the players who have taken a turn in the current round
    public GameObject m_isForcedMove;

    // Use this for initialization
    void Start ()
    {
        m_roundCount = 0;
        m_isForcedMove = null;
        Renderer r = GetComponent<Renderer>();
        // If the user didn't assign a m_tile size in the editor, auto adjust the m_tiles to fit the board
        if (m_height == 0 && m_width == 0)
        {
            m_width = (int) (r.bounds.size.x / m_tile.GetComponent<Renderer>().bounds.size.x);
            m_height = (int)(r.bounds.size.z / m_tile.GetComponent<Renderer>().bounds.size.z);
        }

        InitBoardTiles();
        AssignNeighbors();
        CharacterInit();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonDown(1))
            OnRightClick();

        Hover();

        if (m_currPlayer == null)
        {
            NewRound();
            NewTurn();
        }
    }

    public void OnRightClick()
    {
        CharacterScript charScript = m_currPlayer.GetComponent<CharacterScript>();
        TileScript currPScript = charScript.m_tile.GetComponent<TileScript>();

        if (currPScript.m_radius.Count == 0)
            return;

        Renderer sRend = currPScript.m_radius[0].GetComponent<Renderer>();
        if (sRend.material.color == new Color(1, 0, 0, 0.5f) || sRend.material.color == Color.yellow)
        {
            PanelScript actPan = m_panels[(int)pnls.ACTION_PANEL].GetComponent<PanelScript>();
            actPan.m_inView = true;
        }
        else if (sRend.material.color == new Color(0, 0, 1, 0.5f))
        {
            PanelScript panScript = m_panels[(int)pnls.MAIN_PANEL].GetComponent<PanelScript>();
            panScript.m_inView = true;
        }

        if (m_highlightedTile)
        {
            TileScript highScript = m_highlightedTile.GetComponent<TileScript>();
            currPScript.ClearRadius(highScript);
        }
        currPScript.ClearRadius(currPScript);
    }

    public void InitBoardTiles()
    {
        Renderer r = GetComponent<Renderer>();
        m_tiles = new GameObject[m_width * m_height];

        for (int z = 0; z < m_height; z++)
        {
            for (int x = 0; x < m_width; x++)
            {
                GameObject newm_tile = Instantiate(m_tile);

                Renderer ntR = newm_tile.GetComponent<Renderer>();
                ntR.material.color = new Color(1, 1, 1, 0f);

                newm_tile.transform.SetPositionAndRotation(new Vector3(x * ntR.bounds.size.x + (r.bounds.min.x + ntR.bounds.size.x / 2), transform.position.y + 0.1f, z * ntR.bounds.size.z + (r.bounds.min.z + ntR.bounds.size.z / 2)), new Quaternion());

                TileScript tScript = newm_tile.GetComponent<TileScript>();
                tScript.m_x = x;
                tScript.m_z = z;
                tScript.m_boardScript = GetComponent<BoardScript>();

                m_tiles[x + z * m_width] = newm_tile;
            }
        }
    }

    public void AssignNeighbors()
    {
        for (int z = 0; z < m_height; z++)
        {
            for (int x = 0; x < m_width; x++)
            {
                GameObject tempm_tile = m_tiles[x + z * m_width];
                TileScript tScript = tempm_tile.GetComponent<TileScript>();

                tScript.m_neighbors = new GameObject[4];

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
                string key = i.ToString() + ',' + j.ToString() + ",name";
                string name = PlayerPrefs.GetString(key);
                if (name.Length > 0)
                {
                    // Set up character
                    GameObject newChar = Instantiate(m_character);
                    CharacterScript cScript = newChar.GetComponent<CharacterScript>();

                    cScript.name = name;
                    key = i.ToString() + ',' + j.ToString() + ",color";
                    string color = PlayerPrefs.GetString(key);
                    cScript.m_color = color;
                    cScript.SetCharColor();
                    
                    key = i.ToString() + ',' + j.ToString() + ",actions";
                    string[] acts = PlayerPrefs.GetString(key).Split(';');
                    cScript.m_actions = acts;

                    // Set up position
                    TileScript script;
                    int randX;
                    int randZ;
                    
                    do
                    {
                        randX = Random.Range(0, m_width - 1);
                        randZ = Random.Range(0, m_height - 1);
                    
                        script = m_tiles[randX + randZ * m_width].GetComponent<TileScript>();
                    } while (script.m_holding);
                    
                    script.m_holding = newChar;
                    newChar.transform.SetPositionAndRotation(m_tiles[randX + randZ * m_width].transform.position, new Quaternion());
                    cScript.m_tile = m_tiles[randX + randZ * m_width];
                    cScript.m_boardScript = GetComponent<BoardScript>();

                    m_characters.Add(newChar);

                    // Link to player
                    cScript.m_player = m_players[i];
                    PlayerScript playScript = m_players[i].GetComponent<PlayerScript>();
                    playScript.m_characters.Add(newChar);
                }
            }
    }

    public void Hover()
    {
        PanelScript mainPanScript = m_panels[(int)pnls.MAIN_PANEL].GetComponent<PanelScript>();
        PanelScript actPanScript = m_panels[(int)pnls.ACTION_PANEL].GetComponent<PanelScript>();
        PanelScript statPanScript = m_panels[(int)pnls.STATUS_PANEL].GetComponent<PanelScript>();
        PanelScript auxPanScript = m_panels[(int)pnls.AUXILIARY_PANEL].GetComponent<PanelScript>();

        if (!m_currPlayer || actPanScript.m_inView || mainPanScript.m_inView || statPanScript.m_inView || auxPanScript.m_inView)
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
            m_highlightedTile = null;
            return;
        }

        PanelScript hudPanScript = m_panels[(int)pnls.HUD_RIGHT_PANEL].GetComponent<PanelScript>();
        if (hit.collider.gameObject.tag == "Tile")
        {
            if (hit.collider.gameObject != m_oldTile) // REFACTOR ALL THE CODE IN THIS IF STATEMENT
                HandleOldTile(hit.collider.gameObject);
        }

        if (hit.collider.gameObject.tag == "Player")
        {
            CharacterScript colScript = hit.collider.gameObject.GetComponent<CharacterScript>();
            if (hit.collider.gameObject != m_currPlayer)
            {
                // Change color of turn panel to indicate where the character is in the turn order
                if (colScript.m_turnPanel)
                {
                    Image turnPanImage = colScript.m_turnPanel.GetComponent<Image>();
                    turnPanImage.color = Color.cyan;
                }

                // Reveal right HUD with highlighted character's data
                hudPanScript.m_character = hit.collider.gameObject;
                hudPanScript.m_cScript = colScript;
                hudPanScript.PopulateHUD();
                hudPanScript.m_inView = true;

            }
            HandleOldTile(hit.collider.gameObject);
        }
        else if (m_highlightedTile)
        {
            TileScript m_highlightedTileScript = m_highlightedTile.GetComponent<TileScript>();
            if (m_highlightedTileScript.m_holding && m_highlightedTileScript.m_holding.tag == "Player")
            {
                CharacterScript colScript = m_highlightedTileScript.m_holding.GetComponent<CharacterScript>();
                if (colScript.m_turnPanel)
                {
                    Image turnPanImage = colScript.m_turnPanel.GetComponent<Image>();
                    turnPanImage.color = Color.white;
                }
                hudPanScript.m_inView = false;
            }
        }

        if (hit.collider.gameObject.tag == "Player")
        {
            CharacterScript charScript = hit.collider.gameObject.GetComponent<CharacterScript>();
            m_highlightedTile = charScript.m_tile;
        }
        else if (hit.collider.gameObject.tag == "Tile")
            m_highlightedTile = hit.collider.gameObject;
    }

    private void HandleOldTile(GameObject _target)
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
            if (oTR.material.color.r == 0 && oTR.material.color.b == 1 && oTR.material.color.a == 0f)
                oTR.material.color = new Color(oTR.material.color.r, oTR.material.color.g, oTR.material.color.b, oTR.material.color.a + 0.5f);
        }

        CharacterScript currCharScript = m_currPlayer.GetComponent<CharacterScript>();
        Renderer tarRend = _target.GetComponent<Renderer>();

        // If target is a player, get that player's tile and make it the target
        CharacterScript colScript;
        if (_target.GetComponent<CharacterScript>())
        {
            colScript = _target.GetComponent<CharacterScript>();
            tarRend = colScript.m_tile.GetComponent<Renderer>();
            _target = colScript.m_tile;
        }

        // TARGET RED TILE

        TileScript selTileScript = _target.GetComponent<TileScript>();
        if (currCharScript.m_currRadius + currCharScript.m_tempStats[(int)CharacterScript.sts.RAD] > 0 && tarRend.material.color == new Color(1, 0, 0, 0.5f))
            selTileScript.FetchTilesWithinRange(currCharScript.m_currRadius + currCharScript.m_tempStats[(int)CharacterScript.sts.RAD], Color.yellow, true, TileScript.targetRestriction.NONE, true, false);
        else if (m_currAction.Length > 0) // REFACTOR: All this code just for piercing attack...
        {
            string[] actSepareted = m_currAction.Split('|');
            string[] id = actSepareted[(int)DatabaseScript.actions.ID].Split(':');
            string[] rng = actSepareted[(int)DatabaseScript.actions.RNG].Split(':');

            if (int.Parse(id[1]) == 11 && tarRend.material.color == new Color(1, 0, 0, 0.5f))
            {
                selTileScript.FetchTilesWithinRange(int.Parse(rng[1]) + currCharScript.m_tempStats[(int)CharacterScript.sts.RAD], Color.yellow, true, TileScript.targetRestriction.HORVERT, false, false);
                m_oldTile = _target;
                return;
            }
            else if (int.Parse(id[1]) == 12 && tarRend.material.color == new Color(1, 0, 0, 0.5f))
            {
                selTileScript.FetchTilesWithinRange(int.Parse(rng[1]) + currCharScript.m_tempStats[(int)CharacterScript.sts.RNG], Color.yellow, false, TileScript.targetRestriction.DIAGNAL, false, true);
                m_oldTile = _target;
                return;
            }
        }
        
        if (currCharScript.m_currRadius + currCharScript.m_tempStats[(int)CharacterScript.sts.RAD] <= 0)
            tarRend.material.color = new Color(tarRend.material.color.r, tarRend.material.color.g, tarRend.material.color.b, tarRend.material.color.a + 0.5f);
        
        m_oldTile = _target;
    }

    public void NewTurn()
    {
        if (m_currRound.Count == 0)
            NewRound();

        // Turn previous character back to original color
        if (m_currPlayer)
        {
            Renderer oldRenderer = m_currPlayer.GetComponent<Renderer>();
            oldRenderer.material.color = Color.white;
        }

        m_currPlayer = m_currRound[0];
        CharacterScript charScript = m_currPlayer.GetComponent<CharacterScript>();
        m_currTile = charScript.m_tile;

        PanelScript HUDLeftScript = m_panels[(int)pnls.HUD_LEFT_PANEL].GetComponent<PanelScript>();
        HUDLeftScript.m_character = m_currPlayer;
        HUDLeftScript.m_cScript = m_currPlayer.GetComponent<CharacterScript>();
        HUDLeftScript.PopulateHUD();

        PlayerScript playScript = charScript.m_player.GetComponent<PlayerScript>();
        if (playScript)
            playScript.SetEnergyPanel();
 
        m_currRound.Remove(m_currPlayer);

        CameraScript camScript = m_camera.GetComponent<CameraScript>();
        camScript.m_target = m_currPlayer;

        Renderer charRenderer = m_currPlayer.GetComponent<Renderer>();
        charRenderer.material.color = Color.green;
    }

    public void NewRound()
    {
        int numPool;
        do
        {
            numPool = 0;
            // Gather all characters speed to pull from
            for (int i = 0; i < m_characters.Count; i++)
            {
                CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();
                numPool += charScript.m_tempStats[(int)CharacterScript.sts.SPD];
            }

            int randNum = Random.Range(0, numPool + 1);
            int currNum = 0;

            for (int i = 0; i < m_characters.Count; i++)
            {
                CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();
                if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] <= 0 || charScript.m_tempStats[(int)CharacterScript.sts.HP] <= 0) // ADD BETTER DEATH CHECK
                    continue;

                currNum += charScript.m_tempStats[(int)CharacterScript.sts.SPD];

                if (randNum < currNum)
                {
                    if (charScript.m_tempStats[(int)CharacterScript.sts.SPD] >= 10)
                        numPool -= 10;
                    else
                        numPool -= charScript.m_tempStats[(int)CharacterScript.sts.SPD];

                    charScript.m_tempStats[(int)CharacterScript.sts.SPD] -= 10;
                    m_currRound.Add(m_characters[i]);
                }
            }
        } while (m_currRound.Count < m_characters.Count && numPool > 0);

        for (int i = 0; i < m_characters.Count; i++)
        {
            CharacterScript charScript = m_characters[i].GetComponent<CharacterScript>();
            charScript.m_tempStats[(int)CharacterScript.sts.SPD] += 10;
        }

        m_roundCount++;
        PanelScript roundPanScript = m_panels[(int)pnls.ROUND_PANEL].GetComponent<PanelScript>();
        roundPanScript.PopulateText();
    }
}
